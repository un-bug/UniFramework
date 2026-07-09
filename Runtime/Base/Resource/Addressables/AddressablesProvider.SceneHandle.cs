#if ENABLE_ADDRESSABLES

using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public partial class AddressablesProvider
    {
        sealed class SceneHandle : ISceneHandle
        {
            public readonly string Key;
            private readonly AsyncOperationHandle<SceneInstance> m_Handle;

            public bool IsDone => m_Handle.IsDone;

            public Scene Scene
            {
                get
                {
                    return m_Handle.Result.Scene;
                }
            }

            public SceneHandle(string sceneKey, AsyncOperationHandle<SceneInstance> handle)
            {
                Key = sceneKey;
                m_Handle = handle;
            }

            public IEnumerator WaitForCompletion()
            {
                yield return m_Handle;
            }

            public void Activate()
            {
                m_Handle.Result.ActivateAsync();
            }

            public IEnumerator ActivateAsync()
            {
                yield return m_Handle.Result.ActivateAsync();
            }

            public void UnloadAsync(Action onCompleted, Action<Exception> onFailed)
            {
                var handle = Addressables.UnloadSceneAsync(m_Handle);
                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        onCompleted?.Invoke();
                    }
                    else
                    {
                        onFailed?.Invoke(new InvalidOperationException($"failed to unload scene with key: {Key}"));
                    }
                };
            }
        }
    }
}

#endif