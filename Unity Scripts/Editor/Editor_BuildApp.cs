using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.Build;

public class Editor_BuildApp : IPreprocessBuildWithReport
{
    private static bool isCheatKey = true;
    public static BuildType buildType = BuildType.None;
    private static string originalDefined = string.Empty;
    private static string defined = string.Empty;
    private static string addDefined = string.Empty;
    private static string path = string.Empty;

    public int callbackOrder { get { return 0; } }

    public enum BuildType
    {
        None = -1,
        Development,
        Release,
    }

    [MenuItem(CommonDefine.ProjectName + "/Build App/AOS Build/APK")]
    public static void Menu_Build_AOS_APK()
    {
        path = EditorUtility.OpenFolderPanel("Choose Location of Built Game", "", "");

        if (string.IsNullOrEmpty(path))
            return;

        var folderName = System.IO.Path.GetFileName(path);
        path += string.Format("/" + folderName + ".apk", PlayerSettings.bundleVersion);

        BuildApp_AOS(true);
    }

    [MenuItem(CommonDefine.ProjectName + "/Build App/AOS Build/App Bundle")]
    public static void Menu_Build_AOS_AppBundle()
    {
        path = EditorUtility.OpenFolderPanel("Choose Location of Built Game", "", "");

        if (string.IsNullOrEmpty(path))
            return;

        var folderName = System.IO.Path.GetFileName(path);
        path += string.Format("/" + folderName + ".aab", PlayerSettings.bundleVersion);

        BuildApp_AOS(false);
    }

    [MenuItem(CommonDefine.ProjectName + "/Build App/iOS Build")]
    public static void Menu_Build_iOS()
    {
        path = EditorUtility.OpenFolderPanel("Choose Location of Built Game", "", "");

        if (string.IsNullOrEmpty(path))
            return;

        var folderName = System.IO.Path.GetFileName(path);
        path += string.Format("/" + folderName, PlayerSettings.bundleVersion);

        BuildApp_iOS();
    }

    public static void BuildApp_AOS(bool isApk)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("<color=red>Error...! path is empty!</color>");
            return;
        }

        if (buildType == BuildType.None)
            buildType = BuildType.Development;

        EditorUserBuildSettings.buildAppBundle = !isApk;

        SetScriptingDefinedSymbols(true);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildSceneList();
        buildPlayerOptions.locationPathName = path;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        PlayerSettings.keystorePass = "9972King@@";
        PlayerSettings.keyaliasPass = "9972King@@";

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("<color=cyan>[Build] Succeeded: " + "\n outputPath: " + summary.outputPath + "  size: " + summary.totalSize + " bytes" + "</color>");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("<color=red>Build failed</color>");
        }

        SetScriptingDefinedSymbols(false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BuildApp_iOS()
    {
        if (string.IsNullOrEmpty(path))
            path = string.Format("Build/iOS/ProjectDB_{0}", PlayerSettings.bundleVersion);

        if (buildType == BuildType.None)
            buildType = BuildType.Release;

        SetScriptingDefinedSymbols(true);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.locationPathName = path;
        options.target = BuildTarget.iOS;
        options.scenes = GetBuildSceneList();
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        PlayerSettings.keystorePass = "9972King@@";
        PlayerSettings.keyaliasPass = "9972King@@";

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes" + "\n outputPath: " + summary.outputPath);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("<color=red>Build failed</color>");
        }

        SetScriptingDefinedSymbols(false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log(target + " Build Complete!   " + pathToBuiltProject);
    }

    private static void SetScriptingDefinedSymbols(bool isChange)
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

            switch (buildType)
            {
                case BuildType.Development:
                    {
                        AddDefinedSymbols(ref defined, ref addDefined, "SERVERTYPE_DEV");
                        RemoveDefinedSymbols(ref defined, "SERVERTYPE_RELEASE");

                        if (isCheatKey)
                            AddDefinedSymbols(ref defined, ref addDefined, "CHEAT");
                        else
                            RemoveDefinedSymbols(ref defined, "CHEAT");
                    }
                    break;

                case BuildType.Release:
                    {
                        AddDefinedSymbols(ref defined, ref addDefined, "SERVERTYPE_RELEASE");
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

    private static void AddDefinedSymbols(ref string defined, ref string addDefined, string def)
    {
        string semiDef = def + ";";
        if (defined.Contains(semiDef) == false && defined.Contains(def) == false)
            addDefined = semiDef + addDefined;
    }

    private static void RemoveDefinedSymbols(ref string defined, string def)
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
    private static string[] GetBuildSceneList()
    {
        EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;

        var level = new List<string>();

        foreach (var i in scenes)
        {
            if (i.enabled && (i.path.Contains(CommonDefine.InGameScene) || i.path.Contains(CommonDefine.OutGameScene)))
                level.Add(i.path);
        }

        if (level.Count == 0)
        {
            Debug.Log("<color=red>Error....! Build Included Scene is 0</color>");
        }

        return level.ToArray();
    }


    //빌드전에 firebase 파일 바꿔줘야함
    public void OnPreprocessBuild(BuildReport report)
    {
        string targetPath = "Assets/google-services.json";
        string sourcePath = "";

        // 여기서 조건을 정의하여 필요한 google-services.json을 선택
#if SERVERTYPE_DEV || UNITY_EDITOR
            sourcePath = "Assets/Resources/FirebaseConfigs/google-services_dev.json";
#elif SERVERTYPE_RELEASE
            sourcePath = "Assets/Resources/FirebaseConfigs/google-services_live.json";
#endif

        // 파일 복사
        System.IO.File.Copy(sourcePath, targetPath, true);
        AssetDatabase.Refresh();
    }
}