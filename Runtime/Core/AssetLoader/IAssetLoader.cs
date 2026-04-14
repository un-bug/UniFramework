using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public interface IAssetLoader : IDisposable
{
    T Load<T>(string key) where T : Object;
    void Release(string key);
}

public class AddressableAssetLoader : IAssetLoader
{
    private Dictionary<string, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle> m_Handles = new();

    public T Load<T>(string key) where T : Object
    {
        if (m_Handles.TryGetValue(key, out var existingHandle))
        {
            return (T)existingHandle.Result;
        }

        var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
        handle.WaitForCompletion();

        m_Handles[key] = handle;
        return handle.Result;
    }
    public void Release(string key)
    {
        if (m_Handles.TryGetValue(key, out var handle))
        {
            UnityEngine.AddressableAssets.Addressables.Release(handle);
            m_Handles.Remove(key);
        }
    }

    public void Dispose()
    {
        foreach (var handle in m_Handles.Values)
        {
            UnityEngine.AddressableAssets.Addressables.Release(handle);
        }

        m_Handles.Clear();
    }
}

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