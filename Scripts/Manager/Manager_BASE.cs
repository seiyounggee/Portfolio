using PNIX.Engine.NetClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_BASE : MonoSingleton<Manager_BASE>
{
    private bool isPaused = false;

    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;

        Debug.Log("OnApplicationPause: " + pauseStatus);

        if (isPaused == false)
        {
            //���ӿ� �ٽ� ������ ���... ��¦ �����̸� ������... ��...
            Invoke("ShowRestartUI", 1f);
        }
        else
        { 
            //Focus�� �Ҿ��� ���

        }
    }

    public void ShowRestartUI()
    {
        if (PhotonNetworkManager.ConnectionStatus == ConnectionStatus.Disconnected
            && PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.Initialize)
        {
            var ui = PrefabManager.Instance.UI_PanelPopup_Default;
            ui.Close();

            string msg = "Network Disconnected!\n Restart Game?";
            UIManager_NGUI.Instance.ActivatePanelDefault_Confirm(msg, () => { RestartGame(); });
        }
    }

    public void RestartGame()
    {
        if (PhotonNetworkManager.ConnectionStatus == ConnectionStatus.Disconnected
            || PhotonNetworkManager.ConnectionStatus == ConnectionStatus.Failed)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.Initialize)
            {
                CNetworkManager.Instance.Close(true);
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Initialize);
            }
        }
    }
}
