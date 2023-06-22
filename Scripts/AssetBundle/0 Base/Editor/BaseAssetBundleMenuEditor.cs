using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

public class BaseAssetBundleMenuEditor : EditorWindow
{
    public enum StateType
    {
        NoChanges,
        Added,
        Modified,
    }

    protected Dictionary<AssetDefines.MakeAssetMenuEnum, string[]> m_dicTypePath = new Dictionary<AssetDefines.MakeAssetMenuEnum, string[]>();
    protected Dictionary<AssetDefines.MakeAssetMenuEnum, string > m_dicOutputNameFormat = new Dictionary<AssetDefines.MakeAssetMenuEnum, string>();

    protected List<AssetDefines.MakeAssetMenuEnum> m_listNeedLookupTable = new List<AssetDefines.MakeAssetMenuEnum>();
    protected Dictionary<AssetDefines.MakeAssetMenuEnum, string> m_dicDependencyNotCheckKeyword = new Dictionary<AssetDefines.MakeAssetMenuEnum, string>();

    AssetDefines.MakeAssetMenuEnum m_assetType = AssetDefines.MakeAssetMenuEnum.None;
    bool m_isAll                      = false;
    int m_selectedCount = 0;

    protected class AssetState
    {
        public string assetName;
        public AssetFilesData assetFileData;

        public bool isChecked;
        public StateType stateType;
    }
    protected List<AssetState> m_listAssets = new List<AssetState>();

    protected class DataAsServer
    {
        public AssetDefines.ServerEnum serverType;
        public int[] versions;
        public int currVersion;

        public Vector2 scrollPos;
        public List<AssetVersionData> listVersionData;

        public int MinVersion
        {
            get
            {
                if( versions != null){
                    return versions[0];
                }

                return 1;
            }
        }

        public int MaxVersion
        {
            get
            {
                if( versions != null ){
                    return versions[versions.Length - 1];
                }

                return 1;
            }
        }

        public int NextVersion
        {
            get
            {
                if( versions != null ){
                    return versions[versions.Length - 1]+1;
                }

                return 1;
            }
        }

        //
        public void Clear()
        {
            Clear(AssetDefines.ServerEnum.None);
        }

        public void Clear( AssetDefines.ServerEnum type )
        {
            serverType = type;
           
            versions = null;
            scrollPos = Vector2.zero;

            if (listVersionData != null)
            {
                listVersionData.Clear();
                listVersionData = null;
            }
        }

        public bool IsModifyState()
        {
            int _min = MinVersion;
            return currVersion == _min && _min != MaxVersion;
        }
    }

    static DataAsServer m_FromData   = new DataAsServer();
    protected static DataAsServer m_TargetData = new DataAsServer();

    class AssetVersionItem
    {
        public AssetVersionData assetVersionData;
        public bool isChecked;
        public StateType stateType;
    }
    List<AssetVersionItem> m_listFromAssetVersions = new List<AssetVersionItem>();
    List<AssetVersionItem> m_listToAssetVersions   = new List<AssetVersionItem>();


    string[] SERVER_NAMES;
    System.Reflection.MethodInfo    m_methodName2Type = null;
    string[] MENU_NAMES;
    System.Reflection.MethodInfo    m_methodName2MenuType = null;
    System.Reflection.MethodInfo    m_methodInt2MenuType = null;


    //
    public BaseAssetBundleMenuEditor()
    {
    }

