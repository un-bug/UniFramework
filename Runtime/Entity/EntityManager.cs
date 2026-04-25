using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    [DisallowMultipleComponent]
    public sealed partial class EntityManager : MonoSingleton<EntityManager>
    {
        private IAssetLoader m_AssetLoader;

        protected override void OnInit()
        {
            base.OnInit();
            m_AssetLoader = AssetLoaderFactory.Get();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            AssetLoaderFactory.Release(m_AssetLoader);
        }

        protected override void OnUpdate(float deltaTime)
        {
        }

        public Entity SpawnEntity(int entityId, string entityAssetKey)
        {
            GameObject entityPrefab = m_AssetLoader.Load<GameObject>(entityAssetKey);
            GameObject entityInstance = GameObject.Instantiate(entityPrefab);
            Entity entity = entityInstance.AddComponent<Entity>();
            entity.OnInit(entityId, entityAssetKey);
            return entity;
        }

        public void DespawnEntity(Entity entity)
        {
        }
    }
}