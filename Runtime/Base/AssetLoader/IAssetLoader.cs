using System;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public interface IAssetLoader : IDisposable
    {
        T Load<T>(string key) where T : Object;

        void Release(string key);
    }
}