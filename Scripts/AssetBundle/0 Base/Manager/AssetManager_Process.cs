using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public delegate void delegateImageLoading( bool is_default, string name, Texture2D img );
public delegate void delegateGameObjectLoading( bool is_default, string name, GameObject obj );
public delegate void delegateSoundLoading( string name, AudioClip clip );
public delegate void delegateAniLoading( string name, AnimationClip clip );
#if NGUI
public delegate void delegateAtlasLoading( UIAtlas atlas );
#endif
public delegate void delegateTextLoading( string data );
public delegate void delegateSceneLoading( string data );
public delegate void delegateMaterialLoading(  bool is_default, string name, Material mat );

class AssetbundleData
{
    public AssetBundle bundle;
    public int version;
    public int saveFlag;

    public Dictionary< string, GameObject > dicScriptedObject;

    public T GetAsset<T>( string res_name, int save_flag, bool get_cloned ) where T: Object
    {
        if( bundle == null ){
            return default(T);
        }

        saveFlag |= save_flag;

        if( get_cloned == true && typeof(T) == typeof(GameObject) )
        {
            if( dicScriptedObject != null && dicScriptedObject.ContainsKey(res_name) == true )
            {
                var _result = dicScriptedObject[res_name];
                if( _result != default(T) ){
                    return _result as T;
                }
            }
            else
            { // set go to loading process
                return default(T);
            }
        }

        return bundle.LoadAsset<T>( res_name );
    }

    public void ReadyToParseScript()
    {
        if (dicScriptedObject != null){
            return;
        }

        dicScriptedObject = new Dictionary<string, GameObject>();
    }

    public Object CheckParsingScript( string res_name, Object origin_asset, Transform parent )
    {
        if( dicScriptedObject == null                    || 
            origin_asset.GetType() != typeof(GameObject) ){
            return origin_asset;
        }

        if( dicScriptedObject.ContainsKey(res_name) == true )
        {
            var _obj_parsed = dicScriptedObject[res_name];
            if( _obj_parsed == default(GameObject) ){
                return origin_asset;
            }
            else{
                return _obj_parsed;
            }
        }
        else
        {
            var _origin = origin_asset as GameObject;
            _origin.SetActive(false);

            var _new  = GameObject.Instantiate( _origin );
           
            bool _parsed = AssetIOUtil.CheckScriptData( null, _new);
            if( _parsed == true )
            {
                _new.name = origin_asset.name;
                _new.transform.parent = parent;
                dicScriptedObject.Add(res_name, _new);

                return _new;
            }
            else
            {
                dicScriptedObject.Add(res_name, default(GameObject) );
                GameObject.Destroy(_new);

                return origin_asset;
            }
        }
    }

    public void Clear()
    {
        if (bundle != null)
        {
            if( dicScriptedObject != null )
            {
                var _enum_script = dicScriptedObject.GetEnumerator();
                while( _enum_script.MoveNext() ){
                    GameObject.Destroy(_enum_script.Current.Value);
                }

                dicScriptedObject.Clear();
            }
            
            bundle.Unload(true);
        }
    }
}

class SourceAssetbundleData : AssetbundleData
{
    //  public int referenceCount;
}

public class DownloadData
{
    public string bundleName;
    public string bundlePath;
    public int version;
    public Hash128 hash;
    public int saveFlag;
    public System.Action<string,float> callbackProgress;
    public int referenceCount;
    public bool load_immediately;
    public bool parse_script;

    public void UpdateData( int save_flag, System.Action<string,float> progress )
    {
        saveFlag  |= save_flag;
        referenceCount++;
        if( progress != null ){
            callbackProgress += progress;
        }
    }

    public void ClearData( System.Action<string,float> progress )
    {
        referenceCount--;

        if( progress != null ){
            callbackProgress -= progress;
        }
    }   

#if UNITY_2017_1_OR_NEWER
    static public DownloadData GetEmpty( string asset_name, string full_path, int save_flag, int target_version, string hash, bool parse_script, bool load_first, System.Action<string,float> progress )
#else
    static public DownloadData GetEmpty( string asset_name, string full_path, int save_flag, int target_version, bool parse_script, bool load_first, System.Action<string,float> progress )
#endif
    {
        DownloadData _new = new DownloadData()
        {
            bundleName = asset_name,
            bundlePath = full_path,
            version = target_version,
            saveFlag = save_flag,
            referenceCount = 1,
            load_immediately = load_first,
        };

        _new.parse_script = parse_script;

#if UNITY_2017_1_OR_NEWER
        if( string.IsNullOrEmpty( hash ) == false ){
            _new.hash = Hash128.Parse(hash);
        }
#endif

        if (progress != null)
        {
            _new.callbackProgress += progress;
        }

        return _new;
    }
}

class ProcessData
{
    public bool useAsyncLoad;
    public string bundleName;
    public string sourceBundleName;
    public AssetBundle assetBundle;

    public List<KeyValuePair<string, System.Type>>  listResNames = new List<KeyValuePair<string, System.Type>>();

    //
    public class CallbackData<T> 
    {
        public T callback;
        public string res_name;
        public string file_name;
    }

    public List< CallbackData<delegateImageLoading>> listCallbackImages = null;
    public List< CallbackData<delegateGameObjectLoading>> listCallbackGameObjects = null;
    public List< CallbackData<delegateSoundLoading>> listCallbackSounds = null;
    public List< CallbackData<delegateAniLoading>> listCallbackAnis = null;
    public List< CallbackData<delegateSceneLoading>> listCallbackScene  = null;
#if NGUI
    public List< CallbackData<delegateAtlasLoading>> listCallbackAtlas = null;
#endif
    public List< CallbackData<delegateTextLoading>> listCallbackText = null;
    public List< CallbackData<delegateMaterialLoading>> listCallbackMaterial = null;

    //
    static public ProcessData GetEmpty( string asset_name, string source_asset_name, bool use_async_load )
    {
        ProcessData _new = new ProcessData()
        {
            bundleName = asset_name,
            sourceBundleName = source_asset_name,
            useAsyncLoad = use_async_load,
        };

        return _new;
    }

    //
    public void SetData( AssetBundle bundle )
    {
        if (assetBundle != null)
        {
            Debug.LogError("duplicated load call!!!");
        }

        assetBundle = bundle;
    }

    public void ClearBundle()
    {
        if (assetBundle != null)
        {
            Debug.Log("AssetBundle Unloaded ==> " + bundleName);
            assetBundle.Unload(false);
        }

        assetBundle = null;
    }

    public void Execute( string res_name, Object asset )
    {
        if( listCallbackAnis != null )
        {   // cloned object return
            var _enum = listCallbackAnis.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    string _file_name = _enum.Current.file_name;
                    if( asset != null ){
                        _enum.Current.callback( _file_name, asset as AnimationClip );  
                    }
                    else{   
                        _enum.Current.callback( _file_name, null );  
                    }

                    listCallbackAnis.Remove( _enum.Current );
                    _enum = listCallbackAnis.GetEnumerator();
                }
            }
        }

        if( listCallbackGameObjects != null )
        {   // cloned object return
            var _origin = asset as GameObject;

            var _enum = listCallbackGameObjects.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    string _file_name = _enum.Current.file_name;

                    if( _origin != null ){
                        _enum.Current.callback( false, _file_name, _origin );    
                    }
                    else{
                        _enum.Current.callback( false, _file_name, null );
                    }

                    listCallbackGameObjects.Remove( _enum.Current );
                    _enum = listCallbackGameObjects.GetEnumerator();
                }
            }
        }

        if( listCallbackImages != null )
        { // original object return
            var _enum = listCallbackImages.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    _enum.Current.callback( false, _enum.Current.file_name, asset as Texture2D ); 

                    listCallbackImages.Remove( _enum.Current );
                    _enum = listCallbackImages.GetEnumerator();
                }
            }
        }

        if( listCallbackSounds != null )
        { // cloned object return
            var _enum = listCallbackSounds.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    string _file_name = _enum.Current.file_name;

                    if( asset != null ){
                        _enum.Current.callback( _file_name, asset as AudioClip );  
                    }
                    else{
                        _enum.Current.callback( _file_name, null );
                    }

                    listCallbackSounds.Remove( _enum.Current );
                    _enum = listCallbackSounds.GetEnumerator();
                }
            }
        }

        if( listCallbackScene != null )
        { // original object return
            var _enum = listCallbackScene.GetEnumerator();
            while( _enum.MoveNext() )
            {
             //    string _res = _enum.Current.Value;
                _enum.Current.callback(_enum.Current.file_name);    

                listCallbackScene.Remove( _enum.Current );
                _enum = listCallbackScene.GetEnumerator();
            }
        }
