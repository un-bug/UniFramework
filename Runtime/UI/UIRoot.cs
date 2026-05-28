using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    [Serializable]
    public sealed class UIGroupData
    {
        [SerializeField] private string m_Name = null;
        [SerializeField] private int m_Depth = 0;
        public string Name => m_Name;
        public int Depth => m_Depth;
        public UIGroupData(string name, int depth)
        {
            m_Name = name;
            m_Depth = depth;
        }
    }
    
    [DefaultExecutionOrder(-10)]
    public class UIRoot : MonoBehaviour, IUIAssetProvider
    {
        [SerializeField] private Canvas m_UICanvas;
        [SerializeField] private Transform m_InstanceRoot;
        [SerializeField] private UIGroupData[] m_UIGroups;

        private IAssetLoader m_AssetLoader;
        private Dictionary<string, UIPanel> m_CacheUIPanels;
        
        public Canvas UICanvas { get { return m_UICanvas; } set { m_UICanvas = value; } }
        public Transform InstanceRoot { get => m_InstanceRoot; set => m_InstanceRoot = value; }

        protected virtual void Awake()
        {
            m_AssetLoader = AssetServices.CreateLoader();
            UIManager.SetUIAssetProvider(this);
            if (m_UICanvas == null)
            {
                m_UICanvas = GetComponentInChildren<Canvas>();
            }

            foreach (var uiGroup in m_UIGroups)
            {
                AddUIGroupRoot(uiGroup.Name, uiGroup.Depth);
            }

            m_CacheUIPanels = new Dictionary<string, UIPanel>();
        }

        protected virtual void OnDestroy()
        {
            AssetServices.Release(m_AssetLoader);
            m_AssetLoader = null;
        }

        private void Reset()
        {
            m_UIGroups = new[]
            {
                new UIGroupData("Default", 0)
            };
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

            GameObject instanceObject = Instantiate(uiPanelAsset);
            if (!instanceObject.TryGetComponent(out uiPanel))
            {
                uiPanel = instanceObject.AddComponent<UIPanel>();
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
            UIManager.AddGroup(groupName, depth, rootObject.transform);
        }
    }
}