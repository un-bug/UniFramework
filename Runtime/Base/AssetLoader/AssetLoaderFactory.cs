using System;
using UnityEngine;

namespace UniFramework
{
    public static class AssetLoaderFactory
    {
        private static readonly AddressableAssetProvider s_AddressableAssetProvider = new AddressableAssetProvider();

        public static IAssetLoader Get()
        {
            return new AssetLoader(s_AddressableAssetProvider);
        }

        [Obsolete("Use Dispose instead.")]
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