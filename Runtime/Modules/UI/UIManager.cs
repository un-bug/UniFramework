using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    [DisallowMultipleComponent]
    public sealed partial class UIManager : UniFrameworkModule<UIManager>
    {
        public event Action<UIPanel> OpenUIPanelSuccess;
        public event Action<UIPanel> CloseUIPanelComplete;

        private Dictionary<string, UIGroup> m_UIGroups;
        private Dictionary<UIPanel, UIGroup> m_UIPanelInfo;
        private IUIRoot m_UIRoot;

        protected override void OnModuleInitialize()
        {
            base.OnModuleInitialize();
            m_UIPanelInfo = new Dictionary<UIPanel, UIGroup>();
            m_UIGroups = new Dictionary<string, UIGroup>();
        }

        protected override void OnModuleShutdown()
        {
            CloseAllUIPanels();
            foreach (UIGroup uiGroup in m_UIGroups.Values)
            {
                uiGroup.Dispose();
            }

            m_UIGroups.Clear();
            m_UIPanelInfo.Clear();
            base.OnModuleShutdown();
        }

        public void SetUIRoot(IUIRoot uiRoot)
        {
            if (uiRoot == null)
            {
                Debug.LogError($"[{nameof(UIManager)}] ui root is invalid.");
                return;
            }

            m_UIRoot = uiRoot;
        }

        public bool HasUIPanel(UIPanel uiPanel)
        {
            return uiPanel != null && m_UIPanelInfo.ContainsKey(uiPanel);
        }

        public T OpenUIPanel<T>() where T : UIPanel
        {
            return OpenUIPanel<T>("Default", default);
        }

        public T OpenUIPanel<T>(object userData) where T : UIPanel
        {
            return OpenUIPanel<T>("Default", userData);
        }

        public T OpenUIPanel<T>(string uiGroupName, object userData) where T : UIPanel
        {
            Type type = typeof(T);
            UIGroup uiGroup = GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                Debug.LogError($"[{nameof(UIManager)}] ui group '{uiGroupName}' is not exist.");
                return null;
            }

            T uiPanel = GetCachedUIPanel<T>();
            if (uiPanel == null)
            {
                Debug.LogError($"[{nameof(UIManager)}] ui panel '{typeof(T)}' is not registered.");
                return null;
            }

            if (HasUIPanel(uiPanel))
            {
                RefocusUIPanel(uiPanel, userData);
                return uiPanel;
            }

            AttachPanelToGroup(uiPanel, uiGroup);
            InternalOpenUIPanel(uiPanel, uiGroup, userData);
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
                Debug.LogError($"[{nameof(UIManager)}] closing a panel that is not managed by any group: {uiPanel.name}");
                return;
            }

            uiGroup.RemoveUIPanel(uiPanel);
            m_UIPanelInfo.Remove(uiPanel);
            uiPanel.OnClose();
            uiGroup.Refresh();
            CloseUIPanelComplete?.Invoke(uiPanel);
            uiPanel.OnRelease();
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
                Debug.LogWarning($"[{nameof(UIManager)}] refocus a panel that is not managed by any group: {uiPanel.name}");
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

        public T GetCachedUIPanel<T>() where T : UIPanel
        {
            if (m_UIRoot == null)
            {
                throw new InvalidOperationException($"[{nameof(UIManager)}] ui root is invalid.");
            }

            return m_UIRoot.GetUIPanel<T>();
        }

        private void AttachPanelToGroup<T>(T uiPanel, UIGroup uiGroup) where T : UIPanel
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

        private void InternalOpenUIPanel(UIPanel uiPanel, UIGroup uiGroup, object userData)
        {
            try
            {
                uiPanel.OnInit(userData);
                uiGroup.AddUIPanel(uiPanel);
                uiPanel.OnOpen(userData);
                uiGroup.Refresh();
                OpenUIPanelSuccess?.Invoke(uiPanel);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(UIManager)}] open ui error: {exception}");
                throw;
            }
        }
    }
}