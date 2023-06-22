using PNIX.ReferenceTable;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using System;
using System.IO;

public partial class DataManager
{
    public enum VersionType
    {
        None = 0,
        Resource,
        Asset,

    }

    protected class VersionData
    {
        public int attached;
        public int download;
        public int revision;
        public string extension;

        //
        public string GetAttachedFileName()
        {
            return string.Format("{0}{1}", attached, extension);
        }

        public void Init(string ext)
        {
            extension = ext;
            download = 0;
            attached = 0;
            revision = 0;
        }
    }

    protected VersionData m_RefsData = new VersionData();
    protected VersionData m_AssetData = new VersionData();

    public bool isPnixReferenceDataLoaded = false;

    private static Dictionary<string, UnityEngine.Object> m_LoadObjects = new Dictionary<string, UnityEngine.Object>();

    public static readonly string REFSFILE_EXTENTION = ".refs";
    public static string GetVersionFilePath()
    {
        return string.Format("{0}{1}", Application.temporaryCachePath, Path.DirectorySeparatorChar);
    }

    Action<bool> m_eventEnd;
    Action<byte[]> m_eventLoadTables;
    Action<bool, byte[]> m_eventLoadAssetLists;

    public void LoadPnixReferenceData()
    {
        var data = WWWManager.Instance.GetCacheData_Byte();
        //var data = WWWManager.Instance.GetCacheData_FilePath();

        if (data != null)
        {
            bool isLoad = CReferenceManager.Instance.Load(data);

            Debug.Log("<color=cyan>LoadPnixReferenceData : " + isLoad + "  /  data:  " + data + "</color>");

            if (isLoad)
                isPnixReferenceDataLoaded = true;
        }
        else
        {
            Debug.LogError("<color=red>No CacheData....!</color>");
        }
    }

    public static string GetCacheData_String()
    {
#if UNITY_EDITOR
        var _file_path = string.Format("{0}{1}{2}", DataManager.GetVersionFilePath(), ClientVersion.ResFileName, DataManager.REFSFILE_EXTENTION);
#elif UNITY_ANDROID || UNTIY_IPHONE || UNITY_IOS
         var _file_path = string.Format("{0}{1}{2}", DataManager.GetVersionFilePath(), ClientVersion.ResFileName, DataManager.REFSFILE_EXTENTION);
#endif

        return _file_path;
    }

    public static byte[] GetCacheData_Byte()
    {
        byte[] result = null;
        string _file_path = string.Empty;

#if LOAD_LOCALTABLE
        _file_path = System.IO.Path.Combine( CommonDefine.Refs_Path, "table" );

        TextAsset asset = Load( _file_path, false ) as TextAsset;
        if( asset != null )
        result = asset.bytes;
#else
        if (ClientVersion.BuiltInResVersion >= ClientVersion.ResVersion)
        {
            _file_path = System.IO.Path.Combine(CommonDefine.Refs_Path, "table");

            TextAsset asset = Load(_file_path, false) as TextAsset;
            if (asset != null)
                result = asset.bytes;
        }
        else
        {
            _file_path = string.Format("{0}{1}{2}", GetVersionFilePath(), ClientVersion.ResFileName, REFSFILE_EXTENTION);
            if (System.IO.File.Exists(_file_path))
                result = System.IO.File.ReadAllBytes(_file_path);
            else
            {
                _file_path = System.IO.Path.Combine(CommonDefine.Refs_Path, "table");
                TextAsset asset = Load(_file_path, false) as TextAsset;
                if (asset != null)
                    result = asset.bytes;
            }
        }
#endif

        return result;
    }

    public static UnityEngine.Object Load(string path, bool save = true)
    {
        if (string.IsNullOrEmpty(path) == true)
            return null;

        if (m_LoadObjects.ContainsKey(path))
            return m_LoadObjects[path];
        else
        {
            UnityEngine.Object obj = Resources.Load(path);
            if (obj != null)
            {
                if (save)
                    m_LoadObjects.Add(path, obj);
                return obj;
            }
            else
            {
                Debug.LogError("no file found ...... path: " + path);
                /* 추후 여기서 default 로 읽어올수 있게 만들자 */
            }
        }
        return null;
    }

    public static string GetStreamingAssetResourceFolder()
    {
        var _builder = new System.Text.StringBuilder();

        string _path = Application.streamingAssetsPath;
#if UNITY_EDITOR || UNITY_STANDALONE
        _builder.Append("file:///");
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
}
