using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    public class ObjectPoolManager : GameModule
    {
        private readonly Dictionary<Object, IObjectPoolWrapper> m_Pools = new Dictionary<Object, IObjectPoolWrapper>();

        public int Count
        {
            get
            {
                return m_Pools.Count;
            }
        }

        public IEnumerable<KeyValuePair<Object, IObjectPoolWrapper>> GetPools()
        {
            return m_Pools;
        }

        public ObjectPoolWrapper<T> GetPool<T>(T original, int defaultCapacity = 10, int maxSize = 100) where T : Object
        {
            if (original == null)
            {
                Debug.LogError($"[{nameof(ObjectPoolManager)}] getPool called with null original.");
                return null;
            }

            if (m_Pools.TryGetValue(original, out var value))
            {
                if (value is ObjectPoolWrapper<T> objectPoolWrapper)
                {
                    objectPoolWrapper.CleanupNulls();
                    return objectPoolWrapper;
                }
                else
                {
                    Debug.LogError($"[{nameof(ObjectPoolManager)}] get pool type mismatch.");
                }
            }

            var newPool = new ObjectPoolWrapper<T>(original, transform, defaultCapacity, maxSize);
            m_Pools.Add(original, newPool);
            return newPool;
        }

        public void ClearPool<T>(T original) where T : Object
        {
            if (original == null)
            {
                Debug.LogError($"[{nameof(ObjectPoolManager)}] clearPool called with null original.");
                return;
            }

            if (m_Pools.TryGetValue(original, out var value))
            {
                if (value is IObjectPoolWrapper objectPoolWrapper)
                {
                    objectPoolWrapper.Clear();
                    m_Pools.Remove(original);
                }
                else
                {
                    Debug.LogError($"[{nameof(ObjectPoolManager)}] clear pool type mismatch.");
                }
            }
            else
            {
                Debug.LogWarning($"[{nameof(ObjectPoolManager)}] no pool found for {original.name} to clear.");
            }
        }

        public void ClearPool<T>(ObjectPoolWrapper<T> poolWrapper) where T : Object
        {
            if (poolWrapper == null)
            {
                Debug.LogError($"[{nameof(ObjectPoolManager)}] clearPool called with null poolWrapper.");
                return;
            }

            var original = poolWrapper.Original;
            ClearPool(original);
        }

        public void ClearAllPools()
        {
            var clearList = new List<Object>(m_Pools.Keys);
            for (int i = clearList.Count - 1; i >= 0; i--)
            {
                Object original = clearList[i];
                ClearPool(original);
            }

            m_Pools.Clear();
        }
    }
}