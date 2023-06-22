using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseAssetLoaderIngames 
{
    public enum IngameAssetType
    {
        None = 0,
        Scene,
        GameObject,
        Texture,
    }
   
    protected List<string>                      m_listPreDownload   = new List<string>();
    List<KeyValuePair<IngameAssetType, string>> m_listCaching       = new List<KeyValuePair<IngameAssetType, string>>();

    protected Action m_callbackEnd;

    Coroutine m_enumDelayed;

    AssetDownloadChecker  m_progress = new AssetDownloadChecker();

    //
    public void Clear()
    {
        m_progress.Clear();

        for( int i = 0, _max = m_listCaching.Count; i < _max; ++i )
        {
            switch (m_listCaching[i].Key)
            {
                case IngameAssetType.GameObject:
                    AssetManager.Instance.CancelReserve<delegateGameObjectLoading>(m_listCaching[i].Value, OnLoaded_GameObj);
                    break;
                case IngameAssetType.Scene:
                    AssetManager.Instance.CancelReserve<delegateSceneLoading>(m_listCaching[i].Value, OnLoaded_Scene);
                    break;
                case IngameAssetType.Texture:
                    AssetManager.Instance.CancelReserve<delegateImageLoading>(m_listCaching[i].Value, OnLoaded_Tex);
                    break;
            }
        }
        m_listCaching.Clear();
        m_listPreDownload.Clear();

        if (m_enumDelayed != null)
        {
            AssetManager.Instance.StopCoroutine(m_enumDelayed);
            m_enumDelayed = null;
        }

        m_callbackEnd = null;
    }

    protected void AttachProgress( Action<float> callback_progress )
    {
        m_progress.CheckPreloadState(m_listPreDownload, callback_progress);
    }

    protected void LoadAsset( string res_name, IngameAssetType type, bool use_async, int save_type, bool parse_attached_script )
    {
        if( string.IsNullOrEmpty(res_name) == true ){
            return;
        }
         
        string _bundle_name, _res_name, _base_bundle_name;
        bool _re = AssetManager.GetBundleNames(res_name, out _bundle_name, out _res_name, out _base_bundle_name);
        if( _re == false ){
            return;
        }

        bool _need_caching1 = NeedCaching(_base_bundle_name, string.Empty);
        bool _need_caching2 = NeedCaching(_bundle_name, _res_name);
        if( _need_caching1 == false && _need_caching2 == false ){
            return;
        }

        switch( type )
        {
         case IngameAssetType.Texture:
             AssetManager.Instance.LoadImage( res_name, save_type, OnLoaded_Tex, false, use_async);
             break;
         case IngameAssetType.GameObject:
             AssetManager.Instance.LoadGameObject(res_name, save_type, OnLoaded_GameObj, parse_attached_script, false, use_async);
             m_listCaching.Add(new KeyValuePair<IngameAssetType,string>(type, res_name));
             break;
         case IngameAssetType.Scene:
             AssetManager.Instance.LoadScene(res_name, save_type, OnLoaded_Scene,  use_async, false, parse_attached_script);
             m_listCaching.Add(new KeyValuePair<IngameAssetType,string>(type, res_name));
             break;
        }
    }

    bool NeedCaching( string bundle_name, string res_name )
    {
        if( string.IsNullOrEmpty(bundle_name) == true )
            return false;

        string _target_name = (string.IsNullOrEmpty(res_name) == true) ? bundle_name : res_name;

        bool _exist = m_listPreDownload.Exists(r => r == _target_name);
        if( _exist == true )
            return false;

        bool _is_cached = AssetManager.Instance.IsPreCached(_target_name);
        if( _is_cached == true )
            return false;

        m_listPreDownload.Add(_target_name);

        return true;
    }

    protected void ExecuteEnd()
    {
        if (m_listPreDownload.Count > 0){
            return;
        }

        if (m_enumDelayed != null){
            return;
        }

        m_enumDelayed = AssetManager.Instance.StartCoroutine(DelayedFunc());
    }

    IEnumerator DelayedFunc()
    {
        yield return null;
        
        m_callbackEnd?.Invoke();

        m_enumDelayed = null;
    }

    //
    void OnLoaded_Tex( bool is_default, string res_name, Texture2D img )
    {
        m_listPreDownload.Remove(res_name);
        ExecuteEnd();
    }

    void OnLoaded_GameObj( bool is_default, string res_name, GameObject obj )
    {
        m_listPreDownload.Remove(res_name);
        ExecuteEnd();
    }

    void OnLoaded_Scene( string name )
    {
        var _target = m_listCaching.Find(r => ( r.Value == name) && ( r.Key == IngameAssetType.Scene) );
        if( _target.Equals( default(KeyValuePair<IngameAssetType, string>)) == false ){
            m_listCaching.Remove(_target);
        }

        m_listPreDownload.Remove(name);
        ExecuteEnd();
    }

    //public void Load(string stadiumAssetId, System.Action<GameObject> onComplete)
    //{
    //    AssetManager.Instance.LoadScene(stadiumAssetId, (int)SaveFlag.Ingame, (res_name) => {
    //        //var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(res_name);
    //        //var rootGameObjects = scene.GetRootGameObjects();
    //
    //        GameObject go = GameObject.FindWithTag("Stadium");
    //
    //        onComplete.SafeInvoke(go);
    //    }, false, true);
    //}
    //
    //public void Load(int stageId, System.Action<GameObject> onComplete)
    //{
    //    var stageInfo = TableManager.Instance.GetData<TableHeader.TableHeader_StageInfo>(stageId);
    //    if (stageInfo == null)
    //    {
    //        Debug.LogError("StageInfo Is Null : " + stageId);
    //        Load("S_Moscow", onComplete);
    //
    //        return;
    //    }
    //
    //    var stadiumAssetId = stageInfo.StadiumAssetID;
    //
    //    Load(stadiumAssetId, onComplete);
    //}
}
