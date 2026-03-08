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

        [SerializeField]
        private List<UIPanel> m_UIPanels = new List<UIPanel>();

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

            m_UIPanels.Clear();
            m_UIPanels.AddRange(GetComponentsInChildren<UIPanel>(true));
            foreach (var uiPanel in m_UIPanels)
            {
                RegisterUIPanel(uiPanel);
            }
        }

        private void OnDestroy()
        {
            if (m_UIManager == null)
            {
                return;
            }

            for (int i = m_UIPanels.Count - 1; i >= 0; i--)
            {
                UIPanel panel = m_UIPanels[i];
                if (panel != null)
                {
                    UnregisterUIPanel(panel);
                }
            }
        }

        private void OnValidate()
        {
            m_UIPanels = new List<UIPanel>(GetComponentsInChildren<UIPanel>(true));
        }

        public T LoadUIPanel<T>() where T : UIPanel
        {
            for (int i = 0; i < m_UIPanels.Count; i++)
            {
                if (m_UIPanels[i] is T panel)
                {
                    return panel;
                }
            }

            return null;
        }

        public void RegisterUIPanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            uiPanel.gameObject.SetActive(false);
            uiPanel.Visible = false;
            Type panelType = uiPanel.GetType();
            for (int i = 0; i < m_UIPanels.Count; i++)
            {
                if (m_UIPanels[i] != null && m_UIPanels[i].GetType() == panelType)
                {
                    Debug.LogWarning($"[{nameof(UIManager)}] Duplicate panel already registered: {panelType.Name}.");
                    return;
                }
            }

            m_UIPanels.Add(uiPanel);
        }

        public void UnregisterUIPanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            uiPanel.Visible = false;
            m_UIPanels.Remove(uiPanel);
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