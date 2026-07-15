#if ENABLE_ADDRESSABLES

using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public partial class AddressablesProvider : IResourceProvider
    {
        public IEnumerator InitializeAsync()
        {
            yield return Addressables.InitializeAsync();
        }

        public IAssetHandle<T> LoadAsset<T>(string assetKey) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                throw new ArgumentException("resource key cannot be null or empty.", nameof(assetKey));
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            var handle = Addressables.LoadAssetAsync<T>(assetKey);
            handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                throw new InvalidOperationException($"failed to load asset key: {assetKey} ({typeof(T).Name})");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            sw.Stop();
            UnityEngine.Debug.Log($"[Addressables] load succeed! assetKey={assetKey}, type={typeof(T).Name}, cost={sw.Elapsed.TotalMilliseconds:F1}ms");
#endif

            IAssetHandle<T> assetHandle = new AssetHandle<T>(handle, () => Addressables.Release(handle));
            return assetHandle;
        }

        public void LoadAssetAsync<T>(string assetKey, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                throw new ArgumentException("resource key cannot be null or empty.", nameof(assetKey));
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            var handle = Addressables.LoadAssetAsync<T>(assetKey);
            handle.Completed += operation =>
            {
                if (operation.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(operation);
                    onFailed?.Invoke(new InvalidOperationException($"failed to load asset key: {assetKey} ({typeof(T).Name})"));
                    return;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                sw.Stop();
                UnityEngine.Debug.Log($"[Addressables] load succeed! assetKey={assetKey}, type={typeof(T).Name}, cost={sw.Elapsed.TotalMilliseconds:F1}ms");
#endif

                IAssetHandle<T> assetHandle = new AssetHandle<T>(handle, () => Addressables.Release(handle));
                onCompleted?.Invoke(assetHandle);
            };
        }

        public void UnloadAsset(IAssetHandle assetHandle)
        {
            if (assetHandle != null)
            {
                assetHandle.Release();
                return;
            }
        }

        public ISceneHandle LoadSceneAsync(string sceneKey, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            AsyncOperationHandle<SceneInstance> handle = default;

            try
            {
                handle = Addressables.LoadSceneAsync(sceneKey, loadMode, activateOnLoad, priority);
            }
            catch (Exception)
            {
                Addressables.Release(handle);
                throw;
            }

            return new SceneHandle(sceneKey, handle);
        }

        public void UnloadSceneAsync(ISceneHandle sceneHandle, Action onCompleted = null, Action<Exception> onFailed = null)
        {
            var addressableSceneHandle = sceneHandle as SceneHandle;
            if (addressableSceneHandle == null)
            {
                onFailed?.Invoke(new InvalidOperationException("scene handle was not created by AddressableSceneLoader."));
                return;
            }

            addressableSceneHandle.UnloadAsync(onCompleted, onFailed);
        }
    }
}

#endif