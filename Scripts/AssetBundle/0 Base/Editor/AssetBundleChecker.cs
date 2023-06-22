using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
 

public class AssetBundleChecker : EditorWindow
{
    List<Object> m_allObjects = new List<Object>();
    Vector2 m_pos;
    string  m_fileName;

    [MenuItem("Assets/AssetBundles/Contents Checker")]
    static void Open()
    {
        var _window = CreateInstance<AssetBundleChecker>();
        bool _re = _window.ReadContents();
        if (_re == true)
        {
            _window.Show();
        }
    }

    [MenuItem("Assets/AssetBundles/File Dependencies Checker")]
    static void OpenFileDep()
    {
        var _selected = AssetDatabase.GetAssetPath( Selection.activeObject);
        if (_selected != null )
        {
            var _window = CreateInstance<AssetBundleChecker>();

            _window.ReadDependencies( _selected);
            _window.Show();
        }
    }

    [MenuItem("Assets/AssetBundles/Duplicate Checker")]
    static void CheckDuplicate()
    {
        Dictionary<string,string> _dic = new Dictionary<string, string>();
        foreach( Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets) )
        {
            if( obj.GetType() == typeof(GameObject) || obj.GetType() == typeof(Material) )
            {
                string _path = AssetDatabase.GetAssetPath(obj);
                var _file_name = System.IO.Path.GetFileNameWithoutExtension(_path);
                if( _dic.ContainsKey( _file_name) == true )
                {
                    Debug.LogError(string.Format("Duplicated Data >> {0} : [ {1} , {2} ]", _file_name, _dic[_file_name], _path));
                }
                else
                {
                    _dic.Add(_file_name, _path);
                }
            }
        }
        
    }

    const string DOWNLOADASSET_PATH_KEYWORD = "DownloadAsset";

    private void OnGUI()
    {
        GUILayout.Label( m_fileName);

        using (var scroll = new EditorGUILayout.ScrollViewScope(m_pos))
        {
            m_pos = scroll.scrollPosition;
            foreach (var obj in m_allObjects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField( obj, obj.GetType(), true, GUILayout.Width( 200 ) );

                var _prev = GUI.color;
                var _path = AssetDatabase.GetAssetPath(obj);
                if (_path.Contains(DOWNLOADASSET_PATH_KEYWORD) == true)
                    GUI.color= Color.red;

                EditorGUILayout.TextField(_path);
                GUI.color = _prev;

                EditorGUILayout.EndHorizontal();
            }
        }
    }


    void ReadDependencies( string path )
    {
        string[] _dep = AssetDatabase.GetDependencies(path, true);

        for (int i = 0; i < _dep.Length; ++i)
        {
            if (path == _dep[i])
                continue;
            
            var _loaded = AssetDatabase.LoadAssetAtPath<Object>(_dep[i]);
            if (_loaded == null)
                continue;

            m_allObjects.Add(_loaded);

            m_allObjects = m_allObjects
                .Where( c => c != null )
                .Where( c => c is Component == false )
                //.Where( c => c is GameObject == false )
                .Where( c => ( c is GameObject == true ) ? ( ((GameObject)c).transform.parent == null ) : true )
                // .Distinct().OrderBy( c => c.name).ToList();
                .OrderBy( c => c.GetType().ToString() ).ThenBy( c=> c.name ).ToList();
        }

        string _file_name = Application.dataPath + path.Remove( 0, 6 );
        m_fileName = _file_name + " " + m_allObjects.Count();
    }

    bool ReadContents()
    {
        bool _success = false;
        AssetBundle _bundle = null;

        try
        {
            var _selected = AssetDatabase.GetAssetPath( Selection.activeObject);
            m_fileName = Application.dataPath +_selected.Remove( 0, 6 );

            _bundle = AssetBundle.LoadFromFile( m_fileName );
            if( _bundle != null )
            {
                SerializedObject _so = new SerializedObject( _bundle );

                foreach( SerializedProperty content in _so.FindProperty("m_PreloadTable"))
                {
                    if( content.objectReferenceValue != null )
                    {
                        m_allObjects.Add( content.objectReferenceValue );
                    }
                    else
                    {
                        Debug.LogError( "Empty Reference ! " + content.displayName );
                    }
                }

                m_allObjects = m_allObjects
                    .Where( c => c != null )
                    .Where( c => c is Component == false )
                  //.Where( c => c is GameObject == false )
                    .Where( c => ( c is GameObject == true ) ? ( ((GameObject)c).transform.parent == null ) : true )
                 // .Distinct().OrderBy( c => c.name).ToList();
                    .OrderBy( c => c.name).ToList();

            

                _so.Dispose();
                _bundle.Unload(false);

                _success = true;
            }
        }
        catch
        {
            _success = false;
        }
        finally
        {
            if (_bundle != null)
            {
                _bundle.Unload(false);
            }
        }

        return _success;
    }


}