#if NGUI
        if( listCallbackAtlas != null )
        { // original object return
            var _origin = asset as GameObject;
            var _enum = listCallbackAtlas.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    if( _origin != null ){
                        _enum.Current.callback( _origin.GetComponent< UIAtlas >() ); 
                    }
                    else{
                        Debug.LogError( "Null Atlas >> " + _enum.Current.file_name );
                    }

                    listCallbackAtlas.Remove( _enum.Current );
                    _enum = listCallbackAtlas.GetEnumerator();
                }
            }
        }
#endif
        if( listCallbackText != null )
        { // original object return
            var _data = asset as TextAsset;

            var _enum = listCallbackText.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    if (_data != null){
                        _enum.Current.callback(_data.text);
                    }
                    else{
                        _enum.Current.callback(null);
                    }

                    listCallbackText.Remove( _enum.Current );
                    _enum = listCallbackText.GetEnumerator();
                }
            }
        }

        if( listCallbackMaterial != null )
        {   // cloned object return
            var _origin = asset as Material;

            var _enum = listCallbackMaterial.GetEnumerator();
            while( _enum.MoveNext() )
            {
                string _res = _enum.Current.res_name;
                if( res_name == _res )
                {
                    _enum.Current.callback( false, _enum.Current.file_name, _origin );    

                    listCallbackMaterial.Remove( _enum.Current );
                    _enum = listCallbackMaterial.GetEnumerator();
                }
            }
        }
    }

    public void AddCallback< T >( T callback, string res_name, string file_name  )
    {
        AddResName<T>( callback, res_name );

        if( EqualityComparer<T>.Default.Equals( default(T), callback ) == false )
        {
            List<CallbackData<T>> _target = GetCallbackList<T>( true );
            if( _target != default( List<CallbackData<T>>) )
            {
                if( _target.Exists(r => (r.callback.Equals(callback) && r.res_name == res_name)) )
                {
                    Debug.LogError("Exist Same File & Callback : " + res_name);
                }
                else
                {
                    var _new = new CallbackData<T>();
                    _new.callback = callback;
                    _new.file_name = file_name;
                    _new.res_name = res_name;

                    _target.Add(_new);
                }
            }
        }
    }

    public void CancelCallback< T >( T callback, string res_name )
    {
        List<CallbackData<T>> _target = GetCallbackList<T>( false );
        if( _target == default( List<CallbackData<T>>) ){
            return;
        }

        var _enum = _target.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _curr = _enum.Current;
            if ( _curr.callback.Equals( callback ) && _curr.res_name == res_name )
            {
                _target.Remove(_curr);
                _enum = _target.GetEnumerator();
            }
        }
    }

    public bool IsExistDelegate()
    {
        if( listCallbackImages != null && listCallbackImages.Count > 0 ){
            return true;
        }

        if( listCallbackGameObjects != null && listCallbackGameObjects.Count > 0 ){
            return true;
        }

        if( listCallbackSounds != null && listCallbackSounds.Count > 0 ){
            return true;
        }

        if( listCallbackAnis != null && listCallbackAnis.Count > 0 ){
            return true;
        }

        if( listCallbackScene != null && listCallbackScene.Count > 0 ){
            return true;
        }

#if NGUI
        if( listCallbackAtlas != null && listCallbackAtlas.Count > 0 ){
            return true;
        }
#endif

        if( listCallbackText != null && listCallbackText.Count > 0 ){
            return true;
        }

        if( listCallbackMaterial != null && listCallbackMaterial.Count > 0 ){
            return true;
        }

        return false;
    }

    void AddResName<T>( T callback, string file_name )
    {
        System.Type _target_type = GetResType<T>( callback );
        if( _target_type != default( System.Type) )
        {
            for( int i=0, _max = listResNames.Count; i < _max; ++i )
            {
                if( listResNames[i].Key == file_name      &&
                    listResNames[i].Value == _target_type  ){
                    return;
                }
            }
        }

        listResNames.Add( new KeyValuePair<string, System.Type>( file_name, _target_type ) );
    }

    System.Type GetResType<T>( T callback )
    {
        System.Type _type = typeof(T);
        if (_type == typeof(delegateImageLoading) ){
            return typeof(Texture2D);
        }
        else if (_type == typeof(delegateGameObjectLoading) ){
            return typeof(GameObject);
        }
        else if (_type == typeof(delegateSoundLoading) ){
            return typeof(AudioClip);
        }
        else if (_type == typeof(delegateAniLoading) ){
            return typeof(AnimationClip);
        }
#if NGUI
        else if (_type == typeof(delegateAtlasLoading) ){
            return typeof(UIAtlas);
        }
#endif
        else if (_type == typeof(delegateTextLoading) ){
            return typeof(TextAsset);
        }
        else if (_type == typeof(delegateMaterialLoading) ){
            return typeof(Material);
        }

        return default( System.Type );
    }

    List<CallbackData<T>> GetCallbackList<T>( bool is_create ) 
    {
        System.Type _type = typeof(T);

        if (_type == typeof(delegateImageLoading))
        {
            if (is_create == true)
            {
                if (listCallbackImages == null)
                {
                    listCallbackImages = new List<CallbackData<delegateImageLoading>>();
                }
            }
            return listCallbackImages as List<CallbackData<T>>;
        }
        else if (_type == typeof(delegateGameObjectLoading))
        {
            if (is_create == true)
            {
                if (listCallbackGameObjects == null)
                {
                    listCallbackGameObjects = new List<CallbackData<delegateGameObjectLoading>>();
                }
            }
            return listCallbackGameObjects as List<CallbackData<T>>;
        }
        else if (_type == typeof(delegateSoundLoading))
        {
            if (is_create == true)
            {
                if (listCallbackSounds == null)
                {
                    listCallbackSounds = new List<CallbackData<delegateSoundLoading>>();
                }
            }
            return listCallbackSounds as List<CallbackData<T>>;
        }
        else if (_type == typeof(delegateAniLoading))
        {
            if (is_create == true)
            {
                if (listCallbackAnis == null)
                {
                    listCallbackAnis = new List<CallbackData<delegateAniLoading>>();
                }
            }
            return listCallbackAnis as List<CallbackData<T>>;
        }
        else if (_type == typeof(delegateSceneLoading))
        {
            if (is_create == true)
            {
                if (listCallbackScene == null)
                {
                    listCallbackScene = new List<CallbackData<delegateSceneLoading>>();
                }
            }
            return listCallbackScene as List<CallbackData<T>>;
        }
#if NGUI
        else if (_type == typeof(delegateAtlasLoading))
        {
            if (is_create == true)
            {
                if (listCallbackAtlas == null)
                {
                    listCallbackAtlas = new List<CallbackData<delegateAtlasLoading>>();
                }
            }
            return listCallbackAtlas as List<CallbackData<T>>;
        }
#endif
        else if (_type == typeof(delegateTextLoading))
        {
            if (is_create == true)
            {
                if (listCallbackText == null)
                {
                    listCallbackText = new List<CallbackData<delegateTextLoading>>();
                }
            }
            return listCallbackText as List<CallbackData<T>>;
        }
        else if (_type == typeof(delegateMaterialLoading))
        {
            if (is_create == true)
            {
                if (listCallbackMaterial == null)
                {
                    listCallbackMaterial = new List<CallbackData<delegateMaterialLoading>>();
                }
            }
            return listCallbackMaterial as List<CallbackData<T>>;
        }

        return default( List<CallbackData<T>>);
    }
}

