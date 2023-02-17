using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        yield return null;

        //Load Local Data...
        //Load Ref
        //Load DB
        //Login

        UIManager_NGUI.Instance.ActivateUI(UIManager_NGUI.UIType.UI_PanelSprite);

        PhotonNetworkManager.Instance.CreatePhotonNetworkRunner();

        DataManager.Instance.SetFirebaseConnection();

        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (DataManager.Instance.isFirebaseConnected
                /*
                && Photon.Pun.PhotonNetwork.IsConnected
                && PhotonNetworkManager.IsConnectedToMaster*/)
                break;

            yield return null;
        }
        
        //꼭 순차적으로 처리하자....! 안그럼 이상한 현상 나옴....!

        DataManager.Instance.LoadBasicData();

        while (DataManager.Instance.isBasicDataLoaded == false)
            yield return null;

        DataManager.Instance.LoadUserData(SystemInfo.deviceUniqueIdentifier);

        while (DataManager.Instance.isUserDataLoaded == false)
            yield return null;

        DataManager.Instance.LoadCarData();

        while (DataManager.Instance.isCarDataLoaded == false)
            yield return null;

        DataManager.Instance.LoadMapData();

        while (DataManager.Instance.isMapDataLoaded == false)
            yield return null;

        yield return null;

        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);
    }

}
