#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;

public class AppBuildEditor : EditorWindow
{
    private static bool isCheatKey = true;
    public static ServerTypes serverType = ServerTypes.None;
    private static string originalDefined = string.Empty;
    private static string defined = string.Empty;
    private static string addDefined = string.Empty;

    public enum ServerTypes
    {
        None = -1,
        DevServer,
        QaServer,
        ReviewServer,
        ReleaseServer,
    }

    [MenuItem(CommonDefine.ProjectName + "/Build/Build App", priority = 2)]
    static public void CreateDollyTRBuildEditor()
    {
        EditorWindow.GetWindow<AppBuildEditor>(false, "Dolly TR Build", true);
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
        GUILayout.Label("DTR Build Editor", style, options);
        GUILayout.Label("AWARE!!!\nSelecting Build Server Will cause script compilation!!!!");

        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        serverType = (ServerTypes)EditorGUILayout.EnumPopup("Select Build ServerTypes", serverType);
        if (EditorGUI.EndChangeCheck())
        {
            SetScriptingDefinedSymbols(true);
        }

        if (serverType == ServerTypes.DevServer)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            isCheatKey = GUILayout.Toggle(isCheatKey, "CheatKey");
            EditorGUILayout.EndHorizontal();
        }
        else if (serverType == ServerTypes.QaServer)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            isCheatKey = GUILayout.Toggle(isCheatKey, "CheatKey");
            EditorGUILayout.EndHorizontal();
        }
        else if (serverType == ServerTypes.ReviewServer)
        {
            isCheatKey = false;
        }
        else if (serverType == ServerTypes.ReleaseServer)
        {
            isCheatKey = false;
        }


        GUI.backgroundColor = Color.blue;
        options = new[] {GUILayout.Height (30),};

        if (serverType != ServerTypes.None)
        {
            if (GUILayout.Button("-- Build App File --", options))
            {
#if UNITY_ANDROID
                BuildApp_Android();
#endif

#if UNITY_IPHONE || UNITY_IOS
            BuildApp_iOS();
#endif
            }
        }
    }

    private void BuildApp_Android()
    {
        string path = EditorUtility.OpenFolderPanel("Choose Location of Built Game", "", "");

        if (string.IsNullOrEmpty(path))
            return;

        BuildPlayerOptions options = new BuildPlayerOptions();
        string dataTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd  H:m:s", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        options.locationPathName = path;
        options.target = BuildTarget.Android;
        options.scenes = GetBuildSceneList();
        options.options = BuildOptions.AcceptExternalModificationsToPlayer;

        PlayerSettings.keystorePass = "Ponglow70";
        PlayerSettings.keyaliasPass = "Ponglow70";

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        BuildPipeline.BuildPlayer(options);

        SetScriptingDefinedSymbols(false); //이전 디파인으로 복구

        /*
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        */
    }

    private void BuildApp_iOS()
    {
        string path = EditorUtility.SaveFilePanel("Choose Location of Built Game", "", "", "");

        if (string.IsNullOrEmpty(path))
            return;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.locationPathName = path;
        options.target = BuildTarget.iOS;
        options.scenes = GetBuildSceneList();
        options.options = BuildOptions.AcceptExternalModificationsToPlayer;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        PlayerSettings.keystorePass = "Ponglow70";
        PlayerSettings.keyaliasPass = "Ponglow70";

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        BuildPipeline.BuildPlayer(options);

        SetScriptingDefinedSymbols(false); //이전 디파인으로 복구

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log(target + " Build Complete!   " + pathToBuiltProject);
    }

    private void SetScriptingDefinedSymbols(bool isChange)
    {
        addDefined = string.Empty;
        defined = string.Empty;
#if UNITY_ANDROID
        defined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
#elif UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX
        defined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.iOS);
#elif UNITY_STANDALONE_WIN
        defined = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone );
#endif
        if (isChange)
        {
            originalDefined = defined;

            switch (serverType)
            {
                case ServerTypes.DevServer:
                    {
                        AddDefinedSymbols(ref defined, ref addDefined, "SERVERTYPE_DEV");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_RELEASE");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_REVIEW");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_QA");

                        if (isCheatKey)
                            AddDefinedSymbols(ref defined, ref addDefined, "CHEAT");
                        else
                            RemoveDefinedSymbols(ref defined, "CHEAT");
                    }
                    break;

                case ServerTypes.QaServer:
                    {
                        AddDefinedSymbols(ref defined, ref addDefined, "SERVERTYPE_QA");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_DEV");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_RELEASE");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_REVIEW");

                        if (isCheatKey)
                            AddDefinedSymbols(ref defined, ref addDefined, "CHEAT");
                        else
                            RemoveDefinedSymbols(ref defined, "CHEAT");
                    }
                    break;

                case ServerTypes.ReviewServer:
                    {
                        AddDefinedSymbols(ref defined, ref addDefined, "SERVERTYPE_REVIEW");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_RELEASE");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_QA");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_DEV");

                        RemoveDefinedSymbols(ref defined, "CHEAT");
                    }
                    break;

                case ServerTypes.ReleaseServer:
                    {
                        AddDefinedSymbols(ref defined, ref addDefined, "SERVERTYPE_RELEASE");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_REVIEW");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_QA");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_DEV");

                        RemoveDefinedSymbols(ref defined, "CHEAT");
                    }
                    break;

                default:
                    break;
            }

            if (string.IsNullOrEmpty(addDefined) == false)
                defined += ";" + addDefined;
        }
        else
        {
            defined = originalDefined;
        }



        Debug.Log("Scripting Defined>>>> " + defined);

#if UNITY_ANDROID
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, defined);
#elif UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX
         PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.iOS, defined);
#elif UNITY_STANDALONE_WIN
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, defined );
#endif
    }

    private void AddDefinedSymbols(ref string defined, ref string addDefined, string def)
    {
        string semiDef = def + ";";
        if (defined.Contains(semiDef) == false && defined.Contains(def) == false)
            addDefined = semiDef + addDefined;
    }

    private void RemoveDefinedSymbols(ref string defined, string def)
    {
        string semiDef = def + ";";
        if (defined.Contains(semiDef))
            defined = defined.Replace(semiDef, "");
        else if (defined.Contains(def))
            defined = defined.Replace(def, "");
    }

    /// <summary>
    /// 현재 빌드세팅에 있는 scenelist 가져오기
    /// </summary>
    /// <returns></returns>
    string[] GetBuildSceneList()
    {
        EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;

        var level = new List<string>();

        foreach (var i in scenes)
        {
            if (i.enabled && (i.path.Contains("InGameScene") || i.path.Contains("OutGameScene")))
                level.Add(i.path);
        }

        if (level.Count == 0)
        {
            Debug.Log("<color=red>Error....! Build Included Scene is 0</color>");
        }

        return level.ToArray();
    }


}
#endif