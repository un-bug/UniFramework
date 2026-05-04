using System;

namespace UniFramework
{

    public partial class AddressableAssetProvider
    {
        private readonly struct AssetCacheKey : IEquatable<AssetCacheKey>
        {
            public readonly string Key;
            public readonly Type AssetType;

            public AssetCacheKey(string key, Type assetType)
            {
                Key = key;
                AssetType = assetType;
            }

            public bool Equals(AssetCacheKey other)
            {
                return Key == other.Key && AssetType == other.AssetType;
            }

            public override bool Equals(object obj)
            {
                return obj is AssetCacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (AssetType != null ? AssetType.GetHashCode() : 0);
                }
            }
        }
    }
}