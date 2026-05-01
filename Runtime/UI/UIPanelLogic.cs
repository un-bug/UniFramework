using UnityEngine;

namespace UniFramework
{
    public abstract class UIPanelLogic : MonoBehaviour
    {
        private bool m_Visible = false;

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
                InternalSetVisible(m_Visible);
            }
        }

        public UIPanel UIPanel { get; private set; }
        public RectTransform RectTransform { get; private set; }

        protected internal virtual bool PauseCoveredUI => true;

        protected internal virtual void OnInit(object userData)
        {
            if (RectTransform == null)
            {
                RectTransform = (RectTransform)transform;
            }

            UIPanel = GetComponent<UIPanel>();
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

        protected internal virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}