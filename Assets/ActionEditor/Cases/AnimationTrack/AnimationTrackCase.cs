using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class AnimationTrackCase : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string stateName = "SkillAttack";
        [SerializeField] private string recoveryStateName = "SkillRecover";
        [SerializeField] private string avatarMaskPath = string.Empty;

        private SkillPlayer player;

        public void Configure(Animator targetAnimator, string targetStateName,
            string targetRecoveryStateName, string maskPath)
        {
            animator = targetAnimator;
            stateName = targetStateName;
            recoveryStateName = targetRecoveryStateName;
            avatarMaskPath = maskPath;
        }

        [ContextMenu("播放动画轨道运行时案例")]
        public void PlayCase()
        {
            StopCase();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                Debug.LogError("动画轨道案例缺少 Animator 或 Runtime Animator Controller。", this);
                return;
            }

            var skill = CreateSkill();
            player = new SkillPlayer(skill, new SkillExecutionContext(gameObject));
            player.Completed += OnCompleted;
            player.Play();
            Debug.Log("动画轨道运行时案例开始播放。", this);
        }

        [ContextMenu("停止动画轨道运行时案例")]
        public void StopCase()
        {
            if (player == null)
                return;

            player.Completed -= OnCompleted;
            player.Stop();
            player = null;
        }

        private void Update()
        {
            player?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            StopCase();
        }

        private CombatSkillAsset CreateSkill()
        {
            var skill = new CombatSkillAsset
            {
                skillId = "case.animation_track",
                duration = 2f,
                frameRate = 30
            };
            var group = skill.AddGroup<Group>("动画验收角色");
            var track = group.AddTrack<AnimationTrack>();
            track.Name = "动画轨道";
            track.layer = 0;
            track.avatarMaskPath = avatarMaskPath;
            track.rootMotion = AnimationRootMotionMode.Disabled;

            var clip = track.AddClip<AnimationStateClip>(0f);
            clip.Name = "技能动画";
            clip.stateName = stateName;
            clip.speed = 1f;
            clip.transitionDuration = 0.15f;
            clip.Length = 1f;

            var recoveryClip = track.AddClip<AnimationStateClip>(1f);
            recoveryClip.Name = "技能收招动画";
            recoveryClip.stateName = recoveryStateName;
            recoveryClip.speed = 1f;
            recoveryClip.transitionDuration = 0.15f;
            recoveryClip.Length = 1f;
            skill.Validate();
            return skill;
        }

        private void OnCompleted(SkillPlayer completedPlayer)
        {
            Debug.Log("动画轨道运行时案例播放完成。", this);
            completedPlayer.Completed -= OnCompleted;
            player = null;
        }
    }
}
