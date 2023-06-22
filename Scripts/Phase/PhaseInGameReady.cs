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
            if (PhotonNetworkManager.Instance.IsRoomMasterClient)
                PhotonNetworkManager.Instance.SetSessionOpen(true);
        }

        PhotonNetworkManager.isMatchSet = false;
        PhotonNetworkManager.matchSuccess = false;
        PhotonNetworkManager.timeWaitingForOtherPlayers = DataManager.GAME_TIME_WAITING_BEFORE_START;

        bool matchNumSuccess = false;
        while (true)
        {
            if (PhotonNetworkManager.isMatchSet)
                break;

            if (PhotonNetworkManager.Instance.ListOfNetworkRunnerPlayerInfos != null)
            {
                if (PhotonNetworkManager.Instance.ListOfNetworkRunnerPlayerInfos.Count == DataManager.Instance.GetSessionRealPlayerCount()
                    && PhotonNetworkManager.cancelSearchingRoom == false)
                {
                    matchNumSuccess = true;
                    break;
                }
            }

            //유저 대기 시간이 모두 소모 되었을때
            if (PhotonNetworkManager.cancelSearchingRoom == false
                && PhotonNetworkManager.timeWaitingForOtherPlayers <= 0)
            {
                matchNumSuccess = false;
                break;
            }

            //취소 버튼 눌렀을 경우
            if (PhotonNetworkManager.cancelSearchingRoom == true)
            {
                LeaveRoomCallback();
                matchNumSuccess = false;
                break;
            }

            PhotonNetworkManager.timeWaitingForOtherPlayers -= Time.fixedDeltaTime; //무조건 fixedDelta로 해야함

            yield return new WaitForFixedUpdate();
        }

        if (matchNumSuccess)
        {
            if (PhotonNetworkManager.Instance.IsRoomMasterClient)
            {
                PhotonNetworkManager.Instance.CreateRPCManager();
            }

            while (true)
            {
                if (photonNetworkManager.MyNetworkInGameRPCManager != null
                    && PhotonNetworkManager.Instance.ListOfNetworkInGameRPCManager.Count == PhotonNetworkManager.Instance.ListOfNetworkRunnerPlayerInfos.Count)
                {
                    photonNetworkManager.MyNetworkInGameRPCManager.RPC_MatchSuccess(true);
                }

                if (PhotonNetworkManager.isMatchSet)
                {
                    break;
                }

                yield return null;
            }
        }
        else
        {
            PhotonNetworkManager.matchSuccess = false;
        }



        if (PhotonNetworkManager.matchSuccess == true) //매치 성공
        {
            Debug.Log("Match Success!");
            yield return new WaitForSecondsRealtime(1f);
            PhotonNetworkManager.isJoiningRoom = false;
            PhotonNetworkManager.isJoinedRoom = false;
            PhotonNetworkManager.isWaitingForOtherPlayers = false;
            PhotonNetworkManager.cancelSearchingRoom = false;

            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGame);

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.MatchSuccess);
        }
        else //매치 실패
        {
            PnixNetworkManager.Instance.SendCancelJoiningRace();
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
