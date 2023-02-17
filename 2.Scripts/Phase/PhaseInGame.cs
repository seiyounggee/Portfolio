﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class PhaseInGame : PhaseBase
{
    IEnumerator phaseInGameCoroutine;

    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkInGameRPCManager myNetworkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;
    
    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInGameCoroutine, PhaseInGameCoroutine(), this);
    }

    IEnumerator PhaseInGameCoroutine()
    {
        UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelCommon);
        UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelNickname);
        UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelLoading); //임시...?

        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene(CommonDefine.InGameScene);

        string sceneName = "";
        var md = DataManager.Instance.mapData;
        var bd = DataManager.Instance.basicData;
        if (md != null && bd != null)
        {
            foreach (var i in md.MapDataList)
            {
                if (i.id.Equals(bd.gameData.GAME_MAP_ID))
                {
                    sceneName = i.SCENE_NAME;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(sceneName) == false && sceneName != "NONE")
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        yield return null;

        bool allSceneIsLoaded = false;
        //인게임 씬이 로드 될때까지 기달려주자
        while (true)
        {
            if (allSceneIsLoaded == true)
                break;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded == false)
                    break;

                if (i == SceneManager.sceneCount - 1)
                    allSceneIsLoaded = true;
            }

            yield return null;
        }

        myNetworkInGameRPCManager.RPC_SceneIsLoaded(PhotonNetworkManager.Instance.MyNetworkRunnerInfo.playerId);

        UIManager_NGUI.Instance.DeactivateLobbyUI();
        UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelInGame);

        //게임 시작
        InGameManager.Instance.StartGame();
        //PhotonNetworkManager.Instance.ChangePhotonNetworkRate(1);

        SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.Ingame_BGM_01);
    }


    public string GetMapSceneName()
    {
#if UNITY_EDITOR
        string sceneName = "";
        foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
        {
            if (scene.path.Contains(CommonDefine.InGameScene))
                continue;
            if (scene.path.Contains(CommonDefine.OutGameScene))
                continue;

            string sceneNameOnly = "";
            if (scene.path.Contains("Assets/1.Scenes/Map/"))
            {
                sceneNameOnly = scene.path.Replace("Assets/1.Scenes/Map/", "");
                sceneNameOnly = sceneNameOnly.Replace(".unity", "");
            }
            sceneName = sceneNameOnly;
            break;
        }

        return sceneName;
#endif
        return "";
    }

}