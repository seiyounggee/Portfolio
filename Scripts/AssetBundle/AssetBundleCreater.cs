#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class AssetBundleCreater : EditorWindow
{
    /*
    [MenuItem(CommonDefine.ProjectName + "/Build/Build AssetBundles", priority = 2)]
    static public void CreateDollyTRAssetBundleEditor()
    {
        EditorWindow.GetWindow<AssetBundleCreater>(false, "Dolly TR AssetBundles", true);
    }

    private const string ASSET_BUNDLE_BASE_PATH = "Assets/6.AssetBundleFiles";

    private class DirectoryInfo
    {
        public string fileName = "";
        public string totalDirectoryPath = "";
        public string parentDirectoryPath = "";
        public string extention = "";
        public bool isIncluded = false;
    }

    private List<DirectoryInfo> listOfDirectoriesInfo = new List<DirectoryInfo>();

    private void OnEnable()
    {
        listOfDirectoriesInfo.Clear();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal(GUI.skin.box);

        GUILayoutOption[] options = new[] {
        GUILayout.Height (50),
        };
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        GUILayout.Label("DTR AssetBundles Editor", style, options);

        GUILayout.EndHorizontal();

        string[] files = System.IO.Directory.GetDirectories(ASSET_BUNDLE_BASE_PATH);
        if (files != null && files.Length > 0)
        {
            foreach (string file in files)
            {
                string[] innerfile = System.IO.Directory.GetDirectories(file);

                if (innerfile != null && innerfile.Length > 0)
                {
                    foreach (var f in innerfile)
                    {
                        EditorGUILayout.BeginHorizontal(GUI.skin.box);
                        string dPath = f.ToString();
                        string _fileName = dPath.Replace(file.ToString() + "\\", "assetbundle_").ToLower();
                        if (listOfDirectoriesInfo.Find(x => x.totalDirectoryPath.Equals(dPath)) == null)
                            listOfDirectoriesInfo.Add(new DirectoryInfo() { fileName = _fileName, totalDirectoryPath = dPath, parentDirectoryPath = file.ToString(), isIncluded = true });
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        foreach (var i in listOfDirectoriesInfo)
        {
            i.isIncluded = GUILayout.Toggle(i.isIncluded, i.fileName + "  |  Path: " + i.totalDirectoryPath);
        }

        List<DirectoryInfo> listOfDirectoriesInsideBuild = listOfDirectoriesInfo.FindAll(x => x.isIncluded.Equals(true));

        EditorGUILayout.Space(20);

        GUI.backgroundColor = Color.blue;
        options = new[] { GUILayout.Height(30), };
        if (GUILayout.Button("-- Build AssetBundles Files --", options))
        {
#if UNITY_ANDROID
            BuildAllAssetBundles(listOfDirectoriesInsideBuild);
#endif

#if UNITY_IPHONE || UNITY_IOS
            BuildAllAssetBundles(listOfDirectoriesInsideBuild);
#endif
        }
    }

    static void BuildAllAssetBundles(List<DirectoryInfo> list)
    {
        if (list == null || list.Count <= 0)
        {
            Debug.Log("Error...! list is 0 or null");
            return;
        }

        string assetBundleDirectory = ASSET_BUNDLE_BASE_PATH;
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        List<AssetBundleBuild> abb = new List<AssetBundleBuild>();

        foreach (var i in list)
        {
            List<string> strArray = new List<string>();

            if (i.totalDirectoryPath.Contains("Map"))
            {
                string[] innerPath = System.IO.Directory.GetFiles(i.totalDirectoryPath);

                foreach (var ip in innerPath)
                {
                    string extension = System.IO.Path.GetExtension(ip);

                    if (ip.Contains(".unity"))
                        strArray.Add(ip);
                }
            }
            else
            {
                strArray.Add(i.totalDirectoryPath);
            }
            



            abb.Add(new AssetBundleBuild()
            {
                assetBundleName = i.fileName,

                assetNames = strArray.ToArray()
            });
        }

#if UNITY_ANDROID
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, abb.ToArray(),
                                        BuildAssetBundleOptions.ForceRebuildAssetBundle,
                                        BuildTarget.Android);
#endif

#if UNITY_IOS
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, abb.ToArray(),
                                        BuildAssetBundleOptions.ForceRebuildAssetBundle,
                                        BuildTarget.iOS);
#endif
    }
        */
}
#endif
