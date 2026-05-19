using UnityEngine;

namespace UniFramework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T s_Instance;

        private bool m_Initialized;
        private bool m_Released;

        protected virtual bool IsDontDestroyOnLoad => true;
        public static bool HasInstance => s_Instance != null;
        public static T Instance
        {
            get
            {
                if (MonoSingletonRuntime.IsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] instance of {typeof(T).Name} requested while application is quitting.");
                    return null;
                }

                return GetOrCreateInstance();
            }
        }

        protected virtual void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] duplicate instance of {typeof(T).Name} destroyed.", gameObject);
                Destroy(gameObject);
                return;
            }

            s_Instance = this as T;

            if (IsDontDestroyOnLoad)
            {
                Transform root = MonoSingletonRuntime.RootTransform;
                if (transform.parent != root.transform)
                {
                    transform.SetParent(root.transform);
                }
            }

            InitSingleton();
            Debug.Log($"[MonoSingleton] {typeof(T).Name} created.", gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (s_Instance == this)
            {
                ReleaseSingleton();
                s_Instance = null;
                Debug.Log($"[MonoSingleton] {typeof(T).Name} disposed.");
            }
        }

        protected virtual void OnSingletonInit() { }

        protected virtual void OnSingletonRelease() { }

        private void InitSingleton()
        {
            if (m_Initialized)
            {
                return;
            }

            OnSingletonInit();
            m_Initialized = true;
        }

        private void ReleaseSingleton()
        {
            if (m_Released)
            {
                return;
            }

            m_Released = true;
            OnSingletonRelease();
        }

        private static T GetOrCreateInstance()
        {
            if (s_Instance != null)
            {
                return s_Instance;
            }

            s_Instance = FindFirstObjectByType<T>();

            if (s_Instance != null)
            {
                return s_Instance;
            }

            GameObject gameObj = new GameObject($"[{typeof(T).Name}]");
            s_Instance = gameObj.AddComponent<T>();
            return s_Instance;
        }
    }
}