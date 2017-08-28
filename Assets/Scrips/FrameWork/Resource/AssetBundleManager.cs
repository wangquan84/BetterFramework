﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 已加载的asset bundle
/// 被引用的个数, 方便自动卸载所依赖的资源
/// </summary>
public class LoadedAssetBundle
{
    public string name;
    public AssetBundle assetBundle;
    public int referencedCount;

    public LoadedAssetBundle(string name, AssetBundle assetBundle)
    {
        this.name = name;
        this.assetBundle = assetBundle;
        referencedCount = 1;
    }
}

public abstract class AssetBundleManager : ResManagerBase
{
    private Dictionary<string, LoadedAssetBundle> loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

    public override ResManagerType ManagerType()
    {
        return ResManagerType.assetBundleMgr;
    }

    protected override ResourceInfo CreateResourceInfo(string name, int type)
    {
		ResourceInfo ret = new ResourceInfo();
        ret.Name = name.ToLower();
		ret.ResType = type;
        ret.Path = resourceDefine.GetResourcePath(type, ret.Name).ToLower();

		return ret;
    }

    #region asset bundle 同步加载
    protected override UnityEngine.Object Load(ResourceInfo info)
    {
        AssetBundle assetBundle = GetCacheAssetBundle(info.Name);
        if (assetBundle != null)
        {
            return assetBundle.LoadAsset(info.Name);
        }

        assetBundle = LoadSyncAssetBundle(info);
        if(assetBundle == null)
        {
            return null;
        }

        UnityEngine.Object assetObj = assetBundle.LoadAsset(info.Name);
        if (assetObj == null)
        {
            return null;
        }

        info._assetObj = assetObj;
        return assetObj;
    }

    private AssetBundle LoadSyncAssetBundle(ResourceInfo info)
    {
        LoadSyncDependencies(info);
        return LoadAssetBundle(info);
    }

    private void LoadSyncDependencies(ResourceInfo info)
    {
        List<string> dependencies = info.Dependencies;
        if(dependencies == null)
        {
            return;    
        }

        foreach (string dependence in dependencies)
        {
            AssetBundle assetBundle = GetCacheAssetBundle(dependence);
            if (assetBundle != null) 
            {
                continue;
            }

            ResourceInfo depData = GetDependResourceInfo(dependence);
            if (depData == null)
            {
                Debug.LogError("Find dependend bundle error! name:" + dependence);
                continue;
            }

            LoadAssetBundle(depData);
        }
    }

	protected virtual ResourceInfo GetDependResourceInfo(string name)
	{
		return null;
	}

    private AssetBundle LoadAssetBundle(ResourceInfo info)
    {
        string path = PathConfiger.GetABFilePath(info.Path);
        if (path == string.Empty || path == "")
        {
            Debug.LogError("load sync asset bundle error! path===" + path);
            return null;
        }

        AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
        if (assetbundle == null)
        {
            Debug.LogError("load sync asset bundle error! path===" + path);
            return null;
        }

        loadedAssetBundles.Add(info.Name, new LoadedAssetBundle(info.Name, assetbundle));
        return assetbundle;
    }
    #endregion

    #region 仅仅加载Bundle
    protected override void LoadBundle(ResourceInfo info)
    {
        //ResData bundleData = GetBundleRes(info.Name, info.ResType);
        //if (bundleData == null)
        //{
        //    Debug.LogError("Get bundle data error!");
        //    return;
        //}

        AssetBundle assetBundle = GetCacheAssetBundle(info.Name);
        if (assetBundle != null)
        {
            return;
        }

        LoadSyncAssetBundle(info);
    }
    #endregion

    #region 异步加载资源
    public override IEnumerator LoadAsync(ResourceInfo info, Action<UnityEngine.Object> load, Action<float> progress)
    {
        yield break;

        /*if (assetBundleManifest == null)
        {
            LoadLocalManifest();
            yield return null;
        }

        LoadAsyncAssetBundle(info.Name);

        CoroutineManger.Instance.StartCoroutine(LoadAsync(info.Name, (_obj) =>
        {
            if(_obj == null || load == null)
            {
                return;
            }

            info._assetObj = _obj;
            load(_obj);

        }, progress));
         */
    }

    private IEnumerator LoadAsync(string name, Action<UnityEngine.Object> load, Action<float> progress)
    {
        yield break;

        /*string assetBundleName = GetAssetBundleName(name);

        LoadedAssetBundle loadBundle = null;
        while (true)
        {
            string error = string.Empty;
            loadBundle = GetLoadedAssetBundle(assetBundleName);

            if (error != null || loadBundle != null)
            {
                break;
            }

            yield return null;
        }

        if (loadBundle == null || loadBundle.assetBundle == null)
        {
            if (load != null)
            {
                load(null);
            }

            yield break;
        }

        AssetBundleRequest request = loadBundle.assetBundle.LoadAssetAsync(name);
        while (!request.isDone)
        {
            if (progress != null)
            {
                progress(request.progress);
            }

            yield return null;
        }

        if (progress != null)
        {
            progress(1.0f);
        }

        if (load != null)
        {
            load(request.asset);
        }*/
    }
    
