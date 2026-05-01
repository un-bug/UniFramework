using System.Collections;

namespace UniFramework
{
    public interface ISceneLoadingScreen
    {
        void OnSceneLoadBegin(string mainScene, string[] addScenes, object userData);
        IEnumerator OnScenePreload(string mainScene, string[] addScenes, object userData);
        void OnSceneLoadEnd(string mainScene, string[] addScenes, object userData);
    }
}