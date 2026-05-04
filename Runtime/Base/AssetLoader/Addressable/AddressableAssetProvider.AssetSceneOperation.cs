using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UniFramework
{
    public partial class AddressableAssetProvider
    {
        private sealed class AssetSceneOperation : IAssetOperation<IAssetSceneHandle>
        {
            private readonly string m_Key;
            private readonly AsyncOperationHandle<SceneInstance> m_Handle;
            private IAssetSceneHandle m_Result;

            public bool IsDone => m_Handle.IsDone;

            public float Progress => m_Handle.PercentComplete;

            public IAssetSceneHandle Result
            {
                get
                {
                    if (!m_Handle.IsDone || m_Handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return null;
                    }

                    if (m_Result == null)
                    {
                        m_Result = new AssetSceneHandle(m_Key, m_Handle.Result);
                    }

                    return m_Result;
                }
            }

            public AssetSceneOperation(string key, AsyncOperationHandle<SceneInstance> handle)
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