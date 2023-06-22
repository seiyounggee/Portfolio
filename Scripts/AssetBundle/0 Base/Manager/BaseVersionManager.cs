using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class BaseVersionManager 
{
    public static readonly string REFSFILE_EXTENTION = ".refs";
    public static string GetVersionFilePath()
    {
        return string.Format( "{0}{1}", Application.temporaryCachePath, Path.DirectorySeparatorChar );
    }
}

public class BaseVersionManager<T> : Singleton<T> where T : class, new()
{
    public enum VersionType
    {
        None = 0,
        Resource,
        Asset,
    }

    protected class VersionData
    {
        public int          attached;
        public int          download;
        public int          revision;
        public string       extension;

        //
        public string GetAttachedFileName()
        {
            return string.Format( "{0}{1}", attached, extension );
        }

        public void Init( string ext )
        {
            extension  = ext;
            download   = 0;
            attached   = 0;
            revision   = 0;
        }
    }

    protected VersionData     m_RefsData   = new VersionData();
    protected VersionData     m_AssetData  = new VersionData();

    //
    class DownloadData
    {
        public VersionType  type;
        public int          target_version;
        public string       target_url;
        public int          try_count;
    }

    List< DownloadData>  m_listDownload = new List<DownloadData>();
    DownloadData         m_activatedDownload = null;


    Action<VersionType, float>     m_eventProcessing = null;

    Action<bool>            m_eventEnd;
    Action<byte[]>          m_eventLoadTables;
    Action<bool,byte[]>     m_eventLoadAssetLists;

    Coroutine               m_wwwEnum = null;

    const int TRY_MAX_COUNT = 2;
    

    //
    public BaseVersionManager()
    {
        LoadVersion();
        Debug.Log( string.Format( "LoadVersion >> attached : {0} , {1}" , m_RefsData.attached, m_AssetData.attached ) );
    }

    public void Clear()
    {
        m_listDownload.Clear();
        m_activatedDownload = null;

        m_eventProcessing = null;

        m_eventLoadAssetLists = null;
        m_eventLoadTables = null;
        m_eventEnd = null;

        if (m_wwwEnum != null)
        {
            AssetManager.Instance.GetComponent<MonoBehaviour>().StopCoroutine(m_wwwEnum);
            m_wwwEnum = null;
        }
    }

    protected virtual void LoadVersion()
    {
        m_RefsData.Init( BaseVersionManager.REFSFILE_EXTENTION );
        m_AssetData.Init( AssetVersions.FILE_EXTENTION );

        var _target_os = AssetDefines.GetOSType();
        ReadAttachedVersion(_target_os, out m_RefsData.attached, out m_AssetData.attached);
    }

