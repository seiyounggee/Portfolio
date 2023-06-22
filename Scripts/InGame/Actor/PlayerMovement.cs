using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using PNIX.Engine.NetClient;
using static PlayerMovement;
using UnityEngine.Rendering;

public partial class PlayerMovement : PlayerBase, INetworkRunnerCallbacks, IObserver<PlayerMovement.ObserverKey, PlayerMovement.ObserverData_input>
{
    public InGameManager inGameManager => InGameManager.Instance;
    public NetworkInGameRPCManager networkInGameRPCManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;
    public NetworkRunner networkRunner => PhotonNetworkManager.Instance.MyNetworkRunner;

    [SerializeField] public Rigidbody rgbdy = null;
    [SerializeField] public PlayerCollisionChecker collisionChecker_Player = null; //플레이어간 충돌 체크
    [SerializeField] public PlayerCollisionChecker collisionChecker_Ground = null; //바닥 충돌 체크
    [SerializeField] public PlayerTriggerChecker triggerChecker_Front = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Back = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Body = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Right = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Left = null;

    [SerializeField] public LineRenderer lr_inner = null;
    [SerializeField] public LineRenderer lr_outer = null;

    [ReadOnly] public WayPointSystem.WaypointsGroup wg = null;

    [ReadOnly] public Vector3? startPosition = null;
    [ReadOnly] public Quaternion? startRotation = null;
    [ReadOnly] public Vector3? startDest = null;

    [ReadOnly] public Vector3? introPosition = null;
    [ReadOnly] public Quaternion? introRotation = null;
    [ReadOnly] public Vector3? introDest = null;

    [ReadOnly] public Vector3 currDirection = Vector3.zero;
    [ReadOnly] public Vector3 upwardDirection = Vector3.up;
    [ReadOnly] public Vector3? currentDest = null;
    [ReadOnly] public int currentRank = 0;
    [ReadOnly] public float previousBatteryAtBooster = 0; //부스터 작용 진전 battery 값

    [Networked] public bool isSpawned { get; set; } = false;
    [Networked] public Vector3 network_position { get; set; } = Vector3.zero;
    [Networked] public int network_currentMoveIndex { get; set; } = 0;
    [Networked] public float network_currentMoveSpeed { get; set; } = 0;
    [Networked] public float network_currentRotationSpeed { get; set; } = 0;
    [Networked] public int network_currentLap { get; set; } = 0;
    [Networked] public float network_currentBattery { get; set; } = 0;
    [Networked] public LaneType network_currentLaneType { get; set; } = LaneType.None;
    [Networked] public LaneType network_previousLaneType { get; set; } = LaneType.None;
    [Networked] public bool network_isChangingLane { get; set; } = false;
    [Networked] public bool network_isMoving { get; set; } = false;
    [Networked] public bool network_isEnteredTheFinishLine { get; set; } = false;
    [Networked] public float network_shieldCooltimeLeftTime { get; set; } = 0;
    [Networked] public bool network_IsLagging { get; set; } = false; //Client & Network Postion 간 lag 여부
    [Networked] public int network_Rank { get; set; } = 1; //그냥 각각 클라이언트에서 주장하는 Rank에 해당함. 네트워크 상태에 따라 같을 수 있음! Ingame의 current rank가 겹치지 않음! 
    [Networked] public long network_PID { get; set; } = -1;

    public int client_currentMoveIndex { get; set; } = 0;
    public float client_currentMoveSpeed { get; set; } = 0;
    public float client_currentFowardRotationSpeed { get; set; } = 0;
    public float client_currentUpwardRotationSpeed { get; set; } = 0;
    public LaneType client_currentLaneType { get; set; } = LaneType.None;
    public LaneType client_previousLaneType { get; set; } = LaneType.None;
    public CarBoosterType client_currentCarBoosterLv { get; set; } = CarBoosterType.None;
    public ChargePadBoosterLevel client_currentChargeBoosterLv { get; set; } = ChargePadBoosterLevel.None;

    public bool isShield { get; set; } = false;
    public bool isShieldCooltime { get; set; } = false;
    public bool isStunned { get; set; } = false;
    public bool isFlipped { get; set; } = false;
    public bool isOutOfBoundary { get; set; } = false;
    public bool isFlippingUp { get; set; } = false;
    public bool isGrounded { get { if (currentTrackType == TrackType.Normal) return true; else return false; } set { } }
    public bool isDrafting { get; set; } = false;
    public bool isFlying { get; set; } = false;
    public bool isMoveLine { get; set; } = false; //network_isChangingLane과 다름...! network_isChangingLane는 change lane 전체 기간동안... isMoveLine는  ref에서 정의한 일정 시간동안...!

    public bool isBoosting { get { if (client_currentChargeBoosterLv != ChargePadBoosterLevel.None || client_currentCarBoosterLv != CarBoosterType.None) return true; else return false; } set { } }
    public bool isCarBoosting { get { if (client_currentCarBoosterLv != CarBoosterType.None) return true; else return false; } set { } }
    public bool isCarAttackBoosting { get { if (client_currentCarBoosterLv == CarBoosterType.CarBooster_LevelTwo || client_currentCarBoosterLv == CarBoosterType.CarBooster_LevelThree || client_currentCarBoosterLv == CarBoosterType.CarBooster_LevelFour_Timing) return true; else return false; } set { } }
    public bool isChargePadBoosting { get { if (client_currentChargeBoosterLv != ChargePadBoosterLevel.None) return true; else return false; } set { } }
    public bool isFullBattery { get { if (network_currentBattery >= ref_fullBusterSpeedBuffNeedBusterGauge) return true; else return false; } set { } }

    public bool isSetToPosition { get; set; } = false;

    public bool isStartingBoosterActiavated { get; set; } = false;

    public bool IsStopInputIfSheild() //temp
    {
        int isOn = DataManager.Instance.GetGameConfig<int>("moveRightLeftWhenShield");
        if (isShield)
        { 
            if(isOn == 1)
                return false;
            else
                return true;
        }
        else
            return false;
    }

    public bool isIntroMoving { get; set; } = false;

    [ReadOnly] public Vector3 previousReachDest = Vector3.zero;
    [ReadOnly] public Vector3 previousDir = Vector3.zero;

    public enum MoveInput { None, Left, Right, Boost, Shield }
    public enum LaneType { None = -1, Zero = 0, One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6 }
    public enum CarBoosterType { None = 0, CarBooster_Starting, CarBooster_LevelOne, CarBooster_LevelTwo, CarBooster_LevelThree , CarBooster_LevelFour_Timing }
    public enum ChargePadBoosterLevel { None, ChargePadBooster_One, ChargePadBooster_Two, ChargePadBooster_Three }
    public enum AnimationState { Idle, Drive, Booster_1, Booster_2, Brake, MoveLeft, MoveRight, Flip, Victory, Complete, Retire, Spin }
    [ReadOnly] public AnimationState currentCarAnimationState = AnimationState.Idle;
    [ReadOnly] public AnimationState currentCharAnimationState = AnimationState.Idle;

    public enum TrackType { None, Normal, OutOfBound }
    [ReadOnly] public TrackType currentTrackType = TrackType.Normal;
    private float timeCounterAfterOutOfBoundary = 0f;

    [ReadOnly] public int carID = -1;
    [ReadOnly] public int characterID = -1;

    [ReadOnly] public bool IsPlayerInFront = false; //플레이어 앞에 다른 플레이어가 있는 경우
    [ReadOnly] public NetworkId playerInFrontViewID;

#if UNITY_EDITOR
    public bool IsLagTestDebugOn = true; //인스팩터상에서 체크해주자
#endif
    private float networkClientLaneDiffTimeCounter = 0f; //network <-> client lane type 맞춰주는 용도로 사용

    public bool IsMine { get { return Object != null && (Object.HasStateAuthority || Object.HasInputAuthority); } }
    public bool IsAI { get; set; }
    public bool IsAutoMovement { get; set; }
    public bool IsMineAndNotAI { get { return IsMine && !IsAI; } }
    public bool IsMineAndAI { get { return IsMine && IsAI; } }

    public float playerDirectionLerpMinSpeed = 4; //얼마나 부드럽게 회전을 할지 (낮을수록 부드럽지만 정확도 떨어짐)
    public float playerDirectionLerpMaxSpeed = 10; //얼마나 부드럽게 회전을 할지 (낮을수록 부드럽지만 정확도 떨어짐)
    public float playerDirectionLerpAddiionalAngleMinSpeed = 10; //얼마나 부드럽게 회전을 할지 (낮을수록 부드럽지만 정확도 떨어짐)
    public float playerDirectionLerpAddiionalAngleMaxSpeed = 60; //얼마나 부드럽게 회전을 할지 (낮을수록 부드럽지만 정확도 떨어짐)

    private double lagTime = 0f;
    private float lagAdjustRate = 0.06f;
    private float lagPositionAdjustRate = 1f;
    private float lagMaxDistance = 2f; //Lag Max
    private float lagMinDistance = 0.4f; //Lag로 판단하는 dist
    private float lagMaxRate = 0.2f; //Lag를 어느정도 수치로 조절할지 ... 이때 MAX 값
    private float lagMinRate = 0.02f; //Lag를 어느정도 수치로 조절할지 ... 이때 MIN 값
    private float lagTeleportTime = 1.5f; //일정 시간 이상 lag 할 경우 teleport 시켜버리기
    private float lagTeleportDist = 5f; //일정 거리 이상 lag 할 경우 teleport 시켜버리기
    private float lagPingAdjustmentRate = 1.4f;
    private float lagPositionAdjustmentRate_Min = 0.65f;
    private float lagPositionAdjustmentRate_Max = 4.0f;
    private float lagPositionAdjustmentRate_Mutiplier = 2.4f;
    private bool lagUseAverageLagDist = false;
    private bool lagUseTeleportLagDist = false;

    private bool rigidBodyUseGravity = false;
    private float rigidBodyMass = 10;
    private float rigidBodyDrag = 1;
    private float rigidBodyAngularDrag = 0;

    private bool useIsOutOfBoundary = true; //isOutOfBoundary 기능 사용여부
    private float outOfBoundaryTimer = 0f;
    private float outOfBoundaryMoveSpeed = 0f;

    private float flipUpSpeed = 18; //공격 받았을 시 높이 상승 속도
    private float flipDownSpeed = 19f; //공격 받았을 시 상승 후 하강 속도

    private float draftingActivationTimer = 0f;
    private float draftingCoolTimer = 0f;
    private bool checkForLapCompletion = false;

    private float autoBatteryChargeCounter = 0f;

    private bool isTimingBoosterReady = false;
    public bool IsTimingBoosterReady { get { return isTimingBoosterReady; } }
    private bool isTimingBoosterActive = false;
    private float timingBoosterOuterCircle_CurrRadius = 0f;

    private float startingBoosterInputTimeCounter = 0f;
    private bool isStartingBoosterInputBlocked = false;

    #region DB Stats

    [HideInInspector] public float ref_maxSpeedLimit = 100f;
    [HideInInspector] public float ref_defaultMaxSpeed = 25f;
    [HideInInspector] public float ref_defaultIncreaseSpeed = 18f;
    [HideInInspector] public float ref_defaultDecreaseSpeed = -17f;

    [HideInInspector] public float ref_playerRotationFowardSpeed = 15f;
    [HideInInspector] public float ref_playerRotationUpwardSpeed = 10f;
    
    [HideInInspector] public float ref_playerMoveIndexCheckMinRange = 15f;
    [HideInInspector] public int ref_playerMoveIndexCheckNumber = 5;
    
    [HideInInspector] public float ref_batteryMax = 100;
    [HideInInspector] public float ref_batteryAutoChargeSpeed = 0.9f;

    [HideInInspector] public float ref_batteryAttackSuccess = 20; //공격성공시 충전 바태리
    [HideInInspector] public float ref_batteryDefenseSuccess = 15; //공격성공시 충전 바태리
    [HideInInspector] public float ref_batteryStartAmount = 30; //시작 바태리양

    [HideInInspector] public float ref_boosterLv1Cost = 20;
    [HideInInspector] public float ref_boosterLv2Cost = 33;
    [HideInInspector] public float ref_boosterLv3Cost = 50;

    
    [HideInInspector] public float ref_flipUpTime = 1.5f; //공격 받았을 시 높이 상승 시간
    [HideInInspector] public float ref_flipGroundDelayTime = 0.5f; //지면 도착 후 딜레이

    [HideInInspector] public float ref_playerOutOfBoundaryTime = 5f; //isOutOfBoundary, 구역 벗어난 후 x초 후 리스폰 시간

    [HideInInspector] public float ref_playerValidReachDest = 1.5f; //목표 dest까지 도달도 판단하는 거리

    [HideInInspector] public int ref_playerShowMoveSpeedMultiplier = 1; //화면 속도계에 표시되는 속도 곱하기계수
    
    [HideInInspector] public float ref_draftActivateNeedTime = 0.5f; //드래프트 발동을 위해 유지 시간
    [HideInInspector] public float ref_draftActivationCooltime = 1f; //드래프트 발동 쿨타임 (발동 이후 다음 발동까지 최소의 시간)
    [HideInInspector] public float ref_draftActivationDist = 10f; //드래프트 발동 거리
    [HideInInspector] public float ref_draftRemainTime = 1.5f;
    [HideInInspector] public float ref_draftMaxSpeed = 1.5f;
    [HideInInspector] public float ref_draftIncreaseSpeed = 3f;

    [HideInInspector] public float ref_increaseSpeedRange1stReferenceValue = 0.5f;
    [HideInInspector] public float ref_increaseSpeedRange2ndReferenceValue = 0.6f;
    [HideInInspector] public float ref_increaseSpeedRange3rdReferenceValue = 0.7f;
    [HideInInspector] public float ref_increaseSpeedRange4thReferenceValue = 0.8f;
    [HideInInspector] public float ref_increaseSpeedRange5thReferenceValue = 0.9f;

    [HideInInspector] public float ref_increaseSpeedRange1stDepreciationValue = 3f;
    [HideInInspector] public float ref_increaseSpeedRange2ndDepreciationValue = 2f;
    [HideInInspector] public float ref_increaseSpeedRange3rdDepreciationValue = 1.5f;
    [HideInInspector] public float ref_increaseSpeedRange4thDepreciationValue = 0.8f;
    [HideInInspector] public float ref_increaseSpeedRange5thDepreciationValue = 0.5f;
    [HideInInspector] public float ref_increaseSpeedRangeEndDepreciationValue = 0.1f;

    [HideInInspector] public float ref_decreaseSpeedRange1stReferenceValue = 0.5f;
    [HideInInspector] public float ref_decreaseSpeedRange2ndReferenceValue = 0.4f;
    [HideInInspector] public float ref_decreaseSpeedRange3rdReferenceValue = 0.3f;
    [HideInInspector] public float ref_decreaseSpeedRange4thReferenceValue = 0.2f;
    [HideInInspector] public float ref_decreaseSpeedRange5thReferenceValue = 0.1f;

    [HideInInspector] public float ref_decreaseSpeedRange1stDepreciationValue = 2f;
    [HideInInspector] public float ref_decreaseSpeedRange2ndDepreciationValue = 1.5f;
    [HideInInspector] public float ref_decreaseSpeedRange3rdDepreciationValue = 1.2f;
    [HideInInspector] public float ref_decreaseSpeedRange4thDepreciationValue = 1.8f;
    [HideInInspector] public float ref_decreaseSpeedRange5thDepreciationValue = 1.0f;
    [HideInInspector] public float ref_decreaseSpeedRangeEndDepreciationValue = 0.1f;

    [HideInInspector] public float ref_boosterStartingIncreaseSpeed = 13f;
    [HideInInspector] public float ref_boosterLv1IncreaseSpeed = 13f;
    [HideInInspector] public float ref_boosterLv2IncreaseSpeed = 16f;
    [HideInInspector] public float ref_boosterLv3IncreaseSpeed = 20f;
    [HideInInspector] public float ref_boosterTimingIncreaseSpeed = 22f;

    [HideInInspector] public float ref_boosterStartingMaxSpeed = 10f;
    [HideInInspector] public float ref_boosterLv1MaxSpeed = 10f;
    [HideInInspector] public float ref_boosterLv2MaxSpeed = 15f;
    [HideInInspector] public float ref_boosterLv3MaxSpeed = 20f;
    [HideInInspector] public float ref_boosterTimingMaxSpeed = 25f;

    [HideInInspector] public float ref_boosterStartingDurationTime = 2f;
    [HideInInspector] public float ref_boosterLv1DurationTime = 2f;
    [HideInInspector] public float ref_boosterLv2DurationTime = 2.4f;
    [HideInInspector] public float ref_boosterLv3DurationTime = 2.8f;
    [HideInInspector] public float ref_boosterTimingDurationTime = 3.0f;

    [HideInInspector] public float ref_playerRankingBuffOnOff = 1f;
    [HideInInspector] public float ref_playerRankingIncreaseSpeed = 1f;
    [HideInInspector] public float ref_playerRankingmaxSpeed = 1f;

    [HideInInspector] public float ref_stunTime = 1.8f;
    [HideInInspector] public float ref_stunMaxSpeedDown = -10f;
    [HideInInspector] public float ref_stunDecreaseSpeed = -6f;

    [HideInInspector] public float ref_shieldMinSpeed = 10f;
    [HideInInspector] public float ref_shieldTime = 2.5f;
    [HideInInspector] public float ref_shieldCooltime = 3f;
    [HideInInspector] public float ref_shieldMaxSpeedDown = -15f;
    [HideInInspector] public float ref_shieldDecreaseSpeed = -7f;

    [HideInInspector] public float ref_moveLineMaxSpeedDown = -10f;
    [HideInInspector] public float ref_moveLineSpeedDurationTime = 2f;
    [HideInInspector] public float ref_moveLineDecreaseSpeed = -4f;

    [HideInInspector] public float ref_chargePadLv1ChargeAmount = 20;
    [HideInInspector] public float ref_chargePadLv2ChargeAmount = 30;
    [HideInInspector] public float ref_chargePadLv3ChargeAmount = 45;

    [HideInInspector] public float ref_chargePadLv1SpeedDurationTime = 1f;
    [HideInInspector] public float ref_chargePadLv2SpeedDurationTime = 1.3f;
    [HideInInspector] public float ref_chargePadLv3SpeedDurationTime = 1.8f;

    [HideInInspector] public float ref_chargePadLv1MaxSpeed = 2f;
    [HideInInspector] public float ref_chargePadLv2MaxSpeed = 4f;
    [HideInInspector] public float ref_chargePadLv3MaxSpeed = 5.5f;

    [HideInInspector] public float ref_chargePadLv1IncreaseSpeed = 1.5f;
    [HideInInspector] public float ref_chargePadLv2IncreaseSpeed = 2.2f;
    [HideInInspector] public float ref_chargePadLv3IncreaseSpeed = 3f;

    [HideInInspector] public float ref_fullBusterSpeedBuffNeedBusterGauge = 100;
    [HideInInspector] public float ref_fullBusterSpeedBuffIncreaseSpeed = 1f;
    [HideInInspector] public float ref_fullBusterSpeedBuffMaxSpeed = 3f;
    
    [HideInInspector] public float ref_boosterGreatTimingDurationTimeConsumedGaugeEfficiency = 0f;
    [HideInInspector] public float ref_boosterGreatTimingMaxSpeedConsumedGaugeEfficiency = 0f;
    [HideInInspector] public float ref_boosterGreatTimingIncreaseSpeedConsumedGaugeEfficiency = 0f;

