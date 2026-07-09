using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public partial class DefaultResourceProvider
    {
        sealed class SceneHandle : ISceneHandle
        {
            public readonly string Key;
            private readonly AsyncOperation m_Operation;
            private readonly Func<Scene> m_GetScene;

            public Scene Scene
            {
                get
                {
                    return m_GetScene.Invoke();
                }
            }

            public bool IsDone
            {
                get
                {
                    if (m_Operation == null)
                    {
                        return false;
                    }

                    if (m_Operation.allowSceneActivation)
                    {
                        return m_Operation.isDone;
                    }
                    else
                    {
                        return m_Operation.progress >= 0.9f;
                    }
                }
            }

            public SceneHandle(string sceneKey, AsyncOperation operation, Func<Scene> getScene)
            {
                Key = sceneKey;
                m_Operation = operation;
                m_GetScene = getScene;
            }

            public IEnumerator WaitForCompletion()
            {
                while (IsDone == false)
                {
                    yield return null;
                }
            }

            public void Activate()
            {
                m_Operation.allowSceneActivation = true;
            }

            public IEnumerator ActivateAsync()
            {
                Activate();
                yield return null;
            }

            public void UnloadAsync(Action onCompleted, Action<Exception> onFailed)
            {
                var scene = m_GetScene();
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    onFailed?.Invoke(new InvalidOperationException($"failed to unload scene with key: {Key}"));
                    return;
                }

                var operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                if (operation == null)
                {
                    onFailed?.Invoke(new InvalidOperationException($"failed to unload scene with key: {Key}"));
                    return;
                }

                operation.completed += _ =>
                {
                    onCompleted?.Invoke();
                };
            }
        }
    }
}