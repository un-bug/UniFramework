using System;
using System.Collections.Generic;

namespace UniFramework.Runtime
{
    internal static class UniFrameworkEntry
    {
        private static readonly LinkedList<IUniFrameworkModule> s_GameDevKitModules = new LinkedList<IUniFrameworkModule>();

        public static void Update(float deltaTime)
        {
            foreach (var module in s_GameDevKitModules)
            {
                module.OnUpdate(deltaTime);
            }
        }

        public static void Shutdown()
        {
            for (var current = s_GameDevKitModules.Last; current != null;)
            {
                var previous = current.Previous;
                current.Value.Shutdown();
                current = previous;
            }

            s_GameDevKitModules.Clear();
        }

        public static T GetModule<T>(Func<T> factory) where T : UniFrameworkModule<T>
        {
            EnsureModuleRegistered();
            Type type = typeof(T);
            foreach (var module in s_GameDevKitModules)
            {
                if (module.GetType() == type)
                {
                    return module as T;
                }
            }

            T newModule = factory?.Invoke();
            RegisterModule(newModule);
            return newModule;
        }

        private static T RegisterModule<T>(T module) where T : UniFrameworkModule<T>
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "module instance cannot be null.");
            }

            var current = s_GameDevKitModules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                s_GameDevKitModules.AddBefore(current, module);
            }
            else
            {
                s_GameDevKitModules.AddLast(module);
            }

            return module;
        }
        
        private static void EnsureModuleRegistered()
        {
            foreach (var module in s_GameDevKitModules)
            {
                if (module.GetType() == typeof(BaseManager))
                {
                    return;
                }
            }

            var baseModule = BaseManager.Instance;
            if (baseModule != null)
            {
                RegisterModule(baseModule);
            }
        }
    }
}