public partial class AssetManager  
{
    Dictionary<string, AssetbundleData>         m_dicLoadedAssetBundles  = new Dictionary<string, AssetbundleData>();
    Dictionary< string,SourceAssetbundleData>   m_dicSourceAssetBundles  = new Dictionary<string, SourceAssetbundleData >();

    List<string>                         m_listSkipAssets = new List<string>();

    //
    List<DownloadData>                   m_listBundlesToDownload      = new List<DownloadData>();
    List<DownloadData>                   m_listBundlesToDownloadFirst = new List<DownloadData>();
    List<DownloadData>                   m_listBundlesToDownloadLate  = new List<DownloadData>();
    DownloadData                         m_activatedDownloadData      = null;
    bool                                 m_bPauseLateQueue = false;

    Dictionary< string, ProcessData>     m_dicCallbackData        = new Dictionary<string, ProcessData>();
    List< ProcessData >                  m_listBundlesToLoad      = new List<ProcessData>();
    List< ProcessData >                  m_listBundlesToLoadFirst = new List<ProcessData>();
    ProcessData                          m_activatedLoadData      = null;

    Dictionary< string, AssetVersionData>  m_dicAssetData         = null;
    Dictionary< string, AssetVersionData > m_dicAttachedAssetData = null;

    Dictionary<string, string>           m_dicLookupTable         = new Dictionary<string, string>();
    List<string>                         m_listPreloadAssets = new List<string>();
 
    //
    protected enum DefaultDataType
    {
        None = 0,
        Texture,
        GameObject,
    }

    Dictionary< string, Texture2D >      m_dicDefaultDataTex        = new Dictionary<string, Texture2D>();
    Dictionary< string, GameObject >     m_dicDefaultDataGameObject = new Dictionary<string, GameObject>();

    delegate KeyValuePair<DefaultDataType, string> delegateGetDefaultName( AssetDefines.AssetObjectEnum type);
    delegateGetDefaultName  m_callbackGetDefaultName   = null;
  
    //
    void InitDownloadEnvs()
    {
        Caching.compressionEnabled        = true;

#if UNITY_EDITOR
        Caching.ClearCache();
#endif

#if UNITY_2018_2_OR_NEWER
        var _cache = Caching.currentCacheForWriting;
        _cache.maximumAvailableStorageSpace = 2048L * 1024L * 1024L;
#else
    #if UNITY_WEBGL
        Caching.maximumAvailableDiskSpace = 256 * 1024 * 1024;
    #else
        Caching.maximumAvailableDiskSpace = 1024 * 1024 * 1024;
    #endif
#endif
        UnityEngine.LightmapSettings.lightmapsMode  = LightmapsMode.NonDirectional;
    }

    void InitDefaultAssets()
    {
        System.Reflection.MethodInfo[] _infos = GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        for (int i = 0, _max = _infos.Length; i < _max; ++i)
        {
            object[] _att = _infos[i].GetCustomAttributes(false);
            for (int j = 0, _maxj = _att.Length; j < _maxj; ++j)
            {
                var _curr_att = _att[j] as AssetDefines.ChildAttribute;
                if (_curr_att != null)
                {
                    m_callbackGetDefaultName = System.Delegate.CreateDelegate(typeof(delegateGetDefaultName), this, _infos[i]) as delegateGetDefaultName;
                }
             // else
             // {
             //     var _func = _att[j] as AssetDefines.FuncLoadDefaultsAttribute;
             //     if (_func != null){
             //         _infos[i].Invoke(this, null);
             //     }
             // }
            }
        }

        //
        AssetDefines.AssetObjectEnum[] _enums = AssetDefines.AssetObjectEnum.GetEnums();
        for (int i = 0, _max = _enums.Length; i < _max; ++i)
        {
            if( _enums[i] != AssetDefines.AssetObjectEnum.None )
            {
                LoadDefault(_enums[i]);
            }
        }
    }

   // void LoadDefaults( System.Type type )
   // {
   //     AssetDefines.AssetObjectEnum _enum = null;
   //     int max = AssetDefines.AssetObjectEnum.Max;
   //     for( int i = 0; i < max; ++i )
   //     {
   //         _enum = AssetDefines.AssetObjectEnum.GetEnum( i );
   //         if( _enum != AssetDefines.AssetObjectEnum.None )
   //             LoadDefault( _enum );
   //     }
   //
   //     //System.Reflection.FieldInfo[] _fieldinfos = type.GetFields(
   //     //    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy );
   //     
   //     //for (int i = 0, _max = _fieldinfos.Length; i < _max; ++i)
   //     //{
   //     //    var _enum =_fieldinfos[i].GetValue(_fieldinfos[i]) as AssetDefines.AssetObjectEnum;
   //     //    if (_enum != null && _enum.Value >= 0)
   //     //    {
   //     //        LoadDefault( _enum );
   //     //    }
   //     //}
   // }

    void LoadDefault( AssetDefines.AssetObjectEnum type )
    {
        var _re = GetDefaultObjectName( type );
        if( _re.Equals( default(KeyValuePair<DefaultDataType, string>))){
           return;
        }

        switch( _re.Key )
        {
        case DefaultDataType.GameObject:
            {
                if( m_dicDefaultDataGameObject.ContainsKey(_re.Value) == true ){
                    break;
                }

                GameObject _data = Resources.Load<GameObject>( _re.Value );
                if( _data == null )
                    break;

                m_dicDefaultDataGameObject.Add( _re.Value, _data );
            }
            break;
        case DefaultDataType.Texture:
            {
                if( m_dicDefaultDataTex.ContainsKey(_re.Value) == true ){
                   break;
                }

                Texture2D _data = Resources.Load<Texture2D>( _re.Value );
                if( _data == null )
                   break;

                m_dicDefaultDataTex.Add( _re.Value, _data );
            }
            break;
        }
    }

    public Texture2D GetDefaultImage( AssetDefines.AssetObjectEnum type )
    {
        var _re = GetDefaultObjectName( type );
        if( default(KeyValuePair<DefaultDataType, string>).Equals( _re )){
            return null;
        }

        if( m_dicDefaultDataTex.ContainsKey( _re.Value ) == false ){
            return null;
        }

        return  m_dicDefaultDataTex[ _re.Value  ] ;
    }

    public GameObject GetDefaultObject( AssetDefines.AssetObjectEnum type )
    {
        var _re = GetDefaultObjectName( type );
        if( default(KeyValuePair<DefaultDataType, string>).Equals( _re ) ){
            return null;
        }

        if( m_dicDefaultDataGameObject.ContainsKey( _re.Value ) == false ){
            return null;
        }

//      GameObject _re = GameObject.Instantiate( m_dicDefaultDataGameObject[ _name ] );
        return m_dicDefaultDataGameObject[ _re.Value ];
    }

    void ClearDefaultData()
    {
        m_dicDefaultDataGameObject.Clear();
        m_dicDefaultDataTex.Clear();
    }

    public void LoadAssetList( bool is_attached, byte[] data )
    {
        if( is_attached == true )
        {
            m_dicAttachedAssetData = AssetVersions.Load( data );
        }
        else
        {
            m_dicAssetData = AssetVersions.Load( data );
        }
    }

    Dictionary<string, AssetVersionData> GetActivatedAssetData()
    {
        return (m_dicAssetData != null) ? m_dicAssetData : m_dicAttachedAssetData;
    }

