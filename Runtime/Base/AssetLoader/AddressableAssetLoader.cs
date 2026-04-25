using System.Collections.Generic;

namespace UniFramework
{
    public class AddressableAssetLoader : IAssetLoader
    {
        private sealed class AssetHandleInfo
        {
            public UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle Handle;
            public int ReferenceCount;

            public AssetHandleInfo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle handle)
            {
                Handle = handle;
                ReferenceCount = 1;
            }
        }

        private Dictionary<string, AssetHandleInfo> m_AssetHandleInfos = new();

        public T Load<T>(string key) where T : UnityEngine.Object
        {
            if (m_AssetHandleInfos.TryGetValue(key, out AssetHandleInfo assetHandleInfo))
            {
                assetHandleInfo.ReferenceCount++;
                return (T)assetHandleInfo.Handle.Result;
            }

            var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
            handle.WaitForCompletion();

            m_AssetHandleInfos.Add(key, new AssetHandleInfo(handle));
            return handle.Result;
        }

        public void Release(string key)
        {
            if (!m_AssetHandleInfos.TryGetValue(key, out AssetHandleInfo assetHandleInfo))
            {
                return;
            }

            assetHandleInfo.ReferenceCount--;
            if (assetHandleInfo.ReferenceCount > 0)
            {
                return;
            }

            UnityEngine.AddressableAssets.Addressables.Release(assetHandleInfo.Handle);
            m_AssetHandleInfos.Remove(key);
        }

        public void Dispose()
        {
            foreach (AssetHandleInfo assetHandleInfo in m_AssetHandleInfos.Values)
            {
                UnityEngine.AddressableAssets.Addressables.Release(assetHandleInfo.Handle);
            }

            m_AssetHandleInfos.Clear();
        }
    }
}
