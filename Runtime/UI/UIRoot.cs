using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public interface IUIRoot
    {
        UIPanel LoadUIPanel(string uiPanelAssetName);
    }

    [DefaultExecutionOrder(-10)]
    public class UIRoot : MonoBehaviour, IUIRoot
    {
        [SerializeField] private Canvas m_UICanvas;
        [SerializeField] private Transform m_InstanceRoot;
        [SerializeField] private UIGroupData[] m_UIGroups;

        private UIManager m_UIManager;
        private IAssetLoader m_AssetLoader;
        private Dictionary<string, UIPanel> m_CacheUIPanels;
        
        public Canvas UICanvas { get { return m_UICanvas; } set { m_UICanvas = value; } }
        public Transform InstanceRoot { get => m_InstanceRoot; set => m_InstanceRoot = value; }

        protected virtual void Awake()
        {
            m_UIManager = UIManager.Instance;
            m_AssetLoader = AssetLoaderFactory.Get();
            m_UIManager.SetUIRoot(this);
            if (m_UICanvas == null)
            {
                m_UICanvas = GetComponentInChildren<Canvas>();
            }

            AddUIGroupRoot("Default", 0);
            foreach (var uiGroup in m_UIGroups)
            {
                AddUIGroupRoot(uiGroup.Name, uiGroup.Depth);
            }

            m_CacheUIPanels = new Dictionary<string, UIPanel>();
        }

        protected virtual void OnDestroy()
        {
            AssetLoaderFactory.Release(m_AssetLoader);
            m_AssetLoader = null;
        }

        public UIPanel LoadUIPanel(string uiPanelAssetName)
        {
            if (m_CacheUIPanels.TryGetValue(uiPanelAssetName, out UIPanel uiPanel))
            {
                if (uiPanel != null)
                {
                    return uiPanel;
                }
            }

            var uiPanelAsset = m_AssetLoader.Load<GameObject>(uiPanelAssetName);
            if (uiPanelAsset == null)
            {
                Debug.LogError($"[UIRoot] ui panel asset '{uiPanelAssetName}' is not exist.");
                return null;
            }

            GameObject uiPanelInstanceObject = Instantiate(uiPanelAsset);
            if (!uiPanelInstanceObject.TryGetComponent(out uiPanel))
            {
                Debug.LogError($"[UIRoot] ui panel '{uiPanelAssetName}' is invalid.");
                Destroy(uiPanelInstanceObject);
                return null;
            }

            m_CacheUIPanels[uiPanelAssetName] = uiPanel;
            return uiPanel;
        }
        
        public void AddUIGroupRoot(string groupName, int depth)
        {
            if (!m_UICanvas)
            {
                Debug.LogError($"[UIRoot] UICanvas is not assigned.");
                return;
            }

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = m_UICanvas.transform;
            }

            var rootObject = new GameObject($"UI Group - {groupName}")
            {
                layer = LayerMask.NameToLayer("UI")
            };

            rootObject.transform.SetParent(m_InstanceRoot, false);
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