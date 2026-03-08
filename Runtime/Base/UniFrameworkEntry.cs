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

        internal static T RegisterModule<T>(T module) where T : IUniFrameworkModule
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "module instance cannot be null.");
            }

            try
            {
                module.Initialize();
            }
            catch (Exception ex)
            {
                throw new Exception($"module '{module.Type.FullName}' initialize failed.", ex);
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

        internal static void UnregisterModule(IUniFrameworkModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "module instance cannot be null.");
            }

            if (s_GameDevKitModules.Remove(module))
            {
                try
                {
                    module.Shutdown();
                }
                catch (Exception ex)
                {
                    throw new Exception($"module '{module.Type.FullName}' shutdown failed.", ex);
                }
            }
        }
    }
}