using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PKC.ActionEditor
{
    /// <summary>
    /// 使用手动 PlayableGraph 对动画轨道求值，使运行时播放与编辑器拖动使用相同时间语义。
    /// </summary>
    public sealed class SkillAnimationEvaluator : IDisposable
    {
        private const float TimeEpsilon = 0.00001f;

        private readonly Animator animator;
        private readonly List<AnimationTrack> tracks = new();
        private readonly List<TrackState> states = new();
        private readonly HashSet<string> warnings = new();
        private readonly Func<string, AvatarMask> avatarMaskResolver;
        private readonly bool originalApplyRootMotion;
        private PlayableGraph graph;
        private AnimationLayerMixerPlayable mixer;
        private bool initialized;

        public SkillAnimationEvaluator(Animator animator, IEnumerable<AnimationTrack> animationTracks,
            Func<string, AvatarMask> avatarMaskResolver = null)
        {
            this.animator = animator;
            this.avatarMaskResolver = avatarMaskResolver ?? ResolveResourceAvatarMask;
            originalApplyRootMotion = animator != null && animator.applyRootMotion;

            if (animationTracks == null)
                return;

            foreach (var track in animationTracks)
            {
                if (track != null && track.IsActive)
                    tracks.Add(track);
            }
        }

        public bool IsValid => animator != null && animator.runtimeAnimatorController != null && tracks.Count > 0;

        public void Evaluate(float previousTime, float currentTime)
        {
            if (!IsValid || Math.Abs(currentTime - previousTime) <= TimeEpsilon)
                return;
            if (!EnsureInitialized())
                return;

            var forward = currentTime > previousTime;
            var deltaTime = forward ? currentTime - previousTime : 0f;
            AnimationRootMotionMode? rootMotion = null;

            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                var activeClip = FindActiveClip(state.Track, currentTime);
                if (activeClip != null && !rootMotion.HasValue &&
                    state.Track.rootMotion != AnimationRootMotionMode.AnimatorDefault)
                {
                    rootMotion = state.Track.rootMotion;
                }

                EvaluateTrack(state, activeClip, previousTime, currentTime, forward);
            }

            animator.applyRootMotion = rootMotion.HasValue
                ? rootMotion.Value == AnimationRootMotionMode.Enabled
                : originalApplyRootMotion;

            if (forward && deltaTime > TimeEpsilon)
                graph.Evaluate(deltaTime);
            else
                graph.Evaluate(0f);
        }

        public void Reset()
        {
            if (graph.IsValid())
                graph.Destroy();

            states.Clear();
            initialized = false;
            if (animator == null)
                return;

            animator.applyRootMotion = originalApplyRootMotion;
            if (animator.runtimeAnimatorController != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        public void Dispose()
        {
            Reset();
        }

        private bool EnsureInitialized()
        {
            if (initialized)
                return graph.IsValid();

            initialized = true;
            graph = PlayableGraph.Create("ActionEditor Skill Animation");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            mixer = AnimationLayerMixerPlayable.Create(graph, tracks.Count);

            var output = AnimationPlayableOutput.Create(graph, "ActionEditor Animation", animator);
            output.SetSourcePlayable(mixer);

            for (var i = 0; i < tracks.Count; i++)
            {
                var controller = AnimatorControllerPlayable.Create(graph, animator.runtimeAnimatorController);
                controller.SetSpeed(0d);
                graph.Connect(controller, 0, mixer, i);
                mixer.SetInputWeight(i, 0f);

                var mask = avatarMaskResolver(tracks[i].avatarMaskPath);
                if (mask != null)
                    mixer.SetLayerMaskFromAvatarMask((uint)i, mask);
                else if (!string.IsNullOrWhiteSpace(tracks[i].avatarMaskPath))
                    WarnOnce($"mask:{tracks[i].avatarMaskPath}",
                        $"动画轨道无法加载 Avatar Mask：{tracks[i].avatarMaskPath}。" +
                        "运行时资源需要放在 Resources 目录中。");

                states.Add(new TrackState(tracks[i], controller, i));
            }

            graph.Play();
            graph.Evaluate(0f);
            return true;
        }

        private void EvaluateTrack(TrackState state, AnimationStateClip activeClip,
            float previousTime, float currentTime, bool forward)
        {
            if (activeClip == null)
            {
                state.ActiveClip = null;
                state.Controller.SetSpeed(0d);
                mixer.SetInputWeight(state.InputIndex, 0f);
                return;
            }

            var stateHash = Animator.StringToHash(activeClip.stateName);
            var layer = state.Track.layer;
            if (layer < 0 || layer >= state.Controller.GetLayerCount())
            {
                WarnOnce($"layer:{layer}", $"动画轨道配置的 Animator 层级 {layer} 不存在。");
                state.ActiveClip = null;
                state.Controller.SetSpeed(0d);
                mixer.SetInputWeight(state.InputIndex, 0f);
                return;
            }

            if (!state.Controller.HasState(layer, stateHash))
            {
                WarnOnce($"state:{layer}:{activeClip.stateName}",
                    $"Animator 第 {layer} 层中不存在状态“{activeClip.stateName}”。");
                state.ActiveClip = null;
                state.Controller.SetSpeed(0d);
                mixer.SetInputWeight(state.InputIndex, 0f);
                return;
            }

            mixer.SetInputWeight(state.InputIndex, 1f);
            var localTime = Mathf.Max(0f, currentTime - activeClip.StartTime) * activeClip.speed +
                            activeClip.animationStartTime;
            var clipChanged = !ReferenceEquals(state.ActiveClip, activeClip);

            if (!forward)
            {
                state.Controller.PlayInFixedTime(stateHash, layer, localTime);
                state.Controller.SetSpeed(0d);
            }
            else if (clipChanged)
            {
                var transition = activeClip.transitionDuration;
                if (state.ActiveClip != null && transition > TimeEpsilon)
                    state.Controller.CrossFadeInFixedTime(stateHash, transition, layer, activeClip.animationStartTime);
                else
                    state.Controller.PlayInFixedTime(stateHash, layer, activeClip.animationStartTime);

                state.Controller.SetSpeed(activeClip.speed);
            }
            else
            {
                state.Controller.SetSpeed(activeClip.speed);
            }

            state.ActiveClip = activeClip;
        }

        private static AnimationStateClip FindActiveClip(AnimationTrack track, float time)
        {
            AnimationStateClip result = null;
            foreach (var clip in track.Clips)
            {
                if (clip is not AnimationStateClip animationClip || !animationClip.IsValid)
                    continue;
                if (time + TimeEpsilon < animationClip.StartTime ||
                    time >= animationClip.EndTime - TimeEpsilon)
                    continue;
                if (result == null || animationClip.StartTime >= result.StartTime)
                    result = animationClip;
            }

            return result;
        }

        private static AvatarMask ResolveResourceAvatarMask(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            const string resourcesSegment = "/Resources/";
            var normalized = assetPath.Replace('\\', '/');
            var resourcesIndex = normalized.IndexOf(resourcesSegment, StringComparison.OrdinalIgnoreCase);
            var resourcePath = resourcesIndex >= 0
                ? normalized.Substring(resourcesIndex + resourcesSegment.Length)
                : normalized;
            var extensionIndex = resourcePath.LastIndexOf('.');
            if (extensionIndex >= 0)
                resourcePath = resourcePath.Substring(0, extensionIndex);

            return Resources.Load<AvatarMask>(resourcePath);
        }

        private void WarnOnce(string key, string message)
        {
            if (warnings.Add(key))
                Debug.LogWarning(message, animator);
        }

        private sealed class TrackState
        {
            public TrackState(AnimationTrack track, AnimatorControllerPlayable controller, int inputIndex)
            {
                Track = track;
                Controller = controller;
                InputIndex = inputIndex;
            }

            public AnimationTrack Track { get; }
            public AnimatorControllerPlayable Controller { get; }
            public int InputIndex { get; }
            public AnimationStateClip ActiveClip { get; set; }
        }
    }
}
