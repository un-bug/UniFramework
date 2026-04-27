using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public class ConfigTableManager : MonoSingleton<ConfigTableManager>
    {
        private Dictionary<string, ConfigTableBase> m_ConfigTables;
        private IAssetLoader m_AssetLoader;

        protected override void OnInit()
        {
            m_ConfigTables = new Dictionary<string, ConfigTableBase>();
            m_AssetLoader = AssetLoaderFactory.Get();
        }

        protected override void OnDispose()
        {
            AssetLoaderFactory.Release(m_AssetLoader);
        }

        public ConfigTable<T> GetConfigTable<T>(string assetKey) where T : ConfigTableRow
        {
            if (string.IsNullOrEmpty(assetKey))
            {
                Debug.LogError($"Config table asset key is empty: {typeof(T).Name}");
                return null;
            }

            if (m_ConfigTables.TryGetValue(assetKey, out var asset))
            {
                return asset as ConfigTable<T>;
            }

            var configTable = m_AssetLoader.Load<ConfigTable<T>>(assetKey);
            if (configTable == null)
            {
                Debug.LogError($"Config table asset not found: {assetKey}");
                return null;
            }

            m_ConfigTables[assetKey] = configTable;
            return configTable;
        }

        public static ConfigTableAttribute GetConfigTableAttribute<T>() where T : ConfigTableRow
        {
            object[] attributes = typeof(T).GetCustomAttributes(typeof(ConfigTableAttribute), false);
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is ConfigTableAttribute attribute)
                {
                    return attribute;
                }
            }

            return null;
        }
    }
}