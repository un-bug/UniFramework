using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    public sealed class UIManager : GameModule
    {
        public event Action<UIPanel> OpenUIPanelSuccess;
        public event Action<UIPanel> CloseUIPanelComplete;

        private Dictionary<string, UIGroup> m_UIGroups;
        private Dictionary<UIPanel, UIGroup> m_UIPanelInfo;
        private IUIAssetProvider m_UIAssetProvider;

        private void Awake()
        {
            m_UIPanelInfo = new Dictionary<UIPanel, UIGroup>();
            m_UIGroups = new Dictionary<string, UIGroup>();
        }

        private void OnDestroy()
        {
            CloseAllUIPanels();
            foreach (UIGroup uiGroup in m_UIGroups.Values)
            {
                uiGroup.Dispose();
            }

            m_UIGroups.Clear();
            m_UIPanelInfo.Clear();
        }

        public void SetUIAssetProvider(IUIAssetProvider uiAssetProvider)
        {
            if (uiAssetProvider == null)
            {
                Debug.LogError($"[UIManager] ui asset provider is invalid.");
                return;
            }

            m_UIAssetProvider = uiAssetProvider;
        }

        public bool HasUIPanel(UIPanel uiPanel)
        {
            return uiPanel != null && m_UIPanelInfo.ContainsKey(uiPanel);
        }

        public bool TryGetUIPanel(string uiPanelAssetName, out UIPanel uiPanel)
        {
            foreach (UIPanel item in m_UIPanelInfo.Keys)
            {
                if (item != null && item.UIPanelAssetName == uiPanelAssetName)
                {
                    uiPanel = item;
                    return true;
                }
            }

            uiPanel = null;
            return false;
        }

        public UIPanel OpenUIPanel(string uiPanelAssetName, string uiGroupName)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, null);
        }

        public UIPanel OpenUIPanel(string uiPanelAssetName, string uiGroupName, object userData)
        {
            if (m_UIAssetProvider == null)
            {
                throw new InvalidOperationException($"[UIManager] ui root is invalid.");
            }

            UIGroup uiGroup = GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                Debug.LogError($"[UIManager] ui group '{uiGroupName}' is not exist.");
                return null;
            }

            if (TryGetUIPanel(uiPanelAssetName, out UIPanel uiPanel))
            {
                RefocusUIPanel(uiPanel, userData);
                return uiPanel;
            }

            uiPanel = m_UIAssetProvider.LoadUIPanel(uiPanelAssetName);
            if (uiPanel == null)
            {
                return null;
            }

            AttachUIPanelToGroup(uiPanel, uiGroup);
            InternalOpenUIPanel(uiPanelAssetName, uiPanel, uiGroup, userData);
            return uiPanel;
        }

        public void CloseUIPanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            if (!m_UIPanelInfo.TryGetValue(uiPanel, out UIGroup uiGroup))
            {
                Debug.LogError($"[UIManager] closing a uiPanel that is not managed by any group: {uiPanel.name}");
                return;
            }

            uiGroup.RemoveUIPanel(uiPanel);
            m_UIPanelInfo.Remove(uiPanel);
            uiPanel.OnClose();
            uiGroup.Refresh();
            CloseUIPanelComplete?.Invoke(uiPanel);
            uiPanel.OnRecycle();
        }

        public void CloseAllUIPanels()
        {
            var uiPanels = new List<UIPanel>(m_UIPanelInfo.Keys);
            foreach (var uiPanel in uiPanels)
            {
                CloseUIPanel(uiPanel);
            }
        }

        public void RefocusUIPanel(UIPanel uiPanel, object userData)
        {
            if (uiPanel == null)
            {
                return;
            }

            if (!m_UIPanelInfo.TryGetValue(uiPanel, out UIGroup uiGroup))
            {
                Debug.LogWarning($"[UIManager] refocus a uiPanel that is not managed by any group: {uiPanel.name}");
                return;
            }

            uiGroup.RefocusUIPanel(uiPanel);
            uiPanel.OnRefocus(userData);
            uiGroup.Refresh();
        }

        public UIGroup GetUIGroup(string uiGroupName)
        {
            if (m_UIGroups.TryGetValue(uiGroupName, out UIGroup uiGroup))
            {
                return uiGroup;
            }

            return null;
        }

        public void AddGroup(string groupName, int depth, Transform instanceRoot)
        {
            if (m_UIGroups.TryGetValue(groupName, out UIGroup uiGroup))
            {
                if (uiGroup.InstanceRoot == null)
                {
                    uiGroup.InstanceRoot = instanceRoot;
                }

                var removeList = new List<UIPanel>();
                foreach (var uiPanel in m_UIPanelInfo.Keys)
                {
                    if (uiPanel == null)
                    {
                        removeList.Add(uiPanel);
                    }
                }

                for (int i = removeList.Count - 1; i >= 0; i--)
                {
                    m_UIPanelInfo.Remove(removeList[i]);
                }

                return;
            }

            var group = new UIGroup(groupName, depth, instanceRoot);
            m_UIGroups.Add(groupName, group);
        }

        private void AttachUIPanelToGroup(UIPanel uiPanel, UIGroup uiGroup)
        {
            // 如果面板已经在其他组，先移除。
            if (m_UIPanelInfo.TryGetValue(uiPanel, out UIGroup oldGroup))
            {
                if (oldGroup != uiGroup)
                {
                    oldGroup.RemoveUIPanel(uiPanel);
                    m_UIPanelInfo.Remove(uiPanel);
                }
            }

            if (!m_UIPanelInfo.ContainsKey(uiPanel))
            {
                m_UIPanelInfo.Add(uiPanel, uiGroup);
            }
        }

        private void InternalOpenUIPanel(string uiPanelAssetName, UIPanel uiPanel, UIGroup uiGroup, object userData)
        {
            try
            {
                uiPanel.OnInit(uiPanelAssetName, userData);
                uiGroup.AddUIPanel(uiPanel);
                uiPanel.OnOpen(userData);
                uiGroup.Refresh();
                OpenUIPanelSuccess?.Invoke(uiPanel);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[UIManager] open ui error: {exception}");
                throw;
            }
        }
    }
}