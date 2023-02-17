using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Failed,
    Connected
}

public class PhotonNetworkManager : MonoSingletonPhotonCallback<PhotonNetworkManager>, INetworkRunnerCallbacks
{
    public static ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

    public static bool isJoiningRoom = false;
    public static bool isJoinedRoom = false;
    public static bool isWaitingForOtherPlayers = false;
    public static float timeWaitingForOtherPlayers = 15f;
    public static bool matchSuccess = false;
    public static bool isMatchSet = false; //master client에 의해 결정됨... matchSuccess 여부가
    public static bool cancelSearchingRoom = false;

    [ReadOnly] public List<NetworkRunnerInfo> listOfNetworkRunners = new List<NetworkRunnerInfo>();
    public List<NetworkRunnerInfo> ListOfNetworkRunners { get { return listOfNetworkRunners; } }

    [Serializable]
    public class NetworkRunnerInfo
    {
        public PlayerRef playerRef;
        public NetworkRunner networkRunner = null;

        public int playerId { get { return playerRef.PlayerId; } }
    }

    [ReadOnly] public NetworkRunner MyNetworkRunner = null;
    [ReadOnly] public NetworkInGameRPCManager MyNetworkInGameRPCManager = null;

    public NetworkRunnerInfo MyNetworkRunnerInfo
    {
        get
        {
            if (ListOfNetworkRunners != null)
            {
                var x = ListOfNetworkRunners.Find(x => x.networkRunner.Equals(MyNetworkRunner));
                if (x != null)
                    return x;
            }

            return null;
        }

    }

    public PlayerRef MyNetworkRunnerPlayerRef
    {
        get
        {
            if (ListOfNetworkRunners != null)
            {
                var x = ListOfNetworkRunners.Find(x => x.networkRunner.Equals(MyNetworkRunner));
                if (x != null)
                    return x.playerRef;
            }

            return new PlayerRef();
        }
    }


    //구 isMaster Client... 같은 기능
    public bool IsHost
    {
        get
        {
            if (MyNetworkRunner != null)
            {
                if (MyNetworkRunner.GameMode == GameMode.Host)
                    return true;
            }

            return false;
        }

    }

    List<SessionInfo> currentSessionList = new List<SessionInfo>();

    [Serializable]
    public class PingInfo
    {
        public string userId = "-1";
        public NetworkRunnerInfo player = null;
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

    public int RoomPlayersCount { get { if (ListOfNetworkRunners != null) return ListOfNetworkRunners.Count; else return 0; } }

    public Action startGameCallback = null;
    public Action startGameErrorCallback = null;

    public const string SESSION_PROPERTY_MAPID = "mapID";
    public const string SESSION_PROPERTY_MAXPLAYERS = "maxPlayers";

    [Serializable]
    public class PlayerIsSceneLoaded
    {
        public bool isLoaded = false;
        public int id = -1; //photon의 PlayerId 사용
    }
    [SerializeField] public List<PlayerIsSceneLoaded> ListOfPlayerSceneLoaded = new List<PlayerIsSceneLoaded>();


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
        Debug.Log("<color=cyan>Start Game! " + " GAME_MAP_ID: " + CommonDefine.GAME_MAP_ID + " MinPlayer: " + CommonDefine.GetMinPlayer() + " MaxPlayer: " + CommonDefine.GetMaxPlayer() + "</color>");
    }

    private void StartErrorGameCallback()
    {
        Debug.Log("<color=red>Start Game Error...!</color>");

        var panelLobbyMain = PrefabManager.Instance.UI_PanelLobby_Main;
        panelLobbyMain.Initialize();
    }

