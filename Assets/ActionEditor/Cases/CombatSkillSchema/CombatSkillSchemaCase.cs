using System;
using System.Collections.Generic;
using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class CombatSkillSchemaCase : MonoBehaviour
    {
        [ContextMenu("Validate Combat Skill Schema")]
        public void ValidateCombatSkillSchema()
        {
            Run();
        }

        public static void Run()
        {
            var skill = CreateSkill();
            skill.Validate();

            Require(skill.SchemaVersion == CombatSkillAsset.CurrentSchemaVersion,
                "The schema version is invalid.");
            Require(skill.Length >= skill.duration,
                "The timeline length is shorter than the authored skill duration.");
            Require(skill.GetPhaseAt(0.1f)?.phaseType == CombatSkillPhaseType.Windup,
                "The windup phase lookup failed.");
            Require(!skill.CanCancelAt(1f, CombatSkillCancellationType.Dodge),
                "A hit-confirmed cancellation window was accepted without a hit confirm.");
            Require(skill.CanCancelAt(1f, CombatSkillCancellationType.Dodge, true),
                "The valid cancellation window was not found.");

            var json = Json.Serialize(skill);
            Require(!string.IsNullOrWhiteSpace(json), "Combat skill serialization returned no data.");

            var restored = Json.Deserialize(typeof(Asset), json) as CombatSkillAsset;
            Require(restored != null, "Combat skill polymorphic deserialization failed.");

            restored.Init();
            Require(restored.skillId == skill.skillId, "The skill ID did not survive serialization.");
            Require(restored.tags.Count == 2 && restored.tags[1] == "Fire",
                "The skill tags did not survive serialization.");
            Require(restored.costs.Count == 1 && Math.Abs(restored.costs[0].amount - 20f) < 0.001f,
                "The resource cost did not survive serialization.");
            Require(restored.targetingMode == CombatSkillTargetingMode.Cone && restored.maxTargets == 3,
                "The targeting rules did not survive serialization.");
            Require(restored.cancellationWindows.Count == 1,
                "The cancellation windows did not survive serialization.");

            Debug.Log("CombatSkillAsset schema validation passed.");
        }

        private static CombatSkillAsset CreateSkill()
        {
            return new CombatSkillAsset
            {
                skillId = "case.combat.fire_slash",
                duration = 1.5f,
                tags = new List<string> { "Melee", "Fire" },
                cooldown = 2.5f,
                costs = new List<CombatSkillCost>
                {
                    new()
                    {
                        resourceId = "Mana",
                        amount = 20f,
                        timing = CombatSkillCostTiming.OnRelease
                    }
                },
                targetingMode = CombatSkillTargetingMode.Cone,
                targetTeam = CombatSkillTargetTeam.Enemies,
                targetRange = 5f,
                targetRadius = 1.25f,
                targetAngle = 60f,
                maxTargets = 3,
                requiresLineOfSight = true,
                cancellationWindows = new List<CombatSkillCancellationWindow>
                {
                    new()
                    {
                        name = "Dodge After Hit",
                        startTime = 0.8f,
                        endTime = 1.2f,
                        cancellationType = CombatSkillCancellationType.Dodge,
                        requiresHitConfirm = true
                    }
                }
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
