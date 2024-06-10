using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SceneManagerExtensions : MonoSingleton<SceneManagerExtensions>
{
    public void LoadScene(string sceneName, LoadSceneMode mode)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);

            if (scene.name.Equals(sceneName))
            {
                if (scene.isLoaded == true)
                {
                    Debug.Log("<color=red>" + "Scene Already Loaded...? >>> " + sceneName + "</color>");
                }
            }
        }

        Debug.Log("<color=cyan>" + "Loading Scene >>> " + sceneName + "</color>");
        var handle = SceneManager.LoadSceneAsync(sceneName, mode);
        handle.completed += op =>
        {
            if (op.isDone)
            {
                NetworkManager_Client.Instance.Quantum_RoomData.isInGameSceneLoaded = true;
                Debug.Log("<color=white>Scene >> " + sceneName + " loaded successfully!</color>");
            }
            else
            {
                Debug.LogError("Failed to load scene: " + sceneName);
            }
        };
    }

    public void SetActiveScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        Debug.Log("<color=cyan>" + "SetActiveScene >>> " + sceneName + "</color>");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    public enum SceneType
    { 
        None,
        OutGame,
        InGame,
    }
    public static bool IsSceneActive(SceneType type)
    {
        Scene currScene = SceneManager.GetActiveScene();

        switch (type)
        {
            case SceneType.None:
                break;

            case SceneType.OutGame:
                if (currScene == SceneManager.GetSceneByName(CommonDefine.OutGameScene))
                    return true;
                break;
            case SceneType.InGame: //IngameScene + MapScene 2개 다 활성화 되어 있음
                if (currScene == SceneManager.GetSceneByName(CommonDefine.InGameScene)
                    || currScene.name.Equals(GetSceneName(NetworkManager_Client.Instance.Quantum_RoomData.mapID))
                    /*|| currScene.name.Contains("MapScene")*/)
                    return true;
                break;
        }

        return false;
    }

    public void LoadMapScene(int mapID, LoadSceneMode mode)
    {
        string sceneName = string.Empty;

        var allRefData = ReferenceManager.Instance.MapData?.MapInfoList;
        var map = allRefData.Find(x => x.MapID.Equals(mapID));
        if (map != null)
        {
            sceneName = map.MapAddressableKey();

            //나중에 GetDownloadSizeAsync 먼저해줘야함 (다운로드 미리 받아보자...)
            var handle = Addressables.LoadSceneAsync(sceneName, mode);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    NetworkManager_Client.Instance.Quantum_RoomData.isMapSceneLoaded = true;
                    Debug.Log("Map Scene >> " + sceneName + " loaded successfully!");
                }
                else
                {
                    Debug.LogError("Failed to load scene: " + sceneName);
                }
            };
        }
    }

    public static string GetSceneName(int mapID)
    {
        string sceneName = string.Empty;

        var allRefData = ReferenceManager.Instance.MapData?.MapInfoList;
        var map = allRefData.Find(x => x.MapID.Equals(mapID));
        if (map != null)
        {
            sceneName = map.MapSceneName;
        }

        return sceneName;
    }
}
