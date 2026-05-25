using System;

namespace UniFramework
{
    public static class EntityManager
    {
        internal static EntityModule m_EntityModuleInstance;

        internal static EntityModule m_EntityModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_EntityModuleInstance, "EntityManager");
            }
        }

        public static EntityGroup GetEntityGroup(string entityGroupName)
        {
            return m_EntityModule.GetEntityGroup(entityGroupName);
        }

        public static bool HasEntityGroup(string entityGroupName)
        {
            return m_EntityModule.HasEntityGroup(entityGroupName);
        }

        public static bool AddEntityGroup(string entityGroupName, IEntityGroupHelper entityGroupHelper)
        {
            return m_EntityModule.AddEntityGroup(entityGroupName, entityGroupHelper);
        }

        public static bool RemoveEntityGroup(string entityGroupName)
        {
            return m_EntityModule.RemoveEntityGroup(entityGroupName);
        }

        public static Entity ShowEntity(int entityId, Type entityLogicType, string entityAssetKey, string entityGroupName, object userData)
        {
            return m_EntityModule.ShowEntity(entityId, entityLogicType, entityAssetKey, entityGroupName, userData);
        }

        public static void HideEntity(Entity entity, object userData)
        {
            m_EntityModule.HideEntity(entity, userData);
        }
    }
}