using UnityEngine;

namespace UniFramework
{
    internal static class MonoSingletonRuntime
    {
        public const string RootName = "[MonoSingleton]";
        private static GameObject s_Root;
        public static bool IsQuitting { get; private set; }
        public static Transform RootTransform
        {
            get
            {
                if (s_Root == null)
                {
                    s_Root = GameObject.Find(RootName);

                    if (s_Root == null)
                    {
                        s_Root = new GameObject(RootName);
                    }

                    Object.DontDestroyOnLoad(s_Root);
                }

                return s_Root.transform;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
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