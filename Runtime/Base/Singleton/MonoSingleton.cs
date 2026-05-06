using UnityEngine;

namespace UniFramework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private const string RootName = "[MonoSingleton]";
        private static bool s_IsQuitting;
        private static T s_Instance;
        protected static GameObject s_Root;

        protected virtual bool IsDontDestroyOnLoad => true;
        public static bool HasInstance => s_Instance != null;
        public static T Instance
        {
            get
            {
                if (s_IsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] instance of {typeof(T).Name} requested while application is quitting.");
                    return null;
                }

                return GetOrCreateInstance();
            }
        }

        private void Awake()
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
                if (s_Root == null)
                {
                    s_Root = GameObject.Find(RootName) ?? new GameObject(RootName);
                    DontDestroyOnLoad(s_Root);
                }

                if (transform.parent != s_Root.transform)
                {
                    transform.SetParent(s_Root.transform);
                }
            }

            Debug.Log($"[MonoSingleton] {typeof(T).Name} created.", gameObject);
            OnInit();
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                OnDispose();
                s_Instance = null;
                Debug.Log($"[MonoSingleton] {typeof(T).Name} disposed.");
            }
        }

        private void OnApplicationQuit()
        {
            s_IsQuitting = true;
        }

        private void Update()
        {
            OnUpdate(Time.deltaTime);
        }

        protected virtual void OnInit()
        { }

        protected virtual void OnDispose()
        { }

        protected virtual void OnUpdate(float deltaTime)
        { }

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