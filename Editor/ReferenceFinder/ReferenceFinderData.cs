using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ReferenceFinderData
{
    //缓存路径
    private const string CACHE_PATH = "Library/ReferenceFinderCache.json";
    private const string CACHE_VERSION = "V2";
    //资源引用信息字典
    public Dictionary<string, AssetDescription> assetDict = new Dictionary<string, AssetDescription>();    

    //收集资源引用信息并更新缓存
    public void CollectDependenciesInfo()
    {
        try
        {          
            ReadFromCache();
            var allAssets = AssetDatabase.GetAllAssetPaths();
            int totalCount = allAssets.Length;
            for (int i = 0; i < allAssets.Length; i++)
            {
                //每遍历100个Asset，更新一下进度条，同时对进度条的取消操作进行处理
                if ((i % 100 == 0) && EditorUtility.DisplayCancelableProgressBar("Refresh", string.Format("Collecting {0} assets", i), (float)i / totalCount))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                if (File.Exists(allAssets[i]))
                    ImportAsset(allAssets[i]);
                if (i % 2000 == 0)
                    GC.Collect();
            }      
            //将信息写入缓存
            EditorUtility.DisplayCancelableProgressBar("Refresh", "Write to cache", 1f);
            WriteToCache();
            //生成引用数据
            EditorUtility.DisplayCancelableProgressBar("Refresh", "Generating asset reference info", 1f);
            UpdateReferenceInfo();
            EditorUtility.ClearProgressBar();
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            EditorUtility.ClearProgressBar();
        }
    }

    //通过依赖信息更新引用信息
    private void UpdateReferenceInfo()
    {
        foreach (var asset in assetDict.Values)
        {
            asset.references.Clear();
        }

        foreach(var asset in assetDict)
        {
            foreach(var assetGuid in asset.Value.dependencies)
            {
                AssetDescription dependency;
                if (string.IsNullOrEmpty(assetGuid) || !assetDict.TryGetValue(assetGuid, out dependency))
                    continue;

                if (!dependency.references.Contains(asset.Key))
                    dependency.references.Add(asset.Key);
            }
        }
    }

    //生成并加入引用信息
    private void ImportAsset(string path)
    {
        if (!path.StartsWith("Assets/"))
            return;

        //通过path获取guid进行储存
        string guid = AssetDatabase.AssetPathToGUID(path);
        //获取该资源的最后修改时间，用于之后的修改判断
        Hash128 assetDependencyHash = AssetDatabase.GetAssetDependencyHash(path);
        //如果assetDict没包含该guid或包含了修改时间不一样则需要更新
        if (!assetDict.ContainsKey(guid) || assetDict[guid].assetDependencyHash != assetDependencyHash.ToString())
        {
            //将每个资源的直接依赖资源转化为guid进行储存
            var guids = AssetDatabase.GetDependencies(path, false).
                Select(p => AssetDatabase.AssetPathToGUID(p)).
                Where(g => !string.IsNullOrEmpty(g) && g != guid).
                Distinct().
                ToList();

            //生成asset依赖信息，被引用需要在所有的asset依赖信息生成完后才能生成
            AssetDescription ad = new AssetDescription();
            ad.name = Path.GetFileNameWithoutExtension(path);
            ad.path = path;
            ad.assetDependencyHash = assetDependencyHash.ToString();
            ad.dependencies = guids;

            if (assetDict.ContainsKey(guid))
                assetDict[guid] = ad;
            else
                assetDict.Add(guid, ad);
        }
    }

    //读取缓存信息
    public bool ReadFromCache()
    {
        assetDict.Clear();
        if (!File.Exists(CACHE_PATH))
        {
            return false;
        }

        ReferenceFinderCache cache;
        try
        {
            EditorUtility.DisplayCancelableProgressBar("Import Cache", "Reading Cache", 0);
            string json = File.ReadAllText(CACHE_PATH);
            cache = JsonUtility.FromJson<ReferenceFinderCache>(json);
            if (cache == null || cache.version != CACHE_VERSION || cache.assets == null)
            {
                return false;
            }
        }
        catch(Exception e)
        {
            Debug.LogWarning("Reference Finder cache is invalid and will be rebuilt. " + e.Message);
            return false;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        foreach (var serializedAsset in cache.assets)
        {
            if (serializedAsset == null || string.IsNullOrEmpty(serializedAsset.guid))
                continue;

            string path = AssetDatabase.GUIDToAssetPath(serializedAsset.guid);
            if (!string.IsNullOrEmpty(path))
            {
                var ad = new AssetDescription();
                ad.name = Path.GetFileNameWithoutExtension(path);
                ad.path = path;
                ad.assetDependencyHash = serializedAsset.assetDependencyHash;
                ad.dependencies = serializedAsset.dependencies ?? new List<string>();
                assetDict[serializedAsset.guid] = ad;
            }
        }

        foreach (var asset in assetDict.Values)
        {
            asset.dependencies = asset.dependencies.
                Where(g => !string.IsNullOrEmpty(g) && assetDict.ContainsKey(g)).
                Distinct().
                ToList();
        }
        UpdateReferenceInfo();
        return true;
    }

    //写入缓存
    private void WriteToCache()
    {
        var cache = new ReferenceFinderCache();
        cache.version = CACHE_VERSION;
        cache.assets = new List<SerializedAssetDescription>();

        foreach (var pair in assetDict)
        {
            var serializedAsset = new SerializedAssetDescription();
            serializedAsset.guid = pair.Key;
            serializedAsset.assetDependencyHash = pair.Value.assetDependencyHash;
            serializedAsset.dependencies = pair.Value.dependencies.
                Where(g => !string.IsNullOrEmpty(g) && assetDict.ContainsKey(g)).
                Distinct().
                ToList();
            cache.assets.Add(serializedAsset);
        }

        string json = JsonUtility.ToJson(cache);
        File.WriteAllText(CACHE_PATH, json);
    }
    
    //更新引用信息状态
    public void UpdateAssetState(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return;

        AssetDescription ad;
        if (assetDict.TryGetValue(guid,out ad) && ad.state != AssetState.NODATA)
        {            
            if (File.Exists(ad.path))
            {
                //修改时间与记录的不同为修改过的资源
                if (ad.assetDependencyHash != AssetDatabase.GetAssetDependencyHash(ad.path).ToString())
                {
                    ad.state = AssetState.CHANGED;
                }
                else
                {
                    //默认为普通资源
                    ad.state = AssetState.NORMAL;
                }
            }
            //不存在为丢失
            else
            {
                ad.state = AssetState.MISSING;
            }
        }
        
        //字典中没有该数据
        else if(!assetDict.TryGetValue(guid, out ad))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ad = new AssetDescription();
            ad.name = Path.GetFileNameWithoutExtension(path);
            ad.path = path;
            ad.state = AssetState.NODATA;
            assetDict.Add(guid, ad);
        }
    }

    //根据引用信息状态获取状态描述
    public static string GetInfoByState(AssetState state)
    {
        if(state == AssetState.CHANGED)
        {
            return "<color=#F0672AFF>Changed</color>";
        }
        else if (state == AssetState.MISSING)
        {
            return "<color=#FF0000FF>Missing</color>";
        }
        else if(state == AssetState.NODATA)
        {
            return "<color=#FFE300FF>No Data</color>";
        }
        return "<color=#8A8A8AFF>Normal</color>";
    }

    public class AssetDescription
    {
        public string name = "";
        public string path = "";
        public string assetDependencyHash;
        public List<string> dependencies = new List<string>();
        public List<string> references = new List<string>();
        public AssetState state = AssetState.NORMAL;
    }

    public enum AssetState
    {
        NORMAL,
        CHANGED,
        MISSING,
        NODATA,        
    }

    [Serializable]
    private class ReferenceFinderCache
    {
        public string version;
        public List<SerializedAssetDescription> assets = new List<SerializedAssetDescription>();
    }

    [Serializable]
    private class SerializedAssetDescription
    {
        public string guid;
        public string assetDependencyHash;
        public List<string> dependencies = new List<string>();
    }
}
