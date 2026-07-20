using UnityEngine;

namespace UniFramework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T s_Instance;
        protected virtual bool IsDontDestroyOnLoad => true;
        public static bool HasInstance => s_Instance != null;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<T>();
                    if (s_Instance == null)
                    {
                        GameObject gameObject = new GameObject($"[{typeof(T).Name}]");
                        s_Instance = gameObject.AddComponent<T>();
                    }
                }

                return s_Instance;
            }
        }

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this as T;
                if (IsDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }

                OnInitialize();
                return;
            }

            if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
                OnRelease();
            }
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnRelease()
        {
        }
    }
}