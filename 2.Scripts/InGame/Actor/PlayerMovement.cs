using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Fusion;

public partial class PlayerMovement : PlayerBase
{
    [SerializeField] public Rigidbody rgbdy = null;
    [SerializeField] public PlayerCollisionChecker collisionChecker_Player = null; //�÷��̾ �浹 üũ
    [SerializeField] public PlayerCollisionChecker collisionChecker_Ground = null; //�ٴ� �浹 üũ
    [SerializeField] public PlayerTriggerChecker triggerChecker_Front = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Back = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Body = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Right = null;
    [SerializeField] public PlayerTriggerChecker triggerChecker_Left = null;

    [ReadOnly] public DataManager.CAR_DATA.CarInfo myCarInfo = null;

    public InGameManager inGameManager => InGameManager.Instance;
    public NetworkInGameRPCManager networkInGameRPCManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;
    public NetworkRunner networkRunner => PhotonNetworkManager.Instance.MyNetworkRunner;

    [ReadOnly] public WayPointSystem.WaypointsGroup wg = null;

    [ReadOnly] public Vector3 currDirection = Vector3.zero;
    [ReadOnly] public Vector3? currentDest = null;
    [ReadOnly] public float currentMoveSpeed = 0f;
    [ReadOnly] public float currentRotationSpeed = 0f;
    [ReadOnly] public int prevMoveIndex = 0;
    [ReadOnly] public int currentMoveIndex = 0;
    [ReadOnly] public int currentLap = 0;
    [ReadOnly] public int currentRank = 0;
    [ReadOnly] public int previousBatteryAtBooster = 0; //�ν��� �ۿ� ���� battery ��
    [ReadOnly] public int currentBattery = 0;

    [ReadOnly] public float additionalCarBoosterSpeed = 0f; //Car �ν��Ϳ� ���� �߰� �ӵ�
    [ReadOnly] public float additionalChargePadSpeed = 0f; //Charge Pad�ν��Ϳ� ���� �߰� �ӵ�
    [ReadOnly] public float additionalGroundSpeed = 0f; //�� �ٴ� ������ ���� �߰� �ӵ�
    [ReadOnly] public float additionalDecelerationSpeed = 0f; //���ӽ� ���ҵǴ� �ӵ�
    [ReadOnly] public float additionalStunSpeed = 0f; //���Ͻ� ���ҵǴ� �ӵ�
    [ReadOnly] public float additionalbuffSpeed = 0f; //������ ���� �߰� ���� �ӵ�

    [Networked] public bool isMoving { get; set; }
    [Networked] public bool isEnteredTheFinishLine { get; set; }
    [Networked] public bool isDecelerating { get; set; }
    [Networked] public bool isParrying { get; set; }
    [Networked] public bool isParryingCooltime { get; set; }
    [Networked] public bool isStunned { get; set; }
    [Networked] public bool isFlipped { get; set; }
    [Networked] public bool isOutOfBoundary { get; set; }
    [Networked] public bool isFlippingUp { get; set; }
    [Networked] public bool isGrounded { get; set; }
    public bool isBoosting { get { if (currentChargeBoosterLv != ChargePadBoosterLevel.None || currentCarBoosterLv != CarBoosterLevel.None) return true; else return false; } }
    public bool isCarBoosting { get { if (currentCarBoosterLv != CarBoosterLevel.None) return true; else return false; } }
    public bool isCarAttackBoosting { get { if (currentCarBoosterLv == CarBoosterLevel.Car_Two || currentCarBoosterLv == CarBoosterLevel.Car_Three) return true; else return false; } }
    public bool isChargePadBoosting { get { if (currentChargeBoosterLv != ChargePadBoosterLevel.None) return true; else return false; } }
    
    public bool IsMine { get { return Object != null && Object.HasInputAuthority; } }

    public Vector3? lagOffestPos = null;
    public Vector3? networkPos = null;
    public Vector3? networkDir = null;
    public Quaternion networkRotation = Quaternion.identity;
    public float? networkMoveSpeed = null;
    public float? networkRotationSpeed = null;
    public int? networkMoveIndex = 0;
    public int? networkLap = 0;
    public int? networkBattery = null;

    [ReadOnly] public float lagTime = 0f;

    [ReadOnly] public bool IsLagging = false; //Client & Network Postion �� lag ����
#if UNITY_EDITOR
    public bool IsLagTestDebugOn = true; //�ν����ͻ󿡼� üũ������
#endif

    private float timeCounterAfterEnteringFinishingLine = 0;

    [ReadOnly] public float playerLagAdjustRate = 0.06f;

    private float speedMaxLimit = 0f;

    [ReadOnly] public Vector3 previousReachDest = Vector3.zero;
    [ReadOnly] public Vector3 previousDir = Vector3.zero;
    public enum MoveInput { None, Left, Right, Boost, Deceleration_Start, Deceleration_End }
    public enum LaneType { None = -1, Zero = 0, One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6 }
    [ReadOnly] public LaneType currentLaneType = LaneType.None;
    [ReadOnly] public LaneType previousLaneType = LaneType.None;
    [ReadOnly] public bool isChangingLane = false;
    private LaneType networkLaneType = LaneType.None;

    public enum CarBoosterLevel { None = 0, Car_One, Car_Two, Car_Three }
    [ReadOnly] public CarBoosterLevel currentCarBoosterLv = CarBoosterLevel.None;

    public enum ChargePadBoosterLevel { None, ChargePad_One, ChargePad_Two, ChargePad_Three }
    [ReadOnly] public ChargePadBoosterLevel currentChargeBoosterLv = ChargePadBoosterLevel.None;

    public enum RoadType { Normal = 0, Ice, Mud, Sand }
    [ReadOnly] public RoadType currentRoadType = RoadType.Normal;

    public enum CarType { None, Type_1, Type_2, Type_3, Type_4 }
    [ReadOnly] public CarType currentCarType = CarType.None;

    public enum AnimationState { Idle, Drive, Booster_1, Booster_2, Brake, MoveLeft, MoveRight, Flip, Victory, Complete, Retire, Spin }
    [ReadOnly] public AnimationState currentCarAnimationState = AnimationState.Idle;
    [ReadOnly] public AnimationState currentCharAnimationState = AnimationState.Idle;

    [ReadOnly] public int carID = -1;
    [ReadOnly] public int characterID = -1;

    [ReadOnly] public float outOfBoundaryTimer = 0f;
    [ReadOnly] public float outOfBoundaryMoveSpeed = 0f;

    [ReadOnly] public bool IsPlayerInFront = false; //�÷��̾� �տ� �ٸ� �÷��̾ �ִ� ���
    [ReadOnly] public int playerInFrontViewID = 0;

    [ReadOnly] public float parryingCoolTimeLeft = 0f;

    public Action FixedUpdateNetworkCallback = null;

    #region DB Stats

    [HideInInspector] public float PLAYER_DIRECTION_LERP_MIN_SPEED = 10; //�󸶳� �ε巴�� ȸ���� ���� (�������� �ε巴���� ��Ȯ�� ������)
    [HideInInspector] public float PLAYER_DIRECTION_LERP_MAX_SPEED = 15; //�󸶳� �ε巴�� ȸ���� ���� (�������� �ε巴���� ��Ȯ�� ������)
    [HideInInspector] public float PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MIN_SPEED = 5; //�󸶳� �ε巴�� ȸ���� ���� (�������� �ε巴���� ��Ȯ�� ������)
    [HideInInspector] public float PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MAX_SPEED = 29; //�󸶳� �ε巴�� ȸ���� ���� (�������� �ε巴���� ��Ȯ�� ������)

    [HideInInspector] public float PLAYER_DEFAULT_MOVE_SPEED = 15f;
    [HideInInspector] public float PLAYER_MAX_MOVE_SPEED = 100f;
    [HideInInspector] public float PLAYER_DEFAULT_ROTATION_SPEED = 15f;
    [HideInInspector] public float PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE = 10f; //finish line ����� �󸶳� �ӵ��� �ٿ�����
    [HideInInspector] public float PLAYER_MOVE_SPEED_BUFF = 0.05f;

    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV1_MAX_SPEED = 10f;
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV1_DECREASE_SPEED = 5f;
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV2_MAX_SPEED = 15f;
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV2_DECREASE_SPEED = 5f;
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV3_MAX_SPEED = 20f;
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV3_DECREASE_SPEED = 5f;

    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV1_MAXSPEED_DURATION_TIME = 0.5f; //�ְ� �ӵ� �����ð�
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV2_MAXSPEED_DURATION_TIME = 1f; //�ְ� �ӵ� �����ð�
    [HideInInspector] public float PLAYER_BOOSTER_CAR_LV3_MAXSPEED_DURATION_TIME = 1.5f; //�ְ� �ӵ� �����ð�

    [HideInInspector] public float PLAYER_BOOSTER_CHARGEPAD_LV1_MAX_SPEED = 5f;
    [HideInInspector] public float PLAYER_BOOSTER_CHARGEPAD_LV1_DECREASE_SPEED = 7f;
    [HideInInspector] public float PLAYER_BOOSTER_CHARGEPAD_LV2_MAX_SPEED = 7f;
    [HideInInspector] public float PLAYER_BOOSTER_CHARGEPAD_LV2_DECREASE_SPEED = 7f;
    [HideInInspector] public float PLAYER_BOOSTER_CHARGEPAD_LV3_MAX_SPEED = 10f;
    [HideInInspector] public float PLAYER_BOOSTER_CHARGEPAD_LV3_DECREASE_SPEED = 7f;
    [HideInInspector] public int PLAYER_BOOSTER_CHARGEPAD_LV1_CHARGE_AMOUNT = 20;
    [HideInInspector] public int PLAYER_BOOSTER_CHARGEPAD_LV2_CHARGE_AMOUNT = 30;
    [HideInInspector] public int PLAYER_BOOSTER_CHARGEPAD_LV3_CHARGE_AMOUNT = 45;

    [HideInInspector] public float PLAYER_DECELERATION_DECREASE_SPEED = 7f;
    [HideInInspector] public float PLAYER_DECELERATION_MAX_SPEED = 15f;
    [HideInInspector] public float PLAYER_DECELERATION_RECOVERY_SPEED = 14f;

    [HideInInspector] public float PLAYER_MOVE_INDEX_CHECK_MIN_RANGE = 15f;
    [HideInInspector] public int PLAYER_MOVE_INDEX_CHECK_NUMBER = 5;

    [HideInInspector] public float PLAYER_LAG_MAX_DISTANCE = 2f; //Lag Max
    [HideInInspector] public float PLAYER_LAG_MIN_DISTANCE = 0.25f; //Lag�� �Ǵ��ϴ� dist
    [HideInInspector] public float PLAYER_LAG_MAX_RATE = 0.6f; //Lag�� ������� ��ġ�� �������� ... �̶� MAX ��
    [HideInInspector] public float PLAYER_LAG_MIN_RATE = 0.06f; //Lag�� ������� ��ġ�� �������� ... �̶� MIN ��
    [HideInInspector] public float PLAYER_LAG_TELEPORT_TIME = 1.5f; //���� �ð� �̻� lag �� ��� teleport ���ѹ�����
    [HideInInspector] public float PLAYER_LAG_TELEPORT_DIST = 15f; //���� �Ÿ� �̻� lag �� ��� teleport ���ѹ�����

    [HideInInspector] public int PLAYER_BATTERY_MAX = 100;
    [HideInInspector] public float PLAYER_BATTERY_AUTOCHARGE_SPEED = 0.9f;
    [HideInInspector] public int PLAYER_BATTERY_CARBOOSTER1_COST = 20;
    [HideInInspector] public int PLAYER_BATTERY_CARBOOSTER2_COST = 33;
    [HideInInspector] public int PLAYER_BATTERY_CARBOOSTER3_COST = 50;
    [HideInInspector] public int PLAYER_BATTERY_ATTACK_SUCCESS = 20; //���ݼ����� ���� ���¸�
    [HideInInspector] public int PLAYER_BATTERY_DEFENCE_SUCCESS = 15; //���ݼ����� ���� ���¸�
    [HideInInspector] public int PLAYER_BATTERY_START_AMOUNT = 30; //���� ���¸���

    [HideInInspector] public bool PLAYER_RIGIDBODY_USE_GRAVITY = false;
    [HideInInspector] public float PLAYER_RIGIDBODY_MASS = 10;
    [HideInInspector] public float PLAYER_RIGIDBODY_DRAG = 1;
    [HideInInspector] public float PLAYER_RIGIDBODY_ANGULAR_DRAG = 0;

    [HideInInspector] public float PLAYER_PARRYING_TIME = 2.5f;
    [HideInInspector] public float PLAYER_PARRYING_COOLTIME = 5f;
    [HideInInspector] public float PLAYER_STUN_TIME = 2.5f;

    [HideInInspector] public float PLAYER_FLIP_UP_TIME = 1.5f; //���� �޾��� �� ���� ��� �ð�
    [HideInInspector] public float PLAYER_FLIP_UP_SPEED = 20f; //���� �޾��� �� ���� ��� �ӵ�
    [HideInInspector] public float PLAYER_FLIP_DOWN_SPEED = 30f; //���� �޾��� �� ��� �� �ϰ� �ӵ�
    [HideInInspector] public float PLAYER_FLIP_GROUND_DELAY_TIME = 0.5f; //���� ���� �� ������

    [HideInInspector] public bool PLAYER_OUT_OF_BOUNDARY_USE = true; //isOutOfBoundary ��� ��뿩��
    [HideInInspector] public float PLAYER_OUT_OF_BOUNDARY_TIME = 5f; //isOutOfBoundary, ���� ��� �� x�� �� ������ �ð�

    [HideInInspector] public float PLAYER_VALID_REACH_DEST = 1.5f; //��ǥ dest���� ���޵� �Ǵ��ϴ� �Ÿ�

    [HideInInspector] public int PLAYER_SHOW_MOVE_SPEED_MULTIPYER = 1;

    #endregion


    private void Start()
    {
        InitializeSettings();

        if (IsMine)
            inGameManager.myPlayer = this;

        transform.parent = null;

        inGameManager.UpdateNetworkPlayerList();
    }

