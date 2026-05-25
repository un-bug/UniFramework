namespace UniFramework
{
    public static class SceneManager
    {
        internal static SceneModule m_SceneModuleInstance;
        internal static SceneModule m_SceneModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_SceneModuleInstance, "SceneManager");
            }
        }

        public static void SetSceneLoadingScreen(ISceneLoadingScreen loadingScreen)
        {
            m_SceneModule.SetSceneLoadingScreen(loadingScreen);
        }

        public static void LoadScene(string mainScene, object userData = null, params string[] addScenes)
        {
            m_SceneModule.LoadScene(mainScene, userData, addScenes);
        }
    }
}