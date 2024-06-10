using Quantum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseOutGame : PhaseBase
{
    IEnumerator phaseOutGameCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.InGame)) //인게임씬인 경우 아웃게임씬으로 전환
        {
            SceneManagerExtensions.Instance.LoadScene(CommonDefine.OutGameScene, LoadSceneMode.Single);
        }
        else if (SceneManagerExtensions.IsSceneActive(SceneManagerExtensions.SceneType.OutGame)) //기존 씬을 유지한채 다시 로드했을 경우...
        {
            UtilityCoroutine.StartCoroutine(ref phaseOutGameCoroutine, PhaseOutGameCoroutine(), this);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator PhaseOutGameCoroutine()
    {
        yield return null; //꼭 1프레임 넣어주자 (Photon Connect 때문에)

        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        UI_Title.Activate("Connecting to P Server", false);
        NetworkManager_Client.Instance.SelectPhotonServerRegion();
        NetworkManager_Client.Instance.ConnectToPhotonServer();

        while (true)
        {
            if (NetworkManager_Client.Instance.Quantum_IsConnectedAndReady && NetworkManager_Client.Instance.Quantum_IsConnectedToMaster)
                break;

            yield return null;
        }

        UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);

        UIManager.Instance.HideGrouped_PreOutgame();
        UIManager.Instance.HideUI(UIManager.UIType.UI_InGameReady);
        UIManager.Instance.HideGrouped_Ingame();

        OutGameManager.Instance.StartOutGame();

        PrefabManager.Instance.UI_SceneTransition.Show_Out();

#if UNITY_EDITOR || SERVERTYPE_DEV
        UIManager.Instance.ShowUI(UIManager.UIType.UI_Debug);
#endif

        yield return null;
        

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals(CommonDefine.OutGameScene))
        {
            SceneManagerExtensions.Instance.SetActiveScene((CommonDefine.OutGameScene));
            UtilityCoroutine.StartCoroutine(ref phaseOutGameCoroutine, PhaseOutGameCoroutine(), this);
        }
    }

}
