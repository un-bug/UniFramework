using System;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public interface IAssetLoader : IDisposable
    {
        [Obsolete("Use LoadAsset<T> instead.")]
        T Load<T>(string key) where T : Object;
        IAssetHandle<T> LoadAsset<T>(string key) where T : Object;
        void LoadAssetAsync<T>(string key, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : Object;
        void Release(string key);
        IAssetOperation<IAssetSceneHandle> LoadSceneAsync(string key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100);
        IAssetOperation<IAssetSceneHandle> UnloadSceneAsync(IAssetSceneHandle sceneHandle, bool autoReleaseHandle = true);
    }
}