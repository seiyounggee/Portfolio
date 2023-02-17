using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;


public partial class InGameManager : MonoSingletonNetworkBehaviour<InGameManager>
{
    [ReadOnly] public PlayerMovement myPlayer = null;
    [ReadOnly] public WayPointSystem.WaypointsGroup wayPoints = null;
    [ReadOnly] public MapObjectManager mapObjectManager = null;
    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkRunner myNetworkRunner => photonNetworkManager.MyNetworkRunner;
    private NetworkInGameRPCManager myNetworkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    public IEnumerator gameRoutine = null;
    public IEnumerator updateNetworkPlayerList = null;
    public IEnumerator updateRankList = null;
    public IEnumerator updatePlayerBehindList = null;
    public IEnumerator updateMapObjectList = null;
    public IEnumerator updatePingInfo = null;

    public enum GameState { Initialize, IsReady, StartCountDown, PlayGame, EndCountDown, EndGame }
    [ReadOnly] public GameState gameState;

    public enum GameMode { Solo, Team }
    [ReadOnly] public GameMode gameMode = GameMode.Solo; //TODO...

    [System.Serializable]
    public class PlayerInfo
    {
        public GameObject go;
        public PlayerMovement pm;
        public PlayerData data = new PlayerData();
        public int playerID { get { if (pm != null) return pm.PlayerID; else return -1; }}
        public float distLeft { get { if (pm != null) return pm.GetMinDistLeft(); else return -1f; } }
        public int currentLap { get { if (pm != null) return pm.currentLap; else return -1; } }

        public int currentRank { get { if (pm != null) return pm.currentRank; else return -1; } }

        public bool isEnterFinishLine_Local { get { if (pm != null) return pm.isEnteredTheFinishLine; else return false; } }

        public bool isEnterFinishLine_Network = false; //EVENT_PassedFinishLine에 의해 설정되는 값... 위에 pm이 null되는 경우때문에 network 변수로 관리

        public double enterFinishLineTime_Network = 0; //EVENT_PassedFinishLine에 의해 설정되는 값

        public float distanceBetweenMyPlayer { get { if (pm != null) return pm.GetDistBetweenMyPlayer(); else return 0f; } }

        public bool isReady = false;
    }

    //플레이어가 나가면 ListOfPlayers count는 유지됨!! go & pm 은 null이 됨
    [ReadOnly] public List<PlayerInfo> ListOfPlayers = new List<PlayerInfo>();


