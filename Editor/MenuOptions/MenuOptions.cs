using UnityEditor;
using UnityEngine;

namespace UniFramework.Editor
{
    public static class MenuOptions
    {
        [MenuItem("GameObject/UniFramework/UI Root", false)]
        public static void AddUIRoot(MenuCommand menuCommand)
        {
            var uiRoot = new GameObject("UIRoot").AddComponent<Runtime.UIRoot>();
            uiRoot.gameObject.layer = LayerMask.NameToLayer("UI");
            Canvas uiCanvas = new GameObject("UI Canvas").AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            uiRoot.UICanvas = uiCanvas;
            GameObjectUtility.SetParentAndAlign(uiCanvas.gameObject, uiRoot.gameObject);

            RectTransform uiPanelRoot = new GameObject("Hide Panel").AddComponent<RectTransform>();
            uiPanelRoot.anchorMin = Vector2.zero;
            uiPanelRoot.anchorMax = Vector2.one;
            uiPanelRoot.anchoredPosition = Vector2.zero;
            uiPanelRoot.sizeDelta = Vector2.zero;
            uiPanelRoot.gameObject.SetActive(false);
            GameObjectUtility.SetParentAndAlign(uiPanelRoot.gameObject, uiCanvas.gameObject);

            Undo.RegisterCreatedObjectUndo(uiRoot.gameObject, "Create UI Root");
            Selection.activeGameObject = uiRoot.gameObject;
        }
    }
}