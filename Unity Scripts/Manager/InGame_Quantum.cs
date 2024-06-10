using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using ExitGames.Client.Photon;
using Photon.Realtime;


public class InGame_Quantum : QuantumCallbacks
{
    public static InGame_Quantum Instance = null;
    //참고! ingame outgame 씬이 전환될떄 이 오브젝트 파괴됨!!

    public static QuantumGame QuantumGame { get; set; }

    public class PlayerInfo
    {
        public EntityRef entityRef;
        public EntityPrototype entityPrototype;
        public GameObject enityGameObj;
        public string pid;
        public int teamID;
        public Unity_PlayerCharacter playerChar;
        public bool isAI;

        public Quantum.InGamePlayMode inGamePlayMode;
        public int rank_solo = -1;
        public int rank_team = -1;

        //Data
        public string nickname;
        public int rankingPoint;
    }

    [SerializeField] public List<PlayerInfo> listOfPlayers = new List<PlayerInfo>(); //memo : Sort 하면 안됨!!

    public PlayerInfo myPlayer = null;

    public class BallInfo
    {
        public EntityRef enityRef;
        public GameObject enityGameObj;
        public Unity_Ball ball;
    }

    public BallInfo ball = null;

    public unsafe GameManager? GameManager
    {
        get
        {
            if (NetworkManager_Client.Instance == null)
                return null;

            var frame = NetworkManager_Client.Instance.GetFramePredicted();
            if (frame == null)
                return null;
            else
            {
                if (frame.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
                    return *gameManager;
            }

            return null;
        }
    }

    public unsafe GamePlayState? GamePlayState
    {
        get
        {
            if (NetworkManager_Client.Instance == null)
                return null;

            var frame = NetworkManager_Client.Instance.GetFrameVerified();

            if (frame != null)
                return frame.Global->gamePlayState;
            else
                return null;
        }
    }

    public PlayerInfo GetPlayerInfo(EntityRef entity)
    {
        if (listOfPlayers != null)
        {
            var player = listOfPlayers.Find(x => x.entityRef.Equals(entity));
            if (player != null)
                return player;
        }

        return null;
    }

    public Action<EventGamePlayStateChanged> OnEventGamePlayStateChangedAction = null;
    public Action<EventPlayerSpawned> OnEventPlayerSpawnedAction = null;
    public Action<EventBallSpawned> OnEventBallSpawnedAction = null;
    public Action<EventGameResult> OnEventGameResultAction = null;
    public Action<EventUpdatePlayerRank> OnEventUpdatePlayerRankAction = null;

    private bool isRankingDataSaved = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        QuantumEvent.Subscribe<EventGamePlayStateChanged>(this, OnEventGamePlayStateChanged);
        QuantumEvent.Subscribe<EventPlayerSpawned>(this, OnEventPlayerSpawned);
        QuantumEvent.Subscribe<EventBallSpawned>(this, OnEventBallSpawned);
        QuantumEvent.Subscribe<EventGameResult>(this, OnEventGameResult); 
        QuantumEvent.Subscribe<EventUpdatePlayerRank>(this, OnEventUpdatePlayerRank); 
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        QuantumEvent.UnsubscribeListener<EventGamePlayStateChanged>(this);
        QuantumEvent.UnsubscribeListener<EventPlayerSpawned>(this);
        QuantumEvent.UnsubscribeListener<EventBallSpawned>(this);
        QuantumEvent.UnsubscribeListener<EventGameResult>(this);
        QuantumEvent.UnsubscribeListener<EventUpdatePlayerRank>(this);
    }

    public void Clear()
    {
        listOfPlayers.Clear();
        myPlayer = null;
        ball = null;
        QuantumGame = null;

        isRankingDataSaved = false;
    }

