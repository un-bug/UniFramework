using UnityEngine;

namespace UniFramework
{
    internal static class ModuleProvider
    {
        private const string ModuleRootName = "UniFramework";
        private static Transform m_ModuleRoot;
        public static bool IsQuitting { get; private set; }
        private static Transform ModuleRoot
        {
            get
            {
                if (m_ModuleRoot != null)
                {
                    return m_ModuleRoot;
                }

                GameObject rootObject = new GameObject(ModuleRootName);
                Object.DontDestroyOnLoad(rootObject);
                m_ModuleRoot = rootObject.transform;
                return m_ModuleRoot;
            }
        }

        public static T GetModule<T>(ref T instance, string gameObjectName) where T : UniFrameworkModule
        {
            if (instance != null)
            {
                return instance;
            }

            if (IsQuitting)
            {
                return null;
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapModule()
        {
            IsQuitting = false;
            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            IsQuitting = true;
        }
    }
}