    //#if UNITY_EDITOR
    //    public void LoadAssetList( string filePath )
    //    {
    //        using( FileStream fs = new FileStream( filePath, FileMode.Open ) )
    //        {
    //            byte[] _new = new byte[ fs.Length ];
    //
    //            fs.Read(_new, 0, (int)fs.Length);
    //            LoadAssetList(true, _new);
    //        }
    //    }
    //#endif

    bool IsLoadFromAttached( string bundle_name )
    {
        if( m_listSkipAssets.Contains(bundle_name) == true ){
            return false;
        }

        int _target_version = GetAssetVersion (bundle_name);
        if( m_dicAttachedAssetData != null && m_dicAttachedAssetData.ContainsKey (bundle_name) == true) 
        {
            if (m_dicAttachedAssetData[bundle_name].version == _target_version) {
                return true;
            }
        }

        return false;
    }

    public bool IsLoadFromLocal( string asset_name )
    {
        if( string.IsNullOrEmpty(asset_name) == true )
            return false;

        var _bundle_name = AssetManager.GetBundleFileName(asset_name);
        if( IsPreCached(_bundle_name) == true )
            return true;

        return IsLoadFromAttached(_bundle_name);
    }

   //int GetAttachedAssetVersion( string bundle_name )
   //{
   //    if( m_listSkipAssets.Contains(bundle_name) == true ){
   //        return -1;
   //    }
   //
   //    if( m_dicAttachedAssetData != null                            &&
   //        m_dicAttachedAssetData.ContainsKey( bundle_name ) == true ){
   //        return m_dicAttachedAssetData[bundle_name].version;
   //    }
   //
   //    return -1;
   //}

    int GetAssetVersion( string bundle_name )
    {
        int _version = -1;
        var _targetAssetData = GetActivatedAssetData();
        if( _targetAssetData != null ) 
        {
            if( _targetAssetData.ContainsKey (bundle_name) == true ){
                _version = _targetAssetData[ bundle_name ].version;
            }
        }
    
        return _version;
    }

#if UNITY_2017_1_OR_NEWER
    string GetAssetHash( string bundle_name )
    {
        string _re = string.Empty;
        var _targetAssetData = GetActivatedAssetData();
        if( _targetAssetData != null ) 
        {
            if( _targetAssetData.ContainsKey (bundle_name) == true ){
                _re = _targetAssetData[ bundle_name ].hash;
            }
        }

        return _re;
    }
#endif

    AssetVersionData GetVersionData( string bundle_name )
    {
        int _version = GetAssetVersion( bundle_name );
        if( _version < 0 )
        {
            Debug.LogError( "Error Get Asset Revision : " + bundle_name );
            return null;
        }
    
        var _targetAssetData = GetActivatedAssetData();
        if( _targetAssetData != null ) 
        {
            if (_targetAssetData.ContainsKey (bundle_name) == true) {
                return _targetAssetData[ bundle_name ];
            }
        }
    
        return null;
    }

    //
    void LoadSkipAssetsList()
    {
        m_listSkipAssets.Clear();

        var _asset = Resources.Load( SKIPLIST_FILENAME ) as TextAsset;
        if( _asset != null )
        {
            string   _result_data = _asset.text.Replace("\r", "");
            string[] _list        = _result_data.Split("\n"[0]);
            for( int i = 0, _max = _list.Length; i < _max; ++i )
            {
                if( string.IsNullOrEmpty(_list[i]) == true ){
                    continue;
                }

                m_listSkipAssets.Add(_list[i]);
            }
        }
    }

    //
    public void SetPauseCaching( bool set_pause )
    {
        m_bPauseLateQueue = set_pause;
        if(set_pause == false )
        {
            if(m_listBundlesToDownloadLate.Count > 0 )
            {
                if( m_activatedDownloadData == null )
                {
                    StartCoroutine("GetNextDownload");
                }
            }
        }
    }

    public void ClearIngameAssets()
    {
        var _enum = m_dicLoadedAssetBundles.GetEnumerator();
        while(_enum.MoveNext() )
        {
            AssetbundleData _data = _enum.Current.Value;
            _data.saveFlag = DeleteFlag(_data.saveFlag, SaveFlag.Ingame);
            if( _data.saveFlag == (int)SaveFlag.None )
            {
                _data.Clear();

                m_dicLoadedAssetBundles.Remove(_enum.Current.Key );
                _enum = m_dicLoadedAssetBundles.GetEnumerator();
            }
        }   

        var _enum_asset = m_dicSourceAssetBundles.GetEnumerator();
        while( _enum_asset.MoveNext() ){
            _enum_asset.Current.Value.Clear();
        }
        m_dicSourceAssetBundles.Clear();

        SetPauseCaching(false);
    }

    public List<string> GetBundleListsToPass(List<string> list_assets)
    {
        if (list_assets.Count == 0 && m_listPreloadAssets.Count == 0 ){
            return null;
        }

        var _result = new List<string>();
        for (int i = 0, _max = list_assets.Count; i < _max; ++i)
        {
            var _file_name = AssetManager.GetBundleFileName(list_assets[i]);
            if (_result.Contains(_file_name) == true)
                continue;

            _result.Add(_file_name);
        }

        for( int i=0, _max = m_listPreloadAssets.Count; i < _max; ++i )
        {
            if (_result.Contains(m_listPreloadAssets[i]) == true)
                continue;

            _result.Add(m_listPreloadAssets[i]);
        }

        return _result;
    }

    public void ClearAssets(List<string> list_using_bundles)
    {
       // List<string> _list_bundles = GetBundleNameLists(list_excepts);
       // if (_list_bundles == null)
       //     return;

        var _enum = m_dicLoadedAssetBundles.GetEnumerator();
        while (_enum.MoveNext())
        {
            string _bundle_name = _enum.Current.Key;
            if (list_using_bundles.Contains(_bundle_name) == true)
                continue;

            AssetbundleData _data = _enum.Current.Value;
            _data.Clear();

#if UNITY_EDITOR
            Debug.LogWarning("Remove Asset From AssetManager >> " + _bundle_name);
#endif

            m_dicLoadedAssetBundles.Remove(_bundle_name);
            _enum = m_dicLoadedAssetBundles.GetEnumerator();
        }

        m_listBundlesToDownload.RemoveAll( r=> list_using_bundles.Contains( r.bundleName ) == false );
        m_listBundlesToDownloadFirst.RemoveAll( r => list_using_bundles.Contains(r.bundleName) == false );
        //  m_listBundlesToDownloadLate.RemoveAll( r => _list_bundles.Contains(r.bundleName) == false );    // no need..

        var _enum1 = m_dicCallbackData.GetEnumerator();
        while (_enum1.MoveNext())
        {
            var _key = _enum1.Current.Key;
            if (list_using_bundles.Contains(_key) == true)
                continue;

            _enum1.Current.Value.ClearBundle();

            m_dicCallbackData.Remove(_key);
            _enum1 = m_dicCallbackData.GetEnumerator();
        }

        var _enum2 = m_listBundlesToLoad.GetEnumerator();
        while (_enum1.MoveNext())
        {
            var _curr = _enum2.Current;
            if (list_using_bundles.Contains(_curr.bundleName) == true)
                continue;

            _curr.ClearBundle();

            m_listBundlesToLoad.Remove(_curr);
            _enum2 = m_listBundlesToLoad.GetEnumerator();
        }

        var _enum3 = m_listBundlesToLoadFirst.GetEnumerator();
        while (_enum1.MoveNext())
        {
            var _curr = _enum3.Current;
            if (list_using_bundles.Contains(_curr.bundleName) == true)
                continue;

            _curr.ClearBundle();

            m_listBundlesToLoad.Remove(_curr);
            _enum3 = m_listBundlesToLoadFirst.GetEnumerator();
        }
    }

