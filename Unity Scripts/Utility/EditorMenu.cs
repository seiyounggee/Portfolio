#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorMenu
{
    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/OutGame Scene")]
    public static void GoToLobbyScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/OutGameScene.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/InGame Scene")]
    public static void GoToInGameScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/InGameScene.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/WorkSpace/Colorize_Workspace")]
    public static void GoToWorkScene_Colorize()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/Workspace/Colorize_Workspace.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/WorkSpace/File_Workspace")]
    public static void GoToWorkScene_File()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/Workspace/File_Workspace.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/WorkSpace/FirebaseDB_Workspace")]
    public static void GoToWorkScene_FirebaseDB()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/Workspace/FirebaseDB_Workspace.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/WorkSpace/UI_Workspace")]
    public static void GoToWorkScene_UI()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/Workspace/UI_Workspace.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/WorkSpace/Map_Workspace")]
    public static void GoToWorkScene_Map()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/Workspace/Map_Workspace.unity");
    }

    [MenuItem(CommonDefine.ProjectName + "/Open Quantum VS Solution")]
    private static void OpenQuantumProject()
    {
        var path = System.IO.Path.GetFullPath(QuantumEditorSettings.Instance.QuantumSolutionPath);

        if (!System.IO.File.Exists(path))
        {
            EditorUtility.DisplayDialog("Open Quantum Project", "Solution file '" + path + "' not found. Check QuantumProjectPath in your QuantumEditorSettings.", "Ok");
        }

        var uri = new Uri(path);
        Application.OpenURL(uri.AbsoluteUri);
    }
}
#endif
