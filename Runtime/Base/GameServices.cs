using UnityEngine;

namespace UniFramework
{
    public static class GameServices
    {
        private const string ModuleRootName = "UniFramework";
        private static bool IsQuitting { get; set; }
        private static Transform m_ModuleRoot;
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

        public static T Get<T>() where T : GameModule
        {
            if (ModuleCache<T>.Instance != null)
            {
                return ModuleCache<T>.Instance;
            }

            if (IsQuitting)
            {
                return null;
            }

            var instance = Object.FindFirstObjectByType<T>();
            if (instance == null)
            {
                var moduleObject = new GameObject(typeof(T).Name);
                instance = moduleObject.AddComponent<T>();
            }

            instance.transform.SetParent(ModuleRoot, false);
            ModuleCache<T>.Instance = instance;

            return instance;
        }

        private static class ModuleCache<T> where T : GameModule
        {
            public static T Instance;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
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