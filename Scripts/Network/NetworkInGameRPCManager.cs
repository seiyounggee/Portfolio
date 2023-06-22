using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using static PlayerMovement;

public class NetworkInGameRPCManager : NetworkBehaviour, IObserver<NetworkInGameRPCManager.ObserverKey, NetworkId?>
{
    InGameManager inGameManager => InGameManager.Instance;
    PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;

    public bool IsMine { get { return Object != null && Object.HasInputAuthority; } }

    public NetworkId networkID
    {
        get { return Object.Id; }
    }

    #region Observer Pattern
    public enum ObserverKey
    {
        None,
        RPC_SetPlayerToPosition,
        RPC_SetPlayerStats,
        RPC_MatchSuccess,
        RPC_SceneIsLoaded,
        RPC_SetLaneAndMoveIndex,
        RPC_SetChargePad,
        RPC_SpawnPlayerToPosition,
        RPC_ActivateContainerBox,
        RPC_ActivateCatapult,
        RPC_SetPlayerBattery,
        RPC_IsSpawnReady,
        RPC_IsGameReady,
        RPC_StartCountDown,
        RPC_PlayGame,
        RPC_EndCountDown,
        RPC_EndGame,
        RPC_ChangeLane_Left,
        RPC_ChangeLane_Right,
        RPC_BoostPlayer,
        RPC_ShieldStart,
        RPC_ShieldStop,
        RPC_PlayerPassedFinishLine,
        RPC_PlayerCompletedLap,
        RPC_TriggerEnterOtherPlayer,
        RPC_GetStuned,
        RPC_GetBatteryBuff,
        RPC_GetFlipped,
        RPC_SetOutOfBoundary,
        RPC_StartDrafting,
        RPC_LightUpHeadlightsAndHonkHorn,
    }

    static Dictionary<MonoBehaviour, Dictionary<ObserverKey, List<Action<NetworkId?>>>> notify => IObserverBase<ObserverKey, NetworkId?>.notify;

    public void AttachObserver(MonoBehaviour observer, ObserverKey key, Action<NetworkId?> ac)
    {
        if (notify.ContainsKey(observer) == false)
        {
            var l = new List<Action<NetworkId?>>();
            l.Add(ac);
            var notify = new Dictionary<ObserverKey, List<Action<NetworkId?>>>();
            notify.Add(key, l);
            IObserverBase<ObserverKey, NetworkId?>.notify.Add(observer, notify);
        }
        else
        {
            if (notify[observer].ContainsKey(key) == false)
            {
                var l = new List<Action<NetworkId?>>();
                l.Add(ac);
                notify[observer].Add(key, l);
            }
            else
            {
                notify[observer][key].Add(ac);
            }
        }
    }

    public void DetachObserver(MonoBehaviour observer)
    {
        if (notify.ContainsKey(observer) == true)
            notify.Remove(observer);
    }

    public void NotifyToObservers(ObserverKey key, NetworkId? data)
    {
        foreach (var i in notify)
        {
            if (i.Value.ContainsKey(key))
            {
                foreach (var j in i.Value[key])
                {
                    if (data.HasValue)
                        j?.Invoke(data.Value);
                }
            }
        }
    }

    #endregion

