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

        if (MapObjectManager.Instance.podium != null)
        {
            CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_SubCam_MapOutroCeremony);
            MapObjectManager.Instance.podium.ActivatePodium();
        }

        //추후 서버에서 관리...?
        UtilityCoroutine.StartCoroutine(ref autoLeaveRoomAfterTime, AutoLeaveRoomAfterTime(), this);
    }


    private IEnumerator autoLeaveRoomAfterTime;
    private IEnumerator AutoLeaveRoomAfterTime()
    {
        //30초 후 자동으로 꺼주자...
        yield return new WaitForSecondsRealtime(DataManager.GAME_AUTO_LEAVE_ROOM_TIME);

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
        {
            PhotonNetworkManager.Instance.LeaveSession(InGameManager.Instance.LeaveRoomCallback);
        }
    }

}
