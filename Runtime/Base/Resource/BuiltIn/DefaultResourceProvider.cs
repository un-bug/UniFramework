using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public partial class DefaultResourceProvider : IResourceProvider
    {
        public IEnumerator InitializeAsync()
        {
            yield return null;
        }

        public IAssetHandle<T> LoadAsset<T>(string assetKey) where T : UnityEngine.Object
        {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            var asset = Resources.Load<T>(assetKey);
            if (asset == null)
            {
                throw new InvalidOperationException($"failed to load asset with key: {assetKey}");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            sw.Stop();
            UnityEngine.Debug.Log($"[Resources] load succeed! assetKey={assetKey}, type={typeof(T).Name}, cost={sw.Elapsed.TotalMilliseconds:F1}ms");
#endif

            AssetHandle<T> assetHandle = null;
            assetHandle = new AssetHandle<T>(assetKey, asset, ()=> { });
            return assetHandle;
        }

        public void LoadAssetAsync<T>(string assetKey, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : UnityEngine.Object
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            var request = Resources.LoadAsync<T>(assetKey);
            request.completed += operation =>
            {
                var asset = request.asset as T;
                if (asset == null)
                {
                    onFailed?.Invoke(new InvalidOperationException($"failed to load asset with key: {assetKey}"));
                    return;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                sw.Stop();
                UnityEngine.Debug.Log($"[Resources] load succeed! assetKey={assetKey}, type={typeof(T).Name}, cost={sw.Elapsed.TotalMilliseconds:F1}ms");
#endif
                AssetHandle<T> assetHandle = null;
                assetHandle = new AssetHandle<T>(assetKey, asset, () => { });
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
            AsyncOperation operation;

            try
            {
                operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneKey, loadMode);
                operation.allowSceneActivation = activateOnLoad;
            }
            catch (Exception)
            {
                throw;
            }
            
            return new SceneHandle(sceneKey, operation, () => GetScene(sceneKey));
        }

        public void UnloadSceneAsync(ISceneHandle sceneHandle, Action onCompleted = null, Action<Exception> onFailed = null)
        {
            var buildSceneHandle = sceneHandle as SceneHandle;
            if (buildSceneHandle == null)
            {
                onFailed?.Invoke(new InvalidOperationException("scene handle was not created by BuildSceneLoader."));
                return;
            }

            buildSceneHandle.UnloadAsync(onCompleted, onFailed);
        }

        private Scene GetScene(string sceneKey)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(sceneKey);
            if (scene.IsValid())
            {
                return scene;
            }

            return UnityEngine.SceneManagement.SceneManager.GetSceneByName(System.IO.Path.GetFileNameWithoutExtension(sceneKey));
        }
    }
}