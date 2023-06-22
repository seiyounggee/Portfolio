using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BasePreloadAssetListEditor : EditorWindow
{
    Vector2 m_posScroll1;
    Vector2 m_posScroll2;

    string[] VERSION_LIST;
    bool m_isValidList;
    int  m_targetVersionIndex;
    int  m_targetVersion;

    AssetDefines.ServerEnum m_servertype;

    class AVData
    {
        public string fileName;
        public bool   IsSave;
    }
    List< AVData > m_listAssets = new List<AVData>();

    class PreloadData
    {
        public string fileName;
        public bool parseScript;
        public bool IsSave;
    }
    List<PreloadData>  m_listOriginPreloadData = new List<PreloadData>();
    List<PreloadData>  m_listPreloadData       = new List<PreloadData>();

    //
    protected void Initialize( AssetDefines.ServerEnum server_type )
    {
        m_servertype  = server_type;
        m_isValidList = SearchVersionLists( EditorUserBuildSettings.activeBuildTarget, server_type );    

        LoadPreloadLists();

        m_listPreloadData.Clear();
        m_listPreloadData.AddRange(m_listOriginPreloadData);

        LoadAssetList();
    }

    void LoadAssetList()
    {
        m_listAssets.Clear();

        string _root = AssetBundleEditorUtil.GetVersionListPath( EditorUserBuildSettings.activeBuildTarget, m_servertype, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );
        var _list = AssetVersions.ReadAssetVersionFile( _root, m_targetVersion );
        for (int i = 0, _max = _list.Count; i < _max; ++i)
        {
            AVData _new   = new AVData();
            _new.fileName = Path.GetFileNameWithoutExtension(_list[i].filename);
            _new.IsSave = m_listPreloadData.Exists(r => r.fileName == _new.fileName && r.IsSave == true);

            m_listAssets.Add(_new);
        }

        m_listAssets.Sort(delegate(AVData x, AVData y){
            if( x.IsSave == y.IsSave ){
                return x.fileName.CompareTo( y.fileName );
            }
            else{
                return y.IsSave.CompareTo( x.IsSave );
            }
        });
    }

    void LoadPreloadLists()
    {
        m_listOriginPreloadData.Clear();

        string _file = Path.GetFullPath( Path.Combine( "Assets/Resources", string.Format( "{0}.txt", AssetManager.PRELOADLIST_FILENAME ) ) );
        if (File.Exists(_file) == true)
        {
            using (FileStream fs = new FileStream(_file, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    while (true)
                    {
                        string _line_data = reader.ReadLine();
                        if (_line_data == null){
                            break;
                        }

                        string[] _parsed = _line_data.Split(","[0]);

                        var _new = new PreloadData();
                        _new.fileName    = _parsed[0];
                        _new.parseScript = System.Convert.ToBoolean(_parsed[1]);
                        _new.IsSave      = true;

                        m_listOriginPreloadData.Add(_new);
                    }
                }
            }

            m_listOriginPreloadData.Sort(delegate(PreloadData x, PreloadData y) {
                return x.fileName.CompareTo( y.fileName );
            });
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal( GUI.skin.box );

        GUI.enabled = m_isValidList;

        EditorGUILayout.BeginVertical( GUI.skin.box );

        var _prev_index = m_targetVersionIndex;
        m_targetVersionIndex = EditorGUILayout.Popup("Target Version : ", m_targetVersionIndex, VERSION_LIST);
        if (_prev_index != m_targetVersionIndex)
        {
            m_targetVersion = int.Parse(VERSION_LIST[m_targetVersionIndex]);
            LoadAssetList();
        }

        m_posScroll1 = EditorGUILayout.BeginScrollView( m_posScroll1, GUI.skin.box );

        bool _is_toggle_changed = false;

        for (int i = 0, _max = m_listAssets.Count; i < _max; ++i)
        {
            EditorGUILayout.BeginHorizontal( GUI.skin.box, GUILayout.Width( 300 ) );

            var _asset_name   = string.Format("{0}",m_listAssets[i].fileName );

            var _prev = m_listAssets[i].IsSave;
            m_listAssets[ i ].IsSave = GUILayout.Toggle( m_listAssets[ i ].IsSave, _asset_name );
            if (_prev != m_listAssets[i].IsSave){
                _is_toggle_changed = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (_is_toggle_changed == true)
        {
            m_listAssets.Sort(delegate(AVData x, AVData y){
                if( x.IsSave == y.IsSave ){
                    return x.fileName.CompareTo( y.fileName );
                }
                else{
                    return y.IsSave.CompareTo( x.IsSave );
                }
            });

            for (int i = 0, _max = m_listAssets.Count; i < _max; ++i)
            {
                if (m_listAssets[i].IsSave == true)
                {
                    var _target = m_listPreloadData.Find(r => r.fileName == m_listAssets[i].fileName);
                    if (_target == null )
                    {
                        var _new = new PreloadData();
                        _new.fileName = m_listAssets[i].fileName;
                        _new.IsSave = true;
                        m_listPreloadData.Add(_new);
                    }
                    else
                    {
                        _target.IsSave = true;
                    }
                }
                else
                {
                    if (m_listOriginPreloadData.Exists(r => r.fileName == m_listAssets[i].fileName) == true)
                    {
                        var _target = m_listPreloadData.Find(r => r.fileName == m_listAssets[i].fileName);
                        if (_target != null)
                        {
                            _target.IsSave = false;
                        }
                    }
                    else
                    {
                        var _target = m_listPreloadData.Find(r => r.fileName == m_listAssets[i].fileName);
                        if (_target != null){
                            m_listPreloadData.Remove(_target);
                        }
                    }
                }
            }

            m_listPreloadData.Sort(delegate(PreloadData x, PreloadData y) {
                if( x.IsSave == y.IsSave ){
                    return x.fileName.CompareTo( y.fileName );
                }
                else{
                    return y.IsSave.CompareTo( x.IsSave );
                }
            });
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUI.enabled = true;

        EditorGUILayout.BeginVertical( GUI.skin.box );

        GUILayout.Label("Preload Asset Files : ");

        _is_toggle_changed = false;
        m_posScroll2 = EditorGUILayout.BeginScrollView( m_posScroll2, GUI.skin.box );
        for (int i = 0, _max = m_listPreloadData.Count; i < _max; ++i)
        {
            EditorGUILayout.BeginHorizontal( GUI.skin.box, GUILayout.Width(300) );

            var _prev = m_listPreloadData[i].IsSave;
            m_listPreloadData[ i ].IsSave = GUILayout.Toggle( m_listPreloadData[ i ].IsSave, "" );
            if (_prev != m_listPreloadData[i].IsSave)
            {
                _is_toggle_changed = true;
            }

            GUI.enabled = m_listPreloadData[i].IsSave;

            var _asset_name   = string.Format("{0}",m_listPreloadData[i].fileName );
            GUILayout.Label(_asset_name);

            m_listPreloadData[ i ].parseScript = GUILayout.Toggle( m_listPreloadData[ i ].parseScript, "Parse Script" );
           
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        if (_is_toggle_changed == true)
        {
            var _list = m_listPreloadData.FindAll(r => r.IsSave == false);
            for (int i = 0, _max = _list.Count; i < _max; ++i)
            {
                var _target = m_listAssets.Find(r => r.fileName == _list[i].fileName);
                if (_target != null){
                    _target.IsSave = false;
                }

                if (m_listOriginPreloadData.Exists(r => r.fileName == _list[i].fileName) == false){
                    m_listPreloadData.Remove(_list[i]);
                }
            }

            _list = m_listPreloadData.FindAll(r => r.IsSave == true);
            for (int i = 0, _max = _list.Count; i < _max; ++i)
            {
                var _target = m_listAssets.Find(r => r.fileName == _list[i].fileName);
                if (_target != null){
                    _target.IsSave = true;
                }
            }

            m_listPreloadData.Sort(delegate(PreloadData x, PreloadData y) {
                if( x.IsSave == y.IsSave ){
                    return x.fileName.CompareTo( y.fileName );
                }
                else{
                    return y.IsSave.CompareTo( x.IsSave );
                }
            });

            m_listAssets.Sort(delegate(AVData x, AVData y){
                if( x.IsSave == y.IsSave ){
                    return x.fileName.CompareTo( y.fileName );
                }
                else{
                    return y.IsSave.CompareTo( x.IsSave );
                }
            });
        }


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        GUI.enabled = m_listPreloadData.Exists(r => r.IsSave == true) || m_listOriginPreloadData.Count > 0;
        var _style = GUI.skin.button;
        _style.richText = true;
        if( GUILayout.Button( "Write Preload Lists" ) == true  )
        {
            var _list = m_listPreloadData.FindAll(r => r.IsSave == true);
            _list.Sort(delegate(PreloadData x, PreloadData y){
                return x.fileName.CompareTo( y.fileName );
            });

            string _file = Path.GetFullPath( Path.Combine( "Assets/Resources", string.Format( "{0}.txt", AssetManager.PRELOADLIST_FILENAME ) ) );
            if (_list.Count > 0)
            {
                using (FileStream fs = new FileStream(_file, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        for (int i = 0, _max = _list.Count; i < _max; ++i)
                        {
                            sw.WriteLine(string.Format("{0},{1}", _list[i].fileName, _list[i].parseScript.ToString().ToLower()));
                        }
                    }
                }
            }
            else
            {
                if (File.Exists(_file) == true){
                    FileUtil.DeleteFileOrDirectory( _file );
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log("Write Process Complete");
            Close();
        }

        GUI.enabled = true;
    }

    bool SearchVersionLists( BuildTarget target, AssetDefines.ServerEnum server_type )
    {
        var _list = AssetBundleEditorUtil.ReadVersionList( target, server_type );
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
            m_targetVersion = 0;
            return false;
        }
    }
	
}
