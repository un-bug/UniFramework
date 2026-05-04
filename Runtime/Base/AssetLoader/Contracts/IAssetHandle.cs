using System;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public interface IAssetHandle<out T> : IDisposable where T : Object
    {
        string Key { get; }

        T Asset { get; }

        bool IsValid { get; }
    }
}