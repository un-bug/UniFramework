using System;
using UnityEngine;

namespace UniFramework.Runtime
{
    public sealed class Entity : MonoBehaviour
    {
        public int Id { get; private set; }
        public string EntityAssetKey { get; private set; }
        public EntityGroup EntityGroup { get; private set; }
        public EntityLogic EntityLogic { get; private set; }

        public void OnInit(int entityId, Type entityLogicType, string entityAssetKey, EntityGroup entityGroup, object userData)
        {
            Id = entityId;
            EntityAssetKey = entityAssetKey;
            EntityGroup = entityGroup;
            if (EntityLogic != null)
            {
                if (EntityLogic.GetType() == entityLogicType)
                {
                    EntityLogic.enabled = true;
                    return;
                }

                Destroy(EntityLogic);
                EntityLogic = null;
            }

            EntityLogic = gameObject.AddComponent(entityLogicType) as EntityLogic;
            if (EntityLogic == null)
            {
                Debug.LogError($"Entity '{Id}' can not add entity logic.");
                return;
            }

            EntityLogic.OnInit(userData);
        }

        public void OnRecycle()
        {
            EntityLogic.OnRecycle();
            if (EntityLogic != null)
            {
                EntityLogic.enabled = false;
            }

            Id = 0;
        }

        public void OnShow(object userData)
        {
            EntityLogic.OnShow(userData);
        }

        public void OnHide(object userData)
        {
            EntityLogic.OnHide(userData);
        }

        public void OnUpdate(float deltaTime)
        {
            EntityLogic.OnUpdate(deltaTime);
        }
    }
}