    [HideInInspector] public float ref_boosterPerfectTimingDurationTimeConsumedGaugeEfficiency = 0f;
    [HideInInspector] public float ref_boosterPerfectTimingMaxSpeedConsumedGaugeEfficiency = 0f;
    [HideInInspector] public float ref_boosterPerfectTimingIncreaseSpeedConsumedGaugeEfficiency = 0f;

    [HideInInspector] public float ref_boosterTimingDurationTimeComboEfficiency = 0.02f;
    [HideInInspector] public float ref_boosterTimingMaxSpeedComboEfficiency = 0.02f;
    [HideInInspector] public float ref_boosterTimingIncreaseSpeedComboEfficiency = 0.02f;

    [HideInInspector] public float ref_boosterTimingComboTimingStartSpeed = 8.5f;
    [HideInInspector] public float ref_boosterTimingComboTimingSpeedEfficiency = 1.1f;
    [HideInInspector] public float ref_boosterTimingComboTimingAreaEfficiency = 0.9f;

    [HideInInspector] public float ref_boosterTimingComboTimingStartRadius = 8f;
    [HideInInspector] public float ref_boosterTimingComboTimingTagertRadius = 2f;
    [HideInInspector] public float ref_boosterTimingComboTimingTagertWidth = 0.7f;

    [HideInInspector] public int ref_boosterTimingMaxCombo = 5;

    [HideInInspector] public float ref_boosterStartingValidTime = 1f;
    [HideInInspector] public float ref_boosterStartingInputCoolTime = 0.5f;



    #endregion

    #region Observer Pattern
    public enum ObserverKey 
    { 
        None,
        StartDrag, 
        Drag,
        EndDrag,
    }

    public struct ObserverData_input
    {
        public Vector3 inputPosition;
    }

    static Dictionary<MonoBehaviour, Dictionary<ObserverKey, List<Action<ObserverData_input>>>> notify => IObserverBase<ObserverKey, ObserverData_input>.notify;