    public void StartGame_Quantum()
    {
        var networkManager = NetworkManager_Client.Instance;
        var pid = AccountManager.Instance.PID;

        GamePlaySettingsAsset gamePlay = Resources.Load("DB/Configs/GamePlaySettings") as GamePlaySettingsAsset;
        InGameDataSettingsAsset ingameData = Resources.Load("DB/Configs/InGameDataSettings") as InGameDataSettingsAsset;
        if (gamePlay != null && ingameData != null)
        {
            
            if (NetworkManager_Client.QuantumClient != null && NetworkManager_Client.QuantumClient.CurrentRoom != null)
            {
                if (NetworkManager_Client.QuantumClient.CurrentRoom.CustomProperties.TryGetValue(NetworkManager_Client.ROOM_PROPERTIES_InGamePlayMode, out var mode))
                {
                    gamePlay.Settings.InGamePlayMode = (int)mode;
#if SERVERTYPE_DEV || UNITY_EDITOR
                    Debug.Log("<color=white>InGamePlayMode >> " + (Quantum.InGamePlayMode)((int)mode) + "</color>");
#endif
                }

                if (NetworkManager_Client.QuantumClient.CurrentRoom.CustomProperties.TryGetValue(NetworkManager_Client.ROOM_PROPERTIES_MatchMakingGroup, out var group))
                {
                    gamePlay.Settings.MatchMakingGroup = (short)group;
#if SERVERTYPE_DEV || UNITY_EDITOR
                    Debug.Log("<color=white>MatchMakingGroup >> " + (CommonDefine.MatchMakingGroup)((short)group) + "</color>");
#endif
                }

                if ((Quantum.InGamePlayMode)gamePlay.Settings.InGamePlayMode == Quantum.InGamePlayMode.SoloMode
                    || (Quantum.InGamePlayMode)gamePlay.Settings.InGamePlayMode == Quantum.InGamePlayMode.TeamMode)
                {
                    gamePlay.Settings.Total_PlayerNumber = networkManager.room_MaxPlayer;
                    gamePlay.Settings.Real_PlayerNumber = NetworkManager_Client.QuantumClient.CurrentRoom.PlayerCount;
                    gamePlay.Settings.AI_PlayerNumber = networkManager.room_MaxPlayer - NetworkManager_Client.QuantumClient.CurrentRoom.PlayerCount;
                }
                else if ((Quantum.InGamePlayMode)gamePlay.Settings.InGamePlayMode == Quantum.InGamePlayMode.PracticeMode)
                {
                    gamePlay.Settings.Total_PlayerNumber = NetworkManager_Client.QuantumClient.CurrentRoom.PlayerCount + 1; //연습모드 총 2명
                    gamePlay.Settings.Real_PlayerNumber = NetworkManager_Client.QuantumClient.CurrentRoom.PlayerCount; //플레이 나 혼자
                    gamePlay.Settings.AI_PlayerNumber = 1; //AI 한명
                }

                gamePlay.Settings.PlayerNumberEachTeam = CommonDefine.DEFAULT_PLAYER_NUM_EACH_TEAM;
                gamePlay.Settings.RandomSeed = NetworkManager_Client.Instance.Quantum_RoomData.randomSeed;

                ingameData.Settings.playerDefaultData = ReferenceManager.Instance.PlayerDefaultStat_Quantum;
                ingameData.Settings.ballDefaultData = ReferenceManager.Instance.BallDefaultStat_Quantum;
                ingameData.Settings.additionalReferenceData = ReferenceManager.Instance.AdditionalReferenceData_Quantum;

                ingameData.Settings.aiDefaultData = ReferenceManager.Instance.AIDefaultData_Quantum((CommonDefine.MatchMakingGroup)gamePlay.Settings.MatchMakingGroup);
                ingameData.Settings.AI_AvailableCharacterSkinIDList = ReferenceManager.Instance.AIData.AvailableCharacterSkinIDList;
                ingameData.Settings.AI_AvailableWeaponSkinIDList = ReferenceManager.Instance.AIData.AvailableWeaponSkinIDList;
                ingameData.Settings.AI_AvailablePassiveSkillList = ReferenceManager.Instance.AIData.AvailablePassiveSkillList;
                ingameData.Settings.AI_AvailableActiveSkillList = ReferenceManager.Instance.AIData.AvailableActiveSkillList;

#if SERVERTYPE_DEV || UNITY_EDITOR
                Debug.Log("<color=cyan>GamePlaySettingsAsset Asset Set! Real Player: " + gamePlay.Settings.Real_PlayerNumber + " | AI Player: " + gamePlay.Settings.AI_PlayerNumber + " </color>");
#endif
            }
        }

        var mapAssetName = NetworkManager_Client.Instance.Quantum_RoomData.GetQuantumMapAssetName();
        var mapDataAssetName = NetworkManager_Client.Instance.Quantum_RoomData.GetQuantumCustomMapDataAssetName();
        if (string.IsNullOrEmpty(mapAssetName) || string.IsNullOrEmpty(mapDataAssetName))
        {
            Debug.Log("<color=red>ERROR...! no map asset existing >>> " + mapAssetName + "</color>");
            NetworkManager_Client.Instance.LeaveRoom();
            return;
        }

        MapAsset mapAsset = Resources.Load("DB/Configs/" + mapAssetName) as MapAsset;
        CustomMapDataSettingsAsset mapDataAsset = Resources.Load("DB/Configs/" + mapDataAssetName) as CustomMapDataSettingsAsset;

        if (networkManager.runtimeConfig != null)
        {
            networkManager.runtimeConfig.Seed = NetworkManager_Client.Instance.Quantum_RoomData.randomSeed;
            networkManager.runtimeConfig.Map = mapAsset.Settings;
            networkManager.runtimeConfig.CustomMapDataSettingsRef = mapDataAsset.Settings;
        }


        var param = new QuantumRunner.StartParameters
        {
            RuntimeConfig = networkManager.runtimeConfig,
            DeterministicConfig = DeterministicSessionConfigAsset.Instance.Config,
            ReplayProvider = null,

            GameMode = Photon.Deterministic.DeterministicGameMode.Multiplayer,

            FrameData = null,
            InitialFrame = 0,
            PlayerCount = NetworkManager_Client.QuantumClient.CurrentRoom.PlayerCount,
            LocalPlayerCount = NetworkManager_Client.LOCAL_PLAYER,
            RecordingFlags = RecordingFlags.None,
            NetworkClient = NetworkManager_Client.QuantumClient,
            StartGameTimeoutInSeconds = 10f,
            RunnerId = name
        };

        QuantumRunner.StartGame(pid, param);
    }


