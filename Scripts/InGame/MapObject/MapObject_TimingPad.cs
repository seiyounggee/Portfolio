using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_TimingPad : MonoBehaviour
{
    [SerializeField] TriggerChecker triggerChecker = null;


    [ReadOnly] public int id = -1;

    [ReadOnly] public int nearestWayPointIndex = -1;
    [ReadOnly] public PlayerMovement.LaneType nearestWayPointLaneType = PlayerMovement.LaneType.None;

    public NetworkInGameRPCManager networkInGameRPCManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

    [System.Serializable]
    private class CoolTimeInfo
    {
        public PlayerMovement pm;
        public float coolTimer = 0;
        public bool isAvailable { get { if (coolTimer <= 0) return true; else return false; } }
    }

    [HideInInspector] private List<CoolTimeInfo> coolTimeList = new List<CoolTimeInfo>();

    private void Awake()
    {
        if (triggerChecker != null)
        {
            triggerChecker._OnTriggerEnter = Event_OnTriggerEnter;
            triggerChecker._OnTriggerStay = Event_OnTriggerStay;
            triggerChecker._OnTriggerExit = Event_OnTriggerExit;
        }

        coolTimeList.Clear();
    }

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }

    private void FixedUpdate()
    {
        if (IsBlockEvent() == false)
        {
            if (coolTimeList != null && coolTimeList.Count > 0)
            {
                foreach (var i in coolTimeList)
                {
                    if (i.isAvailable == false)
                        i.coolTimer -= Time.fixedDeltaTime;
                }
            }
        }
    }

    public void SetData(int id)
    {
        this.id = id;

        SetNearestWaypointIndexAndLane();
    }

    public void SetData()
    {

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

                if (pm.isStunned || pm.isFlipped || pm.isOutOfBoundary)
                    return;

                if (pm.network_isEnteredTheFinishLine)
                    return;

                var p = coolTimeList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isAvailable == false)
                    return;

                if (pm.IsMine)
                {
                    float coolTime = 1f;

                    if (pm.IsMineAndNotAI)
                    {
                        pm.ActivateTimingBooster();
                    }
                    else if(pm.IsMineAndAI) //AIÀÇ °æ¿ì...!
                    {
                        pm.SetTimingBoosterAvailable_AI();
                    }

                    if (p == null)
                        coolTimeList.Add(new CoolTimeInfo() { pm = pm, coolTimer = coolTime });
                    else
                        p.coolTimer = coolTime;
                }
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

        if (other.CompareTag(CommonDefine.TAG_NetworkPlayer_TriggerChecker))
        {
            var cc = other.GetComponent<PlayerTriggerChecker>();
            if (cc != null && cc.currentCheckParts == PlayerTriggerChecker.CheckParts.Body)
            {
                var pm = cc.pm;
                if (pm == null)
                    return;

                if (pm.isStunned || pm.isFlipped || pm.isOutOfBoundary)
                    return;

                if (pm.network_isEnteredTheFinishLine)
                    return;

                if (pm.IsMine)
                {
                    var p = coolTimeList.Find(x => x.pm.Equals(pm));
                    if (p != null && p.isAvailable == false)
                    {
                        p.coolTimer = 0f; //make available
                    }
                }
            }
        }
    }

    private bool IsBlockEvent()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return true;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
        || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
        || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return true;
        else
            return false;
    }

    private void FirebaseBasicValueChanged_SubscriptionEvent()
    {
        SetData();
    }
}
