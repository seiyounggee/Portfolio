using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_ChargeZone : MonoBehaviour
{
    [ReadOnly] public int id = -1;

    [SerializeField] List<TriggerChecker> triggerChecker = new List<TriggerChecker>();

    public NetworkInGameRPCManager networkInGameRPCManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

    [System.Serializable]
    public class ChargeInfo
    {
        public PlayerMovement pm;
        public float counter = 0;
        public bool isChargeAvailable { get { if (counter <= 0) return true; else return false; } }
    }

    [ReadOnly] public List<ChargeInfo> chargeList = new List<ChargeInfo>();

    [ReadOnly] public float CHARGEZONE_CHARGE_COOLTIME = 0.2f;
    [ReadOnly] public int CHARGEZONE_CHARGE_AMOUNT = 3;

    [ReadOnly] public bool isActive = true;

    private void Awake()
    {
        if (triggerChecker != null)
        {
            foreach (var i in triggerChecker)
            {
                i._OnTriggerEnter = Event_OnTriggerEnter;
                i._OnTriggerStay = Event_OnTriggerStay;
                i._OnTriggerExit = Event_OnTriggerExit;
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsBlockEvent())
        {
            chargeList.Clear();
            return;
        }

        if (chargeList != null && chargeList.Count > 0)
        {
            foreach (var i in chargeList)
            {
                if (i.isChargeAvailable == false)
                    i.counter -= Time.fixedDeltaTime;
            }
        }
    }

    public void SetData(int id)
    {
        this.id = id;

        CHARGEZONE_CHARGE_COOLTIME = DataManager.Instance.GetGameConfig<float>("chargeZoneChargeCooltime");
        CHARGEZONE_CHARGE_AMOUNT = DataManager.Instance.GetGameConfig<int>("chargeZoneChargeAmount");
    }

    public void ActivateChargeZone(bool isActive)
    {
        this.isActive = isActive;
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

                var p = chargeList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isChargeAvailable == false)
                    return;

                float coolTime = CHARGEZONE_CHARGE_COOLTIME;

                if (pm.IsMine)
                {
                    networkInGameRPCManager.RPC_SetPlayerBattery(pm.networkPlayerID, true, CHARGEZONE_CHARGE_AMOUNT, (int)NetworkInGameRPCManager.SetBatteryType.ChargeZone);
                }

                if (pm.playerCar != null)
                    pm.playerCar.ActivateChargeZoneFX();

                if (p == null)
                    chargeList.Add(new ChargeInfo() { pm = pm, counter = coolTime });
                else
                    p.counter = coolTime;
            }
        }
    }

    private void Event_OnTriggerStay(Collider other)
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


                var p = chargeList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isChargeAvailable == false)
                    return;

                float coolTime = CHARGEZONE_CHARGE_COOLTIME;

                if (pm.IsMine)
                {
                    networkInGameRPCManager.RPC_SetPlayerBattery(pm.networkPlayerID, true, CHARGEZONE_CHARGE_AMOUNT, (int)NetworkInGameRPCManager.SetBatteryType.ChargeZone);
                }

                if (pm.playerCar != null)
                    pm.playerCar.ActivateChargeZoneFX();

                if (p == null)
                    chargeList.Add(new ChargeInfo() { pm = pm, counter = coolTime });
                else
                    p.counter = coolTime;
            }
        }
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

                var p = chargeList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isChargeAvailable == false)
                    return;

                if (pm.playerCar != null)
                    pm.playerCar.DeactivateChargeZoneFX();
            }
        }
    }

    private bool IsBlockEvent()
    {
        if (id == -1)
            return true;

        if (isActive == false)
            return true;

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
