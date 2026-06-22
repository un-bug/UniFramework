using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public partial class AddressableAssetProvider : IAssetProvider
    {
        private readonly Dictionary<AssetCacheKey, CachedAsset> m_CachedAssets;
        private readonly Dictionary<AssetCacheKey, AsyncOperationHandle> m_LoadingHandles;

        public AddressableAssetProvider()
        {
            m_CachedAssets = new Dictionary<AssetCacheKey, CachedAsset>();
            m_LoadingHandles = new Dictionary<AssetCacheKey, AsyncOperationHandle>();
        }

        public void Clear()
        {
            foreach (CachedAsset cachedAsset in m_CachedAssets.Values)
            {
                Addressables.Release(cachedAsset.AddressableHandle);
            }

            m_CachedAssets.Clear();
        }

        public bool HasAsset<T>(string key) where T : Object
        {
            return HasAsset(key, typeof(T));
        }

        public bool HasAsset(string key, Type assetType = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("asset key cannot be null or empty.", nameof(key));
            }

            if (assetType != null && !typeof(Object).IsAssignableFrom(assetType))
            {
                throw new ArgumentException($"asset type must inherit from UnityEngine.Object. type: {assetType}", nameof(assetType));
            }

            if (HasCachedAsset(key, assetType))
            {
                return true;
            }

            var handle = Addressables.LoadResourceLocationsAsync(key, assetType);

            try
            {
                var locations = handle.WaitForCompletion();
                return locations != null && locations.Count > 0;
            }
            finally
            {
                Addressables.Release(handle);
            }

            bool HasCachedAsset(string key, Type assetType)
            {
                if (assetType != null)
                {
                    return m_CachedAssets.ContainsKey(new AssetCacheKey(key, assetType));
                }

                foreach (AssetCacheKey cacheKey in m_CachedAssets.Keys)
                {
                    if (cacheKey.Key == key)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public IAssetHandle<T> LoadAsset<T>(string key) where T : Object
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("resource key cannot be null or empty.", nameof(key));
            }

            AssetCacheKey cacheKey = new AssetCacheKey(key, typeof(T));
            if (TryUseCachedAsset(key, cacheKey, out IAssetHandle<T> handle))
            {
                return handle;
            }

            return LoadInternal<T>(key, cacheKey);
        }

        public void LoadAssetAsync<T>(string key, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : Object
        {
            if (onCompleted == null)
            {
                throw new ArgumentNullException(nameof(onCompleted));
            }

            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("resource key cannot be null or empty.", nameof(key));
                }

                AssetCacheKey cacheKey = new AssetCacheKey(key, typeof(T));
                if (TryUseCachedAsset(key, cacheKey, out IAssetHandle<T> handle))
                {
                    onCompleted.Invoke(handle);
                    return;
                }

                LoadAsyncInternal(key, cacheKey, onCompleted, onFailed);
            }
            catch (Exception exception)
            {
                if (onFailed != null)
                {
                    onFailed.Invoke(exception);
                    return;
                }

                throw;
            }
        }

        public IAssetOperation<IAssetSceneHandle> LoadSceneAsync(string key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
            return new AssetSceneOperation(key, handle);
        }

        public IAssetOperation<IAssetSceneHandle> UnloadSceneAsync(IAssetSceneHandle sceneHandle, bool autoReleaseHandle = true)
        {
            AssetSceneHandle assetSceneHandle = sceneHandle as AssetSceneHandle;
            if (assetSceneHandle == null)
            {
                return null;
            }

            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(assetSceneHandle.SceneInstance, autoReleaseHandle);
            return new AssetSceneOperation(assetSceneHandle.Key, handle);
        }

        private bool TryUseCachedAsset<T>(string key, AssetCacheKey cacheKey, out IAssetHandle<T> handle) where T : Object
        {
            if (m_CachedAssets.TryGetValue(cacheKey, out CachedAsset cachedAsset))
            {
                cachedAsset.ReferenceCount++;
                handle = CreateHandle(key, cacheKey, (T)cachedAsset.Asset);
                return true;
            }

            handle = null;
            return false;
        }

        private IAssetHandle<T> CreateHandle<T>(string key, AssetCacheKey cacheKey, T asset) where T : Object
        {
            return new AssetHandle<T>(key, asset, () => Release(cacheKey));
        }

        private void Release(AssetCacheKey cacheKey)
        {
            if (!m_CachedAssets.TryGetValue(cacheKey, out CachedAsset cachedAsset))
            {
                return;
            }

            cachedAsset.ReferenceCount--;
            if (cachedAsset.ReferenceCount > 0)
            {
                return;
            }

            Addressables.Release(cachedAsset.AddressableHandle);
            m_CachedAssets.Remove(cacheKey);
        }

        private IAssetHandle<T> LoadInternal<T>(string key, AssetCacheKey cacheKey) where T : Object
        {
            AsyncOperationHandle<T> operationHandle = Addressables.LoadAssetAsync<T>(key);
            T asset = operationHandle.WaitForCompletion();
            if (operationHandle.Status != AsyncOperationStatus.Succeeded || asset == null)
            {
                Addressables.Release(operationHandle);
                throw new KeyNotFoundException($"failed to load addressable asset: {key} ({typeof(T).Name})");
            }

            AddCacheAsset(cacheKey, asset, operationHandle);
            return CreateHandle(key, cacheKey, asset);
        }

        private void LoadAsyncInternal<T>(string key, AssetCacheKey cacheKey, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : Object
        {
            if (m_LoadingHandles.TryGetValue(cacheKey, out AsyncOperationHandle loadingHandle))
            {
                loadingHandle.Completed += completedHandle =>
                {
                    if (completedHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        InvokeLoadFailed(onFailed, new KeyNotFoundException($"failed to load addressable asset: {key} ({typeof(T).Name})"));
                        return;
                    }

                    if (TryUseCachedAsset(key, cacheKey, out IAssetHandle<T> cached))
                    {
                        onCompleted.Invoke(cached);
                        return;
                    }

                    InvokeLoadFailed(onFailed, new KeyNotFoundException($"failed to load cached addressable asset: {key} ({typeof(T).Name})"));
                };

                return;
            }

            AsyncOperationHandle<T> operationHandle = Addressables.LoadAssetAsync<T>(key);
            m_LoadingHandles.Add(cacheKey, operationHandle);
            operationHandle.Completed += completedHandle =>
            {
                m_LoadingHandles.Remove(cacheKey);

                if (completedHandle.Status != AsyncOperationStatus.Succeeded || completedHandle.Result == null)
                {
                    Addressables.Release(completedHandle);
                    InvokeLoadFailed(onFailed, new KeyNotFoundException($"failed to load addressable asset: {key} ({typeof(T).Name})"));
                    return;
                }

                if (TryUseCachedAsset(key, cacheKey, out IAssetHandle<T> cachedHandle))
                {
                    Addressables.Release(completedHandle);
                    onCompleted.Invoke(cachedHandle);
                    return;
                }

                AddCacheAsset(cacheKey, completedHandle.Result, completedHandle);
                onCompleted.Invoke(CreateHandle(key, cacheKey, completedHandle.Result));
            };

            return;
            static void InvokeLoadFailed(Action<Exception> onFailed, Exception exception)
            {
                if (onFailed != null)
                {
                    onFailed.Invoke(exception);
                    return;
                }

                Log.Exception(exception);
            }
        }

        private void AddCacheAsset<T>(AssetCacheKey cacheKey, T asset, AsyncOperationHandle<T> operationHandle) where T : Object
        {
            m_CachedAssets.Add(cacheKey, new CachedAsset(asset, operationHandle));
        }
    }
}