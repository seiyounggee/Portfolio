using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SaveFlag 
{
    None   = 0,
    Global = 0x01,
    Ingame = 0x02,
    Caching = 0x04,         // only download to caching folder
}

public partial class AssetManager 
{
    static bool IsExistFlag( int target, params SaveFlag[] flags )
    {
        if (flags.Length > 0)
        {
            for (int i = 0, _max = flags.Length; i < _max; ++i)
            {
                if ((target & (int)flags[i]) > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static int DeleteFlag( int target, SaveFlag flag )
    {
        target &= ~(int)flag;
        return target;
    }

    //
    T GetLoadedData<T>( string img_name, int save_flag, bool get_cloned = false ) where T: Object
    {
        string _bundle_name, _res_name, _base_bundle_name;
        bool _re = GetBundleNames( img_name, out _bundle_name, out _res_name, out _base_bundle_name );
        if( _re == false ){
            return default(T);
        }

        if (string.IsNullOrEmpty(_res_name))
        {
            return default(T);
        }

        if( string.IsNullOrEmpty( _base_bundle_name ) == false )
        {
            if( m_dicSourceAssetBundles.ContainsKey( _base_bundle_name ) == false ){
                return default(T);
            }
        }

        if( m_dicLoadedAssetBundles.ContainsKey( _bundle_name ) == false ){
            return default(T);
        }

        if( m_dicLoadedAssetBundles[_bundle_name].bundle == null )
        {
            Debug.LogError("Not Exist Assetbundle in LoadedAssetBundles " + _bundle_name);
            m_dicLoadedAssetBundles.Remove(_bundle_name);

            return default(T);
        }

        T _result = m_dicLoadedAssetBundles[ _bundle_name ].GetAsset<T>( _res_name, save_flag, get_cloned );
        return _result;
    }

    public void GetImage( string img_name, delegateImageLoading callback, int save_type, AssetDefines.AssetObjectEnum type, bool load_first = false, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<Texture2D>(img_name, save_type);
        if( _loaded != null )
        {
            if( callback != null ){
                callback(false, img_name, _loaded);
            }
            return;
        }

        if( type != AssetDefines.AssetObjectEnum.None && callback != null )
            callback( true, img_name, GetDefaultImage( type ) );

        bool _use_async_load = true;
        AddToDownload< delegateImageLoading>( callback, img_name, save_type, _use_async_load, load_first, false, callback_progress );
    }

    public void LoadImage( string img_name, int save_type, delegateImageLoading callback, bool load_first = false, bool use_async_load = true, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<Texture2D>( img_name, save_type );
        if( _loaded != null )
        {
            if( callback != null ){
                callback(false, img_name, _loaded);
            }
            return;
        }

        AddToDownload< delegateImageLoading>( callback, img_name, save_type, use_async_load, load_first, false, callback_progress );
    }

    public void GetGameObject( string name, delegateGameObjectLoading callback, int save_type, AssetDefines.AssetObjectEnum type, bool parse_attached_script,
        bool load_first = false, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<GameObject>( name, save_type, parse_attached_script );
        if( _loaded != null  )
        {
            if( callback != null ){
                callback(false, name, _loaded);
            }
            return;
        }

        if( type != AssetDefines.AssetObjectEnum.None && callback != null )
            callback( true, name, GetDefaultObject( type ) );

        bool _use_async_load = true;
        AddToDownload< delegateGameObjectLoading>( callback, name, save_type, _use_async_load, load_first, parse_attached_script, callback_progress );
    }

    public void LoadGameObject( string name, int save_type, delegateGameObjectLoading callback, bool parse_attached_script, bool load_first = false, bool use_async_load = true, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<GameObject>( name, save_type, parse_attached_script );
        if( _loaded != null )
        {
            if( callback != null ){
                callback(false, name, _loaded);
            }
            return;
        }

        AddToDownload< delegateGameObjectLoading>( callback, name, save_type, use_async_load, load_first, parse_attached_script, callback_progress );
    }

    public void LoadSound( string name, int save_type, delegateSoundLoading callback, bool load_first = false, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<AudioClip>(name, save_type);
        if (_loaded != null)
        {
            if( callback != null ){
                callback( name, _loaded);
            }
            return;
        }

        AddToDownload< delegateSoundLoading>( callback, name, save_type, false, load_first, false, callback_progress );
    }

    public void LoadAni( string name, int save_type, delegateAniLoading callback, bool load_first = false, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<AnimationClip>(name, save_type);
        if (_loaded != null)
        {
            if( callback != null ){
                callback( name, _loaded );
            }
            return;
        }

        AddToDownload< delegateAniLoading>( callback, name, save_type, false, load_first, false, callback_progress );
    }

#if NGUI
    public void LoadAtlas( string img_name, int save_type, delegateAtlasLoading callback, bool load_first = false, bool use_async_load = true, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<GameObject>( img_name, save_type );
        if( _loaded != null )
        {
            if( callback != null ){
                callback(_loaded.GetComponent<UIAtlas>());
            }
            return;
        }

        AddToDownload< delegateAtlasLoading>( callback, img_name, save_type, use_async_load, load_first, false, callback_progress );
    }
#endif

    public void LoadScene( string scene_name, int save_type, delegateSceneLoading callback, bool use_async_load, bool load_first, bool parse_attached_script, System.Action<string,float> callback_progress = null )
    {
        AddToDownload< delegateSceneLoading>( callback, scene_name, save_type, use_async_load, load_first, parse_attached_script, callback_progress );
    }

    public void LoadScriptInfo( string name, int save_type, delegateTextLoading callback, bool use_async_load, bool load_first, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<TextAsset>( name, save_type );
        if( _loaded != null )
        {
            if( callback != null ){
                callback(_loaded.text);
            }
            return;
        }

        AddToDownload< delegateTextLoading>( callback, name, save_type, use_async_load, load_first, false, callback_progress );
    }

    public void LoadMaterial( string img_name, int save_type, delegateMaterialLoading callback, bool load_first = false, bool use_async_load = true, System.Action<string,float> callback_progress = null )
    {
        var _loaded = GetLoadedData<Material>( img_name, save_type );
        if( _loaded != null )
        {
            if( callback != null ){
                callback(false, img_name, _loaded);
            }
            return;
        }

        AddToDownload< delegateMaterialLoading>( callback, img_name, save_type, use_async_load, load_first, false, callback_progress );
    }

    //
    public void CancelReserve<T>( string data_name, T callback, System.Action<string,float> callback_progress = null )
    {
        string _bundle_name, _res_name, _base_bundle_name;
        bool _re = GetBundleNames( data_name, out _bundle_name, out _res_name, out _base_bundle_name );
        if( _re == false )
        {
            Debug.LogError( "Not Exact Bundle Format" + data_name );
            return;
        }

        // clear download list
        DownloadData _target_data = m_listBundlesToDownload.Find( r => r.bundleName == _bundle_name );
        ClearDownloadData(_target_data, callback_progress);

        _target_data = m_listBundlesToDownloadFirst.Find(r => r.bundleName == _bundle_name);
        ClearDownloadData(_target_data, callback_progress);

        _target_data = m_listBundlesToDownloadLate.Find(r => r.bundleName == _bundle_name);
        ClearDownloadData(_target_data, callback_progress);

        if ( m_activatedDownloadData != null && m_activatedDownloadData.bundleName == _bundle_name ){
            m_activatedDownloadData.ClearData(callback_progress);
        }

        if( string.IsNullOrEmpty(_base_bundle_name) == false )
        {
            if( m_dicCallbackData.ContainsKey(_base_bundle_name) == true ){
                m_dicCallbackData.Remove( _base_bundle_name );
            }

            _target_data = m_listBundlesToDownload.Find( r => r.bundleName == _base_bundle_name );
            ClearDownloadData(_target_data, callback_progress);

            _target_data = m_listBundlesToDownloadFirst.Find(r => r.bundleName == _base_bundle_name);
            ClearDownloadData(_target_data, callback_progress);

            _target_data = m_listBundlesToDownloadLate.Find(r => r.bundleName == _base_bundle_name);
            ClearDownloadData(_target_data, callback_progress);

            if ( m_activatedDownloadData != null && m_activatedDownloadData.bundleName == _base_bundle_name ){
                m_activatedDownloadData.ClearData(callback_progress);
            }
        }

        // clear load list
        if( m_dicCallbackData.ContainsKey( _bundle_name ) == true )
        {
            var _data = m_dicCallbackData[ _bundle_name ];
            _data.CancelCallback<T>( callback, _res_name );

            if( _data.IsExistDelegate() == false )
            {
                m_dicCallbackData.Remove( _bundle_name );

                var _listLoaded = m_listBundlesToLoad.FindAll(r => r == _data);
                for(int i = 0, _max = _listLoaded.Count; i < _max; ++i)
                {
                    _listLoaded[i].ClearBundle();
                    m_listBundlesToLoad.Remove(_listLoaded[i]);
                }

                _listLoaded = m_listBundlesToLoadFirst.FindAll(r => r == _data);
                for(int i = 0, _max = _listLoaded.Count; i < _max; ++i)
                {
                    _listLoaded[i].ClearBundle();
                    m_listBundlesToLoadFirst.Remove(_listLoaded[i]);
                }

                if( _data == m_activatedLoadData )
                {
                    m_activatedLoadData.ClearBundle();

                    Debug.Log("Bundle Loading Data Clear : " + _bundle_name);

                    StopCoroutine("LoadAssetbundle");
                    StopCoroutine("LoadAssets");
                    m_activatedLoadData = null;

                    // SetNextLoad();
                    if( IsInvoking( "IvkSetNextLoad" ) == false ){
                        Invoke( "IvkSetNextLoad", 0f );
                    }
                }
            }
        }
    }

    void IvkSetNextLoad()
    {
        SetNextLoad();
    }

    void ClearDownloadData( DownloadData data, System.Action<string,float> callback_progress )
    {
        if( data == null ){
            return;
        }

        data.ClearData(callback_progress);

        if( data.referenceCount == 0 )
        {
            m_listBundlesToDownload.Remove(data);
            m_listBundlesToDownloadFirst.Remove(data);
            m_listBundlesToDownloadLate.Remove(data);
        }
    }
	
}
