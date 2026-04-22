using System;

namespace UniFramework.Runtime
{
    public static class AssetLoaderFactory
    {
        public static IAssetLoader Get()
        {
            return new AddressableAssetLoader();
        }

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