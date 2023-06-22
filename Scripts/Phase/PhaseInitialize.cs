using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseInitialize : PhaseBase
{
    IEnumerator phaseInitializeCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInitializeCoroutine, PhaseInitializeCoroutine(), this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

    }

    IEnumerator PhaseInitializeCoroutine()
    {
        yield return LoadSceneCoroutine(); //현재 OutGameScene인지 확인하자

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        yield return null;

        PnixNetworkManager.Instance.Initialize();
        PhotonNetworkManager.Instance.Initialize();
        DataManager.Instance.Initialize();
        AccountManager.Instance.Initialize();
        AssetManager.Instance.Initialize_AssetBundleManager();
        ClientVersion.Initialize();

        UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelSprite);

        yield return null;

        PnixNetworkManager.Instance.CheckResourceServer();
        while (PnixNetworkManager.Instance.isResouceLoaded == false)
            yield return null;
        while (DataManager.Instance.isPnixReferenceDataLoaded == false)
            yield return null;

        PnixNetworkManager.Instance.JoinAndLoginToPnixGameServer();
        while (PnixNetworkManager.Instance.isLoginSuccess == false)
            yield return null;

        PhotonNetworkManager.Instance.LeaveSession(() => PhotonNetworkManager.Instance.CreatePhotonNetworkRunner()); ;

        while (AssetManager.Instance.IsDefaultAssetsAllDownloaded() == false)
            yield return null;

        AssetManager.Instance.SetAssetBundleToManager();

        yield return null;

        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);
    }

    IEnumerator LoadSceneCoroutine()
    {
        //현재 OutGameScene인지 확인하자

        Scene currScene = SceneManager.GetActiveScene();
        if (currScene.name.Equals(CommonDefine.InGameScene))
        {
            InGameManager.Instance.InitializeSettings();
            SceneManager.LoadScene(CommonDefine.OutGameScene);
        }
        else if (currScene.name.Equals(CommonDefine.OutGameScene))
        {
            yield break;
        }


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

        yield break;
    }
}
