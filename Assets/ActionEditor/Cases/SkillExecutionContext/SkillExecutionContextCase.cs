using System;
using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class SkillExecutionContextCase : MonoBehaviour
    {
        [ContextMenu("验证技能执行上下文")]
        public void ValidateSkillExecutionContext()
        {
            Run();
        }

        public static void Run()
        {
            var caster = new GameObject("Context Case Caster");
            var target = new GameObject("Context Case Target");

            try
            {
                caster.transform.position = new Vector3(1f, 0f, 2f);
                target.transform.position = new Vector3(4f, 0f, 2f);

                var damageService = new CaseDamageService();
                var services = new CombatServiceRegistry()
                    .Register<ICaseDamageService>(damageService);
                var context = new SkillExecutionContext(caster, target, services: services);

                Require(context.Caster == caster && context.Target == target,
                    "施法者或目标没有写入上下文。");
                Require(context.WorldPosition == target.transform.position,
                    "默认世界坐标没有使用目标位置。");
                Require(Vector3.Distance(context.Direction, Vector3.right) < 0.0001f,
                    "默认方向没有从施法者指向目标。");
                Require(context.TryGetService<ICaseDamageService>(out var resolvedService) &&
                        ReferenceEquals(resolvedService, damageService),
                    "共享战斗服务解析失败。");

                context.GetRequiredService<ICaseDamageService>().ApplyDamage(25f);
                Require(Math.Abs(damageService.TotalDamage - 25f) < 0.0001f,
                    "上下文中的战斗服务没有被调用。");

                var explicitContext = new SkillExecutionContext(caster, target,
                    new Vector3(8f, 0f, 9f), new Vector3(0f, 0f, 10f), services);
                Require(explicitContext.WorldPosition == new Vector3(8f, 0f, 9f),
                    "显式世界坐标被错误覆盖。");
                Require(Vector3.Distance(explicitContext.Direction, Vector3.forward) < 0.0001f,
                    "显式方向没有归一化。");

                var skill = new CombatSkillAsset { duration = 1f, frameRate = 20 };
                skill.Validate();
                var player = new SkillPlayer(skill);
                var contextChangedCount = 0;
                player.ContextChanged += (_, _, _) => contextChangedCount++;

                Require(player.Play(context), "带上下文播放技能失败。");
                Require(ReferenceEquals(player.Context, context), "SkillPlayer 没有保存执行上下文。");
                Require(contextChangedCount == 1, "上下文变更事件触发次数不正确。");

                player.Tick(0.25f);
                Require(Math.Abs(player.CurrentTime - 0.25f) < 0.0001f,
                    "绑定上下文后播放器没有正常推进。");

                Debug.Log("技能执行上下文验证通过。");
            }
            finally
            {
                DestroyCaseObject(caster);
                DestroyCaseObject(target);
            }
        }

        private static void DestroyCaseObject(GameObject value)
        {
            if (value == null)
                return;

            if (Application.isPlaying)
                Destroy(value);
            else
                DestroyImmediate(value);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private interface ICaseDamageService
        {
            void ApplyDamage(float amount);
        }

        private sealed class CaseDamageService : ICaseDamageService
        {
            public float TotalDamage { get; private set; }

            public void ApplyDamage(float amount)
            {
                TotalDamage += amount;
            }
        }
    }
}