    private void LoadAsyncAssetBundle(string name)
    {
        string assetBundleName = GetAssetBundleName(name);

        // Check if the assetBundle has already been processed.
        bool isAlreadyProcessed = LoadAsyncAssetBundleInternal(assetBundleName);

        // Load dependencies.
        if (!isAlreadyProcessed)
            LoadDependencies(assetBundleName);
    }

    private void LoadDependencies(string assetBundleName)
    {
        /*if(assetBundleManifest == null)
        {
            LoadLocalManifest();
        }

        // Get dependecies from the AssetBundleManifest object..
        // 获取
        string[] dependencies = assetBundleManifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
            return;

        dependenciesAssetBundle.Add(assetBundleName, dependencies);
        for (int i = 0; i < dependencies.Length; i++)
            LoadAsyncAssetBundleInternal(dependencies[i]);
         */
    }
    
    private bool LoadAsyncAssetBundleInternal(string assetBundleName)
    {
        /*
        // Already loaded.
		ResourceInfo bundle = null;
		resourceDic.TryGetValue (assetBundleName, out bundle);
		if (bundle != null)
        {
			bundle.RefCount++;
            return true;
        }

        if (loadingWWWs.ContainsKey(assetBundleName))
            return true;

        string url = GetWWWFileBundlePath(assetBundleName);
        //Debugger.Log("load asset bundle url==" + url);
        loadingWWWs.Add(assetBundleName, new WWW(url));

        loadingErrors.Remove(assetBundleName);
        return false;
        */

        return false;
    }
    #endregion

    #region 资源释放
    /// <summary>
    /// 只释放自己的资源，依赖不释放
    /// </summary>
    /// <param name="info"></param>
    /// <param name="unloadObject"></param>
    public override void UnloadResource(ResourceInfo info, bool unloadObject = false)
    {
        //Debuger.Log(loadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + assetBundleName);
        UnloadAssetBundleInternal(info, unloadObject);
        //Debuger.Log(loadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + assetBundleName);    
    }

    public override void UnloadAllResource(ResourceInfo info, bool unloadObject = false)
    {
        UnloadAssetBundleInternal(info, unloadObject);
        UnloadDependencies(info, unloadObject);
    }

    private void UnloadAssetBundleInternal(ResourceInfo info, bool unloadObject)
    {
        LoadedAssetBundle bundleRes = GetLoadedAssetBundle(info);
        if (bundleRes == null)
            return;

        UnloadAssetBundle(bundleRes, unloadObject);
    }

    private void UnloadDependencies(ResourceInfo info, bool unloadObject)
    {
        // Loop dependencies.
        foreach (var dependency in info.Dependencies)
        {
            LoadedAssetBundle bundle = null;
            if (!loadedAssetBundles.TryGetValue(dependency, out bundle))
            {
                continue;
            }

            UnloadAssetBundle(bundle, unloadObject);
        }
    }

    private void UnloadAssetBundle(LoadedAssetBundle bundleRes, bool unloadObject)
    {
        if (--bundleRes.referencedCount == 0)
        {
            bundleRes.assetBundle.Unload(unloadObject);
            bundleRes.assetBundle = null;
            loadedAssetBundles.Remove(bundleRes.name);
            //Debugger.Log("AssetBundle " + assetBundleName + " has been unloaded successfully");
        }
    }
    #endregion

    #region 公用功能模块
    //public abstract ResData GetBundleRes(string name, int type);

    //public abstract ResData GetBundleResDirect(string name);

    //private ResData GetBundleRes(string name, int type)
    //{
    //    return ABAssetDataMgr.Instance.FindResData(name, type);
    //}

    //private ResData GetBundleResDirect(string name)
    //{
    //    return ABAssetDataMgr.Instance.FindResData(name);
    //}

    private AssetBundle GetCacheAssetBundle(string name)
    {
        LoadedAssetBundle bundle = null;
        if (loadedAssetBundles.TryGetValue(name, out bundle))
        {
            bundle.referencedCount++;
            return bundle.assetBundle;
        }

        return null;
    }

    private LoadedAssetBundle GetLoadedAssetBundle(ResourceInfo info)
    {
        LoadedAssetBundle bundle = null;
        if (loadedAssetBundles.TryGetValue(info.Name, out bundle))
        {
            return bundle;
        }

        return null;
    }

    /// <summary>
    /// 获取asset bundle 名称
    /// </summary>
    /// <param name="assetName">资源名称</param>
    /// <returns></returns>
    private string GetAssetBundleName(string assetName)
    {
        string assetBundleName = string.Format("{0}.{1}", assetName, PathConfiger.BUNDLE_SUFFIX);
        return assetBundleName;
    }
    #endregion
}

