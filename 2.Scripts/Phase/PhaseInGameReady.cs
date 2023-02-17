using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PhaseInGameReady : PhaseBase
{
    IEnumerator phaseInGameReadyCoroutine;

    public Action OnFailToFindOtherPlayers = null;

    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkInGameRPCManager myNetworkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInGameReadyCoroutine, PhaseInGameReadyCoroutine(), this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

    }

    IEnumerator PhaseInGameReadyCoroutine()
    {
        DataManager.Instance.SaveUserData();

        yield return new WaitForFixedUpdate();

        PhotonNetworkManager.cancelSearchingRoom = false;
        PhotonNetworkManager.isWaitingForOtherPlayers = true;

        if (PhotonNetworkManager.Instance.MyNetworkRunner != null
            && PhotonNetworkManager.Instance.MyNetworkRunner.SessionInfo != null)
        {
            if (PhotonNetworkManager.Instance.IsHost)
                PhotonNetworkManager.Instance.MyNetworkRunner.SessionInfo.IsOpen = true;
        }

        PhotonNetworkManager.isMatchSet = false;
        PhotonNetworkManager.matchSuccess = false;
        PhotonNetworkManager.timeWaitingForOtherPlayers = CommonDefine.GAME_TIME_WAITING_BEFORE_START;

        while (true)
        {
            if (PhotonNetworkManager.isMatchSet)
                break;

            if (PhotonNetworkManager.Instance.ListOfNetworkRunners != null)
            {
                if (PhotonNetworkManager.Instance.ListOfNetworkRunners.Count == CommonDefine.GetMaxPlayer()
                    && PhotonNetworkManager.cancelSearchingRoom == false)
                {
                    if (PhotonNetworkManager.Instance.IsHost)
                        myNetworkInGameRPCManager.RPC_MatchSuccess(true);
                }
            }

            //취소 버튼 누르지 않았는데 유저 대기 시간이 모두 소모 되었을때
            if (PhotonNetworkManager.cancelSearchingRoom == false
                && PhotonNetworkManager.timeWaitingForOtherPlayers <= 0
                && PhotonNetworkManager.matchSuccess == false)
            {
                //최소 대기인원 이상이면 진행
                if (PhotonNetworkManager.Instance.ListOfNetworkRunners.Count >= CommonDefine.GetMinPlayer())
                {
                    if (PhotonNetworkManager.Instance.IsHost)
                        myNetworkInGameRPCManager.RPC_MatchSuccess(true);
                }
                else //최소 대기인원 이하면 실패
                {
                    if (PhotonNetworkManager.Instance.IsHost)
                        myNetworkInGameRPCManager.RPC_MatchSuccess(false);
                }
            }

            //취소 버튼 눌렀을 경우
            if (PhotonNetworkManager.cancelSearchingRoom == true)
            {
                LeaveRoomCallback();
            }

            PhotonNetworkManager.timeWaitingForOtherPlayers -= Time.fixedDeltaTime; //무조건 fixedDelta로 해야함

            yield return new WaitForFixedUpdate();

#if CHEAT
            if (CommonDefine.isForcePlaySolo == true)
            {
                if (PhotonNetwork.IsMasterClient)
                    InGameManager.Instance.RaiseEvent_MatchSuccess(true);
            }

            if (PhotonNetworkManager.isMatchSet)
                break;
#endif
        }

        if (PhotonNetworkManager.matchSuccess == true) //매치 성공
        {
            Debug.Log("Match Success!");
            yield return new WaitForSecondsRealtime(1f);
            /*
            if (PhotonNetwork.InRoom == true)
                PhotonNetwork.CurrentRoom.IsOpen = false;
            */
            PhotonNetworkManager.isJoiningRoom = false;
            PhotonNetworkManager.isJoinedRoom = false;
            PhotonNetworkManager.isWaitingForOtherPlayers = false;
            PhotonNetworkManager.cancelSearchingRoom = false;

            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGame);

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.MatchSuccess);
        }
        else //매치 실패
        {
            PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
        }
    }

    private void LeaveRoomCallback()
    {
        Debug.Log("Leaving Room...");
        Debug.Log("Returning to Lobby");
        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby); //로비로 돌아가자

        OnFailToFindOtherPlayers?.Invoke();
    }
}
