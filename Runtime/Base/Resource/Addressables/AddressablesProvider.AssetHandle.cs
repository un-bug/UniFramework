#if ENABLE_ADDRESSABLES

using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniFramework
{
    public partial class AddressablesProvider
    {
        private sealed class AssetHandle<T> : IAssetHandle<T> where T : UnityEngine.Object
        {
            private readonly AsyncOperationHandle<T> m_Handle;
            private readonly Action m_Release;
            private bool m_IsReleased;

            public bool IsValid
            {
                get
                {
                    return !m_IsReleased && m_Handle.Result != null;
                }
            }

            public T Asset
            {
                get
                {
                    if (m_IsReleased)
                    {
                        throw new ObjectDisposedException(nameof(AssetHandle<T>));
                    }

                    return m_Handle.Result;
                }
            }

            public AssetHandle(AsyncOperationHandle<T> handle, Action release)
            {
                m_Handle = handle;
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

#endif