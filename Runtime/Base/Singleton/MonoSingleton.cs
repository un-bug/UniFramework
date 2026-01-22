using UnityEngine;

namespace UniFramework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected static GameObject s_Root;
        private static bool s_IsShutDown = false;
        private static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance;
                }

                s_Instance = FindObjectOfType<T>();

                if (s_Instance != null)
                {
                    return s_Instance;
                }

                if (s_IsShutDown)
                {
                    Debug.LogWarning($"instance of {typeof(T).Name} already destroyed. Returning null.");
                    return null;
                }

                if (s_Root == null)
                {
                    s_Root = GameObject.Find("[MonoSingleton]") ?? new GameObject("[MonoSingleton]");
                    DontDestroyOnLoad(s_Root);
                }

                GameObject go = new GameObject($"[{typeof(T).Name}]");
                go.transform.SetParent(s_Root.transform);
                s_Instance = go.AddComponent<T>();
                return s_Instance;
            }
        }

        private void Awake()
        {
            if (s_Instance && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this as T;
            Debug.Log($"{typeof(T).Name} initialized.");
            OnInit();
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                OnDispose();
                s_Instance = null;
                Debug.Log($"{typeof(T).Name} disposed.");
            }
        }

        private void OnApplicationQuit()
        {
            s_IsShutDown = true;
        }

        protected virtual void OnInit() { }

        protected virtual void OnDispose() { }
    }
}