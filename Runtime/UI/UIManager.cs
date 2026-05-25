using UnityEngine;

namespace UniFramework
{
    public static class UIManager
    {
        internal static UIModule m_UIModuleInstance;
        internal static UIModule m_UIModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_UIModuleInstance, "UIManager");
            }
        }

        public static void SetUIRoot(IUIRoot uiRoot)
        {
            m_UIModule.SetUIRoot(uiRoot);
        }

        public static bool HasUIPanel(UIPanel uiPanel)
        {
            return m_UIModule.HasUIPanel(uiPanel);
        }

        public static bool TryGetUIPanel(string uiPanelAssetName, out UIPanel uiPanel)
        {
            return m_UIModule.TryGetUIPanel(uiPanelAssetName, out uiPanel);
        }

        public static UIPanel OpenUIPanel(string uiPanelAssetName)
        {
            return m_UIModule.OpenUIPanel(uiPanelAssetName);
        }

        public static UIPanel OpenUIPanel(string uiPanelAssetName, object userData)
        {
            return m_UIModule.OpenUIPanel(uiPanelAssetName, userData);
        }

        public static UIPanel OpenUIPanel(string uiPanelAssetName, string uiGroupName, object userData)
        {
            return m_UIModule.OpenUIPanel(uiPanelAssetName, uiGroupName, userData);
        }

        public static void CloseUIPanel(UIPanel uiPanel)
        {
            m_UIModule.CloseUIPanel(uiPanel);
        }

        public static void CloseAllUIPanels()
        {
            m_UIModule.CloseAllUIPanels();
        }

        public static void RefocusUIPanel(UIPanel uiPanel, object userData)
        {
            m_UIModule.RefocusUIPanel(uiPanel, userData);
        }

        public static UIGroup GetUIGroup(string uiGroupName)
        {
            return m_UIModule.GetUIGroup(uiGroupName);
        }

        public static void AddGroup(string groupName, int depth, Transform instanceRoot)
        {
            m_UIModule.AddGroup(groupName, depth, instanceRoot);
        }
    }
}