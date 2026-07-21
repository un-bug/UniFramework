using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public interface IAssetHandle
    {
        bool IsValid { get; }
        void Release();
    }

    public interface IAssetHandle<out T> : IAssetHandle where T : UnityEngine.Object
    {
        T Result { get; }
    }
    
    public interface IAssetLoader : IDisposable
    {
        IAssetHandle<T> LoadAsset<T>(string assetKey) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string assetKey, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : UnityEngine.Object;
        void UnloadAsset(IAssetHandle assetHandle);
        void UnloadAllAssets();
    }

    public interface ISceneHandle
    {
        Scene Scene { get; }
        bool IsDone { get; }
        IEnumerator WaitForCompletion();
        void Activate();
        IEnumerator ActivateAsync();
    }

    public interface ISceneLoader
    {
        ISceneHandle LoadSceneAsync(string sceneKey, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100);
        void UnloadSceneAsync(ISceneHandle sceneHandle, Action onCompleted = null, Action<Exception> onFailed = null);
    }

    public interface IResourceProvider
    {
        IEnumerator InitializeAsync();
        IAssetHandle<T> LoadAsset<T>(string assetKey) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string assetKey, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : UnityEngine.Object;
        void UnloadAsset(IAssetHandle assetHandle);
        ISceneHandle LoadSceneAsync(string sceneKey, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100);
        void UnloadSceneAsync(ISceneHandle sceneHandle, Action onCompleted = null, Action<Exception> onFailed = null);
    }

    public static class ResourceManager
    {
        private static IResourceProvider s_Provider;
        public static IResourceProvider Provider
        {
            get
            {
                if (s_Provider == null)
                {
#if ENABLE_ADDRESSABLES
                    s_Provider = new AddressablesProvider();
#else
                    s_Provider = new DefaultResourceProvider();
#endif
                }

                return s_Provider;
            }
        }
        
        public static IEnumerator InitializeAsync()
        {
            yield return Provider.InitializeAsync();
        }

        public static IAssetLoader CreateAssetLoader()
        {
            return new AssetLoader(Provider);
        }
        
        public static ISceneLoader CreateSceneLoader()
        {
            return new SceneLoader(Provider);
        }
    }
}