    public void InitializeNetworkMatchSettings()
    {
        matchSuccess = false;
        isJoiningRoom = false;
        isJoinedRoom = false;
        isWaitingForOtherPlayers = false;
        isMatchSet = false;
        cancelSearchingRoom = false;
        listOfNetworkRunners.Clear();

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
        /*
        ChangePhotonNetworkRate(0);

        UtilityCoroutine.StopCoroutine(ref recordPing, this);
        UtilityCoroutine.StartCoroutine(ref recordPing, RecordPing(), this);
        */

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
        if (MyNetworkRunner == null)
        {
            var runnerGo = new GameObject();
            runnerGo.transform.SetParent(this.transform);
            runnerGo.name = "Photon Network Runner";
            MyNetworkRunner = runnerGo.AddComponent<NetworkRunner>();
            MyNetworkRunner.ProvideInput = true;
            MyNetworkRunner.AddCallbacks(this); //꼭 이 스크립트에 callback 포함시켜주자...!
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
        }
        else
        {
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    async void StartGame(GameMode mode, int mapID = 0, int maxPlayers = 2)
    {
        if (MyNetworkRunner == null)
        {
            return;
        }

        var customProps = new Dictionary<string, SessionProperty>();

        customProps[SESSION_PROPERTY_MAPID] = mapID;
        customProps[SESSION_PROPERTY_MAXPLAYERS] = maxPlayers;

        if (mode == GameMode.Single)
            maxPlayers = 1;

        var result = await MyNetworkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionProperties = customProps,
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

    public async void JoinRandomSession()
    {
        if (isWaitingForOtherPlayers || isJoiningRoom || isJoinedRoom)
            return;

        if (currentSessionList != null && currentSessionList.Count > 0)
        {
            // Store the target session
            SessionInfo session = null;

            foreach (var sessionItem in currentSessionList)
            {
                // Check for a specific Custom Property
                if (sessionItem.Properties.TryGetValue(SESSION_PROPERTY_MAPID, out var propertyType) && propertyType.IsInt)
                {

                    var mapID = (int)propertyType.PropertyValue;

                    // Check for the desired Game Type
                    if (mapID == CommonDefine.GAME_MAP_ID)
                    {

                        // Store the session info
                        session = sessionItem;
                        break;
                    }
                }
            }

            // Check if there is any valid session
            if (session != null)
            {
                Debug.Log($"Joining {session.Name}");

                // Join
                var result = await MyNetworkRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Client, // Server GameMode, could be Shared as well
                    SessionName = session.Name, // Session to Join
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
                StartGame(GameMode.Host, CommonDefine.GAME_MAP_ID, CommonDefine.GetMaxPlayer());
            }
        }
        else
        {
            StartGame(GameMode.Host, CommonDefine.GAME_MAP_ID, CommonDefine.GetMaxPlayer());
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

    public int GetPing()
    {
        //TODO
        return 0;
    }

    public int GetPingVariance()
    {
        //TODO
        return 0;
    }

    [ReadOnly] public Queue<int> pingContainer = new Queue<int>();
    private IEnumerator recordPing = null;
    private IEnumerator RecordPing()
    {
        if (pingContainer == null)
            pingContainer = new Queue<int>();

        int count = 10;
        float sec = 0.5f;

        while (true)
        {
            pingContainer.Enqueue(GetPing());

            if (pingContainer.Count > count)
                pingContainer.Dequeue();

            yield return new WaitForSeconds(sec);
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

    public const int BAD_PING_LIMIT = 100;
    public const int BEST_PING_LIMIT = 25;

    public bool IsPingAtBadCondition()
    {
        int pingLimit = BAD_PING_LIMIT;
        if (GetAveragePing() < pingLimit)
            return false;
        else
            return true;

        return false;
    }

    public bool IsPingAtGoodCondition()
    {
        return !IsPingAtBadCondition();
    }

    public bool IsPingAtBestCondition()
    {
        int pingLimit = BEST_PING_LIMIT;
        if (GetAveragePing() <= pingLimit)
            return true;
        else
            return false;

        return false;
    }

    public void ClearPingInfoList()
    {
        ListOfPingInfo.Clear();
    }

    public void SetPingInfoList(string pid, int ping)
    {
        if (listOfNetworkRunners == null || ListOfPingInfo == null)
            return;

        var player = listOfNetworkRunners.Find(x => x.playerId.Equals(pid));
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

    public NetworkRunnerInfo GetBestPingPlayer()
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

        if (MyNetworkRunner != null)
        {
            var prefab = PrefabManager.Instance.NetworkInGameRPCManager;
            MyNetworkRunner.Spawn(prefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.Log("Error.... MyNetworkRunner is null!!!");
        }
    }

    public void UpdateListOfNetworkRunners(NetworkRunner runner, PlayerRef player)
    {
        foreach (var i in listOfNetworkRunners)
        {
            if (i == null)
                listOfNetworkRunners.Remove(i);
        }

        bool playerExists = false;
        foreach (var i in listOfNetworkRunners)
        {
            if (i.playerRef.Equals(player))
                playerExists = true;
        }

        if (playerExists == false)
            listOfNetworkRunners.Add(new NetworkRunnerInfo() { networkRunner = runner, playerRef = player });
    }

    public void UpdateSceneLoadedList()
    {
        ListOfPlayerSceneLoaded = new List<PlayerIsSceneLoaded>();
        ListOfPlayerSceneLoaded.Clear();

        var list = ListOfNetworkRunners;
        for (int i = 0; i < list.Count; i++)
        {
            ListOfPlayerSceneLoaded.Add(new PlayerIsSceneLoaded() { isLoaded = false, id = list[i].playerId });
        }
    }





    public Action<NetworkRunner, PlayerRef> PhotonCallback_OnPlayerJoined;
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log("<color=cyan>OnPlayerJoined</color>");

        //Runner 명단 Update
        UpdateListOfNetworkRunners(runner, player);

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
        {
            UpdateSceneLoadedList();
            timeWaitingForOtherPlayers = CommonDefine.GAME_TIME_WAITING_BEFORE_START; //사람 들어올떄마다 초기화 해주자
        }

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby)
        {
            //Host만...!
            if (runner.GameMode == GameMode.Host)
            {
                CreateRPCManager();
            }
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
        NetworkRunnerInfo info = null;
        foreach (var i in listOfNetworkRunners)
        {
            if (i.playerRef.Equals(player))
            {
                info = i;
                playerExists = true;
            }
        }

        if (playerExists == true && info != null)
            listOfNetworkRunners.Remove(info);

        //씬 명단 update (일부 경우)
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
            UpdateSceneLoadedList();

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameReady)
            timeWaitingForOtherPlayers = CommonDefine.GAME_TIME_WAITING_BEFORE_START; //사람 들어올떄마다 초기화 해주자

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
        Debug.Log("<color=cyan>OnConnectedToServer</color>");

        SetConnectionStatus(ConnectionStatus.Connected);
        PhotonCallback_OnConnectedToServer?.Invoke(runner);
    }  

    public Action<NetworkRunner> PhotonCallback_OnDisconnectedFromServer;
    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("<color=cyan>OnDisconnectedFromServer</color>");

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
        Debug.Log("<color=cyan>OnSessionListUpdated....! Session List Count: " + sessionList.Count + "</color>");
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
