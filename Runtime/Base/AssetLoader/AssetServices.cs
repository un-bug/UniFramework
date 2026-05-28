using System;

namespace UniFramework
{
    public static class AssetServices
    {
        private static IAssetProvider s_Provider = new AddressableAssetProvider();
        public static IAssetProvider Provider => s_Provider;
        
        public static IAssetLoader CreateLoader()
        {
            return new AssetLoader(s_Provider);
        }

        [Obsolete("Use IAssetLoader.Dispose instead.")]
        public static void Release(IAssetLoader assetLoader)
        {
            if (assetLoader == null)
            {
                return;
            }

            if (assetLoader is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}