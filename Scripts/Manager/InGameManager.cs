using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using System;
using System.Runtime.InteropServices;
using DTR.Shared;
using UnityEngine.TextCore.Text;

public partial class InGameManager : MonoSingletonNetworkBehaviour<InGameManager>, IEventBus<InGameManager.GameState>
{
    [Header("[InGame Base]")]
    [ReadOnly] public PlayerMovement myPlayer = null;
    [ReadOnly] public WayPointSystem.WaypointsGroup wayPoints = null;
    [ReadOnly] public MapObjectManager mapObjectManager = null;
    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkRunner myNetworkRunner => photonNetworkManager.MyNetworkRunner;
    private NetworkInGameRPCManager networkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    public IEnumerator gameRoutine = null;
    public IEnumerator updateNetworkPlayerList = null;
    public IEnumerator updateRankList = null;
    public IEnumerator updatePlayerBehindList = null;
    public IEnumerator updateMapObjectList = null;
    public IEnumerator updatePingInfo = null;

    public enum GameState { Initialize, IsGameReady, StartCountDown, PlayGame, EndCountDown, EndGame }
    [ReadOnly] public GameState gameState;

    public enum GameMode { Solo, Team }
    [ReadOnly] public GameMode gameMode = GameMode.Solo;

    [System.Serializable]
    public class PlayerInfo
    {
        public GameObject go;
        public PlayerMovement pm;
        public PlayerData data = new PlayerData();
        public CRacerInfo racerInfo { get { if (Instance.allRacerInfo != null && Instance.allRacerInfo.Count > 0) return Instance.allRacerInfo.Find(x => x.ActorID.Equals(this.PID)); else return null; } }
        public long PID { get{ if (pm != null) return pm.network_PID; else return 0; } }
        public NetworkId photonNetworkID { get { if (pm != null && pm.networkPlayerID != null) return pm.networkPlayerID; else return new NetworkId(); } }
        public float distLeft { get { if (pm != null) return pm.GetMinDistLeft(); else return -1f; } }
        public int currentLap { get { if (pm != null) return pm.network_currentLap; else return -1; } }

        public int currentRank { get { if (pm != null) return pm.currentRank; else return -1; } }
        public bool isAI { get { if (pm != null) return pm.IsAI; else return false; } }

        public bool isEnterFinishLine_Local { get { if (pm != null) return pm.network_isEnteredTheFinishLine; else return false; } }

        public bool isEnterFinishLine_Network = false; //EVENT_PassedFinishLine에 의해 설정되는 값... 위에 pm이 null되는 경우때문에 network 변수로 관리

        public float enterFinishLineTime_Network = 0; //EVENT_PassedFinishLine에 의해 설정되는 값

        public bool isRetire { get { if (isEnterFinishLine_Network == false && InGameManager.Instance.isGameEnded) return true; else return false; }  }

        public float distanceBetweenMyPlayer { get { if (pm != null) return pm.GetDistBetweenMyPlayer(); else return 0f; } }
        public bool isSetToPosition { get { if (pm != null) return pm.isSetToPosition; else return false; } }

    }

    //플레이어가 나가면 ListOfPlayers count는 유지됨!! go & pm 은 null이 됨
    [ReadOnly] public List<PlayerInfo> ListOfPlayers = new List<PlayerInfo>();
    public List<PlayerInfo> ListOfAIPlayers { get { return ListOfPlayers.FindAll(x => x != null && x.pm != null && x.isAI); } }

    public List<CRacerInfo> allRacerInfo = new List<CRacerInfo>();
    public List<CRacerInfo> playerAckData = new List<CRacerInfo>();
    public List<CRacerInfo> aiAckData = new List<CRacerInfo>();
    public CRacerInfo myRacerInfo { get { if (allRacerInfo != null) return allRacerInfo.Find(x => x.ActorID.Equals(AccountManager.Instance.PID)); else return null; } }
    
    public int totalPlayerCount { get { if (playerAckData != null && aiAckData != null) return playerAckData.Count + aiAckData.Count; else return 9999; } }

    [ReadOnly] public List<PlayerInfo> ListOfPlayerBehind = new List<PlayerInfo>(); //뒤쪽에 있는 player list...

    #region JSON용 Class
    [System.Serializable] 
    public class PlayerDataList 
    {
        [SerializeField] public List<PlayerData> listOfPlayerData;

