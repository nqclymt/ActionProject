using System;
using System.Collections.Generic;

namespace PKC.ActionEditor
{
    public interface ICombatServiceProvider
    {
        bool TryGetService<T>(out T service) where T : class;
    }

    /// <summary>
    /// 保存一次战斗流程共享的服务。建议按接口类型显式注册服务。
    /// </summary>
    public sealed class CombatServiceRegistry : ICombatServiceProvider
    {
        private readonly Dictionary<Type, object> services = new();

        public CombatServiceRegistry Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            services[typeof(T)] = service;
            return this;
        }

        public bool Remove<T>() where T : class
        {
            return services.Remove(typeof(T));
        }

        public void Clear()
        {
            services.Clear();
        }

        public bool TryGetService<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out var value) && value is T typedService)
            {
                service = typedService;
                return true;
            }

            service = null;
            return false;
        }

        public T GetRequiredService<T>() where T : class
        {
            if (TryGetService<T>(out var service))
                return service;

            throw new InvalidOperationException($"Combat service '{typeof(T).FullName}' is not registered.");
        }
    }

    internal sealed class EmptyCombatServiceProvider : ICombatServiceProvider
    {
        public static readonly EmptyCombatServiceProvider Instance = new();

        private EmptyCombatServiceProvider()
        {
        }

        public bool TryGetService<T>(out T service) where T : class
        {
            service = null;
            return false;
        }
    }
}
