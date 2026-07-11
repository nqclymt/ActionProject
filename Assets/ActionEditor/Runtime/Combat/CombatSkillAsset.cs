using System;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine;

namespace PKC.ActionEditor
{
    [Serializable]
    [Name("战斗技能")]
    [Description("带版本号的战斗技能基础数据和时间轴数据。")]
    public sealed class CombatSkillAsset : Asset
    {
        public const int CurrentSchemaVersion = 1;

        public CombatSkillAsset()
        {
            Validate();
        }

        [HideInInspector]
        public int schemaVersion = CurrentSchemaVersion;

        [MenuName("技能 ID")]
        public string skillId = Guid.NewGuid().ToString("N");

        [MenuName("持续时间（秒）")]
        public float duration = 1f;

        [MenuName("求值帧率")]
        public int frameRate = 30;

        [MenuName("技能标签")]
        public List<string> tags = new();

        [MenuName("施法阶段")]
        public List<CombatSkillCastPhase> castPhases = CreateDefaultPhases();

        [MenuName("冷却时间（秒）")]
        public float cooldown;

        [MenuName("资源消耗")]
        public List<CombatSkillCost> costs = new();

        [MenuName("目标模式")]
        public CombatSkillTargetingMode targetingMode = CombatSkillTargetingMode.LockedTarget;

        [MenuName("目标阵营")]
        public CombatSkillTargetTeam targetTeam = CombatSkillTargetTeam.Enemies;

        [MenuName("施法距离")]
        public float targetRange = 3f;

        [MenuName("作用半径")]
        [OptionRelateParam(nameof(targetingMode), CombatSkillTargetingMode.Point,
            CombatSkillTargetingMode.Cone, CombatSkillTargetingMode.Circle)]
        public float targetRadius = 0.5f;

        [MenuName("扇形角度")]
        [OptionRelateParam(nameof(targetingMode), CombatSkillTargetingMode.Cone)]
        public float targetAngle = 60f;

        [MenuName("最大目标数")]
        public int maxTargets = 1;

        [MenuName("需要视线检测")]
        public bool requiresLineOfSight = true;

        [MenuName("取消窗口")]
        public List<CombatSkillCancellationWindow> cancellationWindows = new();

        [fsIgnore]
        public int SchemaVersion => schemaVersion;

        [fsIgnore]
        public override int EvaluationFrameRate => frameRate;

        protected override float MinimumLength => Math.Max(duration, 0.1f);

        protected override void OnValidateAsset()
        {
            if (schemaVersion <= 0)
                schemaVersion = CurrentSchemaVersion;

            skillId ??= string.Empty;
            duration = Math.Max(0.1f, duration);
            frameRate = Math.Max(1, Math.Min(frameRate, 240));
            cooldown = Math.Max(0f, cooldown);
            targetRange = Math.Max(0f, targetRange);
            targetRadius = Math.Max(0f, targetRadius);
            targetAngle = Math.Max(0f, Math.Min(targetAngle, 360f));
            maxTargets = Math.Max(1, maxTargets);

            tags ??= new List<string>();
            for (var i = 0; i < tags.Count; i++)
                tags[i] ??= string.Empty;

            castPhases ??= new List<CombatSkillCastPhase>();
            castPhases.RemoveAll(phase => phase == null);
            foreach (var phase in castPhases)
                phase.Normalize(duration);

            costs ??= new List<CombatSkillCost>();
            costs.RemoveAll(cost => cost == null);
            foreach (var cost in costs)
                cost.Normalize();

            cancellationWindows ??= new List<CombatSkillCancellationWindow>();
            cancellationWindows.RemoveAll(window => window == null);
            foreach (var window in cancellationWindows)
                window.Normalize(duration);
        }

        public CombatSkillCastPhase GetPhaseAt(float time)
        {
            if (castPhases == null)
                return null;

            foreach (var phase in castPhases)
            {
                if (phase != null && time >= phase.startTime && time < phase.endTime)
                    return phase;
            }

            return null;
        }

        public bool CanCancelAt(float time, CombatSkillCancellationType cancellationType,
            bool hasHitConfirmed = false, string skillTag = null)
        {
            if (cancellationWindows == null)
                return false;

            foreach (var window in cancellationWindows)
            {
                if (window == null || time < window.startTime || time > window.endTime)
                    continue;

                if (window.requiresHitConfirm && !hasHitConfirmed)
                    continue;

                if (window.cancellationType != cancellationType)
                    continue;

                if (cancellationType != CombatSkillCancellationType.SkillTag ||
                    string.Equals(window.requiredSkillTag, skillTag, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static List<CombatSkillCastPhase> CreateDefaultPhases()
        {
            return new List<CombatSkillCastPhase>
            {
                new()
                {
                    name = "前摇",
                    phaseType = CombatSkillPhaseType.Windup,
                    startTime = 0f,
                    endTime = 0.2f
                },
                new()
                {
                    name = "生效",
                    phaseType = CombatSkillPhaseType.Active,
                    startTime = 0.2f,
                    endTime = 0.4f
                },
                new()
                {
                    name = "后摇",
                    phaseType = CombatSkillPhaseType.Recovery,
                    startTime = 0.4f,
                    endTime = 1f
                }
            };
        }
    }
}
