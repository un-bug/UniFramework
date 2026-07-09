using System;

namespace UniFramework
{
    public partial class DefaultResourceProvider
    {
        sealed class AssetHandle<T> : IAssetHandle<T> where T : UnityEngine.Object
        {
            public readonly string Key;
            private readonly T m_Asset;
            private readonly Action m_Release;
            private bool m_IsReleased;

            public bool IsValid
            {
                get
                {
                    return !m_IsReleased && m_Asset != null;
                }
            }

            public T Asset
            {
                get
                {
                    if (m_IsReleased)
                    {
                        throw new InvalidOperationException("asset handle has been released.");
                    }

                    return m_Asset;
                }
            }

            public AssetHandle(string assetKey, T asset, Action release)
            {
                Key = assetKey;
                m_Asset = asset;
                m_Release = release;
            }

            public void Release()
            {
                if (m_IsReleased)
                {
                    return;
                }

                m_IsReleased = true;
                m_Release?.Invoke();
            }
        }
    }
}