using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public partial class AddressableAssetProvider
    {
        public class AssetOperation<T> : IAssetOperation<IAssetHandle<T>> where T : Object
        {
            private readonly string m_Key;
            private readonly AsyncOperationHandle<T> m_Handle;
            private IAssetHandle<T> m_Result;

            public bool IsDone => m_Handle.IsDone;

            public float Progress => m_Handle.PercentComplete;

            public IAssetHandle<T> Result
            {
                get
                {
                    if (!m_Handle.IsDone || m_Handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return null;
                    }

                    if (m_Result == null)
                    {
                        m_Result = new AssetHandle<T>(m_Key, m_Handle.Result, () => Addressables.Release(m_Handle));
                    }

                    return m_Result;
                }
            }

            public AssetOperation(string key, AsyncOperationHandle<T> handle)
            {
                m_Key = key;
                m_Handle = handle;
            }

            public void Dispose()
            {
                if (m_Handle.IsValid())
                {
                    Addressables.Release(m_Handle);
                }
            }

            public IEnumerator WaitForCompletion()
            {
                while (!IsDone)
                {
                    yield return null;
                }
            }
        }
    }
}