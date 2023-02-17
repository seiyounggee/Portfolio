using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseInGameResult : PhaseBase
{
    IEnumerator phaseInGameResultCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInGameResultCoroutine, PhaseInGameResultCoroutine(), this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    IEnumerator PhaseInGameResultCoroutine()
    {
        yield return null;

        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
        if (ingamePanel.gameObject.activeSelf == true)
        {
            ingamePanel.ActivateEndGameResultTxt();
        }

        UtilityCoroutine.StartCoroutine(ref autoLeaveRoomAfterTime, AutoLeaveRoomAfterTime(), this);
    }


    private IEnumerator autoLeaveRoomAfterTime;
    private IEnumerator AutoLeaveRoomAfterTime()
    {
        //30초 후 자동으로 꺼주자...
        yield return new WaitForSecondsRealtime(CommonDefine.GAME_AUTO_LEAVE_ROOM_TIME);

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
        {
            /*
            if (Photon.Pun.PhotonNetwork.OfflineMode || Photon.Pun.PhotonNetwork.CurrentRoom == null)
            {
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);

            }
            else
            {
                PhotonNetworkManager.Instance.LeaveRoom(LeaveRoomCallback);
            }
            */
            PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
        }
    }

    private void LeaveRoomCallback()
    {
        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);
    }
}