        public PlayerDataList()
        {
            listOfPlayerData = new List<PlayerData>();
        }
    }
    #endregion

    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] public long PID;
        [SerializeField] public string ownerNickName;
        [SerializeField] public NetworkId photonNetworkId;
        [SerializeField] public int carID;
        [SerializeField] public int carLevel;
        [SerializeField] public int characterID;
        [SerializeField] public int aiType;

        //추후에... 추가 정보 넣자...!
        //TODO....
    }

    [ReadOnly] public bool isPlayGame = false;
    [ReadOnly] public bool isStartCountDown = false;

    [ReadOnly] public bool isAllPlayerSetInPosition = false;
    [ReadOnly] public bool myPlayerDataIsSent = false;

    public double totalTimeGameElapsed
    {
        get
        {
            if (myPlayer != null)
                return (myPlayer.Runner.Tick - startRaceTick) * myPlayer.Runner.DeltaTime;
            else
                return 0f;
        }
    }

    [ReadOnly] public bool isEndCountStarted = false;
    [ReadOnly] public bool isGameEnded = false;
    [ReadOnly] public bool isInputDelay = false;

    public bool myPlayerPassedFinishLine
    {
        get
        {
            if (myPlayer != null&& myPlayer.isSpawned)
                return myPlayer.network_isEnteredTheFinishLine;

            return false;
        }
    }

    public int myCurrentRank
    {
        get
        {
            if (myPlayer != null)
                return myPlayer.currentRank;

            return -1;
        }
    }
    public PlayerInfo myPlayerInfo
    {
        get
        {
            if (ListOfPlayers != null && ListOfPlayers.Count > 0 && myPlayer != null)
            {
                var p = ListOfPlayers.Find(x => x.photonNetworkID.Equals(myPlayer.networkPlayerID));
                return p;
            }

            return null;
        }
    }

    public NetworkId myPlayerPlayerID
    {
        get
        {
            if (myPlayer != null)
                return myPlayer.networkPlayerID;

            return new NetworkId();
        }
    }

    public PlayerInfo GetPlayerInfo(Fusion.NetworkId viewID)
    {
        PlayerInfo info = null;

        if (ListOfPlayers != null && ListOfPlayers.Count > 0)
        {
            info = ListOfPlayers.Find(x => x.pm != null && x.pm.networkPlayerID.Equals(viewID));
        }

        return info;
    }

    public PlayerData myPlayerData
    {
        get
        {
            if (myPlayerInfo != null)
            {
                return myPlayerInfo.data;
            }

            return null;
        }
    }
    public int startRaceTick { get; set; } = 0;

    public int endRaceTick { get; set; } = 0;

    public DateTime countDownEndTime_StartRace = DateTime.Now;
    public DateTime countDownEndTime_EndRace = DateTime.Now;

    #region Event Bus
    static Dictionary<GameState, List<Action>> events => IEventBus<GameState>.events;
    public void SubscribeEvent(GameState key, System.Action ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    public void UnSubscribeEvent(GameState key, System.Action ac)
    {
        if (events.ContainsKey(key) == true)
        {
            var l = events[key];
            if (l != null)
            {
                foreach (var i in l)
                {
                    if (i.Equals(ac))
                    {
                        l.Remove(i);
                        break;
                    }
                }
            }
        }
    }

    public void UnSubscribeAllEvent(GameState key)
    {
        if (events.ContainsKey(key) == true)
        {
            events.Remove(key);
        }
    }

    public void ExcecuteEvent(GameState key)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke();
        }
    }
    #endregion

    [Serializable]
    public class PlayerIsSceneLoaded
    {
        public bool isLoaded = false;
        public long pid = -1; //pid
    }
    [SerializeField] public List<PlayerIsSceneLoaded> ListOfPlayerSceneLoaded = new List<PlayerIsSceneLoaded>();

    [Serializable]
    public class PlayerIsReady
    {
        public bool isReady = false;
        public NetworkId id; //id
    }
    [SerializeField] public List<PlayerIsReady> ListOfPlayerSpawnReady = new List<PlayerIsReady>();
    [SerializeField] public List<PlayerIsReady> ListOfPlayerIsGameReady = new List<PlayerIsReady>();


    private void Update()
    {
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameState != GameState.PlayGame)
                    return;

                string msg = "Ingame_EndMatch".Localize();

                UIManager_NGUI.Instance.ActivatePanelDefault_YesNo(msg, () =>
                {
                    if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
                        return;

                    if (gameState != GameState.PlayGame)
                        return;

                    if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
                        PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
                });

            }
        }

        //temp for test...!
