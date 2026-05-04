using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace UniFramework
{

    public partial class AddressableAssetProvider
    {
        private sealed class CachedAsset
        {
            public Object Asset;
            public AsyncOperationHandle AddressableHandle;
            public int ReferenceCount;

            public CachedAsset(Object asset, AsyncOperationHandle addressableHandle)
            {
                Asset = asset;
                AddressableHandle = addressableHandle;
                ReferenceCount = 1;
            }
        }
    }
}