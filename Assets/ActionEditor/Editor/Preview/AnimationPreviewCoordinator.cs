using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PKC.ActionEditor
{
    internal static class AnimationPreviewCoordinator
    {
        private static readonly Dictionary<Group, PreviewSession> Sessions = new();
        private static Asset currentAsset;

        public static void Evaluate(Asset asset, float previousTime, float currentTime)
        {
            if (asset == null)
            {
                Reset();
                return;
            }

            if (!ReferenceEquals(currentAsset, asset))
            {
                Reset();
                currentAsset = asset;
            }

            var activeGroups = new HashSet<Group>();
            foreach (var group in asset.groups)
            {
                if (group == null || !group.IsActive || group.Actor == null)
                    continue;

                var tracks = group.Tracks.OfType<AnimationTrack>().Where(track => track.IsActive).ToArray();
                if (tracks.Length == 0)
                    continue;

                var animator = group.Actor.GetComponentInChildren<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                    continue;

                activeGroups.Add(group);
                if (!Sessions.TryGetValue(group, out var session) || session.Animator != animator)
                {
                    session?.Dispose();
                    session = new PreviewSession(animator,
                        new SkillAnimationEvaluator(animator, tracks, ResolveAvatarMask));
                    Sessions[group] = session;
                }

                session.Evaluator.Evaluate(previousTime, currentTime);
            }

            foreach (var pair in Sessions.Where(pair => !activeGroups.Contains(pair.Key)).ToArray())
            {
                pair.Value.Dispose();
                Sessions.Remove(pair.Key);
            }
        }

        public static void Reset()
        {
            foreach (var session in Sessions.Values)
                session.Dispose();

            Sessions.Clear();
            currentAsset = null;
        }

        private static AvatarMask ResolveAvatarMask(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
        }

        private sealed class PreviewSession : IDisposable
        {
            public PreviewSession(Animator animator, SkillAnimationEvaluator evaluator)
            {
                Animator = animator;
                Evaluator = evaluator;
            }

            public Animator Animator { get; }
            public SkillAnimationEvaluator Evaluator { get; }

            public void Dispose()
            {
                Evaluator.Dispose();
            }
        }
    }
}
