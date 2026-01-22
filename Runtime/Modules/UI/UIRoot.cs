using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField]
        private Canvas m_UICanvas;

        [SerializeField]
        private UIGroupData[] m_UIGroups = new UIGroupData[] { new UIGroupData("Default", 0) };

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
            m_UIManager = GameEntry.UI;
            if (m_UIManager == null)
            {
                return;
            }

            if (!m_UICanvas)
            {
                m_UICanvas = GetComponentInChildren<Canvas>();
            }

            foreach (var uiGroup in m_UIGroups)
            {
                AddUIGroupRoot(uiGroup.Name, uiGroup.Depth);
            }

            m_UIPanels.AddRange(GetComponentsInChildren<UIPanel>(true));
            foreach (var uiPanel in m_UIPanels)
            {
                if (uiPanel == null)
                {
                    continue;
                }

                Register(uiPanel);
            }
        }

        private void OnDestroy()
        {
            if (m_UIManager == null)
            {
                return;
            }

            foreach (var panel in m_UIPanels)
            {
                if (panel != null)
                {
                    Unregister(panel);
                }
            }
        }

        public void AddPanel(UIPanel uiPanel)
        {
            if (!m_UIPanels.Contains(uiPanel))
            {
                m_UIPanels.Add(uiPanel);
            }

            Register(uiPanel);
        }

        public void RemovePanel(UIPanel uiPanel)
        {
            if (m_UIPanels.Contains(uiPanel))
            {
                m_UIPanels.Remove(uiPanel);
            }

            Unregister(uiPanel);
        }

        public void Register(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            uiPanel.gameObject.SetActive(false);
            m_UIManager.RegisterUIPanel(uiPanel);
        }

        public void Unregister(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            m_UIManager.UnregisterUIPanel(uiPanel);
        }

        public void AddUIGroupRoot(string groupName, int depth)
        {
            if (!m_UICanvas)
            {
                return;
            }

            var rootObject = new GameObject($"UI Group - {groupName}");
            rootObject.gameObject.layer = LayerMask.NameToLayer("UI");
            rootObject.transform.SetParent(m_UICanvas.transform, false);
            RectTransform rectTransform = rootObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.SetSiblingIndex(depth);
            m_UIManager.AddGroup(groupName, depth, rootObject.transform);
        }

        //public static void StretchFull(RectTransform rectTransform)
        //{
        //    rectTransform.anchorMin = Vector2.zero;
        //    rectTransform.anchorMax = Vector2.one;
        //    rectTransform.anchoredPosition = Vector2.zero;
        //    rectTransform.sizeDelta = Vector2.zero;
        //    rectTransform.pivot = new Vector2(0.5f, 0.5f);
        //}
    }
}