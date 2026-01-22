using System;
using Object = UnityEngine.Object;

namespace UniFramework.Runtime
{
    public interface IAssetLoader
    {
        void LoadAsync<T>(string key, Action<T> onComplete) where T : Object;

        T Load<T>(string key) where T : Object;

        void Release(string key);

        void ReleaseAll();
    }
}