using System;
using System.Collections.Generic;

namespace TW08.Core.Services
{
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> services = new();

        public void Register(object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            RegisterType(service.GetType(), service);
            foreach (Type interfaceType in service.GetType().GetInterfaces())
            {
                if (typeof(IGameService).IsAssignableFrom(interfaceType))
                {
                    RegisterType(interfaceType, service);
                }
            }
        }

        public T Get<T>() where T : class
        {
            if (TryGet(out T service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service '{typeof(T).FullName}' is not registered.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out object value))
            {
                service = value as T;
                return service != null;
            }

            service = null;
            return false;
        }

        public void Clear()
        {
            services.Clear();
        }

        private void RegisterType(Type type, object service)
        {
            if (services.TryGetValue(type, out object existing) && !ReferenceEquals(existing, service))
            {
                throw new InvalidOperationException($"Service type '{type.FullName}' is already registered.");
            }

            services[type] = service;
        }
    }
}
