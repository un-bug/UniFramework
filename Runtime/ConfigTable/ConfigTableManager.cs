using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    public class ConfigTableManager : GameModule
    {
        private Dictionary<string, ConfigTableBase> m_ConfigTables;
        private IAssetLoader m_AssetLoader;

        private void Awake()
        {
            m_ConfigTables = new Dictionary<string, ConfigTableBase>();
            m_AssetLoader = ResourceManager.CreateAssetLoader();
        }

        private void OnDestroy()
        {
            m_AssetLoader.Dispose();
            m_AssetLoader = null;
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

            var handle = m_AssetLoader.LoadAsset<ConfigTable<T>>(configTableAssetKey);
            var configTable = handle.Asset;
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