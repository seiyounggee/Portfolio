using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerTriggerChecker : MonoBehaviour
{
    [SerializeField] public PlayerMovement pm = null;
    [ReadOnly] public Collider coll = null;
    public enum CheckParts { None , Body, Front, Back, Left, Right}
    public CheckParts currentCheckParts = CheckParts.None; //인스펙터에서 지정!!!!

    [ReadOnly] public List<PlayerMovement> listOfCollision = new List<PlayerMovement>();

    [Serializable]
    public class CollisionDelayInfo
    {
        public int viewID;
        public float delayCooltime = 0.5f;
        public bool isCollisionAvailable { get { if (delayCooltime <= 0f) return true; else return false; } }
    }

    private float PLAYER_COLLISION_DELAY_COOLTIME = 0.1f;

    [ReadOnly] public List<CollisionDelayInfo> listOfCollisionDelayInfo = new List<CollisionDelayInfo>();

    public bool isCollisionUsed = true; //Collision check 사용하는지
    private bool isCollisionActivated = false; //Collision 사용했는지 여부

    public void Initialize()
    {
        listOfCollision.Clear();
        listOfCollisionDelayInfo.Clear();

        if (pm == null)
            pm = GetComponentInParent<PlayerMovement>();

        if (coll == null)
            coll = GetComponent<Collider>();

        if (currentCheckParts == CheckParts.Front)
            isCollisionUsed = false;
        else
            isCollisionUsed = true;

        if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.playerData != null)
            PLAYER_COLLISION_DELAY_COOLTIME = DataManager.Instance.basicData.playerData.PLAYER_COLLISION_DELAY_COOLTIME;
    }

    private void Awake()
    {
        if (pm == null)
            pm = GetComponentInParent<PlayerMovement>();

        if (coll == null)
            coll = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (listOfCollisionDelayInfo != null && listOfCollisionDelayInfo.Count > 0)
        {
            foreach (var i in listOfCollisionDelayInfo)
            {
                if (i.delayCooltime > 0f)
                {
                    i.delayCooltime -= Time.fixedDeltaTime;
                }
                else
                    i.delayCooltime = 0f;
            }
        }
    }



    void OnTriggerEnter(Collider other)
    {
        if (pm == null)
            return;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
            || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;

        #region TAG_NetworkPlayer_CollisionChecker
        if (other.CompareTag(CommonDefine.TAG_NetworkPlayer_TriggerChecker))
        {
            var cc = other.GetComponent<PlayerTriggerChecker>();
            if (cc != null && cc.pm != null)
            {
                if (cc.pm.PlayerID != pm.PlayerID)
                {
                    var o = listOfCollision.Find(x => x.PlayerID.Equals(cc.pm.PlayerID));
                    if (o == null)
                        listOfCollision.Add(cc.pm);
                }
            }

            if (currentCheckParts == CheckParts.Front)
            {
                var otherCC = other.GetComponent<PlayerTriggerChecker>();
                if (otherCC != null && otherCC.pm != null && otherCC.pm.PlayerID != pm.PlayerID)
                {
                    if (isCollisionUsed == true)
                    {
                        //내가 수비하고 있지 않거나 2,3단계 부스팅 상태에서만 공격!!
                        if (pm.isDecelerating == false && pm.isCarAttackBoosting == true)
                        {
                            HitOtherPlayer(otherCC);
                            isCollisionActivated = true;
                        }
                        else
                        {
                            isCollisionActivated = false;
                        }
                    }

                    switch (otherCC.currentCheckParts)
                    {
                        case CheckParts.Left:
                        case CheckParts.Right:
                        case CheckParts.Back:
                        case CheckParts.Body:
                            {
                                if (pm.IsPlayerInFront == false)
                                    pm.RaiseRPC_PlayerIsInFrontTrue(otherCC.pm.PlayerID);
                            }
                            break;
                    }
                }
            }
        }
        #endregion


    }

    void OnTriggerStay(Collider other)
    {
        if (pm == null)
            return;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
            || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;

        #region TAG_NetworkPlayer_CollisionChecker
        if (other.CompareTag(CommonDefine.TAG_NetworkPlayer_TriggerChecker))
        {
            var cc = other.GetComponent<PlayerTriggerChecker>();
            if (cc != null && cc.pm != null)
            {
                if (cc.pm.PlayerID != pm.PlayerID)
                {
                    var o = listOfCollision.Find(x => x.PlayerID.Equals(cc.pm.PlayerID));
                    if (o == null)
                        listOfCollision.Add(cc.pm);
                }

                if (currentCheckParts == CheckParts.Front)
                {
                    var otherCC = other.GetComponent<PlayerTriggerChecker>();

                    //OnTriggerEnter시 발동 안되는 문제 때문에 Stay에서도 체크하자...!
                    if (isCollisionActivated == false)
                    {
                        if (isCollisionUsed == true)
                        {
                            if (pm != null && pm.isDecelerating == false && pm.isCarAttackBoosting == true)
                            {
                                HitOtherPlayer(otherCC);
                                isCollisionActivated = true;
                            }
                        }
                    }
                }

            }
        }
        #endregion
    }

    private void OnTriggerExit(Collider other)
    {
        if (pm == null)
            return;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
            || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return;

        #region TAG_NetworkPlayer_CollisionChecker
        if (other.CompareTag(CommonDefine.TAG_NetworkPlayer_TriggerChecker))
        {
            var cc = other.GetComponent<PlayerTriggerChecker>();
            if (cc != null && cc.pm != null)
            {
                var o = listOfCollision.Find(x => x.PlayerID.Equals(cc.pm.PlayerID));
                if (o != null)
                    listOfCollision.Remove(o);


                if (currentCheckParts == CheckParts.Front)
                {
                    var otherCC = other.GetComponent<PlayerTriggerChecker>();
                    if (otherCC != null && otherCC.pm != null)
                    {
                        if (pm.IsPlayerInFront == true && pm.IsMine)
                            pm.RaiseRPC_PlayerIsInFrontFalse();
                    }

                    isCollisionActivated = false;
                }
            }
        }
        #endregion
    }

    private void HitOtherPlayer(PlayerTriggerChecker otherCC)
    {
        switch (otherCC.currentCheckParts)
        {
            case CheckParts.Left:
                {
                    if (pm.IsMine)
                    {
                        var otherNO = otherCC.pm.networkObject;
                        var c = listOfCollisionDelayInfo.Find(x => x.viewID.Equals(otherCC.pm.PlayerID));
                        if (c == null)
                        {
                            pm.RaiseRPC_PlayerTriggerEnterOtherPlayer(otherNO, otherCC.currentCheckParts, otherCC.pm.currentMoveSpeed, this);
                            listOfCollisionDelayInfo.Add(new CollisionDelayInfo() { viewID = otherCC.pm.PlayerID, delayCooltime = PLAYER_COLLISION_DELAY_COOLTIME });
                        }
                        else
                        {
                            if (c.isCollisionAvailable)
                            {
                                c.delayCooltime = PLAYER_COLLISION_DELAY_COOLTIME;
                                pm.RaiseRPC_PlayerTriggerEnterOtherPlayer(otherNO, otherCC.currentCheckParts, otherCC.pm.currentMoveSpeed, this);
                            }
                            else
                            {
                                //쿨타임 중 불가...!
                            }
                        }

                    }
                }
                break;
            case CheckParts.Right:
                {
                    if (pm.IsMine)
                    {
                        var otherNO = otherCC.pm.networkObject;
                        var c = listOfCollisionDelayInfo.Find(x => x.viewID.Equals(otherCC.pm.PlayerID));
                        if (c == null)
                        {
                            pm.RaiseRPC_PlayerTriggerEnterOtherPlayer(otherNO, otherCC.currentCheckParts, otherCC.pm.currentMoveSpeed, this);
                            listOfCollisionDelayInfo.Add(new CollisionDelayInfo() { viewID = otherCC.pm.PlayerID, delayCooltime = PLAYER_COLLISION_DELAY_COOLTIME });
                        }
                        else
                        {
                            if (c.isCollisionAvailable)
                            {
                                c.delayCooltime = PLAYER_COLLISION_DELAY_COOLTIME;
                                pm.RaiseRPC_PlayerTriggerEnterOtherPlayer(otherNO, otherCC.currentCheckParts, otherCC.pm.currentMoveSpeed, this);
                            }
                            else
                            {
                                //쿨타임 중 불가...!
                            }
                        }
                    }
                }
                break;

            case CheckParts.Back:
            case CheckParts.Body:
                {
                    if (pm.IsMine)
                    {
                        var otherNO = otherCC.pm.networkObject;
                        var c = listOfCollisionDelayInfo.Find(x => x.viewID.Equals(otherCC.pm.PlayerID));
                        if (c == null)
                        {
                            pm.RaiseRPC_PlayerTriggerEnterOtherPlayer(otherNO, otherCC.currentCheckParts, otherCC.pm.currentMoveSpeed, this);
                            listOfCollisionDelayInfo.Add(new CollisionDelayInfo() { viewID = otherCC.pm.PlayerID, delayCooltime = PLAYER_COLLISION_DELAY_COOLTIME });
                        }
                        else
                        {
                            if (c.isCollisionAvailable)
                            {
                                c.delayCooltime = PLAYER_COLLISION_DELAY_COOLTIME;
                                pm.RaiseRPC_PlayerTriggerEnterOtherPlayer(otherNO, otherCC.currentCheckParts, otherCC.pm.currentMoveSpeed, this);
                            }
                            else
                            {
                                //쿨타임 중 불가...!
                            }
                        }
                    }
                }
                break;
        }
    }

}