    public void AttachObserver(MonoBehaviour observer, ObserverKey key, Action<ObserverData_input> ac)
    {
        if (notify.ContainsKey(observer) == false)
        {
            var l = new List<Action<ObserverData_input>>();
            l.Add(ac);
            var notify = new Dictionary<ObserverKey, List<Action<ObserverData_input>>>();
            notify.Add(key, l);
            IObserverBase<ObserverKey, ObserverData_input>.notify.Add(observer, notify);
        }
        else
        {
            if (notify[observer].ContainsKey(key) == false)
            {
                var l = new List<Action<ObserverData_input>>();
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

    public void NotifyToObservers(ObserverKey key, ObserverData_input data)
    {
        foreach (var i in notify)
        {
            if (i.Value.ContainsKey(key))
            {
                foreach (var j in i.Value[key])
                {
                    j?.Invoke(data);
                }
            }
        }
    }
    #endregion //Observer Pattern


    public override void Spawned()
    {
        base.Spawned();

        isSpawned = true;

        inGameManager.UpdateNetworkPlayerList();

        InitializeSettings();

        if (IsMineAndNotAI)
        {
            Debug.Log("Adding PlayerMovement Callbacks");

            if (Runner != null)
                Runner.AddCallbacks(this);

            var panelIngame = PrefabManager.Instance.UI_PanelIngame;
            AttachObserver(panelIngame, ObserverKey.StartDrag, panelIngame.StartDrag_ObservedNotification);
            AttachObserver(panelIngame, ObserverKey.Drag, panelIngame.Drag_ObservedNotification);
            AttachObserver(panelIngame, ObserverKey.EndDrag, panelIngame.EndDrag_ObservedNotification);
        }

        InGameManager.Instance.SubscribeEvent(InGameManager.GameState.EndGame, () => { DeactivateAllFX(); });
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        isSpawned = false;

        if (IsMineAndNotAI)
        {
            if (Runner != null)
                Runner.RemoveCallbacks(this);

            var panelIngame = PrefabManager.Instance.UI_PanelIngame;
            DetachObserver(panelIngame);
        }

        InGameManager.Instance.UnSubscribeAllEvent(InGameManager.GameState.EndGame);
    }

    public void InitializeSettings()
    {
        SetName();
        SetBasicData();
        InitializeLineRenderer();

        transform.parent = null;

        wg = InGameManager.Instance.wayPoints;

        currentDest = null;
        upwardDirection = Vector3.up;

        startPosition = null;
        startRotation = null;

        client_currentMoveIndex = 0;
        client_currentMoveSpeed = 0;
        client_currentFowardRotationSpeed = 0;
        client_currentUpwardRotationSpeed = 0;
        client_currentLaneType = LaneType.None;
        client_previousLaneType = LaneType.None;

        network_currentLap = 0;
        network_currentMoveIndex = 0;
        network_currentRotationSpeed = 0;
        network_currentMoveSpeed = 0;
        network_currentBattery = 0;

        currentRank = 0;
        network_Rank = 1;

        network_IsLagging = false;

        network_isMoving = false;
        network_isEnteredTheFinishLine = false;

        client_currentCarBoosterLv = CarBoosterType.None;
        client_currentChargeBoosterLv = ChargePadBoosterLevel.None;

        network_currentLaneType = LaneType.None;
        network_previousLaneType = LaneType.None;
        network_isChangingLane = false;

        currentCarAnimationState = AnimationState.Idle;
        currentCharAnimationState = AnimationState.Idle;

        currentTrackType = TrackType.Normal;

        collisionChecker_Player.enabled = true;
        collisionChecker_Ground.enabled = true;
        rgbdy.isKinematic = true;

        isShield = false;
        isShieldCooltime = false;
        isStunned = false;
        isFlipped = false;
        isOutOfBoundary = false;
        isFlippingUp = false;
        isSetToPosition = false;
        isDrafting = false;
        isFlying = false;
        isMoveLine = false;
        isIntroMoving = false;
        isStartingBoosterActiavated = false;
        startingBoosterInputTimeCounter = 0f;
        isStartingBoosterInputBlocked = false;

        checkForLapCompletion = false;

        outOfBoundaryTimer = 0f;
        outOfBoundaryMoveSpeed = 0f;

        network_shieldCooltimeLeftTime = 0f;

        draftingCoolTimer = 0f;
        draftingActivationTimer = 0f;

        previousReachDest = Vector3.zero;
        previousDir = Vector3.zero;

        lagTime = 0;

        isTimingBoosterActive = false;
        isTimingBoosterReady = false;
        timingBoosterCurrentCombo = 0;
        currentTimingBoosterSuccessType = TimingBoosterSuccessType.None;

        triggerChecker_Front.Initialize();
        triggerChecker_Back.Initialize();
        triggerChecker_Right.Initialize();
        triggerChecker_Left.Initialize();

        SetRigidbodyConstraints(ConstraintType.XY);

        ClearLagDistQueue();

        if (GetComponent<Player_AI>() != null)
        {
            Initialize_AI();
        }
        else
        {
            IsAI = false;
        }

        IsAutoMovement = false;
    }

    private void SetName()
    {
        if (IsMine)
        {
            if (IsMineAndNotAI)
                gameObject.name = "MINE-" + CommonDefine.GetGoNameWithHeader("PLAYER") + "PID:" + networkPlayerID;
            else
                gameObject.name = "AI_MINE-" + CommonDefine.GetGoNameWithHeader("PLAYER") + "PID:" + networkPlayerID;
        }
        else
        {
            if (!IsAI)
                gameObject.name = "OTHER-" + CommonDefine.GetGoNameWithHeader("PLAYER") + "PID:" + networkPlayerID;
            else
                gameObject.name = "AI_OTHER-" + CommonDefine.GetGoNameWithHeader("PLAYER") + "PID:" + networkPlayerID;
        }
    }

    private void InitializeLineRenderer()
    {
        if (lr_inner != null && lr_outer != null)
        {
            lr_inner.gameObject.SafeSetActive(false);
            lr_outer.gameObject.SafeSetActive(false);
        }
    }

    public void SetBasicData()
    {
        lagMaxDistance = 2.5f; //2.0f
        lagMinDistance = 0.3f;
        lagMaxRate = 0.23f; // 0.2f
        lagMinRate = 0.03f; //0.02f
        lagTeleportTime = 1.5f;
        lagTeleportDist = 8f;
        lagPingAdjustmentRate = 1.4f;
        lagPositionAdjustmentRate_Min = 0.65f;
        lagPositionAdjustmentRate_Max = 4f;
        lagPositionAdjustmentRate_Mutiplier = 2.4f;
        lagUseAverageLagDist = true;
        lagUseTeleportLagDist = false;

        rigidBodyUseGravity = true;

        useIsOutOfBoundary = true;

        playerDirectionLerpMinSpeed = 4;
        playerDirectionLerpMaxSpeed = 10;
        playerDirectionLerpAddiionalAngleMinSpeed = 10;
        playerDirectionLerpAddiionalAngleMaxSpeed = 60;

        flipUpSpeed = 18;
        flipDownSpeed = 19;

        if (rgbdy != null)
        {
            rigidBodyMass = 10f;
            rigidBodyDrag = 7f;
            rigidBodyAngularDrag = 100;
            rigidBodyUseGravity = true;

            rgbdy.mass = rigidBodyMass;
            rgbdy.drag = rigidBodyDrag;
            rgbdy.angularDrag = rigidBodyAngularDrag;
            rgbdy.useGravity = rigidBodyUseGravity;
        }
    }

    public void SetCarData(int carID, int level)
    {
        var carData = DataManager.Instance.GetCarStat(carID, level);

        if (carData != null)
        {
            ref_maxSpeedLimit = carData.moveSpeedMax;

            ref_defaultMaxSpeed = carData.maxSpeed;
            ref_defaultIncreaseSpeed = carData.increaseSpeed;
            ref_defaultDecreaseSpeed = carData.decreaseSpeed;

            ref_increaseSpeedRange1stReferenceValue = carData.increaseSpeedRange1stReferenceValue;
            ref_increaseSpeedRange2ndReferenceValue = carData.increaseSpeedRange2ndReferenceValue;
            ref_increaseSpeedRange3rdReferenceValue = carData.increaseSpeedRange3rdReferenceValue;
            ref_increaseSpeedRange4thReferenceValue = carData.increaseSpeedRange4thReferenceValue;
            ref_increaseSpeedRange5thReferenceValue = carData.increaseSpeedRange5thReferenceValue;

            ref_increaseSpeedRange1stDepreciationValue = carData.increaseSpeedRange1stDepreciationValue;
            ref_increaseSpeedRange2ndDepreciationValue = carData.increaseSpeedRange2ndDepreciationValue;
            ref_increaseSpeedRange3rdDepreciationValue = carData.increaseSpeedRange3rdDepreciationValue;
            ref_increaseSpeedRange4thDepreciationValue = carData.increaseSpeedRange4thDepreciationValue;
            ref_increaseSpeedRange5thDepreciationValue = carData.increaseSpeedRange5thDepreciationValue;
            ref_increaseSpeedRangeEndDepreciationValue = carData.increaseSpeedRangeEndDepreciationValue;

            ref_decreaseSpeedRange1stReferenceValue = carData.decreaseSpeedRange1stReferenceValue;
            ref_decreaseSpeedRange2ndReferenceValue = carData.decreaseSpeedRange2ndReferenceValue;
            ref_decreaseSpeedRange3rdReferenceValue = carData.decreaseSpeedRange3rdReferenceValue;
            ref_decreaseSpeedRange4thReferenceValue = carData.decreaseSpeedRange4thReferenceValue;
            ref_decreaseSpeedRange5thReferenceValue = carData.decreaseSpeedRange5thReferenceValue;

            ref_decreaseSpeedRange1stDepreciationValue = carData.decreaseSpeedRange1stDepreciationValue;
            ref_decreaseSpeedRange2ndDepreciationValue = carData.decreaseSpeedRange2ndDepreciationValue;
            ref_decreaseSpeedRange3rdDepreciationValue = carData.decreaseSpeedRange3rdDepreciationValue;
            ref_decreaseSpeedRange4thDepreciationValue = carData.decreaseSpeedRange4thDepreciationValue;
            ref_decreaseSpeedRange5thDepreciationValue = carData.decreaseSpeedRange5thDepreciationValue;
            ref_decreaseSpeedRangeEndDepreciationValue = carData.decreaseSpeedRangeEndDepreciationValue;

            ref_boosterStartingIncreaseSpeed = carData.boosterStartingIncreaseSpeed;
            ref_boosterLv1IncreaseSpeed = carData.boosterLv1IncreaseSpeed;
            ref_boosterLv2IncreaseSpeed = carData.boosterLv2IncreaseSpeed;
            ref_boosterLv3IncreaseSpeed = carData.boosterLv3IncreaseSpeed;
            ref_boosterTimingIncreaseSpeed = carData.boosterTimingIncreaseSpeed;

            ref_boosterStartingMaxSpeed = carData.boosterStartingMaxSpeed;
            ref_boosterLv1MaxSpeed = carData.boosterLv1MaxSpeed;
            ref_boosterLv2MaxSpeed = carData.boosterLv2MaxSpeed;
            ref_boosterLv3MaxSpeed = carData.boosterLv3MaxSpeed;
            ref_boosterTimingMaxSpeed = carData.boosterTimingMaxSpeed;

            ref_boosterStartingDurationTime = carData.boosterStartingDurationTime;
            ref_boosterLv1DurationTime = carData.boosterLv1DurationTime;
            ref_boosterLv2DurationTime = carData.boosterLv2DurationTime;
            ref_boosterLv3DurationTime = carData.boosterLv3DurationTime;
            ref_boosterTimingDurationTime = carData.boosterTimingDurationTime;

            ref_batteryMax = carData.batteryMax;
            ref_batteryAutoChargeSpeed = carData.batteryAutoChargeSpeed;
            ref_boosterLv1Cost = carData.boosterLv1Cost;
            ref_boosterLv2Cost = carData.boosterLv2Cost;
            ref_boosterLv3Cost = carData.boosterLv3Cost;
            ref_batteryAttackSuccess = carData.batteryAttackSuccess;
            ref_batteryDefenseSuccess = carData.batteryDefenseSuccess;
            ref_batteryStartAmount = carData.batteryStartAmount;

            ref_shieldMaxSpeedDown = carData.shieldMaxSpeedDown;
            ref_shieldDecreaseSpeed = carData.shieldDecreaseSpeed;
            ref_shieldMinSpeed = carData.shieldMinSpeed;
            ref_shieldTime = carData.shieldTime;
            ref_shieldCooltime = carData.shieldCooltime;

            ref_flipUpTime = carData.flipUpTime;
            ref_flipGroundDelayTime = carData.flipGroundDelayTime;

            ref_draftActivateNeedTime = carData.draftActivateNeedTime;
            ref_draftActivationCooltime = carData.draftActivationCooltime;
            ref_draftActivationDist = carData.draftActivationDist;
            ref_draftMaxSpeed = carData.draftMaxSpeed;
            ref_draftIncreaseSpeed = carData.draftIncreaseSpeed;
            ref_draftRemainTime = carData.draftRemainTime;

            ref_stunTime = carData.stunTime;
            ref_stunMaxSpeedDown = carData.stunMaxSpeedDown;
            ref_stunDecreaseSpeed = carData.stunDecreaseSpeed;

            ref_moveLineMaxSpeedDown = carData.moveLineMaxSpeedDown;
            ref_moveLineSpeedDurationTime = carData.moveLineSpeedDurationTime;
            ref_moveLineDecreaseSpeed = carData.moveLineDecreaseSpeed;

            ref_playerMoveIndexCheckMinRange = carData.playerMoveIndexCheckMinRange;
            ref_playerMoveIndexCheckNumber = carData.playerMoveIndexCheckNumber;

            ref_playerRotationFowardSpeed = carData.playerRotationFowardSpeed;
            ref_playerRotationUpwardSpeed = carData.playerRotationUpwardSpeed;

            ref_fullBusterSpeedBuffNeedBusterGauge = carData.fullBusterSpeedBuffNeedBusterGauge;
            ref_fullBusterSpeedBuffIncreaseSpeed = carData.fullBusterSpeedBuffIncreaseSpeed;
            ref_fullBusterSpeedBuffMaxSpeed = carData.fullBusterSpeedBuffMaxSpeed;

            ref_chargePadLv1MaxSpeed = DataManager.Instance.GetGameConfig<float>("chargePadLv1MaxSpeed");
            ref_chargePadLv2MaxSpeed = DataManager.Instance.GetGameConfig<float>("chargePadLv2MaxSpeed");
            ref_chargePadLv3MaxSpeed = DataManager.Instance.GetGameConfig<float>("chargePadLv3MaxSpeed");

            ref_chargePadLv1IncreaseSpeed = DataManager.Instance.GetGameConfig<float>("chargePadLv1IncreaseSpeed");
            ref_chargePadLv2IncreaseSpeed = DataManager.Instance.GetGameConfig<float>("chargePadLv2IncreaseSpeed");
            ref_chargePadLv3IncreaseSpeed = DataManager.Instance.GetGameConfig<float>("chargePadLv3IncreaseSpeed");

            ref_chargePadLv1SpeedDurationTime = DataManager.Instance.GetGameConfig<float>("chargePadLv1SpeedDurationTime");
            ref_chargePadLv2SpeedDurationTime = DataManager.Instance.GetGameConfig<float>("chargePadLv2SpeedDurationTime");
            ref_chargePadLv3SpeedDurationTime = DataManager.Instance.GetGameConfig<float>("chargePadLv3SpeedDurationTime");

            ref_chargePadLv1ChargeAmount = DataManager.Instance.GetGameConfig<float>("chargePadLv1ChargeAmount");
            ref_chargePadLv2ChargeAmount = DataManager.Instance.GetGameConfig<float>("chargePadLv2ChargeAmount");
            ref_chargePadLv3ChargeAmount = DataManager.Instance.GetGameConfig<float>("chargePadLv3ChargeAmount");

            ref_playerRankingBuffOnOff = DataManager.Instance.GetGameConfig<int>("playerRankingBuffOnOff");
            ref_playerRankingIncreaseSpeed = DataManager.Instance.GetGameConfig<float>("playerRankingIncreaseSpeed");
            ref_playerRankingmaxSpeed = DataManager.Instance.GetGameConfig<float>("playerRankingmaxSpeed");

            ref_playerOutOfBoundaryTime = DataManager.Instance.GetGameConfig<float>("playerOutOfBoundaryTime");
            ref_playerValidReachDest = DataManager.Instance.GetGameConfig<float>("playerValidReachDest");
            ref_playerShowMoveSpeedMultiplier = DataManager.Instance.GetGameConfig<int>("playerShowMoveSpeedMultiplier");

            ref_boosterGreatTimingDurationTimeConsumedGaugeEfficiency = DataManager.Instance.GetGameConfig<float>("boosterGreatTimingDurationTimeConsumedGaugeEfficiency");
            ref_boosterGreatTimingMaxSpeedConsumedGaugeEfficiency = DataManager.Instance.GetGameConfig<float>("boosterGreatTimingMaxSpeedConsumedGaugeEfficiency");
            ref_boosterGreatTimingIncreaseSpeedConsumedGaugeEfficiency = DataManager.Instance.GetGameConfig<float>("boosterGreatTimingIncreaseSpeedConsumedGaugeEfficiency");

            ref_boosterPerfectTimingDurationTimeConsumedGaugeEfficiency = DataManager.Instance.GetGameConfig<float>("boosterPerfectTimingDurationTimeConsumedGaugeEfficiency");
            ref_boosterPerfectTimingMaxSpeedConsumedGaugeEfficiency = DataManager.Instance.GetGameConfig<float>("boosterPerfectTimingMaxSpeedConsumedGaugeEfficiency");
            ref_boosterPerfectTimingIncreaseSpeedConsumedGaugeEfficiency = DataManager.Instance.GetGameConfig<float>("boosterPerfectTimingIncreaseSpeedConsumedGaugeEfficiency");

            ref_boosterTimingDurationTimeComboEfficiency = DataManager.Instance.GetGameConfig<float>("boosterTimingDurationTimeComboEfficiency");
            ref_boosterTimingMaxSpeedComboEfficiency = DataManager.Instance.GetGameConfig<float>("boosterTimingMaxSpeedComboEfficiency");
            ref_boosterTimingIncreaseSpeedComboEfficiency = DataManager.Instance.GetGameConfig<float>("boosterTimingIncreaseSpeedComboEfficiency");


            ref_boosterTimingComboTimingStartSpeed = DataManager.Instance.GetGameConfig<float>("boosterTimingComboTimingStartSpeed");
            ref_boosterTimingComboTimingSpeedEfficiency = DataManager.Instance.GetGameConfig<float>("boosterTimingComboTimingSpeedEfficiency");
            ref_boosterTimingComboTimingAreaEfficiency = DataManager.Instance.GetGameConfig<float>("boosterTimingComboTimingAreaEfficiency");

            ref_boosterTimingComboTimingStartRadius = DataManager.Instance.GetGameConfig<float>("boosterTimingComboTimingStartRadius");
            ref_boosterTimingComboTimingTagertRadius = DataManager.Instance.GetGameConfig<float>("boosterTimingComboTimingTagertRadius");
            ref_boosterTimingComboTimingTagertWidth = DataManager.Instance.GetGameConfig<float>("boosterTimingComboTimingTagertWidth");

            ref_boosterTimingMaxCombo = DataManager.Instance.GetGameConfig<int>("boosterTimingMaxCombo");

            ref_boosterStartingValidTime = DataManager.Instance.GetGameConfig<float>("boosterStartingValidTime");
            ref_boosterStartingInputCoolTime = DataManager.Instance.GetGameConfig<float>("boosterStartingInputCoolTime");

        }
        else
        {
            Debug.Log("data is null?????? carID: " + carID + "    lv: " + level);
        }
    }

    public void SetCar(int carID)
    {
        this.carID = carID;

        var carRef = DataManager.Instance.GetCarRef(carID);
        if (carRef != null)
        {
            playerCar.SetCar(carRef);
        }
    }

    public void SetCharacter(int characterID)
    {
        this.characterID = characterID;

        var charRef = DataManager.Instance.GetCharacterRef(characterID);
        playerCharacter.SetCharacter(charRef);

        if (playerCar != null && playerCar.currentCar != null && playerCar.currentCar.dollyRoot != null)
        {
            playerCharacter.transform.SetParent(playerCar.currentCar.dollyRoot.transform);
            playerCharacter.transform.localScale = Vector3.one;
        }
    }

    public void InitializePosition(int randomLaneIndex, int indexNumber)
    {
        if (wg == null)
            wg = InGameManager.Instance.wayPoints;

        if (wg == null)
        {
            Debug.Log("Warning!!! wayPoints is null");
            return;
        }

        int startNextIndex = 1;
        int laneIndex = 0;
        int counter = 0;
        //들어오는 index는 무조건 0~인원수 까지니까...
        for (int i = 0; i <= (int)LaneType.Six; i++)
        {
            if (IsValidDestination((LaneType)i, startNextIndex) == true)
            {
                if (counter == randomLaneIndex)
                {
                    //들어오는 index는 무조건 0~인원수 까지니까...
                    laneIndex = i;
                    break;
                }

                counter++;
            }
        }

        //경기 시작 Position 설정
        SetStartPosition(laneIndex);

        //Intro Position 설정
        SetIntroPosition(laneIndex, indexNumber);
        MoveToIntroPosition();

        isSetToPosition = true;
    }

    public void ActivateIntroMovement()
    {
        isIntroMoving = true;
    }

    public void DeactivateIntroMovement()
    {
        isIntroMoving = false;
    }

    private void SetIntroPosition(int laneIndex, int randomIndex)
    {
        Vector3? firstWayPos;
        Vector3? nextWayPos;

        int startIndex = GetLastIndex() - 3 - randomIndex;
        int startNextIndex = startIndex + 1;

        if (startIndex < 0)
        {
            //혹시 모를 예외처리... 이런 경우는 없다고 보면...
            startIndex = 0;
            startNextIndex = 1;
        }

        if (IsValidDestination((LaneType)laneIndex, startNextIndex))
        {
            firstWayPos = GetWayPointPosition((LaneType)laneIndex, startIndex);
            nextWayPos = GetWayPointPosition((LaneType)laneIndex, startNextIndex);
        }
        else
        {
            firstWayPos = GetWayPointPosition(LaneType.Three, startIndex);
            nextWayPos = GetWayPointPosition(LaneType.Three, startNextIndex);
        }

        var dir = (nextWayPos.Value - firstWayPos.Value).normalized;

        introPosition = firstWayPos.Value;
        introRotation = Quaternion.LookRotation(dir, Vector3.up);
        if (startPosition.HasValue)
            introDest = startPosition.Value;
    }

    public void MoveToIntroPosition()
    {
        if (introPosition.HasValue)
            transform.position = introPosition.Value; //여기서는 꼭 transform.position 활용하자 rigidboy.transform 말구
        if (introRotation.HasValue)
            transform.rotation = introRotation.Value;

        if (introDest.HasValue)
        {
            currentDest = introDest.Value;
            currDirection = GetCurrentDirection();
        }

        collisionChecker_Player.enabled = true;
        collisionChecker_Ground.enabled = true;
        rgbdy.isKinematic = false;

        previousReachDest = transform.position;
        previousDir = currDirection;
    }

    private void SetStartPosition(int laneIndex, int moveIndex = 0)
    {
        int startIndex = moveIndex;
        int startNextIndex = moveIndex + 1;

        Vector3? firstWayPos;
        Vector3? nextWayPos;

        if (IsValidDestination((LaneType)laneIndex, startNextIndex))
        {
            firstWayPos = GetWayPointPosition((LaneType)laneIndex, startIndex);
            nextWayPos = GetWayPointPosition((LaneType)laneIndex, startNextIndex);
            SetCurrentLaneNumberType((LaneType)laneIndex);
        }
        else
        {
            Debug.Log("<color=red>No valid initial spwn point...! Automatically moved to index 3</color>");
            firstWayPos = GetWayPointPosition(LaneType.Three, startIndex);
            nextWayPos = GetWayPointPosition(LaneType.Three, startNextIndex);
            SetCurrentLaneNumberType(LaneType.Three);
        }

        client_currentMoveIndex = startNextIndex;

        var dir = (nextWayPos.Value - firstWayPos.Value).normalized;

        startPosition = firstWayPos.Value;
        startRotation = Quaternion.LookRotation(dir, Vector3.up);
        startDest = nextWayPos.Value;

        transform.rotation = startRotation.Value;
        currDirection = GetCurrentDirection();

        if (startDest.HasValue)
            currentDest = startDest.Value;
    }

    public void MoveToStartPosiiton()
    {
        DeactivateIntroMovement();
        int startIndex = 0;
        SetStartPosition((int)client_currentLaneType, startIndex);
        if (GetWayPointPosition(client_currentLaneType, startIndex).HasValue)
            transform.position = GetWayPointPosition(client_currentLaneType, startIndex).Value;

        collisionChecker_Player.enabled = true;
        collisionChecker_Ground.enabled = true;
        rgbdy.isKinematic = false;

        previousReachDest = transform.position;
        previousDir = currDirection;
    }

    public void OnRPCEvent_SetIngameInput(MoveInput type, Vector3 playerPos, int networkMoveIndex, LaneType networklaneType, float battery, CarBoosterType boosterLv = CarBoosterType.None, TimingBoosterSuccessType timingBoosterSuccessType = TimingBoosterSuccessType.None)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
            || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;


        if (networklaneType != client_currentLaneType)
            client_currentLaneType = networklaneType;

        switch (type)
        {
            case MoveInput.None:
                {

                }
                break;
            case MoveInput.Left:
                {
                    LaneType nextLaneType = LaneType.None;

                    if (client_currentLaneType != LaneType.Zero)
                    {
                        nextLaneType = client_currentLaneType - 1;
                    }

                    if (nextLaneType != client_currentLaneType)
                    {
                        if (isBoosting)
                        {
                            SetAnimation_CharacterOnly(AnimationState.MoveLeft, true);
                        }
                        else
                        {
                            SetAnimation_Both(AnimationState.MoveLeft, true);
                            SetAnimation_Both(AnimationState.Drive, false);
                        }
                    }

                    int newMoveIndex = GetNewMoveIndexAtRange(client_currentMoveIndex, nextLaneType, playerPos, ref_playerMoveIndexCheckMinRange, ref_playerMoveIndexCheckNumber);

                    if (IsValidLeftRightMove(client_currentLaneType, client_currentMoveIndex, nextLaneType, newMoveIndex) == false)
                        return;

                    SetCurrentLaneNumberTypeAndMoveIndex_LeftAndRight(nextLaneType, newMoveIndex);
                }
                break;
            case MoveInput.Right:
                {
                    LaneType nextLaneType = LaneType.None;

                    if (client_currentLaneType != LaneType.Six)
                    {
                        nextLaneType = client_currentLaneType + 1;
                    }

                    if (nextLaneType != client_currentLaneType)
                    {
                        if (isBoosting)
                        {
                            SetAnimation_CharacterOnly(AnimationState.MoveRight, true);
                        }
                        else
                        {
                            SetAnimation_Both(AnimationState.MoveRight, true);
                            SetAnimation_Both(AnimationState.Drive, false);
                        }
                    }

                    int newMoveIndex = GetNewMoveIndexAtRange(client_currentMoveIndex, nextLaneType, playerPos, ref_playerMoveIndexCheckMinRange, ref_playerMoveIndexCheckNumber);

                    if (IsValidLeftRightMove(client_currentLaneType, client_currentMoveIndex, nextLaneType, newMoveIndex) == false)
                        return;

                    SetCurrentLaneNumberTypeAndMoveIndex_LeftAndRight(nextLaneType, newMoveIndex);
                }
                break;

            case MoveInput.Boost:
                {
                    if (isStunned || isOutOfBoundary || !isGrounded)
                        return;

                    UseBattery(boosterLv); //valid 여부는 event 보내기 전에 판단함... 강제로 소모시키자
                    Car_BoostSpeed(boosterLv, battery, timingBoosterSuccessType);
                    ResetTimingBooster();
                }
                break;

            case MoveInput.Shield:
                {
                    if (isStunned || isFlipped || isOutOfBoundary || !isGrounded)
                        return;

                    isShield = true;
                    StartSheild();
                    DeactivateBooster();
                }
                break;
        }
    }


    public void SetCurrentLaneNumberType(LaneType type)
    {
        if (IsValidDestination(type, client_currentMoveIndex) == false)
        {
            Debug.Log("<color=red>InValid Lane...! Cannot move to that Lane!!!</color>");
            return;
        }

        client_previousLaneType = client_currentLaneType;

        switch (type)
        {
            case LaneType.Zero:
            case LaneType.One:
            case LaneType.Two:
            case LaneType.Three:
            case LaneType.Four:
            case LaneType.Five:
            case LaneType.Six:
                {
                    client_currentLaneType = type;
                }
                break;
        }

        if (IsMine)
        {
            if (client_previousLaneType != client_currentLaneType)
            {
                //인풋이 아닌 자동으로 레인 바뀐경우 network 상으로 알려주자...
                networkInGameRPCManager.RPC_SetLaneAndMoveIndex(networkPlayerID, (int)client_currentLaneType, client_currentMoveIndex);
            }
        }

        if (client_previousLaneType != client_currentLaneType
            && client_previousLaneType != LaneType.None
            && client_currentLaneType != LaneType.None)
        {
            UtilityCoroutine.StartCoroutine(ref moveLine, MoveLine(), this);
            network_isChangingLane = true;
        }
    }

    public void SetCurrentLaneNumberTypeAndMoveIndex_LeftAndRight(LaneType nextLane, int moveIndex)
    {
        if (IsValidDestinationIncludingOutOfBoundary(nextLane, moveIndex) == false)
        {
#if UNTIY_EDITOR
            if (IsMine)
                Debug.Log("<color=red>InValid Lane...! Cannot move to that Lane!!!</color>");
#endif
            return;
        }

        client_previousLaneType = client_currentLaneType;

        switch (nextLane)
        {
            case LaneType.Zero:
            case LaneType.One:
            case LaneType.Two:
            case LaneType.Three:
            case LaneType.Four:
            case LaneType.Five:
            case LaneType.Six:
                {
                    client_currentLaneType = nextLane;
                    client_currentMoveIndex = moveIndex;
                }
                break;
        }

        if (client_previousLaneType != client_currentLaneType
            && client_previousLaneType != LaneType.None
            && client_currentLaneType != LaneType.None)
        {
            UtilityCoroutine.StartCoroutine(ref moveLine, MoveLine(), this);
            network_isChangingLane = true;
        }
    }

    private IEnumerator moveLine = null;
    private IEnumerator MoveLine()
    {
        isMoveLine = true;

        var coolTimer = 0f;

        while (true)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                yield break;

            if (Runner != null)
                coolTimer += Runner.DeltaTime;

            if (coolTimer > ref_moveLineSpeedDurationTime)
                break;

            yield return null;
        }

        isMoveLine = false;
    }

    private void SetValidDestination(ref LaneType currLane, ref int currMoveIndex)
    {
        //lane 움직여줘서 valid dest 찾아주자...!

        LaneType originalLane = currLane;
        int originalMoveIndex = currMoveIndex;

        int laneTypeCount = (int)currLane;
        bool isMovingRight = true;
        int checkCount = 0;

        while (true)
        {
            if (laneTypeCount > (int)LaneType.Six || laneTypeCount <= (int)LaneType.None)
            {
                currLane = originalLane;
                currMoveIndex = originalMoveIndex;

                Debug.Log("<color=red>No Valid Dest....!!!! Check your map....!" +  "</color>");
                break;
            }

            if ((LaneType)laneTypeCount == LaneType.Zero)
            {
                if (checkCount == 0)
                {
                    laneTypeCount++;
                    isMovingRight = true;
                }
                else
                {
                    if (isMovingRight)
                        laneTypeCount++;
                    else
                        laneTypeCount--;
                }
            }

            else if ((LaneType)laneTypeCount >= LaneType.One && (LaneType)laneTypeCount <= LaneType.Five)
            {
                if (checkCount == 0)
                {
                    var leftLane = (LaneType)(laneTypeCount - 1);
                    var centerLane = (LaneType)laneTypeCount;
                    var rightLane = (LaneType)(laneTypeCount + 1);

                    var v1 = GetWayPointPosition(leftLane, GetNextMoveIndex(leftLane, currMoveIndex));
                    var v2 = GetWayPointPosition(centerLane, currMoveIndex);
                    var v3 = GetWayPointPosition(rightLane, GetNextMoveIndex(rightLane, currMoveIndex));

                    //거리 기반으로 가까운 곳으로 가게 하는 동시에 Valid 여부도 판단하자!!
                    if (v1.HasValue && v2.HasValue && v3.HasValue)
                    {
                        if (Vector3.Distance(v2.Value, v1.Value) <= Vector3.Distance(v2.Value, v3.Value))
                        {
                            if (IsValidDestinationIncludingOutOfBoundary(leftLane, currMoveIndex))
                            {
                                laneTypeCount--;
                                isMovingRight = false;
                            }
                            else
                            {
                                laneTypeCount++;
                                isMovingRight = true;
                            }
                        }
                        else
                        {
                            if (IsValidDestinationIncludingOutOfBoundary(rightLane, currMoveIndex))
                            {
                                laneTypeCount++;
                                isMovingRight = true;
                            }
                            else
                            {
                                laneTypeCount--;
                                isMovingRight = false;
                            }
                        }
                    }
                    else
                        Debug.Log("Erroor... v1 v2 v3 Has no value?");
                }
                else
                {
                    if (isMovingRight)
                    {
                        laneTypeCount++;
                    }
                    else
                    {
                        laneTypeCount--;
                    }
                }
            }
            else if ((LaneType)laneTypeCount == LaneType.Six)
            {
                if (checkCount == 0)
                {
                    laneTypeCount--;
                    isMovingRight = false;
                }
                else
                {
                    if (isMovingRight)
                        laneTypeCount++;
                    else
                        laneTypeCount--;
                }
            }

            if (IsValidDestination((LaneType)laneTypeCount, currMoveIndex))
            {
                currLane = (LaneType)laneTypeCount;
                SetCurrentLaneNumberType((LaneType)laneTypeCount);
                break;
            }

            checkCount++;
        }
    }

    //유호한 Dest인지
    private bool IsValidDestination(LaneType type, int moveIndex)
    {
        bool isValid = false;

        if (wg == null)
            return false;

        var pointList = GetWayPointsList(type);

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[moveIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        isValid = true;
                    }
                    break;
                case WayPointSystem.Waypoint.WayPointType.Blocked:
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                    {
                        isValid = false;
                    }
                    break;
            }
        }


