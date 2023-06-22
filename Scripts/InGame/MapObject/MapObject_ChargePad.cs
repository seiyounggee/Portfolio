using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_ChargePad : MonoBehaviour
{
    [SerializeField] TriggerChecker triggerChecker = null;
    [SerializeField] Animator anim = null;

    public enum ChargePadLevel { One, Two, Three}
    [ReadOnly] public ChargePadLevel currentLevel = ChargePadLevel.One;

    [ReadOnly] public int id = -1;

    //TEMP?
    [SerializeField] MeshRenderer mesh = null;
    [SerializeField] Material m1, m2, m3;

    [ReadOnly] public bool isOn = true;

    #region Charge Pad Data
    [ReadOnly] public float CHARGEPAD_LV2_CHARGE_RESET_TIME = 5f;
    [ReadOnly] public float CHARGEPAD_LV3_CHARGE_RESET_TIME = 3f;

    [ReadOnly] public float CHARGEPAD_LV1_ANIM_SPEED = 1f;
    [ReadOnly] public float CHARGEPAD_LV2_ANIM_SPEED = 1.2f;
    [ReadOnly] public float CHARGEPAD_LV3_ANIM_SPEED = 1.5f;
    #endregion

    public const string ANIM_SPEED = "ANIM_SPEED";

    [ReadOnly] public int nearestWayPointIndex = -1;
    [ReadOnly] public PlayerMovement.LaneType nearestWayPointLaneType = PlayerMovement.LaneType.None;

    public NetworkInGameRPCManager networkInGameRPCManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

    private void Awake()
    {
        if (triggerChecker != null)
        {
            triggerChecker._OnTriggerEnter = Event_OnTriggerEnter;
            triggerChecker._OnTriggerStay = Event_OnTriggerStay;
            triggerChecker._OnTriggerExit = Event_OnTriggerExit;
        }
    }

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }

    public void SetData(int id)
    {
        this.id = id;
        currentLevel = ChargePadLevel.One;
        SetMat();

        CHARGEPAD_LV2_CHARGE_RESET_TIME = DataManager.Instance.GetGameConfig<float>("chargePadLv2ResetTime");
        CHARGEPAD_LV3_CHARGE_RESET_TIME = DataManager.Instance.GetGameConfig<float>("chargePadLv3ResetTime");
        CHARGEPAD_LV1_ANIM_SPEED = 1;
        CHARGEPAD_LV2_ANIM_SPEED = 2;
        CHARGEPAD_LV3_ANIM_SPEED = 3;

        SetAnim();
        SetNearestWaypointIndexAndLane();
    }

    [ReadOnly] public float timer = 0f;
    private void FixedUpdate()
    {

        //이 방법은 완벽한 동기화는 적용되지 않을듯...?
        switch (currentLevel)
        {
            case ChargePadLevel.Two:
                {
                    timer += Time.fixedDeltaTime;
                    if (timer > CHARGEPAD_LV2_CHARGE_RESET_TIME)
                    {
                        currentLevel = ChargePadLevel.One;
                        timer = 0f;
                        SetMat();
                        SetAnim();
                    }
                }
                break;
            case ChargePadLevel.Three:
                {
                    timer += Time.fixedDeltaTime;
                    if (timer > CHARGEPAD_LV3_CHARGE_RESET_TIME)
                    {
                        currentLevel = ChargePadLevel.Two;
                        timer = 0f;
                        SetMat();
                        SetAnim();
                    }
                }
                break;
        }
    }

    public void ActivateChargePad(ChargePadLevel lv)
    {
        if (id == -1)
            return;

        if (lv == ChargePadLevel.One)
        {
            timer = 0f;                    
            currentLevel = ChargePadLevel.Two;
        }

        if (lv == ChargePadLevel.Two)
        {
            timer = 0f;
            currentLevel = ChargePadLevel.Three;
        }

        if (lv == ChargePadLevel.Three)
        {
            timer = 0f;
            //또 밟혔으면... 초기화
        }

        SetMat();
        SetAnim();
    }

    public void ResetChargePad(ChargePadLevel lv)
    {
        if (id == -1)
            return;

        if (lv == ChargePadLevel.Two)
            currentLevel = ChargePadLevel.One;

        if (lv == ChargePadLevel.Three)
            currentLevel = ChargePadLevel.Two;

        SetMat();
        SetAnim();
    }

    public void SetMat()
    {
        if (mesh == null || m1 == null || m2 == null || m3 == null)
            return;

        switch (currentLevel)
        {
            case ChargePadLevel.One:
                mesh.material = m1;
                break;
            case ChargePadLevel.Two:
                mesh.material = m2;
                break;
            case ChargePadLevel.Three:
                mesh.material = m3;
                break;
        }
    }

    private void SetAnim()
    {
        if (anim == null)
            return;

        switch (currentLevel)
        {
            case ChargePadLevel.One:
                anim.SetFloat(ANIM_SPEED, CHARGEPAD_LV1_ANIM_SPEED);
                break;
            case ChargePadLevel.Two:
                anim.SetFloat(ANIM_SPEED, CHARGEPAD_LV2_ANIM_SPEED);
                break;
            case ChargePadLevel.Three:
                anim.SetFloat(ANIM_SPEED, CHARGEPAD_LV3_ANIM_SPEED);
                break;
        }
    }

    private void SetNearestWaypointIndexAndLane()
    {
        if (InGameManager.Instance.wayPoints == null)
        {
            Debug.Log("InGameManager.Instance.wayPoints is null");
            return;
        }

        if (InGameManager.Instance.wayPoints.allWaypoints == null)
        {
            Debug.Log("allWaypoints is null");
            return;
        }

        var wp = InGameManager.Instance.wayPoints.allWaypoints;
        if (wp != null)
        {
            var minDist = float.MaxValue;

            foreach (var list in wp)
            {
                foreach (var way in list)
                {
                    float dist = Vector3.Distance(way.GetPosition(), this.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestWayPointIndex = way.index;
                        nearestWayPointLaneType = (PlayerMovement.LaneType)way.laneNumber;
                    }
                }
            }
        }
    }

    private void Event_OnTriggerEnter(Collider other)
    {
        if (IsBlockEvent())
            return;

        if (other.CompareTag(CommonDefine.TAG_NetworkPlayer_TriggerChecker))
        {
            var cc = other.GetComponent<PlayerTriggerChecker>();
            if (cc != null && cc.currentCheckParts == PlayerTriggerChecker.CheckParts.Body)
            {
                var pm = cc.pm;
                if (pm == null)
                    return;

                if (pm.isFlipped || pm.isOutOfBoundary)
                    return;

                if (pm.IsMine)
                    networkInGameRPCManager.RPC_SetChargePad(id, (int)currentLevel, false, pm.networkPlayerID);
            }
        }
    }

    private void Event_OnTriggerStay(Collider other)
    {
        if (IsBlockEvent())
            return;


    }

    private void Event_OnTriggerExit(Collider other)
    {
        if (IsBlockEvent())
            return;


    }

    private bool IsBlockEvent()
    {
        if (id == -1)
            return true;

        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame
            || PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGameResult)
        {
            if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
                || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
                || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown)
                return true;
            else
                return false;
        }
        else
            return true;

    }

    private void FirebaseBasicValueChanged_SubscriptionEvent()
    {
    }
}