    public override void OnGameStart(QuantumGame game)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("OnGameStart!! >>> " + game.GetLocalPlayers().Length);
#endif

        QuantumGame = game;

        var runtimePlayer = NetworkManager_Client.Instance.runtimePlayer;

        var accountData = AccountManager.Instance.AccountData;

        if (accountData != null)
        {
            runtimePlayer.PID = accountData.PID;
            runtimePlayer.Nickname = accountData.nickname;
            runtimePlayer.TeamID = NetworkManager_Client.Instance.GetTeamID();
            runtimePlayer.RankingPoint = accountData.RankingPoint;
            
            runtimePlayer.Character_SkinID = accountData.characterSkinID;
            runtimePlayer.Weapon_SkinID = accountData.weaponSkinID;
            runtimePlayer.Passive_SkillID = accountData.passiveSkillID;
            runtimePlayer.Active_SkillID = accountData.activeSkillID;

            //일단 그냥 Default값 세팅 해주자
            runtimePlayer.MaxHealthPoint = ReferenceManager.Instance.GetMaxHealthPoint();
            runtimePlayer.AttackDamage = ReferenceManager.Instance.GetAttackDamage();
            runtimePlayer.AttackRange = ReferenceManager.Instance.GetAttackRange();
            runtimePlayer.AttackDuration = ReferenceManager.Instance.GetAttackDuration();
            runtimePlayer.MaxSpeed = ReferenceManager.Instance.GetMaxSpeed();

            runtimePlayer.Input_AttackCooltime = ReferenceManager.Instance.GetInputAttackCootime();
            runtimePlayer.Input_JumpCooltime = ReferenceManager.Instance.GetInputJumpCootime();
            runtimePlayer.Input_SkillCooltime = ReferenceManager.Instance.GetInputSkillCootime();
        }

