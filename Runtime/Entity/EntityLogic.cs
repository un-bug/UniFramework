using UnityEngine;

namespace UniFramework.Runtime
{
    public abstract class EntityLogic : MonoBehaviour
    {
        private bool m_Visible = false;
        public Entity Entity { get; private set; }
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
                InternalSetVisible(value);
            }
        }

        protected internal virtual void OnInit()
        {
            Entity = GetComponent<Entity>();
        }

        protected internal virtual void OnRecycle()
        {
        }

        protected internal virtual void OnShow()
        {
            Visible = true;
        }

        protected internal virtual void OnHide()
        {
            Visible = false;
        }

        protected internal virtual void OnUpdate()
        {
        }

        protected virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}