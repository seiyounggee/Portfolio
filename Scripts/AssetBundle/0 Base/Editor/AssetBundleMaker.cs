using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.SceneManagement;

public class AssetFilesData 
{
    public enum ScriptExtractionStep
    {
        None,
        ScriptOnly,
        ScriptAndAttachAsset,
    }
    public string outputFileName;
    public int    buildMenutype;  
    public int    revisionNumber;
    public int    folderNumber;
#if UNITY_2017_1_OR_NEWER
    public string hashResult;
#endif
 // public int    count = 0;                /* 프리셋 갯수 확인하기 위해서 */
    public bool   bMakeLookupTable;
    public ScriptExtractionStep step;
    public List<string> listSourceFiles;
}

public class ComparerClass : IComparer<GameObject>
{
    public int Compare( GameObject x, GameObject y )
    {
        return x.name.CompareTo(y.name);
    }
}

public class AssetBundleMaker : BaseAssetBundleMaker
{
    private static Dictionary<AssetDefines.MakeAssetMenuEnum, List<FileRecord>> m_dicFileRecords = new Dictionary<AssetDefines.MakeAssetMenuEnum, List<FileRecord>>();
    private static Dictionary<string, int> m_SvnFileVersion = new Dictionary<string, int>();

    public static void InitializeData()
    {
        m_SvnFileVersion.Clear();
        m_dicFileRecords.Clear();
    }

    public static AssetBundleMaker GetInstance( BuildTarget target )
    {
        return new AssetBundleMaker( target );
    }

    //
    public AssetBundleMaker( BuildTarget target )
    {
        m_BuildTarget = target;
    }

    bool MakeScriptInfo( GameObject[] roots, GameObject target_obj, Dictionary<string, object> dic_result, ref Dictionary<string,UnityEngine.Object> dic_attach )
    {
        bool _exist_script = false;

        MonoBehaviour[] _scripts = target_obj.GetComponentsInChildren<MonoBehaviour>(true);
        if (_scripts != null && _scripts.Length > 0)
        {
            _exist_script = true;

            for (int i = 0, _max = _scripts.Length; i < _max; ++i)
            {
                if (_scripts[i] == null){
                    continue;
                }

                var _type = _scripts[i].GetType();
                if (_type == typeof(SceneAssetBundleLightmaps)){
                    continue;
                }

                var _dic_scriptdata = new Dictionary<string, object>();
                FieldInfo[] _infos  = _type.GetFields( BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance );
                for (int m = 0, _maxm = _infos.Length; m < _maxm; ++m)
                {
                    if( AssetIOUtil.IsSaveData( _infos[m] ) == false ){
                        continue;
                    }

                    AssetIOUtil.Serialize( roots, _infos[m].FieldType, _infos[m].Name, _infos[m].GetValue(_scripts[i]), _dic_scriptdata, ref dic_attach );
                }

                var _pos_data = AssetIOUtil.GetPathAsString( roots, _scripts[i].transform );

                var _result = new Dictionary<string,object>();
                _result.Add("pos", _pos_data );
                _result.Add("data", _dic_scriptdata);

                var _key = string.Format( "{0}{1}{2}", _type.ToString(), AssetIOUtil.SEPERATOR_SCRIPTDATA, _pos_data );
                dic_result.Add( _key, _result);
            }
        }

        return _exist_script;
    }

