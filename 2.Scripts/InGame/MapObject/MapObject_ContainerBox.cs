using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_ContainerBox : MonoBehaviour
{
    [ReadOnly] public int id = 0;
    [SerializeField] public float coolTime = 0f;
    [SerializeField] public float startDelay = 0f;

    [SerializeField] Collider coll = null;
    [SerializeField] Animation anim = null;
    [SerializeField] TriggerChecker triggerChecker = null;



    public enum MovingType { None, LeftAndRight, UpDown }
    public MovingType movingType = MovingType.None;

    [System.Serializable]
    public class HitDelayInfo
    {
        public PlayerMovement pm;
        public float counter = 0;
        public bool isHitAvailable { get { if (counter <= 0) return true; else return false; } }
    }

    [ReadOnly] public List<HitDelayInfo> hitDelayList = new List<HitDelayInfo>();


    private void Awake()
    {
        if (coll == null)
            coll = GetComponent<Collider>();

        if (anim == null)
        {
            anim = GetComponent<Animation>();
            anim.playAutomatically = false;
        }

        if (coolTime <= 0 && anim != null && anim.clip != null)
        {
            coolTime = anim.clip.length;
        }

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
        || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
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

    public void ActivateContainerBox()
    {
        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
            || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;

        switch (movingType)
        {
            case MovingType.None:
                break;
            case MovingType.LeftAndRight:
                {
                    anim.SafePlay(anim.clip.name);
                }
                break;
            case MovingType.UpDown:
                {
                    anim.SafePlay(anim.clip.name);
                }
                break;
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

                var p = hitDelayList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isHitAvailable == false)
                    return;

                float coolTime = 3f;

                switch (movingType)
                {
                    case MovingType.None:
                        break;

                    case MovingType.LeftAndRight:
                        {
                            if (pm.IsMine)
                                pm.RaiseRPC_GetFlipped(pm.networkObject, pm.transform.position, pm.transform.rotation);

                            InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_02, pm.transform.position);

                            //남의 hit 정보는 정확하지 않을수 있음...
                            if (p == null)
                                hitDelayList.Add(new HitDelayInfo() { pm = pm, counter = coolTime });
                            else
                                p.counter = coolTime;
                        }
                        break;
                    case MovingType.UpDown:
                        {
                            if (pm.IsMine)
                                pm.RaiseRPC_GetFlipped(pm.networkObject, pm.transform.position, pm.transform.rotation);

                            InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_02, pm.transform.position);

                            //남의 hit 정보는 정확하지 않을수 있음...
                            if (p == null)
                                hitDelayList.Add(new HitDelayInfo() { pm = pm, counter = coolTime });
                            else
                                p.counter = coolTime;
                        }
                        break;
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
    }

    private bool IsBlockEvent()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return true;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
        || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
        || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return true;
        else
            return false;
    }
}
