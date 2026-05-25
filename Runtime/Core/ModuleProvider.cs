using UnityEngine;

namespace UniFramework
{
    internal static class ModuleProvider
    {
        private static Transform m_ModuleRoot;

        private static Transform ModuleRoot
        {
            get
            {
                if (m_ModuleRoot != null)
                {
                    return m_ModuleRoot;
                }

                GameObject rootObject = new GameObject("UniFramework");
                Object.DontDestroyOnLoad(rootObject);
                m_ModuleRoot = rootObject.transform;
                return m_ModuleRoot;
            }
        }

        internal static T GetModule<T>(ref T instance, string gameObjectName) where T : Component
        {
            if (instance != null)
            {
                return instance;
            }

            instance = Object.FindFirstObjectByType<T>();
            if (instance != null)
            {
                instance.transform.SetParent(ModuleRoot);
                return instance;
            }

            GameObject moduleObject = new GameObject(gameObjectName);
            moduleObject.transform.SetParent(ModuleRoot);
            instance = moduleObject.AddComponent<T>();
            return instance;
        }
    }
}