    protected override void AddDataToList( string file_name, AssetFilesData.ScriptExtractionStep step, ref List<FileRecord> list_data )
    {
        Object _t = AssetDatabase.LoadAssetAtPath( file_name, typeof(Object));
        if( _t == null )
            return;

        var _dic_tosave = new Dictionary<string, object>();
        if( _t.GetType() == typeof(SceneAsset) )
        {
            bool _apply_clone = false;

            if( step != AssetFilesData.ScriptExtractionStep.None )
            {
                var _prev_path = EditorSceneManager.GetActiveScene().path;

                var _scene = EditorSceneManager.OpenScene(file_name, OpenSceneMode.Single);
                if (_scene != default(UnityEngine.SceneManagement.Scene))
                {
                    GameObject[] _arrObjs = _scene.GetRootGameObjects();
                    for( int j = 0, _maxj = _arrObjs.Length; j < _maxj; j++ )
                    {
#if NOT_USE
                       if( BaseAssetBundleMaker.IsExistLightmap(_arrObjs[j]) == true )
                       {
                           _apply_clone = true;
                           break;
                       }
#endif

                        MonoBehaviour[] _scripts = _arrObjs[j].GetComponentsInChildren<MonoBehaviour>(true);
                        if (_scripts != null && _scripts.Length > 0)
                        {
                            _apply_clone = true;
                            break;
                        }
                    }
      
                    //
                //  EditorSceneManager.CloseScene(_scene, true);
                }

                if (EditorSceneManager.GetActiveScene().path != _prev_path)
                {
                    EditorSceneManager.OpenScene(_prev_path, OpenSceneMode.Single);
                }
            }

            if( _apply_clone == false )
            {
                var _record = FileRecord.Get();
                _record.mAssetPath = file_name;
                _record.mName = _t.name;
                _record.mOriginAssetPath = string.Empty;
               
                list_data.Add(_record);
            }
            else
            {
                string _cloned_file = string.Format("{0}/{1}", Path.GetDirectoryName(file_name), string.Format("{0}_Cloned.unity", Path.GetFileNameWithoutExtension(file_name)));

                AssetDatabase.RenameAsset(file_name, Path.GetFileNameWithoutExtension(_cloned_file) );
                AssetDatabase.Refresh();
                  
                FileUtil.CopyFileOrDirectory(_cloned_file, file_name);
                AssetDatabase.Refresh();
                //

                var _prev_path = EditorSceneManager.GetActiveScene().path;

                var _scene = EditorSceneManager.OpenScene(file_name, OpenSceneMode.Single );
                if( _scene != default(UnityEngine.SceneManagement.Scene) )
                {
                    GameObject[] _objs = _scene.GetRootGameObjects();
                    System.Array.Sort(_objs, new ComparerClass() );

                    Dictionary<string,UnityEngine.Object> _dic_attach_asset = null;
                    if( step == AssetFilesData.ScriptExtractionStep.ScriptAndAttachAsset )
                    {
                        _dic_attach_asset = new Dictionary<string, Object>();
                    }

                    for (int j = 0, _maxj = _objs.Length; j < _maxj; j++)
                    {
                        MakeScriptInfo(_objs, _objs[j], _dic_tosave, ref _dic_attach_asset );
                    }

                    for (int j = 0, _maxj = _objs.Length; j < _maxj; j++)
                    {
                        MonoBehaviour[] _scripts = _objs[j].GetComponentsInChildren<MonoBehaviour>(true);
  
                        for (int k = 0, _maxk = _scripts.Length; k < _maxk; ++k)
                        {
                            Object.DestroyImmediate(_scripts[k], true);
                        }

#if NOT_USE
                        // lightmap info setting..
                        bool _re = BaseAssetBundleMaker.AttachLightmapScript( LightmapSettings.lightmapsMode, _objs[j]);
                        if (_re == true)
                        {
                            Debug.Log("LightMap Attached !!!");
                        }
#endif
                    }

                    // !!!
                    var _data_script = _objs[0].AddComponent<AssetBundleEtcData>();
                    if (_data_script != null)
                    {
                        _data_script.objLists = _objs;
                        _data_script.ScriptData = MiniJSON.Json.Serialize(_dic_tosave);

                        if( _dic_attach_asset != null )
                        {
                            var _temp_list = new List<UnityEngine.Object>();
                            var _enum = _dic_attach_asset.GetEnumerator();
                            while( _enum.MoveNext() )
                            {
                                var _asset_obj = _enum.Current.Value; 
                                _temp_list.Add( _asset_obj );
                            }
                        
                            _data_script.objAttached = _temp_list.ToArray();
                        }
                    }

                    //

                    EditorSceneManager.SaveScene(_scene);
            
                //  EditorSceneManager.CloseScene(_scene, true);

                    if( EditorSceneManager.GetActiveScene().path != _prev_path )
                    {
                        EditorSceneManager.OpenScene(_prev_path, OpenSceneMode.Single);
                    }
                }

                //
               var _record = FileRecord.Get();
               _record.mAssetPath = file_name;
               _record.mName = Path.GetFileNameWithoutExtension(file_name);
               //

               _record.mOriginAssetPath = _cloned_file;
               //
               list_data.Add(_record);
            }
        }
        else if (_t.GetType() == typeof(GameObject))
        {
            bool _apply_clone = false;

            Dictionary<string,UnityEngine.Object> _dic_attach_asset = null;

            if( step != AssetFilesData.ScriptExtractionStep.None )
            {
                GameObject _data = _t as GameObject;
                if (_data != null)
                {
                    if( step == AssetFilesData.ScriptExtractionStep.ScriptAndAttachAsset )
                        _dic_attach_asset = new Dictionary<string, Object>();

                    _apply_clone = MakeScriptInfo(new GameObject[]{ _data }, _data, _dic_tosave, ref _dic_attach_asset);
                }
            }

            var _record = FileRecord.Get();
            if( _apply_clone == false )
            {
                _record.mAssetPath = file_name;
                _record.mName      = _t.name;
                _record.mOriginAssetPath = string.Empty;
            }
            else
            {
                string _cloned_file = string.Format("{0}/{1}", Path.GetDirectoryName(file_name), string.Format("{0}_Cloned.prefab", Path.GetFileNameWithoutExtension(file_name)));

                AssetDatabase.RenameAsset(file_name, Path.GetFileNameWithoutExtension(_cloned_file) );
                AssetDatabase.Refresh();

                FileUtil.CopyFileOrDirectory(_cloned_file, file_name);
                AssetDatabase.Refresh();
                //
                var _root_object = AssetDatabase.LoadAssetAtPath<GameObject>(file_name);
                if (_root_object != null)
                {
                    MonoBehaviour[] _scripts = _root_object.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int k = 0, _maxk = _scripts.Length; k < _maxk; ++k)
                    {
                        Object.DestroyImmediate(_scripts[k], true);
                    }

                    // !!!
                    var _data_script = _root_object.AddComponent<AssetBundleEtcData>();
                    if (_data_script != null)
                    {
                        _data_script.objLists   = new GameObject[]{ _root_object };
                        _data_script.ScriptData = MiniJSON.Json.Serialize(_dic_tosave);

                        if( _dic_attach_asset != null )
                        {
                            var _temp_list = new List<UnityEngine.Object>();
                            var _enum = _dic_attach_asset.GetEnumerator();
                            while( _enum.MoveNext() )
                            {
                                var _asset_obj = _enum.Current.Value; 
                                _temp_list.Add( _asset_obj );
                            }

                            _data_script.objAttached = _temp_list.ToArray();
                        }
                    }
                }

                _record.mAssetPath = file_name;
                _record.mName      = Path.GetFileNameWithoutExtension(file_name);
                //
                _record.mOriginAssetPath = _cloned_file;
            }

            list_data.Add(_record);
        }
        else
        {
            var _record = FileRecord.Get();
            _record.mAssetPath = file_name;
            _record.mName      = _t.name;

            list_data.Add(_record);
        }
    }

    //
    public static List<AssetFilesData> GetAssetBundleRevisions( AssetDefines.MakeAssetMenuEnum type, string folder_path, FindAssetFiles find_type, bool make_lookuptable, string skip_keyword )
    {  
        EditorUtility.DisplayProgressBar( folder_path, string.Format( "{0} ....-> {1}%", folder_path, 0 ), 0 );

        float currValue              = 0f;
        List<AssetFilesData> _result = new List<AssetFilesData>();


        Dictionary<string, List<string>> _dic_files = BaseAssetBundleMaker.GetAssetTargetFiles( folder_path, find_type );
        float max = _dic_files.Count;
        int curr = 0;

        var _enum = _dic_files.GetEnumerator();
        while( _enum.MoveNext() )
        {
            int _revision = 0;
            var _list_target_files = _enum.Current.Value;
            for( int i = 0, _max = _list_target_files.Count; i < _max; ++i )
            {
                int _file_revision = GetAssetFileRevision( _list_target_files[ i ], skip_keyword);
                _revision = Mathf.Max( _revision, _file_revision );

                currValue = ( float )i / ( float )_max;
                EditorUtility.DisplayProgressBar( folder_path, string.Format( "{0} )....-> {1}%",  _list_target_files[ i ], ( int )( currValue * 100f ) ), currValue );
            }

            switch( find_type )
            {
                case FindAssetFiles.OnlyFolders:
                case FindAssetFiles.AllAsFolders:
                    {
                        List<string> _parent_folders = new List<string>();
                        for( int i = 0, _max = _list_target_files.Count; i < _max; ++i )
                        {
                            var _parent = Directory.GetParent( _list_target_files[i] ).FullName;
                            if( string.IsNullOrEmpty( _parent ) == true )
                                continue;

                            if( _parent_folders.Contains( _parent ) == true )
                                continue;

                            _parent_folders.Add( _parent );
                        }

                        for( int i=0, _max = _parent_folders.Count; i < _max; ++i )
                        {
                             var _folder_revision = GetSvn( _parent_folders[i], false );
                             _revision = Mathf.Max( _revision, _folder_revision );
                        }
                    }
                    break;

                case FindAssetFiles.FileAndFolders:
                    {
                        List<string> _parent_folders = new List<string>();
                        for( int i = 0, _max = _list_target_files.Count; i < _max; ++i )
                        {
                            var _parent = Directory.GetParent( _list_target_files[i] ).FullName;
                            if( string.IsNullOrEmpty( _parent ) == true )
                                continue;

                            if( _parent == folder_path )    // file type..
                                continue;

                            if( _parent_folders.Contains( _parent ) == true )
                                continue;

                            _parent_folders.Add( _parent );
                        }

                        for( int i=0, _max = _parent_folders.Count; i < _max; ++i )
                        {
                             var _folder_revision = GetSvn( _parent_folders[i], false );
                             _revision = Mathf.Max( _revision, _folder_revision );
                        }
                    }
                    break;
            }

            if( _result.Exists( r => r.outputFileName == _enum.Current.Key ) == true )
            {
                Debug.LogError( "GetAssetBundleRevisions: Exist Same AssetFileName! : " + _enum.Current.Key );
                continue;
            }
            else if( _revision == 0 )
            {
                Debug.LogError( "GetAssetBundleRevisions : Svn Error! : " + _enum.Current.Key );
                continue;
            }

            _result.Add( new AssetFilesData()
            {
                buildMenutype = ( int )type,
                outputFileName = _enum.Current.Key,
                revisionNumber = _revision,
                listSourceFiles = _list_target_files,
                bMakeLookupTable = make_lookuptable,
            //  count = _enum.Current.Value.Count
            } );

            currValue = ( float )curr / ( float )max;
            EditorUtility.DisplayProgressBar( folder_path, string.Format( "{0} )....-> {1}%", _enum.Current.Key, ( int )( currValue * 100f ) ), currValue );
            ++curr;
        }

       EditorUtility.ClearProgressBar();


       return _result;
    }

    static int GetSvn( string folder_path, bool check_meta = true )
    {
        int _revision = 0;

        var _path = Path.GetFullPath( folder_path );
        _revision = SvnCommand.GetSvnRevision( _path );

        if( check_meta == true )
        {
            _path = Path.GetFullPath( string.Format( "{0}.meta", folder_path ) );
            _revision = Mathf.Max( _revision, SvnCommand.GetSvnRevision( _path ) );
        }

        return _revision;
    }

    static int GetAssetFileRevision( string file_path, string skip_keyword )
    {
        int _revision = 0;

    //  string _path;
        int    _temp_revision = 0;
    //  int    _meta_revision = 0;

        string[] _depen;
        if( Path.GetExtension(file_path) == ".unity" )
        {
            List<string> _result = new List<string>();

            _result.Add(file_path);

            string[] _items = AssetDatabase.GetDependencies(file_path, false);
            for( int i = 0, _max = _items.Length; i < _max; ++i )
            {
                string[] _dep;

                if( Path.GetFileName(_items[i]) == "LightingData.asset" ){
                    _dep = AssetDatabase.GetDependencies(_items[i], false);
                }
                else{
                    _dep = AssetDatabase.GetDependencies(_items[i], true);
                }

                for( int j = 0, _maxj = _dep.Length; j < _maxj; ++j ) 
                {
                    //-----------------------------------------------------------
                    // need?
                    if (Path.GetExtension(_dep[j]) == ".unity")
                        continue;
                    //-----------------------------------------------------------

                    if( _result.Contains(_dep[j]) == false )
                        _result.Add(_dep[j]);
                }

                if( _result.Contains(_items[i]) == false )
                    _result.Add(_items[i]);
            }

            _depen = _result.ToArray();
        }
        else
        {
            _depen = AssetDatabase.GetDependencies( file_path, true );
        }

        for( int i = 0, _max = _depen.Length; i < _max; ++i )
        {
            if( string.IsNullOrEmpty( skip_keyword ) == false && IsSkipKeyword( skip_keyword, _depen[i] ) ){
                continue;
            }

            if( m_SvnFileVersion.ContainsKey( _depen[ i ] ) )
            {
                _revision = Mathf.Max( _revision, m_SvnFileVersion[ _depen[ i ] ] );
            }
            else
            {
                _temp_revision = GetSvn( _depen[i] );
                _revision = Mathf.Max( _revision, _temp_revision );

                m_SvnFileVersion.Add( _depen[ i ], _temp_revision );
            }
        }

        return _revision;
    }

    static string[] GetSkipKeyWords(string keywords)
    {
        keywords = keywords.Replace(" ", string.Empty);
        var splitKeywords = keywords.Split(new char[] { ',' });

        return splitKeywords;
    }

    static bool IsSkipKeyword(string keywords, string str)
    { 
        bool result = false;
        var split = GetSkipKeyWords(keywords);

        foreach (var i in split)
        {
            if (str.Contains(i))
            {
                result = true; 
                break;
            }
        }

        return result;
    }

    //
    public void MakeAssetBundles( string output_path, List<AssetFilesData> list_targetfiles, System.Action<AssetDefines.MakeAssetMenuEnum> callback_make_prev = null, System.Action<AssetDefines.MakeAssetMenuEnum> callback_make_post = null )
    {
        if( Directory.Exists(Path.GetFullPath(output_path)) == false ){
            Directory.CreateDirectory(Path.GetFullPath(output_path));
        }   

        m_dicFileRecords.Clear();
        AssetFilesData _currData = null;
        AssetDefines.MakeAssetMenuEnum _menuType = AssetDefines.MakeAssetMenuEnum.All;
        for (int i = 0, _max = list_targetfiles.Count; i < _max; ++i)
        {
            List<FileRecord> _listData = new List<FileRecord>();
            _currData = list_targetfiles[i];
            _menuType = ( AssetDefines.MakeAssetMenuEnum )_currData.buildMenutype;

            for( int j = 0, _maxj = _currData.listSourceFiles.Count; j < _maxj; ++j )
            {
                AddDataToList( _currData.listSourceFiles[ j ], _currData.step, ref _listData );
            }

            if( m_dicFileRecords.ContainsKey(_menuType) == false ){
                m_dicFileRecords.Add(_menuType, new List<FileRecord>());
            }

            for (int j = 0; j < _listData.Count; ++j)
            {
                if( string.IsNullOrEmpty(_listData[j].mOriginAssetPath) == false ){
                    m_dicFileRecords[_menuType].Add(_listData[j]);
                }
            }

            if( callback_make_prev != null )
            {
                callback_make_prev(_menuType);
            }

            // ToDo : null argument >> source asset bundle making...
#if UNITY_2017_1_OR_NEWER
            var _hash = MakeData( _listData, output_path, Path.GetFileNameWithoutExtension( _currData.outputFileName ), null );
            _currData.hashResult = _hash;
#else
            MakeData( _listData, output_path, Path.GetFileNameWithoutExtension( _currData.outputFileName ), null );
#endif

            if( callback_make_post != null )
            {
                callback_make_post(_menuType);
            }
        }

        FileRevert();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        RemoveUnusedFiles(output_path);
    }

    void RemoveUnusedFiles( string path )
    {
        if (string.IsNullOrEmpty(path) == true)
            return;

        var _dir_info = new DirectoryInfo(path);
        FileInfo[] _files = _dir_info.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0, _max = _files.Length; i < _max; ++i)
        {
            string _extension = _files[i].Extension;
            if (_extension.Contains(".ab")  || 
                _extension.Contains(".txt") ||
                _extension.Contains(".av")  )
                continue;

            FileUtil.DeleteFileOrDirectory(_files[i].FullName);
        }
    }

    void FileRevert()
    {
        if( m_dicFileRecords.Count == 0 )
            return;
    
        var _enum = m_dicFileRecords.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _list = _enum.Current.Value;
            for( int k = 0, _max = _list.Count; k < _max; ++k ){
                FileUtil.DeleteFileOrDirectory( _list[ k ].mAssetPath );
            }
        }

        AssetDatabase.Refresh();
    
        _enum = m_dicFileRecords.GetEnumerator();
        while (_enum.MoveNext())
        {
            var _list = _enum.Current.Value;
            for( int k = 0, _max = _list.Count; k < _max; ++k ){
                AssetDatabase.RenameAsset(_list[ k ].mOriginAssetPath, Path.GetFileNameWithoutExtension( _list[ k ].mAssetPath ) );
            }
        }
    }

    public void MakeVersionListFile( string output_path, int target_version, List<AssetFilesData> list_targetfiles, List< AssetVersionData > list_prev_versions, bool keep_file_version )
    {
        if( Directory.Exists(Path.GetFullPath(output_path)) == false ){
            Directory.CreateDirectory(Path.GetFullPath(output_path));
        }  

        List<AssetVersionData> _list_writes = new List<AssetVersionData>();

        if( list_prev_versions != null )
        {
            for( int i = 0; i < list_prev_versions.Count; ++i ){
                _list_writes.Add( list_prev_versions[ i ] );
            }
        }

        AssetVersionData _exist_data = null;
        // update prev list from curr list
        List< AssetVersionData > _list_curr   = AssetVersions.ReadAssetVersionFile(output_path, target_version );
        for (int i = 0, _max = _list_curr.Count; i < _max; ++i)
        {
            _exist_data = _list_writes.Find( r => r.filename == _list_curr[ i ].filename );
            if( _exist_data == null )
            {
                _list_writes.Add( _list_curr[ i ] );
                continue;
            }

            _exist_data.revision = _list_curr[ i ].revision;
            _exist_data.version  = _list_curr[ i ].version;
#if UNITY_2017_1_OR_NEWER
            _exist_data.hash = _list_curr[i].hash;
#endif
            _exist_data.lookuptable = _list_curr[i].lookuptable;
        }

        // modify or add 
        AssetVersionData data = null;
        for( int i = 0, _max = list_targetfiles.Count; i < _max; ++i )
        {
            _exist_data = _list_writes.Find( r => r.filename == list_targetfiles[ i ].outputFileName );
            if( _exist_data == null )
            {
                data = new AssetVersionData()
                {
                    version  = ( keep_file_version == true) ? list_targetfiles[ i ].folderNumber : target_version,
                    filename = list_targetfiles[ i ].outputFileName,
                    revision = list_targetfiles[ i ].revisionNumber,
#if UNITY_2017_1_OR_NEWER
                    hash     = list_targetfiles[ i ].hashResult,
#endif
                    lookuptable = ""
                };
                              
                if (list_targetfiles[i].bMakeLookupTable == true )
                {
                    string _lookup_data = "";
                    for (int j = 0; j < list_targetfiles[i].listSourceFiles.Count; ++j)
                    {
                        var _file_name = Path.GetFileNameWithoutExtension(list_targetfiles[i].listSourceFiles[j]);
                        _lookup_data += string.Format("{0}{1}", _file_name, AssetIOUtil.SEPERATOR_LOOKUPTABLE);
                    }
                    data.lookuptable = _lookup_data.TrimEnd(AssetIOUtil.SEPERATOR_LOOKUPTABLE[0]);
                }
                                                 
                _list_writes.Add( data );
            }
            else
            {
                if( _exist_data.revision > list_targetfiles[ i ].revisionNumber )
                {
                    Debug.LogError( "Revision Compare Error!!!!" );
                }
                else
                {
                    _exist_data.version = ( keep_file_version == true) ? list_targetfiles[ i ].folderNumber : target_version;
                    _exist_data.revision = list_targetfiles[ i ].revisionNumber;
#if UNITY_2017_1_OR_NEWER
                    _exist_data.hash = list_targetfiles[i].hashResult;
#endif
                    if ( list_targetfiles[i].bMakeLookupTable == true )
                    {
                        string _lookup_data = "";
                        for (int j = 0; j < list_targetfiles[i].listSourceFiles.Count; ++j)
                        {
                            var _file_name = Path.GetFileNameWithoutExtension(list_targetfiles[i].listSourceFiles[j]);
                            _lookup_data += string.Format("{0}:", _file_name);
                        }
                        _exist_data.lookuptable = _lookup_data.TrimEnd(':');
                    }
                    else
                    {
                        _exist_data.lookuptable = "";
                    }
                }
            }
        }

        _list_writes.Sort(delegate(AssetVersionData x, AssetVersionData y){
            if( x.version == y.version )
            {
                if( x.revision == y.revision ){
                    return x.filename.CompareTo( y.filename );
                }
                else{
                    return x.revision.CompareTo( y.revision );
                }
            }
            else{
                return x.version.CompareTo( y.version );
            }
        });

        AssetVersions.WriteAssetVersionFile( output_path, target_version, _list_writes );
    }

}
