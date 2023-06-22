using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BaseAssetBundleCopyEditor : EditorWindow
{
    System.Reflection.MethodInfo  m_methodName2Type = null;

    AssetDefines.ServerEnum  m_targetServer = AssetDefines.ServerEnum.None;
    string[] SERVER_NAMES;

    int m_targetVersionIndex;
    int m_targetVersion;
    string[] VERSION_LIST;

    bool m_isValidList;

    class SkipData
    {
        public string fileName;
        public bool isSkip;
    }

    List<SkipData>  m_listSkipData = new List<SkipData>();
    Vector2 m_posScroll;
    int m_nSkipCount;

    List<string> m_listNeedIncludingAssets = new List<string>();    // tutorial & costume default..etc..

    //
    public BaseAssetBundleCopyEditor()
    {
    }

    protected void MakeServerNameLists( System.Type type )
    {
        m_methodName2Type = type.GetMethod( "op_Implicit", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy, 
            null, System.Reflection.CallingConventions.Any, new System.Type[]{typeof(string)}, null );

        var _methodInfo = type.GetMethod("GetNames", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy );
        if (_methodInfo != null)
        {
            var _names = (string[])_methodInfo.Invoke(null, new object[] {type});

            SERVER_NAMES = new string[ _names.Length ];
            for (int i = 0, _max = _names.Length; i < _max; ++i){
                SERVER_NAMES[i] = _names[i];
            }
        }
    }

    public void SetServerType( AssetDefines.ServerEnum type )
    {
        m_targetServer = type;
        m_isValidList = SearchVersionLists( EditorUserBuildSettings.activeBuildTarget );
    }

    void OnGUI()
    {
        var _prev_nameindex = m_targetServer.NameIndex;
        int _nameindex      = EditorGUILayout.Popup("Target Server : ", _prev_nameindex, SERVER_NAMES);
        if( _prev_nameindex != _nameindex )
        {
            m_targetServer = (AssetDefines.ServerEnum)m_methodName2Type.Invoke(null, new object[]{ SERVER_NAMES[_nameindex] });

            m_isValidList  = SearchVersionLists( EditorUserBuildSettings.activeBuildTarget );
            LoadSkipList();
        }

        //
        GUI.enabled = m_isValidList;

        var _prev_index = m_targetVersionIndex;
        m_targetVersionIndex = EditorGUILayout.Popup("Target Version : ", m_targetVersionIndex, VERSION_LIST);
        if (_prev_index != m_targetVersionIndex)
        {
            m_targetVersion = int.Parse(VERSION_LIST[m_targetVersionIndex]);
            LoadSkipList();
        }

        GUI.enabled = true;

        //
        EditorGUILayout.BeginVertical( GUI.skin.box );

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Asset Files to Skip : ");

        if (GUILayout.Button("Load From Default AssetList")== true)
        {
            string _path = EditorUtility.OpenFilePanel("Load File", ".",  "txt" );
            if( _path.Length != 0 )
            {
                List<string> _loaded = new List<string>();
                using (System.IO.FileStream fs = new System.IO.FileStream(_path, System.IO.FileMode.Open))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null){
                            _loaded.Add(line);
                        }
                    }
                }

                for( int i=0, _max = m_listSkipData.Count; i < _max; ++i )
                {
                    m_listSkipData[i].isSkip = !_loaded.Contains(m_listSkipData[i].fileName);
                }
                m_nSkipCount = m_listSkipData.FindAll(r => r.isSkip == true).Count;
            }
        }
        EditorGUILayout.EndHorizontal();

        bool _toggle_changed = false;
        m_posScroll = EditorGUILayout.BeginScrollView( m_posScroll, GUI.skin.box );
        for (int i = 0, _max = m_listSkipData.Count; i < _max; ++i)
        {
            EditorGUILayout.BeginHorizontal( GUI.skin.box );

            var _prev_color = GUI.color;
            if( m_listNeedIncludingAssets.Contains(m_listSkipData[i].fileName) == true ){
                GUI.color = Color.red;
            }
 
            var _asset_name   = string.Format("{0}",m_listSkipData[i].fileName );

            bool _prev = m_listSkipData[i].isSkip;
            m_listSkipData[ i ].isSkip = GUILayout.Toggle( m_listSkipData[ i ].isSkip, _asset_name, GUILayout.Width( 400 ) );
            if( _prev != m_listSkipData[i].isSkip ){
                _toggle_changed = true;
            }

            GUI.color = _prev_color;

            EditorGUILayout.EndHorizontal();
        }

        if( _toggle_changed == true )
        {
            //m_listSkipData.Sort(delegate(SkipData x, SkipData y){
            //    return y.isSkip.CompareTo( x.isSkip );
            //});

            m_nSkipCount = m_listSkipData.FindAll(r => r.isSkip == true).Count;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        //
        GUI.enabled = m_targetVersion > 0;

        var _style = GUI.skin.button;
        _style.richText = true;
        if( GUILayout.Button( string.Format( "Copy AssetBundles ( <color=yellow>Ver.{0}</color> ) To Local [ <color=red>Skip</color> Files : <color=yellow>{1}</color> ]", m_targetVersion, m_nSkipCount), _style) == true )
        {
            var _list_skipdata = m_listSkipData.FindAll(r => r.isSkip == true);

            MakeSkipLists( _list_skipdata );
            CopyAssetBundleFiles( m_targetServer, m_targetVersion, _list_skipdata );

            //
            var _from_file = GetSkipFilePath( EditorUserBuildSettings.activeBuildTarget );
            string _to_file = Path.GetFullPath( Path.Combine( "Assets/Resources", string.Format( "{0}.txt", AssetManager.SKIPLIST_FILENAME ) ) );
            if (File.Exists(_to_file) == true){
                FileUtil.DeleteFileOrDirectory( _to_file );
            }

            if( _list_skipdata.Count > 0 )
            {
                FileUtil.CopyFileOrDirectory( _from_file, _to_file );
            }
            //

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Copy Process Complete");
            Close();
        }

        GUI.enabled = true;
    }

    void MakeSkipLists( List<SkipData> list_skipdata )
    {
        if( list_skipdata.Count == 0 ){
            return;
        }

        string _file_path = GetSkipFilePath( EditorUserBuildSettings.activeBuildTarget );

        using( FileStream fs = new FileStream( _file_path, FileMode.Create ) )
        {
            using( StreamWriter writer = new StreamWriter( fs ) )
            {
                for( int i = 0, _max = list_skipdata.Count; i < _max; ++i )
                {
                    writer.WriteLine( Path.GetFileNameWithoutExtension( list_skipdata[ i ].fileName ) );
                }
            }
        }

        Debug.Log( "Make Skip List!!" );
    }

    void CopyAssetBundleFiles( AssetDefines.ServerEnum serverType, int targetVersion, List<SkipData> list_skipdata )
    {
        var _target_os_type = AssetDefines.GetOSType( EditorUserBuildSettings.activeBuildTarget );

        string _local_target_dir = Path.Combine( Application.streamingAssetsPath, _target_os_type.ToString() );
        if( Directory.Exists( _local_target_dir ) == false ){
            Directory.CreateDirectory( _local_target_dir );
        }

        string[] _prevFiles = Directory.GetFiles( _local_target_dir );
        for( int i = 0, _max = _prevFiles.Length; i < _max; ++i )
        {
            if( Path.GetExtension(_prevFiles[i]) == ".res" )
                continue;
            
            FileUtil.DeleteFileOrDirectory( _prevFiles[ i ] );
        }

        var _root = AssetBundleEditorUtil.GetAssetOutputPath( EditorUserBuildSettings.activeBuildTarget );
        var _version_path = AssetBundleEditorUtil.GetVersionListPath( EditorUserBuildSettings.activeBuildTarget, serverType, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );

        List< AssetVersionData > listAssetData = AssetVersions.ReadAssetVersionFile( _version_path, targetVersion );
        for( int i = 0, _max = listAssetData.Count; i < _max; ++i )
        {
            string _file_name = listAssetData[i].filename;
            if (list_skipdata.Find(r => r.fileName == _file_name) == null)
            {
                string _target_path = Path.Combine(_root, listAssetData[i].version.ToString());

                var _from_file = Path.GetFullPath(Path.Combine(_target_path, _file_name));
                var _to_file = Path.GetFullPath(Path.Combine(_local_target_dir, _file_name));
                FileUtil.CopyFileOrDirectory(_from_file, _to_file);
            }
   
            string _display = string.Format( "{0}/{1} : {2}", i, _max + 1, listAssetData[ i ].filename );

            EditorUtility.DisplayProgressBar( "Copy Bundles..", _display, ( float )i / ( _max + 1 ) );
        }

        string _version_file = string.Format( "{0}{1}", targetVersion, AssetVersions.FILE_EXTENTION );
        FileUtil.CopyFileOrDirectory(
            Path.Combine( _version_path, _version_file ), Path.Combine( _local_target_dir, _version_file ) );

        EditorUtility.ClearProgressBar();

        WriteAttachedVersion.WriteAssetVersion(_target_os_type, targetVersion);
       
        Debug.Log( "Copy Completed!!" );
    }

    bool SearchVersionLists( BuildTarget target )
    {
        var _list = AssetBundleEditorUtil.ReadVersionList( target, m_targetServer);
        if( _list.Count > 0 )
        {
            VERSION_LIST = new string[ _list.Count ];
            for (int i = 0, _max = _list.Count; i < _max; ++i)
            {
                VERSION_LIST[i] = _list[i].ToString();
            }

            m_targetVersionIndex = _list.Count-1;
            m_targetVersion = _list[m_targetVersionIndex];

            return true;
        }
        else
        {
            VERSION_LIST = new string[ 1 ]{ string.Empty };
            m_targetVersionIndex = 0;
            m_targetVersion      = 0;

            return false;
        }
    }

    void LoadSkipList()
    {
        List<string> _list_skip_files = new List<string>();
        GetSkipFileList( ref _list_skip_files, EditorUserBuildSettings.activeBuildTarget );

        m_listSkipData.Clear();
        m_posScroll = Vector2.zero;

        string _root = AssetBundleEditorUtil.GetVersionListPath( EditorUserBuildSettings.activeBuildTarget, m_targetServer, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );
        List<AssetVersionData> _list = AssetVersions.ReadAssetVersionFile( _root, m_targetVersion );
        for (int i = 0, _max = _list.Count; i < _max; ++i)
        {
            var _data = new SkipData();
            _data.fileName = _list[i].filename;
            _data.isSkip = _list_skip_files.Contains( Path.GetFileNameWithoutExtension( _data.fileName ) );

            m_listSkipData.Add(_data);
        }

        m_listSkipData.Sort(delegate(SkipData x, SkipData y){

            if( y.isSkip == x.isSkip ){
                return x.fileName.CompareTo( y.fileName );
            }
            else{
                return y.isSkip.CompareTo( x.isSkip );
            }
        });

        m_nSkipCount = m_listSkipData.FindAll(r => r.isSkip == true).Count;

        //
        m_listNeedIncludingAssets.Clear();
        if (System.IO.File.Exists("attach_assetlist.txt") == true)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream("attach_assetlist.txt", System.IO.FileMode.Open))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        m_listNeedIncludingAssets.Add(line);
                    }
                }
            }
        }

    }

    static bool GetSkipFileList( ref List<string> list_skip, BuildTarget target )
    {
        bool _re = false;
        string _full_path = GetSkipFilePath( target );
        if( File.Exists(_full_path) == true )
        {
            using (FileStream fs = new FileStream(_full_path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    while (true)
                    {
                        string _data = reader.ReadLine();
                        if (_data == null)
                            break;

                        list_skip.Add(_data);
                    }

                    _re = true;
                }
            }
        }

        return _re;
    }

    static string GetSkipFilePath( BuildTarget target )
    {
        string _root = AssetBundleEditorUtil.GetAssetOutputPath( target );

        string _path = Path.Combine( _root, string.Format( "{0}.txt", AssetManager.SKIPLIST_FILENAME ));
        return Path.GetFullPath(_path);
    }

   
    public static void ShowCopyAssetBundles( System.Type type, AssetDefines.ServerEnum server_type )
    {
        var _editor = EditorWindow.GetWindow(type) as BaseAssetBundleCopyEditor;

        _editor.SetServerType(server_type);
        _editor.LoadSkipList();

    }

    public static void CopyAssetBundles( AssetDefines.ServerEnum server_type, bool skip_write_attachedversion = false, bool use_skiplist = true )
    {
        var _build_target = EditorUserBuildSettings.activeBuildTarget;
        var _list_versions = AssetBundleEditorUtil.ReadVersionList( _build_target, server_type);
        if (_list_versions.Count == 0)
            return;

        int _target_version = _list_versions[_list_versions.Count - 1];

        List<string> _list_skip_files = new List<string>();
        if( use_skiplist == true ){
            GetSkipFileList( ref _list_skip_files, _build_target );
        }

        string _version_path = AssetBundleEditorUtil.GetVersionListPath( _build_target, server_type, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );
        List<AssetVersionData> _listAssetData = AssetVersions.ReadAssetVersionFile( _version_path, _target_version );

        // clear platform streaming folder
        var _target_os_type = AssetDefines.GetOSType( _build_target );
        string _local_target_dir = Path.Combine( Application.streamingAssetsPath, _target_os_type.ToString() );
        if( Directory.Exists( _local_target_dir ) == false ){
            Directory.CreateDirectory( _local_target_dir );
        }
             
        string[] _prevFiles = Directory.GetFiles( _local_target_dir );
        for( int i = 0, _max = _prevFiles.Length; i < _max; ++i )
        {
            if( Path.GetExtension(_prevFiles[i]) == ".res" )
                continue;

            FileUtil.DeleteFileOrDirectory( _prevFiles[ i ] );
        }

        // copy asset bundles from platform asset bundle path
        var _platform_asset_path = AssetBundleEditorUtil.GetAssetOutputPath( _build_target );

        for( int i = 0, _max = _listAssetData.Count; i < _max; ++i )
        {
            string _file_name = _listAssetData[i].filename;

            if( _list_skip_files.Contains( Path.GetFileNameWithoutExtension( _file_name)) == false)
            {
                string _target_path = Path.Combine(_platform_asset_path, _listAssetData[i].version.ToString());

                var _from_file = Path.GetFullPath(Path.Combine(_target_path, _file_name));
                var _to_file   = Path.GetFullPath(Path.Combine(_local_target_dir, _file_name));
                FileUtil.CopyFileOrDirectory(_from_file, _to_file);
            }

            string _display = string.Format( "{0}/{1} : {2}", i, _max + 1, _listAssetData[ i ].filename );
            EditorUtility.DisplayProgressBar( "Copy Bundles..", _display, ( float )i / ( _max + 1 ) );
        }

        string _version_file = string.Format( "{0}{1}", _target_version, AssetVersions.FILE_EXTENTION );
        FileUtil.CopyFileOrDirectory(
            Path.Combine( _version_path, _version_file ), Path.Combine( _local_target_dir, _version_file ) );

        EditorUtility.ClearProgressBar();

        if( skip_write_attachedversion == false)
        {
            WriteAttachedVersion.WriteAssetVersion(_target_os_type, _target_version);
        }

        Debug.Log( "Copy Completed!!" );

        // copy skip file list
        string _skiplist_to_file = Path.GetFullPath( Path.Combine( "Assets/Resources", string.Format( "{0}.txt", AssetManager.SKIPLIST_FILENAME ) ) );
        if (File.Exists(_skiplist_to_file) == true){
            FileUtil.DeleteFileOrDirectory( _skiplist_to_file );
        }

        var _skiplist_from_file = GetSkipFilePath( _build_target );
        if (File.Exists(_skiplist_from_file) == true){
            FileUtil.CopyFileOrDirectory(_skiplist_from_file, _skiplist_to_file);
        }

     //   AssetDatabase.SaveAssets();
     //   AssetDatabase.Refresh();

    }

    //static public void MakeSkipAssetLits( string filepath )
    //{
    //    int _asset_version;
    //    int _res_version;
    //
    //    PnixCommon.eOSType _type = PnixCommon.ConvertTarget.ToOSType(EditorUserBuildSettings.activeBuildTarget);
    //
    //    VersionManager.ReadAttachedVersion( _type, out _res_version, out _asset_version );
    //
    //    if( _asset_version <= 0 )
    //    {
    //        Debug.LogError( "Not Exist Attached Version" );
    //        return;
    //    }
    //
    //
    //    string _local_target_dir = Path.Combine( Application.streamingAssetsPath, _type.ToString() );
    //    var _list = AssetVersions.ReadAssetVersionFile( _local_target_dir, _asset_version );
    //
    //    using( FileStream fs = new FileStream( filepath, FileMode.Create ) )
    //    {
    //        using( StreamWriter writer = new StreamWriter( fs ) )
    //        {
    //            for( int i = 0, _max = _list.Count; i < _max; ++i )
    //            {
    //                var _file_path = Path.Combine( _local_target_dir, _list[ i ].filename );
    //                if( File.Exists( _file_path ) == false )
    //                {
    //                    writer.WriteLine( Path.GetFileNameWithoutExtension( _list[ i ].filename ) );
    //                }
    //            }
    //        }
    //    }
    //
    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //    Debug.Log( "SkipAsset List Complete" );
    //}
}
