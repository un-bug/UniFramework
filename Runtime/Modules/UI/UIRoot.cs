using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public class UIRoot : MonoBehaviour, IUIRoot
    {
        [SerializeField]
        private Canvas m_UICanvas;

        [SerializeField]
        private UIGroupData[] m_UIGroups = new UIGroupData[] { new UIGroupData("Default", 0) };

        private readonly Dictionary<Type, UIPanel> m_CachedUIPanels = new Dictionary<Type, UIPanel>();
        private UIManager m_UIManager;

        public Canvas UICanvas
        {
            set
            {
                m_UICanvas = value;
            }
        }

        private void Awake()
        {
            m_UIManager = UIManager.Instance;
            if (m_UIManager == null)
            {
                return;
            }

            m_UIManager.SetUIRoot(this);
            if (m_UICanvas == null)
            {
                m_UICanvas = GetComponentInChildren<Canvas>();
            }

            foreach (var uiGroup in m_UIGroups)
            {
                AddUIGroupRoot(uiGroup.Name, uiGroup.Depth);
            }

            foreach (var uiPanel in GetComponentsInChildren<UIPanel>(true))
            {
                Register(uiPanel);
            }
        }

        private void OnDestroy()
        {
            if (m_UIManager == null)
            {
                return;
            }

            var uiPanels = new List<UIPanel>(m_CachedUIPanels.Values);
            foreach (var panel in uiPanels)
            {
                if (panel != null)
                {
                    Unregister(panel);
                }
            }
        }

        public T GetUIPanel<T>() where T : UIPanel
        {
            return m_CachedUIPanels.TryGetValue(typeof(T), out var panel) ? panel as T : null;
        }

        public void AddUIPanel(UIPanel uiPanel) => Register(uiPanel);

        public void RemoveUIPanel(UIPanel uiPanel) => Unregister(uiPanel);

        private void Register(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            uiPanel.gameObject.SetActive(false);
            uiPanel.Visible = false;

            Type panelType = uiPanel.GetType();

            if (m_CachedUIPanels.TryGetValue(panelType, out var existingPanel) && existingPanel != null)
            {
                Debug.LogWarning($"[{nameof(UIManager)}] Duplicate panel already registered: {panelType.Name}.");
                return;
            }

            m_CachedUIPanels[panelType] = uiPanel;
        }

        private void Unregister(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            uiPanel.Visible = false;
            m_CachedUIPanels.Remove(uiPanel.GetType());
        }

        public void AddUIGroupRoot(string groupName, int depth)
        {
            if (!m_UICanvas)
            {
                return;
            }

            var rootObject = new GameObject($"UI Group - {groupName}")
            {
                layer = LayerMask.NameToLayer("UI")
            };

            rootObject.transform.SetParent(m_UICanvas.transform, false);
            RectTransform rectTransform = rootObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.SetSiblingIndex(depth);
            m_UIManager.AddGroup(groupName, depth, rootObject.transform);
        }
    }
}