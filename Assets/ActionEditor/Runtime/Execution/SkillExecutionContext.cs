using System;
using UnityEngine;

namespace PKC.ActionEditor
{
    /// <summary>
    /// 描述一次技能执行所需的角色、空间信息和共享战斗服务。
    /// </summary>
    public sealed class SkillExecutionContext
    {
        public static SkillExecutionContext Empty { get; } = new();

        public SkillExecutionContext(GameObject caster = null, GameObject target = null,
            Vector3? worldPosition = null, Vector3? direction = null,
            ICombatServiceProvider services = null)
        {
            Caster = caster;
            Target = target;
            WorldPosition = worldPosition ?? ResolveWorldPosition(caster, target);
            Direction = ResolveDirection(caster, target, direction);
            Services = services ?? EmptyCombatServiceProvider.Instance;
        }

        public GameObject Caster { get; }

        public GameObject Target { get; }

        public Vector3 WorldPosition { get; }

        public Vector3 Direction { get; }

        public ICombatServiceProvider Services { get; }

        public bool HasCaster => Caster != null;

        public bool HasTarget => Target != null;

        public bool TryGetService<T>(out T service) where T : class
        {
            return Services.TryGetService(out service);
        }

        public T GetRequiredService<T>() where T : class
        {
            if (TryGetService<T>(out var service))
                return service;

            throw new InvalidOperationException($"Combat service '{typeof(T).FullName}' is not available.");
        }

        private static Vector3 ResolveWorldPosition(GameObject caster, GameObject target)
        {
            if (target != null)
                return target.transform.position;
            if (caster != null)
                return caster.transform.position;

            return Vector3.zero;
        }

        private static Vector3 ResolveDirection(GameObject caster, GameObject target, Vector3? direction)
        {
            if (direction.HasValue && direction.Value.sqrMagnitude > Mathf.Epsilon)
                return direction.Value.normalized;

            if (caster != null && target != null)
            {
                var targetDirection = target.transform.position - caster.transform.position;
                if (targetDirection.sqrMagnitude > Mathf.Epsilon)
                    return targetDirection.normalized;
            }

            if (caster != null && caster.transform.forward.sqrMagnitude > Mathf.Epsilon)
                return caster.transform.forward.normalized;

            return Vector3.forward;
        }
    }
}