#if UNITY_EDITOR && CHEAT
        if (myPlayer != null && myPlayer.IsMineAndNotAI)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                if (myPlayer != null && !myPlayer.network_isEnteredTheFinishLine && !myPlayer.IsStopInputIfSheild())
                    networkInGameRPCManager.RPC_ChangeLane_Left(myPlayer.networkPlayerID);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                if (myPlayer != null && !myPlayer.network_isEnteredTheFinishLine && !myPlayer.IsStopInputIfSheild())
                    networkInGameRPCManager.RPC_ChangeLane_Right(myPlayer.networkPlayerID);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                UI_IngameControl_TouchSwipe.OnDrag = true;

                if (myPlayer != null && !myPlayer.network_isEnteredTheFinishLine)
                {
                    if (myPlayer.isShield|| myPlayer.isShieldCooltime)
                        return;

                    networkInGameRPCManager.RPC_Shield(myPlayer.networkPlayerID);
                }
            }

            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
            {
                UI_IngameControl_TouchSwipe.OnDrag = false;
                /*
                if (myPlayer.isDecelerating == true)
                    networkInGameRPCManager.RPC_Deceleration_End(myPlayer.networkPlayerID);
                */
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                switch (gameState)
                {
                    case GameState.StartCountDown:
                        {
                            myPlayer.ActivateStartingBooster();
                        }
                        break;
                    case GameState.PlayGame:
                    case GameState.EndCountDown:
                        {
                            PlayerMovement.CarBoosterType boosterLv = myPlayer.GetAvailableInputBooster();
                            if (boosterLv != PlayerMovement.CarBoosterType.None)
                            {
                                if (myPlayer != null && !myPlayer.network_isEnteredTheFinishLine && !myPlayer.isShield && myPlayer.isGrounded && !myPlayer.isOutOfBoundary && !myPlayer.isStunned)
                                    networkInGameRPCManager.RPC_BoostPlayer(myPlayer.networkPlayerID, (int)boosterLv, (int)myPlayer.currentTimingBoosterSuccessType);
                            }
                            else // boosterLv == PlayerMovement.CarBoosterLevel.None
                            {
                                if (myPlayer != null)
                                {
                                    if (myPlayer.IsTimingBoosterReady)
                                        myPlayer.ResetTimingBooster();
                                }
                            }
                        }
                        break;
                }
            }
        }
