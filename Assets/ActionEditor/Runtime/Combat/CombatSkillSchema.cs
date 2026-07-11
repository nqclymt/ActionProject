using System;

namespace PKC.ActionEditor
{
    public enum CombatSkillPhaseType
    {
        [MenuName("前摇")]
        Windup,
        [MenuName("施放")]
        Cast,
        [MenuName("生效")]
        Active,
        [MenuName("引导")]
        Channel,
        [MenuName("后摇")]
        Recovery,
        [MenuName("自定义")]
        Custom
    }

    public enum CombatSkillCostTiming
    {
        [MenuName("施法开始时")]
        OnCastStart,
        [MenuName("释放时")]
        OnRelease,
        [MenuName("每秒")]
        PerSecond
    }

    public enum CombatSkillTargetingMode
    {
        [MenuName("自身")]
        Self,
        [MenuName("锁定目标")]
        LockedTarget,
        [MenuName("指定位置")]
        Point,
        [MenuName("指定方向")]
        Direction,
        [MenuName("扇形")]
        Cone,
        [MenuName("圆形")]
        Circle,
        [MenuName("矩形")]
        Box
    }

    public enum CombatSkillTargetTeam
    {
        [MenuName("自身")]
        Self,
        [MenuName("友方")]
        Allies,
        [MenuName("敌方")]
        Enemies,
        [MenuName("自身与友方")]
        AlliesAndSelf,
        [MenuName("任意目标")]
        Any
    }

    public enum CombatSkillCancellationType
    {
        [MenuName("任意技能")]
        AnySkill,
        [MenuName("移动")]
        Movement,
        [MenuName("闪避")]
        Dodge,
        [MenuName("指定技能标签")]
        SkillTag
    }

    [Serializable]
    public sealed class CombatSkillCastPhase
    {
        [MenuName("阶段名称")]
        public string name = "阶段";

        [MenuName("阶段类型")]
        public CombatSkillPhaseType phaseType;

        [MenuName("开始时间")]
        public float startTime;

        [MenuName("结束时间")]
        public float endTime = 0.1f;

        internal void Normalize(float duration)
        {
            name ??= string.Empty;
            startTime = Math.Max(0f, Math.Min(startTime, duration));
            endTime = Math.Max(startTime, Math.Min(endTime, duration));
        }
    }

    [Serializable]
    public sealed class CombatSkillCost
    {
        [MenuName("资源 ID")]
        public string resourceId = "Mana";

        [MenuName("消耗数量")]
        public float amount;

        [MenuName("扣除时机")]
        public CombatSkillCostTiming timing;

        internal void Normalize()
        {
            resourceId ??= string.Empty;
            amount = Math.Max(0f, amount);
        }
    }

    [Serializable]
    public sealed class CombatSkillCancellationWindow
    {
        [MenuName("窗口名称")]
        public string name = "取消窗口";

        [MenuName("开始时间")]
        public float startTime;

        [MenuName("结束时间")]
        public float endTime = 0.1f;

        [MenuName("取消类型")]
        public CombatSkillCancellationType cancellationType;

        [MenuName("要求的技能标签")]
        public string requiredSkillTag = string.Empty;

        [MenuName("需要命中确认")]
        public bool requiresHitConfirm;

        internal void Normalize(float duration)
        {
            name ??= string.Empty;
            requiredSkillTag ??= string.Empty;
            startTime = Math.Max(0f, Math.Min(startTime, duration));
            endTime = Math.Max(startTime, Math.Min(endTime, duration));
        }
    }
}