    bool ReadAttachedVersion( AssetDefines.eOSType type, out int resource_version, out int asset_version )
    {
        resource_version = 0;
        asset_version    = 0;

        TextAsset _asset = Resources.Load( type.ToString() ) as TextAsset;
        if( _asset == null ){
            return false;
        }

        string[] _split = _asset.text.Split(","[0]);
        if( _split.Length > 0 )
        {
            int.TryParse(_split[0], out resource_version);

            if( _split.Length > 1 )
            {
                if( int.TryParse(_split[1], out asset_version) == true )
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void LoadResultTableData( int revision, string fileURL, Action<byte[]> eventTables, Action<bool> eventEnd )
    {
        m_eventEnd = eventEnd;
        m_eventLoadTables = eventTables;
        m_eventLoadAssetLists = null;

        CheckDownloadData( VersionType.Resource, fileURL, revision, m_RefsData );
        SetNextDownload( true );
    }

    public void LoadResourceData( int revision, string fileURL, string avURL, Action<byte[]> eventTables, Action<bool,byte[]> eventAssetLists, Action<bool> eventEnd)
    {
        //const string HTTP = "https://";
        //if( string.IsNullOrEmpty( fileURL ) == false && fileURL.Contains( HTTP ) == false ){
        //    fileURL = HTTP + fileURL;
        //}
        //
        //if( string.IsNullOrEmpty(avURL) == false && avURL.Contains(HTTP) == false ){
        //    avURL = HTTP + avURL;
        //}

        m_eventEnd            = eventEnd;
        m_eventLoadTables     = eventTables;
        m_eventLoadAssetLists = eventAssetLists;

        //리소스 다운로드
        CheckDownloadData( VersionType.Resource, fileURL, revision, m_RefsData );

        //에셋 다운로드
#if UNITY_EDITOR
        if( string.IsNullOrEmpty( avURL ) == true ){
            CheckDownloadData( VersionType.Asset, avURL, 0, m_AssetData, m_AssetData.attached );
        }
        else{
            CheckDownloadData( VersionType.Asset, avURL, 0, m_AssetData );
        }
#else
      //  CheckDownloadData( VersionType.Asset, avURL, 0, m_AssetData );
        if( string.IsNullOrEmpty( avURL ) == true ){
            CheckDownloadData( VersionType.Asset, avURL, 0, m_AssetData, m_AssetData.attached );
        }
        else{
            CheckDownloadData( VersionType.Asset, avURL, 0, m_AssetData );
        }
#endif

        SetNextDownload( true );
    }

    public void SetCallback( Action<VersionType,float> callbackProcess )
    {
        m_eventProcessing = callbackProcess;
    }

    public int GetVersionFromUrl( string url )
    {
        if (string.IsNullOrEmpty(url))
            return 0;

        string _version     = ( string.IsNullOrEmpty( url ) == false ) ? Path.GetFileNameWithoutExtension( url ) : string.Empty;
        if( string.IsNullOrEmpty(_version) == true )
        {
            Debug.LogError("Not Exist Version data!" + _version + "  url: " + url);
            return 0;
        }

        int _result;
        if( int.TryParse(_version, out _result) == false )
        {
            Debug.LogError("Not Exist Version data!" + _version + "  url: " + url);
            return 0;
        }

        return _result;
    }

    void CheckDownloadData( VersionType type, string url, int revision, VersionData data, int target_version = 0 )
    {
        if( target_version == 0 ){
            target_version = GetVersionFromUrl( url );
        }

        if( target_version == 0 ){
            return;
        }

        data.revision = ( revision == 0 ) ? target_version : revision;
        data.download = target_version;

        if( type == VersionType.Resource )
        {
            if( data.attached == target_version ) {
                AddDownloadList( type, string.Format( "{0}{1}", AssetManager.GetStreamingAssetResourceFolder(), data.GetAttachedFileName() ) );
                UnityEngine.Debug.Log("data.GetAttachedFileName(): " + data.GetAttachedFileName());
            }
            else{
                AddDownloadList( type, url, target_version, data.extension );
            }
        }
        else
        {
            if( data.attached > 0 ){
                AddDownloadList( type, string.Format( "{0}{1}", AssetManager.GetStreamingAssetResourceFolder(), data.GetAttachedFileName() ) );
            }

            if( data.attached != target_version ){
                AddDownloadList( type, url, target_version, data.extension);
            }
        }
    }

    void AddDownloadList( VersionType type, string url )
    {
        DownloadData _new   = new DownloadData();
        _new.type           = type;
        _new.target_version = 0;
        _new.target_url     = url;
        m_listDownload.Add( _new );
    }

    void AddDownloadList( VersionType type, string url, int target_version, string ext )
    {
        DownloadData _new   = new DownloadData();
        _new.type           = type;
        _new.target_version = target_version;

#if UNITY_WEBGL 
        _new.target_url     = url;
#else
        string _file_path = string.Format( "{0}{1}{2}", BaseVersionManager.GetVersionFilePath(), target_version, ext );
        if( File.Exists( _file_path ) == false ){
            _new.target_url     = url;
        }
        else
        {
            Debug.Log("Load From Temporary");

    #if UNITY_EDITOR || UNITY_STANDALONE
            _new.target_url     = string.Format("file://{0}", _file_path );
    #else
            _new.target_url     = string.Format("file://{0}", _file_path );
    #endif
        }
#endif

        if( string.IsNullOrEmpty( _new.target_url ) == true )
        {
            Debug.LogError( "Not Exist URLs" );
            return;
        }

        m_listDownload.Add( _new );
    }

    void SetNextDownload( bool is_success )
    {
        if( m_activatedDownload != null ){
            return;
        }

        if( is_success == true && m_listDownload.Count > 0   )
        {
            m_activatedDownload = m_listDownload[0];
            m_listDownload.RemoveAt(0);

            if (m_activatedDownload.type == VersionType.Resource)
                SendWWW(m_activatedDownload.target_url, OnCallbackDownload, UpdateProcessing, true);
            else //Assets
                SendWWW(m_activatedDownload.target_url, OnCallbackDownload, UpdateProcessing);
        }
        else
        {
            m_eventProcessing.SafeInvoke(VersionType.None, 0.0f);
            m_eventEnd.SafeInvoke( is_success );
        }

    }

    protected virtual void SendWWW( string url, Action<UnityEngine.Networking.UnityWebRequest> callback, Action<float> progress, bool isSaveFile = false)
    {
        m_wwwEnum = AssetManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine( ProcessWWW( url, callback, progress ) );
    }

    protected virtual void CompleteDownload( VersionType type, int target_version, string file_name )
    {
        //DtrVersionManager에서 override
    }

    protected virtual bool DownloadError( string url, bool retry, Action<UnityEngine.Networking.UnityWebRequest> callback, Action<float> progress )
    {
        if (m_wwwEnum != null)
        {
            AssetManager.Instance.GetComponent<MonoBehaviour>().StopCoroutine(m_wwwEnum);
            m_wwwEnum = null;
        }

        if( retry == true )
        {
            SendWWW(url, callback, progress);
        }

        return retry;
    }

    IEnumerator ProcessWWW( string url, Action<UnityEngine.Networking.UnityWebRequest> callback, Action<float> progress )
    {
        using ( UnityEngine.Networking.UnityWebRequest www = new UnityEngine.Networking.UnityWebRequest( url))
        {
            while (www.isDone == false)
            {
                progress.SafeInvoke(www.downloadProgress );
                yield return null;
            }

            progress.SafeInvoke(1f);
            
            callback.SafeInvoke(www);
        }
    }


    void OnCallbackDownload( UnityEngine.Networking.UnityWebRequest webdata )
    {
   //   if( WWWManager.Instance.IsSuccess( data ) == true )
        if( webdata != null && string.IsNullOrEmpty( webdata.error ) == true && webdata.downloadHandler != null && webdata.downloadHandler.data != null )
        {
      //    WWWManager.Instance.eventProgress -= UpdateProcessing;

#if UNITY_WEBGL 
#else
            if( m_activatedDownload.target_version > 0 )
            {
                VersionData _target = ( VersionType.Asset == m_activatedDownload.type )  ? m_AssetData : m_RefsData;
                bool _re    = WriteData( webdata, string.Format( "{0}{1}", m_activatedDownload.target_version, _target.extension )  );
                if( _re == false ){
                    Debug.LogError( "OnCallbackLoad WriteData Fail" );
                }
            }
#endif
            switch( m_activatedDownload.type )
            {
                case VersionType.Asset:
                    {
                        bool _is_attached = true;
                        if( m_activatedDownload.target_version > 0 )
                        {
                            CompleteDownload(m_activatedDownload.type, m_activatedDownload.target_version, m_activatedDownload.target_version.ToString() );
                            _is_attached = false;
                        }

                        m_eventLoadAssetLists.SafeInvoke(_is_attached, webdata.downloadHandler.data );
                    }
                    break;

                case VersionType.Resource:
                    {
                        CompleteDownload(m_activatedDownload.type, m_RefsData.revision, m_activatedDownload.target_version.ToString() );
                        m_eventLoadTables.SafeInvoke( webdata.downloadHandler.data );
                    }
                    break;
            }

            m_activatedDownload = null;
            SetNextDownload( true );
        }
        else
        {
            Debug.LogError( "WWW Failed" );

            m_activatedDownload.try_count++;
            bool _retry = DownloadError(m_activatedDownload.target_url, m_activatedDownload.try_count < TRY_MAX_COUNT, OnCallbackDownload, UpdateProcessing );
            if( _retry == false )
            {
                m_activatedDownload = null;
                SetNextDownload( false );
            }
        }
    }

    void UpdateProcessing( float rate )
    {
        if( m_activatedDownload != null ){
            m_eventProcessing.SafeInvoke( m_activatedDownload.type, rate);
        }
    }

    public int GetResourceFileVersion( bool get_attached = false )
    {
        int _version;
        if( get_attached == false )
        {
            _version = m_RefsData.download;
            if( _version <= 0 ){
                _version = m_RefsData.attached;
            }
        }
        else {
            _version = m_RefsData.attached;
        }

        return _version;
    }

    public int GetAssetFileVersion( bool get_attached = false )
    {
        int _version;
        if( get_attached == false )
        {
            _version = m_AssetData.download;
            if (_version <= 0){
                _version = m_AssetData.attached;
            }
        }
        else{
            _version =  m_AssetData.attached;
        }

        return _version;
    }

#if UNITY_WEBGL 
#else
    bool WriteData( UnityEngine.Networking.UnityWebRequest data, string target_file_name )
    {
        string _file_path = string.Format( "{0}{1}", BaseVersionManager.GetVersionFilePath(), target_file_name );
        if( File.Exists( _file_path ) == false )
        {
            try
            {
                using( FileStream fs = new FileStream( _file_path, FileMode.Create ) )
                {
                    byte[] _data = data.downloadHandler.data;
                    fs.Write( _data, 0, _data.Length );
                }
            }
            catch( Exception e )
            {
                Debug.LogError( "Error File Write : " + e.Message );
                return false;
            }
        }

        return true;
    }
#endif

#if OLD
    public List<KeyValuePair<string,int>> GetConnectable( string[] data )
    {
        List<KeyValuePair<string,int>> _result = new List<KeyValuePair<string, int>>();
        if( data.Length > 0 )
        {
        //  var _curr = new Shared.BuildVersion(GetBuildVersion());

            ulong _max_version = ulong.MinValue;
            List< KeyValuePair<Shared.BuildVersion, string> > _list = new List<KeyValuePair<Shared.BuildVersion, string>>();
            for( int i = 0, _max = data.Length; i < _max; ++i )
            {
                string[] _value     = data[i].Split(":"[0]);
                var _target_version = new Shared.BuildVersion(_value[0]);
       //       if( _curr.Version <= _target_version.Version )
                {
                    _list.Add(new KeyValuePair<Shared.BuildVersion, string>(_target_version, data[i]) );

                    if( _target_version.Version > _max_version ){
                        _max_version = _target_version.Version;
                    }
                }
            }
           
            var _list_versions = _list.FindAll(r => r.Key.Version == _max_version);
            for( int i = 0, _max = _list_versions.Count; i < _max; ++i )
            {
                string[] _value     = _list_versions[i].Value.Split(":"[0]);
                _result.Add( new KeyValuePair<string,int>( _value[1], int.Parse( _value[2] )));
            }
        }

        return _result;
    }

    //
    public string GetFullVersion()
    {
        return string.Format( "{0}.{1}.{2}.{3}.{4}", GetBuildVersion(), GetResourceVersion(), GetAssetVersion(), LocalVersionChecker.GetLocalVersion(), (int)UrlData.GetServerType() );
    }

    public string GetBuildVersion()
    {
        return VersionValues.BuildVersion;
    }
#endif
}