#endif
    }

    public void StartGame()
    {
        UtilityCoroutine.StopCoroutine(ref gameRoutine, this);
        UtilityCoroutine.StartCoroutine(ref gameRoutine, GameRoutine(), this);
    }

    public void InitializeSettings()
    {
        myPlayer = null;
        wayPoints = null;
        mapObjectManager = null;

        isAllPlayerSetInPosition = false;
        isStartCountDown = false;
        isPlayGame = false;
        isEndCountStarted = false;
        isGameEnded = false;
        isInputDelay = false;
        myPlayerDataIsSent = false;

        ListOfPlayers.Clear();
        ListOfAIPlayers.Clear();
        ListOfPlayerBehind.Clear();

        startRaceTick = 0;
        endRaceTick = 0;

        countDownEndTime_StartRace = DateTime.UtcNow;
        countDownEndTime_EndRace = DateTime.UtcNow;

        DataManager.Instance.SetFinalLapCount();

        InitializeMiniMap();

        PhotonNetworkManager.Instance.ClearPingInfoList();
        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerJoined = PhotonCallback_OnPlayerJoined;
        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerLeft = PhotonCallback_OnPlayerLeft;

        UtilityCoroutine.StopCoroutine(ref updateMapObjectList, this);
        UtilityCoroutine.StopCoroutine(ref updateNetworkPlayerList, this);
        UtilityCoroutine.StopCoroutine(ref updateRankList, this);
        UtilityCoroutine.StopCoroutine(ref updatePlayerBehindList, this);
        UtilityCoroutine.StopCoroutine(ref updatePingInfo, this);

        UnSubscribeAllEvent(GameState.Initialize);
        UnSubscribeAllEvent(GameState.IsGameReady);
        UnSubscribeAllEvent(GameState.StartCountDown);
        UnSubscribeAllEvent(GameState.PlayGame);
        UnSubscribeAllEvent(GameState.EndCountDown);
        UnSubscribeAllEvent(GameState.EndGame);

        if (events != null)
            events.Clear();

#if UNITY_EDITOR
        SetPinkMaterial_EditorOnly();
#endif
    }

    public void InitializeInGameUI()
    {
        PrefabManager.Instance.UI_PanelIngame.Initialize();
    }

    private IEnumerator GameRoutine()
    {
        yield return StartCoroutine(ChangeGameState(GameState.Initialize));
        yield return StartCoroutine(WaitForAllPlayersLoadScene());

        SetMap();
        SpawnPlayer();
        SpawnAI();
        SetUI();
        PoolGameObjects();
        SetCam();
        SetMiniMap();

        yield return null;

        yield return StartCoroutine(WaitForAllPlayersToSpawnPlayers());

        yield return StartCoroutine(SendPlayerData());

        yield return StartCoroutine(ChangeGameState(GameState.IsGameReady));

        yield return StartCoroutine(WaitForAllPlayerToBeReady());

        yield return StartCoroutine(SetPlayerToStartPosition());

        PnixNetworkManager.Instance.SendReadyToStartRace();

        yield return StartCoroutine(WaitForPlayGame());

        yield return StartCoroutine(WaitForGameOver());

        yield break;
    }


    public void SetMap()
    {
        wayPoints = FindObjectOfType<WayPointSystem.WaypointsGroup>();
        mapObjectManager = FindObjectOfType<MapObjectManager>();
        if (mapObjectManager != null)
            mapObjectManager.Initialize();
    }

    public void SpawnPlayer()
    {
        var prefab = PrefabManager.Instance.NetworkPlayer;
        var runner = PhotonNetworkManager.Instance.MyNetworkRunner.Spawn(prefab, Vector3.zero, Quaternion.identity, PhotonNetworkManager.Instance.MyPlayerRef);
        if (runner != null)
        {
            runner.GetComponent<PlayerMovement>().network_PID = AccountManager.Instance.PID;
            myPlayer = runner.GetComponent<PlayerMovement>();
        }
    }

    public void SpawnAI()
    {
        if (PhotonNetworkManager.Instance.IsRoomMasterClient == false)
            return;

        int playerCount = PhotonNetworkManager.Instance.listOfNetworkPlayerInfos.Count;
        int aiCount = InGameManager.Instance.aiAckData.Count;

        if (aiCount > 0 && PhotonNetworkManager.Instance.IsRoomMasterClient)
        {
            for (int i = 0; i < aiCount; i++)
            {
                var prefab = PrefabManager.Instance.NetworkPlayer_AI;
                var runner = PhotonNetworkManager.Instance.MyNetworkRunner.Spawn(prefab, Vector3.zero, Quaternion.identity, PhotonNetworkManager.Instance.MyPlayerRef);
                if (runner != null)
                {
                    runner.GetComponent<PlayerMovement>().network_PID = aiAckData[i].ActorID;
                }
            }
        }
    }

    public void SetPnixAckRaceInfoData(List<CRacerInfo> list) //PnixNetwork ack Data
    {
        //ack 데이터는 phaseingame 들어오고 initialize 전에 설정하는거라 initialize때 clear때리면 안됨!
        allRacerInfo = list;
        playerAckData = list.FindAll(x => x.IsAI == false);
        aiAckData = list.FindAll(x => x.IsAI == true);
    }

    public void SetListOfPlayerSceneLoaded()
    {
        if (playerAckData == null)
        {
            Debug.Log("Error... playerAckData is null");
            return;
        }

        ListOfPlayerSceneLoaded.Clear();
        ListOfPlayerSceneLoaded = new List<PlayerIsSceneLoaded>();

        var list = playerAckData;
        int realPlayerCount = playerAckData.Count;

        for (int i = 0; i < realPlayerCount; i++)
        {
            ListOfPlayerSceneLoaded.Add(new PlayerIsSceneLoaded() { isLoaded = false, pid = list[i].ActorID });
        }
    }

    public void SetListOfPlayerReady()
    {
        ListOfPlayerSpawnReady.Clear();
        ListOfPlayerSpawnReady = new List<PlayerIsReady>();
        ListOfPlayerIsGameReady.Clear();
        ListOfPlayerIsGameReady = new List<PlayerIsReady>();
    }

    private void SetCam()
    {
        CameraManager.Instance.SetInGameCam();
    }

    private void SetUI()
    {
        InitializeInGameUI();
    }

    IEnumerator WaitForAllPlayersLoadScene()
    {
        //모든 플레이어가 씬을 로딩할때까지 기달려주자...!
        //다 로딩을 한 후 Spwan을 해줘야함

        float timeElapsed = 0f;
        bool isError = false;
        while (true)
        {
            timeElapsed += Time.deltaTime;
            bool isAllPlayerReady = true;
            foreach (var i in ListOfPlayerSceneLoaded)
            {
                if (i.isLoaded == false)
                {
                    isAllPlayerReady = false;
                    break;
                }
            }

            if (isAllPlayerReady == true)
                break;

            if (timeElapsed > 10f && isError == false)
            {
                //게속 기달려도 Ready 하는 사람이 없을때....
                UtilityCommon.ColorLog("Error...! All players Loading Scene Are not ready....", UtilityCommon.DebugColor.Red);

                string msg = "Ingame_MatchError_01".Localize();
                UIManager_NGUI.Instance.ActivatePanelDefault_Confirm(msg, null);

                PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
                timeElapsed = 0f;
                isError = true;
            }

            yield return null;
        }

        yield return null;
    }

    IEnumerator WaitForAllPlayersToSpawnPlayers()
    {
        yield return null;

        while (myPlayer == null)
        {
            yield return null;
        }

        while (true)
        {
            yield return UpdateNetworkPlayerListRoutinte();

            if (ListOfPlayers != null)
            {
                if (totalPlayerCount <= ListOfPlayers.Count)
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        //Set My Player
        networkInGameRPCManager.RPC_IsSpawnReady(myPlayer.networkPlayerID);
        //Set AI
        if (PhotonNetworkManager.Instance.IsRoomMasterClient)
        {
            foreach (var i in ListOfAIPlayers)
            {
                if (i.pm != null && i.pm.IsMineAndAI)
                    networkInGameRPCManager.RPC_IsSpawnReady(i.pm.networkPlayerID);
            }
        }

        //모든 사람이 다 스폰에 성공할때까지 기다리자...!
        while (true)
        {
            yield return null;

            if (ListOfPlayers != null)
            {
                if (totalPlayerCount <= ListOfPlayerSpawnReady.Count)
                    break;
            }

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("PhotonNetworkManager ListOfNetworkRunners Count: " + PhotonNetworkManager.Instance.listOfNetworkPlayerInfos.Count);

        UtilityCoroutine.StartCoroutine(ref updateNetworkPlayerList, UpdateNetworkPlayerListRoutinte(), this);

        yield return new WaitForSeconds(2.0f);
    }

    IEnumerator WaitForAllPlayerToBeReady()
    {
        float timeElapsed = 0f;
        bool isError = false;
        while (true)
        {
            timeElapsed += Time.deltaTime;

            if (ListOfPlayerIsGameReady.Count >= totalPlayerCount)
            {
                //isAllPlayerReady = true;
                break;
            }

            if (timeElapsed > 10f && isError == false)
            {
                //게속 기달려도 Ready 하는 사람이 없을때....
                UtilityCommon.ColorLog("Error...! All players Are not ready....", UtilityCommon.DebugColor.Red);

                string msg = "Ingame_MatchError_02".Localize();
                UIManager_NGUI.Instance.ActivatePanelDefault_Confirm(msg, null);

                PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
                isError = true;
            }

            yield return null;
        }
    }

    public void LeaveRoomCallback()
    {
        PnixNetworkManager.Instance.SendLeaveRaceReq();
    }

    IEnumerator SendPlayerData()
    {
        //스폰에 꼭 다 성공하고 Player Data 보내주자...!

        PlayerDataList playerDataList = new PlayerDataList();

        //내 정보를 남들한테 보내주자  + AI data
        PlayerData data = new PlayerData()
        {
            PID = AccountManager.Instance.PID,
            ownerNickName = myRacerInfo.NickName,
            photonNetworkId = myPlayer.networkPlayerID,
            carID = myRacerInfo.CarID,
            carLevel = myRacerInfo.CarLevel,
            characterID = myRacerInfo.CharacterID,
            aiType = 0,
        };

        playerDataList.listOfPlayerData.Add(data);

        //Set AI
        if (PhotonNetworkManager.Instance.IsRoomMasterClient)
        {
            int index = 0;
            var aiList = ListOfAIPlayers;
            for (int i = 0; i < aiAckData.Count; ++i)
            {
                if (i >= ListOfAIPlayers.Count)
                    break; //이럴 경우가 있나...? 혹시 모르니까...

                int aitype = AccountManager.Instance.TEMP_AI_TYPE;

                PlayerData aiData = new PlayerData()
                {
                    PID = aiAckData[i].ActorID,
                    ownerNickName = aiAckData[i].NickName,
                    photonNetworkId = ListOfAIPlayers[i].photonNetworkID,
                    carID = aiAckData[i].CarID,
                    carLevel = aiAckData[i].CarLevel,
                    characterID = aiAckData[i].CharacterID,
                    aiType = aitype,
                };

                if (ListOfAIPlayers[i].pm != null && ListOfAIPlayers[i].pm.IsMineAndAI)
                    playerDataList.listOfPlayerData.Add(aiData);

                ++index;
            }
        }

        string jsonData = JsonUtility.ToJson(playerDataList);

        networkInGameRPCManager.RPC_SetPlayerStats(jsonData);

        yield return null;

        while (myPlayerDataIsSent == false)
            yield return null;
    }


    IEnumerator SetPlayerToStartPosition()
    {
        if (PhotonNetworkManager.Instance.IsRoomMasterClient)
        {
            var randomSpawnPointList = new List<int>();

            var spawnCount = DataManager.Instance.GetSessionTotalCount();

            for (int i = 0; i < spawnCount; i++)
                randomSpawnPointList.Add(i);

            UtilityCommon.ShuffleList<int>(ref randomSpawnPointList);

            for (int i = 0; i < ListOfPlayers.Count; i++)
            {
                var pm = ListOfPlayers[i].pm;
                if (pm != null)
                {
                    if (i < randomSpawnPointList.Count)
                    {
                        networkInGameRPCManager.RPC_SetPlayerToPosition(pm.networkPlayerID, randomSpawnPointList[i]);
                    }
                    else
                    {
                        networkInGameRPCManager.RPC_SetPlayerToPosition(pm.networkPlayerID, 0);
                    }
                }
            }
        }

        yield return null;


        //모든 플레이어가 Position에 세팅 될때까지 대기...
        float timeLimit = 10;
        float timeCounter = 0;
        while (isAllPlayerSetInPosition == false)
        {
            int counter = 0;
            foreach (var i in ListOfPlayers)
            {
                if (i.isSetToPosition == true)
                    ++counter;

                if (counter.Equals(ListOfPlayers.Count))
                {
                    isAllPlayerSetInPosition = true;
                    break;
                }
            }

            timeCounter += Time.deltaTime;
            if (timeCounter >= timeLimit)
            {
                //10초에도 세팅이 안된다면 그냥 넘겨주자...
                break;
            }
            yield return null;
        }
    }

    IEnumerator WaitForPlayGame()
    {
        yield return null;

        while (true)
        {
            if (isPlayGame)
                break;

            yield return null;
        }
    }

    IEnumerator WaitForGameOver()
    {
        yield return null;

        while (true)
        {
            if (gameState == GameState.EndGame)
                break;

            yield return null;
        }
    }

    public IEnumerator ChangeGameState(GameState state)
    {
        if (gameState != state)
            Debug.Log("<color=cyan>Game State prev: " + gameState + " => current: " + state + "</color>");

        gameState = state;

        switch (gameState)
        {
            case GameState.Initialize:
                {
                    InitializeSettings();

                    ExcecuteEvent(GameState.Initialize);
                }
                break;

            case GameState.IsGameReady:
                {
                    if (myPlayer != null)
                    {
                        networkInGameRPCManager.RPC_IsGameReady(myPlayer.networkPlayerID);

                        //Set AI
                        foreach (var i in ListOfAIPlayers)
                        {
                            if (i.pm != null && i.pm.IsMineAndAI)
                                networkInGameRPCManager.RPC_IsGameReady(i.pm.networkPlayerID);
                        }

                        ExcecuteEvent(GameState.IsGameReady);
                    }
                    else
                    {
                        Debug.Log("Error....! myPlayer is Null???");
                    }
                }
                break;

            case GameState.StartCountDown:
                {
                    if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                        break;

                    if (isStartCountDown)
                        break;

                    isStartCountDown = true;

                    UIManager_NGUI.Instance.DeactivateUI(UIManager_NGUI.UIType.UI_PanelLoading);

                    var ingamePanel = PrefabManager.Instance.UI_PanelIngame;

                    if (ingamePanel.gameObject.activeSelf == true)
                    {
                        ingamePanel.ActivateCountDownTxt(UI_PanelIngame.CountDownType.Start, countDownEndTime_StartRace);
                    }

                    //camChangeTime 이후에 카메라 바꿔주자...

                    var ts = countDownEndTime_StartRace - PnixNetworkManager.Instance.ServerTime;
    
                    var camChangeTime_1 = (float)ts.TotalSeconds - 5f;
                    if (camChangeTime_1 > 0)
                    {
                        this.Invoke(() =>
                        {
                            foreach (var i in ListOfPlayers)
                            {
                                if (i != null && i.pm != null)
                                    i.pm.ActivateIntroMovement();
                            }

                            if (gameState == GameState.StartCountDown)
                                CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_LookAtPlayerIntro);
                        }, camChangeTime_1);
                    }
                    else
                    {
                        Debug.Log("CountDown Time is too short to play playerIntro movement");
                    }

                    var camChangeTime_2 = (float)ts.TotalSeconds - 3f;
                    if (camChangeTime_2 > 0)
                    {
                        this.Invoke(() =>
                        {
                            if (gameState == InGameManager.GameState.StartCountDown)
                            {
                                foreach (var i in InGameManager.Instance.ListOfPlayers)
                                {
                                    if (i != null && i.pm != null)
                                        i.pm.MoveToStartPosiiton();
                                }

                                CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default);
                            }
                        }, camChangeTime_2);
                    }
                    else
                    {
                        Debug.Log("CountDown Time is too short to change cam type to InGame_MainCam_FollowPlayerBack_Default");
                    }

                    if (myPlayer != null)
                        networkInGameRPCManager.RPC_SetPlayerBattery(myPlayer.networkPlayerID, true, myPlayer.ref_batteryStartAmount);

                    ExcecuteEvent(GameState.StartCountDown);
                }
                break;

            case GameState.PlayGame:
                {
                    if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                        break;

                    if (isPlayGame)
                        break;

                    if (InGameManager.Instance.wayPoints == null)
                    {
                        Debug.Log("ERRRRRRoor....! waypoint is null");
                        break;
                    }

                    isPlayGame = true;

                    CameraManager.Instance.ChangeCamType(CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default);

                    foreach (var i in ListOfPlayers)
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

                    UtilityCoroutine.StartCoroutine(ref updateRankList, UpdateRankList(), this);
                    UtilityCoroutine.StartCoroutine(ref updatePlayerBehindList, UpdatePlayerBehindList(), this);
                    UtilityCoroutine.StartCoroutine(ref updateMapObjectList, UpdateMapObjectList(), this);
                    UtilityCoroutine.StartCoroutine(ref updateMiniMap, UpdateMiniMap(), this);

                    ExcecuteEvent(GameState.PlayGame);
                }
                break;

            case GameState.EndCountDown:
                {
                    if (isEndCountStarted == true)
                        break;

                    isEndCountStarted = true;

                    var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
                    if (ingamePanel.gameObject.activeSelf == true)
                    {
                        ingamePanel.ActivateCountDownTxt(UI_PanelIngame.CountDownType.End, countDownEndTime_EndRace);
                    }

                    ExcecuteEvent(GameState.EndCountDown);
                }
                break;

            case GameState.EndGame:
                {
                    if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                        break;

                    if (isGameEnded == true)
                        break;

                    var ingamePanel = PrefabManager.Instance.UI_PanelIngame;
                    ingamePanel.ActivateStandingInfo(); //retire된 사람들을 위해 한번 더 켜주자...!

                    isPlayGame = false;
                    isGameEnded = true;

                    FinalUpdateRankList();
                    SetEndGameResultToUserData();
                    SetEndGameResultToMatchRecordData();

                    foreach (var i in ListOfPlayers)
                    {
                        if (i != null && i.pm != null && i.pm.IsMine
                            && (i.pm.isFlipped || i.pm.isOutOfBoundary))
                            i.pm.MovePlayerToValidPosition();
                    }

                    SoundManager.Instance.PlaySound(SoundManager.SoundClip.Applause);
                    SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.Ingame_BGM_01, SoundManager.PlaySoundType.Immediate);

                    PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameResult);

                    ExcecuteEvent(GameState.EndGame);
                }
                break;

            default:
                break;
        }

        yield return null;
    }

    public void UpdateNetworkPlayerList()
    {
        UtilityCoroutine.StartCoroutine(ref updateNetworkPlayerList, UpdateNetworkPlayerListRoutinte(), this);
    }

    IEnumerator UpdateNetworkPlayerListRoutinte()
    {
        PlayerMovement[] playerClass = GameObject.FindObjectsOfType(typeof(PlayerMovement)) as PlayerMovement[];
      
        foreach (var i in playerClass)
        {
            if (i == null || i.networkPlayerID == null)
                continue;

            if (ListOfPlayers != null && ListOfPlayers.Exists(x => x != null && x.photonNetworkID.Equals(i.networkPlayerID)))
                continue;

            ListOfPlayers.Add(new PlayerInfo() { go = i.gameObject, pm = i, enterFinishLineTime_Network = 0f });
        }

        yield return null;
    }

    public IEnumerator UpdateRankList()
    {
        if (ListOfPlayers != null)
        {
            bool breakFlag = false;

            while (true)
            {
                if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                    break;

                foreach (var i in ListOfPlayers)
                {
                    if (i == null || i.pm == null)
                    {
                        breakFlag = true;
                        break;
                    }
                }

                if (breakFlag == true)
                    break;

                var result = ListOfPlayers.OrderByDescending(x => x.isEnterFinishLine_Network)
                    .ThenByDescending(x => x.currentLap)
                    .ThenByDescending(x=>x.pm.client_currentMoveIndex)
                    .ThenBy(x => x.distLeft);
                ListOfPlayers = result.ToList();

                for (int i = 0; i < ListOfPlayers.Count; i++)
                {
                    ListOfPlayers[i].pm.currentRank = i + 1;
                }

                yield return new WaitForSeconds(0.3f);

                if (myPlayerPassedFinishLine == true)
                    break;

                if (isGameEnded == true)
                    break;
            }
        }
    }

    public IEnumerator UpdatePlayerBehindList()
    {
        while (true)
        {
            var ingameCam = Camera_Base.Instance.mainCam;
            var uiCam = UIRoot_Base.Instance.uiCam;
            var playerList = ListOfPlayers.ToList();
            if (gameState == GameState.PlayGame || gameState == GameState.EndCountDown)
            {
                foreach (var i in playerList)
                {
                    if (myPlayer == null || i.photonNetworkID.Equals(myPlayer.networkPlayerID))
                        continue;

                    if (i.pm == null || i.pm.playerCar == null || i.pm.playerCar.currentCar == null || i.pm.playerCar.currentCar.go == null)
                    {
                        if (ListOfPlayerBehind != null && ListOfPlayerBehind.Contains(i))
                            ListOfPlayerBehind.Remove(i);
                        continue;
                    }

                    var renderer = i.pm.playerCar.currentCar.bodyMesh;

                    if (renderer != null)
                    {
                        if (CameraManager.Instance.currentCamType == CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default
                            && myPlayer.network_isMoving && !myPlayer.isFlipped && !myPlayer.isOutOfBoundary && !myPlayer.network_isEnteredTheFinishLine)
                        {
                            if (UtilityCommon.IsObjectVisible(ingameCam, renderer))
                            {
                                //보이는 중...!
                                if (ListOfPlayerBehind.Contains(i))
                                    ListOfPlayerBehind.Remove(i);
                            }
                            else
                            {
                                //안 보이는 중...!
                                if (Vector3.Distance(myPlayer.transform.position, i.pm.transform.position) < DataManager.PLAYER_INDICATOR_CHECK_BEHIND_DIST
                                    && i.pm.isFlipped == false && i.pm.isOutOfBoundary == false && i.pm.network_isEnteredTheFinishLine == false)
                                {
                                    if (ListOfPlayerBehind.Contains(i) == false)
                                        ListOfPlayerBehind.Add(i);
                                }
                                else
                                {
                                    if (ListOfPlayerBehind.Contains(i))
                                        ListOfPlayerBehind.Remove(i);
                                }
                            }
                        }
                        else
                        {
                            ListOfPlayerBehind.Clear();
                        }
                    }
                }

                var ui = PrefabManager.Instance.UI_PanelIngame;
                if (ListOfPlayerBehind != null && ListOfPlayerBehind.Count > 0
                    && CameraManager.Instance.currentCamType == CameraManager.CamType.InGame_MainCam_FollowPlayerBack_Default)
                {
                    ui.ActivateWarningGO();

                    if (uiCam != null && ui.warning_Go != null)
                    {
                        ListOfPlayerBehind.Sort((x, y) => x.distanceBetweenMyPlayer.CompareTo(y.distanceBetweenMyPlayer));
                    }
                }
                else
                {
                    ui.DeactivateWarningGO();
                }
            }

            if (isGameEnded == true)
                break;

            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                break;

            yield return new WaitForSeconds(0.2f);
        }
    }


    private void SetEndGameResultToUserData()
    {
        DataManager.Instance.SaveUserData();
    }

    private void SetEndGameResultToMatchRecordData()
    {
        if (PhotonNetworkManager.Instance.IsRoomMasterClient == false)
            return;

        string matchName = System.DateTime.UtcNow.ToString("yyyy-MM-dd  H:m:s", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "__" + SystemInfo.deviceUniqueIdentifier;
    }


    private void FinalUpdateRankList()
    {
        var result = ListOfPlayers.OrderByDescending(x => x.isEnterFinishLine_Network).ThenBy(x => x.enterFinishLineTime_Network);
        ListOfPlayers = result.ToList();

        for (int i = 0; i < ListOfPlayers.Count; i++)
        {
            ListOfPlayers[i].pm.currentRank = i + 1;
        }
    }


    public IEnumerator UpdateMapObjectList()
    {
        //IsMasterClient 아닌 겨우 -> 물려 받는 경우를 위해.... 무한 루프로 체크
        while (true)
        {
            if (gameState == GameState.EndGame || PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                break;

            if (PhotonNetworkManager.Instance.IsRoomMasterClient)
            {
                if (mapObjectManager != null)
                {
                    mapObjectManager.ContainerLoop();
                    mapObjectManager.CatapultLoop();
                }
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        yield break;
    }




    private IEnumerator UpdatePingInfo()
    {
        yield break; //TODO...

        float PING_INFO_SEND_RATE = 0.5f;
        while (true)
        {
            if (networkInGameRPCManager == null)
                break;

            yield return new WaitForSeconds(PING_INFO_SEND_RATE);
            networkInGameRPCManager.RPC_SendPingInfo();


            if (gameState == GameState.EndGame)
                break;

            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                break;
        }

        PhotonNetworkManager.Instance.ClearPingInfoList();
    }

    public bool BlockInput()
    {
        bool block = false;

        if (gameState == GameState.Initialize || PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            block = true;

        if (isGameEnded)
            block = true;

        return block;
    }

    public void ActivateInputDelay()
    {
        UtilityCoroutine.StopCoroutine(ref setInputDelay, this);
        UtilityCoroutine.StartCoroutine(ref setInputDelay, SetInputDelay(), this);
    }

    private IEnumerator setInputDelay;
    private IEnumerator SetInputDelay()
    {
        isInputDelay = true;

        float timer = DataManager.GAME_INPUT_DELAY;

        while (true)
        {
            timer -= Time.fixedDeltaTime;

            if (timer < 0)
                break;

            yield return null;
        }

        isInputDelay = false;
    }

    public bool IsLastLap(int lap) //마지막 Lap
    {
        bool isLastLap = false;

        if (DataManager.FinalLapCount == -1)
            return false;

        if (lap == DataManager.FinalLapCount - 1)
            return true;

        return isLastLap;
    }

    public bool IsFinishLap(int lap) //최종 Lap
    {
        bool isFinalLap = false;

        if (DataManager.FinalLapCount == -1)
            return false;

        if (lap >= DataManager.FinalLapCount)
            return true;

        return isFinalLap;
    }


    private void PhotonCallback_OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        UpdateNetworkPlayerList();
    }

    private void PhotonCallback_OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        UpdateNetworkPlayerList();
    }


    private void SetPinkMaterial_EditorOnly()
    {
#if UNITY_EDITOR
        //에디터에서 핑크 현상....
        var list = FindObjectsOfType<Renderer>();

        foreach (var i in list)
        {
            foreach (var j in i.materials)
            {
                if (j != null)
                {
                    if (j.shader.name.Contains(CommonDefine.ShaderName_DTRBasicUnlitShader))
                        j.shader = Shader.Find("Shader Graphs/" + CommonDefine.ShaderName_DTRBasicUnlitShader);
                    else if (j.shader.name.Contains(CommonDefine.ShaderName_DTRBasicLitShader))
                        j.shader = Shader.Find("Shader Graphs/" + CommonDefine.ShaderName_DTRBasicLitShader);
                }
            }
        }

#endif
    }
}
