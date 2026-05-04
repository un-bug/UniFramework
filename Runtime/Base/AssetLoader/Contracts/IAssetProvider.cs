using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public interface IAssetProvider
    {
        IAssetHandle<T> LoadAsset<T>(string key) where T : Object;
        void LoadAssetAsync<T>(string key, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : Object;
        IAssetOperation<IAssetSceneHandle> LoadSceneAsync(string key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100);
        IAssetOperation<IAssetSceneHandle> UnloadSceneAsync(IAssetSceneHandle sceneHandle, bool autoReleaseHandle = true);
    }
}