    void ClearAssets()
    {
        StopCoroutine( "DownloadAssetbundle" );

        m_listBundlesToDownload.Clear();
        m_listBundlesToDownloadFirst.Clear();
        m_listBundlesToDownloadLate.Clear();
        m_activatedDownloadData  = null;
        SetPauseCaching(false);

        StopCoroutine("LoadAssets");
        StopCoroutine( "LoadAssetbundle" );
        StopCoroutine( "GetNextDownload" );

        var _enum1 = m_dicCallbackData.GetEnumerator();
        while(_enum1.MoveNext() )
        {
            var _re = m_listBundlesToLoad.Find(r => r == _enum1.Current.Value);
            if (_re != null){
                _re.ClearBundle();
            }

            _re = m_listBundlesToLoadFirst.Find(r => r == _enum1.Current.Value);
            if (_re != null){
                _re.ClearBundle();
            }
        }
        m_dicCallbackData.Clear();
        m_listBundlesToLoad.Clear();
        m_listBundlesToLoadFirst.Clear();
       
        if( m_activatedLoadData != null )
        {
            m_activatedLoadData.ClearBundle();
            m_activatedLoadData = null;
        }

        var _enum = m_dicLoadedAssetBundles.GetEnumerator();
        while(_enum.MoveNext() ){
         //  _enum.Current.Value.Clear(false);
            _enum.Current.Value.Clear();
        }
        m_dicLoadedAssetBundles.Clear();

        m_listPreloadAssets.Clear();
    }

    KeyValuePair<DefaultDataType,string> GetDefaultObjectName( AssetDefines.AssetObjectEnum type )
    {
        if (m_callbackGetDefaultName == null)
        {
            return default( KeyValuePair<DefaultDataType,string>);
        }

        return m_callbackGetDefaultName(type);
    }

    void AddToDownload<T>( T callback, string file_name, int save_type, bool use_async_load, bool load_first, bool parse_attached_script, System.Action<string,float> callback_progress )
    {
        string _bundle_name, _res_name, _base_bundle_name;
        bool _re = GetBundleNames( file_name, out _bundle_name, out _res_name, out _base_bundle_name );
        if( _re == false )
        {
            Debug.LogError( "Not Exact Bundle Format" + file_name );
            return;
        }

        if( EqualityComparer<T>.Default.Equals( default(T), callback) == false )
        {
            if( m_dicCallbackData.ContainsKey( _bundle_name ) == false ){
                m_dicCallbackData.Add( _bundle_name, ProcessData.GetEmpty( _bundle_name, _base_bundle_name, use_async_load ) );
            }

            m_dicCallbackData[ _bundle_name ].AddCallback<T>( callback, _res_name, file_name );
        }

        //
        _re = ProcessDownLoad( _bundle_name, _base_bundle_name, save_type, load_first, parse_attached_script, callback_progress );
        if( _re == false )
        {
            Debug.LogWarning("AssetBundle download failed : " + file_name);
            
            if (m_dicCallbackData.ContainsKey(_bundle_name) == true )
            {
                ProcessData _process = m_dicCallbackData[_bundle_name];
                if( _process != null )
                {
                    var _res_data = _process.listResNames.Find(r => _res_name == r.Key );
                    if (default(KeyValuePair<string, System.Type>).Equals(_res_data) == false){
                        _process.Execute(_res_data.Key, null);                               
                    }
                    else{
                        _process.Execute(_res_name, null);
                    }

                    if( _process.IsExistDelegate() == false ){
                        m_dicCallbackData.Remove(_bundle_name);
                    }
                }
            }
        }
    }

    bool ProcessDownLoad( string bundle_name, string base_bundle_name, int save_type, bool load_first, bool parse_attached_script, System.Action<string,float> callback_progress )
    {
        if( string.IsNullOrEmpty( bundle_name ) == true ){
            return false;
        }

        if( string.IsNullOrEmpty( base_bundle_name ) == false )
        {
           int _version = GetAssetVersion( base_bundle_name );
           if( m_dicSourceAssetBundles.ContainsKey( base_bundle_name ) == true )
           {
               if( _version != m_dicSourceAssetBundles[ base_bundle_name ].version )
               {
                   m_dicSourceAssetBundles[ base_bundle_name ].Clear();
                   m_dicSourceAssetBundles.Remove( base_bundle_name );
               }
           }
           
           bool _re = AddDownloadList( base_bundle_name, save_type, load_first, false, callback_progress );
           if( _re == true )
           {
               if( m_dicSourceAssetBundles.ContainsKey( base_bundle_name ) == true )
               {
                   Debug.LogError( "???" );    
               }
               else
               {
                   m_dicSourceAssetBundles.Add( base_bundle_name, new SourceAssetbundleData(){
                   //  referenceCount = 0,
                       version = _version,
                       bundle = null,
                   });
               }
           }
        }

        // check if preloaded assetbundle..
        if( m_dicLoadedAssetBundles.ContainsKey( bundle_name ) == true )
        {
            int _version = GetAssetVersion( bundle_name );
            if( _version == m_dicLoadedAssetBundles[ bundle_name ].version )
            {
                if( parse_attached_script == true ){
                    m_dicLoadedAssetBundles[bundle_name].ReadyToParseScript(); 
                }

                AddLoadData( m_dicLoadedAssetBundles[ bundle_name ].bundle, bundle_name, save_type, load_first );
                return true;
            }
            else{
                m_dicLoadedAssetBundles.Remove( bundle_name );
            }
        }

        //
        bool _success = AddDownloadList( bundle_name, save_type, load_first, parse_attached_script, callback_progress );
        if (_success == true)
        {
            StopCoroutine("GetNextDownload");
            StartCoroutine("GetNextDownload");
        }

        return _success;
    }

    bool AddDownloadList( string bundle_name, int save_type, bool load_first, bool parse_attached_script, System.Action<string,float> callback_progress )
    {
        // check exist in download list 
        DownloadData _target_data = m_listBundlesToDownloadLate.Find(r => r.bundleName == bundle_name);
        if (_target_data != null)
        {
            _target_data.UpdateData(save_type, callback_progress);

            if (load_first == true)
            {
                m_listBundlesToDownloadLate.Remove(_target_data);
                m_listBundlesToDownloadFirst.Add(_target_data);

                _target_data.load_immediately = true;
            }
            else if(_target_data.saveFlag != (int)SaveFlag.Caching)
            {
                m_listBundlesToDownloadLate.Remove(_target_data);
                m_listBundlesToDownload.Add(_target_data);
            }

            return true;
        }

        _target_data = m_listBundlesToDownload.Find( r => r.bundleName == bundle_name );
        if( _target_data != null )
        {
            _target_data.UpdateData(save_type, callback_progress);

            if( load_first == true )
            {
                m_listBundlesToDownload.Remove(_target_data);
                m_listBundlesToDownloadFirst.Add(_target_data);

                _target_data.load_immediately = true;
            }
    
            return true;
        }

        _target_data = m_listBundlesToDownloadFirst.Find(r => r.bundleName == bundle_name);
        if( _target_data != null )
        {
            _target_data.UpdateData(save_type, callback_progress);

            if( load_first == false )
            {
                Debug.LogError("Really Need This ?? ");
                // m_listBundlesToDownloadFirst.Remove(_target_data);
                // m_listBundlesToDownload.Add(_target_data);
                // _target_data.load_immediately = false;
            }

            return true;
        }

        if( m_activatedDownloadData != null                    && 
            m_activatedDownloadData.bundleName == bundle_name  )
        {
            m_activatedDownloadData.UpdateData(save_type, callback_progress);
            return true;
        }

        // check already downloaded...
        if( m_activatedLoadData != null && m_activatedLoadData.bundleName == bundle_name)
        {
            Debug.Log("Exist in m_activatedLoadData " + bundle_name);
            return true;
        }

        if( m_listBundlesToLoad.Exists(r => r.bundleName == bundle_name) == true        || 
            m_listBundlesToLoadFirst.Exists( r => r.bundleName == bundle_name ) == true )
        {
            Debug.Log("Exist in loaded list!");
            return true;
        }

        //
        string _full_path = GetBundlePath(  bundle_name );
        if( string.IsNullOrEmpty( _full_path ) == true ){
            return false;
        }

        int _version = GetAssetVersion( bundle_name );
        if( _version < 0 )
        {
            Debug.LogError( "Not Exist Revision ! : " + bundle_name );
            return false;
        }

#if UNITY_2017_1_OR_NEWER
        string _hash = GetAssetHash(bundle_name);
        _target_data = DownloadData.GetEmpty( bundle_name, _full_path, save_type, _version, _hash, parse_attached_script, load_first, callback_progress );
#else
        _target_data = DownloadData.GetEmpty( bundle_name, _full_path, save_type, _version, parse_attached_script, load_first, callback_progress );
#endif
        if( load_first == true ){
            m_listBundlesToDownloadFirst.Add( _target_data );
        }
        else
        {
            if (save_type == (int)SaveFlag.Caching){
                m_listBundlesToDownloadLate.Add(_target_data);
            }
            else{
                m_listBundlesToDownload.Add(_target_data);
            }
        }
       
        return true;
    }

