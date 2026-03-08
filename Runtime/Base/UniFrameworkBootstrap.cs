using UnityEngine;

namespace UniFramework.Runtime
{
    internal static class UniFrameworkBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Object.FindObjectOfType<UniFrameworkDriver>() != null)
            {
                return;
            }

            var go = new GameObject("[UniFrameworkDriver]");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<UniFrameworkDriver>();
        }
    }
}