using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniFramework.Runtime
{
    public class ResourcesLoader : IAssetLoader
    {
        private readonly Dictionary<string, Object> m_LoadedAssets = new Dictionary<string, Object>();

        public T Load<T>(string key) where T : Object
        {
            if (m_LoadedAssets.TryGetValue(key, out var asset))
            {
                return asset as T;
            }

            var loaded = Resources.Load<T>(key);
            if (loaded != null)
            {
                m_LoadedAssets.TryAdd(key, loaded);
            }
            else
            {
                Debug.LogError($"[ResourcesLoader] Load Failed: {key}");
            }

            return loaded;
        }

        public void LoadAsync<T>(string key, Action<T> onComplete) where T : Object
        {
            if (m_LoadedAssets.TryGetValue(key, out var cached))
            {
                onComplete?.Invoke(cached as T);
                return;
            }

            var request = Resources.LoadAsync<T>(key);
            request.completed += operation =>
            {
                var asset = request.asset as T;
                if (asset != null)
                {
                    m_LoadedAssets.TryAdd(key, asset);
                }
                else
                {
                    Debug.LogError($"[ResourcesLoader] LoadAsync Failed: {key}");
                }

                onComplete?.Invoke(request.asset as T);
            };
        }

        public void Release(string key)
        {
            if (m_LoadedAssets.TryGetValue(key, out var asset))
            {
                Resources.UnloadAsset(asset);
                m_LoadedAssets.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var asset in m_LoadedAssets.Values)
            {
                Resources.UnloadAsset(asset);
            }

            m_LoadedAssets.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}