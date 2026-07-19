using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PKC.ActionEditor.Cases.Editor
{
    public static class AnimationTrackEditorCase
    {
        private const string ResourcesFolder = "Assets/ActionEditor/Cases/AnimationTrack/Resources";
        private const string GeneratedFolder = ResourcesFolder + "/Generated";
        private const string AnimationPath = GeneratedFolder + "/SkillAttack.anim";
        private const string RecoveryAnimationPath = GeneratedFolder + "/SkillRecover.anim";
        private const string ControllerPath = GeneratedFolder + "/SkillAttack.controller";
        private const string MaskPath = GeneratedFolder + "/SkillAttack.mask";
        private const string SkillPath = GeneratedFolder + "/AnimationTrackCase.json";
        private const string ActorName = "AnimationTrackCaseActor";
        private const string StateName = "SkillAttack";
        private const string RecoveryStateName = "SkillRecover";

        [MenuItem("Tools/ActionEditor Cases/创建动画轨道验收案例")]
        public static void Create()
        {
            CreateInternal(true);
        }

        private static void CreateInternal(bool openWindow)
        {
            Cleanup();
            EnsureGeneratedFolder();

            var actor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            actor.name = ActorName;
            actor.transform.position = Vector3.zero;

            var animationClip = CreateAnimation(AnimationPath, StateName, 1.6f);
            var recoveryAnimationClip = CreateAnimation(RecoveryAnimationPath, RecoveryStateName, 0.6f);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var state = controller.layers[0].stateMachine.AddState(StateName);
            state.motion = animationClip;
            controller.layers[0].stateMachine.defaultState = state;
            var recoveryState = controller.layers[0].stateMachine.AddState(RecoveryStateName);
            recoveryState.motion = recoveryAnimationClip;

            var mask = new AvatarMask { name = "SkillAttack" };
            mask.AddTransformPath(actor.transform, true);
            AssetDatabase.CreateAsset(mask, MaskPath);

            var animator = actor.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = true;

            var runtimeCaseType = Type.GetType(
                "PKC.ActionEditor.Cases.AnimationTrackCase, PKC.ActionEditor.Cases.AnimationTrack");
            if (runtimeCaseType == null)
                throw new InvalidOperationException("无法加载动画轨道运行时案例程序集。");

            var runtimeCase = actor.AddComponent(runtimeCaseType);
            runtimeCaseType.GetMethod("Configure")?.Invoke(runtimeCase,
                new object[] { animator, StateName, RecoveryStateName, MaskPath });

            var skill = CreateSkill();
            var serializedSkill = Json.Serialize(skill);
            ValidateSerializedSkill(serializedSkill);
            File.WriteAllText(Path.GetFullPath(SkillPath), serializedSkill);
            AssetDatabase.ImportAsset(SkillPath, ImportAssetOptions.ForceUpdate);

            if (openWindow)
                ActionEditorWindow.OpenWindow();
            App.TextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(SkillPath);
            if (App.AssetData?.groups.Count > 0)
                App.AssetData.groups[0].Actor = actor;

            Selection.activeGameObject = actor;
            EditorSceneManager.MarkSceneDirty(actor.scene);
            Debug.Log("动画轨道验收案例已创建。ActionEditor 已加载案例技能。", actor);
        }

        [MenuItem("Tools/ActionEditor Cases/清理动画轨道验收案例")]
        public static void Cleanup()
        {
            App.Stop();
            if (App.TextAsset != null && AssetDatabase.GetAssetPath(App.TextAsset).StartsWith(GeneratedFolder))
                App.TextAsset = null;

            var existing = GameObject.Find(ActorName);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing);

            if (AssetDatabase.IsValidFolder(GeneratedFolder))
                AssetDatabase.DeleteAsset(GeneratedFolder);
            if (AssetDatabase.IsValidFolder(ResourcesFolder))
                AssetDatabase.DeleteAsset(ResourcesFolder);

            AssetDatabase.Refresh();
        }

        public static void RunAutomated()
        {
            try
            {
                CreateInternal(false);
                var actor = GameObject.Find(ActorName);
                if (actor == null)
                    throw new InvalidOperationException("没有创建动画轨道案例角色。");

                AssetPlayer.Inst.Reset();
                AssetPlayer.Inst.Sample(0.5f);
                Require(actor.transform.localScale.x > 1.4f,
                    "编辑器预览没有在 0.5 秒求值动画姿势。");
                AssetPlayer.Inst.Sample(1.5f);
                Require(actor.transform.localScale.x < 0.8f,
                    "编辑器预览没有切换到第二个动画状态。");
                AssetPlayer.Inst.Reset();

                var animator = actor.GetComponent<Animator>();
                var runtimeSkill = CreateSkill();
                var runtimePlayer = new SkillPlayer(runtimeSkill, new SkillExecutionContext(actor));
                Require(runtimePlayer.Play(), "运行时动画案例没有开始播放。");
                runtimePlayer.Tick(0.5f);
                Require(actor.transform.localScale.x > 1.4f,
                    "运行时 SkillPlayer 没有在 0.5 秒求值动画姿势。");
                Require(!animator.applyRootMotion, "运行时没有应用 Root Motion 禁用设置。");
                runtimePlayer.Tick(1f);
                Require(actor.transform.localScale.x < 0.8f,
                    "运行时 SkillPlayer 没有切换到第二个动画状态。");
                runtimePlayer.Stop();
                Require(animator.applyRootMotion, "停止播放后没有恢复 Animator 的 Root Motion 设置。");

                Debug.Log("动画轨道编辑器与运行时自动验证通过。");
            }
            finally
            {
                Cleanup();
            }
        }

        private static AnimationClip CreateAnimation(string path, string clipName, float middleScale)
        {
            var clip = new UnityEngine.AnimationClip
            {
                name = clipName,
                frameRate = 30f,
                wrapMode = WrapMode.ClampForever
            };

            var scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.5f, middleScale),
                new Keyframe(1f, 1f));
            clip.SetCurve(string.Empty, typeof(Transform), "localScale.x", scaleCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localScale.y", scaleCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localScale.z", scaleCurve);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static CombatSkillAsset CreateSkill()
        {
            var skill = new CombatSkillAsset
            {
                skillId = "case.editor.animation_track",
                duration = 2f,
                frameRate = 30
            };
            var group = skill.AddGroup<Group>("动画验收角色");
            var track = group.AddTrack<AnimationTrack>();
            track.Name = "技能动画";
            track.layer = 0;
            track.avatarMaskPath = MaskPath;
            track.rootMotion = AnimationRootMotionMode.Disabled;

            var animation = track.AddClip<AnimationStateClip>(0f);
            animation.Name = "技能动作";
            animation.stateName = StateName;
            animation.speed = 1f;
            animation.transitionDuration = 0.15f;
            animation.animationStartTime = 0f;
            animation.Length = 1f;

            var recovery = track.AddClip<AnimationStateClip>(1f);
            recovery.Name = "技能收招";
            recovery.stateName = RecoveryStateName;
            recovery.speed = 1f;
            recovery.transitionDuration = 0.15f;
            recovery.animationStartTime = 0f;
            recovery.Length = 1f;
            skill.Validate();
            return skill;
        }

        private static void ValidateSerializedSkill(string json)
        {
            var restored = Json.Deserialize(typeof(Asset), json) as CombatSkillAsset;
            if (restored?.groups == null || restored.groups.Count == 0 ||
                restored.groups[0].Tracks.Count == 0)
                throw new InvalidDataException("动画轨道数据序列化后缺少轨道。");

            var restoredTrack = restored.groups[0].Tracks[0] as AnimationTrack;
            var restoredClip = restoredTrack != null && restoredTrack.Clips.Count > 0
                ? restoredTrack.Clips[0] as AnimationStateClip
                : null;
            var restoredRecoveryClip = restoredTrack != null && restoredTrack.Clips.Count > 1
                ? restoredTrack.Clips[1] as AnimationStateClip
                : null;
            if (restoredClip == null || restoredClip.stateName != StateName ||
                restoredRecoveryClip == null || restoredRecoveryClip.stateName != RecoveryStateName ||
                restoredTrack.avatarMaskPath != MaskPath ||
                restoredTrack.rootMotion != AnimationRootMotionMode.Disabled)
            {
                throw new InvalidDataException("动画轨道数据序列化往返验证失败。");
            }
        }

        private static void EnsureGeneratedFolder()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
                AssetDatabase.CreateFolder("Assets/ActionEditor/Cases/AnimationTrack", "Resources");
            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
                AssetDatabase.CreateFolder(ResourcesFolder, "Generated");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
