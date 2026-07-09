using System;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public sealed class SceneLoader : ISceneLoader
    {
        private readonly IResourceProvider m_ResourceLoader;

        public SceneLoader(IResourceProvider resourceLoader)
        {
            m_ResourceLoader = resourceLoader;
        }

        public ISceneHandle LoadSceneAsync(string sceneKey, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return m_ResourceLoader.LoadSceneAsync(sceneKey, loadMode, activateOnLoad, priority);
        }

        public void UnloadSceneAsync(ISceneHandle sceneHandle, Action onCompleted = null, Action<Exception> onFailed = null)
        {
            m_ResourceLoader.UnloadSceneAsync(sceneHandle, onCompleted, onFailed);
        }
    }
}
