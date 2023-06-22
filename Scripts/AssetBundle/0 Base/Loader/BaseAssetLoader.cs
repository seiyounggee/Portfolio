using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAssetLoader  
{

	
}

public abstract class BaseAssetLoaderWithScriptData : BaseAssetLoader
{
    List<PostLoadingAssetInfo> m_listPostAssetInfo      = new List<PostLoadingAssetInfo>();
    PostLoadingAssetInfo       m_activatedPostAssetInfo = null;

    string                     m_post_asset_filename;

    bool                       m_bPostLoadFirst;
    bool                       m_bPostAsyncLoad;
    int                        m_nPostSaveFlag;
    System.Action              m_callbackPostLoadEnd;

    //
    protected abstract string GetPostAssetFileNameFormat( System.Type type, string res_name );
    protected abstract bool   IsLoadFromResourcesDirectly( System.Type type, string res_name );
    protected abstract string GetPostAssetFilePathInResources(System.Type type, string res_name);

    //
    public virtual void Clear()
    {
        if( m_activatedPostAssetInfo != null )
        {
            if (m_activatedPostAssetInfo.asset_type == typeof(AnimationClip))
            {
                AssetManager.Instance.CancelReserve<delegateAniLoading>(m_post_asset_filename, OnResAnimationCallback );
            }
            else if (m_activatedPostAssetInfo.asset_type == typeof(AudioClip))
            {
                AssetManager.Instance.CancelReserve<delegateSoundLoading>(m_post_asset_filename, OnResAudioCallback );
            }
            else if (m_activatedPostAssetInfo.asset_type == typeof(Texture))
            {
                AssetManager.Instance.CancelReserve<delegateImageLoading>(m_post_asset_filename, OnResTextureCallback );
            }

            m_post_asset_filename = string.Empty;
            m_activatedPostAssetInfo  = null;
        }

        m_listPostAssetInfo.Clear();

        m_bPostLoadFirst = false;
        m_bPostAsyncLoad = false;
        m_nPostSaveFlag  = 0;

        m_callbackPostLoadEnd = null;
    }

    //
    protected bool CheckScriptData( params GameObject[] roots )
    {
        bool _re = AssetIOUtil.CheckScriptData( m_listPostAssetInfo, roots );
        return _re;
    }

    protected void LoadPostAsset( System.Action callback, bool load_async, int save_flag, bool load_first )
    {
        m_callbackPostLoadEnd = callback;
        m_nPostSaveFlag       = save_flag;
        m_bPostAsyncLoad      = load_async;
        m_bPostLoadFirst      = load_first;

        if( m_listPostAssetInfo.Count > 0 )
        {
            // extract resources asset from list
            List< PostLoadingAssetInfo> _listLoadFromResources = new List<PostLoadingAssetInfo>();
            var _enum = m_listPostAssetInfo.GetEnumerator();
            while( _enum.MoveNext() )
            {
                PostLoadingAssetInfo _curr = _enum.Current;

                if( IsLoadFromResourcesDirectly( _curr.asset_type, _curr.asset_name ) == true )
                {
                    _listLoadFromResources.Add(_curr);

                    m_listPostAssetInfo.Remove(_curr);
                    _enum = m_listPostAssetInfo.GetEnumerator();
                }
            }

            // load from resources folder
            _enum = _listLoadFromResources.GetEnumerator();
            while (_enum.MoveNext())
            {
                PostLoadingAssetInfo _curr = _enum.Current;

                string _path = GetPostAssetFilePathInResources(_curr.asset_type, _curr.asset_name);
                if( string.IsNullOrEmpty(_path) == true )
                {
                    Debug.LogError("Error : Try to Load From Resources >> " + _curr.asset_name);
                    _listLoadFromResources.Remove( _curr );
                    continue;
                }
                
                var _loaded  = Resources.Load(_path);
                var _infos = _listLoadFromResources.FindAll(r => (r.asset_name == _curr.asset_name && r.asset_type == _curr.asset_type));
                if (_infos.Count > 0)
                {
                    for (int j = 0, _maxj = _infos.Count; j < _maxj; ++j)
                    {
                        AssetIOUtil.ApplyLoadedAsset(_infos[j], _loaded);
                
                        _listLoadFromResources.Remove(_infos[j]);
                    }
                    _enum = _listLoadFromResources.GetEnumerator();
                }
            }

            _listLoadFromResources.Clear();
        }

        SetNextPostBundleLoad();
    }

