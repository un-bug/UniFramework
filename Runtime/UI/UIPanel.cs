using UnityEngine;

namespace UniFramework
{
    public sealed class UIPanel : MonoBehaviour
    {
        [SerializeField] private bool m_Paused = true;
        [SerializeField] private bool m_Covered = true;
        [SerializeField] private bool m_PauseCoveredUI = true;
        [SerializeField] private string m_UIPanelAssetName;
        [SerializeField] private UIPanelLogic m_Logic;

        public string UIPanelAssetName => m_UIPanelAssetName;
        public UIPanelLogic Logic => m_Logic;
        public bool PauseCoveredUI => m_PauseCoveredUI;
        public bool Paused
        {
            get
            {
                return m_Paused;
            }
            internal set
            {
                m_Paused = value;
            }
        }
        public bool Covered
        {
            get
            {
                return m_Covered;
            }
            internal set
            {
                m_Covered = value;
            }
        }

        public void OnInit(string uiPanelAssetName, object userData)
        {
            m_UIPanelAssetName = uiPanelAssetName;
            m_Logic = GetComponent<UIPanelLogic>();
            if (m_Logic)
            {
                m_PauseCoveredUI = m_Logic.PauseCoveredUI;
                m_Logic.OnInit(userData);
            }
        }

        public void OnRelease()
        {
            if (m_Logic)
            {
                m_Logic.OnRelease();
            }
        }

        public void OnOpen(object userData)
        {
            if (m_Logic)
            {
                m_Logic.OnOpen(userData);
            }
        }

        public void OnClose()
        {
            if (m_Logic)
            {
                m_Logic.OnClose();
            }
        }

        public void OnResume()
        {
            if (m_Logic)
            {
                m_Logic.OnResume();
            }
        }

        public void OnPause()
        {
            if (m_Logic)
            {
                m_Logic.OnPause();
            }
        }

        public void OnReveal()
        {
            if (m_Logic)
            {
                m_Logic.OnReveal();
            }
        }

        public void OnCover()
        {
            if (m_Logic)
            {
                m_Logic.OnCover();
            }
        }

        public void OnRefocus(object userData)
        {
            if (m_Logic)
            {
                m_Logic.OnRefocus(userData);
            }
        }
    }
}