using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

public class BaseAssetBundleMaker 
{
    public enum FindAssetFiles
    {
        OnlyFiles,
        OnlyFolders,
        AllAsFolders,
        FileAndFolders,
    }

    protected class FileRecord
    {
        public string mName;
        public string mAssetPath;

        public string mOriginAssetPath;


        public static FileRecord Get()
        {
            return new FileRecord();
        }
    }

    protected BuildTarget m_BuildTarget;

    //
    protected static Dictionary<string,List<string>> GetAssetTargetFiles( string target_path, FindAssetFiles find_type )
    {
        Dictionary<string,List<string>> _new = new Dictionary<string, List<string>>();
    
        switch (find_type)
        {
            case FindAssetFiles.AllAsFolders:
                {
                    List<string> listDirs = new List<string>();
                    listDirs.AddRange( Directory.GetDirectories(target_path) );

                    for( int i = 0, _max = listDirs.Count; i < _max; ++i )
                    {
                        var _list_data = new List<string>();

                        listDirs[ i ] = listDirs[ i ].Replace( "/.svn", "" );
                        AddDataInFolder( ref _list_data, listDirs[ i ], SearchOption.AllDirectories );

                        var _output_file_name = GetOutputFileName(listDirs[ i ], null);
                        if (string.IsNullOrEmpty(_output_file_name) == true)
                        {
                            Debug.LogError("GetOutputFileName() Error");
                            continue;
                        }

                        _new.Add( _output_file_name.ToLower(), _list_data );
                    }

                    //
                    listDirs.Clear();
                    AddDataInFolder( ref listDirs, target_path, SearchOption.TopDirectoryOnly );

                    if (listDirs.Count > 0)
                    {
                        var _output_file = GetOutputFileName(target_path, null);
                        if (string.IsNullOrEmpty(_output_file) == true){
                            Debug.LogError("GetOutputFileName() Error");
                        }
                        else {
                            _new.Add(_output_file.ToLower(), listDirs);
                        }
                    }
                }
                break;

            case FindAssetFiles.FileAndFolders:
                {
                    List<string> listDirs = new List<string>();
                    listDirs.AddRange( Directory.GetDirectories(target_path) );

                    for( int i = 0, _max = listDirs.Count; i < _max; ++i )
                    {
                        var _list_data = new List<string>();

                        listDirs[ i ] = listDirs[ i ].Replace( "/.svn", "" );
                        AddDataInFolder( ref _list_data, listDirs[ i ], SearchOption.AllDirectories );

                        var _output_file_name = GetOutputFileName(listDirs[ i ], null);
                        if (string.IsNullOrEmpty(_output_file_name) == true)
                        {
                            Debug.LogError("GetOutputFileName() Error");
                            continue;
                        }

                        _new.Add( _output_file_name.ToLower(), _list_data );
                    }

                    //
                    listDirs.Clear();
                    AddDataInFolder( ref listDirs, target_path, SearchOption.TopDirectoryOnly );

                    for (int i = 0, _max = listDirs.Count; i < _max; ++i)
                    {
                        var _list_data = new List<string>();
                        _list_data.Add(listDirs[i]);

                        var _output_file_name = GetOutputFileName(listDirs[i], null);
                        if (string.IsNullOrEmpty(_output_file_name) == true)
                        {
                            Debug.LogError("GetOutputFileName() Error");
                            continue;
                        }

                        _new.Add( _output_file_name.ToLower(), _list_data);
                    }
                }
                break;

            case FindAssetFiles.OnlyFiles:
                {
                    var _listFilesInFolder = new List<string>();
                    AddDataInFolder( ref _listFilesInFolder, target_path, SearchOption.TopDirectoryOnly );

                    for (int i = 0, _max = _listFilesInFolder.Count; i < _max; ++i)
                    {
                        var _list_data = new List<string>();
                        _list_data.Add(_listFilesInFolder[i]);

                        var _output_file_name = GetOutputFileName( _listFilesInFolder[i], null );
                        if (string.IsNullOrEmpty(_output_file_name) == true)
                        {
                            Debug.LogError("GetOutputFileName() Error");
                            continue;
                        }

                        _new.Add( _output_file_name.ToLower(), _list_data);
                    }
                }
                break;

            case FindAssetFiles.OnlyFolders:
                {
                    List<string> listDirs = new List<string>();
                     //listDirs.AddRange( Directory.GetDirectories( target_path ) );
                    GetEndDirectoriesRecursively( listDirs, target_path, target_path );
                     
                    for( int i = 0, _max = listDirs.Count; i < _max; ++i )
                    {
                        var _list_data = new List<string>();

                        listDirs[ i ] = listDirs[ i ].Replace( "/.svn", "" );
                        AddDataInFolder( ref _list_data, listDirs[ i ], SearchOption.AllDirectories );

                        var _output_file_name = GetOutputFileName( listDirs[i], null );
                        if (string.IsNullOrEmpty(_output_file_name) == true)
                        {
                            Debug.LogError("GetOutputFileName() Error");
                            continue;
                        }
                        _new.Add( _output_file_name.ToLower(), _list_data );
                    }
                }
                break;
        }
      
        return _new;
    }

