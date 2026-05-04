using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public sealed class AssetLoader : IAssetLoader
    {
        private readonly IAssetProvider m_AssetProvider;
        private readonly List<IDisposable> m_Handles;
        private bool m_Disposed;

        public AssetLoader(IAssetProvider assetProvider)
        {
            m_AssetProvider = assetProvider;
            m_Handles = new List<IDisposable>();
        }
        
        public T Load<T>(string key) where T : Object
        {
            return LoadAsset<T>(key).Asset;
        }

        public IAssetHandle<T> LoadAsset<T>(string key) where T : Object
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(AssetLoader));
            }

            IAssetHandle<T> handle = m_AssetProvider.LoadAsset<T>(key);
            Track(handle);
            return handle;
        }

        public void LoadAssetAsync<T>(string key, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : Object
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(AssetLoader));
            }

            m_AssetProvider.LoadAssetAsync<T>(key, handle =>
            {
                Track(handle);
                onCompleted?.Invoke(handle);
            }, onFailed);
        }

        public void Release(string key)
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(AssetLoader));
            }

            for (int i = m_Handles.Count - 1; i >= 0; i--)
            {
                if (m_Handles[i] is IAssetHandle<Object> handle && handle.Key == key)
                {
                    m_Handles[i]?.Dispose();
                    m_Handles.RemoveAt(i);
                }
            }
        }

        public void ReleaseAll()
        {
            for (int i = m_Handles.Count - 1; i >= 0; i--)
            {
                m_Handles[i]?.Dispose();
            }

            m_Handles.Clear();
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }

            m_Disposed = true;
            ReleaseAll();
        }

        public IAssetOperation<IAssetSceneHandle> LoadSceneAsync(string key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return m_AssetProvider.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        public IAssetOperation<IAssetSceneHandle> UnloadSceneAsync(IAssetSceneHandle sceneHandle, bool autoReleaseHandle = true)
        {
            return m_AssetProvider.UnloadSceneAsync(sceneHandle, autoReleaseHandle);
        }

        private void Track(IDisposable handle)
        {
            if (handle != null)
            {
                m_Handles.Add(handle);
            }
        }
    }
}
