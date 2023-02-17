using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkInGameRPCManager : NetworkBehaviour
{
    [Networked] public int StartRaceTick { get; set; }

    [Networked] public int EndRaceTick { get; set; }

    InGameManager inGameManager => InGameManager.Instance;

    public bool IsMine { get { return Object != null && Object.HasInputAuthority; } }

    public bool IsLeader => Object != null && Object.IsValid && Object.HasStateAuthority;

    public int ID
    {
        get { return Id.Behaviour; }
    }

    private void Awake()
    {
        PhotonNetworkManager.Instance.MyNetworkInGameRPCManager = this;
    }

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        StartRaceTick = 0;
        EndRaceTick = 0;

        if (IsMine == false)
        {
            transform.SetParent(inGameManager.transform);
            gameObject.name = "NetworkInGameRPCManager_" + ID;
        }
        else
        {
            transform.SetParent(inGameManager.transform);
            gameObject.name = "NetworkInGameRPCManager_" + ID;
        }
    }


    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SetPlayerToPosition(int id, int spawnIndex)
    {
        Debug.Log("<color=yellow>Photon OnEvent! EVENT_SetPlayerToPosition</color>");

        foreach (var player in inGameManager.ListOfPlayers)
        {
            if (player.pm != null)
            {
                if (player.pm.PlayerID.Equals(id))
                {
                    player.pm.InitializePosition(spawnIndex);
                }
            }
        }

        if (inGameManager.myPlayer.Equals(id))
        {
            CameraManager.Instance.ChangeCamType(CameraManager.CamType.RotateAroundPlayer);
        }

        inGameManager.playerIsSetInPosition = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SetPlayerStats(string jsonData)
    {
        InGameManager.PlayerData playerData = new InGameManager.PlayerData();
        playerData = JsonUtility.FromJson<InGameManager.PlayerData>(jsonData);

        if (playerData != null)
        {
            Debug.Log("<color=yellow>Photon OnEvent! EVENT_SetPlayerStats: viewID-" + playerData.playerId + " | playerData.carID: " + playerData.carID + " | playerData.characterID: " + playerData.characterID + "</color>");

            if (playerData.playerId == inGameManager.myPlayer.PlayerID)
                inGameManager.myPlayerDataIsSent = true;

            foreach (var i in inGameManager.ListOfPlayers)
            {
                if (i.playerID.Equals(playerData.playerId))
                {
                    i.data = playerData;
                    i.pm.SetCar(playerData.carID);
                    i.pm.SetCarData(playerData.carID);
                    i.pm.SetCharacter(playerData.characterID);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_MatchSuccess(bool isSuccess)
    {
        if (isSuccess)
            PhotonNetworkManager.matchSuccess = true;
        else
            PhotonNetworkManager.matchSuccess = false;

        PhotonNetworkManager.isMatchSet = true;

        if (PhotonNetworkManager.Instance.MyNetworkRunner != null 
            && PhotonNetworkManager.Instance.MyNetworkRunner.SessionInfo != null)
        {
            PhotonNetworkManager.Instance.MyNetworkRunner.SessionInfo.IsOpen = false;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SceneIsLoaded(int id)
    {
        Debug.Log("<color=yellow>Photon RPC_SceneIsLoaded!   id:" + id + "</color>");

        foreach (var i in PhotonNetworkManager.Instance.ListOfPlayerSceneLoaded)
        {
            if (id.Equals(i.id))
            {
                i.isLoaded = true;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetLaneAndMoveIndex(int playerID, int lanetype, int moveIndex)
    {
        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null)
        {
            p.pm.OnEvent_SetCurrentLaneNumberTypeAndMoveIndex((PlayerMovement.LaneType)lanetype, moveIndex);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetChargePad(int chargePadID, int charPadLv, bool isReset, int playerID = -1)
    {
        if (inGameManager.mapObjectManager != null)
        {
            if (isReset == false)
                inGameManager.mapObjectManager.OnEvent_ActivateChargePad(playerID, chargePadID, (MapObject_ChargePad.ChargePadLevel)charPadLv);
            else
                inGameManager.mapObjectManager.OnEvent_ResetChargePad(chargePadID, (MapObject_ChargePad.ChargePadLevel)charPadLv);
        }

        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null)
        {
            if (isReset == false)
                p.pm.OnEvent_ActivateChargePad(playerID, chargePadID, (MapObject_ChargePad.ChargePadLevel)charPadLv);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SpawnPlayerToPosition(int playerID, int lane, int moveIndex)
    {
        //특정 지역으로 즉각 이동
        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null)
        {
            p.pm.OnEvent_SpawnPlayerToPosition(lane, moveIndex);

            if (p.pm.IsMine)
                CameraManager.Instance.ChangeCamType(CameraManager.CamType.Follow_Type_Default);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ActivateContainerBox(int playerID, int containerID)
    {
        if (inGameManager.mapObjectManager != null)
        {
            inGameManager.mapObjectManager.OnEvent_ActivateContainerBox(containerID);
        }
    }


    public enum SetBatteryType { Normal, CarBooster, ChargePadBooster, ChargeZone }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetPlayerBattery(int playerID, bool isAdded, int battery, int type = 0)
    {
        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null)
        {
            //isAdded일 경우 추가 해주고 아닐 경우 바태리 값 세팅 해주자...!
            if (isAdded == false)
                p.pm.SetCurrentBattery(battery);
            else
            {
                p.pm.SetCurrentBatteryAdded(battery);

                if (p.pm.IsMine && (SetBatteryType)type == SetBatteryType.CarBooster)
                {
                    var ui = PrefabManager.Instance.UI_PanelIngame;
                    ui.ActivateMovingProgressBar();
                }
            }
        }
    }

    public void RaiseEvent_EVENT_SpawnFoodTruckToPosition(int id, int lane, int moveIndex)
    {
        /*
        object[] content = new object[] { id, lane, moveIndex };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
        ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(PhotonEventRPCDefine.EVENT_SpawnFoodTruckToPosition, content, raiseEventOptions, sendOptions);
        */
    }

    public void RaiseEvent_EVENT_FoodTruckSetLaneAndMoveIndex(int id, int lane, int moveIndex)
    {
        /*
        if (id == -1)
        {
            Debug.Log("Error.... food truck id is not set properly!");
            return;
        }

        object[] content = new object[] { id, lane, moveIndex };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache };
        ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(PhotonEventRPCDefine.EVENT_FoodTruckSetLaneAndMoveIndex, content, raiseEventOptions, sendOptions);
        */

    }


    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_IsReady(int playerID)
    {
        /*
        if (myPlayer != null)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
            ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(PhotonEventRPCDefine.EVENT_IsReady, myPlayer.PlayerID, raiseEventOptions, sendOptions);
        }
        */

        foreach (var i in inGameManager.ListOfPlayers)
        {
            if (playerID.Equals(i.playerID))
            {
                i.isReady = true;
            }
        }
    }

    //Invoke 로 호출됨!!!
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_StartCountDown()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return;

        if (inGameManager.isStartCountDown)
            return;

        Debug.Log("<color=yellow>Photon OnEvent! EVENT_StartCountDown</color>");

        inGameManager.isStartCountDown = true;

        UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelLoading);

        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;

        if (ingamePanel.gameObject.activeSelf == true)
        {
            ingamePanel.ActivateCountDownTxt(CommonDefine.START_COUNTDOWN_TIME);
        }

        //camChangeTime 이후에 카메라 바꿔주자...
        var camChangeTime = CommonDefine.START_COUNTDOWN_TIME - 3f;
        if (camChangeTime > 0)
            this.Invoke(() => { if (inGameManager.gameState == InGameManager.GameState.StartCountDown) CameraManager.Instance.ChangeCamType(CameraManager.CamType.Follow_Type_Default); }, camChangeTime);

        if (inGameManager.myPlayer != null)
            RPC_SetPlayerBattery(inGameManager.myPlayer.PlayerID, true, inGameManager.myPlayer.PLAYER_BATTERY_START_AMOUNT);
    }


    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayGame()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return;

        if (inGameManager.isPlayGame)
            return;

        Debug.Log("<color=yellow>Photon OnEvent! EVENT_PlayGame</color>");

        inGameManager.isPlayGame = true;
        //playStartTime = Fusion.TickTimer.

        CameraManager.Instance.ChangeCamType(CameraManager.CamType.Follow_Type_Default);

        foreach (var player in inGameManager.ListOfPlayers)
        {
            player.pm.StartMoving();
        }

        StartRaceTick = PhotonNetworkManager.Instance.MyNetworkRunner.Simulation.Tick;

        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
        ingamePanel.ActivateIngamePlayTxt();
        ingamePanel.ActivateGoTxt();
        ingamePanel.ActivateParryingCooltimeGO();
        ingamePanel.ActivateNickname();

        UtilityCoroutine.StartCoroutine(ref inGameManager.updateRankList, inGameManager.UpdateRankList(), this);
        UtilityCoroutine.StartCoroutine(ref inGameManager.updatePlayerBehindList, inGameManager.UpdatePlayerBehindList(), this);
        UtilityCoroutine.StartCoroutine(ref inGameManager.updateMapObjectList, inGameManager.UpdateMapObjectList(), this);
        UtilityCoroutine.StartCoroutine(ref inGameManager.updateMiniMap, inGameManager.UpdateMiniMap(), this);
    }

    //Invoke 로 호출됨!!!
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    private void RPC_EndGame()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return;

        if (inGameManager.isGameEnded == true)
        {
            //2번 보내주는 경우가 있을까?? 없을것 같은데
            Debug.Log("<color=red>Game is already Ended...!</color>");
            return;
        }

        Debug.Log("<color=yellow>Photon OnEvent! EVENT_EndGame</color>");

        inGameManager.isPlayGame = false;
        inGameManager.isGameEnded = true;

        StartCoroutine(inGameManager.ChangeGameState(InGameManager.GameState.EndGame));

        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameResult);
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ChangeLane_Left(int playerID)
    {
        if (inGameManager.BlockInput())
            return;

        if (inGameManager.isInputDelay)
            return;

        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.isFlipped)
                return;

            if (pm.isOutOfBoundary)
                return;

            if (pm.isEnteredTheFinishLine)
                return;

            foreach (var i in inGameManager.ListOfPlayers)
            {
                if (i.go != null && i.pm != null)
                {
                    if (i.playerID.Equals(inGameManager.myPlayer.PlayerID))
                        i.pm.SetIngameInput(PlayerMovement.MoveInput.Left, pm.transform.position, pm.currentMoveIndex, pm.currentLaneType, PlayerMovement.CarBoosterLevel.None);
                }
            }

            inGameManager.ActivateInputDelay();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ChangeLane_Right(int playerID)
    {
        if (inGameManager.BlockInput())
            return;

        if (inGameManager.isInputDelay)
            return;

        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.isFlipped)
                return;

            if (pm.isOutOfBoundary)
                return;

            if (pm.isEnteredTheFinishLine)
                return;

            foreach (var i in inGameManager.ListOfPlayers)
            {
                if (i.go != null && i.pm != null)
                {
                    if (i.playerID.Equals(inGameManager.myPlayer.PlayerID))
                        i.pm.SetIngameInput(PlayerMovement.MoveInput.Right, pm.transform.position, pm.currentMoveIndex, pm.currentLaneType, PlayerMovement.CarBoosterLevel.None);
                }
            }

            inGameManager.ActivateInputDelay();
        }
    }


    //자동으로 가능한 Booster 사용
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_BoostPlayer(int playerID)
    {
        if (inGameManager.BlockInput())
            return;

        if (inGameManager.isInputDelay)
            return;

        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.isGrounded == false)
                return;

            if (pm.isOutOfBoundary)
                return;

            if (pm.isEnteredTheFinishLine)
                return;

            if (pm.isStunned)
                return;

            foreach (var i in ListOfPlayers)
            {
                if (i.go != null && i.pm != null)
                {
                    if (i.playerID.Equals(pm.PlayerID))
                        i.pm.SetIngameInput(PlayerMovement.MoveInput.Boost, pm.transform.position, pm.currentMoveIndex, pm.currentLaneType, PlayerMovement.CarBoosterLevel.None);
                }
            }

            inGameManager.ActivateInputDelay();
        }
    }

    // input이 아니라 특정 상황에서 Event가 Raise되는 용도..
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_BoostPlayer(int playerID, int boosterLv)
    {
        if (inGameManager.BlockInput())
            return;

        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.playerID.Equals(playerID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.isGrounded == false)
                return;

            if (pm.isOutOfBoundary)
                return;

            if (pm.isEnteredTheFinishLine)
                return;

            if (pm.isStunned)
                return;

            foreach (var i in ListOfPlayers)
            {
                if (i.go != null && i.pm != null)
                {
                    if (i.playerID.Equals(pm.PlayerID))
                        i.pm.SetIngameInput(PlayerMovement.MoveInput.Boost, pm.transform.position, pm.currentMoveIndex, pm.currentLaneType, (PlayerMovement.CarBoosterLevel)boosterLv);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Deceleration_Start(int playerID)
    {
        if (inGameManager.BlockInput())
            return;

        if (inGameManager.isInputDelay)
            return;

        var myPlayer = inGameManager.myPlayer;
        var ListOfPlayers = inGameManager.ListOfPlayers;

        if (myPlayer != null && myPlayer.IsMine && myPlayer.isDecelerating == false)
        {
            if (myPlayer.isStunned)
                return;

            if (myPlayer.isFlipped)
                return;

            if (myPlayer.isEnteredTheFinishLine)
                return;

            if (myPlayer.isGrounded == false)
                return;

            if (myPlayer.isOutOfBoundary)
                return;

            foreach (var i in ListOfPlayers)
            {
                if (i.go != null && i.pm != null)
                {
                    if (i.playerID.Equals(myPlayer.PlayerID))
                        i.pm.SetIngameInput(PlayerMovement.MoveInput.Deceleration_Start, myPlayer.transform.position, myPlayer.currentMoveIndex, myPlayer.currentLaneType, PlayerMovement.CarBoosterLevel.None);
                }
            }

            inGameManager.ActivateInputDelay();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Deceleration_End(int playerID)
    {
        if (inGameManager.BlockInput())
            return;

        var myPlayer = inGameManager.myPlayer;
        var ListOfPlayers = inGameManager.ListOfPlayers;

        if (myPlayer != null && myPlayer.IsMine && myPlayer.isDecelerating == true)
        {
            foreach (var i in ListOfPlayers)
            {
                if (i.go != null && i.pm != null)
                {
                    if (i.playerID.Equals(myPlayer.PlayerID))
                        i.pm.SetIngameInput(PlayerMovement.MoveInput.Deceleration_End, myPlayer.transform.position, myPlayer.currentMoveIndex, myPlayer.currentLaneType, PlayerMovement.CarBoosterLevel.None);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendPingInfo()
    {
        /*
        if (PhotonNetworkManager.Instance.MyNetworkPlayer != null && string.IsNullOrEmpty(PhotonNetworkManager.Instance.MyNetworkPlayer.UserId) == false)
        {
            string userID = PhotonNetworkManager.Instance.MyNetworkPlayer.UserId;
            int ping = PhotonNetworkManager.Instance.GetAveragePing();
            object[] content = new object[] { userID, ping };

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache };
            ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(PhotonEventRPCDefine.EVENT_SendPingInfo, content, raiseEventOptions, sendOptions);
        }
        */
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_EndCountDown()
    {
        Debug.Log("<color=yellow>Photon OnEvent! EVENT_EndCountDown</color>");

        inGameManager.isEndCountStarted = true;
        StartCoroutine(inGameManager.ChangeGameState(InGameManager.GameState.EndCountDown));

        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
        if (ingamePanel.gameObject.activeSelf == true)
        {
            ingamePanel.ActivateCountDownTxt(CommonDefine.END_COUNTDOWN_TIME);
        }

        //마스터 클라이언트가 게임 종료 알림....!
        if (PhotonNetworkManager.Instance.IsHost)
            Invoke("RPC_EndGame", CommonDefine.END_COUNTDOWN_TIME);
        else
        {
            //혹시나 마스터 클라이언트가 상태가 메롱하면... 다른 사람이 알려주자...!
            //간혹 이런 경우가 있....
            Invoke("RPC_EndGame", CommonDefine.END_COUNTDOWN_TIME + 3f);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayerPassedFinishLine(int playerID)
    {
        var myPlayer = inGameManager.myPlayer;
        var ListOfPlayers = inGameManager.ListOfPlayers;

        if (myPlayer == null)
            return;

        //내꺼인 경우...
        if (playerID.Equals(myPlayer.PlayerID) == true)
        {
            var ingameUI = PrefabManager.Instance.UI_PanelIngame;
            if (ingameUI.gameObject.activeSelf == true)
            {
                ingameUI.ActivatePassedFinishLineTxt();
                ingameUI.DeactiveHUDBase();
                Camera_Base.Instance.TurnOffCameraFX();
            }

            SoundManager.Instance.StopSound(SoundManager.SoundClip.Ingame_BGM_01, SoundManager.StopSoundType.Immediate);
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.PassFinishLine);
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Victory, SoundManager.PlaySoundType.Delay, 1.5f);

            EndRaceTick = PhotonNetworkManager.Instance.MyNetworkRunner.Simulation.Tick;
        }

        foreach (var i in ListOfPlayers)
        {
            if (i.playerID.Equals(i.playerID))
            {
                i.pm.isEnteredTheFinishLine = true;
                i.isEnterFinishLine_Network = true;
                i.enterFinishLineTime_Network = inGameManager.totalTimeGameElapsed;
            }
        }

        if (inGameManager.isEndCountStarted == false)
        {
            //마스터 클라이언트가 게임 종료 카운트다운을 알림!
            if (PhotonNetworkManager.Instance.IsHost)
            {
                RPC_EndCountDown();
            }
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayerCompletedLap(int playerID, int lapNumber) //next lap 기준으로 들어옴!
    {
        var myPlayer = inGameManager.myPlayer;
        var ListOfPlayers = inGameManager.ListOfPlayers;

        //내 플레이어의 경우...!
        if (myPlayer != null && myPlayer.PlayerID == playerID)
        {
            var ingameUI = PrefabManager.Instance.UI_PanelIngame;
            string msg = "";

            if (inGameManager.IsFinishLap(lapNumber) == false && lapNumber >= 0)
            {
                if (inGameManager.IsLastLap(lapNumber) == false)
                {
                    if (CommonDefine.GetFinalLapCount() != -1)
                        msg = "LAP " + (lapNumber + 1).ToString() + "/" + CommonDefine.GetFinalLapCount().ToString();
                    else
                        msg = "LAP " + (lapNumber + 1).ToString() + "/-";

                    SoundManager.Instance.PlaySound(SoundManager.SoundClip.LapCheckPoint);
                }
                else
                    msg = "FINAL LAP!";

                ingameUI.ActivateTextBase(msg);
            }
        }
    }
}
