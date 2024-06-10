using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Reflection;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UI = UnityEngine.UI;
using Quantum;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager_Client : MonoSingleton<NetworkManager_Client>, IConnectionCallbacks, IMatchmakingCallbacks
{
    public int room_MaxPlayer { get; set; } = 0;
    public const int LOCAL_PLAYER = 1; //1: me 0: spectator mode

    [Header("Inspector Settings")]
    public RuntimeConfig runtimeConfig = null;
    public RuntimePlayer runtimePlayer = null;

    public static QuantumLoadBalancingClient QuantumClient
    {
        get; set;
    }

    public enum PhotonEventCode : byte
    {
        JoinRoomFailed = 110,
        SendRoomData = 111,
        RoomDataReady = 112,
        SceneLoadReady = 113,
        SceneLoadFailed = 114,
    }

    public string BestRegion 
    { 
        get
        {
            if (QuantumClient != null)
                return QuantumClient.CloudRegion.ToString();
            else
                return string.Empty;
        } 
    }

    [Space(10)]
    [Header("Network Parameters")]
    [ReadOnly] public bool Quantum_IsConnectedAndReady = false;
    [ReadOnly] public bool Quantum_IsConnectedToMaster = false;
    [ReadOnly] public bool Quantum_IsInRoom = false;
    [ReadOnly] public bool Quantum_IsMatchMakingFailed = false;

    public bool Quantum_IsRoomFull
    {
        get
        {
            if (QuantumClient == null || QuantumClient.CurrentRoom == null)
                return false;

            if (QuantumClient.CurrentRoom.MaxPlayers == QuantumClient.CurrentRoom.PlayerCount)
                return true;

            return false;
        }
    }

    private List<string> playersRoomDataReceived = new List<string>();

    public bool Quantum_IsMyPlayerRoomDataReceived
    {
        get
        {
            if (playersRoomDataReceived.Contains(AccountManager.Instance.PID))
                return true;

            return false;
        }
    }

    public bool Quantum_IsAllPlayerRoomDataReceived
    {
        get
        {
            if (playersRoomDataReceived.Count == QuantumClient.CurrentRoom.PlayerCount)
                return true;

            return false;
        }
    }

    public Quantum.InGamePlayMode CurrentPlayMode
    {
        get
        {
            if (Instance != null && AccountManager.Instance != null)
                return (Quantum.InGamePlayMode)AccountManager.Instance.CurrentPlayMode;
            else
                return InGamePlayMode.SoloMode;
        }
    }

    [Serializable]
    public class RoomData
    {
        public bool isRoomDataSet = false;
        public int mapID = -1;
        public bool isInGameSceneLoaded = false;
        public bool isMapSceneLoaded = false;
        public int ingamePlayMode = -1;
        public List<Photon.Realtime.Player> randomShuffledListOfPlayerForTeamMatch = new List<Photon.Realtime.Player>();
        public int randomSeed = 0;

        public void Clear()
        {
            isRoomDataSet = false;
            isInGameSceneLoaded = false;
            isMapSceneLoaded = false;
            mapID = -1;
            ingamePlayMode = -1;
            randomShuffledListOfPlayerForTeamMatch.Clear();
            randomSeed = 0;
        }

        public string GetQuantumMapAssetName()
        {
            string assetName = string.Empty;

            var data = ReferenceManager.Instance.MapData;
            if (data != null && data.MapInfoList != null)
            {
                var map = data.MapInfoList.Find(x => x.MapID.Equals(mapID));
                if (map != null)
                {
                    assetName = map.QuantumMapAssetName;
                }
            }

            return assetName;
        }

        public string GetQuantumCustomMapDataAssetName()
        {
            string assetName = string.Empty;

            var data = ReferenceManager.Instance.MapData;
            if (data != null && data.MapInfoList != null)
            {
                var map = data.MapInfoList.Find(x => x.MapID.Equals(mapID));
                if (map != null)
                {
                    assetName = map.QuantumCustomMapDataAssetName;
                }
            }

            return assetName;
        }
    }

    public RoomData Quantum_RoomData = new RoomData();


    private List<string> playersSceneLoaded = new List<string>(); //PID list에 넣어주자
    public bool Quantum_IsAllPlayerSceneLoaded
    {
        get         
        {
            if (playersSceneLoaded.Count == QuantumClient.CurrentRoom.PlayerCount)
                return true;

            return false;
        }
    }

    public bool Quantum_IsMasterClient
    {
        get
        {
            if (QuantumClient == null || QuantumClient.LocalPlayer == null)
                return false;

            if (QuantumClient.LocalPlayer.IsMasterClient)
                return true;
            else
                return false;
        }
    }

    TimeSpan server2clientTimeOffset = TimeSpan.Zero;


    private void ClearAll()
    {
        QuantumRunner.ShutdownAll(true);
        room_MaxPlayer = 0;
        Quantum_IsConnectedAndReady = false;
        Quantum_IsConnectedToMaster = false;
        Quantum_IsInRoom = false;
        Quantum_RoomData.Clear();
        playersRoomDataReceived.Clear();
        playersSceneLoaded.Clear();
        QuantumClient = null;
    }

    private void Update()
    {
        QuantumClient?.Service();
    }

    public Frame GetFrameVerified()
    {
        if (QuantumRunner.Default == null)
            return null;

        var game = QuantumRunner.Default.Game;
        if (game == null || game.Frames == null)
            return null;

        return game.Frames.Verified;
    }

    public Frame GetFramePredicted()
    {
        if (QuantumRunner.Default == null)
            return null;

        var game = QuantumRunner.Default.Game;
        if (game == null || game.Frames == null)
            return null;

        return game.Frames.Predicted;
    }

    public Frame GetFramePredictedPrevious()
    {
        if (QuantumRunner.Default == null)
            return null;

        var game = QuantumRunner.Default.Game;
        if (game == null || game.Frames == null)
            return null;

        return game.Frames.PredictedPrevious;
    }

    public QuantumGame GetQuantumGame()
    {
        if (QuantumRunner.Default == null)
            return null;

        return QuantumRunner.Default.Game;
    }

    public int GetQuantumGamePing()
    {
        var game = GetQuantumGame();
        if (game != null && game.Session != null && game.Session.Stats != null)
            return game.Session.Stats.Ping;
        else
            return -1;
    }

    #region Photon Server Connection Related Stuffs

    public void SelectPhotonServerRegion()
    {
        /// if BestRegion IsNullOrEmpty() AND UseNameServer == true, use BestRegion. else, use a server
        PhotonServerSettings.Instance.AppSettings.FixedRegion = string.Empty;
    }

    public void ConnectToPhotonServer()
    {
        var pid = AccountManager.Instance.PID;
        var appSettings = PhotonServerSettings.CloneAppSettings(PhotonServerSettings.Instance.AppSettings);

        if (QuantumClient == null)
        {
            QuantumClient = new QuantumLoadBalancingClient(PhotonServerSettings.Instance.AppSettings.Protocol);
            QuantumClient.ConnectionCallbackTargets.Add(this);
            QuantumClient.MatchMakingCallbackTargets.Add(this);
            QuantumClient.EventReceived += OnEvent;
            QuantumClient?.ConnectUsingSettings(appSettings, pid);
        }
    }

    public void DisconnectToPhotonServer()
    {
        if (QuantumClient != null)
        {
            if (QuantumClient.IsConnected)
            {
                QuantumClient.Disconnect();
            }
        }
    }

    public void OnConnected()
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnConnected</color>");
#endif

        Quantum_IsConnectedAndReady = true;
    }

    public void OnConnectedToMaster()
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnConnectedToMaster</color>");
#endif

        Quantum_IsConnectedToMaster = true;
    }

    public void OnDisconnected(DisconnectCause cause)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnDisconnected : " + cause + "</color>");