    string GetBundlePath( string bundle_name )
    {
        var _asset_data = GetVersionData( bundle_name );
        if( _asset_data == null )
        {
            Debug.LogError( "Not Exist Asset in AssetList >> " + bundle_name );
            return string.Empty;
        }

        string _result_path;
        if( IsLoadFromAttached( bundle_name ) == true ){
            _result_path = string.Format( "{0}{1}", GetStreamingAssetResourceFolder(), _asset_data.filename );
        }
        else
        {
            _result_path = string.Format( "{0}/{1}/{2}", m_assetBaseUrl, _asset_data.version, _asset_data.filename );
        }

        return _result_path;
    }

    public static bool GetBundleNames( string name, out string bundle_name, out string res_name, out string base_bundle_name )
    {
        if( name.Contains( AssetIOUtil.SEPERATOR_BASEASSET) == true )
        {
            string[] _split = name.Split(  AssetIOUtil.SEPERATOR_BASEASSET[0] );

            bundle_name      = name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            base_bundle_name = _split[0].ToLower(System.Globalization.CultureInfo.InvariantCulture);
            res_name         = _split[1];
        }
        else if( name.Contains( AssetIOUtil.SEPERATOR_ONEPIECE ) == true )
        {
            base_bundle_name = string.Empty;

            string[] _split = name.Split( AssetIOUtil.SEPERATOR_ONEPIECE[0] );
            bundle_name = _split[0].ToLower(System.Globalization.CultureInfo.InvariantCulture);
            res_name    = _split[1];
        }
        else
        {
            base_bundle_name = string.Empty;

            bundle_name = name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            res_name    = name;
        }

        return !( string.IsNullOrEmpty( bundle_name ) || string.IsNullOrEmpty( res_name ) );
    }

    IEnumerator GetNextDownload()
    {
        yield return new WaitForEndOfFrame();

        if( m_activatedDownloadData != null){
            yield break;
        }

        if( m_listBundlesToDownloadFirst.Count == 0 && m_listBundlesToDownload.Count == 0 && m_listBundlesToDownloadLate.Count == 0){
            yield break;
        }

        if (m_listBundlesToDownloadFirst.Count > 0)
        {
            m_activatedDownloadData = m_listBundlesToDownloadFirst[0];
            m_listBundlesToDownloadFirst.RemoveAt(0);
        }
        else if( m_listBundlesToDownload.Count > 0 )
        {
            m_activatedDownloadData = m_listBundlesToDownload[0];
            m_listBundlesToDownload.RemoveAt(0);
        }
        else if(m_bPauseLateQueue == false )
        {
            m_activatedDownloadData = m_listBundlesToDownloadLate[0];
            m_listBundlesToDownloadLate.RemoveAt(0);
        }

        if (m_activatedDownloadData != null)
        {
            StartCoroutine("DownloadAssetbundle");
        }
    }

    void AddLoadData( AssetBundle bundle, string bundle_name, int save_flag, bool load_first )
    {
        if( m_dicCallbackData.ContainsKey( bundle_name ) )
        {
            ProcessData _process = m_dicCallbackData[ bundle_name ];

            if (m_activatedLoadData != _process)
            {
                _process.SetData(bundle);

                if (load_first == false)
                {
                    if (m_listBundlesToLoadFirst.Contains(_process) == true)
                    {
                        Debug.LogError("Really Need This ??? ");
                   //   m_listBundlesToLoadFirst.Remove(_process);
                    }
                    else
                    {
                        if (m_listBundlesToLoad.Contains(_process) == false)
                        {
                            m_listBundlesToLoad.Add(_process);
                        }
                    }
                }
                else
                {
                    if (m_listBundlesToLoad.Contains(_process) == true)
                    {
                        m_listBundlesToLoad.Remove(_process);
                    }

                    if (m_listBundlesToLoadFirst.Contains(_process) == false)
                    {
                        m_listBundlesToLoadFirst.Add(_process);
                    }
                }
            }
        }
        else
        {
            if( m_dicLoadedAssetBundles.ContainsKey(bundle_name) == false &&
                m_dicSourceAssetBundles.ContainsKey(bundle_name) == false )
            {
                if (bundle != null)
                {
                    bundle.Unload(false);
                }
#if SHOW_LOG || UNITY_EDITOR
                Debug.Log("Asset Callback Cleared! >> " + bundle_name);
#endif
            }
        }

        SetNextLoad();
    }

    void SetNextLoad()
    {
        if( m_activatedLoadData != null ){
            return;
        }

        if( m_listBundlesToLoad.Count == 0 && m_listBundlesToLoadFirst.Count == 0 ){
            return;
        }

        //
        // check direct load..
        CheckSyncLoad();
        if( m_listBundlesToLoad.Count == 0 && m_listBundlesToLoadFirst.Count == 0 ){
            return;
        }
        //

        StartCoroutine( "LoadAssets" );
    }

    void CheckSyncLoad()
    {
        var _enum = m_listBundlesToLoadFirst.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _process_data = _enum.Current;
            if (_process_data.useAsyncLoad == false)
            {
                bool _process_end = LoadAssetsDirect(_process_data);
                if (_process_end == true)
                {
                    m_listBundlesToLoadFirst.Remove(_process_data);
                    _enum = m_listBundlesToLoadFirst.GetEnumerator();
                }
            }
        }

