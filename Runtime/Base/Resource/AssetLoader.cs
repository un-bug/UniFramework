using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public sealed class AssetLoader : IAssetLoader
    {
        private readonly IResourceProvider m_ResourceLoader;
        private readonly List<IAssetHandle> m_Handles;
        private bool m_Disposed;

        public AssetLoader(IResourceProvider resourceLoader)
        {
            m_ResourceLoader = resourceLoader;
            m_Handles = new List<IAssetHandle>();
        }
        
        public T Load<T>(string assetKey) where T : Object
        {
            return LoadAsset<T>(assetKey).Asset;
        }

        public IAssetHandle<T> LoadAsset<T>(string assetKey) where T : Object
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(AssetLoader));
            }

            IAssetHandle<T> handle = m_ResourceLoader.LoadAsset<T>(assetKey);
            Track(handle);
            return handle;
        }

        public void LoadAssetAsync<T>(string assetKey, Action<IAssetHandle<T>> onCompleted, Action<Exception> onFailed = null) where T : Object
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(AssetLoader));
            }

            m_ResourceLoader.LoadAssetAsync<T>(assetKey, handle =>
            {
                Track(handle);
                onCompleted?.Invoke(handle);
            }, onFailed);
        }

        public void Release(string assetKey)
        {
        }

        public void ReleaseAll()
        {
            for (int i = m_Handles.Count - 1; i >= 0; i--)
            {
                m_Handles[i]?.Release();
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

        private void Track(IAssetHandle handle)
        {
            if (handle != null)
            {
                m_Handles.Add(handle);
            }
        }
    }
}
