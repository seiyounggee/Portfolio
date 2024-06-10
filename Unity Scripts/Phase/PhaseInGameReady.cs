using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseInGameReady : PhaseBase
{
    IEnumerator phaseInGameReadyCoroutine;

    private float roomWaitTime = 10f;

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
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        UIManager.Instance.ShowUI(UIManager.UIType.UI_InGameReady);

        yield return null;

        var isSuccess = NetworkManager_Client.Instance.JoinRoom();

        if (isSuccess == false)
        {
            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.OutGame);
            yield break;
        }

        var errorCheckTimer = 5f;
        while (true)
        {
            if (NetworkManager_Client.QuantumClient != null && NetworkManager_Client.QuantumClient.CurrentRoom != null)
                break;

            errorCheckTimer -= Time.deltaTime;

            if (errorCheckTimer <= 0)
            {
                //제대로 방에 들어 가지 않았을 경우.....
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.OutGame);
                break;
            }

            yield return null;
        }

        roomWaitTime = GetRoomMatchTime(); //wait time for other players... if waitTime turns to 0 all other players are ai (if AI match)
        while (true)
        {
            roomWaitTime -= Time.deltaTime;

            //대기 시간 모두 소요
            if (NetworkManager_Client.Instance.Quantum_IsInRoom 
                && (NetworkManager_Client.Instance.Quantum_IsRoomFull || roomWaitTime <= 0))
            {
                //일반 유저로 다 찼을 경우
                if (NetworkManager_Client.Instance.Quantum_IsRoomFull)
                {
                    NetworkManager_Client.Instance.CloseRoom();
                    PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGame); //인게임 집입
                    break;
                }
                else if (roomWaitTime <= 0) //일반 유저가 다 차지 않았는데 시간이 다 소요되었을 경우
                {
                    NetworkManager_Client.Instance.CloseRoom();
                    PrefabManager.Instance.UI_InGameReady.MakeFullCount(); //강제로 StatusText FullCount로 갱신

                    if (IsAIMatch()) //AI매칭이 가능한 경우
                    {
                        //AI 매치 가능...! Phase Ingame으로 보내자
                        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGame);
                        break;
                    }
                    else
                    {
                        //AI 매칭 불가능 하고 모든 대기 시간이 소요 되었을 경우...
                        var commonUI = PrefabManager.Instance.UI_CommonPopup;
                        commonUI.SetUp(UI_CommonPopup.CommonPopupType.Okay_WithoutExit, "MATCHMAKING_TIMEOVER_TITLE", "MATCHMAKING_TIMEOVER_DESC", () =>
                        {
                            UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                            NetworkManager_Client.Instance.LeaveRoom();
                        });
                        UIManager.Instance.ShowUI(commonUI);
                        break;
                    }
                }
                   
            }

            if (NetworkManager_Client.Instance.Quantum_IsConnectedAndReady == false || NetworkManager_Client.Instance.Quantum_IsConnectedToMaster == false)
            {
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.OutGame);
                yield break;
            }

            yield return null;
        }
    }

    public void SetRoomWaitTime(float time)
    {
        roomWaitTime = time;
    }

    private float GetRoomMatchTime()
    {
        float roomWaitTime = 10f;

        if (NetworkManager_Client.QuantumClient != null && NetworkManager_Client.QuantumClient.CurrentRoom != null
            && NetworkManager_Client.QuantumClient.CurrentRoom.CustomProperties.TryGetValue(NetworkManager_Client.ROOM_PROPERTIES_MatchMakingGroup, out var group))
        {
            roomWaitTime = ReferenceManager.Instance.GetMatchRoomWaitTime((CommonDefine.MatchMakingGroup)((short)group));
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("<color=red>Error no exsisting group...?</color>");
#endif
        }

        return roomWaitTime;
    }

    private bool IsAIMatch()
    {
        bool isAIMatch = false;

        if (NetworkManager_Client.QuantumClient.CurrentRoom.CustomProperties.TryGetValue(NetworkManager_Client.ROOM_PROPERTIES_MatchMakingGroup, out var group))
        {
            isAIMatch = ReferenceManager.Instance.IsAIMatchAvailable((CommonDefine.MatchMakingGroup)((short)group));
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("<color=red>Error no exsisting group...?</color>");
#endif
        }

        return isAIMatch;
    }
}