    public void OnEnable()
    {
        DataManager.Instance.firebaseBasicValueChangedCallback += OnFirebaseBasicValueChangedCallback;
        DataManager.Instance.firebaseCarValueChangedCallback += OnFirebaseCarValueChangedCallback;
    }

    public void OnDisable()
    {
        DataManager.Instance.firebaseBasicValueChangedCallback -= OnFirebaseBasicValueChangedCallback;
        DataManager.Instance.firebaseCarValueChangedCallback -= OnFirebaseCarValueChangedCallback;
    }

    public void InitializeSettings()
    {
        SetName();
        SetPhotonSettings();
        SetBasicData();

        wg = InGameManager.Instance.wayPoints;

        currentDest = null;

        currentLap = 0;
        currentMoveIndex = 0;
        currentMoveSpeed = 0;
        currentBattery = 0;

        currentRank = 0;

        lagTime = 0;
        IsLagging = false;

        isMoving = false;
        isEnteredTheFinishLine = false;

        networkPos = null;
        networkMoveIndex = null;
        networkMoveSpeed = null;
        networkRotationSpeed = null;
        networkBattery = null;

        currentCarBoosterLv = CarBoosterLevel.None;
        currentChargeBoosterLv = ChargePadBoosterLevel.None;

        currentRoadType = RoadType.Normal;

        currentLaneType = LaneType.None;
        previousLaneType = LaneType.None;
        isChangingLane = false;

        currentCarAnimationState = AnimationState.Idle;
        currentCharAnimationState = AnimationState.Idle;

        collisionChecker_Player.enabled = true;
        collisionChecker_Ground.enabled = true;
        rgbdy.isKinematic = true;

        isDecelerating = false;
        isParrying = false;
        isParryingCooltime = false;
        isStunned = false;
        isFlipped = false;
        isOutOfBoundary = false;
        isFlippingUp = false;

        additionalCarBoosterSpeed = 0f;
        additionalChargePadSpeed = 0f;
        additionalDecelerationSpeed = 0f;
        additionalGroundSpeed = 0f;
        additionalStunSpeed = 0f;
        additionalbuffSpeed = 0f;

        speedMaxLimit = 100;

        outOfBoundaryTimer = 0f;
        outOfBoundaryMoveSpeed = PLAYER_DEFAULT_MOVE_SPEED;

        parryingCoolTimeLeft = 0f;

        previousReachDest = Vector3.zero;
        previousDir = Vector3.zero;

        triggerChecker_Front.Initialize();
        triggerChecker_Back.Initialize();
        triggerChecker_Right.Initialize();
        triggerChecker_Left.Initialize();

        rgbdy.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void SetName()
    {
        if (IsMine)
        {
            gameObject.name = "MINE-" + CommonDefine.GetGoNameWithHeader("PLAYER") + "PID:" + networkObject.Id;
        }
        else
        {
            gameObject.name = "OTHER-" + CommonDefine.GetGoNameWithHeader("PLAYER") + "PID:" + networkObject.Id;
        }
    }

    private void SetPhotonSettings()
    {
        if (IsMine)
        {

        }
    }

    public void SetBasicData()
    {
        if (DataManager.Instance.isBasicDataLoaded == true)
        {
            var data = DataManager.Instance.basicData.playerData;

            PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE = data.PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE;
            PLAYER_MOVE_INDEX_CHECK_MIN_RANGE = data.PLAYER_MOVE_INDEX_CHECK_MIN_RANGE;
            PLAYER_MOVE_INDEX_CHECK_NUMBER = data.PLAYER_MOVE_INDEX_CHECK_NUMBER;
            PLAYER_MOVE_SPEED_BUFF = data.PLAYER_MOVE_SPEED_BUFF;

            PLAYER_LAG_MAX_DISTANCE = data.PLAYER_LAG_MAX_DISTANCE;
            PLAYER_LAG_MIN_DISTANCE = data.PLAYER_LAG_MIN_DISTANCE;
            PLAYER_LAG_MAX_RATE = data.PLAYER_LAG_MAX_RATE;
            PLAYER_LAG_MIN_RATE = data.PLAYER_LAG_MIN_RATE;
            PLAYER_LAG_TELEPORT_TIME = data.PLAYER_LAG_TELEPORT_TIME;
            PLAYER_LAG_TELEPORT_DIST = data.PLAYER_LAG_TELEPORT_DIST;

            PLAYER_RIGIDBODY_USE_GRAVITY = data.PLAYER_RIGIDBODY_USE_GRAVITY;

            PLAYER_OUT_OF_BOUNDARY_USE = data.PLAYER_OUT_OF_BOUNDARY_USE;
            PLAYER_OUT_OF_BOUNDARY_TIME = data.PLAYER_OUT_OF_BOUNDARY_TIME;

            PLAYER_DIRECTION_LERP_MIN_SPEED = data.PLAYER_DIRECTION_LERP_MIN_SPEED;
            PLAYER_DIRECTION_LERP_MAX_SPEED = data.PLAYER_DIRECTION_LERP_MAX_SPEED;
            PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MIN_SPEED = data.PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MIN_SPEED;
            PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MAX_SPEED = data.PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MAX_SPEED;

            PLAYER_VALID_REACH_DEST = data.PLAYER_VALID_REACH_DEST;

            if (rgbdy != null)
            {
                rgbdy.useGravity = data.PLAYER_RIGIDBODY_USE_GRAVITY;
            }
                
            PLAYER_SHOW_MOVE_SPEED_MULTIPYER = data.PLAYER_SHOW_MOVE_SPEED_MULTIPYER;
        }
        else
        {
            Debug.Log("Data Loaded is False....! Using Default values....");

            PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE = 10f;
            PLAYER_MOVE_INDEX_CHECK_MIN_RANGE = 15f;
            PLAYER_MOVE_INDEX_CHECK_NUMBER = 5;

            PLAYER_LAG_MAX_DISTANCE = 2f;
            PLAYER_LAG_MIN_DISTANCE = 0.25f;
            PLAYER_LAG_MAX_RATE = 0.6f;
            PLAYER_LAG_MIN_RATE = 0.06f;
            PLAYER_LAG_TELEPORT_TIME = 1.5f;
        }

    }

    public void SetCarData(int carID)
    {
        if (DataManager.Instance.isCarDataLoaded == true)
        {
            var carData = DataManager.Instance.carData;
            if (carData != null)
            {
                foreach (var i in carData.CarDataList)
                {
                    if (i.carId == carID)
                    {
                        myCarInfo = i;
                        break;
                    }
                }
            }

            if (myCarInfo != null)
            {
                PLAYER_DEFAULT_MOVE_SPEED = myCarInfo.PLAYER_MOVE_SPEED;
                PLAYER_MAX_MOVE_SPEED = myCarInfo.PLAYER_MAX_MOVE_SPEED;
                PLAYER_DEFAULT_ROTATION_SPEED = myCarInfo.PLAYER_ROTATION_SPEED;

                PLAYER_BATTERY_MAX = myCarInfo.PLAYER_BATTERY_MAX;
                PLAYER_BATTERY_AUTOCHARGE_SPEED = myCarInfo.PLAYER_BATTERY_AUTOCHARGE_SPEED;
                PLAYER_BATTERY_CARBOOSTER1_COST = myCarInfo.PLAYER_BATTERY_CARBOOSTER1_COST;
                PLAYER_BATTERY_CARBOOSTER2_COST = myCarInfo.PLAYER_BATTERY_CARBOOSTER2_COST;
                PLAYER_BATTERY_CARBOOSTER3_COST = myCarInfo.PLAYER_BATTERY_CARBOOSTER3_COST;
                PLAYER_BATTERY_ATTACK_SUCCESS = myCarInfo.PLAYER_BATTERY_ATTACK_SUCCESS;
                PLAYER_BATTERY_DEFENCE_SUCCESS = myCarInfo.PLAYER_BATTERY_DEFENCE_SUCCESS;
                PLAYER_BATTERY_START_AMOUNT = myCarInfo.PLAYER_BATTERY_START_AMOUNT;

                PLAYER_RIGIDBODY_MASS = myCarInfo.PLAYER_RIGIDBODY_MASS;
                PLAYER_RIGIDBODY_DRAG = myCarInfo.PLAYER_RIGIDBODY_DRAG;
                PLAYER_RIGIDBODY_ANGULAR_DRAG = myCarInfo.PLAYER_RIGIDBODY_ANGULAR_DRAG;

                PLAYER_BOOSTER_CAR_LV1_MAX_SPEED = myCarInfo.PLAYER_BOOSTER_CAR_LV1_MAX_SPEED;
                PLAYER_BOOSTER_CAR_LV1_DECREASE_SPEED = myCarInfo.PLAYER_BOOSTER_CAR_LV1_DECREASE_SPEED;
                PLAYER_BOOSTER_CAR_LV2_MAX_SPEED = myCarInfo.PLAYER_BOOSTER_CAR_LV2_MAX_SPEED;
                PLAYER_BOOSTER_CAR_LV2_DECREASE_SPEED = myCarInfo.PLAYER_BOOSTER_CAR_LV2_DECREASE_SPEED;
                PLAYER_BOOSTER_CAR_LV3_MAX_SPEED = myCarInfo.PLAYER_BOOSTER_CAR_LV3_MAX_SPEED;
                PLAYER_BOOSTER_CAR_LV3_DECREASE_SPEED = myCarInfo.PLAYER_BOOSTER_CAR_LV3_DECREASE_SPEED;

                if (DataManager.Instance.isBasicDataLoaded && DataManager.Instance.basicData != null && DataManager.Instance.basicData.chargePadData != null)
                {
                    var chargePadData = DataManager.Instance.basicData.chargePadData;

                    PLAYER_BOOSTER_CHARGEPAD_LV1_MAX_SPEED = chargePadData.CHARGEPAD_LV1_MAX_SPEED;
                    PLAYER_BOOSTER_CHARGEPAD_LV1_DECREASE_SPEED = chargePadData.CHARGEPAD_LV1_DECREASE_SPEED;
                    PLAYER_BOOSTER_CHARGEPAD_LV2_MAX_SPEED = chargePadData.CHARGEPAD_LV2_MAX_SPEED;
                    PLAYER_BOOSTER_CHARGEPAD_LV2_DECREASE_SPEED = chargePadData.CHARGEPAD_LV2_DECREASE_SPEED;
                    PLAYER_BOOSTER_CHARGEPAD_LV3_MAX_SPEED = chargePadData.CHARGEPAD_LV3_MAX_SPEED;
                    PLAYER_BOOSTER_CHARGEPAD_LV3_DECREASE_SPEED = chargePadData.CHARGEPAD_LV3_DECREASE_SPEED;

                    PLAYER_BOOSTER_CHARGEPAD_LV1_CHARGE_AMOUNT = chargePadData.CHARGEPAD_LV1_CHARGE_AMOUNT;
                    PLAYER_BOOSTER_CHARGEPAD_LV2_CHARGE_AMOUNT = chargePadData.CHARGEPAD_LV2_CHARGE_AMOUNT;
                    PLAYER_BOOSTER_CHARGEPAD_LV3_CHARGE_AMOUNT = chargePadData.CHARGEPAD_LV3_CHARGE_AMOUNT;
                }

                PLAYER_BOOSTER_CAR_LV1_MAXSPEED_DURATION_TIME = myCarInfo.PLAYER_BOOSTER_CAR_LV1_MAXSPEED_DURATION_TIME;
                PLAYER_BOOSTER_CAR_LV2_MAXSPEED_DURATION_TIME = myCarInfo.PLAYER_BOOSTER_CAR_LV2_MAXSPEED_DURATION_TIME;
                PLAYER_BOOSTER_CAR_LV3_MAXSPEED_DURATION_TIME = myCarInfo.PLAYER_BOOSTER_CAR_LV3_MAXSPEED_DURATION_TIME;

                PLAYER_DECELERATION_DECREASE_SPEED = myCarInfo.PLAYER_DECELERATION_DECREASE_SPEED;
                PLAYER_DECELERATION_MAX_SPEED = myCarInfo.PLAYER_DECELERATION_MAX_SPEED;
                PLAYER_DECELERATION_RECOVERY_SPEED = myCarInfo.PLAYER_DECELERATION_RECOVERY_SPEED;

                PLAYER_PARRYING_TIME = myCarInfo.PLAYER_PARRYING_TIME;
                PLAYER_PARRYING_COOLTIME = myCarInfo.PLAYER_PARRYING_COOLTIME;
                PLAYER_STUN_TIME = myCarInfo.PLAYER_STUN_TIME;

                PLAYER_FLIP_UP_TIME = myCarInfo.PLAYER_FLIP_UP_TIME;
                PLAYER_FLIP_UP_SPEED = myCarInfo.PLAYER_FLIP_UP_SPEED;
                PLAYER_FLIP_DOWN_SPEED = myCarInfo.PLAYER_FLIP_DOWN_SPEED;
                PLAYER_FLIP_GROUND_DELAY_TIME = myCarInfo.PLAYER_FLIP_GROUND_DELAY_TIME;

                if (rgbdy != null)
                {
                    rgbdy.mass = myCarInfo.PLAYER_RIGIDBODY_MASS;
                    rgbdy.drag = myCarInfo.PLAYER_RIGIDBODY_DRAG;
                    rgbdy.angularDrag = myCarInfo.PLAYER_RIGIDBODY_ANGULAR_DRAG;
                }
            }
        }
    }

    public void SetCar(int carID)
    {
        this.carID = carID;
        playerCar.SetCar((DataManager.CAR_DATA.CarID)carID);
    }

    public void SetCharacter(int characterID)
    {
        this.characterID = characterID;
        playerCharacter.SetCharacter((DataManager.CHARACTER_DATA.CharacterType)characterID);

        //TEMP
        playerCharacter.SetHeadObj(carID);

        if (playerCar != null && playerCar.currentCar != null && playerCar.currentCar.dollyRoot != null)
        {
            playerCharacter.transform.SetParent(playerCar.currentCar.dollyRoot.transform);
            playerCharacter.transform.localScale = Vector3.one;
        }
    }

    public void InitializePosition(int randomIndex)
    {
        var spawnPosi = Vector3.zero;

        if (wg == null)
            wg = InGameManager.Instance.wayPoints;

        if (wg == null)
        {
            Debug.Log("Warning!!! wayPoints is null");
            return;
        }

        int startMoveIndex = 1;
        int laneIndex = 0;
        int counter = 0;
        //������ index�� ������ 0~�ο��� �����ϱ�...
        for (int i = 0; i <= (int)LaneType.Six; i++)
        {
            if (IsValidDestination((LaneType)i, startMoveIndex) == true)
            {
                if (counter == randomIndex)
                {
                    //������ index�� ������ 0~�ο��� �����ϱ�...
                    laneIndex = i;
                    break;
                }

                counter++;
            }
        }

        WayPointSystem.Waypoint wp_first = null;
        WayPointSystem.Waypoint wp_next = null;

        if (laneIndex == 0 && IsValidDestination(LaneType.Zero, startMoveIndex))
        {
            wp_first = wg.waypoints_0[0];
            wp_next = wg.waypoints_0[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Zero);
        }
        else if (laneIndex == 1 && IsValidDestination(LaneType.One, startMoveIndex))
        {
            wp_first = wg.waypoints_1[0];
            wp_next = wg.waypoints_1[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.One);
        }
        else if (laneIndex == 2 && IsValidDestination(LaneType.Two, startMoveIndex))
        {
            wp_first = wg.waypoints_2[0];
            wp_next = wg.waypoints_2[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Two);
        }
        else if (laneIndex == 3 && IsValidDestination(LaneType.Three, startMoveIndex))
        {
            wp_first = wg.waypoints_3[0];
            wp_next = wg.waypoints_3[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Three);
        }
        else if (laneIndex == 4 && IsValidDestination(LaneType.Four, startMoveIndex))
        {
            wp_first = wg.waypoints_4[0];
            wp_next = wg.waypoints_4[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Four);
        }
        else if (laneIndex == 5 && IsValidDestination(LaneType.Five, startMoveIndex))
        {
            wp_first = wg.waypoints_5[0];
            wp_next = wg.waypoints_5[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Five);
        }
        else if (laneIndex == 6 && IsValidDestination(LaneType.Six, startMoveIndex))
        {
            wp_first = wg.waypoints_6[0];
            wp_next = wg.waypoints_6[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Six);
        }
        else
        {
            Debug.Log("<color=red>No valid initial spwn point...! Automatically moved to index 3</color>");
            wp_first = wg.waypoints_3[0];
            wp_next = wg.waypoints_3[1];
            spawnPosi = wp_first.GetPosition();
            SetCurrentLaneNumberType(LaneType.Three);
        }

        currentMoveIndex = startMoveIndex;

        var dir = (wp_next.GetPosition() - wp_first.GetPosition()).normalized;

        transform.position = spawnPosi; //���⼭�� �� transform.position Ȱ������ rigidboy.transform ����
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        currDirection = transform.rotation.eulerAngles;
        currentDest = wp_next.GetPosition();

        collisionChecker_Player.enabled = true;
        collisionChecker_Ground.enabled = true;
        rgbdy.isKinematic = false;

        previousReachDest = transform.position;
        previousDir = currDirection;
    }

    public void SetIngameInput(MoveInput type, Vector3 playerPos, int networkMoveIndex, LaneType networklaneType, CarBoosterLevel boosterLv)
    {
        if (InGameManager.Instance.gameState != InGameManager.GameState.PlayGame
            && InGameManager.Instance.gameState != InGameManager.GameState.EndCountDown)
            return;

        if (isEnteredTheFinishLine)
            return;

        if (networklaneType != currentLaneType)
            currentLaneType = networklaneType;

        if (IsMine == false) //�� isMine �ƴ� ��쿡��!!! ����
        {
            if (IsLastIndex(currentLaneType, networkMoveIndex) == false)
            {
                if (currentMoveIndex <= networkMoveIndex)
                    currentMoveIndex = networkMoveIndex;
            }
            else
            {
                currentMoveIndex = 0;
            }
        }

        switch (type)
        {
            case MoveInput.None:
                {

                }
                break;
            case MoveInput.Left:
                {
                    LaneType nextLaneType = LaneType.None;

                    if (currentLaneType == LaneType.One)
                        nextLaneType = LaneType.Zero;
                    else if (currentLaneType == LaneType.Two)
                        nextLaneType = LaneType.One;
                    else if (currentLaneType == LaneType.Three)
                        nextLaneType = LaneType.Two;
                    else if (currentLaneType == LaneType.Four)
                        nextLaneType = LaneType.Three;
                    else if (currentLaneType == LaneType.Five)
                        nextLaneType = LaneType.Four;
                    else if (currentLaneType == LaneType.Six)
                        nextLaneType = LaneType.Five;

                    if (nextLaneType != currentLaneType)
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

                    int newMoveIndex = GetNewMoveIndexAtRange(currentMoveIndex, nextLaneType, playerPos, PLAYER_MOVE_INDEX_CHECK_MIN_RANGE, PLAYER_MOVE_INDEX_CHECK_NUMBER);
                    SetCurrentLaneNumberTypeAndMoveIndex(nextLaneType, newMoveIndex);
                }
                break;
            case MoveInput.Right:
                {
                    LaneType nextLaneType = LaneType.None;

                    if (currentLaneType == LaneType.Zero)
                        nextLaneType = LaneType.One;
                    else if (currentLaneType == LaneType.One)
                        nextLaneType = LaneType.Two;
                    else if (currentLaneType == LaneType.Two)
                        nextLaneType = LaneType.Three;
                    else if (currentLaneType == LaneType.Three)
                        nextLaneType = LaneType.Four;
                    else if (currentLaneType == LaneType.Four)
                        nextLaneType = LaneType.Five;
                    else if (currentLaneType == LaneType.Five)
                        nextLaneType = LaneType.Six;

                    if (nextLaneType != currentLaneType)
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

                    int newMoveIndex = GetNewMoveIndexAtRange(currentMoveIndex, nextLaneType, playerPos, PLAYER_MOVE_INDEX_CHECK_MIN_RANGE, PLAYER_MOVE_INDEX_CHECK_NUMBER);
                    SetCurrentLaneNumberTypeAndMoveIndex(nextLaneType, newMoveIndex);
                }
                break;

            case MoveInput.Boost:
                {
                    if (isStunned || isOutOfBoundary || !isGrounded)
                        return;

                    UseBattery(boosterLv); //valid ���δ� event ������ ���� �Ǵ���... ������ �Ҹ��Ű��
                    Car_BoostSpeed(boosterLv);
                }
                break;

            case MoveInput.Deceleration_Start:
                {
                    if (isStunned || isFlipped || isOutOfBoundary || !isGrounded)
                        return;

                    isDecelerating = true;

                    if (isParryingCooltime == false)
                        StartParrying();

                    DeactivateBooster();
                }
                break;

            case MoveInput.Deceleration_End:
                {
                    isDecelerating = false;

                    if (isParryingCooltime == false && isParrying == true)
                        StopParring();
                }
                break;
        }
    }


    public void SetCurrentLaneNumberType(LaneType type)
    {
        if (IsValidDestination(type, currentMoveIndex) == false)
        {
            Debug.Log("<color=red>InValid Lane...! Cannot move to that Lane!!!</color>");
            return;
        }

        previousLaneType = currentLaneType;

        switch (type)
        {
            case LaneType.Zero:
                {
                    currentLaneType = LaneType.Zero;
                }
                break;
            case LaneType.One:
                {
                    currentLaneType = LaneType.One;
                }
                break;
            case LaneType.Two:
                {
                    currentLaneType = LaneType.Two;
                }
                break;
            case LaneType.Three:
                {
                    currentLaneType = LaneType.Three;
                }
                break;
            case LaneType.Four:
                {
                    currentLaneType = LaneType.Four;
                }
                break;
            case LaneType.Five:
                {
                    currentLaneType = LaneType.Five;
                }
                break;
            case LaneType.Six:
                {
                    currentLaneType = LaneType.Six;
                }
                break;
        }

        if (IsMine)
        {
            if (previousLaneType != currentLaneType)
            {
                //��ǲ�� �ƴ� �ڵ����� ���� �ٲ��� network ������ �˷�����...
                networkInGameRPCManager.RPC_SetLaneAndMoveIndex(PlayerID, (int)currentLaneType, currentMoveIndex);
            }
        }

        networkLaneType = currentLaneType;

        if (previousLaneType != currentLaneType
            && previousLaneType != LaneType.None
            && currentLaneType != LaneType.None)
        {
            isChangingLane = true;
        }
    }

    public void SetCurrentLaneNumberTypeAndMoveIndex(LaneType type, int moveIndex)
    {
        if (IsValidDestinationIncludingOutOfBoundary(type, moveIndex) == false)
        {
#if UNTIY_EDITOR
            if (IsMine)
                Debug.Log("<color=red>InValid Lane...! Cannot move to that Lane!!!</color>");
#endif
            return;
        }


        previousLaneType = currentLaneType;

        switch (type)
        {
            case LaneType.Zero:
                {
                    currentLaneType = LaneType.Zero;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.One:
                {
                    currentLaneType = LaneType.One;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Two:
                {
                    currentLaneType = LaneType.Two;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Three:
                {
                    currentLaneType = LaneType.Three;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Four:
                {
                    currentLaneType = LaneType.Four;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Five:
                {
                    currentLaneType = LaneType.Five;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Six:
                {
                    currentLaneType = LaneType.Six;
                    currentMoveIndex = moveIndex;
                }
                break;
        }

        networkLaneType = currentLaneType;
        networkMoveIndex = currentMoveIndex;

        if (previousLaneType != currentLaneType
            && previousLaneType != LaneType.None
            && currentLaneType != LaneType.None)
        {
            isChangingLane = true;
        }
    }

    private void SetValidDestination(ref LaneType currLane, ref int currMoveIndex)
    {
        //lane �������༭ valid dest ã������...!

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

                Debug.Log("<color=red>No Valid Dest....!!!! Check your map....! </color>");
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
                    {
                        laneTypeCount++;
                    }
                    else
                    {
                        laneTypeCount--;
                    }
                }
            }
            else if ((LaneType)laneTypeCount == LaneType.One)
            {
                if (checkCount == 0)
                {
                    laneTypeCount++;
                    isMovingRight = true;
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
            else if ((LaneType)laneTypeCount == LaneType.Two)
            {
                if (checkCount == 0)
                {
                    laneTypeCount++;
                    isMovingRight = true;
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
            else if ((LaneType)laneTypeCount == LaneType.Three)
            {
                if (checkCount == 0)
                {
                    var v1 = GetWayPointPosition(LaneType.Two, GetNextMoveIndex(LaneType.Two, currMoveIndex));
                    var v2 = GetWayPointPosition(LaneType.Three, currMoveIndex);
                    var v3 = GetWayPointPosition(LaneType.Four, GetNextMoveIndex(LaneType.Four, currMoveIndex));

                    if (v1.HasValue && v2.HasValue && v3.HasValue)
                    {
                        if (Vector3.Distance(v2.Value, v1.Value) <= Vector3.Distance(v2.Value, v3.Value))
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
            else if ((LaneType)laneTypeCount == LaneType.Four)
            {
                if (checkCount == 0)
                {
                    laneTypeCount--;
                    isMovingRight = false;
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
            else if ((LaneType)laneTypeCount == LaneType.Five)
            {
                if (checkCount == 0)
                {
                    laneTypeCount--;
                    isMovingRight = false;
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
                    {
                        laneTypeCount++;
                    }
                    else
                    {
                        laneTypeCount--;
                    }
                }
            }

            if (IsValidDestination((LaneType)laneTypeCount, currMoveIndex))
            {
                SetCurrentLaneNumberType((LaneType)laneTypeCount);
                break;
            }

            checkCount++;
        }
    }

    private bool IsValidDestination(LaneType type, int moveIndex)
    {
        bool isValid = false;

        if (wg == null)
            return false;

        List<WayPointSystem.Waypoint> pointList = new List<WayPointSystem.Waypoint>();

        switch (type)
        {
            case LaneType.Zero:
                pointList = wg.waypoints_0;
                break;
            case LaneType.One:
                pointList = wg.waypoints_1;
                break;
            case LaneType.Two:
                pointList = wg.waypoints_2;
                break;
            case LaneType.Three:
                pointList = wg.waypoints_3;
                break;
            case LaneType.Four:
                pointList = wg.waypoints_4;
                break;
            case LaneType.Five:
                pointList = wg.waypoints_5;
                break;
            case LaneType.Six:
                pointList = wg.waypoints_6;
                break;
            default:
                break;
        }

        if (pointList != null && pointList.Count > 0)
        {
            switch (pointList[moveIndex].currentWayPointType)
            {
                case WayPointSystem.Waypoint.WayPointType.Normal:
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

    private bool IsValidDestinationIncludingOutOfBoundary(LaneType type, int moveIndex)
    {
        bool isValid = false;

        if (wg == null)
            return false;

        List<WayPointSystem.Waypoint> pointList = new List<WayPointSystem.Waypoint>();

        switch (type)
        {
            case LaneType.Zero:
                pointList = wg.waypoints_0;
                break;
            case LaneType.One:
                pointList = wg.waypoints_1;
                break;
            case LaneType.Two:
                pointList = wg.waypoints_2;
                break;
            case LaneType.Three:
                pointList = wg.waypoints_3;
                break;
            case LaneType.Four:
                pointList = wg.waypoints_4;
                break;
            case LaneType.Five:
                pointList = wg.waypoints_5;
                break;
            case LaneType.Six:
                pointList = wg.waypoints_6;
                break;
            default:
                break;
        }

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
            if (Vector3.Distance(transform.position, currentDest.Value) < PLAYER_VALID_REACH_DEST)
                isReached = true;

            if (Vector3.Distance(transform.position, currentDest.Value) < 3f
                && UtilityCommon.IsFront(transform.forward, transform.position, currentDest.Value) == false)
                isReached = true;
        }


        return isReached;
    }

    private bool IsLastLap(int lap) //������ Lap
    {
        return InGameManager.Instance.IsLastLap(lap);
    }

    public bool IsCurrentLastLap()
    {
        return IsLastLap(currentLap);
    }

    private bool IsFinishLap(int lap) //���� Lap
    {
        return InGameManager.Instance.IsFinishLap(lap);
    }

    public bool IsCurrentFinishLap()
    {
        return IsFinishLap(currentLap);
    }

    private bool IsLastIndex(LaneType type, int index)
    {
        bool isLast = false;

        if (wg == null)
            return false;

        List<WayPointSystem.Waypoint> pointList = new List<WayPointSystem.Waypoint>();

        switch (type)
        {
            case LaneType.Zero:
                pointList = wg.waypoints_0;
                break;
            case LaneType.One:
                pointList = wg.waypoints_1;
                break;
            case LaneType.Two:
                pointList = wg.waypoints_2;
                break;
            case LaneType.Three:
                pointList = wg.waypoints_3;
                break;
            case LaneType.Four:
                pointList = wg.waypoints_4;
                break;
            case LaneType.Five:
                pointList = wg.waypoints_5;
                break;
            case LaneType.Six:
                pointList = wg.waypoints_6;
                break;
            default:
                break;
        }

        if (pointList != null && pointList.Count > 0)
        {
            if (index >= pointList.Count - 1)
                isLast = true;
        }

        return isLast;
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
        collisionChecker_Player.enabled = true;
        collisionChecker_Ground.enabled = true;
        rgbdy.isKinematic = false;

        isMoving = true;

        SetAnimation_Both(AnimationState.Idle, false);
        SetAnimation_Both(AnimationState.Drive, true);

        if (wg == null)
            wg = InGameManager.Instance.wayPoints;

        if (IsMine)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Drive);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        collisionChecker_Ground.enabled = true;

        DeactivateBooster();

        if (playerCar != null)
        {
            playerCar.DeactivateWheelFX();
        }


        if (IsMine)
        {
            SoundManager.Instance.StopSound(SoundManager.SoundClip.Drive);
        }
    }

    private class WayPointIndexCheck
    {
        public int moveIndex = 0;
        public float distance = 0f;
    }

    //���� �ٲܶ� or Dest ���޽� ����(?)�� index�� �̵��ϵ��� ����
    private int GetNewMoveIndexAtRange(int index, LaneType laneType, Vector3 playerPos, float checkRange, int checkIndexNum = 0)
    {
        //������ finish lap�� ��� �Ժη� moveIndex�� �ǳʶٸ� �ȵ�!!!
        if (IsLastIndex(currentLaneType, index) == true
            && currentLap >= CommonDefine.GetFinalLapCount()
            && CommonDefine.GetFinalLapCount() != -1)
            return index;

        if (checkRange <= 0f)
            return index;

        int newIndex = index;
        var list = new List<WayPointIndexCheck>();

        if (checkIndexNum <= 0)
            checkIndexNum = PLAYER_MOVE_INDEX_CHECK_NUMBER;

        for (int i = 0; i < checkIndexNum; i++)
        {
            if (IsLastIndex(laneType, index + i) == true)
                break;

            int nextIndex = GetNextMoveIndex(laneType, index + i);
            var posi = GetWayPointPosition(laneType, nextIndex);
            if (posi.HasValue)
            {
                var dist = Vector3.Distance(playerPos, posi.Value);
                if (dist < checkRange)
                    list.Add(new WayPointIndexCheck() { moveIndex = nextIndex, distance = dist });
            }
        }

        if (list != null && list.Count > 0)
        {
            list.Sort((x, y) => y.distance.CompareTo(x.distance));
            newIndex = list[0].moveIndex;
        }
        else
        {
            //���� currentIndex ���...!
            newIndex = index;
        }

        return newIndex;
    }


    //Jitter ���󋚹��� �� fixedUpdate�� player �̵� ��������
    public override void FixedUpdateNetwork()
    {
        FixedUpdate_NetworkInput();
        FixedUpdate_SetCollider();
        FixedUpdate_Move();
        FixedUpdate_ChargeBattery();
        FixedUpate_Animation();

        FixedUpdate_TeleportIfLag();
        FixedUpdate_SetDriveSound();

        CameraManager.Instance.FixedUpdate_Cam();

        FixedUpdateNetworkCallback?.Invoke();
    }

    private void FixedUpdate_NetworkInput()
    {
        //network input ����...!

        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            if (GetInput(out TouchSwipe_NetworkInputData data))
            {
                switch (data.currentEventType)
                {
                    case TouchSwipe_NetworkInputData.EventType.None:
                        break;

                    case TouchSwipe_NetworkInputData.EventType.StartDrag:
                        {
                            PrefabManager.Instance.UI_PanelIngame.OnDragStartCallback();
                        }
                        break;
                    case TouchSwipe_NetworkInputData.EventType.Drag:
                        {
                            PrefabManager.Instance.UI_PanelIngame.OnDragCallback(data.position);
                        }
                        break;
                    case TouchSwipe_NetworkInputData.EventType.EndDrag:
                        {
                            PrefabManager.Instance.UI_PanelIngame.OnDragEndCallback();
                        }
                        break;
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

    private float autoBatteryChargeCounter = 0f;
    private void FixedUpdate_ChargeBattery()
    {
        if (IsMine)
        {
            if (!isOutOfBoundary && 
                (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown))
            {
                if (PLAYER_BATTERY_AUTOCHARGE_SPEED > 0)
                {
                    autoBatteryChargeCounter += Runner.DeltaTime;

                    if (autoBatteryChargeCounter > PLAYER_BATTERY_AUTOCHARGE_SPEED)
                    {
                        autoBatteryChargeCounter = 0;

                        if (currentBattery < PLAYER_BATTERY_MAX)
                            networkInGameRPCManager.RPC_SetPlayerBattery(PlayerID, true, 1);
                    }

                    if (currentBattery >= PLAYER_BATTERY_MAX)
                        currentBattery = PLAYER_BATTERY_MAX;
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
                if (isMoving == true && isOutOfBoundary == false && isFlipped == false && isEnteredTheFinishLine == false)
                {
                    // x�� �̻� ������ ���.. ������ ��Ʈ��ũ ������ �����������
                    if (lagTime > PLAYER_LAG_TELEPORT_TIME)
                    {
                        SetToNetworkValue();
                    }

                    // x �Ÿ� �̻� ������ ���.. ������ ��Ʈ��ũ ������ �����������
                    if (networkPos.HasValue)
                    {
                        if (Vector3.Distance(transform.position, networkPos.Value) > PLAYER_LAG_TELEPORT_DIST)
                        {
                            SetToNetworkValue();
                        }
                    }
                }
            }
        }
    }

    private void SetToNetworkValue()
    {
        UtilityCommon.ColorLog("PLAYER Lagging Hard!!!! Moved " + gameObject.name + " to network position...!", UtilityCommon.DebugColor.Red);

        if (networkMoveSpeed.HasValue)
            currentMoveSpeed = networkMoveSpeed.Value;

        if (networkLaneType != LaneType.None)
            currentLaneType = networkLaneType;

        if (networkMoveIndex.HasValue)
            currentMoveIndex = networkMoveIndex.Value;

        if (networkPos.HasValue)
        {
            var lagAdustment = currDirection * lagTime * Runner.DeltaTime * currentMoveSpeed;
            transform.position = networkPos.Value + lagAdustment;
        }

        if (networkLap.HasValue)
            currentLap = networkLap.Value;

        if (networkBattery.HasValue)
            currentBattery = networkBattery.Value;
    }

    private void FixedUpdate_SetDriveSound()
    {
        if (IsMine && isMoving)
        {
            float pitch = 1f;

            var MAX_PITCH = 1.7f;
            var MIN_PITCH = 0.45f;

            var speed = MAX_PITCH * (currentMoveSpeed / PLAYER_MAX_MOVE_SPEED);
            pitch = Mathf.Clamp(speed, MIN_PITCH, MAX_PITCH);
            SoundManager.Instance.SetSoundPitch(SoundManager.SoundClip.Drive, pitch);
        }
    }

    private void FixedUpate_Animation()
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.IsReady)
        {
            SetAnimation_Both(AnimationState.Idle, true);
        }

            
        if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
            || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
        {
            if (isEnteredTheFinishLine == false && currentMoveSpeed > 0f)
                SetAnimation_Both(AnimationState.Idle, false);

            if (isEnteredTheFinishLine)
            {
                SetAnimation_CharacterOnly(AnimationState.Idle, false);
                SetAnimation_CharacterOnly(AnimationState.Victory, true);
            }

        }

        if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
        {
            if (isEnteredTheFinishLine == false)
            {
                if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame && isMoving == false)
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
                SetAnimation_CharacterOnly(AnimationState.Victory, true);
            }

            SetAnimation_Both(AnimationState.MoveLeft, false);
            SetAnimation_Both(AnimationState.MoveRight, false);
            SetAnimation_Both(AnimationState.Drive, false);
        }


    }

    private void SetAnimation_Both(AnimationState state, bool isOn)
    {
        SetAnimation_CarOnly(state, isOn);
        SetAnimation_CharacterOnly(state, isOn);
    }

    private void SetAnimation_CarOnly(AnimationState state, bool isOn)
    {
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
                        carAnim.ResetTrigger(playerCar.GetString_ANIM_BOOSTER_1());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_BOOSTER_1());
                    }
                }
                break;
            case AnimationState.Booster_2:
                {
                    if (isOn)
                    {
                        carAnim.ResetTrigger(playerCar.GetString_ANIM_BOOSTER_2());
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
                        carAnim.ResetTrigger(playerCar.GetString_ANIM_FLIP());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_FLIP());
                    }
                }
                break;
            case AnimationState.Spin:
                {
                    if (isOn)
                    {
                        carAnim.ResetTrigger(playerCar.GetString_ANIM_SPIN());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_SPIN());
                    }
                }
                break;
        }
    }

    private void SetAnimation_CharacterOnly(AnimationState state, bool isOn)
    {
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
                        charAnim.ResetTrigger(playerCharacter.GetString_ANIM_BOOSTER_1());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_BOOSTER_1());
                    }
                }
                break;
            case AnimationState.Booster_2:
                {
                    if (isOn)
                    {
                        charAnim.ResetTrigger(playerCharacter.GetString_ANIM_BOOSTER_2());
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
                        charAnim.ResetTrigger(playerCharacter.GetString_ANIM_FLIP());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_FLIP());
                    }
                }
                break;
            case AnimationState.Spin:
                {
                    if (isOn)
                    {
                        charAnim.ResetTrigger(playerCharacter.GetString_ANIM_SPIN());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_SPIN());
                    }
                }
                break;
        }
    }

    private void FixedUpdate_Move()
    {
        if (isMoving && !isFlipped && !isOutOfBoundary)
        {
            if (IsWayPointSet() == false)
                return;

            rgbdy.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            if (IsMine)
            {
                currentMoveSpeed = GetCurrentMoveSpeed();
                currentRotationSpeed = GetCurrentRotationSpeed();
                currentDest = GetDestination(currentMoveIndex, currentLaneType);
                currDirection = GetCurrentDirection();

                //Position ����
                rgbdy.MovePosition(transform.position + currDirection * Runner.DeltaTime * currentMoveSpeed);

                //Rotation ����
                Vector3 fowardDir = GetFowardDirection();
                if (IsRotationLerpActivated())
                {
                    var lerpRotation = Vector3.Slerp(fowardDir, currDirection, Runner.DeltaTime * currentRotationSpeed);
                    rgbdy.rotation = Quaternion.LookRotation(lerpRotation, Vector3.up);
                }
                else
                {
                    Vector3 destDir;
                    if (currentDest.HasValue)
                        destDir = (currentDest.Value - transform.position).normalized;
                    else
                        destDir = transform.TransformDirection(Vector3.forward).normalized;

                    if (destDir != Vector3.zero)
                        rgbdy.rotation = Quaternion.LookRotation(destDir, Vector3.up);
                    else
                        rgbdy.rotation = Quaternion.Euler(destDir);
                }

                //��ǥ ���� ���޽�
                if (IsReachedToDestination() == true)
                {
                    if (GetDestinationWaypoint(currentMoveIndex, currentLaneType) == WayPointSystem.Waypoint.WayPointType.OutOfBoundary)
                    {
                        isOutOfBoundary = true;
                    }

                    previousReachDest = transform.position;
                    if (currentDest.HasValue)
                        previousDir = (currentDest.Value - transform.position).normalized;

                    if (IsLastIndex(currentLaneType, currentMoveIndex) == true)
                    {
                        //index = 0 ���� �ٽ� ����
                        currentMoveIndex = GetNextMoveIndex(currentLaneType, currentMoveIndex);
                        currentLap++;

                        networkInGameRPCManager.RPC_PlayerCompletedLap(PlayerID, currentLap);

                        //LAP FINISH!!!!!!!!!
                        if (IsFinishLap(currentLap))
                        {
                            if (isEnteredTheFinishLine == false && CommonDefine.GetFinalLapCount() != -1)
                            {
                                if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
                                    || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
                                {
                                    isEnteredTheFinishLine = true;
                                    networkInGameRPCManager.RPC_PlayerPassedFinishLine(PlayerID);
                                }

                                if (playerCar != null)
                                {
                                    Camera_Base.Instance.TurnOffCameraFX();
                                    playerCar.DeactivateStunFX();
                                }
                            }
                        }
                    }
                    else
                    {
                        //���� �ӵ��� ����ؼ� check�� Index���� ������....
                        var checkIndex = (int)System.Math.Truncate(Mathf.Lerp(1, 3, (Mathf.Clamp((currentMoveSpeed - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED) / Mathf.Clamp((PLAYER_MAX_MOVE_SPEED - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED))));

                        //���� �ε����� ����...
                        int newMoveIndex = GetNewMoveIndexAtRange(currentMoveIndex, currentLaneType, transform.position, PLAYER_MOVE_INDEX_CHECK_MIN_RANGE, checkIndex);
                        if (newMoveIndex != currentMoveIndex || IsLastIndex(currentLaneType, newMoveIndex) == true)
                            currentMoveIndex = newMoveIndex;
                        else
                            currentMoveIndex = GetNextMoveIndex(currentLaneType, currentMoveIndex);
                    }

                    //���� �ε����� valid�� dest���� Ȯ��...!
                    if (IsValidDestinationIncludingOutOfBoundary(currentLaneType, currentMoveIndex) == false)
                    {
                        //move to a valid lane
                        SetValidDestination(ref currentLaneType, ref currentMoveIndex);
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

                    isChangingLane = false;
                }

                if (isEnteredTheFinishLine == true && currentMoveSpeed <= 0f)
                {
                    //������ ���� ����...
                    StopMoving();
                }
            }

            // IsMine�� �ƴ� ���...! Network ����ȭ?
            else
            {
                if (isFlipped == false)
                {
                    collisionChecker_Player.enabled = true;
                }

                //Ŭ���̾�Ʈ���� moveIndex ���� networkIndex ������ �� ���� ��쿡�� ����x ���� moveIndex�� �������
                if (networkMoveIndex.HasValue)
                {
                    if (networkMoveIndex.Value < wg.waypoints_3.Count - 1)
                    {
                        if (currentMoveIndex <= networkMoveIndex.Value)
                            currentMoveIndex = networkMoveIndex.Value;
                        else
                        {

                        }
                    }
                    else
                    {
                        if (currentMoveIndex == 0)
                        {
                            currentMoveIndex = 0;
                        }
                    }
                }

                //currentLap�� networkLap value�� �������� ���...!
                if (networkLap.HasValue)
                {
                    currentLap = networkLap.Value;
                }

                ///���� CurrDirection��������...! Lag Pos ������� //��ȭ���� �ʰ����� ŷ¿�� ��...
                var lagPos = currDirection * lagTime * Runner.DeltaTime * currentMoveSpeed;

                currentDest = GetDestination(currentMoveIndex, currentLaneType) + lagPos;
                currDirection = GetCurrentDirection();
                currentMoveSpeed = GetCurrentMoveSpeed();
                currentRotationSpeed = GetCurrentRotationSpeed();

                var nextPosition = transform.position + currDirection * Runner.DeltaTime * currentMoveSpeed;
                nextPosition += lagPos;

                /*
                //��Ʈ��ũ ���ϰ� Ŭ���̾�Ʈ �� �����̰� ���� ��� ����������...!
                if (IsPositionLagging() == true)
                {
                    IsLagging = true;

                    var clientPosition = transform.position;
                    var networkPosition = networkPos.Value + lagPos;

                    if (UtilityCommon.IsFront(currDirection, networkPosition, clientPosition))
                    {
                        //�ӵ��� �ٿ��� ������
                        nextPosition -= currDirection * Time.fixedDeltaTime * currentMoveSpeed * playerLagAdjustRate;
                    }
                    else
                    {
                        //�ӵ��� ������ ������
                        nextPosition += currDirection * Time.fixedDeltaTime * currentMoveSpeed * playerLagAdjustRate;
                    }
                }
                else
                    IsLagging = false;
                */

                //Position ����
                rgbdy.MovePosition(nextPosition);

                //Rotation ����
                Vector3 fowardDir = GetFowardDirection();
                if (IsRotationLerpActivated())
                {
                    var lerpRotation = Vector3.Slerp(fowardDir, currDirection, Runner.DeltaTime * currentRotationSpeed);
                    rgbdy.rotation = Quaternion.LookRotation(lerpRotation, Vector3.up);
                }
                else
                {
                    Vector3 destDir;
                    if (currentDest.HasValue)
                        destDir = (currentDest.Value - transform.position).normalized;
                    else
                        destDir = transform.TransformDirection(Vector3.forward).normalized;

                    if (destDir != Vector3.zero)
                        rgbdy.rotation = Quaternion.LookRotation(destDir, Vector3.up);
                    else
                        rgbdy.rotation = Quaternion.Euler(destDir);
                }


                if (IsReachedToDestination() == true)
                {
                    if (IsLastIndex(currentLaneType, currentMoveIndex) == true)
                    {
                        //index = 0 ���� �ٽ� ����
                        currentMoveIndex = GetNextMoveIndex(currentLaneType, currentMoveIndex);

                        networkInGameRPCManager.RPC_PlayerCompletedLap(PlayerID, currentLap);
                    }
                    else
                    {
                        var checkIndex = (int)System.Math.Truncate(Mathf.Lerp(1, 3, (Mathf.Clamp((currentMoveSpeed - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED) / Mathf.Clamp((PLAYER_MAX_MOVE_SPEED - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED))));

                        int newMoveIndex = GetNewMoveIndexAtRange(currentMoveIndex, currentLaneType, transform.position, PLAYER_MOVE_INDEX_CHECK_MIN_RANGE, checkIndex);
                        if (newMoveIndex != currentMoveIndex || IsLastIndex(currentLaneType, newMoveIndex) == true)
                            currentMoveIndex = newMoveIndex;
                        else
                            currentMoveIndex = GetNextMoveIndex(currentLaneType, currentMoveIndex);
                    }

                    //���� �ε����� valid�� dest���� Ȯ��...!
                    if (IsValidDestinationIncludingOutOfBoundary(currentLaneType, currentMoveIndex) == false)
                    {
                        //move to a valid lane
                        SetValidDestination(ref currentLaneType, ref currentMoveIndex);
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
                    isChangingLane = false;
                }

                if (isEnteredTheFinishLine == true && currentMoveSpeed <= 0f)
                {
                    //������ ���� ����...
                    StopMoving();
                }
            }
        }
        else if (isOutOfBoundary)
        {
            outOfBoundaryTimer += Runner.DeltaTime;
            outOfBoundaryMoveSpeed = GetCurrentMoveSpeed();

            if (outOfBoundaryTimer > PLAYER_OUT_OF_BOUNDARY_TIME)
            {
                if (IsMine && isEnteredTheFinishLine == false)
                {
                    MovePlayerToPosition();
                    outOfBoundaryTimer = 0f;
                }
            }
            else
            {
                collisionChecker_Player.enabled = false;
                rgbdy.useGravity = true;
                //rgbdy.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                rgbdy.constraints = RigidbodyConstraints.None;

                var fallingDir = UtilityCommon.GetDirectionByAngle_YZ(-45f, transform.TransformDirection(Vector3.forward));
                rgbdy.MovePosition(transform.position + fallingDir * Runner.DeltaTime * outOfBoundaryMoveSpeed);
            }

        }
        else if (isFlipped)
        {
            rgbdy.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            if (isFlippingUp)
            {
                rgbdy.MovePosition(transform.position + Vector3.up * Runner.DeltaTime * PLAYER_FLIP_UP_SPEED);
            }
            else
            {
                if (isGrounded == false)
                    rgbdy.MovePosition(transform.position + Vector3.down * Runner.DeltaTime * PLAYER_FLIP_DOWN_SPEED * 1.5f);
            }

        }
        else
        {

        }
    }

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
            isMoving = false;
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

    public WayPointSystem.Waypoint.WayPointType GetDestinationWaypoint(int moveIndex, LaneType laneType)
    {
        if (wg == null)
            return WayPointSystem.Waypoint.WayPointType.Normal;

        var list_0 = wg.waypoints_0;
        var list_1 = wg.waypoints_1;
        var list_2 = wg.waypoints_2;
        var list_3 = wg.waypoints_3;
        var list_4 = wg.waypoints_4;
        var list_5 = wg.waypoints_5;
        var list_6 = wg.waypoints_6;


        WayPointSystem.Waypoint.WayPointType type = WayPointSystem.Waypoint.WayPointType.Normal;
        switch (laneType)
        {
            case LaneType.Zero:
                type = list_0[moveIndex].currentWayPointType;
                break;
            case LaneType.One:
                type = list_1[moveIndex].currentWayPointType;
                break;
            case LaneType.Two:
                type = list_2[moveIndex].currentWayPointType;
                break;
            case LaneType.Three:
                type = list_3[moveIndex].currentWayPointType;
                break;
            case LaneType.Four:
                type = list_4[moveIndex].currentWayPointType;
                break;
            case LaneType.Five:
                type = list_5[moveIndex].currentWayPointType;
                break;
            case LaneType.Six:
                type = list_6[moveIndex].currentWayPointType;
                break;
            default:
                type = list_3[moveIndex].currentWayPointType;
                break;
        }

        return type;
    }

    public Vector3 GetCurrentDirection()
    {
        Vector3 dir = Vector3.zero;

        //�ʹݿ� �鸮�� ���󶧹��� �����Ҷ��� dest������ �ƴ� ���� ���ؼ� ����...!!
        if (currentLap == 0 && (currentMoveIndex == 0 || currentMoveIndex == 1))
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

                //�ӵ� ����ؼ� lerpSpeed ��������
                lerpSpeed = Mathf.Lerp(PLAYER_DIRECTION_LERP_MIN_SPEED, PLAYER_DIRECTION_LERP_MAX_SPEED, (Mathf.Clamp((currentMoveSpeed - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED) / Mathf.Clamp((PLAYER_MAX_MOVE_SPEED - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED)));
                //��ǥ���� angle�� Ŭ ��� �߰� ���� ����
                lerpSpeed += Mathf.Lerp(PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MIN_SPEED, PLAYER_DIRECTION_LERP_ADDITIONAL_ANGLE_MAX_SPEED, Mathf.Clamp(1f - ((float)(45 - angle) / 45f), 0f, 1f));

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

    private float GetCurrentMoveSpeed()
    {
        float speed;

        SetDecelerationSpeed();
        SetAdditionalBuffSpeed();
        SetSpeedLimit();

        speed = PLAYER_DEFAULT_MOVE_SPEED + additionalCarBoosterSpeed + additionalChargePadSpeed + additionalGroundSpeed + additionalDecelerationSpeed + additionalStunSpeed + additionalbuffSpeed;

        if (isFlipped)
            speed = 0f;

        if (speed >= speedMaxLimit)
        {
            speed = speedMaxLimit;
        }
        else if (speed <= 0f)
            speed = 0f;

        //���� ���� �Ǵ� ����� ��� ���Ŀ� �ڵ����� �ӵ� �ٿ�����....
        if (isEnteredTheFinishLine || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
        {
            speed -= timeCounterAfterEnteringFinishingLine * PLAYER_REDUCE_STRENGTH_AFTER_FINISHLINE;

            if (speed <= 0f)
                speed = 0f;
            else
                timeCounterAfterEnteringFinishingLine += Runner.DeltaTime;
        }

        //��ܷ� ������...
        if (isOutOfBoundary)
        {
            speed = PLAYER_DEFAULT_MOVE_SPEED + additionalCarBoosterSpeed + additionalChargePadSpeed + additionalGroundSpeed + additionalDecelerationSpeed + additionalStunSpeed + additionalbuffSpeed;
        }

        return speed;
    }

    private void SetDecelerationSpeed()
    {
        if (isDecelerating)
        {
            additionalDecelerationSpeed -= Runner.DeltaTime * PLAYER_DECELERATION_DECREASE_SPEED;

            if (additionalDecelerationSpeed <= (-1 * PLAYER_DECELERATION_MAX_SPEED))
            {
                additionalDecelerationSpeed = (-1 * PLAYER_DECELERATION_MAX_SPEED);
            }
        }
        else
        {
            additionalDecelerationSpeed += Runner.DeltaTime * PLAYER_DECELERATION_RECOVERY_SPEED;

            if (additionalDecelerationSpeed >= 0)
                additionalDecelerationSpeed = 0f;
        }
    }

    private void SetAdditionalBuffSpeed()
    {
        if (InGameManager.Instance.myCurrentRank != -1)
        {
            additionalbuffSpeed = (float)(InGameManager.Instance.myCurrentRank - 1) * PLAYER_MOVE_SPEED_BUFF;
        }
        else
            additionalbuffSpeed = 0f;
    }

    private void SetSpeedLimit()
    {
        //������ �հ� �������� ���� ����
        if (IsPlayerInFront == true && triggerChecker_Front.listOfCollision.Count > 0)
        {
            if (isCarAttackBoosting == false)
            {
                var otherP = InGameManager.Instance.GetPlayerInfo(playerInFrontViewID);
                if (otherP != null && otherP.pm != null && otherP.pm.isEnteredTheFinishLine == false && otherP.pm.currentMoveSpeed > 0f)
                {
                    if (otherP.pm.isParrying)
                    {
                        speedMaxLimit = otherP.pm.currentMoveSpeed + 0.5f;
                    }
                    else
                    {
                        //2f �� �� ������� IsPlayerInFront on /off ������ �߻�x ������ �̼��ϰ� ���� 
                        speedMaxLimit = otherP.pm.currentMoveSpeed + 2f;
                    }
                }
                else
                    speedMaxLimit = PLAYER_MAX_MOVE_SPEED;
            }
            else
            {
                //�ν��� ���¿��� ����x?
                speedMaxLimit = PLAYER_MAX_MOVE_SPEED;
            }
        }
        else
        {
            IsPlayerInFront = false;
            speedMaxLimit = PLAYER_MAX_MOVE_SPEED;
        }
    }

    public void SetGroundSpeed(MapObject_Ground.GroundType type, float speed = 0)
    {
        switch (type)
        {
            case MapObject_Ground.GroundType.Mud:
                {
                    additionalGroundSpeed = speed;
                }
                break;

            case MapObject_Ground.GroundType.None:
            default:
                {
                    additionalGroundSpeed = 0f;
                }
                break;
        }
    }

    private float GetCurrentRotationSpeed()
    {
        float speed = 0f;

        //����...!
        if (IsMine)
        {
            speed = PLAYER_DEFAULT_ROTATION_SPEED;
        }
        //Network...  ����
        else
        {
            //TODO... �̰� ����...? networkRotationSpeed�� �������°� �������� ���� �ȵ�
            speed = PLAYER_DEFAULT_ROTATION_SPEED;
        }

        return speed;
    }

    private bool IsRotationLerpActivated()
    {
        Vector3 destDir;
        if (currentDest.HasValue)
        {
            destDir = (currentDest.Value - transform.position).normalized;
        }
        else
        {
            destDir = transform.TransformDirection(Vector3.forward).normalized;
        }

#if UNITY_EDITOR
        //Debug.Log("angle: " + Vector3.Angle(previousDir, destDir) + "   | previousDir: " + previousDir + "  | destDir:" + destDir);
#endif

        //Angle�� ������ lerp���� ���� ������ Dir���� �̵���������
        if (Vector3.Angle(previousDir, destDir) < 0.05f)
        {
            return false;
        }
        else
            return true;
    }

    public int previousMoveIndex
    {
        get
        {
            if (currentMoveIndex > 0)
                return currentMoveIndex - 1;
            else
            {
                if (wg != null && wg.waypoints_1 != null)
                {
                    var lastIndex = wg.waypoints_1.Count - 1;

                    return lastIndex;
                }
                else
                    return 0;
            }
        }
    }


    public int GetBatteryCost(CarBoosterLevel lv)
    {
        int cost = 0; //���...! 0�̸��ϰ�� �Ҹ� 0�̻��� ��� ����
        switch (lv)
        {
            case CarBoosterLevel.None:
                cost = 0;
                break;
            case CarBoosterLevel.Car_One:
                cost = -PLAYER_BATTERY_CARBOOSTER1_COST;
                break;
            case CarBoosterLevel.Car_Two:
                cost = -PLAYER_BATTERY_CARBOOSTER2_COST;
                break;
            case CarBoosterLevel.Car_Three:
                cost = -PLAYER_BATTERY_CARBOOSTER3_COST;
                break;
            default:
                break;
        }

        return cost;
    }

    public int GetBatteryCost(ChargePadBoosterLevel lv)
    {
        int cost = 0; //���...! 0�̸��ϰ�� �Ҹ� 0�̻��� ��� ����
        switch (lv)
        {

            case ChargePadBoosterLevel.ChargePad_One:
                cost = PLAYER_BOOSTER_CHARGEPAD_LV1_CHARGE_AMOUNT;
                break;
            case ChargePadBoosterLevel.ChargePad_Two:
                cost = PLAYER_BOOSTER_CHARGEPAD_LV2_CHARGE_AMOUNT;
                break;
            case ChargePadBoosterLevel.ChargePad_Three:
                cost = PLAYER_BOOSTER_CHARGEPAD_LV3_CHARGE_AMOUNT;
                break;
            default:
                break;
        }

        return cost;
    }

    public void SetCurrentBattery(int battery)
    {
        currentBattery = battery;

        if (currentBattery >= PLAYER_BATTERY_MAX)
            currentBattery = PLAYER_BATTERY_MAX;
        else if (currentBattery <= 0)
            currentBattery = 0;
    }

    public void SetCurrentBatteryAdded(int addedBattery)
    {
        currentBattery = currentBattery + addedBattery;

        if (currentBattery >= PLAYER_BATTERY_MAX)
            currentBattery = PLAYER_BATTERY_MAX;
        else if (currentBattery <= 0)
            currentBattery = 0;
    }

    public void UseBattery(CarBoosterLevel lv)
    {
        int cost = GetBatteryCost(lv);

        if (IsValidBooster(lv) && IsMine)
        {
            previousBatteryAtBooster = currentBattery;
            networkInGameRPCManager.RPC_SetPlayerBattery(PlayerID, true, cost, (int)NetworkInGameRPCManager.SetBatteryType.CarBooster);
        }
    }

    public void UseBattery(ChargePadBoosterLevel lv)
    {
        int cost = GetBatteryCost(lv);

        if (IsValidBooster(lv) && IsMine)
        {
            networkInGameRPCManager.RPC_SetPlayerBattery(PlayerID, true, cost, (int)NetworkInGameRPCManager.SetBatteryType.ChargePadBooster);
        }
    }

    public bool IsValidBooster(CarBoosterLevel lv)
    {
        bool isValid;
        int cost = GetBatteryCost(lv);

        if (cost <= 0)
        {
            if (currentBattery < -cost) //����� �� ū ���
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

    public bool IsValidBooster(ChargePadBoosterLevel lv)
    {
        bool isValid;
        int cost = GetBatteryCost(lv);

        if (cost <= 0)
        {
            if (currentBattery < -cost) //����� �� ū ���
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

    public CarBoosterLevel GetAvailableInputBooster()
    {
        if (IsValidBooster(CarBoosterLevel.Car_Three))
            return CarBoosterLevel.Car_Three;

        if (IsValidBooster(CarBoosterLevel.Car_Two))
            return CarBoosterLevel.Car_Two;

        //level 1 �� ���... Ư�� ��Ȳ���� ������ ���ֱ⸸��... Input�� ���ؼ��� x... ���ܽ�Ű��
        /*
        if (IsValidBooster(CarBoosterLevel.Car_One))
            return CarBoosterLevel.Car_One;
        */

        return CarBoosterLevel.None;
    }


    public void Car_BoostSpeed(CarBoosterLevel lv)
    {
        if (isEnteredTheFinishLine)
            return;

        UtilityCoroutine.StopCoroutine(ref carbooster, this);
        UtilityCoroutine.StartCoroutine(ref carbooster, Car_Booster(lv), this);
    }

    private IEnumerator carbooster = null;
    private IEnumerator Car_Booster(CarBoosterLevel lv)
    {
        currentCarBoosterLv = lv;

        bool overrideFXAndAnim = false; //FXȿ��Anim�� ���� ������... Booster ���¿��� �� Booster�ɸ��� ����... 

        //�ڵ����� ���� �׳� ������ �ִϸ��̼� ��������...
        overrideFXAndAnim = true;

        if (IsMine)
        {
            if (overrideFXAndAnim)
            {
                Camera_Base.Instance.TurnOffCameraFX();
            }

        }


        float boosterMaxSpeed = 0f;
        float boosterDecreaseSpeed = 0f;
        float boosterMaxSpeedDuration = 0f;

        switch (currentCarBoosterLv)
        {
            case CarBoosterLevel.None:
                break;

            case CarBoosterLevel.Car_One:
                {
                    boosterMaxSpeed = PLAYER_BOOSTER_CAR_LV1_MAX_SPEED;
                    boosterDecreaseSpeed = PLAYER_BOOSTER_CAR_LV1_DECREASE_SPEED;
                    boosterMaxSpeedDuration = PLAYER_BOOSTER_CAR_LV1_MAXSPEED_DURATION_TIME;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(currentCarBoosterLv);
                        if (IsMine)
                            Camera_Base.Instance.FX_0.SafeSetActive(true);
                        SetAnimation_CharacterOnly(AnimationState.Booster_1, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);
                    }
                }
                break;
            case CarBoosterLevel.Car_Two:
                {
                    boosterMaxSpeed = PLAYER_BOOSTER_CAR_LV2_MAX_SPEED;
                    boosterDecreaseSpeed = PLAYER_BOOSTER_CAR_LV2_DECREASE_SPEED;
                    boosterMaxSpeedDuration = PLAYER_BOOSTER_CAR_LV2_MAXSPEED_DURATION_TIME;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(currentCarBoosterLv);
                        if (IsMine)
                            Camera_Base.Instance.FX_1.SafeSetActive(true);
                        SetAnimation_Both(AnimationState.Booster_2, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);

                        if(IsMine)
                            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Booster_02);
                    }
                }
                break;
            case CarBoosterLevel.Car_Three:
                {
                    boosterMaxSpeed = PLAYER_BOOSTER_CAR_LV3_MAX_SPEED;
                    boosterDecreaseSpeed = PLAYER_BOOSTER_CAR_LV3_DECREASE_SPEED;
                    boosterMaxSpeedDuration = PLAYER_BOOSTER_CAR_LV3_MAXSPEED_DURATION_TIME;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(currentCarBoosterLv);
                        if (IsMine)
                            Camera_Base.Instance.FX_2.SafeSetActive(true);
                        SetAnimation_Both(AnimationState.Booster_2, true);
                        SetAnimation_Both(AnimationState.MoveLeft, false);
                        SetAnimation_Both(AnimationState.MoveRight, false);

                        if (IsMine)
                            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Booster_03);
                    }
                }
                break;
        }


        additionalCarBoosterSpeed = boosterMaxSpeed;

        yield return new WaitForSecondsRealtime(boosterMaxSpeedDuration);

        if (overrideFXAndAnim)
        {
            if (IsMine)
            {
                Camera_Base.Instance.TurnOffCameraFX();
            }
        }

        while (true)
        {
            additionalCarBoosterSpeed -= Runner.DeltaTime * boosterDecreaseSpeed;
            yield return new WaitForFixedUpdate();

            if (additionalCarBoosterSpeed <= 0f)
            {
                additionalCarBoosterSpeed = 0f;
                break;
            }
        }

        playerCar.DeactivateBoosterFX_2and3();
        currentCarBoosterLv = CarBoosterLevel.None;

        while (true)
        {
            if (additionalCarBoosterSpeed <= 0 && additionalChargePadSpeed <= 0f)
                break;

            yield return new WaitForFixedUpdate();
        }

        //Initialize booster settings

        if (additionalCarBoosterSpeed <= 0 && additionalChargePadSpeed <= 0f)
        {
            playerCar.DeactivateAllBoosterFX();

            if (overrideFXAndAnim)
            {
                if (isChangingLane == false)
                    SetAnimation_Both(AnimationState.Drive, true);
            }
            currentCarBoosterLv = CarBoosterLevel.None;
        }
    }

    public void ChargePad_BoostSpeed(ChargePadBoosterLevel lv)
    {
        if (isEnteredTheFinishLine)
            return;

        UtilityCoroutine.StopCoroutine(ref chargePadBooster, this);
        UtilityCoroutine.StartCoroutine(ref chargePadBooster, ChargePad_Booster(lv), this);
    }

    private IEnumerator chargePadBooster = null;
    private IEnumerator ChargePad_Booster(ChargePadBoosterLevel lv)
    {
        currentChargeBoosterLv = lv;

        bool overrideFXAndAnim = false;
        if (currentCarBoosterLv != CarBoosterLevel.Car_Two && currentCarBoosterLv != CarBoosterLevel.Car_Three)
            overrideFXAndAnim = true;

        if (IsMine)
        {
            if (overrideFXAndAnim)
            {
                Camera_Base.Instance.TurnOffCameraFX();
            }

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.ChargePad);
        }


        float boosterMaxSpeed = 0f;
        float boosterDecreaseSpeed = 0f;

        switch (currentChargeBoosterLv)
        {
            case ChargePadBoosterLevel.ChargePad_One:
                {
                    boosterMaxSpeed = PLAYER_BOOSTER_CHARGEPAD_LV1_MAX_SPEED;
                    boosterDecreaseSpeed = PLAYER_BOOSTER_CHARGEPAD_LV1_DECREASE_SPEED;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(currentChargeBoosterLv);
                        SetAnimation_CharacterOnly(AnimationState.Booster_1, true);
                    }
                }
                break;
            case ChargePadBoosterLevel.ChargePad_Two:
                {
                    boosterMaxSpeed = PLAYER_BOOSTER_CHARGEPAD_LV2_MAX_SPEED;
                    boosterDecreaseSpeed = PLAYER_BOOSTER_CHARGEPAD_LV2_DECREASE_SPEED;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(currentChargeBoosterLv);
                        SetAnimation_Both(AnimationState.Booster_1, true);
                    }
                }
                break;
            case ChargePadBoosterLevel.ChargePad_Three:
                {
                    boosterMaxSpeed = PLAYER_BOOSTER_CHARGEPAD_LV3_MAX_SPEED;
                    boosterDecreaseSpeed = PLAYER_BOOSTER_CHARGEPAD_LV3_DECREASE_SPEED;

                    if (overrideFXAndAnim)
                    {
                        playerCar.ActivateBoosterFx(currentChargeBoosterLv);
                        SetAnimation_Both(AnimationState.Booster_1, true);
                    }
                }
                break;
        }

        additionalChargePadSpeed = boosterMaxSpeed;

        while (true)
        {
            additionalChargePadSpeed -= Runner.DeltaTime * boosterDecreaseSpeed;
            yield return new WaitForFixedUpdate();

            if (additionalChargePadSpeed <= 0f)
            {
                additionalChargePadSpeed = 0f;
                break;
            }
        }

        while (true)
        {
            if (additionalCarBoosterSpeed <= 0 && additionalChargePadSpeed <= 0f)
                break;

            yield return new WaitForFixedUpdate();
        }

        //Initialize booster settings

        if (additionalCarBoosterSpeed <= 0 && additionalChargePadSpeed <= 0f)
        {
            playerCar.DeactivateAllBoosterFX();

            if (IsMine)
            {
                if (overrideFXAndAnim)
                {
                    Camera_Base.Instance.TurnOffCameraFX();
                }
            }

            if (overrideFXAndAnim)
                SetAnimation_Both(AnimationState.Drive, true);

            currentChargeBoosterLv = ChargePadBoosterLevel.None;
        }
    }


    public void DeactivateBooster()
    {
        UtilityCoroutine.StopCoroutine(ref chargePadBooster, this);
        UtilityCoroutine.StopCoroutine(ref carbooster, this);
        currentChargeBoosterLv = ChargePadBoosterLevel.None;
        currentCarBoosterLv = CarBoosterLevel.None;

        Camera_Base.Instance.TurnOffCameraFX();

        additionalCarBoosterSpeed = 0f;
        additionalChargePadSpeed = 0f;

        playerCar.DeactivateAllBoosterFX();
    }

    public void StartParrying()
    {
        UtilityCoroutine.StopCoroutine(ref parrying, this);
        UtilityCoroutine.StartCoroutine(ref parrying, Parrying(), this);
    }

    private IEnumerator parrying = null;
    private IEnumerator Parrying()
    {
        DeactivateBooster();

        isParrying = true;
        playerCar.ActivateShieldFX();

        if (IsMine)
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.ShieldOn);

        float timer = PLAYER_PARRYING_TIME;
        while (true)
        {
            //PLAYER_PARRYING_TIME �� 0���� ������ ������ ������.... isDecelerating ==false �ɶ� ���� ��
            if (PLAYER_PARRYING_TIME > 0)
            {
                timer -= Runner.DeltaTime;

                if (timer < 0)
                    break;
            }

            yield return new WaitForFixedUpdate();
        }

        StopParring();
    }

    private void StopParring()
    {
        UtilityCoroutine.StopCoroutine(ref parrying, this);
        playerCar.DeactivateShieldFX();
        isParrying = false;

        if (IsMine)
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.ShieldOff);

        if (PLAYER_PARRYING_COOLTIME > 0)
        {
            if (isParryingCooltime == false)
            {
                isParryingCooltime = true;
                UtilityCoroutine.StopCoroutine(ref setParryingCoolTime, this);
                UtilityCoroutine.StartCoroutine(ref setParryingCoolTime, SetParryingCoolTime(), this);
            }
        }
        else
            isParryingCooltime = false;
    }

    private IEnumerator setParryingCoolTime = null;
    private IEnumerator SetParryingCoolTime()
    {
        parryingCoolTimeLeft = PLAYER_PARRYING_COOLTIME;

        while (true)
        {
            parryingCoolTimeLeft -= Runner.DeltaTime;

            if (parryingCoolTimeLeft <= 0)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        isParryingCooltime = false;
    }

    public float GetParryingCoolTimeLeftRate()
    {
        if (parryingCoolTimeLeft > 0)
        {
            return parryingCoolTimeLeft / PLAYER_PARRYING_COOLTIME;
        }
        else
            return 0f;
    }


    //���� �� <-> Ŭ���̾�Ʈ �� ���� ����...!
    public bool IsPositionLagging()
    {
        if (networkPos.HasValue == true)
        {
            var clientPosition = transform.position;
            var networkPosition = networkPos.Value;

            var lagPosi = currDirection * lagTime * Runner.DeltaTime * currentMoveSpeed;
            networkPosition += lagPosi;


            var diff = Vector3.Distance(clientPosition, networkPosition);

#if UNITY_EDITOR
            if (IsLagTestDebugOn == true) //�ν����ͻ󿡼� üũ������
            {
                if (diff >= PLAYER_LAG_MIN_DISTANCE)
                    Debug.Log("<color=red>DIFF- " + System.Math.Round(diff, 4) + "</color>");
                else
                    Debug.Log("DIFF- " + System.Math.Round(diff, 4));
            }
#endif

            if (diff > PLAYER_LAG_MIN_DISTANCE)
            {
                playerLagAdjustRate = GetPlayerLagAdjustRate(diff);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public float GetPlayerLagAdjustRate(float currentLagDist)
    {
        float lagRate = 0f;

        if (PLAYER_LAG_MIN_DISTANCE >= PLAYER_LAG_MAX_DISTANCE
            || PLAYER_LAG_MIN_RATE >= PLAYER_LAG_MAX_RATE)
            return PLAYER_LAG_MIN_RATE;

        currentLagDist = Mathf.Clamp(currentLagDist, PLAYER_LAG_MIN_DISTANCE, PLAYER_LAG_MAX_DISTANCE);

        float percentage = ((currentLagDist - PLAYER_LAG_MIN_DISTANCE) / (PLAYER_LAG_MAX_DISTANCE - PLAYER_LAG_MIN_DISTANCE));

        lagRate = PLAYER_LAG_MIN_RATE + (PLAYER_LAG_MIN_RATE / (PLAYER_LAG_MAX_RATE - PLAYER_LAG_MIN_RATE)) * percentage;
        lagRate = Mathf.Clamp(lagRate, PLAYER_LAG_MIN_RATE, PLAYER_LAG_MAX_RATE);


        var avgPing = PhotonNetworkManager.Instance.GetAveragePing();
        var bd = DataManager.Instance.basicData;
        if (bd != null && bd.playerData != null)
        {
            lagRate *= bd.playerData.PLAYER_LAG_PING_ADJUSTMENT_RATE;
        }

#if UNITY_EDITOR
        if (IsLagTestDebugOn == true) //�ν����ͻ󿡼� üũ������
            Debug.Log("percentage: " + percentage + "   lagRate: " + lagRate);
#endif

        return lagRate;
    }


    //UI ����� MoveSpeed ��ġ
    public double GetShowMoveSpeed()
    {
        double speed = 0f;

        if (rgbdy != null)
        {
            if (isFlipped == false && isOutOfBoundary == false)
                speed = System.Math.Round(currentMoveSpeed, 1);
            else
                speed = 0;
        }

        return speed * PLAYER_SHOW_MOVE_SPEED_MULTIPYER;
    }

    public Vector3 GetPlayerDirection()
    {
        Vector3 dir = Vector3.zero;
        dir = currDirection;

        return dir;
    }

    public List<WayPointSystem.Waypoint> GetCurrentWayPoints()
    {
        if (wg == null)
            return null;

        var dest = Vector3.zero;
        switch (currentLaneType)
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
        if (currWay != null && currentMoveIndex < currWay.Count && currentMoveIndex >= 0)
        {
            var pos1 = currWay[GetNextMoveIndex(currentLaneType, currentMoveIndex)].GetPosition();
            var pos2 = currWay[currentMoveIndex].GetPosition();

            dir = (pos1 - pos2).normalized;
        }


        return dir;
    }

    //�����ϴ� �� �÷��̾���� �Ÿ�
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
    //Lap ���ᶧ���� ���� �ּ� �Ÿ�
    public float GetMinDistLeft()
    {
        float distLeft = 0f;
        List<WayPointSystem.Waypoint> list = new List<WayPointSystem.Waypoint>();

        //ù��° point
        switch (currentLaneType)
        {
            case LaneType.Zero:
                list.Add(wg.waypoints_0[currentMoveIndex]);
                break;
            case LaneType.One:
                list.Add(wg.waypoints_1[currentMoveIndex]);
                break;
            case LaneType.Two:
                list.Add(wg.waypoints_2[currentMoveIndex]);
                break;
            case LaneType.Three:
                list.Add(wg.waypoints_3[currentMoveIndex]);
                break;
            case LaneType.Four:
                list.Add(wg.waypoints_4[currentMoveIndex]);
                break;
            case LaneType.Five: 
                list.Add(wg.waypoints_5[currentMoveIndex]);
                break;
            case LaneType.Six:
                list.Add(wg.waypoints_6[currentMoveIndex]);
                break;
            default:
                break;
        }

        //�߰� ��� point��
        if (currentMoveIndex < wg.waypoints_3.Count)
        {
            for (int i = currentMoveIndex + 1; i < wg.waypoints_3.Count; i++)
            {
                list.Add(wg.waypoints_3[i]);
            }
        }

        //������ point
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

    public float GetColliderLength() //����
    {
        if (collisionChecker_Player != null)
            return collisionChecker_Player.coll.bounds.size.z;
        else
            return 0f;
    }

    public float GetColliderWidth() //��
    {
        if (collisionChecker_Player != null)
            return collisionChecker_Player.coll.bounds.size.x;
        else
            return 0f;
    }

    public float GetColliderHeight() //����
    {
        if (collisionChecker_Player != null)
            return collisionChecker_Player.coll.bounds.size.y;
        else
            return 0f;
    }

    public float GetCurrentBatteryPercent()
    {
        if (PLAYER_BATTERY_MAX > 0)
            return GetCurrentBatteryRate() * PLAYER_BATTERY_MAX;
        else
            return 0;
    }

    public float GetCurrentBatteryRate()
    {
        if (PLAYER_BATTERY_MAX > 0)
            return (float)currentBattery / PLAYER_BATTERY_MAX;
        else
            return 0;
    }

    public List<InGameManager.PlayerInfo> listOfPlayerCollided = new List<InGameManager.PlayerInfo>();

    private void OnCollisionEnter(Collision collision)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (PLAYER_OUT_OF_BOUNDARY_USE == true)
        {
            if (collision.collider.CompareTag(CommonDefine.TAG_OutOfBound))
            {
                if (isOutOfBoundary == false)
                {
                    if (IsMine)
                        isOutOfBoundary = true;

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
                    if (pm.PlayerID != PlayerID)
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
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (PLAYER_OUT_OF_BOUNDARY_USE == true)
        {
            if (collision.collider.CompareTag(CommonDefine.TAG_OutOfBound))
            {
                if (isOutOfBoundary == false)
                {
                    if (IsMine)
                        isOutOfBoundary = true;
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
                    if (pm.PlayerID != PlayerID)
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
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (IsMine)
        {
            if (collision.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer))
            {
                var pm = collision.gameObject.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    if (pm.PlayerID != PlayerID)
                    {

                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
            return;

        if (PLAYER_OUT_OF_BOUNDARY_USE == true)
        {
            if (other.CompareTag(CommonDefine.TAG_OutOfBound))
            {
                if (isOutOfBoundary == false)
                {
                    if (IsMine)
                        isOutOfBoundary = true;
                }
            }
        }
    }

    public void OnEvent_SetCurrentLaneNumberTypeAndMoveIndex(LaneType type, int moveIndex)
    {
        if (IsValidDestinationIncludingOutOfBoundary(type, moveIndex) == false)
        {
            Debug.Log("<color=red>InValid Lane...! Cannot move to that Lane!!!</color>");
            return;
        }


        previousLaneType = currentLaneType;

        switch (type)
        {
            case LaneType.Zero:
                {
                    currentLaneType = LaneType.Zero;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.One:
                {
                    currentLaneType = LaneType.One;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Two:
                {
                    currentLaneType = LaneType.Two;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Three:
                {
                    currentLaneType = LaneType.Three;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Four:
                {
                    currentLaneType = LaneType.Four;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Five:
                {
                    currentLaneType = LaneType.Five;
                    currentMoveIndex = moveIndex;
                }
                break;
            case LaneType.Six:
                {
                    currentLaneType = LaneType.Six;
                    currentMoveIndex = moveIndex;
                }
                break;
        }

        networkLaneType = currentLaneType;
        networkMoveIndex = currentMoveIndex;

        if (previousLaneType != currentLaneType
            && previousLaneType != LaneType.None
            && currentLaneType != LaneType.None)
        {
            isChangingLane = true;
        }
    }

    public void OnEvent_ActivateChargePad(int viewID, int id, MapObject_ChargePad.ChargePadLevel currentLv)
    {
        ChargePadBoosterLevel boosterlv = ChargePadBoosterLevel.None;
        switch (currentLv)
        {
            case MapObject_ChargePad.ChargePadLevel.One:
                {
                    boosterlv = ChargePadBoosterLevel.ChargePad_One;
                }
                break;
            case MapObject_ChargePad.ChargePadLevel.Two:
                {
                    boosterlv = ChargePadBoosterLevel.ChargePad_Two;
                }
                break;
            case MapObject_ChargePad.ChargePadLevel.Three:
                {
                    boosterlv = ChargePadBoosterLevel.ChargePad_Three;
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

    //������ Valid�� ������ �̵�
    public void MovePlayerToPosition()
    {
        if (IsMine == false)
            return;

        var validLane = (int)GetClosestValidLane();
        var validMoveIndex = currentMoveIndex;


        if ((LaneType)validLane != LaneType.None)
            networkInGameRPCManager.RPC_SpawnPlayerToPosition(PlayerID, validLane, validMoveIndex);
        else
            Debug.Log("<color=red>Error....! No Valid Place......???? </color>");
    }

    private LaneType GetClosestValidLane()
    {

        float dist = float.MaxValue;
        LaneType validLane = LaneType.None;
        for (int i = 0; i <= (int)LaneType.Six; i++)
        {
            if (IsValidDestination((LaneType)i, currentMoveIndex) && GetWayPointPosition((LaneType)i, currentMoveIndex).HasValue)
            {
                var tempDist = Vector3.Distance(player.transform.position, GetWayPointPosition((LaneType)i, currentMoveIndex).Value);

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
        //currentLaneType ���� ���� Lane 
        //Valid�Ѱ� ������ None ��ȯ

        if (currentLaneType == LaneType.Zero || currentLaneType == LaneType.None)
            return LaneType.None;

        var curr = LaneType.None;

        if (IsValidDestinationIncludingOutOfBoundary((currentLaneType - 1), currentMoveIndex))
        {
            curr = (currentLaneType - 1);
        }

        return curr;
    }

    private LaneType GetRightValidLaneWhenHit()
    {
        //currentLaneType ���� ������ Lane 
        //Valid�Ѱ� ������ None ��ȯ

        if (currentLaneType == LaneType.Six || currentLaneType == LaneType.None)
            return LaneType.None;

        var curr = LaneType.None;

        if (IsValidDestinationIncludingOutOfBoundary((currentLaneType + 1), currentMoveIndex))
        {
            curr = (currentLaneType + 1);
        }

        return curr;
    }


    public void OnEvent_SpawnPlayerToPosition(int lane, int moveIndex)
    {
        rgbdy.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        isOutOfBoundary = false;
        isFlipped = false;
        isFlippingUp = false;
        outOfBoundaryTimer = 0f;
        outOfBoundaryMoveSpeed = PLAYER_DEFAULT_MOVE_SPEED;

        currentLaneType = (LaneType)lane;
        currentMoveIndex = moveIndex;

        if (IsMine)
            Camera_Base.Instance.TurnOffCameraFX();
        playerCar.DeactivateAllBoosterFX();

        //Spawn �Ҷ��� transform.position Ȱ������...! rbdy.position Ȱ��x
        if (GetWayPointPosition(currentLaneType, currentMoveIndex).HasValue)
            transform.position = GetWayPointPosition(currentLaneType, currentMoveIndex).Value;
        rgbdy.rotation = Quaternion.LookRotation(GetCurrentDirection(), Vector3.up);
    }


    public void RaiseRPC_PlayerIsInFrontTrue(int otherViewID)
    {
        if (IsMine == false)
            return;

        RPC_PlayerIsInFrontTrue(otherViewID);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_PlayerIsInFrontTrue(int otherPlayerID)
    {
        if (triggerChecker_Front.listOfCollision.Count > 0)
        {
            IsPlayerInFront = true;
            playerInFrontViewID = otherPlayerID;
        }
    }

    public void RaiseRPC_PlayerIsInFrontFalse()
    {
        if (IsMine == false)
            return;

        RPC_PlayerIsInFrontFalse();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_PlayerIsInFrontFalse()
    {
        if (triggerChecker_Front.listOfCollision.Count == 0)
        {
            IsPlayerInFront = false;
            playerInFrontViewID = 0;
        }
    }


    public void RaiseRPC_PlayerTriggerEnterOtherPlayer(NetworkObject no, PlayerTriggerChecker.CheckParts parts, float moveSpeed, PlayerTriggerChecker trigger)
    {
        if (IsMine == false)
            return;

        if (no == null)
            return;

        if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;

        Vector3 triggerEnterPosition = trigger.transform.position;
        InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_02, triggerEnterPosition);

        if (IsMine)
            Vibration.Vibrate((long)200);

        RPC_TriggerEnterOtherPlayer(PlayerID, (int)parts, moveSpeed);
    }


    //�ǰ� ���� ������ RPC 
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_TriggerEnterOtherPlayer(int otherViewID, int triggerParts, float moveSpeed)
    {
        //otherViewID�� ���� ����(?) ���� ���

        var parts = (PlayerTriggerChecker.CheckParts)triggerParts;

        switch (parts)
        {
            case PlayerTriggerChecker.CheckParts.Left:
                {
                    //������ ���ϴ� ���忡�� ���ʿ��� �浹�� �ߴ��� Ȯ��
                    var other = triggerChecker_Left.listOfCollision.Find(x => x.PlayerID.Equals(otherViewID));
                    if (other != null)
                    {
#if UNITY_EDITOR
                        if (IsMine)
                            Debug.Log(gameObject.name + " is HIT Left");
#endif

                        if (isParrying) //��� ���� ���
                        {
                            //���� ���� �Ծ��ֱ�
                            if (other.isFlipped == false && other.isMoving && other.isOutOfBoundary == false)
                            {
                                other.RaiseRPC_GetFlipped(other.networkObject, other.transform.position, other.transform.rotation);

                                //���� ���¸� ���� �ޱ�
                                this.RaiseRPC_GetBatteryBuff(networkObject, PLAYER_BATTERY_DEFENCE_SUCCESS);

                                //���ÿ� collision �Ͼ�� ���� ���Ÿ� ����...!
                                //�� ������ ��� list���� ���� & collider�� ��� �ٷ� ������...
                                other.collisionChecker_Player.enabled = false;
                                if (triggerChecker_Left.listOfCollision.Contains(other))
                                    triggerChecker_Left.listOfCollision.Remove(other);
                            }
                        }
                        else //���x ���� ���Ѱ��
                        {
                            if (isFlipped == false && isMoving && isOutOfBoundary == false)
                            {
                                //���ʿ��� ������ �������� ���������� �о�����
                                if (GetRightValidLaneWhenHit() != LaneType.None)
                                {
                                    var newLane = GetRightValidLaneWhenHit();
                                    networkInGameRPCManager.RPC_SetLaneAndMoveIndex(PlayerID, (int)newLane, currentMoveIndex);
                                    this.RaiseRPC_GetStuned(networkObject, PLAYER_STUN_TIME);

                                    //���� ���¸� ���� �ޱ�
                                    other.RaiseRPC_GetBatteryBuff(other.networkObject, other.PLAYER_BATTERY_ATTACK_SUCCESS);
                                }
                                else
                                {
                                    //���� ������ �׳� ������ ����
                                    this.RaiseRPC_GetFlipped(networkObject, transform.position, transform.rotation);

                                    //���� ���¸� ���� �ޱ�
                                    other.RaiseRPC_GetBatteryBuff(other.networkObject, other.PLAYER_BATTERY_ATTACK_SUCCESS);
                                }
                            }
                        }
                    }
                }
                break;

            case PlayerTriggerChecker.CheckParts.Right:
                {
                    //������ ���ϴ� ���忡�� �����ʿ��� �浹�� �ߴ��� Ȯ��
                    var other = triggerChecker_Right.listOfCollision.Find(x => x.PlayerID.Equals(otherViewID));
                    if (other != null)
                    {
#if UNITY_EDITOR
                        if (IsMine)
                            Debug.Log(gameObject.name + " is HIT Right");
#endif

                        if (isParrying) //��� ���� ���
                        {
                            //���� ���� �Ծ��ֱ�
                            if (other.isFlipped == false && other.isMoving && other.isOutOfBoundary == false)
                            {
                                other.RaiseRPC_GetFlipped(other.networkObject, other.transform.position, other.transform.rotation);

                                //���� ���¸� ���� �ޱ�
                                this.RaiseRPC_GetBatteryBuff(networkObject, PLAYER_BATTERY_DEFENCE_SUCCESS);

                                //���ÿ� collision �Ͼ�� ���� ���Ÿ� ����...!
                                //�� ������ ��� list���� ���� & collider�� ��� �ٷ� ������...
                                other.collisionChecker_Player.enabled = false;
                                if (triggerChecker_Right.listOfCollision.Contains(other))
                                    triggerChecker_Right.listOfCollision.Remove(other);

                            }
                        }
                        else //���x ���� ���Ѱ��
                        {
                            if (isFlipped == false && isMoving && isOutOfBoundary == false)
                            {
                                //�����ʿ��� ������ �������� �������� �о�����
                                if (GetLeftValidLaneWhenHit() != LaneType.None)
                                {
                                    var newLane = GetLeftValidLaneWhenHit();
                                    networkInGameRPCManager.RPC_SetLaneAndMoveIndex(PlayerID, (int)newLane, currentMoveIndex);
                                    this.RaiseRPC_GetStuned(networkObject, PLAYER_STUN_TIME);

                                    //���� ���¸� ���� �ޱ�
                                    other.RaiseRPC_GetBatteryBuff(other.networkObject, other.PLAYER_BATTERY_ATTACK_SUCCESS);
                                }
                                else
                                {
                                    //���� ������ �׳� ������ ����
                                    this.RaiseRPC_GetFlipped(networkObject, transform.position, transform.rotation);

                                    //���� ���¸� ���� �ޱ�
                                    other.RaiseRPC_GetBatteryBuff(other.networkObject, other.PLAYER_BATTERY_ATTACK_SUCCESS);
                                }
                            }
                        }
                    }
                }
                break;

            case PlayerTriggerChecker.CheckParts.Back:
            case PlayerTriggerChecker.CheckParts.Body:
                {
                    //������ ���ϴ� ���忡�� Back/Body���� �浹�� �ߴ��� Ȯ��
                    PlayerMovement other = null;
                    if (parts == PlayerTriggerChecker.CheckParts.Back)
                    {
                        other = triggerChecker_Back.listOfCollision.Find(x => x.PlayerID.Equals(otherViewID));
                        if (other == null) //������ body�� üũ�غ���...!
                            other = triggerChecker_Body.listOfCollision.Find(x => x.PlayerID.Equals(otherViewID));
                    }
                    if (parts == PlayerTriggerChecker.CheckParts.Body)
                        other = triggerChecker_Body.listOfCollision.Find(x => x.PlayerID.Equals(otherViewID));

                    if (other != null)
                    {
#if UNITY_EDITOR
                        if (IsMine)
                        { 
                            if(parts == PlayerTriggerChecker.CheckParts.Body)
                                Debug.Log(gameObject.name + " is HIT body");
                            else if (parts == PlayerTriggerChecker.CheckParts.Back)
                                Debug.Log(gameObject.name + " is HIT back");
                        }
#endif

                        if (isParrying)//��� ���� ���
                        {
                            //���� ���� �Ծ��ֱ�
                            if (other.isFlipped == false && other.isMoving && other.isOutOfBoundary == false)
                            {
                                other.RaiseRPC_GetFlipped(other.networkObject, other.transform.position, other.transform.rotation);

                                //���� ���¸� ���� �ޱ�
                                this.RaiseRPC_GetBatteryBuff(networkObject, PLAYER_BATTERY_DEFENCE_SUCCESS);


                                //���ÿ� collision �Ͼ�� ���� ���Ÿ� ����...!
                                //�� ������ ��� list���� ���� & collider�� ��� �ٷ� ������...
                                other.collisionChecker_Player.enabled = false;
                                if (triggerChecker_Back.listOfCollision.Contains(other))
                                    triggerChecker_Back.listOfCollision.Remove(other);
                                if (triggerChecker_Body.listOfCollision.Contains(other))
                                    triggerChecker_Body.listOfCollision.Remove(other);
                            }
                        }
                        else
                        {
                            //���� Flip �Ծ������...
                            if (isFlipped == false && isMoving && isOutOfBoundary == false)
                            {
                                this.RaiseRPC_GetFlipped(networkObject, transform.position, transform.rotation);

                                //���� ���¸� ���� �ޱ�
                                other.RaiseRPC_GetBatteryBuff(other.networkObject, other.PLAYER_BATTERY_ATTACK_SUCCESS);
                            }
                        }
                    }
                }
                break;
        }

    }

    public void RaiseRPC_GetStuned(NetworkObject no, float stunTime, bool useSpinAnim = true)
    {
        RPC_GetStuned(stunTime, useSpinAnim);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_GetStuned(float stunTime, bool useSpinAnim = true)
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
            timer = PLAYER_STUN_TIME;
        else
            timer = stunTime;

        isStunned = true;
        //additionalStunSpeed = -4f;

        if (useSpinAnim)
            SetAnimation_Both(AnimationState.Spin, true);

        if (IsMine)
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
        additionalStunSpeed = 0f;

        SetAnimation_Both(AnimationState.Drive, true);

        playerCar.DeactivateStunFX();
    }

    public void RaiseRPC_GetBatteryBuff(NetworkObject no, int battery)
    {
        if (no == null)
            return;

        RPC_GetBatteryBuff(battery);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_GetBatteryBuff(int battery)
    {
        networkInGameRPCManager.RPC_SetPlayerBattery(PlayerID, true, battery);
    }

    public void RaiseRPC_GetFlipped(NetworkObject no, Vector3 pos, Quaternion rot)
    {
        if (no == null)
            return;

        RPC_GetFlipped(pos, rot);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_GetFlipped(Vector3 pos, Quaternion rot)
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

        //�䰪 ���� ���ִ°� ����غ���.... ����� �� �� ������
        if (PhotonNetworkManager.Instance.IsPingAtGoodCondition())
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        float timer = PLAYER_FLIP_UP_TIME;
        float timer_2 = PLAYER_FLIP_GROUND_DELAY_TIME;

        isFlipped = true;
        isFlippingUp = true;
        rgbdy.useGravity = false;

        collisionChecker_Player.enabled = false;

        SetAnimation_Both(AnimationState.Flip, true);

        if (IsMine)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Hit);
            Vibration.Vibrate((long)500);
        }

        while (true)
        {
            timer -= Runner.DeltaTime;

            if (timer > PLAYER_FLIP_UP_TIME / 2)
                isFlippingUp = true;
            else
                isFlippingUp = false;

            yield return new WaitForFixedUpdate();

            if (timer < 0)
                break;


            //network player�� ���.... falseó���� �ɼ� ����
            if (IsMine == false)
            {
                if (isFlipped == false && isFlippingUp == false)
                    break;
            }
            //if (isFlipped == false && isFlippingUp == false)
            //if (isFlipped == false)
            //    break;

            //�ٴڿ� �����ϸ�... ���� ���� �������
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
            var checkIndex = (int)System.Math.Truncate(Mathf.Lerp(1, 3, (Mathf.Clamp((currentMoveSpeed - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED) / Mathf.Clamp((PLAYER_MAX_MOVE_SPEED - PLAYER_DEFAULT_MOVE_SPEED), 0f, PLAYER_MAX_MOVE_SPEED))));

            //���� �ε����� ����...
            int newMoveIndex = GetNewMoveIndexAtRange(currentMoveIndex, newLane, transform.position, PLAYER_MOVE_INDEX_CHECK_MIN_RANGE, checkIndex);
            if (newMoveIndex != currentMoveIndex || IsLastIndex(newLane, newMoveIndex) == true)
                currentMoveIndex = newMoveIndex;

            networkInGameRPCManager.RPC_SetLaneAndMoveIndex(PlayerID, (int)newLane, currentMoveIndex);

            //�ν��� ����� ��� ���� ���� ���� None �����ϰܿ� 1�ܰ� �ν��� ���� ����
            if (currentCarBoosterLv == CarBoosterLevel.None)
                networkInGameRPCManager.RPC_BoostPlayer((int)CarBoosterLevel.Car_One);
        }


        SetAnimation_Both(AnimationState.Drive, true);

        playerCar.DeactivateStunFX();
        playerCar.DeactivateMudFX();
    }

    private bool IsValidFlip()
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return false;

       if (isEnteredTheFinishLine == true)
            return false;

        if (isOutOfBoundary == true)
            return false;

        if (isFlipped == true)
            return false;

        return true;
    }

    public void SetIsGrounded(bool isGrounded)
    {
        this.isGrounded = isGrounded;
    }


    /*
#region IPunObservable Callbacks
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize)
            return;

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
            return;

        base.OnPhotonSerializeView(stream, info);


        if (stream.IsWriting) //��ǻ� isMine...
        {
            stream.SendNext(this.rgbdy.position);
            stream.SendNext(currDirection);
            stream.SendNext(this.rgbdy.rotation);
            //stream.SendNext(this.rgbdy.velocity);
            stream.SendNext(currentMoveIndex);
            stream.SendNext(currentMoveSpeed);
            stream.SendNext(currentRotationSpeed);
            stream.SendNext(currentBattery);
            stream.SendNext(currentLap);
            stream.SendNext((int)currentLaneType);
            stream.SendNext(isEnteredTheFinishLine);
            stream.SendNext(isDecelerating);
            stream.SendNext(isParrying);
            stream.SendNext(isStunned);
            stream.SendNext(isFlipped);
            stream.SendNext(isOutOfBoundary);
        }
        else
        {
            networkPos = (Vector3)stream.ReceiveNext();
            networkDir = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            //rgbdy.velocity = (Vector3)stream.ReceiveNext();
            networkMoveIndex = (int)stream.ReceiveNext();
            networkMoveSpeed = (float)stream.ReceiveNext();
            networkRotationSpeed = (float)stream.ReceiveNext();
            networkBattery = (int)stream.ReceiveNext();
            networkLap = (int)stream.ReceiveNext();
            networkLaneType = (LaneType)(int)stream.ReceiveNext();
            isEnteredTheFinishLine = (bool)stream.ReceiveNext();
            isDecelerating = (bool)stream.ReceiveNext();
            isParrying = (bool)stream.ReceiveNext();
            isStunned = (bool)stream.ReceiveNext();
            isFlipped = (bool)stream.ReceiveNext();
            isOutOfBoundary = (bool)stream.ReceiveNext();

            lagTime = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

        }
    }
#endregion
    */

    private void OnFirebaseBasicValueChangedCallback()
    {
        SetBasicData();
    }

    private void OnFirebaseCarValueChangedCallback()
    {
        if (carID != -1)
        {
            SetCarData(carID);
        }
    }












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
                    Gizmos.DrawWireSphere(transform.position, PLAYER_MOVE_INDEX_CHECK_MIN_RANGE);
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
}