        //Player Data 보내면 퀀텀 코드 내부적으로 player를 create 생성해주고 있음 + AI도 같이...
        foreach (var localPlayer in game.GetLocalPlayers())
        {
            game.SendPlayerData(localPlayer, NetworkManager_Client.Instance.runtimePlayer);
        }
    }

    private unsafe void OnEventGamePlayStateChanged(EventGamePlayStateChanged _event)
    {
        switch (_event.StateType)
        {
            case Quantum.GamePlayState.CountDown:
                {
                    PrefabManager.Instance.UI_SceneTransition.Show_Out();
                }
                break;

            case Quantum.GamePlayState.Play:
                {
                    //test???
                    if (QuantumGame != null)
                    {
                        var cmd2 = new CommandShareData();
                        InGameDataSettingsAsset ingameData = Resources.Load("DB/Configs/InGameDataSettings") as InGameDataSettingsAsset;
                        cmd2.ingameDataSettings = ingameData.Settings;
                        QuantumGame.SendCommand(cmd2);
                    }

                    var ui = PrefabManager.Instance.UI_InGame;
                    ui.ActivateIngameMessage_Localized("INGAME_GAMESTART");
                    SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_Start);

                    if (NetworkManager_Client.Instance.CurrentPlayMode == InGamePlayMode.SoloMode 
                        || NetworkManager_Client.Instance.CurrentPlayMode == InGamePlayMode.TeamMode)
                        AccountManager.Instance.SaveMyData_FirebaseServer_IngamePlay();
                }
                break;

            case Quantum.GamePlayState.End:
                {
                    UtilityInvoker.Invoke(this, () =>{
                        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameResult);
                    }, 3f); //3초 딜레이 주고 Phase 바꾸자

                    var ui = PrefabManager.Instance.UI_InGame;
                    ui.ActivateIngameMessage_Localized("INGAME_GAMEOVER");
                }
                break;
        }


        OnEventGamePlayStateChangedAction?.Invoke(_event);
    }

    private unsafe void OnEventPlayerSpawned(EventPlayerSpawned _event)
    {
        if (listOfPlayers.Find(x=>x.Equals(_event.Entity)) == null)
        {
            var entityView = EntityViewUpdater.Instance.GetView(_event.Entity);
            if (entityView != null)
            {
                var prefab = PrefabManager.Instance.Unity_PlayerCharacter;
                GameObject playerObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
                Unity_PlayerCharacter playerScript = playerObj.GetComponent<Unity_PlayerCharacter>();
                playerScript.SetData_Ingame(_event.Entity, _event.PID, _event.characterSkinID, _event.weaponSkinID);
                playerObj.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);

                playerObj.transform.SetParent(entityView.gameObject.transform);
                playerObj.transform.localPosition = Vector3.zero;
                playerObj.transform.localRotation = Quaternion.identity;

                var info = new PlayerInfo();
                info.entityRef = _event.Entity;
                info.enityGameObj = entityView.gameObject;
                info.pid = _event.PID;
                info.nickname = _event.nickname;
                info.teamID = _event.teamID;
                info.playerChar = playerScript;
                info.isAI = _event.isAI;
                info.rankingPoint = _event.rankingPoint;

                listOfPlayers.Add(info);

                if (_event.PID == AccountManager.Instance.PID)
                {
                    myPlayer = info;
                }
            }
        }

        OnEventPlayerSpawnedAction?.Invoke(_event);
    }

    private unsafe void OnEventBallSpawned(EventBallSpawned _event)
    {
        var entityView = EntityViewUpdater.Instance.GetView(_event.Entity);
        if (entityView != null)
        {
            var prefab = PrefabManager.Instance.Unity_Ball;
            GameObject ballObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
            Unity_Ball ballScript = ballObj.GetComponent<Unity_Ball>();
            ballScript.SetData(_event.Entity);

            ballObj.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Ball);
            ballObj.transform.SetParent(entityView.gameObject.transform);
            ballObj.transform.localPosition = Vector3.zero;

            var info = new BallInfo();
            info.enityRef = _event.Entity;
            info.enityGameObj = entityView.gameObject;
            info.ball = ballScript;

            ball = info;
        }

        OnEventBallSpawnedAction?.Invoke(_event);
    }

    private unsafe void OnEventGameResult(EventGameResult _event)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=white>[Quantum] OnEventGameResult</color>");