        return isValid;
    }

    //OutOfBoundary 포함해서 유효한 Dest인지
    private bool IsValidDestinationIncludingOutOfBoundary(LaneType type, int moveIndex)
    {
        bool isValid = false;

        if (wg == null)
            return false;

        var pointList = GetWayPointsList(type);

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[moveIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        isValid = true;
                    }
                    break;
                case WayPointSystem.Waypoint.WayPointType.Blocked:
                    {
                        isValid = false;
                    }
                    break;
            }
        }


        return isValid;
    }

    //왼쪽 오른 이동이 가능한 웨이포인트인지 확인
    private bool IsValidLeftRightMove(LaneType type, int moveIndex)
    {
        bool isValid = false;

        if (wg == null)
            return false;

        var pointList = GetWayPointsList(type);

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[moveIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                    {
                        isValid = true;
                    }
                    break;
                case WayPointSystem.Waypoint.WayPointType.Blocked:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        isValid = false;
                    }
                    break;
            }
        }


        return isValid;
    }

    //왼쪽 오른 이동이 가능한 웨이포인트인지 확인
    private bool IsValidLeftRightMove(LaneType currType, int currMoveIndex, LaneType nextType, int nextMoveIndex)
    {
        bool isValid = false;

        if (wg == null)
            return false;

        var nextPointList = GetWayPointsList(nextType);
        var currPointList = GetWayPointsList(currType);

        if (nextPointList != null && nextPointList.Count > 0
            && currPointList != null && currPointList.Count > 0)
        {
            switch (nextPointList[nextMoveIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
                    {
                        //빠르게 input을 받을 경우 Normal 구역 -> OutOfBoundary 구역 -> Normal 구역 통과하는 현상 제거 하기 위해 추가된 로직
                        if (currPointList[currMoveIndex].currentWayPointType == WayPointSystem.Waypoint.WayPointType.OutOfBoundary
                            && client_previousLaneType != nextType)
                        {
                            isValid = false;
                        }
                        else
                            isValid = true;
                    }
                    break;
                case WayPointSystem.Waypoint.WayPointType.OutOfBoundary:
                    {
                        isValid = true;
                    }
                    break;
                case WayPointSystem.Waypoint.WayPointType.Blocked:
                case WayPointSystem.Waypoint.WayPointType.OnlyFront:
                case WayPointSystem.Waypoint.WayPointType.Sky:
                    {
                        isValid = false;
                    }
                    break;
            }
        }


        return isValid;
    }

    private bool IsReachedToDestination()
    {
        bool isReached = false;

        if (currentDest.HasValue)
        {
            if (Vector3.Distance(transform.position, currentDest.Value) < ref_playerValidReachDest)
                isReached = true;

            if (Vector3.Distance(transform.position, currentDest.Value) < 3f
                && UtilityCommon.IsFront(transform.forward, transform.position, currentDest.Value) == false)
                isReached = true;
        }


        return isReached;
    }

    private bool IsCompleteLap() //1바퀴 Lap을 돌았을 경우
    {
        bool isCompleteLap = false;

        var lastIndex = GetLastIndex();

        var w1 = GetWayPointPosition(client_currentLaneType, lastIndex);
        var w2 = GetWayPointPosition(client_currentLaneType, 0);

        if (w1.HasValue && w2.HasValue)
        {
            var dir = (w1.Value - w2.Value).normalized;

            if (UtilityCommon.IsFront(dir, transform.position, w2.Value)
                && Vector3.Distance(transform.position, w2.Value) < 10f)
            {
                if (checkForLapCompletion)
                {
                    isCompleteLap = true;
                    checkForLapCompletion = false;
                }
            }
            else
            {
                //중간이상 도달하면 lap complete 체크하자!!
                if (GetLastIndex() / 2 < client_currentMoveIndex
                    && checkForLapCompletion == false)
                    checkForLapCompletion = true;
            }
        }
    


        return isCompleteLap;
    }

    private int GetTotalMoveIndex()
    {
        if (wg == null)
            return -1;

        var pointList = GetWayPointsList(LaneType.Three);

        if (pointList != null && pointList.Count > 0)
        {
            if (DataManager.FinalLapCount != -1)
                return pointList.Count * DataManager.FinalLapCount;
            else
                return pointList.Count;
        }

        return -1;
    }

    private bool IsLastLap(int lap) //마지막 Lap
    {
        return InGameManager.Instance.IsLastLap(lap);
    }

    public bool IsCurrentLastLap()
    {
        return IsLastLap(network_currentLap);
    }

    private bool IsFinishLap(int lap) //최종 Lap
    {
        return InGameManager.Instance.IsFinishLap(lap);
    }

    public bool IsCurrentFinishLap()
    {
        return IsFinishLap(network_currentLap);
    }

    private bool IsLastIndex(LaneType type, int index)
    {
        bool isLast = false;

        if (wg == null)
            return false;

        var pointList = GetWayPointsList(type);

        if (pointList != null && pointList.Count > 0)
        {
            if (index >= pointList.Count - 1)
                isLast = true;
        }

        return isLast;
    }

    private int GetLastIndex(LaneType type = LaneType.Three)
    {
        if (wg == null)
            return -1;

        var pointList = GetWayPointsList(type);

        if (pointList != null && pointList.Count > 0)
        {
            return pointList.Count - 1;
        }

        return -1;
    }

    public int GetNextMoveIndex(LaneType type, int index)
    {
        if (IsLastIndex(type, index) == false)
            return ++index;
        else
            return 0;
    }

    public void StartMoving()
    {
        isIntroMoving = false;

        if (IsMine)
        {
            network_isMoving = true;

            if (isStartingBoosterActiavated)
                networkInGameRPCManager.RPC_BoostPlayer(this.networkPlayerID, (int)CarBoosterType.CarBooster_Starting, (int)TimingBoosterSuccessType.None);
        }

        if (IsMineAndNotAI)
        {
            inGameManager.startRaceTick = this.Runner.Tick;
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Drive);
        }
    }

    public void StopMoving()
    {
        if (IsMine)
            network_isMoving = false;

        collisionChecker_Ground.enabled = true;

        DeactivateBooster();

        if (playerCar != null)
        {
            playerCar.DeactivateWheelFX();
        }


        if (IsMineAndNotAI)
        {
            SoundManager.Instance.StopSound(SoundManager.SoundClip.Drive);
        }
    }


    private class WayPointIndexCheck
    {
        public int moveIndex = 0;
        public float distance = 0f;
    }

    //레인 바꿀때 or Dest 도달시 최적(?)의 index로 이동하도록 변경
    private int GetNewMoveIndexAtRange(int index, LaneType laneType, Vector3 playerPos, float checkRange, int checkIndexNum = 0)
    {
        if (checkRange <= 0f)
            return index;

        int newIndex = index;
        var list = new List<WayPointIndexCheck>();

        if (checkIndexNum <= 0)
            checkIndexNum = ref_playerMoveIndexCheckNumber;

        int cnt = 0;
        for (int i = 0; i < checkIndexNum; i++)
        {
            int nextIndex = GetNextMoveIndex(laneType, index + cnt);
            var posi = GetWayPointPosition(laneType, nextIndex);

            if (posi.HasValue)
            {
                var dist = Vector3.Distance(playerPos, posi.Value);
                if (dist < checkRange)
                    list.Add(new WayPointIndexCheck() { moveIndex = nextIndex, distance = dist });
            }


            //다시 시작부터 체크
            if (IsLastIndex(laneType, nextIndex))
            {
                index = 0;
                cnt = 0;
            }
            ++cnt;
        }

        if (list != null && list.Count > 0)
        {
            list.Sort((x, y) => y.distance.CompareTo(x.distance));
            newIndex = list[0].moveIndex;
        }
        else
        {
            //기존 currentIndex 사용...!
            newIndex = index;
        }

        return newIndex;
    }

    public override void FixedUpdateNetwork()
    {
        FixedUpdate_SetNetworkValues();
        FixedUpdate_CalcLagTime();
        FixedUpdate_NetworkInput();
        FixedUpdate_SetCollider();
        FixedUpdate_Move();
        FixedUpdate_ChargeBattery();
        FixedUpdate_Animation();
        FixedUpdate_TeleportIfLag();
        FixedUpdate_SetDriveSound();
        FixedUpdate_Camera();
        FixedUpdate_AI();
        FixedUpdate_SetLineRenderer();
        FixedUpdate_StartingBoosterTimer();
    }

    private void FixedUpdate_SetNetworkValues()
    {
        if (IsMine)
        {
            network_position = transform.position;
            network_currentMoveIndex = client_currentMoveIndex;
            network_currentMoveSpeed = client_currentMoveSpeed;
            network_currentRotationSpeed = client_currentFowardRotationSpeed;
            network_currentLaneType = client_currentLaneType;
            network_previousLaneType = client_previousLaneType;
            network_Rank = currentRank;
        }
    }

    private void FixedUpdate_CalcLagTime()
    {
        lagTime = Runner.GetPlayerRtt(PlayerRef.None);
    }

    private void FixedUpdate_NetworkInput()
    {
        //network input 적용...!

        if (IsMineAndNotAI)
        {
            if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
            {
                if (GetInput(out TouchSwipe_NetworkInputData data))
                {
                    ObserverData_input obData = new ObserverData_input();
                    obData.inputPosition = data.position;

                    switch (data.currentEventType)
                    {
                        case TouchSwipe_NetworkInputData.EventType.None:
                            break;

                        case TouchSwipe_NetworkInputData.EventType.StartDrag:
                            {
                                NotifyToObservers(ObserverKey.StartDrag, obData);
                            }
                            break;
                        case TouchSwipe_NetworkInputData.EventType.Drag:
                            {
                                NotifyToObservers(ObserverKey.Drag, obData);
                            }
                            break;
                        case TouchSwipe_NetworkInputData.EventType.EndDrag:
                            {
                                NotifyToObservers(ObserverKey.EndDrag, obData);
                            }
                            break;
                    }
                }
            }
        }
    }

    private void FixedUpdate_SetCollider()  
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
            || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
        {
            if (triggerChecker_Front != null)
            {
                if (isCarAttackBoosting)
                    triggerChecker_Front.isCollisionUsed = true;
                else
                    triggerChecker_Front.isCollisionUsed = false;
            }
        }
    }

    #region Move

    private void FixedUpdate_Move()
    {
        if (IsWayPointSet() == false)
            return;

        if (network_isMoving && !isFlipped && !isOutOfBoundary)
        {
            if (IsMine)
            {
                // 내 플레이어 앞으로 이동
                Move_Foward_Mine();
            }
            else
            {
                // 내가 아닌 경우 Network 동기화
                Move_Foward_Other();
            }
        }
        else if (isOutOfBoundary)
        {
            Move_OutOfBoundary();
        }
        else if (isFlipped)
        {
            Move_Flip();
        }
        else if (isIntroMoving)
        {
            Move_Intro();
        }
    }

    //Move Mine.... Client Movement
    private void Move_Foward_Mine()
    {
        SetRigidbodyConstraints(ConstraintType.XY);

        client_currentMoveSpeed = GetCurrentMoveSpeed();
        client_currentFowardRotationSpeed = GetCurrentFowardRotationSpeed();
        client_currentUpwardRotationSpeed = GetCurrentUpwardRotationSpeed();
        currentDest = GetDestination(client_currentMoveIndex, client_currentLaneType);
        currDirection = GetCurrentDirection();

        //Position 세팅
        rgbdy.MovePosition(transform.position + currDirection * Runner.DeltaTime * client_currentMoveSpeed);

        //Rotation 세팅
        Vector3 fowardDir = GetFowardDirection();
        Vector3 upwardDir = GetUpwardDirection();
        var lerpUpwardRotation = Vector3.Slerp(upwardDir, transform.TransformDirection(Vector3.up), Runner.DeltaTime * client_currentUpwardRotationSpeed);
        
        if (IsFowardRotationLerpActivated())
        {
            var lerpFowardRotation = Vector3.Slerp(fowardDir, currDirection, Runner.DeltaTime * client_currentFowardRotationSpeed);
            rgbdy.rotation = Quaternion.LookRotation(lerpFowardRotation, lerpUpwardRotation);
        }
        else
        {
            Vector3 destDir;
            if (currentDest.HasValue)
                destDir = (currentDest.Value - transform.position).normalized;
            else
                destDir = transform.TransformDirection(Vector3.forward).normalized;

            if (destDir != Vector3.zero)
                rgbdy.rotation = Quaternion.LookRotation(destDir, lerpUpwardRotation);
            else
                rgbdy.rotation = Quaternion.Euler(destDir);
        }

        if (GetDestinationWaypointType(client_currentMoveIndex, client_currentLaneType) == WayPointSystem.Waypoint.WayPointType.OutOfBoundary
            && isGrounded == false && isOutOfBoundary == false && network_isEnteredTheFinishLine == false)
        {
            networkInGameRPCManager.RPC_SetOutOfBoundary(networkPlayerID);
            return;
        }

        //목표 지점 도달시
        if (IsReachedToDestination() == true)
        {
            previousReachDest = transform.position;
            if (currentDest.HasValue)
                previousDir = (currentDest.Value - transform.position).normalized;

            //현재 속도에 기반해서 check할 Index개수 정하자....
            var checkIndex = (int)System.Math.Truncate(Mathf.Lerp(1, 3, (Mathf.Clamp((client_currentMoveSpeed - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit) / Mathf.Clamp((ref_maxSpeedLimit - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit))));

            //다음 인덱스로 가자...
            int newMoveIndex = GetNewMoveIndexAtRange(client_currentMoveIndex, client_currentLaneType, transform.position, ref_playerMoveIndexCheckMinRange, checkIndex);
            if (newMoveIndex != client_currentMoveIndex || IsLastIndex(client_currentLaneType, newMoveIndex) == true)
                client_currentMoveIndex = newMoveIndex;
            else
                client_currentMoveIndex = GetNextMoveIndex(client_currentLaneType, client_currentMoveIndex);

            //다음 인덱스가 valid한 dest인지 확인...!
            if (IsValidDestinationIncludingOutOfBoundary(client_currentLaneType, client_currentMoveIndex) == false)
            {
                //move to a valid lane
                LaneType validLane = client_currentLaneType;
                int validMoveIndex = client_currentMoveIndex;
                SetValidDestination(ref validLane, ref validMoveIndex);
                client_currentLaneType = validLane;
                client_currentMoveIndex = validMoveIndex;
            }

            if (isBoosting)
            {
                SetAnimation_CharacterOnly(AnimationState.MoveLeft, false);
                SetAnimation_CharacterOnly(AnimationState.MoveRight, false);
            }
            else
            {
                SetAnimation_Both(AnimationState.MoveLeft, false);
                SetAnimation_Both(AnimationState.MoveRight, false);
                SetAnimation_Both(AnimationState.Drive, true);
            }

            network_isChangingLane = false;
        }

        //Lap (1바퀴 돌았을때)
        if (IsCompleteLap())
        {
            //index = 0 부터 다시 시작
            network_currentLap++;

            networkInGameRPCManager.RPC_PlayerCompletedLap(networkPlayerID, network_currentLap);

            //LAP FINISH!!!!!!!!! 마지막 랩 종료 되었을 경우
            if (IsFinishLap(network_currentLap))
            {
                if (network_isEnteredTheFinishLine == false && DataManager.FinalLapCount != -1)
                {
                    if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
                        || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
                    {
                        network_isEnteredTheFinishLine = true;

                        inGameManager.endRaceTick = this.Runner.Tick;
                        float finishTime = (inGameManager.endRaceTick - inGameManager.startRaceTick) * this.Runner.DeltaTime;

                        networkInGameRPCManager.RPC_PlayerPassedFinishLine(networkPlayerID, finishTime);
                        PnixNetworkManager.Instance.SendCrossTheRaceFinishLine(network_PID, CNetworkManager.Instance.ServerTime);
                    }

                    if (playerCar != null)
                    {
                        Camera_Base.Instance.TurnOffCameraAllFX();
                        playerCar.DeactivateStunFX();
                    }
                }
            }
        }

        //드레이프팅 세팅
        SetDrafting();

        if (network_isEnteredTheFinishLine == true && client_currentMoveSpeed <= 0f)
        {
            //움직임 최종 종료...
            StopMoving();
        }
    }

    //Network Sync Other Player
    private void Move_Foward_Other()
    {
        SetRigidbodyConstraints(ConstraintType.XY);

        if (isFlipped == false)
        {
            collisionChecker_Player.enabled = true;
        }

        if (IsNearStartWayPoint() == false
            && network_currentMoveIndex > client_currentMoveIndex)
        {
            client_currentMoveIndex = network_currentMoveIndex;
            client_currentLaneType = network_currentLaneType;
        }

        client_currentMoveSpeed = GetCurrentMoveSpeed();
        client_currentFowardRotationSpeed = GetCurrentFowardRotationSpeed();
        client_currentUpwardRotationSpeed = GetCurrentUpwardRotationSpeed();
        currentDest = GetDestination(client_currentMoveIndex, client_currentLaneType);
        currDirection = GetCurrentDirection();

        var nextPosition = transform.position + currDirection * Runner.DeltaTime * client_currentMoveSpeed;
        var lagPosition = (float)lagTime * currDirection * Runner.DeltaTime * client_currentMoveSpeed;
        nextPosition += lagPosition * GetLagPositionAdjustRate();

        //네트워크 값하고 클라이언트 내 값차이가 심할 경우 조정해주자...!
        if (IsPositionLagging() == true)
        {
            var clientPosition = transform.position + currDirection * Runner.DeltaTime * client_currentMoveSpeed;
            var networkPosition = this.network_position;
            networkPosition += lagPosition * GetLagPositionAdjustRate();
            clientPosition += lagPosition * GetLagPositionAdjustRate();

            network_IsLagging = true;

            var diff = Vector3.Distance(clientPosition, networkPosition);
            lagAdjustRate = GetPlayerLagAdjustRate(diff);

            if (UtilityCommon.IsFront(currDirection, clientPosition, networkPosition))
            {
                //속도를 높여서 맞추자
                nextPosition += currDirection * Runner.DeltaTime * client_currentMoveSpeed * lagAdjustRate;
            }
            else
            {
                //속도를 줄여서 맞추자
                nextPosition -= currDirection * Runner.DeltaTime * client_currentMoveSpeed * lagAdjustRate;
            }
        }
        else
            network_IsLagging = false;


        //Position 세팅
        rgbdy.MovePosition(nextPosition);

        //Rotation 세팅
        Vector3 fowardDir = GetFowardDirection();
        Vector3 upwardDir = GetUpwardDirection();
        var lerpUpwardRotation = Vector3.Slerp(upwardDir, transform.TransformDirection(Vector3.up), Runner.DeltaTime * client_currentUpwardRotationSpeed);

        if (IsFowardRotationLerpActivated())
        {
            var lerpRotation = Vector3.Slerp(fowardDir, currDirection, Runner.DeltaTime * client_currentFowardRotationSpeed);
            rgbdy.rotation = Quaternion.LookRotation(lerpRotation, lerpUpwardRotation);
        }
        else
        {
            Vector3 destDir;
            if (currentDest.HasValue)
                destDir = (currentDest.Value - transform.position).normalized;
            else
                destDir = transform.TransformDirection(Vector3.forward).normalized;

            if (destDir != Vector3.zero)
                rgbdy.rotation = Quaternion.LookRotation(destDir, lerpUpwardRotation);
            else
                rgbdy.rotation = Quaternion.Euler(destDir);
        }


        if (IsReachedToDestination() == true)
        {
            if (IsLastIndex(client_currentLaneType, client_currentMoveIndex) == true)
            {
                //index = 0 부터 다시 시작
                client_currentMoveIndex = GetNextMoveIndex(client_currentLaneType, client_currentMoveIndex);
            }
            else
            {
                var checkIndex = (int)System.Math.Truncate(Mathf.Lerp(1, 3, (Mathf.Clamp((client_currentMoveSpeed - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit) / Mathf.Clamp((ref_maxSpeedLimit - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit))));

                int newMoveIndex = GetNewMoveIndexAtRange(client_currentMoveIndex, client_currentLaneType, transform.position, ref_playerMoveIndexCheckMinRange, checkIndex);
                if (newMoveIndex != client_currentMoveIndex || IsLastIndex(client_currentLaneType, newMoveIndex) == true)
                    client_currentMoveIndex = newMoveIndex;
                else
                    client_currentMoveIndex = GetNextMoveIndex(client_currentLaneType, client_currentMoveIndex);
            }

            //다음 인덱스가 valid한 dest인지 확인...!
            if (IsValidDestinationIncludingOutOfBoundary(client_currentLaneType, client_currentMoveIndex) == false)
            {
                //move to a valid lane
                LaneType validLane = client_currentLaneType;
                int validMoveIndex = client_currentMoveIndex;
                SetValidDestination(ref validLane, ref validMoveIndex);
                client_currentLaneType = validLane;
                client_currentMoveIndex = validMoveIndex;
            }

            if (isBoosting)
            {
                SetAnimation_CharacterOnly(AnimationState.MoveLeft, false);
                SetAnimation_CharacterOnly(AnimationState.MoveRight, false);
            }
            else
            {
                SetAnimation_Both(AnimationState.MoveLeft, false);
                SetAnimation_Both(AnimationState.MoveRight, false);
                SetAnimation_Both(AnimationState.Drive, true);
            }

            previousDir = currDirection;
        }

        if (network_isEnteredTheFinishLine == true && client_currentMoveSpeed <= 0f)
        {
            //움직임 최종 종료...
            StopMoving();
        }
    }

    private void Move_OutOfBoundary()
    {
        outOfBoundaryTimer += Runner.DeltaTime;
        outOfBoundaryMoveSpeed = GetCurrentMoveSpeed();

        if (outOfBoundaryTimer > ref_playerOutOfBoundaryTime)
        {
            if (IsMine && network_isEnteredTheFinishLine == false)
            {
                MovePlayerToValidPosition();
                outOfBoundaryTimer = 0f;
            }
        }
        else
        {
            collisionChecker_Player.enabled = false;
            rgbdy.useGravity = true;
            SetRigidbodyConstraints(ConstraintType.None);

            var direction = transform.forward;

            if (currentTrackType == TrackType.None)
            {
                direction = UtilityCommon.GetDirectionByAngle_YZ(-45f, transform.TransformDirection(Vector3.forward));
            }
            else if (currentTrackType == TrackType.OutOfBound)
            {
                direction = transform.forward;
            }

            rgbdy.MovePosition(transform.position + direction * Runner.DeltaTime * GetCurrentMoveSpeed());

        }
    }

    private void Move_Flip()
    {
        SetRigidbodyConstraints(ConstraintType.XY);

        if (isFlippingUp)
        {
            rgbdy.MovePosition(transform.position + transform.TransformDirection(Vector3.up) * Runner.DeltaTime * flipUpSpeed);
        }
        else
        {
            if (isGrounded == false)
                rgbdy.MovePosition(transform.position + transform.TransformDirection(Vector3.down) * Runner.DeltaTime * flipDownSpeed * 1.5f);
        }
    }

    private void Move_Intro()
    {
        if (isIntroMoving)
        {
            SetRigidbodyConstraints(ConstraintType.None);

            client_currentMoveSpeed = ref_defaultMaxSpeed * 0.3f;
            if (introDest.HasValue)
            {
                currentDest = introDest.Value;
                currDirection = (currentDest.Value - transform.position).normalized;
            }

            //Position 세팅
            rgbdy.MovePosition(transform.position + currDirection * Runner.DeltaTime * client_currentMoveSpeed);

            //Rotation 세팅
            rgbdy.rotation = Quaternion.LookRotation(currDirection);


            if (currentDest.HasValue && Vector3.Distance(transform.position, currentDest.Value) < ref_playerValidReachDest * 0.3f)
            {
                isIntroMoving = false;
            }
  
            if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame)
                isIntroMoving = false;
        }
    }

    #endregion

    private void FixedUpdate_ChargeBattery()
    {
        if (IsMine)
        {
            if (!isOutOfBoundary && network_isMoving)
            {
                if (ref_batteryAutoChargeSpeed > 0)
                {
                    autoBatteryChargeCounter += Runner.DeltaTime;

                    if (autoBatteryChargeCounter > ref_batteryAutoChargeSpeed)
                    {
                        autoBatteryChargeCounter = 0;

                        if (network_currentBattery < ref_batteryMax)
                            networkInGameRPCManager.RPC_SetPlayerBattery(networkPlayerID, true, 1);
                    }

                    if (network_currentBattery >= ref_batteryMax)
                        network_currentBattery = ref_batteryMax;
                }
            }
        }
        else
        {

        }
    }

    private void FixedUpdate_TeleportIfLag()
    {
        if (IsMine == false)
        {
            if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
                || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
            {
                if (network_isMoving == true && isOutOfBoundary == false && isFlipped == false && network_isEnteredTheFinishLine == false)
                {
                    var networkPosition = this.network_position;
                    var lagPosition = (float)lagTime * currDirection * client_currentMoveSpeed * Runner.DeltaTime;
                    networkPosition += lagPosition * GetLagPositionAdjustRate();

                    // GetPlayerLagTeleportDist() 거리 이상의 지연이 있는 경우.. 강제로 네트워크 데이터 덮어씌워버리기
                    if (Vector3.Distance(transform.position, networkPosition) > GetPlayerLagTeleportDist())
                    {
                        if (lagUseTeleportLagDist == true)
                            SetToNetworkValue();
                    }

                    //Lane Type이 맞지 않는 경우 일정 시간 이상 맞지 않으면 network에 lane type 맞춰주자
                    if (network_currentLaneType != client_currentLaneType)
                    {
                        networkClientLaneDiffTimeCounter += Runner.DeltaTime;
                        if (networkClientLaneDiffTimeCounter > 1.0f) //1초 이상 값이 다른 경우..... lanetype 맞춰주자
                        {
                            if (IsValidDestination(network_currentLaneType, client_currentMoveIndex))
                            {
                                client_currentLaneType = network_currentLaneType;
                                networkClientLaneDiffTimeCounter = 0f;
                            }
                        }
                    }
                    else
                    {
                        networkClientLaneDiffTimeCounter = 0;
                    }
                }
            }
        }
    }

    private float GetPlayerLagTeleportDist()
    {
        float dist = lagTeleportDist;

        float lagDist_Min = lagTeleportDist;
        float lagDist_Max = lagTeleportDist * 5;

        var avgPing = PhotonNetworkManager.Instance.GetAveragePing();

        float value = lagDist_Max - ((avgPing - PhotonNetworkManager.GOOD_PING_LIMIT) / (PhotonNetworkManager.GOOD_PING_LIMIT) * (lagDist_Max - lagDist_Min));
        dist = Math.Clamp(value, lagDist_Min, lagDist_Max);

        return dist;
    }

    private void SetToNetworkValue()
    {
        Debug.Log("<color=red>" + "PLAYER Lagging Hard!!!! Moved " + gameObject.name + " to network position...!" + "  lag dist: " + Vector3.Distance(transform.position, network_position) + "</color>");

        client_currentMoveIndex = network_currentMoveIndex;
        client_currentMoveSpeed = network_currentMoveSpeed;
        client_currentLaneType = network_currentLaneType;
        transform.position = network_position + (float)lagTime * currDirection * Runner.DeltaTime * network_currentMoveSpeed;
    }

    private void FixedUpdate_SetDriveSound()
    {
        var MAX_PITCH = SoundManager.Instance.drive_MAX_pitch;
        var MIN_PITCH = SoundManager.Instance.drive_MIN_pitch;

        if (IsMine && network_isMoving)
        {
            float pitch = 1f;

            if (MIN_PITCH > MAX_PITCH)
                MIN_PITCH = MAX_PITCH;

            pitch = MIN_PITCH + (MAX_PITCH - MIN_PITCH) * (client_currentMoveSpeed / ref_maxSpeedLimit);
            SoundManager.Instance.SetSoundPitch(SoundManager.SoundClip.Drive, pitch);

            SoundManager.Instance.player_speed = client_currentMoveSpeed;
            SoundManager.Instance.drive_pitch = pitch;
        }
        else
        {
            SoundManager.Instance.player_speed = client_currentMoveSpeed;
            SoundManager.Instance.drive_pitch = MIN_PITCH;
        }
    }

    private void FixedUpdate_Animation()
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady)
        {
            SetAnimation_Both(AnimationState.Idle, true);
        }

            
        if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
            || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
        {
            if (network_isEnteredTheFinishLine == false && client_currentMoveSpeed > 0f)
                SetAnimation_Both(AnimationState.Idle, false);

            if (network_isEnteredTheFinishLine)
            {
                SetAnimation_CharacterOnly(AnimationState.Idle, false);
                SetAnimation_CharacterOnly(AnimationState.Complete, true);
            }

        }

        if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
        {
            if (network_isEnteredTheFinishLine == false)
            {
                if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame && network_isMoving == false)
                {
                    SetAnimation_CharacterOnly(AnimationState.Idle, false);
                    SetAnimation_CharacterOnly(AnimationState.Retire, true);
                }
                else
                {
                    SetAnimation_Both(AnimationState.Idle, true);
                }
            }
            else
            {
                SetAnimation_Both(AnimationState.Idle, true);
                SetAnimation_CharacterOnly(AnimationState.Complete, true);
            }

            SetAnimation_Both(AnimationState.MoveLeft, false);
            SetAnimation_Both(AnimationState.MoveRight, false);
            SetAnimation_Both(AnimationState.Drive, false);
        }


    }

    private void FixedUpdate_Camera()
    {
        if (IsMineAndNotAI)
        {
            CameraManager.Instance.FixedUpdate_Cam();
        }
    }

    //Timing booster 용 LineRenderer 세팅
    private void FixedUpdate_SetLineRenderer()
    {
        if (lr_inner != null && lr_outer != null)
        {
            if (IsMineAndNotAI
                && network_isMoving
                && isFlipped == false
                && isOutOfBoundary == false
                && isFlying == false
                && isTimingBoosterReady == true)
            {
                var timingBoosterOuterCircle_Speed = ref_boosterTimingComboTimingStartSpeed + ref_boosterTimingComboTimingStartSpeed * (ref_boosterTimingComboTimingSpeedEfficiency - 1f) * timingBoosterCurrentCombo;

                lr_inner.gameObject.SafeSetActive(true);
                lr_outer.gameObject.SafeSetActive(true);

                int cnt = 200;
                float perDegree = 360f / (float)cnt;
                Vector3 arc_dir = transform.forward;

                lr_inner.positionCount = cnt + 1;
                lr_outer.positionCount = cnt + 1;

                float innerWidth = ref_boosterTimingComboTimingTagertWidth;
                lr_inner.startWidth = innerWidth;
                lr_inner.endWidth = innerWidth;

                float innerRadius = ref_boosterTimingComboTimingTagertRadius;
                for (int i = 0; i < lr_inner.positionCount; i++)
                {
                    Quaternion _rot = Quaternion.AngleAxis(perDegree * i, Vector3.up);
                    var _rotatedDir = (_rot * arc_dir).normalized;
                    Vector3 pos = transform.position + _rotatedDir * innerRadius;
                    pos.y = transform.position.y + 0.2f;
                    lr_inner.SetPosition(i, pos);
                }

                float outerWidth = 0.2f;
                lr_outer.startWidth = outerWidth;
                lr_outer.endWidth = outerWidth;

                timingBoosterOuterCircle_CurrRadius -= Runner.DeltaTime * timingBoosterOuterCircle_Speed;
                if (timingBoosterOuterCircle_CurrRadius <= 0f)
                    timingBoosterOuterCircle_CurrRadius = 0f;

                float outerRadius = timingBoosterOuterCircle_CurrRadius;
                for (int i = 0; i < lr_outer.positionCount; i++)
                {
                    Quaternion _rot = Quaternion.AngleAxis(perDegree * i, Vector3.up);
                    var _rotatedDir = (_rot * arc_dir).normalized;
                    Vector3 pos = transform.position + _rotatedDir * outerRadius;
                    pos.y = transform.position.y + 0.22f; //살짝 위쪽에 그려주자...
                    lr_outer.SetPosition(i, pos);
                }

                if (Math.Abs(outerRadius - innerRadius) < innerWidth / 2)
                {
                    if (outerRadius > innerRadius)
                        currentTimingBoosterSuccessType = TimingBoosterSuccessType.Great;
                    else
                        currentTimingBoosterSuccessType = TimingBoosterSuccessType.Perfect;

                    isTimingBoosterActive = true;
                }
                else
                {
                    currentTimingBoosterSuccessType = TimingBoosterSuccessType.None;
                    isTimingBoosterActive = false;

                    //콤보 실패...!
                    if (outerRadius < innerRadius)
                    {
                        timingBoosterCurrentCombo = 0;
                    }
                }

                if (timingBoosterOuterCircle_CurrRadius <= 0f)
                {
                    timingBoosterOuterCircle_CurrRadius = 0f;
                    ResetTimingBooster();
                }
            }
            else
            {
                lr_inner.gameObject.SafeSetActive(false);
                lr_outer.gameObject.SafeSetActive(false);

                isTimingBoosterActive = false;
            }
        }
    }

    //스타팅 부스터용
    private void FixedUpdate_StartingBoosterTimer()
    {
        if (inGameManager.isPlayGame)
            return;

        if (startingBoosterInputTimeCounter > 0)
        {
            if (isStartingBoosterActiavated == false)
                startingBoosterInputTimeCounter -= Runner.DeltaTime;
        }
        else
        {
            isStartingBoosterInputBlocked = false;
            startingBoosterInputTimeCounter = 0f;
        }
    }

    #region Animation

    private void SetAnimation_Both(AnimationState state, bool isOn)
    {
        SetAnimation_CarOnly(state, isOn);
        SetAnimation_CharacterOnly(state, isOn);
    }

    private void SetAnimation_CarOnly(AnimationState state, bool isOn)
    {
        if (playerCar == null ||playerCar.currentCar == null || playerCar.currentCar.animator == null)
            return;

        if (isOn)
            currentCarAnimationState = state;

        var carAnim = playerCar.currentCar.animator;

        switch (state)
        {
            case AnimationState.Idle:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_IDLE(), isOn);
                }
                break;
            case AnimationState.Drive:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_DRIVE(), isOn);
                }
                break;
            case AnimationState.Booster_1:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_BOOSTER_1());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_BOOSTER_1());
                    }
                }
                break;
            case AnimationState.Booster_2:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_BOOSTER_2());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_BOOSTER_2());
                    }
                }
                break;
            case AnimationState.Brake:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_BRAKE(), isOn);
                }
                break;
            case AnimationState.MoveLeft:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_LEFT(), isOn);
                }
                break;
            case AnimationState.MoveRight:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_RIGHT(), isOn);
                }
                break;

            case AnimationState.Flip:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_FLIP());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_FLIP());
                    }
                }
                break;
            case AnimationState.Spin:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_SPIN());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_SPIN());
                    }
                }
                break;
        }
    }

    private void SetAnimation_CharacterOnly(AnimationState state, bool isOn)
    {
        if (playerCharacter == null || playerCharacter.currentCharacter == null || playerCharacter.currentCharacter.animator == null)
            return;

        if (isOn)
            currentCharAnimationState = state;

        var charAnim = playerCharacter.currentCharacter.animator;

        switch (state)
        {
            case AnimationState.Idle:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_IDLE(), isOn);
                }
                break;
            case AnimationState.Drive:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_DRIVE(), isOn);
                }
                break;
            case AnimationState.Booster_1:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_BOOSTER_1());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_BOOSTER_1());
                    }
                }
                break;
            case AnimationState.Booster_2:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_BOOSTER_2());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_BOOSTER_2());
                    }
                }
                break;
            case AnimationState.Brake:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_BRAKE(), isOn);
                }
                break;
            case AnimationState.MoveLeft:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_LEFT(), isOn);
                }
                break;
            case AnimationState.MoveRight:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_RIGHT(), isOn);
                }
                break;
            case AnimationState.Victory:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_VICTORY(), isOn);
                }
                break;
            case AnimationState.Complete:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_COMPLETE(), isOn);
                }
                break;
            case AnimationState.Retire:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_RETIRE(), isOn);
                }
                break;
            case AnimationState.Flip:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_FLIP());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_FLIP());
                    }
                }
                break;
            case AnimationState.Spin:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_SPIN());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_SPIN());
                    }
                }
                break;
        }
    }

    #endregion


    private bool IsWayPointSet()
    {
        if (wg == null)
            return false;

        var list_0 = wg.waypoints_0;
        var list_1 = wg.waypoints_1;
        var list_2 = wg.waypoints_2;
        var list_3 = wg.waypoints_3;
        var list_4 = wg.waypoints_4;
        var list_5 = wg.waypoints_5;
        var list_6 = wg.waypoints_6;

        if (list_0 == null || list_1 == null || list_2 == null || list_3 == null || list_4 == null || list_5 == null || list_6 == null)
            return false;

        if (list_1.Count != list_2.Count || list_1.Count != list_3.Count || list_1.Count != list_4.Count || list_1.Count != list_5.Count)
        {
            network_isMoving = false;
            Debug.Log("Check Your Waypoints settings....!");
            return false;
        }

        return true;
    }


    public Vector3 GetDestination(int moveIndex, LaneType type)
    {
        if (wg == null)
            return Vector3.zero;

        var list_0 = wg.waypoints_0;
        var list_1 = wg.waypoints_1;
        var list_2 = wg.waypoints_2;
        var list_3 = wg.waypoints_3;
        var list_4 = wg.waypoints_4;
        var list_5 = wg.waypoints_5;
        var list_6 = wg.waypoints_6;


        var dest = Vector3.zero;
        switch (type)
        {
            case LaneType.Zero:
                dest = list_0[moveIndex].GetPosition();
                break;
            case LaneType.One:
                dest = list_1[moveIndex].GetPosition();
                break;
            case LaneType.Two:
                dest = list_2[moveIndex].GetPosition();
                break;
            case LaneType.Three:
                dest = list_3[moveIndex].GetPosition();
                break;
            case LaneType.Four:
                dest = list_4[moveIndex].GetPosition();
                break;
            case LaneType.Five:
                dest = list_5[moveIndex].GetPosition();
                break;
            case LaneType.Six:
                dest = list_6[moveIndex].GetPosition();
                break;
            default:
                dest = list_3[moveIndex].GetPosition();
                break;
        }

        return dest;
    }

    public WayPointSystem.Waypoint.WayPointType GetDestinationWaypointType(int moveIndex, LaneType laneType)
    {
        if (wg == null)
            return WayPointSystem.Waypoint.WayPointType.Normal;

        var list = GetWayPointsList(laneType);
        var type = list[moveIndex].currentWayPointType;

        return type;
    }

    public Vector3 GetCurrentDirection()
    {
        Vector3 dir = Vector3.zero;

        //초반에 들리는 현상때문에 시작할때는 dest기준이 아닌 앞을 향해서 가자...!!
        if (network_currentLap == 0 && (client_currentMoveIndex == 0 || client_currentMoveIndex == 1) && isIntroMoving == false)
        {
            dir = transform.TransformDirection(Vector3.forward).normalized;
        }
        else
        {
            if (currentDest.HasValue)
            {
                float lerpSpeed;

                Vector3 destDir = (currentDest.Value - transform.position).normalized;
                var angle = Vector3.Angle(destDir, transform.TransformDirection(Vector3.forward).normalized);

                //속도 기반해서 lerpSpeed 정해주자
                lerpSpeed = Mathf.Lerp(playerDirectionLerpMinSpeed, playerDirectionLerpMaxSpeed, (Mathf.Clamp((client_currentMoveSpeed - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit) / Mathf.Clamp((ref_maxSpeedLimit - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit)));
                //목표지점 angle이 클 경우 추가 보간 하자
                lerpSpeed += Mathf.Lerp(playerDirectionLerpAddiionalAngleMinSpeed, playerDirectionLerpAddiionalAngleMaxSpeed, Mathf.Clamp(1f - ((float)(45 - angle) / 45f), 0f, 1f));

                Vector3 lerpedDir = Vector3.Slerp(currDirection, destDir, Runner.DeltaTime * lerpSpeed);
                dir = lerpedDir.normalized;
            }
            else
                dir = transform.TransformDirection(Vector3.forward).normalized;
        }


        return dir;
    }

    private Vector3 GetFowardDirection()
    {
        return transform.TransformDirection(Vector3.forward).normalized;
    }

    private Vector3 GetUpwardDirection()
    {
        return upwardDirection;
    }

    private void SetDrafting()
    {
        if (IsMine)
        {
            if (isDrafting == false && isFlipped == false && isStunned == false && isShield == false 
                && draftingCoolTimer <= 0f)
            {
                foreach (var i in inGameManager.ListOfPlayers)
                {
                    if (i == null || i.go == null || i.pm == null)
                        continue;

                    if (i.pm.IsMineAndNotAI)
                        continue;

                    if (Vector3.Distance(i.go.transform.position, transform.position) < ref_draftActivationDist
                        && UtilityCommon.IsFront(transform.forward, transform.position, i.go.transform.position)
                        && i.pm.client_currentLaneType == client_currentLaneType)
                    {
                        draftingActivationTimer -= Runner.DeltaTime;

                        if (draftingActivationTimer <= 0)
                        {
                            draftingCoolTimer = ref_draftActivationCooltime;
                            draftingActivationTimer = ref_draftActivateNeedTime;
                            networkInGameRPCManager.RPC_StartDrafting(this.networkPlayerID);
                        }
                    }
                }
            }
            else
            {
                if (draftingCoolTimer > 0f)
                    draftingCoolTimer -= Runner.DeltaTime;
            }
        }
    }

 
    public void SetGroundSpeed(MapObject_Ground.GroundType type, float speed = 0)
    {
        if (network_isEnteredTheFinishLine)
            return;

        if (InGameManager.Instance.isGameEnded)
            return;

        switch (type)
        {
            case MapObject_Ground.GroundType.Mud:
                {

                }
                break;

            case MapObject_Ground.GroundType.None:
            default:
                {

                }
                break;
        }
    }

    private float GetCurrentFowardRotationSpeed()
    {
        float speed = 0f;

        speed = ref_playerRotationFowardSpeed;

        return speed;
    }

    private bool IsFowardRotationLerpActivated()
    {
        Vector3 destDir;
        if (currentDest.HasValue)
            destDir = (currentDest.Value - transform.position).normalized;
        else
            destDir = transform.TransformDirection(Vector3.forward).normalized;

        //Angle이 작으면 lerp하지 말고 동일한 Dir으로 이동시켜주자
        if (Vector3.Angle(previousDir, destDir) < 0.05f)
            return false;
        else
            return true;
    }

    private float GetCurrentUpwardRotationSpeed()
    {
        float speed = 0f;

        speed = ref_playerRotationUpwardSpeed;

        return speed;
    }

    public int previousMoveIndex
    {
        get
        {
            if (client_currentMoveIndex > 0)
            {
                return client_currentMoveIndex - 1;
            }
            else
            {
                if (GetLastIndex() != -1)
                {
                    return GetLastIndex();
                }
                else
                    return 0;
            }
        }
    }


    public float GetBatteryCost(CarBoosterType lv)
    {
        float cost = 0; //비용...! 0미만일경우 소모 0이상일 경우 충전
        switch (lv)
        {
            case CarBoosterType.None:
                cost = 0;
                break;
            case CarBoosterType.CarBooster_Starting:
                cost = 0;
                break;
            case CarBoosterType.CarBooster_LevelOne:
                cost = -ref_boosterLv1Cost;
                break;
            case CarBoosterType.CarBooster_LevelTwo:
                cost = -ref_boosterLv2Cost;
                break;
            case CarBoosterType.CarBooster_LevelThree:
                cost = -ref_boosterLv3Cost;
                break;

            case CarBoosterType.CarBooster_LevelFour_Timing:
                {
                    float timingBoosterCost = Mathf.Max(ref_boosterLv1Cost, ref_boosterLv2Cost, ref_boosterLv3Cost);
                    if (network_currentBattery > timingBoosterCost)
                        timingBoosterCost = network_currentBattery;
                    cost = -timingBoosterCost;
                }
                break;
            default:
                break;
        }

        return cost;
    }

    public float GetBatteryCost(ChargePadBoosterLevel lv)
    {
        float cost = 0; //비용...! 0미만일경우 소모 0이상일 경우 충전
        switch (lv)
        {

            case ChargePadBoosterLevel.ChargePadBooster_One:
                cost = ref_chargePadLv1ChargeAmount;
                break;
            case ChargePadBoosterLevel.ChargePadBooster_Two:
                cost = ref_chargePadLv2ChargeAmount;
                break;
            case ChargePadBoosterLevel.ChargePadBooster_Three:
                cost = ref_chargePadLv3ChargeAmount;
                break;
            default:
                break;
        }

        return cost;
    }

    public void OnRPCEvent_SetCurrentBattery(float battery)
    {
        network_currentBattery = battery;

        if (network_currentBattery >= ref_batteryMax)
            network_currentBattery = ref_batteryMax;
        else if (network_currentBattery <= 0)
            network_currentBattery = 0;
    }

    public void OnRPCEvent_SetCurrentBatteryAdded(float addedBattery)
    {
        network_currentBattery = network_currentBattery + addedBattery;

        if (network_currentBattery >= ref_batteryMax)
            network_currentBattery = ref_batteryMax;
        else if (network_currentBattery <= 0)
            network_currentBattery = 0;
    }

    public void UseBattery(CarBoosterType lv)
    {
        float cost = GetBatteryCost(lv);

        if (IsValidBooster(lv) && IsMine)
        {
            previousBatteryAtBooster = network_currentBattery;
            networkInGameRPCManager.RPC_SetPlayerBattery(networkPlayerID, true, cost, (int)NetworkInGameRPCManager.SetBatteryType.CarBooster);
        }
    }

    public void UseBattery(ChargePadBoosterLevel lv)
    {
        float cost = GetBatteryCost(lv);

        if (IsValidBooster(lv) && IsMine)
        {
            networkInGameRPCManager.RPC_SetPlayerBattery(networkPlayerID, true, cost, (int)NetworkInGameRPCManager.SetBatteryType.ChargePadBooster);
        }
    }

    public bool IsValidBooster(CarBoosterType lv)
    {
        bool isValid;
        float cost = GetBatteryCost(lv);

        if (cost <= 0)
        {
            if (network_currentBattery < -cost) //비용이 더 큰 경우
            {
                isValid = false;
            }
            else
            {
                isValid = true;
            }
        }
        else
        {
            isValid = true;
        }

        //SP 부스터의 경우 변수로 가능여부 체크하자...!
        if (lv == CarBoosterType.CarBooster_LevelFour_Timing)
        {
            if (isTimingBoosterActive == false)
                isValid = false;
            else
                isValid = true;
        }


        return isValid;
    }

    public bool IsValidBooster(ChargePadBoosterLevel lv)
    {
        bool isValid;
        float cost = GetBatteryCost(lv);

        if (cost <= 0)
        {
            if (network_currentBattery < -cost) //비용이 더 큰 경우
            {
                isValid = false;
            }
            else
            {
                isValid = true;
            }
        }
        else
        {
            isValid = true;
        }

        return isValid;
    }

    public CarBoosterType GetAvailableInputBooster()
    {
        if (IsValidBooster(CarBoosterType.CarBooster_LevelFour_Timing))
            return CarBoosterType.CarBooster_LevelFour_Timing;

        if (IsValidBooster(CarBoosterType.CarBooster_LevelThree))
            return CarBoosterType.CarBooster_LevelThree;

        if (IsValidBooster(CarBoosterType.CarBooster_LevelTwo))
            return CarBoosterType.CarBooster_LevelTwo;

        //level 1 의 경우... 특정 상황에서 강제로 켜주기만함... Input에 의해서는 x... 제외시키자
        //StartingBooster 의 경우도 제외


        return CarBoosterType.None;
    }


    public void Car_BoostSpeed(CarBoosterType lv, float battery = 0f, TimingBoosterSuccessType timingBoosterSuccessType = TimingBoosterSuccessType.None)
    {
        if (network_isEnteredTheFinishLine)
            return;

        UtilityCoroutine.StopCoroutine(ref carbooster, this);
        UtilityCoroutine.StartCoroutine(ref carbooster, Car_Booster(lv, battery, timingBoosterSuccessType), this);
    }

    private IEnumerator carbooster = null;
    private IEnumerator Car_Booster(CarBoosterType lv, float battery = 0f, TimingBoosterSuccessType timingBoosterSuccessType = TimingBoosterSuccessType.None)
    {
        client_currentCarBoosterLv = lv;

        bool overrideFXAndAnim = false; //FX효와Anim을 덮어 씌울지... Booster 상태에서 또 Booster걸리는 경우라서... 

        //자동차의 경우는 그냥 무조건 애니메이션 보여주자...
        overrideFXAndAnim = true;

        if (IsMineAndNotAI)
        {
            if (overrideFXAndAnim)
            {
                Camera_Base.Instance.TurnOffCameraBoosterFX();
            }
        }

        float boosterMaxSpeedDuration = 0f;

        switch (client_currentCarBoosterLv)
        {
            case CarBoosterType.None:
                break;
            case CarBoosterType.CarBooster_Starting:
                {
                    boosterMaxSpeedDuration = ref_boosterStartingDurationTime;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentCarBoosterLv);
                        if (IsMineAndNotAI)
                            Camera_Base.Instance.FX_Line_2.SafeSetActive(true);
                        SetAnimation_CharacterOnly(AnimationState.Booster_2, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);
                    }
                }
                break;
            case CarBoosterType.CarBooster_LevelOne:
                {
                    boosterMaxSpeedDuration = ref_boosterLv1DurationTime;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentCarBoosterLv);
                        if (IsMineAndNotAI)
                            Camera_Base.Instance.FX_Line_0.SafeSetActive(true);
                        SetAnimation_CharacterOnly(AnimationState.Booster_1, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);
                    }
                }
                break;
            case CarBoosterType.CarBooster_LevelTwo:
                {
                    boosterMaxSpeedDuration = ref_boosterLv2DurationTime;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentCarBoosterLv);
                        if (IsMineAndNotAI)
                            Camera_Base.Instance.FX_Line_1.SafeSetActive(true);
                        SetAnimation_Both(AnimationState.Booster_2, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);

                        if(IsMineAndNotAI)
                            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Booster_02);
                    }
                }
                break;
            case CarBoosterType.CarBooster_LevelThree:
                {
                    boosterMaxSpeedDuration = ref_boosterLv3DurationTime;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentCarBoosterLv);
                        if (IsMineAndNotAI)
                            Camera_Base.Instance.FX_Line_2.SafeSetActive(true);
                        SetAnimation_Both(AnimationState.Booster_2, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);

                        if (IsMineAndNotAI)
                            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Booster_03);
                    }
                }
                break;
            case CarBoosterType.CarBooster_LevelFour_Timing:
                {
                    timingBoosterBatteryCost = battery;
                    currentTimingBoosterSuccessType = timingBoosterSuccessType;
                    if (timingBoosterSuccessType == TimingBoosterSuccessType.Great)
                    {
                        boosterMaxSpeedDuration = ref_boosterTimingDurationTime + ref_boosterTimingDurationTime * (ref_boosterTimingDurationTimeComboEfficiency - 1f) * timingBoosterCurrentCombo + timingBoosterBatteryCost * ref_boosterGreatTimingDurationTimeConsumedGaugeEfficiency;
                    }
                    else if (timingBoosterSuccessType == TimingBoosterSuccessType.Perfect)
                    {
                        boosterMaxSpeedDuration = ref_boosterTimingDurationTime + ref_boosterTimingDurationTime * (ref_boosterTimingDurationTimeComboEfficiency - 1f) * timingBoosterCurrentCombo + timingBoosterBatteryCost * ref_boosterPerfectTimingDurationTimeConsumedGaugeEfficiency;
                    }

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentCarBoosterLv);
                        if (IsMineAndNotAI)
                            Camera_Base.Instance.FX_Line_3.SafeSetActive(true);
                        SetAnimation_Both(AnimationState.Booster_2, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);

                        if (IsMineAndNotAI)
                            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Booster_03);
                    }
                }
                break;
        }

        if (client_currentCarBoosterLv == CarBoosterType.CarBooster_LevelFour_Timing)
        {
            if (timingBoosterCurrentCombo < timingBoosterMaxCombo)
                timingBoosterCurrentCombo += 1;
        }
        else if (client_currentCarBoosterLv == CarBoosterType.CarBooster_LevelTwo || client_currentCarBoosterLv == CarBoosterType.CarBooster_LevelThree)
        {
            if (isTimingBoosterActive)
                timingBoosterCurrentCombo = 0;
        }

        float timer = 0f;
        while (true)
        {
            timer += Runner.DeltaTime;

            if (timer > boosterMaxSpeedDuration)
                break;

            yield return new WaitForFixedUpdate();
        }

        //Initialize booster settings
        client_currentCarBoosterLv = CarBoosterType.None;
        timingBoosterBatteryCost = 0f;

        while (true)
        {
            if (client_currentChargeBoosterLv == ChargePadBoosterLevel.None
                && client_currentCarBoosterLv == CarBoosterType.None)
                break;

            yield return null;
        }

        DeactivateBooster();
        playerCar.DeactivateAllBoosterFX();

        if (IsMineAndNotAI)
        {
            if (overrideFXAndAnim)
            {
                Camera_Base.Instance.TurnOffCameraBoosterFX();
            }
        }

        if (overrideFXAndAnim)
            SetAnimation_Both(AnimationState.Drive, true);
    }

    public void ChargePad_BoostSpeed(ChargePadBoosterLevel lv)
    {
        if (network_isEnteredTheFinishLine)
            return;

        UtilityCoroutine.StopCoroutine(ref chargePadBooster, this);
        UtilityCoroutine.StartCoroutine(ref chargePadBooster, ChargePad_Booster(lv), this);
    }

    private IEnumerator chargePadBooster = null;
    private IEnumerator ChargePad_Booster(ChargePadBoosterLevel lv)
    {
        client_currentChargeBoosterLv = lv;

        bool overrideFXAndAnim = false;
        if (client_currentCarBoosterLv != CarBoosterType.CarBooster_LevelTwo 
            && client_currentCarBoosterLv != CarBoosterType.CarBooster_LevelThree
            && client_currentCarBoosterLv != CarBoosterType.CarBooster_LevelFour_Timing)
            overrideFXAndAnim = true;

        if (IsMineAndNotAI)
        {
            if (overrideFXAndAnim)
            {
                Camera_Base.Instance.TurnOffCameraBoosterFX();
            }

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.ChargePad);
        }

        float chargePadSpeedDuration = 0f;

        switch (client_currentChargeBoosterLv)
        {
            case ChargePadBoosterLevel.ChargePadBooster_One:
                {
                    chargePadSpeedDuration = ref_chargePadLv1SpeedDurationTime;
                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentChargeBoosterLv);
                        SetAnimation_CharacterOnly(AnimationState.Booster_1, true);
                    }
                }
                break;
            case ChargePadBoosterLevel.ChargePadBooster_Two:
                {
                    chargePadSpeedDuration = ref_chargePadLv2SpeedDurationTime;
                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentChargeBoosterLv);
                        SetAnimation_Both(AnimationState.Booster_1, true);
                    }
                }
                break;
            case ChargePadBoosterLevel.ChargePadBooster_Three:
                {
                    chargePadSpeedDuration = ref_chargePadLv3SpeedDurationTime;
                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(client_currentChargeBoosterLv);
                        SetAnimation_Both(AnimationState.Booster_1, true);
                    }
                }
                break;
        }


        float timer = 0f;
        while (true)
        {
            timer += Runner.DeltaTime;

            if (timer > chargePadSpeedDuration)
                break;

            yield return new WaitForFixedUpdate();
        }

        //Initialize booster settings
        client_currentChargeBoosterLv = ChargePadBoosterLevel.None;

        while (true)
        {
            if (client_currentChargeBoosterLv == ChargePadBoosterLevel.None 
                && client_currentCarBoosterLv == CarBoosterType.None)
                break;

            yield return null;
        }

        DeactivateBooster();
        playerCar.DeactivateAllBoosterFX();

        if (IsMineAndNotAI)
        {
            if (overrideFXAndAnim)
            {
                Camera_Base.Instance.TurnOffCameraBoosterFX();
            }
        }

        if (overrideFXAndAnim)
            SetAnimation_Both(AnimationState.Drive, true);
    }


    public void DeactivateBooster()
    {
        UtilityCoroutine.StopCoroutine(ref chargePadBooster, this);
        UtilityCoroutine.StopCoroutine(ref carbooster, this);
        client_currentChargeBoosterLv = ChargePadBoosterLevel.None;
        client_currentCarBoosterLv = CarBoosterType.None;

        if (IsMineAndNotAI)
            Camera_Base.Instance.TurnOffCameraBoosterFX();

        playerCar.DeactivateAllBoosterFX();
    }

    public void ActivateTimingBooster()
    {
        //이미 켜져 있는게 있으면 무시해주자
        if (isTimingBoosterReady)
        {
            return;
        }

        timingBoosterOuterCircle_CurrRadius = ref_boosterTimingComboTimingStartRadius;
        isTimingBoosterReady = true;
    }

    public void ResetTimingBooster()
    {
        if (isTimingBoosterActive || isTimingBoosterReady)
        {
            isTimingBoosterActive = false;
            isTimingBoosterReady = false;
        }
    }

    public void StartSheild()
    {
        UtilityCoroutine.StopCoroutine(ref sheild, this);
        UtilityCoroutine.StartCoroutine(ref sheild, Shield(), this);
    }

    private IEnumerator sheild = null;
    private IEnumerator Shield()
    {
        DeactivateBooster();

        playerCar.ActivateShieldFX();

        if (IsMineAndNotAI)
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.ShieldOn);

        float timer = ref_shieldTime;
        while (true)
        {
            //PLAYER_PARRYING_TIME 이 0보다 작으면 무한정 켜주자.... isDecelerating == false 될때 까지 ㅎ
            if (ref_shieldTime > 0)
            {
                timer -= Runner.DeltaTime;

                if (timer < 0)
                    break;
            }

            yield return new WaitForFixedUpdate();
        }

        if (IsMine)
            networkInGameRPCManager.RPC_StopShield(this.networkPlayerID);
    }

    public void StopShield()
    {
        if (isShield == false)
            return;

        UtilityCoroutine.StopCoroutine(ref sheild, this);
        playerCar.DeactivateShieldFX();

        if (IsMineAndNotAI)
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.ShieldOff);

        if (ref_shieldCooltime > 0)
        {
            if (isShieldCooltime == false)
            {
                isShieldCooltime = true;
                UtilityCoroutine.StopCoroutine(ref setParryingCoolTime, this);
                UtilityCoroutine.StartCoroutine(ref setParryingCoolTime, SetParryingCoolTime(), this);
            }
        }
        else
            isShieldCooltime = false;

        isShield = false;
    }

    private IEnumerator setParryingCoolTime = null;
    private IEnumerator SetParryingCoolTime()
    {
        network_shieldCooltimeLeftTime = ref_shieldCooltime;

        while (true)
        {
            network_shieldCooltimeLeftTime -= Runner.DeltaTime;

            if (network_shieldCooltimeLeftTime <= 0)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        isShieldCooltime = false;
    }

    public float GetParryingCoolTimeLeftRate()
    {
        if (network_shieldCooltimeLeftTime > 0)
        {
            return network_shieldCooltimeLeftTime / ref_shieldCooltime;
        }
        else
            return 0f;
    }


    //서버 값 <-> 클라이언트 값 맞춰 주자...!
    public bool IsPositionLagging()
    {
        var clientPosition = transform.position + currDirection * Runner.DeltaTime * client_currentMoveSpeed;
        var networkPosition = this.network_position;

        var diff = Vector3.Distance(clientPosition, networkPosition);

#if UNITY_EDITOR
        if (IsLagTestDebugOn == true) //인스팩터상에서 체크해주자
        {
            if (diff >= lagMinDistance)
                Debug.Log("<color=red>DIFF- " + System.Math.Round(diff, 4) + "</color>");
            else
                Debug.Log("DIFF- " + System.Math.Round(diff, 4));
        }
#endif

        if (diff > lagMinDistance)
        {
            SetLagDistQueue(diff);
            return true;
        }
        else
        {
            ClearLagDistQueue();
            return false;
        }
    }

    private Queue<float> LagDistContainer = new Queue<float>();

    public void SetLagDistQueue(float diff)
    {
        int maxCount = 50; //거의 1초마다 체크
        if (LagDistContainer.Count >= maxCount)
            LagDistContainer.Dequeue();

        LagDistContainer.Enqueue(diff);
    }

    public void ClearLagDistQueue()
    {
        if (LagDistContainer.Count > 0)
        {
            LagDistContainer.Clear();
        }
    }

    private float GetAverageLagDist()
    { 
        float avg = 0f;
        foreach (var i in LagDistContainer)
            avg += i;

        avg = avg / LagDistContainer.Count;

        return avg;
    }

    public float GetPlayerLagAdjustRate(float currentLagDist)
    {
        float lagRate = 0f;

        if (lagMinDistance >= lagMaxDistance
            || lagMinRate >= lagMaxRate)
            return lagMinRate;

        currentLagDist = Mathf.Clamp(currentLagDist, lagMinDistance, lagMaxDistance);

        float percentage = ((currentLagDist - lagMinDistance) / (lagMaxDistance - lagMinDistance));

        lagRate = lagMinRate + (lagMinRate / (lagMaxRate - lagMinRate)) * percentage;
        lagRate = Mathf.Clamp(lagRate, lagMinRate, lagMaxRate);


        float adjustRate;
        float adjustRate_Min = 1f;
        float adjustRate_Max = lagPingAdjustmentRate;

        var avgPing = PhotonNetworkManager.Instance.GetAveragePing();

        float value = adjustRate_Max - ((avgPing - PhotonNetworkManager.GOOD_PING_LIMIT) / (PhotonNetworkManager.GOOD_PING_LIMIT) * (adjustRate_Max - adjustRate_Min));
        adjustRate = Math.Clamp(value, adjustRate_Min, adjustRate_Max);
        lagRate *= adjustRate;

        if (lagUseAverageLagDist == true)
        {
            //lagrate으로 조절 해줬는데 오히려 더 lag dist가 늘어나는 경우를 위해 추가 보정
            if (currentLagDist > GetAverageLagDist())
            {
#if UNITY_EDITOR
                if (IsLagTestDebugOn == true)
                    Debug.Log("currentLagDist - GetAverageLagDist(): " + (currentLagDist - GetAverageLagDist()));
#endif
                //currentLagDist - GetAverageLagDist()은 대략 0.1~1 사이 값에 해당 (값이 클수록 Lag dist 값이 늘어난경우)
                var addRate = 1 - (currentLagDist - GetAverageLagDist());
                addRate = Math.Clamp(addRate, 0f, 1f);
                lagRate *= addRate; //addRate값이 작을 수록 lagRate로 조절하는 값 크기가 작아짐
            }
        }

#if UNITY_EDITOR
        if (IsLagTestDebugOn == true) //인스팩터상에서 체크해주자
            Debug.Log("percentage: " + percentage + "   lagRate: " + lagRate);
#endif

        return lagRate;
    }

    public float GetLagPositionAdjustRate()
    {
        float lagRate;

        float lagRate_Min = lagPositionAdjustmentRate_Min;
        float lagRate_Max = lagPositionAdjustmentRate_Max;

        var avgPing = PhotonNetworkManager.Instance.GetAveragePing();

        float value = lagRate_Max - ((avgPing - PhotonNetworkManager.GOOD_PING_LIMIT) / (PhotonNetworkManager.GOOD_PING_LIMIT) * (lagRate_Max - lagRate_Min));
        value *= lagPositionAdjustmentRate_Mutiplier;
        lagRate = Math.Clamp(value, lagRate_Min, lagRate_Max) ;

        return lagRate;
    }


    //UI 노출될 MoveSpeed 수치
    public double GetShowMoveSpeed()
    {
        double speed = 0f;

        if (rgbdy != null)
        {
            if (isFlipped == false && isOutOfBoundary == false)
                speed = System.Math.Round(client_currentMoveSpeed, 1);
            else
                speed = 0;
        }

        return speed * ref_playerShowMoveSpeedMultiplier;
    }

    public List<WayPointSystem.Waypoint> GetCurrentWayPoints()
    {
        if (wg == null)
            return null;

        var dest = Vector3.zero;
        switch (client_currentLaneType)
        {
            case LaneType.Zero:
                return wg.waypoints_0;
            case LaneType.One:
                return wg.waypoints_1;
            case LaneType.Two:
                return wg.waypoints_2;
            case LaneType.Three:
                return wg.waypoints_3;
            case LaneType.Four:
                return wg.waypoints_4;
            case LaneType.Five:
                return wg.waypoints_5;
            case LaneType.Six:
                return wg.waypoints_6;
            default:
                return null;
        }

        return null;
    }

    public List<WayPointSystem.Waypoint> GetWayPointsList(LaneType type)
    {
        if (wg == null)
            return null;

        var dest = Vector3.zero;
        switch (type)
        {
            case LaneType.Zero:
                return wg.waypoints_0;
            case LaneType.One:
                return wg.waypoints_1;
            case LaneType.Two:
                return wg.waypoints_2;
            case LaneType.Three:
                return wg.waypoints_3;
            case LaneType.Four:
                return wg.waypoints_4;
            case LaneType.Five:
                return wg.waypoints_5;
            case LaneType.Six:
                return wg.waypoints_6;
            default:
                return null;
        }

        return null;
    }

    public Vector3? GetWayPointPosition(LaneType type, int index)
    {
        if (wg == null)
            return null;

        List<WayPointSystem.Waypoint> wpg = null;

        var dest = Vector3.zero;
        switch (type)
        {
            case LaneType.Zero:
                wpg = wg.waypoints_0;
                break;
            case LaneType.One:
                wpg = wg.waypoints_1;
                break;
            case LaneType.Two:
                wpg = wg.waypoints_2;
                break;
            case LaneType.Three:
                wpg = wg.waypoints_3;
                break;
            case LaneType.Four:
                wpg = wg.waypoints_4;
                break;
            case LaneType.Five:
                wpg = wg.waypoints_5;
                break;
            case LaneType.Six:
                wpg = wg.waypoints_6;
                break;
            default:
                return null;
        }

        if (index < wpg.Count)
            return wpg[index].GetPosition();

        return null;
    }


    public Vector3 GetCurrentLaneDirection()
    {
        Vector3 dir = Vector3.zero;

        var currWay = GetCurrentWayPoints();
        if (currWay != null && client_currentMoveIndex < currWay.Count && client_currentMoveIndex >= 0)
        {
            var pos1 = currWay[GetNextMoveIndex(client_currentLaneType, client_currentMoveIndex)].GetPosition();
            var pos2 = currWay[client_currentMoveIndex].GetPosition();

            dir = (pos1 - pos2).normalized;
        }


        return dir;
    }

    //조종하는 내 플레이어와의 거리
    public float GetDistBetweenMyPlayer()
    {
        if (IsMine)
            return 0f;
        else
        {
            var myPlayer = InGameManager.Instance.myPlayer;
            if (myPlayer != null)
            {
                return Vector3.Distance(transform.position, myPlayer.transform.position);
            }
        }

        return 0f;
    }

    //Lap 종료때까지 남은 최소 거리
    public float GetMinDistLeft()
    {
        if (wg == null)
            return 0f;

        float distLeft = 0f;
        List<WayPointSystem.Waypoint> list = new List<WayPointSystem.Waypoint>();

        //첫번째 point
        switch (client_currentLaneType)
        {
            case LaneType.Zero:
                list.Add(wg.waypoints_0[client_currentMoveIndex]);
                break;
            case LaneType.One:
                list.Add(wg.waypoints_1[client_currentMoveIndex]);
                break;
            case LaneType.Two:
                list.Add(wg.waypoints_2[client_currentMoveIndex]);
                break;
            case LaneType.Three:
                list.Add(wg.waypoints_3[client_currentMoveIndex]);
                break;
            case LaneType.Four:
                list.Add(wg.waypoints_4[client_currentMoveIndex]);
                break;
            case LaneType.Five: 
                list.Add(wg.waypoints_5[client_currentMoveIndex]);
                break;
            case LaneType.Six:
                list.Add(wg.waypoints_6[client_currentMoveIndex]);
                break;
            default:
                break;
        }

        //중간 경로 point들
        if (client_currentMoveIndex < wg.waypoints_3.Count)
        {
            for (int i = client_currentMoveIndex + 1; i < wg.waypoints_3.Count; i++)
            {
                list.Add(wg.waypoints_3[i]);
            }
        }

        //마지막 point
        list.Add(wg.waypoints_3[0]);

        for (int i = 0; i < list.Count - 1; i++)
        {
            var p1 = list[i].GetPosition();
            var p2 = list[i + 1].GetPosition();

            var dist = Vector3.Distance(p1, p2);

            distLeft += dist;
        }


        return distLeft;
    }

    public float GetColliderLength() //길이
    {
        if (collisionChecker_Player != null)
            return collisionChecker_Player.coll.bounds.size.z;
        else
            return 0f;
    }

    public float GetColliderWidth() //폭
    {
        if (collisionChecker_Player != null)
            return collisionChecker_Player.coll.bounds.size.x;
        else
            return 0f;
    }

    public float GetColliderHeight() //높이
    {
        if (collisionChecker_Player != null)
            return collisionChecker_Player.coll.bounds.size.y;
        else
            return 0f;
    }

    public float GetCurrentBatteryPercent()
    {
        if (ref_batteryMax > 0)
            return GetCurrentBatteryRate() * ref_batteryMax;
        else
            return 0;
    }

    public float GetCurrentBatteryRate()
    {
        if (ref_batteryMax > 0)
            return (float)network_currentBattery / ref_batteryMax;
        else
            return 0;
    }

    public List<InGameManager.PlayerInfo> listOfPlayerCollided = new List<InGameManager.PlayerInfo>();

    private void OnCollisionEnter(Collision collision)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (useIsOutOfBoundary == true)
        {
            if (collision.collider.CompareTag(CommonDefine.TAG_OutOfBound))
            {
                if (isOutOfBoundary == false)
                {
                    if (IsMine)
                        networkInGameRPCManager.RPC_SetOutOfBoundary(networkPlayerID);

                    //InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_01, collision.GetContact(0).point);
                }
            }
        }

        if (IsMine)
        {
            if (collision.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer))
            {
                var pm = collision.gameObject.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    if (pm.networkPlayerID != networkPlayerID)
                    {
                        if (triggerChecker_Body != null && triggerChecker_Body.listOfCollision != null) 
                        {
                            if (triggerChecker_Body.listOfCollision.Contains(pm) == false)
                                triggerChecker_Body.listOfCollision.Add(pm);
                        }

                        if (IsMine)
                            InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_03, collision.GetContact(0).point);
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (useIsOutOfBoundary == true)
        {
            if (collision.collider.CompareTag(CommonDefine.TAG_OutOfBound))
            {
                if (isOutOfBoundary == false)
                {
                    if (IsMine)
                        networkInGameRPCManager.RPC_SetOutOfBoundary(networkPlayerID);
                }
            }
        }
            

        if (IsMine)
        {
            if (collision.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer))
            {
                var pm = collision.gameObject.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    if (pm.networkPlayerID != networkPlayerID)
                    {
                        if (triggerChecker_Body != null && triggerChecker_Body.listOfCollision != null)
                        {
                            if (triggerChecker_Body.listOfCollision.Contains(pm) == false)
                                triggerChecker_Body.listOfCollision.Add(pm);
                        }
                    }
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (IsMine)
        {
            if (collision.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer))
            {
                var pm = collision.gameObject.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    if (pm.networkPlayerID != networkPlayerID)
                    {

                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (useIsOutOfBoundary == true)
        {
            if (other.CompareTag(CommonDefine.TAG_OutOfBound))
            {
                if (isOutOfBoundary == false)
                {
                    if (IsMine)
                        networkInGameRPCManager.RPC_SetOutOfBoundary(networkPlayerID);
                }
            }
        }
    }

    public void OnRPCEvent_SetCurrentLaneNumberTypeAndMoveIndex(LaneType type, int moveIndex)
    {
        if (IsValidDestinationIncludingOutOfBoundary(type, moveIndex) == false)
        {
            Debug.Log("<color=red>InValid Lane...! Cannot move to that Lane!!!</color>");
            return;
        }


        client_previousLaneType = client_currentLaneType;

        if (IsMine == false)
        {
            if (moveIndex >= client_currentMoveIndex)
                client_currentMoveIndex = moveIndex;
        }
        else
        {
            client_currentMoveIndex = moveIndex;
        }

        switch (type)
        {
            case LaneType.Zero:
                {
                    client_currentLaneType = LaneType.Zero;
                }
                break;
            case LaneType.One:
                {
                    client_currentLaneType = LaneType.One;
                }
                break;
            case LaneType.Two:
                {
                    client_currentLaneType = LaneType.Two;
                }
                break;
            case LaneType.Three:
                {
                    client_currentLaneType = LaneType.Three;
                }
                break;
            case LaneType.Four:
                {
                    client_currentLaneType = LaneType.Four;
                }
                break;
            case LaneType.Five:
                {
                    client_currentLaneType = LaneType.Five;
                }
                break;
            case LaneType.Six:
                {
                    client_currentLaneType = LaneType.Six;
                }
                break;
        }

        network_currentLaneType = client_currentLaneType;
        network_currentMoveIndex = client_currentMoveIndex;

        if (client_previousLaneType != client_currentLaneType
            && client_previousLaneType != LaneType.None
            && client_currentLaneType != LaneType.None)
        {
            UtilityCoroutine.StartCoroutine(ref moveLine, MoveLine(), this);
            network_isChangingLane = true;
        }
    }

    public void OnRPCEvent_ActivateChargePad(NetworkId playerID, int id, MapObject_ChargePad.ChargePadLevel currentLv)
    {
        ChargePadBoosterLevel boosterlv = ChargePadBoosterLevel.None;
        switch (currentLv)
        {
            case MapObject_ChargePad.ChargePadLevel.One:
                {
                    boosterlv = ChargePadBoosterLevel.ChargePadBooster_One;
                }
                break;
            case MapObject_ChargePad.ChargePadLevel.Two:
                {
                    boosterlv = ChargePadBoosterLevel.ChargePadBooster_Two;
                }
                break;
            case MapObject_ChargePad.ChargePadLevel.Three:
                {
                    boosterlv = ChargePadBoosterLevel.ChargePadBooster_Three;
                }
                break;
        }

        ActivateChargePadFX(currentLv);
        UseBattery(boosterlv);
        ChargePad_BoostSpeed(boosterlv);
    }


    private void ActivateChargePadFX(MapObject_ChargePad.ChargePadLevel padLv)
    {
        switch (padLv)
        {
            case MapObject_ChargePad.ChargePadLevel.One:
                {
                    InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Charging_Once_Blue, transform.position, 1f, transform);
                }
                break;
            case MapObject_ChargePad.ChargePadLevel.Two:
                { 
                    InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Charging_Once_Yellow, transform.position, 1f, transform);
                }
                break;
            case MapObject_ChargePad.ChargePadLevel.Three:
                {
                    InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Charging_Once_Red, transform.position, 1f, transform);
                }
                break;
        }
    }

    //강제로 Valid한 곳으로 이동
    public void MovePlayerToValidPosition()
    {
        if (IsMine == false)
            return;

        var validLane = GetClosestValidLane();
        var validMoveIndex = client_currentMoveIndex;


        if (validLane != LaneType.None)
            networkInGameRPCManager.RPC_SpawnPlayerToPosition(networkPlayerID, (int)validLane, validMoveIndex);
        else
            Debug.Log("<color=red>Error....! No Valid Place......???? </color>");
    }

    private LaneType GetClosestValidLane()
    {
        float dist = float.MaxValue;
        LaneType validLane = LaneType.None;
        for (int i = 0; i <= (int)LaneType.Six; i++)
        {
            if (IsValidDestination((LaneType)i, client_currentMoveIndex) && GetWayPointPosition((LaneType)i, client_currentMoveIndex).HasValue)
            {
                var tempDist = Vector3.Distance(player.transform.position, GetWayPointPosition((LaneType)i, client_currentMoveIndex).Value);

                if (tempDist < dist)
                {
                    dist = tempDist;
                    validLane = (LaneType)i;
                }
            }
        }

        return validLane;
    }

    private LaneType GetLeftValidLaneWhenHit()
    {
        //currentLaneType 기준 왼쪽 Lane 
        //Valid한게 없으면 None 반환

        if (client_currentLaneType == LaneType.Zero || client_currentLaneType == LaneType.None)
            return LaneType.None;

        var curr = LaneType.None;

        if (IsValidDestinationIncludingOutOfBoundary((client_currentLaneType - 1), client_currentMoveIndex)
            && IsValidLeftRightMove(client_currentLaneType, client_currentMoveIndex))
        {
            curr = (client_currentLaneType - 1);
        }

        return curr;
    }

    private LaneType GetRightValidLaneWhenHit()
    {
        //currentLaneType 기준 오른쪽 Lane 
        //Valid한게 없으면 None 반환

        if (client_currentLaneType == LaneType.Six || client_currentLaneType == LaneType.None)
            return LaneType.None;

        var curr = LaneType.None;

        if (IsValidDestinationIncludingOutOfBoundary((client_currentLaneType + 1), client_currentMoveIndex)
            && IsValidLeftRightMove(client_currentLaneType, client_currentMoveIndex))
        {
            curr = (client_currentLaneType + 1);
        }

        return curr;
    }


    public void OnRPCEvent_SpawnPlayerToPosition(int lane, int moveIndex)
    {
        SetRigidbodyConstraints(ConstraintType.XY);
        isOutOfBoundary = false;
        isFlipped = false;
        isFlippingUp = false;
        outOfBoundaryTimer = 0f;
        outOfBoundaryMoveSpeed = GetCurrentMoveSpeed();

        client_currentLaneType = (LaneType)lane;
        client_currentMoveIndex = moveIndex;

        if (IsMineAndNotAI)
            Camera_Base.Instance.TurnOffCameraAllFX();
        playerCar.DeactivateAllBoosterFX();

        //Spawn 할때는 transform.position 활용하자...! rbdy.position 활용x
        if (GetWayPointPosition(client_currentLaneType, client_currentMoveIndex).HasValue)
            transform.position = GetWayPointPosition(client_currentLaneType, client_currentMoveIndex).Value;

        upwardDirection = Vector3.up; //꼭 초기화 해주자...
        rgbdy.rotation = Quaternion.LookRotation(GetCurrentDirection(), Vector3.up);
        SetGroundType(TrackType.Normal);
    }

    public void OnEvent_PlayerIsInFrontTrue(NetworkId otherPlayerID)
    {
        if (triggerChecker_Front.listOfCollision.Count > 0)
        {
            IsPlayerInFront = true;
            playerInFrontViewID = otherPlayerID;
        }
    }

    public void OnRPCEvent_PlayerIsInFrontFalse()
    {
        if (triggerChecker_Front.listOfCollision.Count == 0)
        {
            IsPlayerInFront = false;
        }
    }

    //피격 받은 입장의 RPC 

    public void OnRPCEvent_TriggerEnterOtherPlayer(NetworkId opponentNetworkID, int triggerParts)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;

        //other networkID에 의해 공격(?) 당한 경우
         
        if (IsMineAndNotAI)
            Vibration.Vibrate((long)150);

        var parts = (PlayerTriggerChecker.CheckParts)triggerParts;

        switch (parts)
        {
            case PlayerTriggerChecker.CheckParts.Left:
                {
                    //실제로 당하는 입장에서 왼쪽에서 충돌을 했는지 확인
                    var other = triggerChecker_Left.listOfCollision.Find(x => x.networkPlayerID.Equals(opponentNetworkID));
                    if (other != null)
                    {
                        if (isShield) //방어 중인 경우
                        {
                            //상대방 실패 먹어주기
                            if (other.isFlipped == false && other.network_isMoving && other.isOutOfBoundary == false)
                            {
                                networkInGameRPCManager.RPC_GetFlipped(other.networkPlayerID, other.transform.position, other.transform.rotation);

                                //나는 바태리 버프 받기
                                networkInGameRPCManager.RPC_GetBatteryBuff(networkPlayerID, ref_batteryDefenseSuccess);

                                //실드 상태 해제
                                networkInGameRPCManager.RPC_StopShield(this.networkPlayerID);

                                //동시에 collision 일어나는 현상 제거를 위해...!
                                //방어가 성공할 경우 list에서 제외 & collider의 경우 바로 꺼주자...
                                other.collisionChecker_Player.enabled = false;
                                if (triggerChecker_Left.listOfCollision.Contains(other))
                                    triggerChecker_Left.listOfCollision.Remove(other);

                                if (IsMineAndAI)
                                    SuccessInBlockingOtherPlayer_AI(parts);
                            }
                        }
                        else //방어x 공격 당한경우
                        {
                            if (isFlipped == false && network_isMoving && isOutOfBoundary == false)
                            {
                                //왼쪽에서 공격을 당했으니 오른쪽으로 밀어주자
                                if (GetRightValidLaneWhenHit() != LaneType.None)
                                {
                                    var newLane = GetRightValidLaneWhenHit();
                                    networkInGameRPCManager.RPC_SetLaneAndMoveIndex(networkPlayerID, (int)newLane, client_currentMoveIndex);
                                    networkInGameRPCManager.RPC_GetStuned(networkPlayerID, ref_stunTime);

                                    //상대방 바태리 버프 받기
                                    networkInGameRPCManager.RPC_GetBatteryBuff(other.networkPlayerID, other.ref_batteryAttackSuccess);
                                }
                                else
                                {
                                    //갈곳 없으면 그냥 뒤집어 주자
                                    networkInGameRPCManager.RPC_GetFlipped(networkPlayerID, transform.position, transform.rotation);

                                    //상대방 바태리 버프 받기
                                    networkInGameRPCManager.RPC_GetBatteryBuff(other.networkPlayerID, other.ref_batteryAttackSuccess);
                                }
                            }
                        }
                    }
                }
                break;

            case PlayerTriggerChecker.CheckParts.Right:
                {
                    //실제로 당하는 입장에서 오른쪽에서 충돌을 했는지 확인
                    var other = triggerChecker_Right.listOfCollision.Find(x => x.networkPlayerID.Equals(opponentNetworkID));
                    if (other != null)
                    {
                        if (isShield) //방어 중인 경우
                        {
                            //상대방 살패 먹어주기
                            if (other.isFlipped == false && other.network_isMoving && other.isOutOfBoundary == false)
                            {
                                networkInGameRPCManager.RPC_GetFlipped(other.networkPlayerID, other.transform.position, other.transform.rotation);

                                //나는 바태리 버프 받기
                                networkInGameRPCManager.RPC_GetBatteryBuff(networkPlayerID, ref_batteryDefenseSuccess);

                                //실드 상태 해제
                                networkInGameRPCManager.RPC_StopShield(this.networkPlayerID);

                                //동시에 collision 일어나는 현상 제거를 위해...!
                                //방어가 성공할 경우 list에서 제외 & collider의 경우 바로 꺼주자...
                                other.collisionChecker_Player.enabled = false;
                                if (triggerChecker_Right.listOfCollision.Contains(other))
                                    triggerChecker_Right.listOfCollision.Remove(other);

                                if (IsMineAndAI)
                                    SuccessInBlockingOtherPlayer_AI(parts);
                            }
                        }
                        else //방어x 공격 당한경우
                        {
                            if (isFlipped == false && network_isMoving && isOutOfBoundary == false)
                            {
                                //오른쪽에서 공격을 당했으니 왼쪽으로 밀어주자
                                if (GetLeftValidLaneWhenHit() != LaneType.None)
                                {
                                    var newLane = GetLeftValidLaneWhenHit();
                                    networkInGameRPCManager.RPC_SetLaneAndMoveIndex(networkPlayerID, (int)newLane, client_currentMoveIndex);
                                    networkInGameRPCManager.RPC_GetStuned(networkPlayerID, ref_stunTime);

                                    //상대방 바태리 버프 받기
                                    networkInGameRPCManager.RPC_GetBatteryBuff(other.networkPlayerID, other.ref_batteryAttackSuccess);
                                }
                                else
                                {
                                    //갈곳 없으면 그냥 뒤집어 주자
                                    networkInGameRPCManager.RPC_GetFlipped(networkPlayerID, transform.position, transform.rotation);

                                    //상대방 바태리 버프 받기
                                    networkInGameRPCManager.RPC_GetBatteryBuff(other.networkPlayerID, other.ref_batteryAttackSuccess);
                                }
                            }
                        }
                    }
                }
                break;

            case PlayerTriggerChecker.CheckParts.Back:
            case PlayerTriggerChecker.CheckParts.Body:
                {
                    //실제로 당하는 입장에서 Back/Body에서 충돌을 했는지 확인
                    PlayerMovement other = null;
                    if (parts == PlayerTriggerChecker.CheckParts.Back)
                    {
                        other = triggerChecker_Back.listOfCollision.Find(x => x.networkPlayerID.Equals(opponentNetworkID));
                        if (other == null) //없으면 body도 체크해보자...!
                            other = triggerChecker_Body.listOfCollision.Find(x => x.networkPlayerID.Equals(opponentNetworkID));
                    }
                    else if (parts == PlayerTriggerChecker.CheckParts.Body)
                    {
                        other = triggerChecker_Body.listOfCollision.Find(x => x.networkPlayerID.Equals(opponentNetworkID));
                        if (other == null) //없으면 back도 체크해보자...!
                            other = triggerChecker_Back.listOfCollision.Find(x => x.networkPlayerID.Equals(opponentNetworkID));
                    }

                    if (other != null)
                    {
                        if (isShield)//방어 중인 경우
                        {
                            //상대방 실패 먹어주기
                            if (other.isFlipped == false && other.network_isMoving && other.isOutOfBoundary == false)
                            {
                                networkInGameRPCManager.RPC_GetFlipped(other.networkPlayerID, other.transform.position, other.transform.rotation);
                                //networkInGameRPCManager.RPC_GetStuned(other.networkPlayerID, other.PLAYER_STUN_TIME);

                                //나는 바태리 버프 받기
                                networkInGameRPCManager.RPC_GetBatteryBuff(networkPlayerID, ref_batteryDefenseSuccess);

                                //실드 상태 해제
                                networkInGameRPCManager.RPC_StopShield(this.networkPlayerID);

                                //동시에 collision 일어나는 현상 제거를 위해...!
                                //방어가 성공할 경우 list에서 제외 & collider의 경우 바로 꺼주자...
                                other.collisionChecker_Player.enabled = false;
                                if (triggerChecker_Back.listOfCollision.Contains(other))
                                    triggerChecker_Back.listOfCollision.Remove(other);
                                if (triggerChecker_Body.listOfCollision.Contains(other))
                                    triggerChecker_Body.listOfCollision.Remove(other);

                                if (IsMineAndAI)
                                    SuccessInBlockingOtherPlayer_AI(parts);
                            }
                        }
                        else
                        {
                            //내가 Flip 먹어버리기...
                            if (isFlipped == false && network_isMoving && isOutOfBoundary == false)
                            {
                                networkInGameRPCManager.RPC_GetFlipped(networkPlayerID, transform.position, transform.rotation);

                                //상대방 바태리 버프 받기
                                networkInGameRPCManager.RPC_GetBatteryBuff(other.networkPlayerID, other.ref_batteryAttackSuccess);
                            }
                        }
                    }
                }
                break;
        }
    }

    public void OnRPCEvent_GetStuned(float stunTime, bool useSpinAnim = true)
    {
        UtilityCoroutine.StartCoroutine(ref getStunned, GetStunned(stunTime, useSpinAnim), this);
    }

    private IEnumerator getStunned = null;

    private IEnumerator GetStunned(float stunTime, bool useSpinAnim = true)
    {
        DeactivateBooster();

        playerCar.DeactivateStunFX();
        playerCar.ActivateStunFX();

        float timer = 0;

        if (stunTime < 0)
            timer = ref_stunTime;
        else
            timer = stunTime;

        isStunned = true;
        //additionalStunSpeed = -4f;

        if (useSpinAnim)
            SetAnimation_Both(AnimationState.Spin, true);

        if (IsMineAndNotAI)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Stun);
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Hit);
            Vibration.Vibrate((long)500);
        }

        while (true)
        {
            timer -= Runner.DeltaTime;

            if (timer < 0)
                break;

            yield return new WaitForFixedUpdate();
        }

        isStunned = false;

        SetAnimation_Both(AnimationState.Drive, true);

        playerCar.DeactivateStunFX();
    }

    public void OnRPCEvent_GetBatteryBuff(float battery)
    {
        networkInGameRPCManager.RPC_SetPlayerBattery(networkPlayerID, true, battery);
    }

    public void OnRPCEvent_SetOutOfBoundary()
    {
        if (network_isEnteredTheFinishLine == true)
            return;

        isOutOfBoundary = true;
    }

    public void OnRPCEvent_StartDraft()
    {
        UtilityCoroutine.StartCoroutine(ref startDraft, StartDraft(), this);
    }

    private IEnumerator startDraft = null;

    private IEnumerator StartDraft()
    {
        isDrafting = true;

        playerCar.DeactivateDraftFX();
        playerCar.ActivateDraftFX();

        float timer = 0f;
        while (true)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                yield break;

            if (Runner != null)
                timer += Runner.DeltaTime;
            
            yield return null;

            if (timer > ref_draftRemainTime)
                break;
        }

        isDrafting = false;

        playerCar.DeactivateDraftFX();

        yield break;
    }

    public void OnRPCEvent_GetFlipped(Vector3 pos, Quaternion rot)
    {
        if (IsValidFlip() == false)
            return;

        UtilityCoroutine.StartCoroutine(ref getFlipped, GetFlipped(pos, rot), this);
    }

    private IEnumerator getFlipped = null;

    private IEnumerator GetFlipped(Vector3 pos, Quaternion rot)
    {
        if (IsValidFlip() == false)
            yield break;

        DeactivateBooster();

        playerCar.DeactivateStunFX();
        playerCar.ActivateStunFX();

        float timer = ref_flipUpTime;
        float timer_2 = ref_flipGroundDelayTime;

        isFlipped = true;
        isFlippingUp = true;
        rgbdy.useGravity = false;

        collisionChecker_Player.enabled = false;

        SetAnimation_Both(AnimationState.Flip, true);

        if (IsMineAndNotAI)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Hit);
            Vibration.Vibrate((long)500);
        }

        while (true)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                yield break;

            timer -= Runner.DeltaTime;

            if (timer > ref_flipUpTime / 2)
                isFlippingUp = true;
            else
                isFlippingUp = false;

            yield return new WaitForFixedUpdate();

            if (timer < 0)
                break;

            if (IsMine == false)
            {
                if (isFlipped == false && isFlippingUp == false)
                    break;
            }

            //바닥에 도착하면... 제한 빨리 출어주자
            if (isFlippingUp == false && isGrounded == true)
            {
                timer_2 -= Runner.DeltaTime;

                if (timer_2 < 0)
                    break;
            }
        }

        isFlipped = false;
        isFlippingUp = false;
        collisionChecker_Player.enabled = true;
        rgbdy.useGravity = true;

        if (IsMine)
        {
            var newLane = GetClosestValidLane();
            var checkIndex = (int)System.Math.Truncate(Mathf.Lerp(1, 3, (Mathf.Clamp((client_currentMoveSpeed - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit) / Mathf.Clamp((ref_maxSpeedLimit - ref_defaultMaxSpeed), 0f, ref_maxSpeedLimit))));

            //다음 인덱스로 가자...
            int newMoveIndex = GetNewMoveIndexAtRange(client_currentMoveIndex, newLane, transform.position, ref_playerMoveIndexCheckMinRange, checkIndex);
            if (newMoveIndex != client_currentMoveIndex || IsLastIndex(newLane, newMoveIndex) == true)
                client_currentMoveIndex = newMoveIndex;

            if (IsValidDestinationIncludingOutOfBoundary(newLane, client_currentMoveIndex))
                networkInGameRPCManager.RPC_SetLaneAndMoveIndex(networkPlayerID, (int)newLane, client_currentMoveIndex);

            //부스터 사용할 경우 버프 주지 말고 None 상태일겨우 1단계 부스터 버프 주자
            if (client_currentCarBoosterLv == CarBoosterType.None)
                networkInGameRPCManager.RPC_BoostPlayer(networkPlayerID, (int)CarBoosterType.CarBooster_LevelOne);
        }


        SetAnimation_Both(AnimationState.Drive, true);

        playerCar.DeactivateStunFX();
        playerCar.DeactivateMudFX();
    }

    private bool IsValidFlip()
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return false;

       if (network_isEnteredTheFinishLine == true)
            return false;

        if (isOutOfBoundary == true)
            return false;

        if (isFlipped == true)
            return false;

        return true;
    }

    public void SetGroundType(TrackType type)
    {
        currentTrackType = type;
    }

    public void SetUpwardDirection(ContactPoint[] points)
    {
        upwardDirection = Vector3.up;

        //일단 아래 코드 사용x....
        //ContactPoint 기반으로 하는건 dir 튀는 현상 존재
        return;

        foreach (var i in points)
        {
            var dir = i.normal;
            var newDir = dir;

            //dir 값이 뒤집히는 현상이 방지 차원
            if ((upwardDirection + -dir).magnitude < 0.1f )
                newDir = dir;
            else
                newDir = -dir;


            if (Vector3.Angle(newDir, upwardDirection) < 90)
                upwardDirection = newDir;

            break;
        }
    }

    private bool IsNearStartWayPoint()
    {
        bool isNear = false;

        var w1 = GetWayPointPosition(client_currentLaneType, client_currentMoveIndex);
        var w2 = GetWayPointPosition(client_currentLaneType, 0);

        if (w1.HasValue && w2.HasValue)
        {
            var dir = (w1.Value - w2.Value).normalized;

            if (Vector3.Distance(transform.position, w2.Value) < 15f)
                isNear = true;
        }

        return isNear;
    }


    private enum ConstraintType { None, XY, XYZ}
    private void SetRigidbodyConstraints(ConstraintType type)
    {
        switch (type)
        {
            case ConstraintType.None:
                rgbdy.constraints = RigidbodyConstraints.None;
                break;
            case ConstraintType.XY:
                rgbdy.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
                break;
            case ConstraintType.XYZ:
                rgbdy.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                break;
        }
    }

    public void OnRPC_LightUpHeadlightsAndHonkHorn(bool isOn)
    {
        if (isOn)
        {
            //불빛 켜기

            if (IsMineAndNotAI)
            {
                //소리...! 관련 처리
            }
        }
        else
        { 
            //불빛 끄기
        }
    }

    public void ActivateStartingBooster()
    {
        if (isStartingBoosterInputBlocked)
            return;

        var ts = InGameManager.Instance.countDownEndTime_StartRace - PnixNetworkManager.Instance.ServerTime ;

        if (ts.TotalSeconds > 0f && ts.TotalSeconds < ref_boosterStartingValidTime)
        {
            isStartingBoosterActiavated = true;
        }
        else
        {
            isStartingBoosterInputBlocked = true;
            startingBoosterInputTimeCounter = ref_boosterStartingValidTime;
        }
    }

    private void FirebaseBasicValueChanged_SubscriptionEvent()
    {

    }

    private void FirebaseCarValueChanged_SubscriptionEvent()
    {

    }

    public void ActivateAutoMovement()
    {
        Initialize_AI();
        IsAutoMovement = true;
    }

    public void DeactivateAllFX()
    {
        if (playerCar != null)
        {
            playerCar.DeactivateStunFX();
            playerCar.DeactivateMudFX();
        }

        if (IsMineAndNotAI)
            Camera_Base.Instance.TurnOffCameraAllFX();
    }

    #region Photon Fusion INetworkRunnerCallbacks
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
        {
            var data = new TouchSwipe_NetworkInputData();

            if (InGameManager.Instance.myPlayer != null)
            {
                if (IsMineAndNotAI)
                {
                    data.networkPlayerID = this.networkPlayerID;

                    if (UI_IngameControl_TouchSwipe.OnDragStart)
                    {
                        data.currentEventType = TouchSwipe_NetworkInputData.EventType.StartDrag;
                    }
                    else if (UI_IngameControl_TouchSwipe.OnDrag)
                    {
                        data.currentEventType = TouchSwipe_NetworkInputData.EventType.Drag;
                        data.position = UI_IngameControl_TouchSwipe.OnDragPositionInfo;
                    }
                    else if (UI_IngameControl_TouchSwipe.OnDragEnd)
                    {
                        data.currentEventType = TouchSwipe_NetworkInputData.EventType.EndDrag;
                    }

                    input.Set(data);
                }
            }
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    #endregion

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (currentDest.HasValue)
            {
                if (IsMine)
                {
                    Gizmos.color = Color.magenta;

                    Gizmos.DrawLine(transform.position, currentDest.Value);


                    Gizmos.color = Color.magenta;

                    Gizmos.DrawWireSphere(currentDest.Value, 1.3f);


                    Gizmos.color = Color.red;

                    Gizmos.DrawWireSphere(transform.position + currDirection * GetColliderLength(), 0.4f);


                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(transform.position, ref_playerMoveIndexCheckMinRange);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, currentDest.Value);
                    Gizmos.DrawWireSphere(currentDest.Value, 1.3f);
                }
            }
        }
    }

#endif


#if CHEAT

    public void ForceFinishLap()
    {
        network_isEnteredTheFinishLine = true;

        inGameManager.endRaceTick = this.Runner.Tick;
        float finishTime = (inGameManager.endRaceTick - inGameManager.startRaceTick) * this.Runner.DeltaTime;

        networkInGameRPCManager.RPC_PlayerPassedFinishLine(networkPlayerID, finishTime);
        PnixNetworkManager.Instance.SendCrossTheRaceFinishLine(network_PID, CNetworkManager.Instance.ServerTime);
    }

#endif
}
