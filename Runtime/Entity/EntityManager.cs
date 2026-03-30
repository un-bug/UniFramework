using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    [DisallowMultipleComponent]
    public sealed partial class EntityManager : MonoSingleton<EntityManager>
    {
        private Dictionary<string, EntityGroup> m_EntityGroups;
        private Queue<Entity> m_RecycleQueue;

        protected override void OnInit()
        {
            base.OnInit();
            m_EntityGroups = new Dictionary<string, EntityGroup>();
            m_RecycleQueue = new Queue<Entity>();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                entityGroup.Shutdown();
            }

            m_EntityGroups.Clear();
            m_RecycleQueue.Clear();
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            ProcessRecycleQueue();
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                entityGroup.OnUpdate(deltaTime);
            }
        }

        public EntityGroup GetEntityGroup(string entityGroupName)
        {
            if (m_EntityGroups.TryGetValue(entityGroupName, out var entityGroup))
            {
                return entityGroup;
            }

            return null;
        }

        public bool HasEntityGroup(string entityGroupName)
        {
            return m_EntityGroups.ContainsKey(entityGroupName);
        }

        public bool AddEntityGroup(string entityGroupName, IEntityGroupHelper entityGroupHelper)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                return false;
            }

            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            m_EntityGroups.Add(entityGroupName, new EntityGroup(entityGroupName, entityGroupHelper));
            return true;
        }

        public bool RemoveEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                return false;
            }

            if (!HasEntityGroup(entityGroupName))
            {
                return false;
            }

            EntityGroup entityGroup = GetEntityGroup(entityGroupName);
            Entity[] entities = entityGroup.GetAllEntities();
            foreach (var entity in entities)
            {
                HideEntity(entity, null);
            }

            entityGroup.Shutdown();
            m_EntityGroups.Remove(entityGroupName);
            return true;
        }

        public Entity ShowEntity(int entityId, Type entityLogicType, string entityAssetKey, string entityGroupName, object userData)
        {
            EntityGroup entityGroup = GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new Exception($"Can not spawn entity because entity group '{entityGroupName}' is invalid.");
            }

            return InternalShowEntity(entityId, entityLogicType, entityAssetKey, entityGroup, userData);
        }

        public void HideEntity(Entity entity, object userData)
        {
            InternalHideEntity(entity, userData);
        }

        private Entity InternalShowEntity(int entityId, Type entityLogicType, string entityAssetKey, EntityGroup entityGroup, object userData)
        {
            Entity entity = entityGroup.SpawnEntity(entityAssetKey);
            entity.OnInit(entityId, entityLogicType, entityAssetKey, entityGroup, userData);
            entity.OnShow(userData);
            return entity;
        }

        private void InternalHideEntity(Entity entity, object userData)
        {
            entity.OnHide(userData);
            EntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new Exception($"Can not despawn entity '{entity.Id}' because it is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            m_RecycleQueue.Enqueue(entity);
        }

        private void ProcessRecycleQueue()
        {
            while (m_RecycleQueue.Count > 0)
            {
                Entity entity = m_RecycleQueue.Dequeue();
                EntityGroup entityGroup = entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new Exception($"Can not recycle entity '{entity.Id}' because it is invalid.");
                }

                entity.OnRecycle();
                entityGroup.UnspawnEntity(entity);
            }
        }
    }
}