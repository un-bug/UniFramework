using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace UniFramework.Runtime
{
    public delegate void LoadSceneSuccessHandler(Scene sceneInstance, string sceneName, object userData);

    public delegate void LoadSceneFailedHandler(string sceneName, object userData);

    [DisallowMultipleComponent]
    public sealed class SceneManager : UniFrameworkModule<SceneManager>
    {
        public event LoadSceneSuccessHandler LoadSceneSuccess;
        public event LoadSceneFailedHandler LoadSceneFailed;

        private ILoadingScreen m_LoadingScreen = null;
        private DefaultLoadingScreen m_DefaultLoadingScreen = null;
        private bool m_IsLoading = false;

        protected override void OnInit()
        {
            base.OnInit();
            m_DefaultLoadingScreen = new DefaultLoadingScreen();
        }

        public void SetLoadingScreen(ILoadingScreen loadingScreen)
        {
            m_LoadingScreen = loadingScreen;
            Debug.Log($"[SceneManager] setting loading screen: {loadingScreen}");
        }

        public void LoadScene(string mainScene, object userData = null, params string[] addScenes)
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

            if (addScenes != null && addScenes.Length > 0)
            {
                Debug.Log($"[SceneManager] start loading main scene: {mainScene}, additive scenes: {string.Join(", ", addScenes)}");
            }
            else
            {
                Debug.Log($"[SceneManager] start loading main scene: {mainScene}");
            }

            m_IsLoading = true;
            StartCoroutine(LoadSceneInternal(mainScene, addScenes, userData));
        }

        private IEnumerator LoadSceneInternal(string mainScene, string[] addScenes, object userData)
        {
            ILoadingScreen sceneTransition = m_LoadingScreen ?? m_DefaultLoadingScreen;
            sceneTransition?.OnSceneLoadBegin(mainScene, addScenes, userData);
            Debug.Log("[SceneManager] asset preloading...");
            yield return sceneTransition?.OnScenePreload(mainScene, addScenes, userData);

            // start loadScene.
            var mainHandle = Addressables.LoadSceneAsync(mainScene, LoadSceneMode.Single, false, 100);
            yield return mainHandle;
            if (mainHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[SceneManager] failed to load main scene: {mainScene}");
                LoadSceneFailed?.Invoke(mainScene, userData);
                yield break;
            }
            // end loadScene.

            var mainSceneInstance = mainHandle.Result;
            yield return null;
            yield return mainSceneInstance.ActivateAsync();
            yield return null;
            foreach (string addScene in addScenes)
            {
                var addHandle = Addressables.LoadSceneAsync(addScene, LoadSceneMode.Additive);
                yield return addHandle;

                if (addHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[SceneManager] failed to load additive scene: {addScene}");
                    LoadSceneFailed?.Invoke(addScene, userData);
                }
            }
            
            sceneTransition?.OnSceneLoadEnd(mainScene, addScenes, userData);
            Debug.Log($"[SceneManager] all requested scenes loaded. main scene: {mainScene}");
            LoadSceneSuccess?.Invoke(mainSceneInstance.Scene, mainScene, userData);
            m_IsLoading = false;
        }
    }
}