    public override void Spawned()
    {
        base.Spawned();

        Initialize();

        if (IsMine)
        {
            if (photonNetworkManager != null)
                UtilityCoroutine.StartCoroutine(ref photonNetworkManager.recordPing, photonNetworkManager.RecordPing(), this);

            var panelIngame = PrefabManager.Instance.UI_PanelIngame;
            AttachObserver(panelIngame, ObserverKey.RPC_PlayerPassedFinishLine, panelIngame.PlayerPassedFinishLine_ObservedNotification);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        if (Runner != null && IsMine)
        {
            var panelIngame = PrefabManager.Instance.UI_PanelIngame;
            DetachObserver(panelIngame);
        }
    }

    public void Initialize()
    {
        PhotonNetworkManager.Instance.UpdateListOfNetworkInGameRPCManager(this);

        transform.SetParent(inGameManager.transform);
        gameObject.name = "NetworkInGameRPCManager_" + networkID;

        notify.Clear();
    }
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SetPlayerToPosition(NetworkId targetID, int spawnIndex)
    {
        Debug.Log("<color=yellow>Photon OnEvent! EVENT_SetPlayerToPosition : " + targetID + "  | spawnIndex: " + spawnIndex + "</color>");

        bool isAllPlayerSetToPosition = true;
        int count = 0;
        foreach (var player in inGameManager.ListOfPlayers)
        {
            if (player.pm != null)
            {
                if (player.pm.networkPlayerID.Equals(targetID))
                {
                    player.pm.InitializePosition(spawnIndex, count);
                }

                if (player.pm.isSetToPosition == false)
                    isAllPlayerSetToPosition = false;

                ++count;
            }
        }

        if (isAllPlayerSetToPosition)
        {
            if (inGameManager.gameState != InGameManager.GameState.PlayGame)
                CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_SubCam_MapIntro);
        }

        NotifyToObservers(ObserverKey.RPC_SetPlayerToPosition, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SetPlayerStats(string jsonData)
    {
        InGameManager.PlayerDataList listOfData = new InGameManager.PlayerDataList();
        listOfData = JsonUtility.FromJson<InGameManager.PlayerDataList>(jsonData);

        foreach (var data in listOfData.listOfPlayerData)
        {
            if (data != null)
            {
                //Debug.Log("<color=yellow>Photon OnEvent! EVENT_SetPlayerStats: playerId-" + data.photonNetworkId + " | playerData.pid: " + data.PID + " | playerData.carID: " + data.carID + " | playerData.characterID: " + data.characterID + "</color>");
                Debug.Log("EVENT_SetPlayerStats: playerId-" + data.photonNetworkId + " | playerData.pid: " + data.PID + " | playerData.carID: " + data.carID + " | playerData.characterID: " + data.characterID);

                if (data.PID == AccountManager.Instance.PID)
                    inGameManager.myPlayerDataIsSent = true;

                foreach (var i in inGameManager.ListOfPlayers)
                {
                    if (i.PID.Equals(data.PID))
                    {
                        i.data = data;
                        i.pm.SetCar(data.carID);
                        i.pm.SetCarData(data.carID, data.carLevel);
                        i.pm.SetCharacter(data.characterID);

                        if (i.pm.IsAI)
                            i.pm.SetAI(data.aiType);
                    }
                }
            }
        }

        NotifyToObservers(ObserverKey.RPC_SetPlayerStats, null);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_MatchSuccess(bool isSuccess)
    {
        Debug.Log("<color=yellow>Photon RPC_MatchSuccess!   isSuccess: " + isSuccess + "</color>");
        if (isSuccess)
            PhotonNetworkManager.matchSuccess = true;
        else
            PhotonNetworkManager.matchSuccess = false;

        PhotonNetworkManager.isMatchSet = true;

        if (PhotonNetworkManager.Instance.MyNetworkRunner != null 
            && PhotonNetworkManager.Instance.MyNetworkRunner.SessionInfo != null)
        {
            PhotonNetworkManager.Instance.SetSessionOpen(false);
        }

        NotifyToObservers(ObserverKey.RPC_MatchSuccess, null);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SceneIsLoaded(long pid)
    {
        Debug.Log("<color=yellow>Photon RPC_SceneIsLoaded!   pid:" + pid + "</color>");

        foreach (var i in InGameManager.Instance.ListOfPlayerSceneLoaded)
        {
            if (pid.Equals(i.pid))
            {
                i.isLoaded = true;
            }
        }

        NotifyToObservers(ObserverKey.RPC_SceneIsLoaded, null);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_IsSpawnReady(NetworkId targetID)
    {
        var list = InGameManager.Instance.ListOfPlayerSpawnReady;

        if (list.Exists(x => x.id.Equals(targetID)) == false)
        {
            list.Add(new InGameManager.PlayerIsReady() { isReady = true, id = targetID });
        }

        NotifyToObservers(ObserverKey.RPC_IsSpawnReady, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_IsGameReady(NetworkId targetID)
    {
        var list = InGameManager.Instance.ListOfPlayerIsGameReady;

        if (list.Exists(x => x.id.Equals(targetID)) == false)
        {
            list.Add(new InGameManager.PlayerIsReady() { isReady = true, id = targetID });
        }

        NotifyToObservers(ObserverKey.RPC_IsGameReady, targetID);
    }


    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SetLaneAndMoveIndex(NetworkId targetID, int lanetype, int moveIndex)
    {
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null)
        {
            p.pm.OnRPCEvent_SetCurrentLaneNumberTypeAndMoveIndex((PlayerMovement.LaneType)lanetype, moveIndex);
        }

        NotifyToObservers(ObserverKey.RPC_SetLaneAndMoveIndex, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetChargePad(int chargePadID, int charPadLv, bool isReset, NetworkId targetID)
    {
        if (inGameManager.mapObjectManager != null)
        {
            if (isReset == false)
                inGameManager.mapObjectManager.OnRPCEvent_ActivateChargePad(targetID, chargePadID, (MapObject_ChargePad.ChargePadLevel)charPadLv);
            else
                inGameManager.mapObjectManager.OnRPCEvent_ResetChargePad(chargePadID, (MapObject_ChargePad.ChargePadLevel)charPadLv);
        }

        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null)
        {
            if (isReset == false)
                p.pm.OnRPCEvent_ActivateChargePad(targetID, chargePadID, (MapObject_ChargePad.ChargePadLevel)charPadLv);
        }

        NotifyToObservers(ObserverKey.RPC_SetChargePad, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SpawnPlayerToPosition(NetworkId targetID, int lane, int moveIndex)
    {
        //특정 지역으로 즉각 이동
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null)
        {
            p.pm.OnRPCEvent_SpawnPlayerToPosition(lane, moveIndex);

            if (p.pm.IsMineAndNotAI)
                CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default);
        }

        NotifyToObservers(ObserverKey.RPC_SpawnPlayerToPosition, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ActivateContainerBox(NetworkId targetID, int containerID)
    {
        if (inGameManager.mapObjectManager != null)
        {
            inGameManager.mapObjectManager.OnRPCEvent_ActivateContainerBox(containerID);
        }

        NotifyToObservers(ObserverKey.RPC_ActivateContainerBox, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ActivateCatapult(NetworkId targetID, int catapultID)
    {
        if (inGameManager.mapObjectManager != null)
        {
            inGameManager.mapObjectManager.OnRPCEvent_ActivateCatapult(catapultID);
        }

        NotifyToObservers(ObserverKey.RPC_ActivateCatapult, targetID);
    }


    public enum SetBatteryType { Normal, CarBooster, ChargePadBooster, ChargeZone }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetPlayerBattery(NetworkId targetID, bool isAdded, float battery, int type = 0)
    {
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null)
        {
            //isAdded일 경우 추가 해주고 아닐 경우 바태리 값 세팅 해주자...!
            if (isAdded == false)
                p.pm.OnRPCEvent_SetCurrentBattery(battery);
            else
            {
                p.pm.OnRPCEvent_SetCurrentBatteryAdded(battery);

                if (p.pm.IsMineAndNotAI && (SetBatteryType)type == SetBatteryType.CarBooster)
                {
                    var ui = PrefabManager.Instance.UI_PanelIngame;
                    ui.ActivateMovingProgressBar();
                }
            }
        }

        NotifyToObservers(ObserverKey.RPC_SetPlayerBattery, targetID);
    }

    /*
     * Photon -> Pnix Server 에서 처리
     * 
    public void StartCountDown(float timer = 0f)
    {
        UtilityCoroutine.StartCoroutine(ref _Coroutine_StartCountDown, Coroutine_StartCountDown(timer), this);
    }

    public IEnumerator _Coroutine_StartCountDown = null;
    public IEnumerator Coroutine_StartCountDown(float timer = 0f)
    {
        var endTime = Time.realtimeSinceStartup + timer;

        while (true)
        {
            if (Time.realtimeSinceStartup > endTime)
                break;

            yield return null;
        }

        RPC_StartCountDown();
    }

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
            ingamePanel.ActivateCountDownTxt(UI_PanelIngame.CountDownType.Start, InGameManager.Instance.countDownEndTime_StartRace);
        }

        //camChangeTime 이후에 카메라 바꿔주자...

        var camChangeTime_1 = DataManager.START_COUNTDOWN_TIME - 6f;
        if (camChangeTime_1 > 0)
            this.Invoke(() => 
            {
                foreach (var i in InGameManager.Instance.ListOfPlayers)
                {
                    if (i != null && i.pm != null)
                        i.pm.ActivateIntroMovement();
                }

                if (inGameManager.gameState == InGameManager.GameState.StartCountDown)
                    CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_LookAtPlayerIntro); 
            }, camChangeTime_1);

        var camChangeTime_2 = DataManager.START_COUNTDOWN_TIME - 3f;
        if (camChangeTime_2 > 0)
            this.Invoke(() => 
            {
                if (inGameManager.gameState == InGameManager.GameState.StartCountDown)
                {
                    foreach (var i in InGameManager.Instance.ListOfPlayers)
                    {
                        if (i!= null && i.pm != null)
                            i.pm.MoveToStartPosiiton();
                    }

                    CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default);
                }
            }, camChangeTime_2);

        if (inGameManager.myPlayer != null)
            RPC_SetPlayerBattery(inGameManager.myPlayer.networkPlayerID, true, inGameManager.myPlayer.PLAYER_BATTERY_START_AMOUNT);

        NotifyToObservers(ObserverKey.RPC_StartCountDown, null);
    }

    public void PlayGame(float timer = 0f)
    {
        UtilityCoroutine.StartCoroutine(ref _Coroutine_PlayGame, Coroutine_PlayGame(timer), this);
    }

    private IEnumerator _Coroutine_PlayGame = null;
    private IEnumerator Coroutine_PlayGame(float timer = 0f)
    {
        var endTime = Time.realtimeSinceStartup + timer;

        while (true)
        {
            if (timer <= 0f)
                break;

            if (Time.realtimeSinceStartup > endTime)
                break;

            yield return null;
        }

        if (inGameManager.isPlayGame == false)
            RPC_PlayGame();
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    private void RPC_PlayGame()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return;

        if (inGameManager.isPlayGame)
            return;

        Debug.Log("<color=yellow>Photon OnEvent! EVENT_PlayGame</color>");

        inGameManager.isPlayGame = true;

        CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default);

        foreach (var i in InGameManager.Instance.ListOfPlayers)
        {
            //나
            if (i.pm != null && i.pm.IsMineAndNotAI)
            {
                i.pm.StartMoving();
            }

            //내 AI
            if (i.pm != null && i.pm.IsMineAndAI)
            {
                i.pm.StartMoving();
            }
        }


        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
        ingamePanel.ActivateIngamePlayTxt();
        ingamePanel.ActivateGoTxt();
        ingamePanel.ActivateParryingCooltimeGO();
        ingamePanel.ActivateNickname();

        UtilityCoroutine.StartCoroutine(ref inGameManager.updateRankList, inGameManager.UpdateRankList(), this);
        UtilityCoroutine.StartCoroutine(ref inGameManager.updatePlayerBehindList, inGameManager.UpdatePlayerBehindList(), this);
        UtilityCoroutine.StartCoroutine(ref inGameManager.updateMapObjectList, inGameManager.UpdateMapObjectList(), this);
        UtilityCoroutine.StartCoroutine(ref inGameManager.updateMiniMap, inGameManager.UpdateMiniMap(), this);

        NotifyToObservers(ObserverKey.RPC_PlayGame, null);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_EndCountDown()
    {
        if (inGameManager.isEndCountStarted)
            return;

        Debug.Log("<color=yellow>Photon OnEvent! EVENT_EndCountDown</color>");

        StartCoroutine(inGameManager.ChangeGameState(InGameManager.GameState.EndCountDown));

        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
        if (ingamePanel.gameObject.activeSelf == true)
        {
            ingamePanel.ActivateCountDownTxt(UI_PanelIngame.CountDownType.End, InGameManager.Instance.countDownEndTime_EndRace);
        }

        NotifyToObservers(ObserverKey.RPC_EndCountDown, null);
    }

    public void EndGame(float timer = 0f)
    {
        UtilityCoroutine.StartCoroutine(ref _Coroutine_EndGame, Coroutine_EndGame(timer), this);
    }

    private IEnumerator _Coroutine_EndGame = null;
    private IEnumerator Coroutine_EndGame(float timer)
    {
        var endTime = Time.realtimeSinceStartup + timer;

        while (true)
        {
            if (Time.realtimeSinceStartup > endTime)
                break;

            yield return null;
        }

        if (inGameManager.isGameEnded == false)
            RPC_EndGame();
    }
    
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


        var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
        ingamePanel.ActivateStandingInfo(); //retire된 사람들을 위해 한번 더 켜주자...!

        StartCoroutine(inGameManager.ChangeGameState(InGameManager.GameState.EndGame));

        NotifyToObservers(ObserverKey.RPC_EndGame, null);
    }
     */

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_ChangeLane_Left(NetworkId targetID)
    {
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.isFlipped)
                return;

            if (pm.isOutOfBoundary)
                return;


            pm.OnRPCEvent_SetIngameInput(PlayerMovement.MoveInput.Left, pm.transform.position, pm.client_currentMoveIndex, pm.client_currentLaneType, pm.network_currentBattery, PlayerMovement.CarBoosterType.None);

            if (pm.IsMineAndNotAI)
                inGameManager.ActivateInputDelay();
        }

        NotifyToObservers(ObserverKey.RPC_ChangeLane_Left, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_ChangeLane_Right(NetworkId targetID)
    {
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.isFlipped)
                return;

            if (pm.isOutOfBoundary)
                return;


            pm.OnRPCEvent_SetIngameInput(PlayerMovement.MoveInput.Right, pm.transform.position, pm.client_currentMoveIndex, pm.client_currentLaneType, pm.client_currentMoveIndex, PlayerMovement.CarBoosterType.None);

            if (pm.IsMineAndNotAI)
                inGameManager.ActivateInputDelay();
        }

        NotifyToObservers(ObserverKey.RPC_ChangeLane_Right, targetID);
    }


    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_BoostPlayer(NetworkId targetID, int boosterLv,  int timingBoosterSuccessType = 0)
    {
        if (inGameManager.BlockInput())
            return;

        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));
        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.network_isEnteredTheFinishLine)
                return;

            pm.OnRPCEvent_SetIngameInput(PlayerMovement.MoveInput.Boost, pm.transform.position, pm.client_currentMoveIndex, pm.client_currentLaneType, pm.client_currentMoveIndex, (PlayerMovement.CarBoosterType)boosterLv, (TimingBoosterSuccessType)timingBoosterSuccessType);

            if (pm.IsMineAndNotAI)
            {
                inGameManager.ActivateInputDelay();
            }
        }

        NotifyToObservers(ObserverKey.RPC_BoostPlayer, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_Shield(NetworkId targetID)
    {
        if (inGameManager.BlockInput())
            return;

        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            if (pm.network_isEnteredTheFinishLine)
                return;

            if (pm.isFlipped)
                return;

            pm.OnRPCEvent_SetIngameInput(PlayerMovement.MoveInput.Shield, pm.transform.position, pm.client_currentMoveIndex, pm.client_currentLaneType, pm.client_currentMoveIndex, PlayerMovement.CarBoosterType.None);

            if (pm.IsMineAndNotAI)
                inGameManager.ActivateInputDelay();
        }

        NotifyToObservers(ObserverKey.RPC_ShieldStart, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_StopShield(NetworkId targetID)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;

            pm.StopShield();
        }

        NotifyToObservers(ObserverKey.RPC_ShieldStop, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendPingInfo()
    {

    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayerPassedFinishLine(NetworkId targetID, float finishTime)
    {
        Debug.Log("<color=yellow>Photon OnEvent! RPC_PlayerPassedFinishLine playerID: " + targetID + "</color>");

        var myPlayer = inGameManager.myPlayer;
        var ListOfPlayers = inGameManager.ListOfPlayers;

        if (myPlayer == null)
            return;

        //내꺼인 경우...
        if (targetID.Equals(myPlayer.networkPlayerID) == true)
        {
            var ingameUI = PrefabManager.Instance.UI_PanelIngame;
            if (ingameUI.gameObject.activeSelf == true)
            {
                ingameUI.ActivatePassedFinishLineTxt();
                ingameUI.DeactiveHUDBase();
            }

            SoundManager.Instance.StopSound(SoundManager.SoundClip.Ingame_BGM_01, SoundManager.StopSoundType.Immediate);
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.PassFinishLine);
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Victory, SoundManager.PlaySoundType.Delay, 1.5f);

            CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_FollowPlayerFront);
        }

        InGameManager.PlayerInfo player = null;
        foreach (var i in ListOfPlayers)
        {
            if (i == null)
                continue;

            if (i.photonNetworkID.Equals(targetID))
            {
                player = i;
            }
        }

        if (player != null)
        {
            player.isEnterFinishLine_Network = true;
            player.enterFinishLineTime_Network = finishTime;

            if (player.pm != null)
            {
                player.pm.network_isEnteredTheFinishLine = true;
                player.pm.DeactivateAllFX();
                player.pm.ActivateAutoMovement();
            }
        }

        var result = ListOfPlayers.OrderByDescending(x => x.isEnterFinishLine_Network).ThenBy(x => x.enterFinishLineTime_Network);
        ListOfPlayers = result.ToList();

        NotifyToObservers(ObserverKey.RPC_PlayerPassedFinishLine, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayerCompletedLap(NetworkId targetID, int lapNumber) //next lap 기준으로 들어옴!
    {
        var myPlayer = inGameManager.myPlayer;
        var ListOfPlayers = inGameManager.ListOfPlayers;

        //내 플레이어의 경우...!
        if (myPlayer != null && myPlayer.networkPlayerID == targetID)
        {
            var ingameUI = PrefabManager.Instance.UI_PanelIngame;
            string msg = "";

            if (inGameManager.IsFinishLap(lapNumber) == false && lapNumber >= 0)
            {
                if (inGameManager.IsLastLap(lapNumber) == false)
                {
                    if (DataManager.FinalLapCount != -1)
                        msg = "LAP " + (lapNumber + 1).ToString() + "/" + DataManager.FinalLapCount.ToString();
                    else
                        msg = "LAP " + (lapNumber + 1).ToString() + "/-";

                    SoundManager.Instance.PlaySound(SoundManager.SoundClip.LapCheckPoint);
                }
                else
                    msg = "FINAL LAP!";

                ingameUI.ActivateTextBase(msg);
            }
        }

        NotifyToObservers(ObserverKey.RPC_PlayerCompletedLap, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayerIsInFrontFalse(NetworkId targetID, NetworkId otherNetworkID)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnEvent_PlayerIsInFrontTrue(otherNetworkID);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_PlayerIsInFrontFalse(NetworkId targetID)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_PlayerIsInFrontFalse();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_TriggerEnterOtherPlayer(NetworkId targetID, NetworkId sourceID, PlayerTriggerChecker.CheckParts parts)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_TriggerEnterOtherPlayer(sourceID, (int)parts);
        }

        NotifyToObservers(ObserverKey.RPC_TriggerEnterOtherPlayer, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_GetStuned(NetworkId targetID, float stunTime, bool useSpinAnim = true)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_GetStuned(stunTime, useSpinAnim);
        }

        NotifyToObservers(ObserverKey.RPC_GetStuned, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_GetBatteryBuff(NetworkId targetID, float battery)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_GetBatteryBuff(battery);
        }

        NotifyToObservers(ObserverKey.RPC_GetBatteryBuff, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_GetFlipped(NetworkId targetID, Vector3 pos, Quaternion rot)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_GetFlipped(pos, rot);
        }

        NotifyToObservers(ObserverKey.RPC_GetFlipped, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_SetOutOfBoundary(NetworkId targetID)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_SetOutOfBoundary();
        }

        NotifyToObservers(ObserverKey.RPC_SetOutOfBoundary, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_StartDrafting(NetworkId targetID)
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPCEvent_StartDraft();
        }

        NotifyToObservers(ObserverKey.RPC_StartDrafting, targetID);
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_LightUpHeadlightsAndHonkHorn(NetworkId targetID, bool isOn) //자동차 상향등 + 경적
    {
        var ListOfPlayers = inGameManager.ListOfPlayers;
        var p = inGameManager.ListOfPlayers.Find(x => x.photonNetworkID.Equals(targetID));

        if (p != null && p.pm != null)
        {
            var pm = p.pm;
            pm.OnRPC_LightUpHeadlightsAndHonkHorn(isOn);
        }

        NotifyToObservers(ObserverKey.RPC_LightUpHeadlightsAndHonkHorn, targetID);
    }
}