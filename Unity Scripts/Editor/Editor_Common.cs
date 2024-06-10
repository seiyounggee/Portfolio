using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Editor_Common
{
    [MenuItem(CommonDefine.ProjectName + "/Common/DeleteCache")]
    public static void Menu_DeleteCache()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Caching.ClearCache(); //Addressable Á¦°Å

        Debug.Log("<color=cyan> Playerprefs, Cached Data Deleted! </color>");
    }

}
