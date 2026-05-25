using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UniFramework
{
    /// <summary>
    /// 对象池包装器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class ObjectPoolWrapper<T> : IObjectPoolWrapper where T : Object
    {
        private readonly T m_Original;
        private readonly IObjectPool<T> m_Pool;
        private readonly HashSet<T> m_ActiveObjects = new HashSet<T>();
        private Transform m_Parent;

        public T Original => m_Original;
        public int ActiveCount => m_ActiveObjects.Count;
        public int CountInactive => m_Pool.CountInactive;
        public IEnumerable<T> ActiveObjects => m_ActiveObjects;
        public IObjectPool<T> Pool => m_Pool;

        /// <summary>
        /// 构造函数，初始化对象池并指定池的初始容量和最大容量。
        /// </summary>
        /// <param name="origin">池中对象的原型实例，用于创建新对象。</param>
        /// <param name="defaultCapacity">对象池的默认容量，默认为 10。</param>
        /// <param name="maxSize">对象池的最大容量，默认为 100。</param>
        public ObjectPoolWrapper(T origin, Transform originParent, int defaultCapacity = 10, int maxSize = 100)
        {
            m_Original = origin;
            m_Parent = originParent;
#if UNITY_EDITOR
            bool collectionCheck = true;
#else
            bool collectionCheck = false;
#endif
            m_Pool = new ObjectPool<T>(CreateFunc, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize);
        }

        /// <summary>
        /// 从对象池中获取一个对象。
        /// </summary>
        /// <returns>从池中获取的对象。</returns>
        public T Get() => m_Pool.Get();

        /// <summary>
        /// 将一个对象释放回对象池。
        /// </summary>
        /// <param name="obj">需要释放回池的对象。</param>
        public void Release(T obj) => m_Pool.Release(obj);

        /// <summary>
        /// 释放所有活跃的对象并将其返回池中。
        /// </summary>
        public void ReleaseAllActive()
        {
            var activeList = new List<T>(m_ActiveObjects);
            foreach (var obj in activeList)
            {
                Release(obj);
            }
        }

        /// <summary>
        /// 移除并返回已清理的空引用数量。
        /// </summary>
        public int CleanupNulls()
        {
            int removed = 0;
            var list = new List<T>(m_ActiveObjects);
            foreach (var item in list)
            {
                if (item == null)
                {
                    m_ActiveObjects.Remove(item);
                    removed++;
                }
            }

            return removed;
        }

        public void Clear()
        {
            CleanupNulls();
            ReleaseAllActive();
            m_Pool.Clear();

            Debug.Log($"[{nameof(ObjectPoolWrapper<T>)}] pool for {m_Original.name} cleared.");
        }

        private T CreateFunc()
        {
            if (m_Original is GameObject gameObj)
            {
                GameObject obj = Object.Instantiate(gameObj, m_Parent);
                return obj as T;
            }

            if (m_Original is Component comp)
            {
                Component obj = Object.Instantiate(comp, m_Parent);
                return obj as T;
            }

            return Object.Instantiate(m_Original);
        }

        private void OnGet(T obj)
        {
            if (obj is GameObject gameObj)
            {
                gameObj.SetActive(true);
                var poolable = gameObj.GetComponent<IPoolable>();
                if (poolable != null)
                {
                    poolable.OnSpawn();
                }
            }
            else if (obj is Component comp)
            {
                comp.gameObject.SetActive(true);
                var poolable = comp.GetComponent<IPoolable>();
                if (poolable != null)
                {
                    poolable.OnSpawn();
                }
            }

            m_ActiveObjects.Add(obj);
        }

        private void OnRelease(T obj)
        {
            if (obj is GameObject gameObj)
            {
                gameObj.SetActive(false);
                gameObj.transform.SetParent(m_Parent, false);
                var poolable = gameObj.GetComponent<IPoolable>();
                if (poolable != null)
                {
                    poolable.OnDespawn();
                }
            }
            else if (obj is Component comp)
            {
                comp.gameObject.SetActive(false);
                comp.transform.SetParent(m_Parent, false);
                var poolable = comp.GetComponent<IPoolable>();
                if (poolable != null)
                {
                    poolable.OnDespawn();
                }
            }

            m_ActiveObjects.Remove(obj);
        }

        private void OnDestroy(T obj)
        {
            Object.Destroy(obj);
        }
    }
}