    static void GetEndDirectoriesRecursively( List<string> result, string target, string root )
    {
        string[] _directory = Directory.GetDirectories( target );
        if( _directory != null && _directory.Length > 0 )
        {
            for( int i=0, _max = _directory.Length; i < _max; ++i ){
                GetEndDirectoriesRecursively( result, _directory[i], root );
            }
        }
        else if( root != target )
        {
            result.Add( target );
        }
    }

    static void AddDataInFolder( ref List<string> list_data, string dir_name, SearchOption option )
    {
        var _curr_dir = Directory.GetCurrentDirectory();

        DirectoryInfo _dir = new DirectoryInfo(dir_name);
        FileInfo[] _infos  = _dir.GetFiles("*.*", option);
        _infos = System.Array.FindAll<FileInfo>(_infos, r => r.Extension != ".xml" && r.Extension != ".meta");

        for (int i = 0, _max = _infos.Length; i < _max; ++i)
        {
            dir_name = _infos[i].Directory.ToString().Replace(_curr_dir, "").Trim('/',' ','\\');

            string _file_name = string.Format("{0}/{1}", dir_name, _infos[i].Name);
            list_data.Add(_file_name);
        }
    }

    //
    protected virtual void AddDataToList( string file_name, AssetFilesData.ScriptExtractionStep step, ref List<FileRecord> list_data )
    {
        Object _t = AssetDatabase.LoadAssetAtPath(file_name, typeof(Object));
        if (_t == null)
            return;
        
        var _record = FileRecord.Get();
        _record.mAssetPath = file_name;
        _record.mName = _t.name;
        _record.mOriginAssetPath = string.Empty;

        list_data.Add(_record);
    }

    protected string MakeData( List<FileRecord> list_data, string target_path, string file_name, List<AssetBundleBuild> lists )
    {
        if( list_data.Count == 0 )
            return string.Empty;

        List<string> _listNames = new List<string>();
        for (int i = 0, _max = list_data.Count; i < _max; ++i)
        {
            if (_listNames.Contains(list_data[i].mAssetPath) == false)
            {
                _listNames.Add(list_data[i].mAssetPath);
            }
        }

        string _output_file_name = GetOutputFileName( file_name, (lists != null && lists.Count > 0 ) ? lists[0].assetBundleName : null );

        var _build = new AssetBundleBuild();
        _build.assetBundleName = _output_file_name;
        _build.assetNames = _listNames.ToArray();

        List< AssetBundleBuild > _listBuilds = new List<AssetBundleBuild>();
       _listBuilds.Add(_build);

        //
        if (lists == null)
        {
            var _mf   = BuildPipeline.BuildAssetBundles(target_path, _listBuilds.ToArray(), BuildAssetBundleOptions.ForceRebuildAssetBundle, m_BuildTarget );
            var _hash = _mf.GetAssetBundleHash( _output_file_name );
            return _hash.ToString();
        }
        else
        {
           // lists.Add(_build);
            lists.AddRange( _listBuilds );
            return string.Empty;
        }
    }

    static string GetOutputFileName( string file_name, string common_asset_name )
    {
        int _index = Mathf.Max( file_name.LastIndexOf("\\"), file_name.LastIndexOf("/") );

        file_name = file_name.Substring(_index + 1);
        file_name = file_name.Split("."[0])[0];

        string _output_file_name;
        if (string.IsNullOrEmpty(common_asset_name) == true)
        {
            _output_file_name = string.Format("{0}.ab", file_name);
        }
        else
        {
            string _common = Path.GetFileNameWithoutExtension(common_asset_name);
            _output_file_name = string.Format("{0}@{1}.ab", _common, file_name);
        }

        return _output_file_name;
    }

    public static bool IsExistLightmap( GameObject root )
    {
        var _renderers = root.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0, _max = _renderers.Length; i < _max; ++i)
        {
            if (_renderers[i].lightmapIndex != -1)
            {
                return true;
            }
        }

        return false;
    }

    public static bool AttachLightmapScript( UnityEngine.LightmapsMode mode, GameObject root )
    {
        bool _apply = IsExistLightmap(root);
        if( _apply == true )
        {
            SceneAssetBundleLightmaps.GenerateLightmapInfo(mode, root);
        }

        return _apply;
    }
}