    [ReadOnly] public List<PlayerInfo> ListOfPlayerBehind= new List<PlayerInfo>(); //뒤쪽에 있는 player list...

    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] public string UserId;
        [SerializeField] public string ownerNickName;
        [SerializeField] public int playerId;
        [SerializeField] public int carID;
        [SerializeField] public int characterID;

        //추후에... 추가 정보 넣자...!
        //TODO....
    }

    public bool isPlayGame = false;
    public bool isStartCountDown = false;
    public double playStartTime;

    public bool playerIsSetInPosition = false;
    public bool myPlayerDataIsSent = false;

    public double totalTimeGameElapsed
    {
        get
        {
            if (myNetworkInGameRPCManager != null)
                return (myNetworkInGameRPCManager.EndRaceTick - myNetworkInGameRPCManager.StartRaceTick) * PhotonNetworkManager.Instance.MyNetworkRunner.DeltaTime;
            else
                return 0f;
        }
    }
    
    public double myTimeRecordElapsed
    {
        get
        {
            if (myNetworkInGameRPCManager != null)
                return (myNetworkRunner.Simulation.Tick - myNetworkInGameRPCManager.StartRaceTick) * PhotonNetworkManager.Instance.MyNetworkRunner.DeltaTime;
            else
                return 0f;
        }
    }

    public bool isEndCountStarted = false;
    public bool isGameEnded = false;
    public bool isInputDelay = false;

    public bool IsHost { get { return PhotonNetworkManager.Instance.IsHost; } }

    public bool myPlayerPassedFinishLine
    {
        get
        {
            if (myPlayer != null)
                return myPlayer.isEnteredTheFinishLine;

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
                var p = ListOfPlayers.Find(x => x.playerID.Equals(myPlayer.PlayerID));
                return p;
            }

            return null;
        }
    }

    public int myPlayerPlayerID
    {
        get
        {
            if (myPlayer != null)
                return myPlayer.PlayerID;
            else
                return -1;
        }
    }

    public PlayerInfo GetPlayerInfo(int viewID)
    {
        PlayerInfo info = null;

        if (ListOfPlayers != null && ListOfPlayers.Count > 0)
        {
            info = ListOfPlayers.Find(x => x.pm != null && x.pm.PlayerID.Equals(viewID));
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

    private void Update()
    {
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var ui = PrefabManager.Instance.UI_PanelCommon;
                string msg = "End Match?\nPress Ok to go Home";

                if (gameState != GameState.PlayGame)
                    return;

                ui.SetData(msg, 
                    () => 
                    {
                        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
                            return;

                        if (gameState != GameState.PlayGame)
                            return;

                        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
                            PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
                    }
                    );
                ui.Show(UI_PanelBase.Depth.High);
            }
        }

        //temp for test...!
#if UNITY_EDITOR
        if (myPlayer != null && myPlayer.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                myNetworkInGameRPCManager.RPC_ChangeLane_Left(myPlayer.PlayerID);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                myNetworkInGameRPCManager.RPC_ChangeLane_Right(myPlayer.PlayerID);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (myPlayer.isDecelerating == false)
                {
                    myNetworkInGameRPCManager.RPC_Deceleration_Start(myPlayer.PlayerID);
                }
            }

            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
            {
                if (myPlayer.isDecelerating == true)
                    myNetworkInGameRPCManager.RPC_Deceleration_End(myPlayer.PlayerID);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                PlayerMovement.CarBoosterLevel boosterLv = myPlayer.GetAvailableInputBooster();
                if (boosterLv == PlayerMovement.CarBoosterLevel.None)
                    return;

                myNetworkInGameRPCManager.RPC_BoostPlayer(myPlayer.PlayerID, (int)boosterLv);
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

        playerIsSetInPosition = false;
        isStartCountDown = false;
        isPlayGame = false;
        isEndCountStarted = false;
        isGameEnded = false;
        isInputDelay = false;

        ListOfPlayers.Clear();
        ListOfPlayerBehind.Clear();
        //ListOfFoodTrucks.Clear();

        InitializeMiniMap();

        PhotonNetworkManager.Instance.ClearPingInfoList();
        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerJoined = PhotonCallback_OnPlayerJoined;
        PhotonNetworkManager.Instance.PhotonCallback_OnPlayerLeft = PhotonCallback_OnPlayerLeft;

        UtilityCoroutine.StopCoroutine(ref updateMapObjectList, this);
        UtilityCoroutine.StopCoroutine(ref updateNetworkPlayerList, this);
        UtilityCoroutine.StopCoroutine(ref updateRankList, this);
        UtilityCoroutine.StopCoroutine(ref updatePlayerBehindList, this);
        UtilityCoroutine.StopCoroutine(ref updatePingInfo, this);
    }

    private IEnumerator GameRoutine()
    {
        yield return StartCoroutine(ChangeGameState(GameState.Initialize));
        yield return null;
        yield return StartCoroutine(WaitForAllPlayersLoadScene());
        yield return null;

        SetMap();
        SpawnPlayer();
        SetUI();
        PoolGameObjects();
        SetCam();
        SetMiniMap();

        yield return null;

        yield return StartCoroutine(WaitForSpawnedPlayers());

        yield return StartCoroutine(SendPlayerData());

        yield return StartCoroutine(ChangeGameState(GameState.IsReady));

        yield return StartCoroutine(WaitForAllPlayerToBeReady());

        yield return StartCoroutine(SetPlayerToStartPosition());

        yield return null;

        yield return StartCoroutine(ChangeGameState(GameState.StartCountDown));
        yield return StartCoroutine(WaitForCountDown());

        yield return StartCoroutine(ChangeGameState(GameState.PlayGame));
        yield return StartCoroutine(WaitForPlayGame());

        yield return StartCoroutine(WaitForGameOver());

        yield break;
    }


    public void SetMap()
    {
        wayPoints = FindObjectOfType<WayPointSystem.WaypointsGroup>();
        mapObjectManager = FindObjectOfType<MapObjectManager>();
    }

    public void SpawnPlayer()
    {
        Debug.Log("PhotonNetworkManager.Instance.IsHost: " + PhotonNetworkManager.Instance.IsHost);
        Debug.Log("ListOfNetworkRunners: " + PhotonNetworkManager.Instance.ListOfNetworkRunners.Count);
        //Host가 Spawn 시킴...
        if (PhotonNetworkManager.Instance.IsHost)
        {
            var prefab = PrefabManager.Instance.NetworkPlayer;

            foreach (var i in PhotonNetworkManager.Instance.ListOfNetworkRunners)
            {
                PhotonNetworkManager.Instance.MyNetworkRunner.Spawn(prefab, Vector3.zero, Quaternion.identity, i.playerRef);
            }
        }
    }

    private void SetCam()
    {
        CameraManager.Instance.SetInGameCam();
    }

    private void SetUI()
    {
        var ingameui = PrefabManager.Instance.UI_PanelIngame;
        ingameui.Initialize();
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
            foreach (var i in PhotonNetworkManager.Instance.ListOfPlayerSceneLoaded)
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

                var ui = PrefabManager.Instance.UI_PanelCommon;
                string msg = "Error....!\nSomeone was not ready!";
                ui.SetData(msg);
                ui.Show(UI_PanelBase.Depth.High);

                PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
                timeElapsed = 0f;
                isError = true;
            }

            yield return null;
        }
    }

    IEnumerator WaitForSpawnedPlayers()
    {
        yield return new WaitForSeconds(1f);

        while (myPlayer == null)
        {
            yield return null;
        }

        while (true)
        {
            UtilityCoroutine.StartCoroutine(ref updateNetworkPlayerList, UpdateNetworkPlayerListRoutinte(), this);

            if (ListOfPlayers != null)
            {
                if (PhotonNetworkManager.Instance.RoomPlayersCount == ListOfPlayers.Count)
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("PhotonNetworkManager.Instance.RoomPlayersCount: " + PhotonNetworkManager.Instance.RoomPlayersCount);

#if CHEAT
        if (CommonDefine.isForcePlaySolo)
        {
            UtilityCoroutine.StartCoroutine(ref updateNetworkPlayerList, UpdateNetwortkPlayerList(), this);
            yield break;
        }
#endif

        /*
        if (CommonDefine.GetMaxPlayer() != 1 && PhotonNetworkManager.Instance.RoomPlayersCount <= 1)
        {
            //Room Player Count에 문제가 있을 경우...
            UtilityCommon.ColorLog("Error...! Room Player Count is " + PhotonNetworkManager.Instance.RoomPlayersCount, UtilityCommon.DebugColor.Red);

            var ui = PrefabManager.Instance.UI_PanelCommon;
            string msg = "Error....!\nRoom Player Count is " + PhotonNetworkManager.Instance.RoomPlayersCount;
            ui.SetData(msg);
            ui.Show(UI_PanelBase.Depth.High);

            PhotonNetworkManager.Instance.LeaveRoom(LeaveRoomCallback);
        }
        */

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
            bool isAllPlayerReady = true;
            foreach (var i in ListOfPlayers)
            {
                if (i.isReady == false)
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
                UtilityCommon.ColorLog("Error...! All players Are not ready....", UtilityCommon.DebugColor.Red);

                var ui = PrefabManager.Instance.UI_PanelCommon;
                string msg = "Error....!\nSomeone was not ready!";
                ui.SetData(msg);
                ui.Show(UI_PanelBase.Depth.High);

                PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
                isError = true;
            }

            yield return null;
        }
    }

    private void LeaveRoomCallback()
    {
        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);
    }

    IEnumerator SendPlayerData()
    {
        while (myPlayer == null)
            yield return null;

        //내 정보를 남들한테 보내주자
        PlayerData data = new PlayerData()
        {
            UserId = DataManager.Instance.userData.UserId,
            ownerNickName = DataManager.Instance.userData.NickName,
            playerId = myPlayer.PlayerID,
            carID = DataManager.Instance.userData.MyCarID,
            characterID = DataManager.Instance.userData.MyCharacterID,
        };
        string jsonData = JsonUtility.ToJson(data);

        myNetworkInGameRPCManager.RPC_SetPlayerStats(jsonData);

        yield return null;

        while (myPlayerDataIsSent == false)
            yield return null;
    }


    IEnumerator SetPlayerToStartPosition()
    {
        if (PhotonNetworkManager.Instance.IsHost)
        {
            var randomSpawnPointList = new List<int>();

            var spawnCount = CommonDefine.GetMaxPlayer();

            for (int i = 0; i < spawnCount; i++)
                randomSpawnPointList.Add(i);

            UtilityCommon.ShuffleList<int>(ref randomSpawnPointList);

            Dictionary<int, int> dicOfPlayerSpawnPosition = new Dictionary<int, int>();

            for (int i = 0; i < ListOfPlayers.Count; i++)
            {
                var pm = ListOfPlayers[i].pm;
                if (pm != null)
                {
                    if (i < randomSpawnPointList.Count)
                    {
                        myNetworkInGameRPCManager.RPC_SetPlayerToPosition(pm.PlayerID, randomSpawnPointList[i]);
                    }
                    else
                    {
                        myNetworkInGameRPCManager.RPC_SetPlayerToPosition(pm.PlayerID, 0);
                    }
                }
            }
        }

        yield return null;

        while (playerIsSetInPosition == false)
            yield return null;
    }

    private IEnumerator WaitForCountDown()
    {
        yield return new WaitForSeconds(CommonDefine.START_COUNTDOWN_TIME);
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
        gameState = state;

        switch (gameState)
        {
            case GameState.Initialize:
                {
                    InitializeSettings();

                    //Initialize이후 핑 정보 계속 보내자....!
                    UtilityCoroutine.StartCoroutine(ref updatePingInfo, UpdatePingInfo(), this);
                }
                break;

            case GameState.IsReady:
                {
                    if (myPlayer != null)
                    {
                        myNetworkInGameRPCManager.RPC_IsReady(myPlayer.PlayerID);
                    }
                    else
                    {
                        Debug.Log("Error....! myPlayer is Null???");
                    }
                }
                break;

            case GameState.StartCountDown:
                {
                    //마스터 클라이언트가 게임 카운트다운을 알림!
                    if (PhotonNetworkManager.Instance.IsHost)
                        myNetworkInGameRPCManager.Invoke("RPC_StartCountDown", 0f);
                    else
                    {
                        //혹시나 마스터 클라이언트가 상태가 메롱하면... 다른 사람이 알려주자...!
                        //간혹 이런 경우가 있.... 나중에 host 기반이 아닌 자체 서버에서 패킷 날려주자...
                        myNetworkInGameRPCManager.Invoke("RPC_StartCountDown", 3f);
                    }
                }
                break;

            case GameState.PlayGame:
                {
                    //마스터 클라이언트가 게임 시작을 알림!
                    if (PhotonNetworkManager.Instance.IsHost)
                        myNetworkInGameRPCManager.Invoke("RPC_PlayGame", 0f);
                    else
                    {
                        //혹시나 마스터 클라이언트가 상태가 메롱하면... 다른 사람이 알려주자...!
                        //간혹 이런 경우가 있.... 나중에 host 기반이 아닌 자체 서버에서 패킷 날려주자...
                        myNetworkInGameRPCManager.Invoke("RPC_PlayGame", 3f);
                    }

                }
                break;

            case GameState.EndCountDown:
                {

                }
                break;

            case GameState.EndGame:
                {
                    FinalUpdateRankList();
                    SetEndGameResultToUserData();
                    SetEndGameResultToMatchRecordData();

                    SoundManager.Instance.PlaySound(SoundManager.SoundClip.Applause);
                    SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.Ingame_BGM_02, SoundManager.PlaySoundType.Immediate);
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
            if (i == null)
                continue;

            if (ListOfPlayers.Exists(x => x.playerID.Equals(i.PlayerID)))
                continue;

            ListOfPlayers.Add(new PlayerInfo() { go = i.gameObject, pm = i, enterFinishLineTime_Network = 0f, isReady = false });
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
                    .ThenByDescending(x=>x.pm.currentMoveIndex)
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
                    if (myPlayer == null || i.playerID.Equals(myPlayer.PlayerID))
                        continue;

                    if (i.pm == null || i.pm.playerCar == null || i.pm.playerCar.currentCar == null || i.pm.playerCar.currentCar.go == null)
                    {
                        if (ListOfPlayerBehind != null && ListOfPlayerBehind.Contains(i))
                            ListOfPlayerBehind.Remove(i);
                        continue;
                    }

                    var renderer = i.pm.playerCar.currentCar.go.GetComponentInChildren<SkinnedMeshRenderer>();

                    if (renderer != null)
                    {
                        if (CameraManager.Instance.currentCamType == CameraManager.CamType.Follow_Type_Default
                            && myPlayer.isMoving && !myPlayer.isFlipped && !myPlayer.isOutOfBoundary && !myPlayer.isEnteredTheFinishLine)
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
                                if (Vector3.Distance(myPlayer.transform.position, i.pm.transform.position) < CommonDefine.PLAYER_INDICATOR_CHECK_BEHIND_DIST
                                    && i.pm.isFlipped == false && i.pm.isOutOfBoundary == false && i.pm.isEnteredTheFinishLine == false)
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
                    && CameraManager.Instance.currentCamType == CameraManager.CamType.Follow_Type_Default)
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

            yield return new WaitForSeconds(0.2f);
        }
    }


    private void SetEndGameResultToUserData()
    {
#if CHEAT
        if (CommonDefine.isForcePlaySolo)
            return;
#endif

        foreach (var i in ListOfPlayers)
        {
            if (i != null && i.pm != null && i.pm.IsMine && DataManager.Instance.userData != null)
            {
                if (i.pm.currentRank == 1)
                    DataManager.Instance.userData.GameStats_FirstPlaceCount++;
                else if (i.pm.currentRank == 2)
                    DataManager.Instance.userData.GameStats_SecondPlaceCount++;
                else if (i.pm.currentRank == 3)
                    DataManager.Instance.userData.GameStats_ThirdPlaceCount++;
                else if (i.pm.currentRank == 4)
                    DataManager.Instance.userData.GameStats_FourthPlaceCount++;
                else if (i.pm.currentRank == 5)
                    DataManager.Instance.userData.GameStats_FivthPlaceCount++;

                if(i.pm.isEnteredTheFinishLine == false)
                    DataManager.Instance.userData.GameStats_RetireCount++;


                DataManager.Instance.userData.GameStats_GamePlayCount++;
            }
        }

        DataManager.Instance.SaveUserData();
    }

    private void SetEndGameResultToMatchRecordData()
    {
#if CHEAT
        if (CommonDefine.isForcePlaySolo)
            return;
#endif

        if (PhotonNetworkManager.Instance.IsHost == false)
            return;

        DataManager.MATCH_RECORD_DATA.MatchResultInfo data = new DataManager.MATCH_RECORD_DATA.MatchResultInfo();
        data.MapId = CommonDefine.GAME_MAP_ID;
        data.TotalPlayers = ListOfPlayers.Count;

        foreach (var i in ListOfPlayers)
        {
            data.PlayerResultInfoList.Add(new DataManager.MATCH_RECORD_DATA.MatchResultInfo.PlayerResultInfo()
            {
                rank = i.currentRank,
                totalGameTime = i.enterFinishLineTime_Network,
                playerId = i.data.UserId,
                playerCarId = i.data.carID,
                playerCharacterId = i.data.characterID,

                attackCount = 0,
                defenceCount = 0,
            });
        }

        string matchName = System.DateTime.UtcNow.ToString("yyyy-MM-dd  H:m:s", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "__" + SystemInfo.deviceUniqueIdentifier;

        DataManager.Instance.SaveMatchRecordData(data, matchName);
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
            if (gameState == GameState.EndGame)
                break;

            if (PhotonNetworkManager.Instance.IsHost)
            {
                if (mapObjectManager != null)
                {
                    foreach (var i in mapObjectManager.containerBoxList)
                    {
                        StartCoroutine(ContainerCoroutine(i));
                    }
                }
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        yield break;
    }

    private IEnumerator ContainerCoroutine(MapObject_ContainerBox box)
    {
        float counter = 0f;
        float coolTime = box.coolTime;
        float initialDelay = box.startDelay;

        yield return new WaitForSecondsRealtime(initialDelay);

        while (true)
        {
            counter += Time.fixedDeltaTime;

            if (counter >= coolTime)
            {
                if (myPlayer != null && box != null)
                    myNetworkInGameRPCManager.RPC_ActivateContainerBox(myPlayer.PlayerID, box.id);

                counter = 0f;
            }

            yield return new WaitForFixedUpdate();


            if (gameState == GameState.EndGame)
                break;

            if (PhotonNetworkManager.Instance.IsHost == false)
                break;
        }
    }


    private IEnumerator UpdatePingInfo()
    {
        float PING_INFO_SEND_RATE = 0.5f;
        while (true)
        {
            if (myNetworkInGameRPCManager == null)
                break;

            yield return new WaitForSeconds(PING_INFO_SEND_RATE);
            myNetworkInGameRPCManager.RPC_SendPingInfo();


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

        if (gameState == GameState.Initialize || gameState == GameState.EndGame)
            block = true;

        if (isGameEnded)
            block = true;

        if (myPlayer.isEnteredTheFinishLine)
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

        float timer = CommonDefine.GAME_INPUT_DELAY;

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

        if (CommonDefine.GetFinalLapCount() == -1)
            return false;

        if (lap == CommonDefine.GetFinalLapCount() - 1)
            return true;

        return isLastLap;
    }

    public bool IsFinishLap(int lap) //최종 Lap
    {
        bool isFinalLap = false;

        if (CommonDefine.GetFinalLapCount() == -1)
            return false;

        if (lap >= CommonDefine.GetFinalLapCount())
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
}
