namespace UniFramework.Runtime
{
    public static class AssetLoaderFactory
    {
        public static IAssetLoader Create()
        {
#if ENABLE_ADDRESSABLES
            return new AddressableLoader();
#else
            return new ResourcesLoader();
#endif
        }
    }
}