using System;
using UnityEngine;

namespace UniFramework.Runtime
{
    public abstract class EntityLogic : MonoBehaviour
    {
        public Entity Entity { get; private set; }

        protected internal virtual void OnInit()
        {
            Entity = GetComponent<Entity>();
        }

        protected internal virtual void OnDespawn()
        {
        }

        protected internal virtual void OnRecycle()
        {
        }

        protected internal virtual void OnSpawn()
        {
        }

        protected internal virtual void OnUpdate()
        {
        }
    }
}