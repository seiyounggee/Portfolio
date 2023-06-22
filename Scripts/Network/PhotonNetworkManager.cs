using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System.Globalization;
using Fusion.Photon.Realtime;
using PNIX.Engine.NetClient;

public enum ConnectionStatus
{
    None,
    Disconnected,
    Connecting,
    Failed,
    Connected
}

public class PhotonNetworkManager : MonoSingletonPhotonCallback<PhotonNetworkManager>, INetworkRunnerCallbacks
{
    public static ConnectionStatus ConnectionStatus = ConnectionStatus.None;

    public static bool isJoinedLobby = false;
    public static bool isJoiningRoom = false;
    public static bool isJoinedRoom = false;
    public static bool isWaitingForOtherPlayers = false;
    public static float timeWaitingForOtherPlayers = 15f;
    public static bool matchSuccess = false;
    public static bool isMatchSet = false; //master client에 의해 결정됨... matchSuccess 여부가
    public static bool cancelSearchingRoom = false;

    [ReadOnly] public List<NetworkRunnerPlayerInfo> listOfNetworkPlayerInfos = new List<NetworkRunnerPlayerInfo>();
    public List<NetworkRunnerPlayerInfo> ListOfNetworkRunnerPlayerInfos { get { return listOfNetworkPlayerInfos; } }

    [Serializable]
    public class NetworkRunnerPlayerInfo
    {
        public PlayerRef playerRef;
        public NetworkRunner networkRunner = null;

        public int playerId { get { return playerRef.PlayerId; } }
    }

    [ReadOnly] public NetworkRunner MyNetworkRunner = null;
    [ReadOnly] public PlayerRef MyPlayerRef;
    [ReadOnly] public NetworkInGameRPCManager MyNetworkInGameRPCManager = null;
    public List<NetworkInGameRPCManager> ListOfNetworkInGameRPCManager = new List<NetworkInGameRPCManager>();

    //방장 개념... 나중엔 서버 처리로 하는게 나을듯....!
    public bool IsRoomMasterClient
    {
        get
        {
            if (MyNetworkRunner != null)
            {
                if(MyNetworkRunner.IsSharedModeMasterClient)
                    return true;
            }

            return false;
        }

    }

    List<SessionInfo> currentSessionList = new List<SessionInfo>();

    public SessionInfo MySessionInfo
    {
        get
        {
            if (MyNetworkRunner != null)
                return MyNetworkRunner.SessionInfo;

            return null;
        }
    }

    public string MyRegion
    {
        get  
        {
            if (MySessionInfo != null && !string.IsNullOrEmpty(MySessionInfo.Region))
            {
                return MySessionInfo.Region;
            }
            else
            {
                var region = PhotonAppSettings.Instance.AppSettings.BestRegionSummaryFromStorage;

                if (string.IsNullOrEmpty(region) || IsValidPhotonRegion(region) == false)
                {
                    region = PhotonAppSettings.Instance.AppSettings.FixedRegion;
                }

                if (string.IsNullOrEmpty(region) || IsValidPhotonRegion(region) == false)
                {
                    region = CNetworkManager.Instance.CountryCode.ToLower();
                }

                return region;
            }
        }
    }

    public bool IsValidPhotonRegion(string region)
    {
        if (string.IsNullOrEmpty(region))
            return false;

        List<string> validList = new List<string>()
        {
            "asia",
            "cn",
            "jp",
            "eu",
            "sa",
            "kr",
            "us",
            "usw",
        };

        foreach (var i in validList)
        {
            if (region.ToLower().Equals(i))
                return true;
        }

        return false;
    }

    [Serializable]
    public class PingInfo
    {
        public string userId = "-1";
        public NetworkRunnerPlayerInfo player = null;
        public Queue<int> pingQueue = new Queue<int>();

        public int GetAveragePing()
        {
            if (pingQueue != null && pingQueue.Count > 0)
            {
                int sum = 0;
                foreach (var i in pingQueue)
                    sum += i;

                int avg = sum / pingQueue.Count;
                return avg;
            }

            return -1;
        }
    }

    [SerializeField] public List<PingInfo> ListOfPingInfo = new List<PingInfo>();

    public int RoomPlayersCount { get { if (ListOfNetworkRunnerPlayerInfos != null) return ListOfNetworkRunnerPlayerInfos.Count; else return 0; } }