#endif

        //클라이언트에서 의도적으로 Disconnect 처리
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.OutGame);
            //AccountManager.Instance.Save_AccountData(); //이거는 쫌...
        }
        else //나머지 네트워크 문제...?
        {
            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.OutGame);
        }

        //꼭 위에서 ChangePhase 이후에 호출하자!!
        //그렇게 안하면 Scene 전환에 문제생김
        ClearAll();
    }

    #endregion


    #region Photon Room, Matchmacking Related Stuffs

    public void SetRoomMaxPlayer(int num)
    {
        if (num > 0)
            room_MaxPlayer = num;
    }

    public bool JoinRoom()
    {
        if (Quantum_IsConnectedAndReady == false)
        {
            Debug.Log("<color=red>" + "Error...! Quantum Connection is FALSE" + "</color>");
            return false;
        }

        if (Quantum_IsConnectedToMaster == false)
        {
            Debug.Log("<color=red>" + "Error...! Quantum Master Connection is FALSE" + "</color>");
            return false;
        }

        if (room_MaxPlayer <= 0)
        {
            Debug.Log("<color=red>" + "room_MaxPlayer is " + room_MaxPlayer + "</color>");
            return false;
        }

        Quantum_IsInRoom = false;
        Quantum_RoomData.Clear();
        playersRoomDataReceived.Clear();
        playersSceneLoaded.Clear();
        Quantum_IsMatchMakingFailed = false;

        //callbacks => OnJoinedRoom() or OnJoinRandomFailed()
        //QuantumClient?.OpJoinRandomRoom(new OpJoinRandomRoomParams { MatchingType = MatchmakingMode.FillRoom });
        QuantumClient?.OpJoinRandomOrCreateRoom(
            JoinRoomParams(),
            EnterRoomParams()
            );

        return true;
    }

    private void CreateRoom()
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.LogFormat("<color=cyan>Creating new room for '{0}' max players</color>", room_MaxPlayer);
#endif

        QuantumClient.OpCreateRoom(EnterRoomParams());
    }

    private OpJoinRandomRoomParams JoinRoomParams()
    {
        var joinRoomParam = new OpJoinRandomRoomParams();

        joinRoomParam.ExpectedCustomRoomProperties = RoomProperties();

        return joinRoomParam;
    }

    private EnterRoomParams EnterRoomParams()
    { 
        var enterRoomParam = new EnterRoomParams();
        enterRoomParam.RoomOptions = GetRoomOptions();

        return enterRoomParam;
    }

    public const string ROOM_PROPERTIES_PhotonQuantumVersion = "qv";
    public const string ROOM_PROPERTIES_RoomMaxPlayers = "rmp";
    public const string ROOM_PROPERTIES_InGamePlayMode = "ipm";
    public const string ROOM_PROPERTIES_MatchMakingGroup = "mmg";

    private RoomOptions GetRoomOptions()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = room_MaxPlayer,
            CustomRoomProperties = RoomProperties(),
            CustomRoomPropertiesForLobby = RoomPropertiesForLobby(),
            Plugins = new string[] { "QuantumPlugin" }
        };

        return roomOptions;
    }

    private Hashtable RoomProperties()
    {
        var roomProperties = new ExitGames.Client.Photon.Hashtable
        {
            //짧을 수록 성능이 좋다고 해서 일부로 짧게 설정함...
            { ROOM_PROPERTIES_PhotonQuantumVersion /*"PhotonQuantumVersion"*/, ReferenceManager.Instance.AppDefines.PhotonQuantumVersion },
            { ROOM_PROPERTIES_RoomMaxPlayers /*"RoomMaxPlayers"*/, room_MaxPlayer },
            { ROOM_PROPERTIES_InGamePlayMode /*"InGamePlayMode"*/, (int)CurrentPlayMode  },
            { ROOM_PROPERTIES_MatchMakingGroup /*"MatchMakingGroup"*/, AccountManager.Instance.GetMatchMakingGroup },
        };

        return roomProperties;
    }

    private string[] RoomPropertiesForLobby()
    {
        return new string[] 
        { 
            ROOM_PROPERTIES_PhotonQuantumVersion, 
            ROOM_PROPERTIES_RoomMaxPlayers, 
            ROOM_PROPERTIES_InGamePlayMode, 
            ROOM_PROPERTIES_MatchMakingGroup 
        };
    }

    //Photon Callback Afer JoiningRoom Or JoingRandomRoom
    public void OnJoinedRoom()
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnJoinedRoom</color>");
#endif
        Quantum_IsInRoom = true;

    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnJoinRandomFailed</color>");
