using System;
using UnityEngine;

namespace PKC.ActionEditor
{
    public enum AnimationRootMotionMode
    {
        [MenuName("使用 Animator 设置")]
        AnimatorDefault,

        [MenuName("启用")]
        Enabled,

        [MenuName("禁用")]
        Disabled
    }

    [Serializable]
    [Name("动画轨道")]
    [Category("战斗/动画")]
    [Attachable(typeof(Group))]
    public sealed class AnimationTrack : Track
    {
        [MenuName("Animator 层级")]
        public int layer;

        [MenuName("Avatar Mask")]
        [SelectObjectPath(typeof(AvatarMask))]
        public string avatarMaskPath = string.Empty;

        [MenuName("Root Motion")]
        public AnimationRootMotionMode rootMotion = AnimationRootMotionMode.AnimatorDefault;

        public override string info => $"Animator Layer {Mathf.Max(0, layer)}";

        public override void OnAfterDeserialize()
        {
            layer = Mathf.Max(0, layer);
            avatarMaskPath ??= string.Empty;
        }
    }

    [Serializable]
    [Name("动画状态")]
    [Category("动画")]
    [Attachable(typeof(AnimationTrack))]
    public sealed class AnimationStateClip : ClipCrossBlend
    {
        public AnimationStateClip()
        {
            Length = 1f;
        }

        [MenuName("状态名称")]
        public string stateName = string.Empty;

        [MenuName("播放速度")]
        public float speed = 1f;

        [MenuName("过渡时间（秒）")]
        public float transitionDuration = 0.1f;

        [MenuName("动画起始时间（秒）")]
        public float animationStartTime;

        public override string Info => string.IsNullOrWhiteSpace(stateName) ? "动画状态" : stateName;

        public override bool IsValid => !string.IsNullOrWhiteSpace(stateName) && speed > 0f && Length > 0f;

        public override void OnAfterDeserialize()
        {
            stateName ??= string.Empty;
            speed = Mathf.Max(0.01f, speed);
            transitionDuration = Mathf.Max(0f, transitionDuration);
            animationStartTime = Mathf.Max(0f, animationStartTime);
        }
    }
}