        _enum = m_listBundlesToLoad.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _process_data = _enum.Current;
            if (_process_data.useAsyncLoad == false)
            {
                bool _process_end = LoadAssetsDirect(_process_data);
                if (_process_end == true)
                {
                    m_listBundlesToLoad.Remove(_process_data);
                    _enum = m_listBundlesToLoad.GetEnumerator();
                }
            }
        }
    }

    //
    IEnumerator DownloadAssetbundle()
    {
        while( Caching.ready == false ){
            yield return null;
        }

        if( m_activatedDownloadData != null )
        {
#if UNITY_2017_1_OR_NEWER
            using( UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle( m_activatedDownloadData.bundlePath, m_activatedDownloadData.hash, 0 ) )
            {
#if RANDOM_DELAY
                float _min_delay, _max_delay;
                if( IsLoadFromAttached(m_activatedDownloadData.bundleName) == true )
                {
                    _min_delay = 0.5f;
                    _max_delay = 2f;
                }
                else
                {
                    _min_delay = 1f;
                    _max_delay = 3f;
                }
#endif
                request.SendWebRequest();
                while( request.isDone == false )
                {
#if RANDOM_DELAY
                yield return new WaitForSeconds( Random.Range( _min_delay, _max_delay ) );               
#endif
                    m_activatedDownloadData.callbackProgress.SafeInvoke( m_activatedDownloadData.bundleName, request.downloadProgress );

                    yield return null;
                }

                string _err = request.error;
#else
                using( WWW www = WWW.LoadFromCacheOrDownload( m_activatedDownloadData.bundlePath, m_activatedDownloadData.version ) )
                {
        #if UNITY_WEBGL
        #else
                  //www.threadPriority = ThreadPriority.High;
                    www.threadPriority = ThreadPriority.Normal;
        #endif
                    while( www.isDone == false )
                    {
        #if RANDOM_DELAY
                          yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));
        #endif
                          m_activatedDownloadData.callbackProgress.SafeInvoke(m_activatedDownloadData.bundleName, www.progress);

                          yield return null;
                    }

                    string _err = www.error;
#endif
                if( m_activatedDownloadData.referenceCount > 0 )
                {
                    if( string.IsNullOrEmpty( _err ) == true )
                    {
#if UNITY_2017_1_OR_NEWER
                        AssetBundle _downloaded = DownloadHandlerAssetBundle.GetContent( request );
#else
                        AssetBundle _downloaded = www.assetBundle;
#endif
                        if( _downloaded != null )
                        {
                            m_activatedDownloadData.callbackProgress.SafeInvoke( m_activatedDownloadData.bundleName, 1.0f );
                            //     if (_is_cached == true) 
#if UNITY_2017_1_OR_NEWER
                            Caching.MarkAsUsed( m_activatedDownloadData.bundlePath, m_activatedDownloadData.hash );
                            Caching.ClearOtherCachedVersions( m_activatedDownloadData.bundleName, m_activatedDownloadData.hash );
#else
                            Caching.MarkAsUsed(m_activatedDownloadData.bundlePath, m_activatedDownloadData.version);
#endif
                            string _bundle_name = m_activatedDownloadData.bundleName;
                            //2023 05 31 지세영 주석 처리함
                            //Flag를 delete처리할 필요가 없는듯....? 
                            //m_activatedDownloadData.saveFlag = DeleteFlag( m_activatedDownloadData.saveFlag, SaveFlag.Caching );

                            if (m_dicLoadedAssetBundles != null && m_activatedDownloadData != null)
                                Debug.LogWarning("bundle downloaded  ==> " + _bundle_name + " | load count: " + m_dicLoadedAssetBundles.Count);

                            //
                            if( m_activatedDownloadData.saveFlag > 0 )
                            {
                                SourceAssetbundleData _src_ab;
                                if( m_dicSourceAssetBundles.TryGetValue( _bundle_name, out _src_ab ) == true )
                                {
                                    if( _src_ab != null && _src_ab.bundle == null )
                                    {
                                        _src_ab.bundle = _downloaded;
                                    }
                                }

                                if( m_dicLoadedAssetBundles.ContainsKey( _bundle_name ) == false )
                                {
                                    var _data = new AssetbundleData()
                                    {
                                        bundle = _downloaded,
                                        version = m_activatedDownloadData.version,
                                        saveFlag = m_activatedDownloadData.saveFlag,
                                    };
                                    m_dicLoadedAssetBundles.Add( _bundle_name, _data );
                                }
                                else
                                {   // ????
                                    m_dicLoadedAssetBundles[ _bundle_name ].saveFlag = m_activatedDownloadData.saveFlag;
                                }

                                if( m_activatedDownloadData.parse_script == true )
                                {
                                    m_dicLoadedAssetBundles[ _bundle_name ].ReadyToParseScript();
                                }

                                AddLoadData( _downloaded, _bundle_name, m_activatedDownloadData.saveFlag, m_activatedDownloadData.load_immediately );
                            }
                            else
                            {
                                if( m_dicCallbackData.ContainsKey( _bundle_name ) )
                                {
                                    ProcessData _process = m_dicCallbackData[ _bundle_name ];
                                    if( _process != null )
                                    {
                                        //var _res_data = _process.listResNames.Find(r => _bundle_name == r.Key.ToLower());
                                        //if ( default(KeyValuePair<string, System.Type>).Equals( _res_data ) == false  ){
                                        //    _process.Execute(_res_data.Key, null);                               
                                        //}
                                        //else{
                                        //    _process.Execute(_bundle_name, null);
                                        //}
                                        for( int i = 0, _max = _process.listResNames.Count; i < _max; ++i )
                                        {
                                            _process.Execute( _process.listResNames[ i ].Key, null );
                                        }

                                        if( _process.IsExistDelegate() == false )
                                        {
                                            m_dicCallbackData.Remove( _bundle_name );
                                        }
                                    }
                                }

                                _downloaded.Unload( true );
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError( "bundle download >> " + _err );
                        Debug.LogError( request.url );

                        if( m_activatedDownloadData.load_immediately == false )
                        {
                            m_listBundlesToDownload.Add( m_activatedDownloadData );
                        }
                        else
                        {
                            m_listBundlesToDownloadFirst.Add( m_activatedDownloadData );
                        }
                    }
                }
                else
                {
                    if( string.IsNullOrEmpty( _err ) == true )
                    {
#if UNITY_2017_1_OR_NEWER
                        AssetBundle _downloaded = DownloadHandlerAssetBundle.GetContent( request );
#else
                        AssetBundle _downloaded = www.assetBundle;
#endif
                        if( _downloaded != null )
                        {
                            Debug.Log( "Ref Count = 0 Unload " );
                            _downloaded.Unload( false );
                        }
                    }
                }
            }

            m_activatedDownloadData = null;
            StopCoroutine("GetNextDownload");
            StartCoroutine( "GetNextDownload" );
        }
    }

    IEnumerator LoadAssets()
    {
        while( m_listBundlesToLoad.Count > 0 || m_listBundlesToLoadFirst.Count > 0 )
        {
            if (m_listBundlesToLoadFirst.Count > 0)
            {
                m_activatedLoadData = m_listBundlesToLoadFirst[ 0 ];
                m_listBundlesToLoadFirst.RemoveAt(0);
            }
            else
            {
                m_activatedLoadData = m_listBundlesToLoad[ 0 ];
                m_listBundlesToLoad.RemoveAt(0);
            }

            yield return StartCoroutine( "LoadAssetbundle" );

            // check direct load...
            CheckSyncLoad();
        }

        m_activatedLoadData = null;
    }

    void CheckBaseAssetBundleExist( ProcessData data )
    {
        //  Object[] _parent_assets = null;
        if( string.IsNullOrEmpty( data.sourceBundleName) == true ){
            return;
        }

        if( m_dicSourceAssetBundles.ContainsKey( data.sourceBundleName ) == false )
        {
            Debug.LogError( "Not Exist Source AssetBundle >> " + data.sourceBundleName );
        }
        else
        {
            SourceAssetbundleData _src_data = m_dicSourceAssetBundles[ data.sourceBundleName ];
            if( _src_data.bundle != null )
            {
                //  var _req = _src_data.bundle.LoadAllAssetsAsync();
                //  yield return new WaitWhile( () => _req.isDone == false );
                //  _parent_assets = _req.allAssets;
                //          _parent_assets = _src_data.bundle.LoadAllAssets();
                //          _src_data.referenceCount++;
            }
            else
            {
                Debug.LogError( "Not Loaded Source AssetBundle >> " + data.sourceBundleName );
            }
        }
    }

    IEnumerator LoadAssetbundle()
    {
        if( m_activatedLoadData == null )
            yield break;

        AssetBundle _target = m_activatedLoadData.assetBundle;
        if( _target != null )
        {
            CheckBaseAssetBundleExist( m_activatedLoadData );

            if( _target.isStreamedSceneAssetBundle == false )
            {
                bool _unload = true;
                if( m_dicLoadedAssetBundles.ContainsKey( m_activatedLoadData.bundleName ) == true ){
                    _unload = false;
                }
            
                int _exec_count = 0;
                while( true )
                {
                    if( m_activatedLoadData.listResNames.Count <= _exec_count ){
                        break;
                    }

                    if( _target == null ){
                        break;
                    }
            
                    string _res_name, _res_file_name;        
                    System.Type _target_type;
                    GetFileInfoToLoad( m_activatedLoadData.listResNames[_exec_count], out _res_name, out _res_file_name, out _target_type);
            
                    var _req = _target.LoadAssetAsync( _res_file_name, _target_type  );
//#if RANDOM_DELAY
//                   while( _req.isDone == false ){
//                        yield return new WaitForSeconds( Random.Range( 0.5f, 2.0f ) );
//                   }
//#else
                    yield return new WaitWhile( () => _req.isDone == false);
//#endif
            
                    Object _loaded = _req.asset;
               
                    Debug.LogWarning( "bundle loaded >> " + _res_file_name + " " + _exec_count );
            
                    if( _loaded == null )
                    {
                        Debug.LogError( "Loaded Asset is Null : " + _res_file_name + " " + _target_type.ToString() );
                        m_activatedLoadData.Execute(_res_name, null);
                    }
                    else
                    {
                        var _bundle_name = Path.GetFileNameWithoutExtension(_target.name);
                        if( m_dicLoadedAssetBundles.ContainsKey( _bundle_name ) == true )
                        {
                            _loaded = m_dicLoadedAssetBundles[_bundle_name].CheckParsingScript(_res_name, _loaded, transform ); 
                            if (_loaded != _req.asset)
                            {   // wait for destroying AssetBundleEtcData script..!!
                                yield return null;
                            }
                        }

 #if UNITY_EDITOR
                        if( _loaded.GetType() == typeof(GameObject) )
                        {
                            Renderer[] _renderers = ((GameObject)_loaded).GetComponentsInChildren<Renderer>(true);
                            for( int i=0, _max = _renderers.Length; i < _max; ++i )
                            {
                                for( int j=0, _maxj = _renderers[i].sharedMaterials.Length; j < _maxj; ++j )
                                {
                                    var _curr_mtrl = _renderers[i].sharedMaterials[j];
                                    if (_curr_mtrl != null && _curr_mtrl.shader != null ){
                                        _curr_mtrl.shader = Shader.Find(_curr_mtrl.shader.name);
                                    }
                                }
                            }
                        }
                        else if( _loaded.GetType() == typeof(Material))
                        {
                            var _mat = _loaded as Material;
                            if( _mat != null && _mat.shader != null)
                            {
                                _mat.shader = Shader.Find( _mat.shader.name );
                            }
                         }
 #endif

                        m_activatedLoadData.Execute( _res_name, _loaded );
                    }
                    _exec_count++;
                }
            
                if( _unload == true  ){
                    m_activatedLoadData.ClearBundle();
                }
            }
            else
            {
                string[] _scenes   = _target.GetAllScenePaths();
                string _scene_name = Path.GetFileNameWithoutExtension( _scenes[0] );
                Scene addScene;

                if( m_activatedLoadData.useAsyncLoad == true )
                {
                    var _sync = SceneManager.LoadSceneAsync(_scene_name, LoadSceneMode.Additive  );
                    while( _sync.isDone == false ){
                        yield return null;
                    }

                    addScene = SceneManager.GetSceneByName( _scene_name );
                }
                else
                {
                    SceneManager.LoadScene( _scene_name, LoadSceneMode.Additive );

                    addScene = SceneManager.GetSceneByName( _scene_name );
                    while( addScene.isLoaded == false )
                        yield return null;
                }

#if UNITY_EDITOR
                GameObject[] _roots = addScene.GetRootGameObjects();
                for (int i = 0, _max = _roots.Length; i < _max; ++i)
                {
                    Renderer[] _renderers = _roots[i].GetComponentsInChildren<Renderer>();
                    for (int j = 0, _maxj = _renderers.Length; j < _maxj; ++j)
                    {
                        Material[] _mats = _renderers[j].sharedMaterials;
                        for (int k = 0, _maxk = _mats.Length; k < _maxk; ++k)
                        {
                            if(  _mats[k] == null )
                                continue;
                            
                            _mats[k].shader = Shader.Find( _mats[k].shader.name);
                        }
                    }
                }
#endif
            // need ?
           //   SceneManager.SetActiveScene( addScene );
           //   UpdateUIDrawcallObjs( _scene_name, true );

                yield return null;

                m_activatedLoadData.Execute( _scene_name, null );

                if( m_dicLoadedAssetBundles.ContainsKey( m_activatedLoadData.bundleName ) == false )
                {
                    Debug.LogError( "Clear Loaded Scene!!" );

                    if( m_activatedLoadData.useAsyncLoad == true )
                    {
                        var _sync = SceneManager.UnloadSceneAsync( addScene );
                        while( _sync.isDone == false ){
                            yield return null;
                        }
                    }
                    else
                    {
                        SceneManager.UnloadScene( addScene );
                        yield return null;
                    }
                }
            }

            m_dicCallbackData.Remove( m_activatedLoadData.bundleName);
        }
    }
#if NGUI
    void UpdateUIDrawcallObjs( string target_scene, bool isUpdateDrawCall )
    {
        var _target = SceneManager.GetSceneByName(target_scene);
        var _enum = UIDrawCall.activeList.GetEnumerator();
        while( _enum.MoveNext() )
        {
            if (_enum.Current == null || _enum.Current.gameObject == null)
                continue;

            SceneManager.MoveGameObjectToScene(_enum.Current.gameObject, _target);
        }

        _enum = UIDrawCall.inactiveList.GetEnumerator();
        while( _enum.MoveNext() )
        {
            if (_enum.Current == null || _enum.Current.gameObject == null)
                continue;

            SceneManager.MoveGameObjectToScene(_enum.Current.gameObject, _target);
        }
    }
#endif
    void GetFileInfoToLoad( KeyValuePair<string,System.Type> data, out string res_name, out string res_file_name, out System.Type target_type )
    {
        res_name    = data.Key;
        target_type = data.Value;

        //--------------------------------------------------------------
        // bug fix : unity for ios...--;
        res_file_name = res_name;
#if NGUI
        if( target_type == typeof(UIAtlas ) )
        {
            res_file_name = string.Format( "{0}.prefab", res_name );
            target_type   = typeof(GameObject);
        }
#endif
        //--------------------------------------------------------------
    }

    bool LoadAssetsDirect( ProcessData process_data )
    {   
        AssetBundle _target = process_data.assetBundle;
        if (_target == null)
        {
            Debug.LogError("LoadAssetsDirect : assetbundle is Null");
            return false;
        }

        if (_target.isStreamedSceneAssetBundle == true)
            return false;
  
        CheckBaseAssetBundleExist( process_data );

        bool _unload = true;
        if( m_dicLoadedAssetBundles.ContainsKey( process_data.bundleName ) == true )
        {
            _unload = false;
        }

        int _exec_count = 0;
        while( true )
        {
            if( process_data.listResNames.Count <= _exec_count ){
                break;
            }

            string _res_name, _res_file_name;        
            System.Type _target_type;
            GetFileInfoToLoad( process_data.listResNames[_exec_count], out _res_name, out _res_file_name, out _target_type);

            Object _loaded = _target.LoadAsset( _res_file_name, _target_type );
            Debug.LogWarning( "bundle loaded >> " + _res_file_name );

            if( _loaded == null )
            {
                Debug.LogError( "Loaded Asset is Null : " + _res_file_name );
            }

            process_data.Execute( _res_name, _loaded );

            _exec_count++;
        }

        if( _unload == true  ){
            process_data.ClearBundle();
        }
     
        m_dicCallbackData.Remove( process_data.bundleName);

        return true;
    }
}