    void SetNextPostBundleLoad()
    {
        while( m_listPostAssetInfo.Count > 0 )
        {
           var _activatedInfo = m_listPostAssetInfo[0];
           m_listPostAssetInfo.RemoveAt(0);

           var _activated_asset_filename = GetPostAssetFileNameFormat(_activatedInfo.asset_type, _activatedInfo.asset_name);
           if( string.IsNullOrEmpty(_activated_asset_filename) == false )
           {
                if( AssetIOUtil.IsPostLoadingType( _activatedInfo.asset_type, null, null ) == true )
               {
                   m_activatedPostAssetInfo = _activatedInfo;
                   m_post_asset_filename    = _activated_asset_filename;
                   break;
               }
               else
               {
                   Debug.LogError("Not Supported Type : " + _activatedInfo.asset_type.ToString());
               }
           }
           else
           {
               Debug.LogError("?? Why Called this ?? : " + _activatedInfo.asset_name );
           }
        }

        if (m_listPostAssetInfo.Count == 0 && m_activatedPostAssetInfo == null)
        {
            if (m_callbackPostLoadEnd != null)
            {
                m_callbackPostLoadEnd();
            }
        }
        else
        {
            LoadAssetData();
        }
    }

    void LoadAssetData()
    {
        if (m_activatedPostAssetInfo.asset_type == typeof(AudioClip))
        {
            AssetManager.Instance.LoadSound(m_post_asset_filename, m_nPostSaveFlag, OnResAudioCallback, m_bPostLoadFirst);
        }
        else if (m_activatedPostAssetInfo.asset_type == typeof(AnimationClip))
        {
            AssetManager.Instance.LoadAni(m_post_asset_filename, m_nPostSaveFlag, OnResAnimationCallback, m_bPostLoadFirst);
        }
        else if (m_activatedPostAssetInfo.asset_type == typeof(Texture))
        {
            AssetManager.Instance.LoadImage(m_post_asset_filename, m_nPostSaveFlag, OnResTextureCallback, m_bPostLoadFirst, m_bPostAsyncLoad);
        }
        else
        {
            Debug.LogError("Not Supported Yet ?! " + m_activatedPostAssetInfo.asset_type.ToString() );
        }
    }

    //
    void OnResAnimationCallback( string name, AnimationClip clip  )
    {
        if (m_activatedPostAssetInfo != null)
        {
            AssetIOUtil.ApplyLoadedAsset(m_activatedPostAssetInfo, clip);

            var _infos = m_listPostAssetInfo.FindAll(r => (r.asset_name == m_activatedPostAssetInfo.asset_name && r.asset_type == m_activatedPostAssetInfo.asset_type));
            for (int j = 0, _maxj = _infos.Count; j < _maxj; ++j)
            {
                AssetIOUtil.ApplyLoadedAsset(_infos[j], clip);

                m_listPostAssetInfo.Remove(_infos[j]);
            }

            m_activatedPostAssetInfo = null;
            m_post_asset_filename = string.Empty;
        }

        SetNextPostBundleLoad();
    }

    void OnResAudioCallback( string name, AudioClip clip )
    {
        if( m_activatedPostAssetInfo != null )
        {
            AssetIOUtil.ApplyLoadedAsset(m_activatedPostAssetInfo, clip);

            var _infos = m_listPostAssetInfo.FindAll(r => (r.asset_name == m_activatedPostAssetInfo.asset_name && r.asset_type == m_activatedPostAssetInfo.asset_type));
            for (int j = 0, _maxj = _infos.Count; j < _maxj; ++j)
            {
                AssetIOUtil.ApplyLoadedAsset(_infos[j], clip);

                m_listPostAssetInfo.Remove(_infos[j]);
            }

            m_activatedPostAssetInfo = null;
            m_post_asset_filename = string.Empty;
        }

        SetNextPostBundleLoad();
    }

    void OnResTextureCallback( bool is_default, string name, Texture2D img )
    {
        if( m_activatedPostAssetInfo != null && is_default == false )
        {
            AssetIOUtil.ApplyLoadedAsset(m_activatedPostAssetInfo, img);

            var _infos = m_listPostAssetInfo.FindAll(r => (r.asset_name == m_activatedPostAssetInfo.asset_name && r.asset_type == m_activatedPostAssetInfo.asset_type));
            for (int j = 0, _maxj = _infos.Count; j < _maxj; ++j)
            {
                AssetIOUtil.ApplyLoadedAsset(_infos[j], img);

                m_listPostAssetInfo.Remove(_infos[j]);
            }

            m_activatedPostAssetInfo = null;
            m_post_asset_filename = string.Empty;
        }

        SetNextPostBundleLoad();
    }
}
