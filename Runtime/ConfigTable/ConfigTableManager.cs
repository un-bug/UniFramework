namespace UniFramework
{
    public static class ConfigTableManager
    {
        internal static ConfigTableModule m_ConfigTableModuleInstance;

        internal static ConfigTableModule m_ConfigTableModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_ConfigTableModuleInstance, "ConfigTableManager");
            }
        }

        public static ConfigTable<T> GetConfigTable<T>(string configTableAssetKey) where T : ConfigTableRow
        {
            return m_ConfigTableModule.GetConfigTable<T>(configTableAssetKey);
        }
    }
}