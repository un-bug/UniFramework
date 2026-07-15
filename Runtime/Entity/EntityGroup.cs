using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UniFramework
{
    public interface IEntityGroupHelper
    {
    }

    public class EntityGroup
    {
        private readonly Dictionary<string, ObjectPool<Entity>> m_EntityPools;
        private readonly Dictionary<Entity, Action> m_ReleaseAssetHandles;
        private LinkedListNode<Entity> m_CachedNode;
        private IAssetLoader m_AssetLoader;
        public string Name { get; private set; }
        public IEntityGroupHelper Helper { get; private set; }
        public LinkedList<Entity> Entities { get; private set; }

        public EntityGroup(string name, IEntityGroupHelper entityGroupHelper)
        {
            Name = name;
            Helper = entityGroupHelper;
            Entities = new LinkedList<Entity>();
            m_EntityPools = new Dictionary<string, ObjectPool<Entity>>();
            m_ReleaseAssetHandles = new Dictionary<Entity, Action>();
            m_AssetLoader = ResourceManager.CreateAssetLoader();
        }

        public void Shutdown()
        {
            Helper = null;
            Entities.Clear();
            foreach (ObjectPool<Entity> entityPool in m_EntityPools.Values)
            {
                entityPool.Clear();
            }

            m_EntityPools.Clear();
            m_ReleaseAssetHandles.Clear();
            m_AssetLoader.Dispose();
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
            if (m_CachedNode != null && m_CachedNode.Value == entity)
            {
                m_CachedNode = m_CachedNode.Next;
            }

            if (!Entities.Remove(entity))
            {
                Debug.LogError($"EntityGroup remove entity failure, entity id is {entity.Id}.");
            }
        }

        public void OnUpdate(float deltaTime)
        {
            LinkedListNode<Entity> current = Entities.First;
            while (current != null)
            {
                m_CachedNode = current.Next;
                current.Value.OnUpdate(deltaTime);
                current = m_CachedNode;
                m_CachedNode = null;
            }
        }

        public Entity SpawnEntity(string entityAssetKey)
        {
            if (Helper == null)
            {
                throw new Exception("EntityGroupHelper is invalid.");
            }

            var pool = GetOrCreatePool(entityAssetKey);
            Entity entity = pool.Get();
            AddEntity(entity);
            return entity;
        }

        public void UnspawnEntity(Entity entity)
        {
            if (!m_EntityPools.TryGetValue(entity.EntityAssetKey, out ObjectPool<Entity> pool))
            {
                Debug.LogError($"Entity pool '{entity.EntityAssetKey}' does not exist.");
                return;
            }

            pool.Release(entity);
        }

        private ObjectPool<Entity> GetOrCreatePool(string entityAssetKey)
        {
            if (m_EntityPools.TryGetValue(entityAssetKey, out var pool))
            {
                return pool;
            }

            pool = new ObjectPool<Entity>(CreateEntity, OnGetEntity, OnReleaseEntity, OnDestroyEntity, true, 8, 64);
            m_EntityPools.Add(entityAssetKey, pool);
            return pool;

            // Create a new entity instance from the asset and return it.
            Entity CreateEntity()
            {
                var handle = m_AssetLoader.LoadAsset<GameObject>(entityAssetKey);
                GameObject instance = GameObject.Instantiate(handle.Asset, ((MonoBehaviour)Helper).transform);
                if (instance.TryGetComponent<Entity>(out Entity entity) == false)
                {
                    entity = instance.AddComponent<Entity>();
                }

                instance.SetActive(false);
                m_ReleaseAssetHandles.Add(entity, () => m_AssetLoader?.UnloadAsset(handle));
                return entity;
            }

            void OnGetEntity(Entity entity)
            {
                entity.gameObject.SetActive(true);
            }

            void OnReleaseEntity(Entity entity)
            {
                entity.gameObject.SetActive(false);
            }

            void OnDestroyEntity(Entity entity)
            {
                if (m_ReleaseAssetHandles.Remove(entity, out Action releaseHandle))
                {
                    releaseHandle.Invoke();
                }

                GameObject.Destroy(entity.gameObject);
            }
        }
    }
}