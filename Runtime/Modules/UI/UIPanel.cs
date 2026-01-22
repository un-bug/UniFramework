using UnityEngine;

namespace UniFramework.Runtime
{
    public class UIPanel : MonoBehaviour
    {
        [SerializeField]
        private bool m_Visible = false;

        [SerializeField]
        private bool m_Paused = true;

        [SerializeField]
        private bool m_Covered = true;

        public virtual bool PauseCoveredUIPanel => true;

        public bool Visible
        {
            get
            {
                return m_Visible;
            }
            set
            {
                if (m_Visible == value)
                {
                    return;
                }

                m_Visible = value;
                gameObject.SetActive(m_Visible);
            }
        }

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

        public virtual void CloseSelf()
        {
            UIManager.Instance.CloseUIPanel(this);
        }

        public virtual void OnInit(object userData)
        {
        }

        public virtual void OnRelease()
        {
        }

        public virtual void OnOpen(object userData)
        {
            Visible = true;
        }

        public virtual void OnClose()
        {
            Visible = false;
        }

        public virtual void OnResume()
        {
            Visible = true;
        }

        public virtual void OnPause()
        {
            Visible = false;
        }

        public virtual void OnReveal()
        {
        }

        public virtual void OnCover()
        {
        }

        public virtual void OnRefocus(object userData)
        {
        }
    }
}