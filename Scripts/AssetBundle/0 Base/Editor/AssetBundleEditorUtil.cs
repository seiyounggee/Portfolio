using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleEditorUtil
{
    public static string COMMON_PATH = "Assets/DownloadAssets";

#if UNITY_2020_3_OR_NEWER
    public static string OUTPUT_PATH = "AssetBundles";
#else
    public static string OUTPUT_PATH = "AssetBundles/Unity2018";
#endif

    public static string VERSIONLIST_FOLDER_NAME = "VersionLists";

    //
    public static string GetAssetOutputPath( BuildTarget target, int target_version = 0 )
    {
        string sz = Directory.GetCurrentDirectory();
        sz = sz.Replace( '/', Path.DirectorySeparatorChar );

        /* Assets 까지 경로가 나온다 지워주자. */
        int index = sz.LastIndexOf( Path.DirectorySeparatorChar );
        if( index >= 0 )
            sz = sz.Substring( 0, index + 1 );

        string path = Path.Combine( sz, OUTPUT_PATH );
        path = Path.Combine( path, target.ToString() );

        if( target_version > 0 )
        {
            path = Path.Combine( path, target_version.ToString() );
        }

        return path;
    }

    public static string GetVersionListPath( BuildTarget target, AssetDefines.ServerEnum server_type, string _versionlists_folder_parent_name = "" )
    {
        string sz = Directory.GetCurrentDirectory();
        sz = sz.Replace( '/', Path.DirectorySeparatorChar );

        /* Assets 까지 경로가 나온다 지워주자. */
        int index = sz.LastIndexOf( Path.DirectorySeparatorChar );
        if( index >= 0 )
            sz = sz.Substring( 0, index + 1 );

        string path = Path.Combine( sz, OUTPUT_PATH );
        path = Path.Combine( path, target.ToString() );
        if( string.IsNullOrEmpty(_versionlists_folder_parent_name) == false ){
            path = Path.Combine(path, _versionlists_folder_parent_name);
        }
        path = Path.Combine( path, server_type.ToString() );

        return path;
    }

    //public static string GetOuptutParentPath( BuildTarget target, PnixCommon.eServerType server_type, int target_version = 0 )
    //{
    //    string sz = Directory.GetCurrentDirectory();
    //    sz = sz.Replace( '/', Path.DirectorySeparatorChar );
    //
    //    /* Assets 까지 경로가 나온다 지워주자. */
    //    int index = sz.LastIndexOf( Path.DirectorySeparatorChar );
    //    if( index >= 0 )
    //        sz = sz.Substring( 0, index + 1 );
    //
    //    string path = Path.Combine( sz, OUTPUT_PATH );
    //    path = Path.Combine( path, target.ToString() );
    //    path = Path.Combine( path, server_type.ToString() );
    //
    //    if( target_version > 0 )
    //    {
    //        path = Path.Combine( path, target_version.ToString() );
    //    }
    //
    //    return path;
    //}

    public static List<int> ReadVersionList( BuildTarget target, AssetDefines.ServerEnum server_type )
    {
        List<int> _list_versions = new List<int>();

        string _root = GetVersionListPath( target, server_type, AssetBundleEditorUtil.VERSIONLIST_FOLDER_NAME );
        if( Directory.Exists( _root ) == true )
        {
            string[] _file_names = Directory.GetFiles( _root );
            for( int i = 0, _max = _file_names.Length; i < _max; ++i )
            {
                int _version;
                if( int.TryParse( Path.GetFileNameWithoutExtension( _file_names[ i ] ), out _version ) == false )
                    continue;

                _list_versions.Add( _version );
            }

            _list_versions.Sort();
        }

        return _list_versions;
    }

    public static int GetPreviosVersion( BuildTarget target, AssetDefines.ServerEnum server_type, int target_version )
    {
        List<int> _versions = ReadVersionList( target, server_type );
        if( _versions.Count == 0 )
            return 0;

        int _prev_version = 0;
        for( int i = 0, _max = _versions.Count; i < _max; ++i )
        {
            if( _versions[ i ] >= target_version )
            {
                break;
            }

            _prev_version = _versions[ i ];
        }

        return _prev_version;
    }

}
