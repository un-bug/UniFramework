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

        public ConfigTable<T> GetConfigTable<T>(string configTableAssetKey) where T : ConfigTableRow
        {
            if (string.IsNullOrEmpty(configTableAssetKey))
            {
                Debug.LogError($"Config table asset key is empty: {typeof(T).Name}");
                return null;
            }

            if (m_ConfigTables.TryGetValue(configTableAssetKey, out var asset))
            {
                return asset as ConfigTable<T>;
            }

            var configTable = m_AssetLoader.Load<ConfigTable<T>>(configTableAssetKey);
            if (configTable == null)
            {
                Debug.LogError($"Config table asset not found: {configTableAssetKey}");
                return null;
            }

            m_ConfigTables[configTableAssetKey] = configTable;
            return configTable;
        }
    }
}