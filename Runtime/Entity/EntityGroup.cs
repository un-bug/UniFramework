using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public interface IEntityGroupHelper
    {
        public Entity SpawnEntityInstance(string entityAssetKey);
    }

    public class EntityGroup
    {
        private IAssetLoader m_AssetLoader;
        public string Name { get; private set; }
        public IEntityGroupHelper Helper { get; private set; }
        public LinkedList<Entity> Entities { get; private set; }

        public EntityGroup(string name, IEntityGroupHelper entityGroupHelper)
        {
            Name = name;
            Helper = entityGroupHelper;
            Entities = new LinkedList<Entity>();
            m_AssetLoader = AssetLoaderFactory.Get();
        }

        public void Shutdown()
        {
            Helper = null;
            Entities.Clear();
            m_AssetLoader.Dispose();
            m_AssetLoader = null;
        }

        public Entity[] GetAllEntities()
        {
            List<Entity> results = new List<Entity>();
            foreach (Entity entity in Entities)
            {
                results.Add(entity);
            }

            return results.ToArray();
        }

        public void AddEntity(Entity entity)
        {
            Entities.AddLast(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (!Entities.Remove(entity))
            {
                throw new Exception($"EntityGroup remove entity failure, entity id is {entity.Id}.");
            }
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in Entities)
            {
                entity.OnUpdate();
            }
        }

        public Entity SpawnEntity(string entityAssetKey)
        {
            if (Helper == null)
            {
                throw new Exception("EntityGroupHelper is invalid.");
            }

            GameObject entityPrefab = m_AssetLoader.Load<GameObject>(entityAssetKey);
            GameObject entityInstance = GameObject.Instantiate(entityPrefab, ((MonoBehaviour)Helper).transform);
            Entity entity = entityInstance.AddComponent<Entity>();
            AddEntity(entity);
            return entity;
        }

        public void UnspawnEntity(Entity entity)
        {
            if (m_AssetLoader != null)
            {
                string assetKey = entity.EntityAssetKey;
                m_AssetLoader.Release(assetKey);
            }
        }
    }
}