using System.Collections;

namespace UniFramework.Runtime
{
    internal class DefaultSceneLoadingScreen : ISceneLoadingScreen
    {
        public void OnSceneLoadBegin(string mainScene, string[] addScenes, object userData)
        {
        }

        public void OnSceneLoadEnd(string mainScene, string[] addScenes, object userData)
        {
        }

        public IEnumerator OnScenePreload(string mainScene, string[] addScenes, object userData)
        {
            yield return null;
        }
    }
}