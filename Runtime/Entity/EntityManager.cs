using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    public sealed partial class EntityManager : GameModule
    {
        private Dictionary<string, EntityGroup> m_EntityGroups;
        private Queue<Entity> m_RecycleQueue;

        private void Awake()
        {
            m_EntityGroups = new Dictionary<string, EntityGroup>();
            m_RecycleQueue = new Queue<Entity>();
        }

        private void OnDestroy()
        {
            ProcessRecycleQueue();
            var entityGroupNames = new List<string>(m_EntityGroups.Keys);
            foreach (string entityGroupName in entityGroupNames)
            {
                RemoveEntityGroup(entityGroupName);
            }

            m_EntityGroups.Clear();
            m_RecycleQueue.Clear();
        }

        private void Update()
        {
            ProcessRecycleQueue();
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                entityGroup.OnUpdate(Time.deltaTime);
            }
        }

        public bool HasEntity(int entityId)
        {
            return TryGetEntity(entityId, out _);
        }

        public Entity GetEntity(int entityId)
        {
            TryGetEntity(entityId, out Entity entity);
            return entity;
        }

        public bool TryGetEntity(int entityId, out Entity entity)
        {
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                foreach (Entity current in entityGroup.Entities)
                {
                    if (current.Id == entityId)
                    {
                        entity = current;
                        return true;
                    }
                }
            }

            entity = null;
            return false;
        }

        public Entity[] GetAllEntities()
        {
            List<Entity> entities = new List<Entity>();
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                entities.AddRange(entityGroup.Entities);
            }

            return entities.ToArray();
        }

        public void GetAllEntities(List<Entity> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                results.AddRange(entityGroup.Entities);
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

            EntityGroup entityGroup = GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                return false;
            }

            foreach (Entity entity in entityGroup.GetAllEntities())
            {
                InternalHideEntity(entity, true, null);
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

            if (!typeof(EntityLogic).IsAssignableFrom(entityLogicType))
            {
                throw new ArgumentException($"Type '{entityLogicType.FullName}' must inherit EntityLogic.", nameof(entityLogicType));
            }

            if (entityLogicType.IsAbstract)
            {
                throw new ArgumentException($"Type '{entityLogicType.FullName}' can not be abstract.", nameof(entityLogicType));
            }

            return InternalShowEntity(entityId, entityLogicType, entityAssetKey, entityGroup, userData);
        }

        public void HideEntity(Entity entity)
        {
            HideEntity(entity, null);
        }

        public void HideEntity(Entity entity, object userData)
        {
            InternalHideEntity(entity, false, userData);
        }

        public void HideAllEntities()
        {
            HideAllEntities(null);
        }

        public void HideAllEntities(object userData)
        {
            foreach (EntityGroup entityGroup in m_EntityGroups.Values)
            {
                foreach (Entity entity in entityGroup.GetAllEntities())
                {
                    InternalHideEntity(entity, false, userData);
                }
            }
        }

        private Entity InternalShowEntity(int entityId, Type entityLogicType, string entityAssetKey, EntityGroup entityGroup, object userData)
        {
            Entity entity = entityGroup.SpawnEntity(entityAssetKey);
            entity.OnInit(entityId, entityLogicType, entityAssetKey, entityGroup, userData);
            entity.OnShow(userData);
            return entity;
        }

        private void InternalHideEntity(Entity entity, bool recycleImmediately, object userData)
        {
            EntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new Exception($"Can not despawn entity '{entity.Id}' because it is invalid.");
            }

            entity.OnHide(userData);
            entityGroup.RemoveEntity(entity);
            if (recycleImmediately)
            {
                RecycleEntity(entity, entityGroup);
            }
            else
            {
                m_RecycleQueue.Enqueue(entity);
            }
        }

        private void ProcessRecycleQueue()
        {
            while (m_RecycleQueue.Count > 0)
            {
                Entity entity = m_RecycleQueue.Dequeue();
                EntityGroup entityGroup = entity.EntityGroup;
                if (entityGroup == null)
                {
                    Debug.LogError($"Can not recycle entity '{entity.Id}' because it is invalid.");
                    continue;
                }

                RecycleEntity(entity, entityGroup);
            }
        }

        private static void RecycleEntity(Entity entity, EntityGroup entityGroup)
        {
            entity.OnRecycle();
            entityGroup.UnspawnEntity(entity);
        }
    }
}