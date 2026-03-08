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

        protected internal virtual bool PauseCoveredUIPanel => true;

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

        protected internal bool Paused
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

        protected internal bool Covered
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

        protected internal virtual void OnInit(object userData)
        {
        }

        protected internal virtual void OnRelease()
        {
        }

        protected internal virtual void OnOpen(object userData)
        {
            Visible = true;
        }

        protected internal virtual void OnClose()
        {
            Visible = false;
        }

        protected internal virtual void OnResume()
        {
            Visible = true;
        }

        protected internal virtual void OnPause()
        {
            Visible = false;
        }

        protected internal virtual void OnReveal()
        {
        }

        protected internal virtual void OnCover()
        {
        }

        protected internal virtual void OnRefocus(object userData)
        {
        }
    }
}