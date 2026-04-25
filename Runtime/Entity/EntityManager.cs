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
            m_EntityGroups.Clear();
            m_RecycleQueue.Clear();
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            while (m_RecycleQueue.Count > 0)
            {
                Entity entity = m_RecycleQueue.Dequeue();
                EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new Exception($"Can not recycle entity '{entity.Id}' because it is invalid.");
                }

                entity.OnRecycle();
                entityGroup.UnspawnEntity(entity);
            }

            foreach (var entityGroup in m_EntityGroups.Values)
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
                DespawnEntity(entity);
            }

            entityGroup.Shutdown();
            m_EntityGroups.Remove(entityGroupName);
            return true;
        }

        public Entity SpawnEntity(int entityId, Type entityLogicType, string entityAssetKey, string entityGroupName)
        {
            EntityGroup entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new Exception($"Can not spawn entity because entity group '{entityGroupName}' is invalid.");
            }

            return InternalSpawnEntity(entityId, entityLogicType, entityAssetKey, entityGroup);
        }

        public void DespawnEntity(Entity entity)
        {
            InternalDespawnEntity(entity);
        }

        private Entity InternalSpawnEntity(int entityId, Type entityLogicType, string entityAssetKey, EntityGroup entityGroup)
        {
            Entity entity = entityGroup.SpawnEntity(entityAssetKey);
            entity.OnInit(entityId, entityLogicType, entityAssetKey, entityGroup);
            entity.OnSpawn();
            return entity;
        }

        private void InternalDespawnEntity(Entity entity)
        {
            entity.OnDespawn();
            EntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new Exception($"Can not despawn entity '{entity.Id}' because it is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            m_RecycleQueue.Enqueue(entity);
        }
    }
}