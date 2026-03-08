using UnityEngine;

namespace UniFramework.Runtime
{
    [DefaultExecutionOrder(-1000)]
    internal class UniFrameworkDriver : MonoBehaviour
    {
        private bool m_IsShutdown;

        private void Update()
        {
            UniFrameworkEntry.Update(Time.deltaTime);
        }

        private void OnApplicationQuit()
        {
            PerformShutdown();
        }

        private void OnDestroy()
        {
            PerformShutdown();
        }

        private void PerformShutdown()
        {
            if (m_IsShutdown)
            {
                return;
            }

            m_IsShutdown = true;
            UniFrameworkEntry.Shutdown();
        }
    }
}