#endif
        //참여할 방이 없으면 방을 만들자 
        CreateRoom();
    }

    public void OnCreatedRoom()
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnCreatedRoom</color>");
#endif

        Quantum_IsInRoom = true;
    }

    public void LeaveRoom()
    {
        QuantumClient?.OpLeaveRoom(false);
    }

    public void OnLeftRoom()
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnLeftRoom</color>");
#endif

        DisconnectToPhotonServer();
    }

    public void CloseRoom()
    {
        if (QuantumClient == null || QuantumClient.CurrentRoom == null)
            return;

        QuantumClient.CurrentRoom.IsOpen = false;
        QuantumClient.CurrentRoom.IsVisible = false;
    }

    public void RaiseEvent(PhotonEventCode eventCode, System.Object obj = null)
    {
        if (QuantumClient == null || QuantumClient.IsConnected == false)
        {
            Debug.LogError("<color=red>[Quantum] Failed to send event : " + eventCode.ToString() + "</color>");
            return;
        }

        var sendOptions = new SendOptions
        {
            //꼭 DeliveryMode.Reliable 해서 순서 보장을 해야함
            DeliveryMode = DeliveryMode.Reliable
        };
        sendOptions.Reliability = true;

        var result = QuantumClient.OpRaiseEvent((byte)eventCode, obj, new RaiseEventOptions { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All }, sendOptions);

        if (result == true)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("<color=magenta>[Quantum] Sending event : " + eventCode.ToString() + "</color>");
#endif
        }
        else
        {
            Debug.LogError("<color=red>[Quantum] Failed to send event : " + eventCode.ToString() + "</color>");
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch ((PhotonEventCode)photonEvent.Code)
        {
            case PhotonEventCode.JoinRoomFailed:
                {
                    LeaveRoom();
                }
                break;

            case PhotonEventCode.SendRoomData: //모든 Client한테 받도록 했음... ! 가장 먼저 오는 사람꺼 쓰자...
                {
                    var newRoomData = JsonUtility.FromJson<RoomData>((string)photonEvent.CustomData);

                    if (Quantum_RoomData.isRoomDataSet == false)
                    {
                        Quantum_RoomData = newRoomData;
                        Quantum_RoomData.isRoomDataSet = true;
                    }
#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=magenta>[Quantum] OnEvent  RoomDataReceived!!  map id>> " + Quantum_RoomData.mapID + "</color>");
#endif
                }
                break;

            case PhotonEventCode.RoomDataReady:
                {
                    var pid = (string)photonEvent.CustomData;
                    if (playersRoomDataReceived.Contains(pid) == false)
                        playersRoomDataReceived.Add(pid);

#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=magenta>[Quantum] OnEvent  RoomDataReady!!  pid>> " + pid + "</color>");
#endif
                }
                break;

            case PhotonEventCode.SceneLoadReady:
                {
                    var pid = (string)photonEvent.CustomData;
                    if (playersSceneLoaded.Contains(pid) == false)
                        playersSceneLoaded.Add(pid);

#if UNITY_EDITOR || SERVERTYPE_DEV
                    Debug.Log("<color=magenta>[Quantum] OnEvent  SceneLoadReady!!  pid>> " + pid + "</color>");
#endif
                }
                break;

            case PhotonEventCode.SceneLoadFailed:
                {
                    LeaveRoom();
                }
                break;
        }

    }

    #endregion


    public void SetServerTime(TimeSpan ts)
    {
        server2clientTimeOffset = ts;
    }

    public DateTime GetServerTime_UTC()
    {
        return DateTime.UtcNow.Add(server2clientTimeOffset);
    }

    public DateTime ServerTime_UTC
    {
        get { return DateTime.UtcNow.Add(server2clientTimeOffset); }
    }

    public String ServerTime_UTC_String
    {
        get { return ServerTime_UTC.ToString("yyyy-MM-dd HH:mm:ss"); }
    }

    //LocalPlayer의 TeamID구하기
    public int GetTeamID()
    {
        int teamID = -1;

        //teamid 는 팀1번-> 팀2번 -> 팀3번 채우고 팀1번-> 팀2번 -> 팀3번 계속 채우면서 지정 반봅 하는 형식
        if (Quantum_RoomData != null && QuantumClient != null)
        {
            if (Quantum_RoomData.isRoomDataSet)
            {
                var maxTeamCount = room_MaxPlayer / CommonDefine.DEFAULT_PLAYER_NUM_EACH_TEAM; //생길수 있는 팀 숫자
                teamID = 0;
                for (int i = 0; i < Quantum_RoomData.randomShuffledListOfPlayerForTeamMatch.Count; i++)
                {
                    if (Quantum_RoomData.randomShuffledListOfPlayerForTeamMatch[i] == QuantumClient.LocalPlayer)
                    {
                        break;
                    }

                    ++teamID;
                    if (teamID >= maxTeamCount)
                    {
                        teamID = 0;
                    }
                }
            }
        }

        return teamID;
    }


    #region Etc Photon Quantum Callbacks

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        string msg = string.Empty;
        foreach (var i in regionHandler.EnabledRegions)
            msg += i.Code + ", ";
        Debug.Log("<color=magenta>[Quantum] OnRegionListReceived >>> count: " + regionHandler.EnabledRegions.Count + "  |  " + msg + "</color>");
#endif
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnCustomAuthenticationResponse</color>");
#endif
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnCustomAuthenticationFailed</color>");
#endif
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnFriendListUpdate >>> count: " + friendList.Count + "</color>");
#endif
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnCreateRoomFailed</color>");
#endif
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=magenta>[Quantum] OnJoinRoomFailed</color>");
#endif
    }

    #endregion


    private DateTime PauseDataTime;
    void OnApplicationPause(bool paused)
    {
#if UNITY_EDITOR
        return;
#endif

        //앱 나갔을때
        if (paused)
        {
            PauseDataTime = GetServerTime_UTC();
        }
        else //앱 들어왔을 때
        {
            var ts = GetServerTime_UTC() - PauseDataTime;

            if (ts.TotalSeconds > 10 
                || QuantumClient == null
                || (QuantumClient != null && QuantumClient.IsConnected == false))
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Initialize);
        }
    }
}
