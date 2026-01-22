#if ENABLE_ADDRESSABLES

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace UniFramework.Runtime
{
    public class AddressableLoader : IAssetLoader
    {
        private readonly Dictionary<string, AsyncOperationHandle> m_AsyncOperations = new Dictionary<string, AsyncOperationHandle>();

        public T Load<T>(string key) where T : Object
        {
            if (m_AsyncOperations.TryGetValue(key, out var handle))
            {
                return handle.Result as T;
            }

            handle = Addressables.LoadAssetAsync<T>(key);
            var result = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                throw new Exception($"[AddressableLoader] load failed : {key}");
            }

            m_AsyncOperations.TryAdd(key, handle);
            return result as T;
        }

        public void LoadAsync<T>(string key, Action<T> onComplete) where T : Object
        {
            if (m_AsyncOperations.TryGetValue(key, out var handle))
            {
                onComplete?.Invoke(handle.Result as T);
                return;
            }

            handle = Addressables.LoadAssetAsync<T>(key);
            handle.Completed += operation =>
            {
                if (operation.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[AddressableLoader] load failed : {key}");
                    Addressables.Release(operation);
                    onComplete?.Invoke(null);
                    return;
                }

                m_AsyncOperations.TryAdd(key, operation);
                onComplete?.Invoke(operation.Result as T);
            };
        }

        public void Release(string key)
        {
            if (m_AsyncOperations.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                m_AsyncOperations.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var handle in m_AsyncOperations.Values)
            {
                Addressables.Release(handle);
            }

            m_AsyncOperations.Clear();
        }
    }
}

#endif