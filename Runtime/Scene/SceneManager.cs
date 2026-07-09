using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniFramework
{
    public delegate void LoadSceneSuccessHandler(Scene sceneInstance, string sceneName, object userData);

    public delegate void LoadSceneFailedHandler(string sceneName, object userData);

    public sealed class SceneManager : GameModule
    {
        public event LoadSceneSuccessHandler LoadSceneSuccess;
        public event LoadSceneFailedHandler LoadSceneFailed;

        private ISceneLoader m_SceneLoader;
        private ISceneLoadingScreen m_SceneLoadingScreen = null;
        private DefaultSceneLoadingScreen m_DefaultLoadingScreen = null;
        private bool m_IsLoading = false;

        private void Awake()
        {
            m_SceneLoader = ResourceManager.CreateSceneLoader();
        }

        private DefaultSceneLoadingScreen DefaultLoadingScreen
        {
            get
            {
                if (m_DefaultLoadingScreen == null)
                {
                    m_DefaultLoadingScreen = new DefaultSceneLoadingScreen();
                }
                return m_DefaultLoadingScreen;
            }
        }

        public void SetSceneLoadingScreen(ISceneLoadingScreen loadingScreen)
        {
            m_SceneLoadingScreen = loadingScreen;
            Debug.Log($"[SceneManager] setting loading screen: {loadingScreen}");
        }
        
        public void LoadScene(string mainScene)
        {
            LoadScene(mainScene, userData: null, subScenes: Array.Empty<string>());
        }

        public void LoadScene(string mainScene, params string[] subScenes)
        {
            LoadScene(mainScene, userData: null, subScenes: subScenes);
        }

        public void LoadScene(string mainScene, object userData, params string[] subScenes)
        {
            if (m_IsLoading)
            {
                Debug.LogWarning("[SceneManager] scene is already loading, please wait...");
                return;
            }

            if (string.IsNullOrEmpty(mainScene))
            {
                Debug.LogError("[SceneManager] main scene name is invalid.");
                return;
            }

            if (subScenes != null && subScenes.Length > 0)
            {
                Debug.Log($"[SceneManager] start loading main scene: {mainScene}, additive scenes: {string.Join(", ", subScenes)}");
            }
            else
            {
                Debug.Log($"[SceneManager] start loading main scene: {mainScene}");
            }

            m_IsLoading = true;
            StartCoroutine(LoadSceneInternal(mainScene, subScenes, userData));
        }

        private IEnumerator LoadSceneInternal(string mainScene, string[] subScenes, object userData)
        {
            ISceneLoadingScreen sceneTransition = m_SceneLoadingScreen ?? DefaultLoadingScreen;
            sceneTransition?.OnSceneLoadBegin(mainScene, subScenes, userData);
            Debug.Log("[SceneManager] asset preloading...");
            yield return sceneTransition?.OnScenePreload(mainScene, subScenes, userData);

            // start loadScene.
            
            var mainHandle = m_SceneLoader.LoadSceneAsync(mainScene, LoadSceneMode.Single, false, 100);
            yield return mainHandle.WaitForCompletion();
            if (mainHandle.IsDone == false)
            {
                Debug.LogError($"[SceneManager] failed to load main scene: {mainScene}");
                LoadSceneFailed?.Invoke(mainScene, userData);
                yield break;
            }
            
            // end loadScene.
            
            yield return null;
            yield return mainHandle.ActivateAsync();
            yield return null;
            foreach (string addScene in subScenes)
            {
                var addHandle = m_SceneLoader.LoadSceneAsync(addScene, LoadSceneMode.Additive);
                yield return addHandle.WaitForCompletion();

                if (addHandle.IsDone == false)
                {
                    Debug.LogError($"[SceneManager] failed to load additive scene: {addScene}");
                    LoadSceneFailed?.Invoke(addScene, userData);
                }
            }

            sceneTransition?.OnSceneLoadEnd(mainScene, subScenes, userData);
            Debug.Log($"[SceneManager] all requested scenes loaded. main scene: {mainScene}");
            LoadSceneSuccess?.Invoke(mainHandle.Scene, mainScene, userData);
            m_IsLoading = false;
        }
    }
}