using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    public class UIGroup : IDisposable
    {
        public string Name;
        public int Depth;
        public Transform InstanceRoot;

        private readonly List<UIPanel> m_UIPanels;

        public UIGroup(string name, int depth, Transform instanceRoot)
        {
            Name = name;
            Depth = depth;
            InstanceRoot = instanceRoot;
            m_UIPanels = new List<UIPanel>();
        }

        public void Dispose()
        {
            m_UIPanels.Clear();
        }

        public void Update(float deltaTime)
        {
            for (int i = m_UIPanels.Count - 1; i >= 0; i--)
            {
                m_UIPanels[i].OnUpdate(deltaTime);
            }
        }

        public void AddUIPanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            if (m_UIPanels.Contains(uiPanel))
            {
                Debug.LogWarning($"[UIManager] UIGroup '{Name}' uiPanel already exists: {uiPanel.name}");
                return;
            }

            m_UIPanels.Add(uiPanel);
            uiPanel.transform.SetParent(InstanceRoot, false);
            uiPanel.transform.SetAsLastSibling();
        }

        public void RemoveUIPanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            if (!uiPanel.Covered)
            {
                uiPanel.Covered = true;
                uiPanel.OnCover();
            }

            if (!uiPanel.Paused)
            {
                uiPanel.Paused = true;
                uiPanel.OnPause();
            }

            m_UIPanels.Remove(uiPanel);
        }

        public void RefocusUIPanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            if (m_UIPanels.Contains(uiPanel))
            {
                m_UIPanels.Remove(uiPanel);
                m_UIPanels.Add(uiPanel);
            }
            else
            {
                m_UIPanels.Add(uiPanel);
            }

            uiPanel.transform.SetAsLastSibling();
        }

        public void Refresh()
        {
            if (m_UIPanels.Count <= 0)
            {
                return;
            }

            bool isPause = false;
            bool isCovered = false;
            for (int index = m_UIPanels.Count - 1; index >= 0; index--)
            {
                UIPanel current = m_UIPanels[index];
                if (current == null)
                {
                    m_UIPanels.RemoveAt(index);
                    continue;
                }

                if (isPause)
                {
                    if (!current.Paused)
                    {
                        current.Paused = true;
                        current.OnPause();
                    }
                }
                else
                {
                    if (current.Paused)
                    {
                        current.Paused = false;
                        current.OnResume();
                    }
                }

                if (current.PauseCoveredUI)
                {
                    isPause = true;
                }

                if (isCovered)
                {
                    if (!current.Covered)
                    {
                        current.Covered = true;
                        current.OnCover();
                    }
                }
                else
                {
                    if (current.Covered)
                    {
                        current.Covered = false;
                        current.OnReveal();
                    }

                    isCovered = true;
                }
            }
        }
    }
}