    public virtual void Initialize( System.Type menu_type, AssetDefines.MakeAssetMenuEnum type )
    {
        m_dicTypePath.Clear();
        m_listNeedLookupTable.Clear();
        m_dicDependencyNotCheckKeyword.Clear();
        
        m_TargetData.Clear();
        m_FromData.Clear();

        m_assetType = type;

        //
        m_methodName2MenuType = menu_type.GetMethod( "op_Implicit", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy, 
            null, System.Reflection.CallingConventions.Any, new System.Type[]{typeof(string)}, null );

        m_methodInt2MenuType = menu_type.GetMethod( "op_Implicit", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy, 
            null, System.Reflection.CallingConventions.Any, new System.Type[]{typeof(int)}, null );
            
        var _methodInfo = menu_type.GetMethod("GetNames", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy );
        if (_methodInfo != null)
        {
            var _names = (string[])_methodInfo.Invoke(null, new object[] {menu_type});

            MENU_NAMES = new string[ _names.Length ];
            for (int i = 0, _max = _names.Length; i < _max; ++i){
                MENU_NAMES[i] = _names[i];
            }
        }
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

    void OnDisable()
    {
        AssetBundleMaker.InitializeData();
    }

    void InitData()
    {
        m_assetType = AssetDefines.MakeAssetMenuEnum.None;
        m_isAll = false;
        m_selectedCount = 0;
    }

    protected void OnGuiDraw_First()
    {
        GUI.enabled = false;
        GUILayout.Label(string.Format("Base Asset Path : {0}/",  AssetBundleEditorUtil.COMMON_PATH ));
        GUI.enabled = true;

        bool _reload = true;
        int _index = 0;
        if (m_TargetData.serverType != null)
        {
            var _previndex = m_TargetData.serverType.NameIndex;
            _index = EditorGUILayout.Popup("Target Server : ", m_TargetData.serverType.NameIndex, SERVER_NAMES);
            _reload = _previndex != _index;
        }
               
        if( _reload == true )
        {
            m_TargetData.serverType = (AssetDefines.ServerEnum)m_methodName2Type.Invoke(null, new object[]{ SERVER_NAMES[_index] });

            InitData();

            m_TargetData.Clear(m_TargetData.serverType);

            UpdateServerData( m_TargetData.serverType );
        }
    }

    protected void OnGuiDraw_MakeAssetVersionReal()
    {
        var _style_tex_field = GUI.skin.textField;
        _style_tex_field.richText = true;

        EditorGUILayout.BeginVertical( GUI.skin.box );

            EditorGUILayout.BeginHorizontal();
            
                GUILayout.Label(string.Format("From <color=yellow>{0}</color> ", m_FromData.serverType), _style_tex_field);
                
                string _version_text = string.Format( "Asset Version : <color=yellow>{0}</color>", m_FromData.currVersion );
                EditorGUILayout.LabelField( _version_text, _style_tex_field );
            
            EditorGUILayout.EndHorizontal();

        m_FromData.scrollPos = EditorGUILayout.BeginScrollView( m_FromData.scrollPos, GUI.skin.box );

        if (m_FromData.listVersionData != null)
        {
            GUILayout.Label("From : ");

            for (int i = 0, _max = m_FromData.listVersionData.Count; i < _max; ++i)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                EditorGUILayout.LabelField( m_FromData.listVersionData[i].filename, _style_tex_field );
  
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Label("To : ");

            var _file_path = AssetBundleEditorUtil.GetVersionListPath(EditorUserBuildSettings.activeBuildTarget, m_TargetData.serverType, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME);
            for (int i = 0, _max = m_FromData.listVersionData.Count; i < _max; ++i)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                EditorGUILayout.LabelField( Path.Combine( _file_path, Path.GetFileName( m_FromData.listVersionData[i].filename ) ), _style_tex_field );

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("No Data to Patch!!");
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        GUI.enabled = m_FromData.listVersionData != null && m_FromData.listVersionData.Count > 0;
        if( GUILayout.Button(string.Format("Copy Version List Review to Real")) == true )  
        {
            var _target_dir = AssetBundleEditorUtil.GetVersionListPath(EditorUserBuildSettings.activeBuildTarget, m_TargetData.serverType, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME);
            if( Directory.Exists( _target_dir ) == false ){
                Directory.CreateDirectory( _target_dir );
            }

            for (int i = 0, _max = m_FromData.listVersionData.Count; i < _max; ++i)
            {
                var _to_file = Path.GetFullPath( Path.Combine( _target_dir, Path.GetFileName(  m_FromData.listVersionData[i].filename) ) );
                FileUtil.CopyFileOrDirectory( m_FromData.listVersionData[i].filename, _to_file );
            }

            Debug.Log("Completed to Copy Asset Versions File !!");

            Close();
        }
        GUI.enabled = true;
    }

    protected void OnGuiDraw_MakeAssetVersion()
    {
        var _style_tex_field = GUI.skin.textField;
        _style_tex_field.richText = true;

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical( GUI.skin.box );

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(string.Format("From <color=yellow>{0}</color> ", m_FromData.serverType), _style_tex_field);

        string _version_text = string.Format( "Asset Version : <color=yellow>{0}</color>", m_FromData.currVersion );
        EditorGUILayout.LabelField( _version_text, _style_tex_field );

        EditorGUILayout.EndHorizontal();

        bool _toggle_changed = false;
        m_FromData.scrollPos = EditorGUILayout.BeginScrollView( m_FromData.scrollPos, GUI.skin.box );
        for( int i = 0, _max = m_listFromAssetVersions.Count; i < _max; ++i )
        {
            EditorGUILayout.BeginHorizontal( GUI.skin.box );

            var _version_data = m_listFromAssetVersions[i].assetVersionData;
            var _asset_name   = string.Format("{0}/{1} [ revision:{2} ]",_version_data.version,  _version_data.filename, _version_data.revision );

            var _prev_color = GUI.color;
            switch( m_listFromAssetVersions[i].stateType )
            {
                case StateType.Added:
                    GUI.color = Color.yellow;
                    break;
                case StateType.Modified:
                    GUI.color = Color.green;
                    break;
                default:
                    GUI.enabled = false;
                    break;
            }

            bool _prev = m_listFromAssetVersions[i].isChecked;
            m_listFromAssetVersions[ i ].isChecked = GUILayout.Toggle( m_listFromAssetVersions[ i ].isChecked, _asset_name, GUILayout.Width( 400 ) );
            if( _prev != m_listFromAssetVersions[i].isChecked ){
                _toggle_changed = true;
            }

            GUI.enabled = true;
            GUI.color   = _prev_color;

            EditorGUILayout.EndHorizontal();
        }

        if( _toggle_changed == true )
        {
          //m_isAll = m_listFromAssetVersions.FindAll( r => r.isChecked == true ).Count == m_listFromAssetVersions.Count;
            m_isAll = m_listFromAssetVersions.FindAll( r => r.isChecked == true ).Count == m_listFromAssetVersions.FindAll( r => r.stateType != StateType.NoChanges ).Count;
            MakeVersionCopyList(ref m_listToAssetVersions, m_TargetData.listVersionData, m_listFromAssetVersions);

            m_listFromAssetVersions.Sort( delegate(AssetVersionItem x, AssetVersionItem y){
                if( x.isChecked == y.isChecked )
                {
                    if(x.stateType == y.stateType)
                    {
                        if( x.assetVersionData.version == y.assetVersionData.version ){
                            return x.assetVersionData.filename.CompareTo( y.assetVersionData.filename );
                        }
                        else{
                            return y.assetVersionData.version.CompareTo( x.assetVersionData.version );
                        }
                    }
                    else{
                         return y.stateType.CompareTo( x.stateType); 
                    }
                }
                else{
                    return y.isChecked.CompareTo( x.isChecked );
                }
            });
        }

        EditorGUILayout.EndScrollView();

        bool prevAll = m_isAll;
        m_isAll = GUILayout.Toggle( m_isAll, "All" );
        if( prevAll != m_isAll )
        {
            m_listFromAssetVersions.ForEach( delegate(AssetVersionItem item){ if( item.stateType != StateType.NoChanges)  item.isChecked = m_isAll; } );
            MakeVersionCopyList(ref m_listToAssetVersions, m_TargetData.listVersionData, m_listFromAssetVersions);

            m_listFromAssetVersions.Sort( delegate(AssetVersionItem x, AssetVersionItem y){
                if( x.isChecked == y.isChecked )
                {
                    if(x.stateType == y.stateType)
                    {
                        if( x.assetVersionData.version == y.assetVersionData.version ){
                            return x.assetVersionData.filename.CompareTo( y.assetVersionData.filename );
                        }
                        else{
                            return y.assetVersionData.version.CompareTo( x.assetVersionData.version );
                        }
                    }
                    else{
                         return y.stateType.CompareTo( x.stateType); 
                    }
                }
                else{
                    return y.isChecked.CompareTo( x.isChecked );
                }
            });
        }

        EditorGUILayout.EndVertical();

        //
        EditorGUILayout.BeginVertical( GUI.skin.box );

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(string.Format("To <color=yellow>{0}</color> ", m_TargetData.serverType), _style_tex_field);

        if (m_TargetData.currVersion == m_TargetData.NextVersion){
            _version_text = string.Format( "Asset Version : <color=red>{0}</color>", m_TargetData.currVersion );
        }
        else{
            _version_text = string.Format( "Asset Version : <color=yellow>{0}</color>", m_TargetData.currVersion );
        }
        EditorGUILayout.LabelField( _version_text, _style_tex_field );

        GUI.enabled = m_TargetData.currVersion > m_TargetData.MaxVersion;
        if( GUILayout.Button( "<<" ) == true ){
            m_TargetData.currVersion = m_TargetData.MaxVersion;
        }
        GUI.enabled = m_TargetData.currVersion < m_TargetData.NextVersion;
        if( GUILayout.Button( ">>" ) == true ){
            m_TargetData.currVersion = m_TargetData.NextVersion;
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        m_TargetData.scrollPos = EditorGUILayout.BeginScrollView( m_TargetData.scrollPos, GUI.skin.box );
        for (int i = 0, _max = m_listToAssetVersions.Count; i < _max; ++i)
        {
            EditorGUILayout.BeginHorizontal( GUI.skin.box );

            var _version_data = m_listToAssetVersions[i].assetVersionData;
            var _asset_name   = string.Format("{0}/{1} [ revision:{2} ]", _version_data.version, _version_data.filename, _version_data.revision );

            GUILayout.Label(_asset_name , _style_tex_field,  GUILayout.Width( 400 ) );

            string _state;
            switch( m_listToAssetVersions[ i ].stateType )
            {
                case StateType.Added:
                    _state = string.Format( "<color=yellow>{0}</color>", m_listToAssetVersions[ i ].stateType);
                    break;
                case StateType.Modified:
                    _state = string.Format( "<color=green>{0}</color>", m_listToAssetVersions[ i ].stateType );
                    break;
                default:
                    _state = string.Format("{0}", m_listToAssetVersions[i].stateType);
                    break;
            }
            GUILayout.Label( _state, _style_tex_field);

            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        var _style = GUI.skin.button;
        _style.richText = true;

        int _modified_count = 0;
        for (int i = 0, _max = m_listToAssetVersions.Count; i < _max; ++i)
        {
            if (m_listToAssetVersions[i].stateType != StateType.NoChanges)
            {
                _modified_count++;
            }
        }

        GUI.enabled = _modified_count > 0;
        if (GUILayout.Button( string.Format( "Make Version List for <color=yellow>{0}</color> , Changed Count : <color=yellow>{1}</color> ", m_TargetData.serverType, _modified_count ), _style) == true )
        {
            System.DateTime now = System.DateTime.Now;

            BuildTarget _build_target = EditorUserBuildSettings.activeBuildTarget;

            string _versionlist_path  = AssetBundleEditorUtil.GetVersionListPath( _build_target, m_TargetData.serverType, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );

            List<AssetFilesData> _listTarget = new List<AssetFilesData>();
          
            var _list_target = m_listToAssetVersions.FindAll(r => r.stateType != StateType.NoChanges);
            for (int i = 0, _max = _list_target.Count; i < _max; ++i)
            {
                var _new = new AssetFilesData();
                _new.revisionNumber = _list_target[i].assetVersionData.revision;
                _new.outputFileName = _list_target[i].assetVersionData.filename;
                _new.folderNumber   = _list_target[i].assetVersionData.version;
#if UNITY_2017_1_OR_NEWER
                _new.hashResult     = _list_target[i].assetVersionData.hash;
#endif
                if( string.IsNullOrEmpty(_list_target[i].assetVersionData.lookuptable) == false )
                {
                    _new.bMakeLookupTable = true;

                    string[] _split = _list_target[i].assetVersionData.lookuptable.Split(AssetIOUtil.SEPERATOR_LOOKUPTABLE[0]);
                    if (_split.Length > 0)
                    {
                        _new.listSourceFiles = new List<string>();
                        for (int k = 0; k < _split.Length; ++k){
                            _new.listSourceFiles.Add(_split[k]);
                        }
                    }
                }
   
                _listTarget.Add(_new);
            }

            List<AssetVersionData> _list_prev_versions = null;
            int _prev_version = AssetBundleEditorUtil.GetPreviosVersion( _build_target, m_TargetData.serverType, m_TargetData.currVersion );
            if (_prev_version > 0) {
                _list_prev_versions = AssetVersions.ReadAssetVersionFile(_versionlist_path, _prev_version);
            }
          
            var _instance = AssetBundleMaker.GetInstance( _build_target );
            _instance.MakeVersionListFile( _versionlist_path, m_TargetData.currVersion, _listTarget, _list_prev_versions, true );

            Debug.LogError( string.Format( "AssetList Build Time : {0}m {1}s", ( System.DateTime.Now - now ).Minutes, ( System.DateTime.Now - now ).Seconds ) );

            Close();

            //
            ShowCopyEditor( m_TargetData.serverType );
        }
        GUI.enabled = true;
    }

    protected void OnGuiDraw_MakeAssetsAndAssetVersion()
    {
        EditorGUILayout.BeginHorizontal();

        var _style_tex_field = GUI.skin.textField;
        _style_tex_field.richText = true;

        string _version_text;
        if( m_TargetData.IsModifyState() ){
            _version_text = string.Format( "Target Asset Version : <color=yellow>{0}</color>", m_TargetData.currVersion );
        }
        else{
            _version_text = string.Format( "Target Asset Version : <color=red>{0}</color>", m_TargetData.currVersion );
        }
        EditorGUILayout.LabelField( _version_text, _style_tex_field );

        GUI.enabled = m_TargetData.currVersion > m_TargetData.MaxVersion;
        if( GUILayout.Button( "<<" ) == true ){
            m_TargetData.currVersion = m_TargetData.MaxVersion;
        }
        GUI.enabled = true;

        GUI.enabled = m_TargetData.currVersion < m_TargetData.NextVersion;
        if( GUILayout.Button( ">>" ) == true ){
            m_TargetData.currVersion = m_TargetData.NextVersion;
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        var _prev_type   = m_assetType.NameIndex;
        int _menu_index  = EditorGUILayout.Popup("Target Asset Type :", m_assetType.NameIndex, MENU_NAMES);
        if( _prev_type != _menu_index && m_methodName2MenuType != null )
        {
            m_assetType = (AssetDefines.MakeAssetMenuEnum)m_methodName2MenuType.Invoke(null, new object[]{ MENU_NAMES[_menu_index] });
            FindAssetLists( m_assetType, m_TargetData.listVersionData );
        }

        EditorGUILayout.BeginVertical( GUI.skin.box );
        m_TargetData.scrollPos = EditorGUILayout.BeginScrollView( m_TargetData.scrollPos, GUI.skin.box );

        var _style = GUI.skin.toggle;
        _style.richText = true;

        Color _prev_color = GUI.color;
        int modifyCount = 0;
        int addCount = 0;
        bool _toggle_changed = false;
        for (int i = 0, _max = m_listAssets.Count; i < _max; ++i)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            var _menu_type = (AssetDefines.MakeAssetMenuEnum)m_methodInt2MenuType.Invoke(null, new object[] { m_listAssets[i].assetFileData.buildMenutype });
            var _asset_name = string.Format("<b>[ {0} ]</b> {1}", _menu_type.ToString(), m_listAssets[i].assetName );

            GUI.enabled = m_listAssets[i].stateType != StateType.NoChanges;
            var _prev = m_listAssets[i].isChecked;
            m_listAssets[i].isChecked = GUILayout.Toggle(m_listAssets[i].isChecked, _asset_name, _style, GUILayout.Width(400));
            if (_prev != m_listAssets[i].isChecked) {
                _toggle_changed = true;
            }
            GUI.enabled = true;

            switch (m_listAssets[i].stateType)
            {
                case StateType.Added:
                    GUI.color = Color.yellow;
                    ++addCount;
                    break;
                case StateType.Modified:
                    GUI.color = Color.green;
                    ++modifyCount;
                    break;
            }

            EditorGUILayout.LabelField(string.Format("{0} ( {1} )",  m_listAssets[ i ].stateType, m_listAssets[i].assetFileData.revisionNumber) );
            GUI.color = _prev_color;

            EditorGUILayout.EndHorizontal();
        }

        if( _toggle_changed == true )
        {
            m_selectedCount = m_listAssets.FindAll( r => r.isChecked == true ).Count;
         // m_isAll = m_selectedCount == m_listAssets.Count;
            m_isAll = m_selectedCount == m_listAssets.FindAll( r => r.stateType != StateType.NoChanges ).Count;
        }


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        bool prevAll = m_isAll;
        m_isAll      = GUILayout.Toggle( m_isAll, "All" );
        if( prevAll != m_isAll )
        {
           // m_selectedCount = (m_isAll == false) ? 0 : m_listAssets.Count;
           // m_listAssets.ForEach( r=>r.isChecked = m_isAll);
            m_listAssets.ForEach( delegate(AssetState item){ if( item.stateType != StateType.NoChanges)  item.isChecked = m_isAll; } );
            m_selectedCount = (m_isAll == false) ? 0 : m_listAssets.FindAll( r => r.isChecked == m_isAll ).Count;
        }

        GUI.color = _prev_color;
        GUILayout.Label( "Selected Counts : " + m_selectedCount );

        GUILayout.Label( "Asset File Count : " + m_listAssets.Count );

        GUI.color = Color.green;
        GUILayout.Label( "Modify File Count : " + modifyCount );
        GUI.color = Color.yellow;
        GUILayout.Label( "Add File Count : " + addCount );

        GUI.color = _prev_color;

        EditorGUILayout.EndHorizontal();


        GUI.enabled = m_selectedCount > 0;

        string _btn_format;
        if( m_TargetData.IsModifyState() ){
            _btn_format = string.Format( "<color=#ffff00ff>Modify</color> Current Version( <color=yellow>Ver.{0}</color> ) Bundles", m_TargetData.currVersion );
        }
        else{
            _btn_format = string.Format( "Make <color=#ff0000ff>New</color> Version( <color=red>Ver.{0}</color> ) Bundles", m_TargetData.currVersion );
        }

        var _style_btn = GUI.skin.button;
        _style_btn.richText = true;
        if( GUILayout.Button( _btn_format, _style_btn ) == true && m_assetType != AssetDefines.MakeAssetMenuEnum.None )
        {
            List<AssetFilesData> _listTarget = new List<AssetFilesData>();
            for( int i = 0, _max = m_listAssets.Count; i < _max; ++i )
            {
                if( m_listAssets[ i ].isChecked == true ){
                    _listTarget.Add( m_listAssets[ i ].assetFileData );
                }
            }

            BuildTarget _build_target = EditorUserBuildSettings.activeBuildTarget;

            string _versionlist_path  = AssetBundleEditorUtil.GetVersionListPath( _build_target, m_TargetData.serverType, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME);

            List<AssetVersionData> _list_prev_versions = null;
            int _prev_version = AssetBundleEditorUtil.GetPreviosVersion( _build_target, m_TargetData.serverType, m_TargetData.currVersion );
            if( _prev_version > 0 ){
                 _list_prev_versions = AssetVersions.ReadAssetVersionFile( _versionlist_path, _prev_version );
            }

            System.DateTime now = System.DateTime.Now;

            string _output_path = AssetBundleEditorUtil.GetAssetOutputPath( _build_target, m_TargetData.currVersion );

            var _instance = AssetBundleMaker.GetInstance( _build_target );
            _instance.MakeAssetBundles( _output_path, _listTarget, OnPrevMakeBundle, OnPostMakeBundle );
            _instance.MakeVersionListFile( _versionlist_path, m_TargetData.currVersion, _listTarget, _list_prev_versions, false );

            Debug.LogError( string.Format( "Assetbundle Build Time : {0}m {1}s", ( System.DateTime.Now - now ).Minutes, ( System.DateTime.Now - now ).Seconds ) );

            Close();

            //
            ShowCopyEditor( m_TargetData.serverType );
           
        }

        GUI.enabled = true;
    }

    protected virtual void ShowCopyEditor( AssetDefines.ServerEnum server_type )
    {
        BaseAssetBundleCopyEditor.ShowCopyAssetBundles( typeof(BaseAssetBundleCopyEditor), server_type );
    }

    void ReloadVersionData( ref DataAsServer targetData )
    {
        var _list = AssetBundleEditorUtil.ReadVersionList( EditorUserBuildSettings.activeBuildTarget, targetData.serverType );
        if( _list.Count > 0 ){
            targetData.versions = _list.ToArray();
        }

        int maxversion  = targetData.MaxVersion;
        if( maxversion > 0 ){
            targetData.currVersion = maxversion;
        }
        else{
            targetData.currVersion = 1;
        }

        LoadVersionList(ref targetData, maxversion);
    }

    void LoadVersionList( ref DataAsServer targetData, int version )
    {
        string _root = AssetBundleEditorUtil.GetVersionListPath( EditorUserBuildSettings.activeBuildTarget, targetData.serverType, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );
        targetData.listVersionData = AssetVersions.ReadAssetVersionFile( _root, version );
    }

    void FindAssetLists( AssetDefines.MakeAssetMenuEnum type, List<AssetVersionData> listCompareVersion)
    {
        m_listAssets.Clear();

        MakeAssetLists(type, m_listAssets);
       
        UpdateAssetState( listCompareVersion );

        m_selectedCount = m_listAssets.FindAll( r => r.isChecked == true ).Count;
        m_isAll = m_selectedCount > 0 && ( m_selectedCount == m_listAssets.FindAll( r => r.stateType != StateType.NoChanges ).Count );
    }

    protected virtual void MakeAssetLists( AssetDefines.MakeAssetMenuEnum type, List<AssetState> list )
    {
        
    }

    void UpdateAssetState( List<AssetVersionData> listCompareVersion, bool isAllChange = false )
    {
        if( listCompareVersion == null )
            return;

        for( int i = 0, _max = m_listAssets.Count; i < _max; ++i )
        {
            m_listAssets[ i ].stateType = StateType.NoChanges;

            var _target = listCompareVersion.Find( r => r.filename == m_listAssets[ i ].assetName );
            if( _target == null )
            {   // add
                m_listAssets[ i ].stateType = StateType.Added;
            }
            else
            {
                if( _target.revision < m_listAssets[ i ].assetFileData.revisionNumber )
                {
                    m_listAssets[ i ].stateType = StateType.Modified;
                }
            }

            if( isAllChange == false )
                m_listAssets[ i ].isChecked = m_listAssets[ i ].stateType != StateType.NoChanges;
            else
                m_listAssets[ i ].isChecked = m_isAll;
        }

        m_listAssets.Sort(delegate(AssetState x, AssetState y){
            bool _left = x.stateType == StateType.NoChanges;
            bool _right = y.stateType == StateType.NoChanges;
            if( _left == _right )
            {
                return x.assetName.CompareTo( y.assetName );
            }
            else
            {
                return _left.CompareTo( _right);
            }
        });
    }

    protected void AddToAssetStateList( AssetDefines.MakeAssetMenuEnum type, ref List<AssetState> list_target, BaseAssetBundleMaker.FindAssetFiles find_type, AssetFilesData.ScriptExtractionStep step = AssetFilesData.ScriptExtractionStep.None )
    {
        string[] _sub_path;
        if (m_dicTypePath.ContainsKey(type) == true){
            _sub_path = m_dicTypePath[type];
        }
        else{
            _sub_path = new string[1]{ "" };
        }

       // if( string.IsNullOrEmpty( sub_path ) == false )
       //     _source_path = Path.Combine( AssetBundleEditorUtil.COMMON_PATH, sub_path.ToString() );
       // else
       //     _source_path = AssetBundleEditorUtil.COMMON_PATH;
        string _source_path;
        for( int i=0, _max = _sub_path.Length; i < _max; ++i )
        {
            _source_path = Path.Combine( AssetBundleEditorUtil.COMMON_PATH, _sub_path[i] );
            if (Directory.Exists(_source_path) == false)
            {
                Debug.LogError("Not Exist Directory : " + _source_path);
                continue;
            }

            string _skip_keyword = "";
            if (m_dicDependencyNotCheckKeyword.ContainsKey(type) == true)
                _skip_keyword = m_dicDependencyNotCheckKeyword[type];
            
            List<AssetFilesData> _list_buildFiles = AssetBundleMaker.GetAssetBundleRevisions( type, _source_path, find_type, m_listNeedLookupTable.Contains( type ), _skip_keyword);
            for( int k = 0, _maxk = _list_buildFiles.Count; k < _maxk; ++k )
            {
                _list_buildFiles[ k ].step = step;

                string _output_file_name = _list_buildFiles[k].outputFileName;
                if( m_dicOutputNameFormat.ContainsKey(type) == true     &&
                    m_dicOutputNameFormat[type].Contains("{0}") == true )
                {
                    var _name = Path.GetFileNameWithoutExtension(_output_file_name);
                    var _ext  = Path.GetExtension(_output_file_name);
                    _output_file_name = string.Format( m_dicOutputNameFormat[type], _name);
                    _output_file_name += _ext;
                    _list_buildFiles[k].outputFileName = _output_file_name;
                }
            
                list_target.Add( new AssetState()
                {
                    assetName = _output_file_name,
                    assetFileData = _list_buildFiles[k],
                } );
            }
        }
    }

    void MakeVersionCopyList( ref List<AssetVersionItem> result, List<AssetVersionData> prev_data, List<AssetVersionItem> from_data = null, bool auto_add = false )
    {
        result.Clear();

        if( prev_data != null )
        {
            for (int i = 0, _max = prev_data.Count; i < _max; ++i)
            {
                var _new_item = new AssetVersionItem();
                _new_item.stateType = StateType.NoChanges;
                _new_item.isChecked = false;

                var _new_version_data = new AssetVersionData();
                _new_version_data.filename = prev_data[i].filename;
                _new_version_data.revision = prev_data[i].revision;
                _new_version_data.version  = prev_data[i].version;
#if UNITY_2017_1_OR_NEWER
                _new_version_data.hash = prev_data[i].hash;
#endif
                _new_version_data.lookuptable = prev_data[i].lookuptable;

                _new_item.assetVersionData = _new_version_data;

                result.Add( _new_item );
            }
        }

        if( from_data != null )
        {
            for( int i = 0, _max = from_data.Count; i < _max; ++i )
            {
                if (auto_add == false && from_data[i].isChecked == false)
          //    if ( from_data[i].isChecked == false)
                    continue;
                    
                var _version_item = result.Find(r => r.assetVersionData.filename == from_data[i].assetVersionData.filename);
                if( _version_item == null )
                {
                    var _new_item = new AssetVersionItem();
                    _new_item.stateType = StateType.Added;
  
                    var _new_version_data = new AssetVersionData();
                    _new_version_data.filename = from_data[i].assetVersionData.filename;
                    _new_version_data.revision = from_data[i].assetVersionData.revision;
                    _new_version_data.version  = from_data[i].assetVersionData.version;
#if UNITY_2017_1_OR_NEWER
                    _new_version_data.hash = from_data[i].assetVersionData.hash;
#endif
                    _new_version_data.lookuptable = from_data[i].assetVersionData.lookuptable;

                    _new_item.assetVersionData = _new_version_data;

                    result.Add(_new_item);

                    from_data[i].stateType = StateType.Added;
                    from_data[i].isChecked = true;
                }
                else
                { 
                    bool _is_apply = 
                         (_version_item.assetVersionData.revision < from_data[i].assetVersionData.revision || 
                         _version_item.assetVersionData.version < from_data[i].assetVersionData.version   ) ;

#if UNITY_2017_1_OR_NEWER
                    if ( _is_apply == false)
                    {
                        _is_apply = _version_item.assetVersionData.hash != from_data[i].assetVersionData.hash;
                    }
#endif
                    if( _is_apply)
                    {
                        _version_item.stateType = StateType.Modified;
                        _version_item.assetVersionData.revision = from_data[i].assetVersionData.revision;
                        _version_item.assetVersionData.version  = from_data[i].assetVersionData.version;
#if UNITY_2017_1_OR_NEWER
                        _version_item.assetVersionData.hash = from_data[i].assetVersionData.hash;
#endif
                        _version_item.assetVersionData.lookuptable = from_data[i].assetVersionData.lookuptable;

                        from_data[i].stateType = StateType.Modified;
                        from_data[i].isChecked = true;
                    }
                }
            }
        }
       
        result.Sort(delegate(AssetVersionItem x, AssetVersionItem y){
            if( x.stateType == y.stateType )
            {
                if( x.assetVersionData.version == y.assetVersionData.version ){
                    return x.assetVersionData.filename.CompareTo( y.assetVersionData.filename );
                }
                else{
                    return y.assetVersionData.version.CompareTo( x.assetVersionData.version );
                }
            }
            else
            {
                return y.stateType.CompareTo( x.stateType);
            }
        });
    }

    protected virtual void UpdateServerData( AssetDefines.ServerEnum type )
    {
        m_FromData.Clear();
    }

    protected void UpdateServerData_MakeMode()
    {
        m_FromData.Clear();

        ReloadVersionData(ref m_TargetData);
        UpdateAssetState(m_TargetData.listVersionData);

        m_TargetData.currVersion = m_TargetData.NextVersion;
        m_selectedCount = m_listAssets.FindAll( r => r.isChecked == true ).Count;
    }

    protected void UpdateServerData_EditMode( AssetDefines.ServerEnum previous )
    {
        m_FromData.Clear( previous );
        ReloadVersionData( ref m_FromData );
        MakeVersionCopyList( ref m_listFromAssetVersions, m_FromData.listVersionData);

        ReloadVersionData( ref m_TargetData );
        m_TargetData.currVersion = m_TargetData.NextVersion;
        MakeVersionCopyList( ref m_listToAssetVersions, m_TargetData.listVersionData, m_listFromAssetVersions, true);

        m_selectedCount = m_listFromAssetVersions.FindAll( r => r.isChecked == true ).Count;
        m_isAll = m_selectedCount > 0 && m_selectedCount == m_listFromAssetVersions.FindAll( r => r.stateType != StateType.NoChanges ).Count;

        m_listFromAssetVersions.Sort( delegate(AssetVersionItem x, AssetVersionItem y){
            if( x.isChecked == y.isChecked )
            {
                if(x.stateType == y.stateType)
                {
                    if( x.assetVersionData.version == y.assetVersionData.version ){
                        return x.assetVersionData.filename.CompareTo( y.assetVersionData.filename );
                    }
                    else{
                        return y.assetVersionData.version.CompareTo( x.assetVersionData.version );
                    }
                }
                else{
                     return y.stateType.CompareTo( x.stateType); 
                }
            }
            else{
                return y.isChecked.CompareTo( x.isChecked );
            }
        });
    }

    protected void UpdateServerData_CopyMode( AssetDefines.ServerEnum previous, AssetDefines.ServerEnum target )
    {
        m_FromData.Clear(previous);
        var _from_list = AssetBundleEditorUtil.ReadVersionList( EditorUserBuildSettings.activeBuildTarget, previous );
        if( _from_list.Count > 0 )
        {
            _from_list.Sort(delegate(int x, int y){
                return y.CompareTo(x);
            });
            m_FromData.versions = _from_list.ToArray();
            m_FromData.currVersion = _from_list[0];
        }

        m_TargetData.Clear(target);
        var _to_list = AssetBundleEditorUtil.ReadVersionList( EditorUserBuildSettings.activeBuildTarget, target );
        if( _to_list.Count > 0 )
        {
            _to_list.Sort(delegate(int x, int y){
                return y.CompareTo(x);
            });

            m_TargetData.versions = _to_list.ToArray();
        }

        if( m_FromData.versions != null )
        {
            var _file_path = AssetBundleEditorUtil.GetVersionListPath(EditorUserBuildSettings.activeBuildTarget, previous, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME);
            for (int i = 0, _max = m_FromData.versions.Length; i < _max; ++i)
            {
                int _value = m_FromData.versions[i];
                if( m_TargetData.versions == null || m_TargetData.versions.Length <= 0 || 
                    ( System.Array.Exists<int>( m_TargetData.versions, r => r == _value ) == false && m_TargetData.versions[ 0 ] < _value ) )
                {
                    if( m_FromData.listVersionData == null )
                    {
                        m_FromData.listVersionData = new List<AssetVersionData>();
                    }

                    var _data = new AssetVersionData();
                    _data.version = _value;
                    _data.filename = Path.Combine( _file_path, string.Format( "{0}.av", _value ) );
                    m_FromData.listVersionData.Add( _data );
                }
            }
        }
    }

    //
    protected virtual void OnPrevMakeBundle( AssetDefines.MakeAssetMenuEnum menu )
    {
        
    }

    protected virtual void OnPostMakeBundle( AssetDefines.MakeAssetMenuEnum menu )
    {
        
    }
   
}