#endif

        var rank_array = _event.PlayerRankInfoArray;
        for (int i = 0; i < rank_array.PlayerRankInfoArr.Length; i++)
        {
            if (i >= InGame_Quantum.Instance.listOfPlayers.Count)
                continue; //설마 count 초과해서 data 세팅이 되어있을리가....

            var player = listOfPlayers.Find(x => x.entityRef.Equals(rank_array.PlayerRankInfoArr[i].player));
            if (player != null)
            {
                player.inGamePlayMode = _event.PlayMode;

                //최종 랭킹 세팅...
                player.rank_solo = rank_array.PlayerRankInfoArr[i].rank_solo;
                player.rank_team = rank_array.PlayerRankInfoArr[i].rank_team;
            }
        }

        var statistics_array = _event.InGamePlayStatisticsArray;

        var networkClient = NetworkManager_Client.Instance;
        AccountManager.Instance.SetMatchData(networkClient.Quantum_RoomData, statistics_array);

        OnEventGameResultAction?.Invoke(_event);
    }

    private unsafe void OnEventUpdatePlayerRank(EventUpdatePlayerRank _event)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        Debug.Log("<color=white>[Quantum] OnEventUpdatePlayerRank  _event.id:" + _event.Id + "</color>");
#endif

        var frame = NetworkManager_Client.Instance.GetFramePredicted();

        var array = _event.PlayerRankInfoArray;
        for (int i = 0; i < array.PlayerRankInfoArr.Length; i++)
        {
            if (i >= InGame_Quantum.Instance.listOfPlayers.Count)
                continue; //설마 count 초과해서 data 세팅이 되어있을리가....

            var player = listOfPlayers.Find(x => x.entityRef.Equals(array.PlayerRankInfoArr[i].player));
            if (player != null)
            {
                player.inGamePlayMode = _event.PlayMode;

                //Team Mode는 rank_solo, rank_team 둘다 보내줌 (참고로 rank값이 0인 경우 갱신하면 안됨, team solo rank 따로따로 보내주고 있음)
                //Solo Mode는 rank_solo만 보내줌
                //중간 중간에 랭킹 세팅... (갱신 되는 경우에만 데이터 override 하자)
                if (array.PlayerRankInfoArr[i].rank_solo > 0)
                    player.rank_solo = array.PlayerRankInfoArr[i].rank_solo;
                if (array.PlayerRankInfoArr[i].rank_team > 0)
                    player.rank_team = array.PlayerRankInfoArr[i].rank_team;
            }
        }

        if (listOfPlayers != null && frame != null)
        {
            //내 랭크가 결정 되었을때 데이터 세팅 해주자
            if (NetworkManager_Client.Instance.CurrentPlayMode == InGamePlayMode.SoloMode)
            {
                if (myPlayer.rank_solo > 0 && isRankingDataSaved == false)
                {
                    RankingManager.Instance.SetAndSaveMyRankingData_Ingame();

                    isRankingDataSaved = true;
                }
            }
            else if (NetworkManager_Client.Instance.CurrentPlayMode == InGamePlayMode.TeamMode)
            {
                if (myPlayer.rank_team > 0 && isRankingDataSaved == false)
                {
                    RankingManager.Instance.SetAndSaveMyRankingData_Ingame();
                    isRankingDataSaved = true;
                }
            }
        }


        OnEventUpdatePlayerRankAction?.Invoke(_event);
    }
}
