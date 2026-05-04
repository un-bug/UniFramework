using System;
using Object = UnityEngine.Object;

namespace UniFramework
{

    public partial class AddressableAssetProvider
    {
        private sealed class AssetHandle<T> : IAssetHandle<T> where T : Object
        {
            private Action m_Release;

            public string Key { get; }

            public T Asset { get; }

            public bool IsValid => m_Release != null && Asset != null;

            public AssetHandle(string key, T asset, Action release)
            {
                Key = key;
                Asset = asset;
                m_Release = release;
            }

            public void Dispose()
            {
                Action release = m_Release;
                m_Release = null;
                release?.Invoke();
            }
        }
    }
}