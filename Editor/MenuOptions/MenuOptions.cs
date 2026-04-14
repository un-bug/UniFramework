using UnityEditor;
using UnityEngine;

namespace UniFramework.Editor
{
    public static class MenuOptions
    {
        [MenuItem("GameObject/UniFramework/UI Root", false, 11)]
        public static void AddUIRoot(MenuCommand menuCommand)
        {
            var uiRoot = new GameObject("UIRoot").AddComponent<Runtime.UIRoot>();
            uiRoot.gameObject.layer = LayerMask.NameToLayer("UI");
            Canvas uiCanvas = new GameObject("UI Canvas").AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            uiRoot.UICanvas = uiCanvas;
            GameObjectUtility.SetParentAndAlign(uiCanvas.gameObject, uiRoot.gameObject);

            Undo.RegisterCreatedObjectUndo(uiRoot.gameObject, "Create UI Root");
            Selection.activeGameObject = uiRoot.gameObject;
        }
    }
}