using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;

public partial class AssetManager
{
    [Serializable]
    public class LoadedPrefabAssets
    {
        public GameObject prefab = null;
        public string path;
    }

    [Serializable]
    public class LoadedMaterialAssets
    {
        public Material material = null;
        public string path;
    }

    [Serializable]
    public class LoadedSceneAssets
    {
        public string scenePath;
        public string path;
    }

    [ReadOnly] public List<LoadedPrefabAssets> loadedPrefabAssets = new List<LoadedPrefabAssets>();
    [ReadOnly] public List<LoadedMaterialAssets> loadedMaterialAssets = new List<LoadedMaterialAssets>();
    [ReadOnly] public List<LoadedSceneAssets> loadedSceneAssets = new List<LoadedSceneAssets>();

    [ReadOnly] public bool isAssetBundleLoaded = false;

    public void Initialize_AssetBundleManager()
    {
        loadedPrefabAssets.Clear();
        loadedMaterialAssets.Clear();
        loadedSceneAssets.Clear();

        isAssetBundleLoaded = false;
    }

    public void SetAssetBundleToManager()
    {
        foreach (var name in GetDefaultAssetsToLoad())
        {
            if (name == null || string.IsNullOrEmpty(name))
                continue;

            if (DataManager.Instance.GetAllCarAssetName().Find(x => x.Contains(name)) != null)
            {
                //차
                AssetManager.Instance.LoadGameObject(name, (int)SaveFlag.None, LoadCallback, false);
            }

            if (DataManager.Instance.GetAllCharacterAssetName().Find(x => x.Contains(name)) != null)
            {
                //케릭터
                AssetManager.Instance.LoadGameObject(name, (int)SaveFlag.None, LoadCallback, false);
            }

            if (DataManager.Instance.GetAllMapAssetName().Find(x => x.Contains(name)) != null)
            {
                //맵
                loadedSceneAssets.Add(new LoadedSceneAssets() { scenePath = name, path = name });
            }
        }
    }

    public void LoadCallback(bool is_default, string name, GameObject obj)
    {
        if (obj != null)
            loadedPrefabAssets.Add(new LoadedPrefabAssets() { prefab = obj, path = name });
        else
            Debug.Log("obj is null   " + name);
    }

    public List<string> GetDefaultAssetsToLoad()
    {
        List<string> list = new List<string>();

        //일단 그냥 다 load 시켜주자
        list.AddRange(DataManager.Instance.GetAllCarAssetName());
        list.AddRange(DataManager.Instance.GetAllCharacterAssetName());

        //editor 상에선 그냥 Scene 불러와서 해주자 (정상적으로 안보이기 때문에...!)
#if !UNITY_EDITOR
        list.AddRange(DataManager.Instance.GetAllMapAssetName());
#endif

        /*
        var defaultAssets = DataManager.Instance.GetCommonConfig<string>("defaultAssetsToLoad");

        if (string.IsNullOrEmpty(defaultAssets) == false)
        {
            defaultAssets = defaultAssets.Replace(" ", string.Empty);
            var split = defaultAssets.Split(',');
            foreach (var i in split)
            {
                if (i != string.Empty)
                {
#if UNITY_EDITOR
                    //editor 상에선 그냥 Scene 불러와서 해주자 (정상적으로 안보이기 때문에...!)
                    if (DataManager.Instance.GetAllMapAssetName().Find(x => x.Contains(i)) != null)
                        continue;
#endif
                    list.Add(i);
                }
            }
        }
        else
        { 
        
        }
        */

        return list;
    }

    public List<string> GetMapAssetsToLoad()
    {
        List<string> list = new List<string>();

#if !UNITY_EDITOR
        list.AddRange(DataManager.Instance.GetAllMapAssetName());
#endif

        /*
        var mapAssets = DataManager.Instance.GetCommonConfig<string>("defaultMapAssetsToLoad");

        if (string.IsNullOrEmpty(mapAssets) == false)
        {
            mapAssets = mapAssets.Replace(" ", string.Empty);
            var split = mapAssets.Split(',');
            foreach (var i in split)
            {
                if (i != string.Empty)
                {
                    if (DataManager.Instance.GetAllMapAssetName().Find(x => x.Contains(i)) != null)
                        continue;

                    list.Add(i);
                }
            }
        }
        */


        return list;
    }

    public bool IsDefaultAssetsAllDownloaded()
    {
        var defaultAssets = GetDefaultAssetsToLoad();

        foreach (var assets in defaultAssets)
        {
            var bundleName = assets.ToLower();

            if (IsAssetDownloaded(bundleName) == false)
                return false;
        }

        return true;
    }

    public bool IsAssetDownloaded(string assetName)
    {
        var bundleName = assetName.ToLower();

        if (m_dicLoadedAssetBundles == null || m_dicLoadedAssetBundles.Count <= 0)
            return false;

        if (m_dicLoadedAssetBundles.ContainsKey(bundleName))
            return true;
        else
            return false;
    }

    public bool IsAllMapLoaded()
    {
#if UNITY_EDITOR
        //Editor에서는 Load 처리하자...! (asset bundle이 아닌 build settings에서 가져오기 옴)
        return true;
#endif

        var mapAssets = GetMapAssetsToLoad();

        foreach (var asset in mapAssets)
        {
            var bundleName = asset.ToLower();

            if (IsAssetDownloaded(bundleName) == false)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsAllMapLoaded(ref List<string> assetNeedToBeLoaded)
    {
#if UNITY_EDITOR
        //Editor에서는 Load 처리하자...! (asset bundle이 아닌 build settings에서 가져오기 옴)
        return true;
#endif

        bool isAllLoaded = true;
        var mapAssets = GetMapAssetsToLoad();

        foreach (var asset in mapAssets)
        {
            var bundleName = asset.ToLower();

            if (IsAssetDownloaded(bundleName) == false)
            {
                isAllLoaded = false;

                if (assetNeedToBeLoaded.Contains(asset) == false)
                    assetNeedToBeLoaded.Add(asset);
            }
        }

        return isAllLoaded;
    }

    public void LoadMap(List<string> assetNeedToBeLoaded)
    {
        if (assetNeedToBeLoaded == null || assetNeedToBeLoaded.Count <= 0)
        {
            Debug.Log("Error...! No map asset to be Loaded...!");
            return;
        }

        AssetDefines.AssetObjectEnum type = DtrAssetEnums.Map;
        int save_flag = (int)SaveFlag.None;

        foreach (var i in assetNeedToBeLoaded)
        {
            AssetManager.Instance.LoadScene(i, save_flag, null, true, true, false);
        }
    }
}
