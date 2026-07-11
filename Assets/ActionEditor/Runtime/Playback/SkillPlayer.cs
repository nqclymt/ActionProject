using System;
using UnityEngine;

namespace PKC.ActionEditor
{
    public enum SkillPlaybackState
    {
        Stopped,
        Playing,
        Paused,
        Interrupted,
        Completed
    }

    /// <summary>
    /// 推进战斗技能时间并管理播放状态。具体轨道通过 TimeEvaluated 接入执行逻辑。
    /// </summary>
    public sealed class SkillPlayer
    {
        private const float TimeEpsilon = 0.00001f;

        private float currentTime;
        private SkillPlaybackState state = SkillPlaybackState.Stopped;

        public SkillPlayer(CombatSkillAsset skill = null)
        {
            SetSkill(skill);
        }

        public CombatSkillAsset Skill { get; private set; }

        public SkillPlaybackState State => state;

        public float CurrentTime => currentTime;

        public float Duration => Skill == null ? 0f : Mathf.Max(0.1f, Skill.duration);

        public bool Loop { get; set; }

        public bool IsPlaying => state == SkillPlaybackState.Playing;

        public string InterruptReason { get; private set; }

        public event Action<SkillPlayer> Played;
        public event Action<SkillPlayer> Paused;
        public event Action<SkillPlayer> Stopped;
        public event Action<SkillPlayer> Looped;
        public event Action<SkillPlayer> Completed;
        public event Action<SkillPlayer, string> Interrupted;
        public event Action<SkillPlayer, SkillPlaybackState, SkillPlaybackState> StateChanged;
        public event Action<SkillPlayer, float, float> TimeChanged;

        /// <summary>
        /// 每个连续时间段触发一次。循环跨越结尾时会拆成“到结尾”和“从零开始”两段。
        /// </summary>
        public event Action<SkillPlayer, float, float> TimeEvaluated;

        public void SetSkill(CombatSkillAsset skill)
        {
            if (ReferenceEquals(Skill, skill))
                return;

            if (Skill != null)
                Stop();

            Skill = skill;
            Skill?.Validate();
            currentTime = 0f;
            InterruptReason = null;
            SetState(SkillPlaybackState.Stopped);
        }

        public bool Play()
        {
            if (Skill == null || state == SkillPlaybackState.Playing)
                return false;

            if (state == SkillPlaybackState.Completed || state == SkillPlaybackState.Interrupted ||
                currentTime >= Duration - TimeEpsilon)
            {
                SetTime(0f, true);
            }

            InterruptReason = null;
            SetState(SkillPlaybackState.Playing);
            Played?.Invoke(this);
            return true;
        }

        public bool Pause()
        {
            if (state != SkillPlaybackState.Playing)
                return false;

            SetState(SkillPlaybackState.Paused);
            Paused?.Invoke(this);
            return true;
        }

        public bool Stop()
        {
            if (state == SkillPlaybackState.Stopped && currentTime <= TimeEpsilon)
                return false;

            if (currentTime > TimeEpsilon)
                SetTime(0f, true);

            InterruptReason = null;
            SetState(SkillPlaybackState.Stopped);
            Stopped?.Invoke(this);
            return true;
        }

        public bool Interrupt(string reason = null)
        {
            if (state != SkillPlaybackState.Playing && state != SkillPlaybackState.Paused)
                return false;

            InterruptReason = reason ?? string.Empty;
            SetState(SkillPlaybackState.Interrupted);
            Interrupted?.Invoke(this, InterruptReason);
            return true;
        }

        public bool Seek(float time)
        {
            if (Skill == null)
                return false;
            if (float.IsNaN(time) || float.IsInfinity(time))
                throw new ArgumentOutOfRangeException(nameof(time), "Seek time must be finite.");

            var targetTime = Mathf.Clamp(time, 0f, Duration);
            if (Mathf.Abs(targetTime - currentTime) <= TimeEpsilon)
                return false;

            SetTime(targetTime, true);
            return true;
        }

        /// <summary>
        /// 由游戏循环传入未缩放或已缩放的增量时间，播放器本身不读取 Time.deltaTime。
        /// </summary>
        public bool Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be finite and non-negative.");
            if (state != SkillPlaybackState.Playing || deltaTime <= TimeEpsilon)
                return false;

            if (Loop)
                TickLooping(deltaTime);
            else
                TickOnce(deltaTime);

            return true;
        }

        private void TickOnce(float deltaTime)
        {
            var targetTime = currentTime + deltaTime;
            if (targetTime < Duration - TimeEpsilon)
            {
                SetTime(targetTime, true);
                return;
            }

            SetTime(Duration, true);
            SetState(SkillPlaybackState.Completed);
            Completed?.Invoke(this);
        }

        private void TickLooping(float deltaTime)
        {
            var remaining = deltaTime;
            while (remaining > TimeEpsilon)
            {
                var timeToEnd = Duration - currentTime;
                if (remaining < timeToEnd - TimeEpsilon)
                {
                    SetTime(currentTime + remaining, true);
                    return;
                }

                SetTime(Duration, true);
                remaining = Mathf.Max(0f, remaining - timeToEnd);
                SetTime(0f, false);
                Looped?.Invoke(this);
            }
        }

        private void SetTime(float value, bool evaluate)
        {
            var previousTime = currentTime;
            currentTime = Mathf.Clamp(value, 0f, Duration);

            if (Mathf.Abs(previousTime - currentTime) <= TimeEpsilon)
                return;

            TimeChanged?.Invoke(this, previousTime, currentTime);
            if (evaluate)
                TimeEvaluated?.Invoke(this, previousTime, currentTime);
        }

        private void SetState(SkillPlaybackState value)
        {
            if (state == value)
                return;

            var previousState = state;
            state = value;
            StateChanged?.Invoke(this, previousState, state);
        }
    }
}