    public Action startGameCallback = null;
    public Action startGameErrorCallback = null;

    public const string SESSION_PROPERTY_ROOMID = "mapID"; //맵id
    public const string SESSION_PROPERTY_REALPLAYERCOUNT = "realPlayerCnt"; 
    public const string SESSION_PROPERTY_AIPLAYERCOUNT = "aiPlayerCnt"; 


    public override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        startGameCallback += StartGameCallback;
        startGameErrorCallback += StartErrorGameCallback;
    }

    private void OnDisable()
    {
        startGameCallback -= StartGameCallback;
        startGameErrorCallback -= StartErrorGameCallback;
    }

    private void StartGameCallback()
    {
        Debug.Log("<color=cyan>Start Game! " + " GAME_MAP_ID: " + DataManager.GameMapID + " Real Player Count: " + DataManager.Instance.GetSessionRealPlayerCount() + "</color>");
    }

    private void StartErrorGameCallback()
    {
        Debug.Log("<color=red>Start Game Error...!</color>");

        var panelLobbyMain = PrefabManager.Instance.UI_PanelLobby_Main;
        panelLobbyMain.Initialize();
    }

    public void Initialize()
    {
        matchSuccess = false;
        isJoinedLobby = false;
        isJoiningRoom = false;
        isJoinedRoom = false;
        isWaitingForOtherPlayers = false;
        isMatchSet = false;
        cancelSearchingRoom = false;
        listOfNetworkPlayerInfos.Clear();
        ListOfNetworkInGameRPCManager.Clear();

        if (MyNetworkInGameRPCManager != null)
        {
            Destroy(MyNetworkInGameRPCManager.gameObject);
            MyNetworkInGameRPCManager = null;
        }

        if (MyNetworkInGameRPCManager != null)
        {
            Destroy(MyNetworkInGameRPCManager.gameObject);
            MyNetworkInGameRPCManager = null;
        }

        if (MyNetworkRunner != null)
        {
            Destroy(MyNetworkRunner.gameObject);
            MyNetworkRunner = null;
        }
    }

    public void ResetNetworkMatchSettings()
    {
        matchSuccess = false;
        isJoinedLobby = false;
        isJoiningRoom = false;
        isJoinedRoom = false;
        isWaitingForOtherPlayers = false;
        isMatchSet = false;
        cancelSearchingRoom = false;
        listOfNetworkPlayerInfos.Clear();
        ListOfNetworkInGameRPCManager.Clear();

        if (MyNetworkInGameRPCManager != null)
        {
            Destroy(MyNetworkInGameRPCManager.gameObject);
            MyNetworkInGameRPCManager = null;
        }

        if (MyNetworkInGameRPCManager != null)
        {
            Destroy(MyNetworkInGameRPCManager.gameObject);
            MyNetworkInGameRPCManager = null;
        }

        LeaveSession(() =>
            {
                var nr = CreatePhotonNetworkRunner();
                if (nr != null)
                    JoinLobby(nr);
                else
                    Debug.Log("nr is null...");
            }
        );

    }

    public NetworkRunner CreatePhotonNetworkRunner()
    {
        if (MyNetworkRunner  == null)
        {
            var runnerGo = new GameObject();
            runnerGo.transform.SetParent(this.transform);
            runnerGo.name = "Photon Network Runner";
            MyNetworkRunner = runnerGo.AddComponent<NetworkRunner>();
            MyNetworkRunner.ProvideInput = true;
            MyNetworkRunner.AddCallbacks(this);
        }

        return MyNetworkRunner;
    }


    public async void JoinLobby(NetworkRunner runner)
    {
        if (runner == null)
        {
            Debug.Log("<color=red>Error...! NetworkRunner is null</color>");
            return;
        }

        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

        if (result.Ok)
        {
            // all good
            isJoinedLobby = true;
            Debug.Log("<color=cyan>NetworkRunner Successfully Joined Lobby!</color>");
        }
        else
        {
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    async void StartGame(GameMode mode, long roomID = 0, int realPlayerCnt = 2, int aiPlayerCnt = 0)
    {
        if (MyNetworkRunner == null)
        {
            return;
        }

        var appSettings = IsValidPhotonRegion(MyRegion) ? BuildCustomAppSetting(MyRegion) : BuildCustomAppSetting();

        var customProps = new Dictionary<string, SessionProperty>();

        customProps[SESSION_PROPERTY_ROOMID] = roomID.ToString();
        customProps[SESSION_PROPERTY_REALPLAYERCOUNT] = realPlayerCnt;
        customProps[SESSION_PROPERTY_AIPLAYERCOUNT] = aiPlayerCnt;

        var result = await MyNetworkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionProperties = customProps,
            SessionName = roomID.ToString(),
            Scene = null,
            CustomPhotonAppSettings = appSettings

        });

        if (result.Ok)
        {
            startGameCallback?.Invoke();
        }
        else
        {
            Debug.Log("<color=red>Error....! Start Random Game</color>");
        }
    }

    private AppSettings BuildCustomAppSetting(string region = null, string customAppID = null, string appVersion = "1.0.0")
    {

        var appSettings = PhotonAppSettings.Instance.AppSettings.GetCopy();

        appSettings.UseNameServer = true;
        appSettings.AppVersion = appVersion;

        if (string.IsNullOrEmpty(region) == false
            && IsValidPhotonRegion(region))
        {
            appSettings.FixedRegion = region.ToLower();
        }

        // If the Region is set to China (CN),
        // the Name Server will be automatically changed to the right one
        // appSettings.Server = "ns.photonengine.cn";

        return appSettings;
    }

    //특정 id로 연결
    public async void JoinSession_Multi_Paticular(long desiredRoomID, int realPlayerCnt, int aiPlayerCnt)
    {
        if (isWaitingForOtherPlayers || isJoiningRoom || isJoinedRoom || !isJoinedLobby)
            return;

        if (currentSessionList != null && currentSessionList.Count > 0)
        {
            // Store the target session
            SessionInfo session = null;

            foreach (var sessionItem in currentSessionList)
            {
                if (sessionItem.Name.Equals(desiredRoomID.ToString()))
                {
                    // Store the session info
                    session = sessionItem;
                    break;
                }
            }

            // Check if there is any valid session
            if (session != null && session.IsOpen && session.IsValid)
            {
                Debug.Log($"Joining {session.Name}");

                DataManager.Instance.SetMapID(DataManager.GameMapID); //todo...!

                // Join
                var result = await MyNetworkRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Shared, // Server GameMode, could be Shared as well
                    SessionName = session.Name, // Session to Join
                    Scene = null,
                    // ...
                });

                if (result.Ok)
                {
                    startGameCallback?.Invoke();
                }
                else
                {

                }
            }
            else
            {
                StartGame(GameMode.Shared, desiredRoomID, realPlayerCnt, aiPlayerCnt);
            }
        }
        else
        {
            StartGame(GameMode.Shared, desiredRoomID, realPlayerCnt, aiPlayerCnt);
        }
    }

    public async void LeaveSession(Action callback = null)
    {
        if (MyNetworkRunner != null)
        {
            //Leave Session
            await MyNetworkRunner.Shutdown();

            //Destroy runner
            Destroy(MyNetworkRunner.gameObject);

            MyNetworkRunner = null;
            currentSessionList.Clear();
        }

        callback?.Invoke();
    }

    public void CancelSearchingRoom(Action callback = null)
    {
        cancelSearchingRoom = true;
        timeWaitingForOtherPlayers = 0f;

        LeaveSession(callback);
    }

    public bool IsSearchingRoom()
    {
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
            return true;
        else
            return false;
    }

    public void SetSessionOpen(bool isOpen)
    {
        if (MyNetworkRunner != null && MyNetworkRunner.SessionInfo != null && MyNetworkRunner.SessionInfo.IsValid)
        {
            MyNetworkRunner.SessionInfo.IsOpen = isOpen;
            MyNetworkRunner.SessionInfo.IsVisible = false; //항상 비공개로!
        }
    }

    public int GetPing() //Rount Trip Time
    {
        if (MyNetworkRunner != null && MyPlayerRef != null
            && InGameManager.Instance.myPlayer != null
            && (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown))
        {
            return (int)(MyNetworkRunner.GetPlayerRtt(MyPlayerRef) * 1000);
        }

        return 0;
    }


    [ReadOnly] public Queue<int> pingContainer = new Queue<int>();
    public IEnumerator recordPing = null;
    public IEnumerator RecordPing()
    {
        if (pingContainer == null)
            pingContainer = new Queue<int>();

        int count = 10;
        float sec = 0.5f;

        while (true)
        {
            if (GetPing() > 0) //Valid한 경우에만...
                pingContainer.Enqueue(GetPing());

            if (pingContainer.Count > count)
                pingContainer.Dequeue();

            yield return new WaitForSeconds(sec);

            if (MyNetworkInGameRPCManager == null)
                break;
        }
    }

    public int GetAveragePing()
    {
        if (pingContainer != null && pingContainer.Count > 0)
        {
            int avg = 0;
            foreach (var i in pingContainer)
                avg += i;

            avg = (int)((float)avg / pingContainer.Count);
            return avg;
        }

        return GetPing();
    }

    public const int BAD_PING_LIMIT = 200;
    public const int GOOD_PING_LIMIT = 30;

    public bool IsPingAtBadCondition()
    {
        int pingLimit = BAD_PING_LIMIT;
        if (GetAveragePing() < pingLimit)
            return false;
        else
            return true;
    }

    public bool IsPingAtGoodCondition()
    {
        return !IsPingAtBadCondition();
    }

    public bool IsPingAtBestCondition()
    {
        int pingLimit = GOOD_PING_LIMIT;
        if (GetAveragePing() <= pingLimit)
            return true;
        else
            return false;
    }

    public void ClearPingInfoList()
    {
        ListOfPingInfo.Clear();
    }

    public void SetPingInfoList(string pid, int ping)
    {
        if (listOfNetworkPlayerInfos == null || ListOfPingInfo == null)
            return;

        var player = listOfNetworkPlayerInfos.Find(x => x.playerId.Equals(pid));
        var info = ListOfPingInfo.Find(x => x.userId.Equals(pid));

        if (info == null)
        {
            var i = new PingInfo() { userId = pid, player = player };
            i.pingQueue.Enqueue(ping);

            ListOfPingInfo.Add(i);
        }
        else
        {
            info.pingQueue.Enqueue(ping);
            if (info.pingQueue.Count > 20)
                info.pingQueue.Dequeue();
        }
    }

    public NetworkRunnerPlayerInfo GetBestPingPlayer()
    {
        if (ListOfPingInfo == null && ListOfPingInfo.Count > 0)
            return null;

        ListOfPingInfo.Sort((x, y) => x.GetAveragePing().CompareTo(y.GetAveragePing()));

        if (ListOfPingInfo[0].player != null)
            return ListOfPingInfo[0].player;
        else
            return null;
    }

    private void SetConnectionStatus(ConnectionStatus status)
    {
        Debug.Log("Setting connection status to " + status.ToString());

        ConnectionStatus = status;

        if (!Application.isPlaying)
            return;
    }

    public void CreateRPCManager()
    {
        if (MyNetworkInGameRPCManager != null)
        {
            Destroy(MyNetworkInGameRPCManager.gameObject);
            MyNetworkInGameRPCManager = null;
        }

        var prefab = PrefabManager.Instance.NetworkInGameRPCManager;
        foreach (var i in ListOfNetworkRunnerPlayerInfos)
        {
            i.networkRunner.Spawn(prefab, Vector3.zero, Quaternion.identity, i.playerRef);
        }
    }

    public void UpdateListOfNetworkRunnerPlayerInfo(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            MyPlayerRef = player;
        }

        listOfNetworkPlayerInfos.Clear();
        foreach (var i in runner.ActivePlayers)
        {
            listOfNetworkPlayerInfos.Add(new NetworkRunnerPlayerInfo() { networkRunner = runner, playerRef = i });
        }

        listOfNetworkPlayerInfos.Sort((x, y) => x.playerRef.PlayerId.CompareTo(y.playerRef.PlayerId));
    }

    public void UpdateListOfNetworkInGameRPCManager(NetworkInGameRPCManager manger)
    {
        if (ListOfNetworkInGameRPCManager.Contains(manger) == false)
            ListOfNetworkInGameRPCManager.Add(manger);

        foreach (var i in ListOfNetworkInGameRPCManager)
        {
            if (i.IsMine)
                MyNetworkInGameRPCManager = i;
        }
    }



    public Action<NetworkRunner, PlayerRef> PhotonCallback_OnPlayerJoined;
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log("<color=cyan>OnPlayerJoined!</color>");

        //Runner 명단 Update
        UpdateListOfNetworkRunnerPlayerInfo(runner, player);

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
        {
            timeWaitingForOtherPlayers = DataManager.GAME_TIME_WAITING_BEFORE_START; //사람 들어올떄마다 초기화 해주자
        }

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby)
        {
            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameReady);
        }

        PhotonCallback_OnPlayerJoined?.Invoke(runner, player);

        SetConnectionStatus(ConnectionStatus.Connected);
    }

    public Action<NetworkRunner, PlayerRef> PhotonCallback_OnPlayerLeft;
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        //Remove Player from List
        bool playerExists = false;
        NetworkRunnerPlayerInfo info = null;
        foreach (var i in listOfNetworkPlayerInfos)
        {
            if (i.playerRef.Equals(player))
            {
                info = i;
                playerExists = true;
            }
        }

        if (playerExists == true && info != null)
            listOfNetworkPlayerInfos.Remove(info);

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
            timeWaitingForOtherPlayers = DataManager.GAME_TIME_WAITING_BEFORE_START; //사람 들어올떄마다 초기화 해주자

        PhotonCallback_OnPlayerLeft?.Invoke(runner, player);
    }

    public Action<NetworkRunner, PlayerRef, NetworkInput> PhotonCallback_OnInputMissing;
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) => PhotonCallback_OnInputMissing?.Invoke(runner, player, input);

    public Action<NetworkRunner, ShutdownReason> PhotonCallback_OnShutdown;
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SetConnectionStatus(ConnectionStatus.Disconnected);        
        PhotonCallback_OnShutdown?.Invoke(runner, shutdownReason);
    }

    public Action<NetworkRunner> PhotonCallback_OnConnectedToServer;
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("<color=cyan>Photon OnConnectedToServer</color>");

        SetConnectionStatus(ConnectionStatus.Connected);
        PhotonCallback_OnConnectedToServer?.Invoke(runner);
    }  

    public Action<NetworkRunner> PhotonCallback_OnDisconnectedFromServer;
    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("<color=cyan>Photon OnDisconnectedFromServer</color>");

        SetConnectionStatus(ConnectionStatus.Disconnected);
        PhotonCallback_OnDisconnectedFromServer?.Invoke(runner);
    }  

    public Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]> PhotonCallback_OnConnectRequest;
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) => PhotonCallback_OnConnectRequest?.Invoke(runner, request, token); 

    public Action<NetworkRunner, NetAddress, NetConnectFailedReason> PhotonCallback_OnConnectFailed;
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        SetConnectionStatus(ConnectionStatus.Failed);
        PhotonCallback_OnConnectFailed?.Invoke(runner, remoteAddress, reason);
    } 

    public Action<NetworkRunner, SimulationMessagePtr> PhotonCallback_OnUserSimulationMessage;
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) => PhotonCallback_OnUserSimulationMessage?.Invoke(runner, message); 

    public Action<NetworkRunner, List<SessionInfo>> PhotonCallback_OnSessionListUpdated;
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("<color=cyan>Photon OnSessionListUpdated....! Session List Count: " + sessionList.Count + "</color>");
        currentSessionList = sessionList;

        PhotonCallback_OnSessionListUpdated?.Invoke(runner, sessionList);
    }

    public Action<NetworkRunner, Dictionary<string, object>> PhotonCallback_OnCustomAuthenticationResponse;
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) => PhotonCallback_OnCustomAuthenticationResponse?.Invoke(runner, data); 

    public Action<NetworkRunner, PlayerRef, ArraySegment<byte>> PhotonCallback_OnReliableDataReceived;
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) => PhotonCallback_OnReliableDataReceived?.Invoke(runner, player, data); 

    public Action<NetworkRunner> PhotonCallback_OnSceneLoadDone;
    public void OnSceneLoadDone(NetworkRunner runner) => PhotonCallback_OnSceneLoadDone?.Invoke(runner); 

    public Action<NetworkRunner> PhotonCallback_OnSceneLoadStart;
    public void OnSceneLoadStart(NetworkRunner runner) => PhotonCallback_OnSceneLoadStart?.Invoke(runner); 

    public Action<NetworkRunner, HostMigrationToken> PhotonCallback_OnHostMigration;
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) => PhotonCallback_OnHostMigration?.Invoke(runner, hostMigrationToken); 

    public Action<NetworkRunner, NetworkInput> PhotonCallback_OnInput;
    public void OnInput(NetworkRunner runner, NetworkInput input) => PhotonCallback_OnInput?.Invoke(runner, input);
}
