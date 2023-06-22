#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

public class CommonEditor : EditorWindow
{
    [MenuItem(CommonDefine.ProjectName + "/Custom Function/Clear Asset Bundle Cache ")]
    public static void ClearCache()
    {
        Caching.ClearCache();

        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "cache"));

        if (directoryInfo != null)
        {
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.Delete();
            }
            Debug.Log("All Assetbundle Cache Cleared!");
        }
        else
        {
            Debug.Log("Erorr...! directoryInfo is null??");
        }
    }

    [MenuItem(CommonDefine.ProjectName + "/Custom Function/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();

        Debug.Log("All PlayerPrefs Deleted!");
    }
}
#endif