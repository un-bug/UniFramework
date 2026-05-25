using UnityEngine;

namespace UniFramework
{
    public static class ObjectPoolManager
    {
        internal static ObjectPoolModule m_ObjectPoolModuleInstance;
        internal static ObjectPoolModule ObjectPoolModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_ObjectPoolModuleInstance, "ObjectPoolManager");
            }
        }

        /// <summary>
        /// 获取指定类型的对象池，如果对象池不存在则创建一个新的池并返回。
        /// </summary>
        /// <typeparam name="T">池中管理的对象类型（必须是 MonoBehaviour 类型）。</typeparam>
        /// <param name="original">池中对象的原型实例（用于创建新对象）。</param>
        /// <param name="defaultCapacity">池的默认容量，默认为 10。</param>
        /// <param name="maxSize">池的最大容量，默认为 100。</param>
        /// <returns>指定类型的对象池。</returns>
        public static ObjectPoolWrapper<T> GetPool<T>(T original, int defaultCapacity = 10, int maxSize = 100) where T : Object
        {
            return ObjectPoolModule.GetPool<T>(original, defaultCapacity, maxSize);
        }

        /// <summary>
        /// 清除指定类型的对象池并释放相关资源。
        /// </summary>
        /// <typeparam name="T">需要清除池的对象类型。</typeparam>
        public static void ClearPool<T>(T original) where T : Object
        {
            ObjectPoolModule.ClearPool<T>(original);
        }

        /// <summary>
        /// 清除指定对象池并释放相关资源。
        /// </summary>
        /// <typeparam name="T">池中管理的对象类型。</typeparam>
        /// <param name="poolWrapper">要清除的对象池实例。</param>
        public static void ClearPool<T>(ObjectPoolWrapper<T> poolWrapper) where T : Object 
        {
            ObjectPoolModule.ClearPool(poolWrapper);
        }

        /// <summary>
        /// 清除所有对象池并释放所有资源。
        /// </summary>
        public static void ClearAllPools()
        {
            ObjectPoolModule.ClearAllPools();
        }
    }
}