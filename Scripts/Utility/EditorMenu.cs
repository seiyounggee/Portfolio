#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorMenu
{
    /*
    [MenuItem(CommonDefine.ProjectName + "/Go to Scene/Splash Scene")]
    public static void GoToSplashScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/SplashScene.unity");
    }
    */

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


    /*
    [MenuItem("Project/Go to Scene/WorkSpace Scene")]
    public static void GoToWorkScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/1.Scenes/TEST_WorkScene.unity");
    }
    */
}
#endif
