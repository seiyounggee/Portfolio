using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class AssetManager : MonoSingleton<AssetManager> 
{
    public const string SKIPLIST_FILENAME    = "skipassets";
    public const string PRELOADLIST_FILENAME = "preloadasset";

    string      m_assetBaseUrl = string.Empty;

    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);

#if UNITY_EDITOR 
  //     Caching.CleanCache();
#endif
        Initialize();
    }

    public void Initialize()
    {
        InitDownloadEnvs();

        InitDefaultAssets();
        LoadSkipAssetsList();
  //    PreloadAssetData();
    }

    public void Clear()
    {
        Debug.Log( "Resource Clear!!!!!!!!!!!!!!!!!!!!!" );

        //m_assetBaseUrl = string.Empty;

        //ClearDefaultData();
        ClearIngameAssets();
        ClearAssets();

        CancelInvoke( "IvkSetNextLoad" );
    }

    public void SetBaseUrl( string path )
    {
        m_assetBaseUrl = path;
    }

    public bool IsPreCached( string bundle_name )
    {
        if( string.IsNullOrEmpty( bundle_name ) == true )
            return false;

        bundle_name = GetBundleFileName(bundle_name);

#if UNITY_2017_1_OR_NEWER
        string _hash = GetAssetHash(bundle_name);
        if (string.IsNullOrEmpty(_hash) == true){
            return false;
        }

        var _hashkey = Hash128.Parse(_hash);

        string _full_path = GetBundlePath( bundle_name );
        return Caching.IsVersionCached( _full_path, _hashkey );
#else
        int _revision          = GetAssetVersion( bundle_name );
        //int _attached_revision = GetAttachedAssetVersion(bundle_name);
        //if( _attached_revision >=0          && 
        //    _revision == _attached_revision ){
        //    return true;
        //}
        string _full_path = GetBundlePath( bundle_name );   
        return Caching.IsVersionCached( _full_path, _revision );
#endif
    }

    public static string GetStreamingAssetResourceFolder()
    {
        var _builder = new System.Text.StringBuilder();

        string _path = Application.streamingAssetsPath;
#if UNITY_EDITOR || UNITY_STANDALONE
        //_builder.Append( "file:///");

        bool isWinEditor = Application.platform == RuntimePlatform.WindowsEditor;
        bool isOSXEditor = Application.platform == RuntimePlatform.OSXEditor;

        //프로젝트가 mac 외장하드에서 불러오는 경우 따로 처리해주자 
        if (isOSXEditor)
        {
            _builder.Append("file://");
        }
        else
        {
            _builder.Append("file:///");
        }

#else
        if( _path.Contains( "://") == false){
        _builder.Append( "file://");
        }
#endif
        var _target_os = AssetDefines.GetOSType();
        _builder.Append(System.IO.Path.Combine(_path, _target_os.ToString()));
        _builder.Append(System.IO.Path.DirectorySeparatorChar);

        return _builder.ToString();
    }

    public static string GetBundleFileName( string bundle_name )
    {
        if (string.IsNullOrEmpty(bundle_name) == true)
            return string.Empty;

        if (bundle_name.Contains(AssetIOUtil.SEPERATOR_ONEPIECE) == true)
        {
            bundle_name = bundle_name.Split(AssetIOUtil.SEPERATOR_ONEPIECE[0])[0];
        }
        else if (bundle_name.Contains(AssetIOUtil.SEPERATOR_BASEASSET) == true)
        {
        }

        return bundle_name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
    }

    public void PreloadAssetData( int save_flag = (int)SaveFlag.Global, List<string> list_to_preload = null )
    {
        m_listPreloadAssets.Clear();

     // string _file_name = string.Format("{0}_{1}", AssetDefines.GetOSType(), PRELOADLIST_FILENAME);
        string _file_name = PRELOADLIST_FILENAME;

        var _asset = Resources.Load( _file_name ) as TextAsset;
        if( _asset != null )
        {
            // ex) test_asset,false
            // string[] _list = _asset.text.Split(","[0]);
            string   _result_data = _asset.text.Replace("\r", "");
            string[] _list        = _result_data.Split("\n"[0]);
            for( int i = 0, _max = _list.Length; i < _max; ++i )
            {
                string[] _data     = _list[i].Split(","[0]);
                if (_data.Length != 2){
                    continue;
                }

                int _version = GetAssetVersion(_data[0]);
                if (_version < 0){
                    continue;
                }

                bool _parse_script = System.Convert.ToBoolean(_data[1]);

                m_listPreloadAssets.Add(_data[0]);

                ProcessDownLoad( _data[0], string.Empty, save_flag, true, _parse_script, null);
            }
        }

        //
        if (list_to_preload != null)
        {
            var _list_result = new List<string>();
            for (int i = 0, _max = list_to_preload.Count; i < _max; ++i)
            {
                var _bundle_name = AssetManager.GetBundleFileName(list_to_preload[i]);
                if (string.IsNullOrEmpty(_bundle_name) == true)
                    continue;

                if (_list_result.Contains(_bundle_name) == false)
                    _list_result.Add(_bundle_name);
            }

            for (int i = 0, _max = _list_result.Count; i < _max; ++i){
                ProcessDownLoad( _list_result[i], string.Empty, (int)SaveFlag.Caching, false, false, null);
            }
        }
    }

    public void LoadLookupTable()
    {
        m_dicLookupTable.Clear();

        //
        Dictionary<string, KeyValuePair<int,string>> _dic_temp = new Dictionary<string, KeyValuePair<int, string>>();

        var _targetAssetData = GetActivatedAssetData();
        if (_targetAssetData == null)
            return;

        var _enum = _targetAssetData.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _target_data = _enum.Current.Value.lookuptable;
            if (string.IsNullOrEmpty(_target_data) == true)
                continue;

            var _file_name = System.IO.Path.GetFileNameWithoutExtension(_enum.Current.Value.filename);
            int _version  = _enum.Current.Value.version;

            string[] _split = _target_data.Split(AssetIOUtil.SEPERATOR_LOOKUPTABLE[0]);
            for (int i = 0, _max = _split.Length; i < _max; ++i)
            {
                if (_dic_temp.ContainsKey(_split[i]) == true)
                {
                    var _saved_version = _dic_temp[_split[i]].Key;
                    if ( _version < _saved_version )
                        continue;
                }

                _dic_temp[_split[i]] = new KeyValuePair<int, string>(_version, _file_name);
            }
        }

        //
        var _enum_dic = _dic_temp.GetEnumerator();
        while (_enum_dic.MoveNext())
        {
            var _data = _enum_dic.Current;
            m_dicLookupTable.Add(_data.Key, _data.Value.Value);
        }
    }

    public string FindFullAssetNameFromLookup( string bundle_name )
    {
        if (bundle_name.Contains(AssetIOUtil.SEPERATOR_ONEPIECE) == true)
        {
            Debug.LogWarning("Seperator In Name >> " + bundle_name);
            return bundle_name;
        }

        if (m_dicLookupTable.ContainsKey(bundle_name) == false)
        {
            Debug.LogError("Not Exist In Lookup >> " + bundle_name);
            return string.Empty;
        }

        return string.Format("{0}{1}{2}", m_dicLookupTable[bundle_name], AssetIOUtil.SEPERATOR_ONEPIECE, bundle_name);
    }

    public string GetFullAssetNameFromLookup(string bundle_name)
    {
        string _lookup_data = FindFullAssetNameFromLookup(bundle_name);
        if (string.IsNullOrEmpty(_lookup_data) == true){
            return bundle_name;
        }

        return _lookup_data;
    }

    //
    //public bool AttachProgressState( bool isPreCached, string bundle_name, int save_type, System.Action<string,float> progress, bool set_load_first )
    public bool AttachProgressState(bool isPreCached, string bundle_name, System.Action<string, float> progress)
    {
        if( isPreCached && IsPreCached(bundle_name) == true )
            return false;

        bundle_name = GetBundleFileName(bundle_name);
        bool _re = false;
        //DownloadData _target_data = m_listBundlesToDownloadFirst.Find( r => r.bundleName == bundle_name );
        //if( _target_data != null )
        //{
        //    _re = true;
        //    _target_data.UpdateData( save_type, progress );
        //}
        //else
        //{
        //    _target_data = m_listBundlesToDownload.Find( r => r.bundleName == bundle_name );
        //    if( _target_data != null )
        //    {
        //        _re = true;
        //        _target_data.UpdateData( save_type, progress );
        //        if( set_load_first == true )
        //        {
        //            m_listBundlesToDownload.Remove( _target_data );
        //            m_listBundlesToDownloadFirst.Add( _target_data );
        //            _target_data.load_immediately = true;
        //        }
        //    }
        //}
        DownloadData _target_data;
        _target_data = m_listBundlesToDownloadFirst.Find(r => r.bundleName == bundle_name);
        if( _target_data != null )
        {
            _re = true;
            _target_data.UpdateData(0, progress);
        }

        _target_data = m_listBundlesToDownload.Find(r => r.bundleName == bundle_name);
        if (_target_data != null)
        {
            _re = true;
            _target_data.UpdateData(0, progress);
        }

        _target_data = m_listBundlesToDownloadLate.Find(r => r.bundleName == bundle_name);
        if (_target_data != null)
        {
            _re = true;
            _target_data.UpdateData(0, progress);
        }

        if (m_activatedDownloadData != null && m_activatedDownloadData.bundleName == bundle_name)
        {
            m_activatedDownloadData.callbackProgress += progress;
            _re = true;
        }

        return _re;
    }

    public void DetachProgressState( string bundle_name, System.Action<string,float> progress )
    {
        bundle_name = GetBundleFileName(bundle_name);

        var _list = m_listBundlesToDownload.FindAll(r => r.bundleName == bundle_name);
        for (int i = 0, _max = _list.Count; i < _max; ++i)
        {
            _list[i].callbackProgress -= progress;
        }

        _list = m_listBundlesToDownloadFirst.FindAll(r => r.bundleName == bundle_name);
        for (int i = 0, _max = _list.Count; i < _max; ++i)
        {
            _list[i].callbackProgress -= progress;
        }

        _list = m_listBundlesToDownloadLate.FindAll(r => r.bundleName == bundle_name);
        for (int i = 0, _max = _list.Count; i < _max; ++i)
        {
            _list[i].callbackProgress -= progress;
        }

        if (m_activatedDownloadData != null && m_activatedDownloadData.bundleName == bundle_name)
        {
            m_activatedDownloadData.callbackProgress -= progress;
        }
    }
}
