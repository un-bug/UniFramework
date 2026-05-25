using System.Collections;

namespace UniFramework
{
    public class CoroutineManager
    {
        internal static CoroutineModule m_CoroutineModuleInstance;

        internal static CoroutineModule m_CoroutineModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_CoroutineModuleInstance, "CoroutineManager");
            }
        }

        public static CoroutineTask StartTask(IEnumerator routine)
        {
            return m_CoroutineModule.StartTask(routine);
        }

        public static CoroutineTask StartTask(string name, IEnumerator routine)
        {
            return m_CoroutineModule.StartTask(name, routine);
        }

        public static void StopTask(CoroutineTask task)
        {
            m_CoroutineModule.StopTask(task);
        }
    }
}