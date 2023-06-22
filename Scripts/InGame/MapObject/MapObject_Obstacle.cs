using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_Obstacle : MonoBehaviour
{
    [ReadOnly] public int id = 0;
    [SerializeField] public float coolTime = 0f;
    [SerializeField] public float startDelay = 0f;

    [SerializeField] Collider coll = null;
    [SerializeField] TriggerChecker triggerChecker = null;

    private NetworkInGameRPCManager rpcManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

    [System.Serializable]
    private class HitDelayInfo
    {
        public PlayerMovement pm;
        public float counter = 0;
        public bool isHitAvailable { get { if (counter <= 0) return true; else return false; } }
    }

    [HideInInspector] private List<HitDelayInfo> hitDelayList = new List<HitDelayInfo>();


    public enum ObstacleType { None, Flip, Stun }
    public ObstacleType obstacleType = ObstacleType.Flip;

    public Vector3 ObstaclePosition
    {
        get
        {
            if (triggerChecker != null)
                return triggerChecker.transform.position;
            else
                return Vector3.zero;
        }
    }

    private void Awake()
    {
        if (coll == null)
            coll = GetComponent<Collider>();

        coolTime = 3f;

        if (triggerChecker != null)
        {
            triggerChecker._OnTriggerEnter = Event_OnTriggerEnter;
            triggerChecker._OnTriggerStay = Event_OnTriggerStay;
            triggerChecker._OnTriggerExit = Event_OnTriggerExit;
        }
    }

    private void FixedUpdate()
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
        || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
        || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
        {
            hitDelayList.Clear();
            return;
        }

        if (hitDelayList != null && hitDelayList.Count > 0)
        {
            foreach (var i in hitDelayList)
            {
                if (i.isHitAvailable == false)
                    i.counter -= Time.fixedDeltaTime;
            }
        }
    }

    public void SetData(int index)
    {
        id = index;
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

                var p = hitDelayList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isHitAvailable == false)
                    return;

                float coolTime = 3f;

                if (pm.IsMine)
                {
                    switch (obstacleType)
                    {
                        case ObstacleType.None:
                            break;
                        case ObstacleType.Flip:
                            {
                                if (pm.isShield)
                                    rpcManager.RPC_StopShield(pm.networkPlayerID);

                                rpcManager.RPC_GetFlipped(pm.networkPlayerID, pm.transform.position, pm.transform.rotation);
                            }
                            break;
                        case ObstacleType.Stun:
                            {
                                if (pm.isShield)
                                    rpcManager.RPC_StopShield(pm.networkPlayerID);

                                rpcManager.RPC_GetStuned(pm.networkPlayerID, pm.ref_stunTime);
                            }
                            break;
                    }
                }

                InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_02, pm.transform.position);

                //남의 hit 정보는 정확하지 않을수 있음...
                if (p == null)
                    hitDelayList.Add(new HitDelayInfo() { pm = pm, counter = coolTime });
                else
                    p.counter = coolTime;
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
}
