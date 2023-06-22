using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseLobby : PhaseBase
{
    IEnumerator phaseLobbyCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnSceneLoaded;

        Scene currScene = SceneManager.GetActiveScene();

        if (currScene.name.Equals(CommonDefine.InGameScene)) //인게임씬인 경우 아웃게임씬으로 전환
        {
            InGameManager.Instance.InitializeSettings();
            SceneManager.LoadScene(CommonDefine.OutGameScene);
        }
        else if (currScene.name.Equals(CommonDefine.OutGameScene)) //기존 씬을 유지한채 다시 로드했을 경우...
        {
            UtilityCoroutine.StartCoroutine(ref phaseLobbyCoroutine, PhaseLobbyCoroutine(), this);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator PhaseLobbyCoroutine()
    {
        while (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Initialize)
            yield return null;

        yield return null;

        PhotonNetworkManager.Instance.ResetNetworkMatchSettings();
        InGameManager.Instance.InitializeInGameUI(); //Ingame씬에서 온경우... 지워주자...
        OutGameManager.Instance.InitializeOutGameUI();

        RobotSystemManager.Instance.ActivateSearchRobotTimer();

        bool allSceneIsLoaded = false;
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

        yield return null;
        OutGameManager.Instance.SpawnOutGameObjects();
        yield return null;
        Camera_Base.Instance.InitializeSettings();
        CameraManager.Instance.SetOutGameCam();
        yield return null;
        OutGameManager.Instance.SetOutGamePlayerData();
        yield return null;
        OutGameManager.Instance.ActivateOutGamePlayerAndCam();
        yield return null;

        UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelInGame);
        UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelSprite);
        UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelLoading);

        UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelLobby_Main);
        UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelLobby_TabMenu);

        SoundManager.Instance.StopSound(SoundManager.SoundClip.Drive);
        SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.Outgame_BGM);

        yield return null;

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals(CommonDefine.OutGameScene))
        {
            UtilityCoroutine.StartCoroutine(ref phaseLobbyCoroutine, PhaseLobbyCoroutine(), this);
        }
    }

}
