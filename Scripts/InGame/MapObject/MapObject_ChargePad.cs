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

    public float CHARGEPAD_LV2_CHARGE_RESET_TIME = 5f;
    public float CHARGEPAD_LV3_CHARGE_RESET_TIME = 3f;

    public float CHARGEPAD_LV1_ANIM_SPEED = 1f;
    public float CHARGEPAD_LV2_ANIM_SPEED = 1.2f;
    public float CHARGEPAD_LV3_ANIM_SPEED = 1.5f;

    public const string ANIM_SPEED = "ANIM_SPEED";

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
        DataManager.Instance.firebaseBasicValueChangedCallback += OnFirebaseBasicValueChangedCallback;
    }

    public void OnDisable()
    {

        DataManager.Instance.firebaseBasicValueChangedCallback -= OnFirebaseBasicValueChangedCallback;
    }

    public void SetData(int id)
    {
        this.id = id;
        currentLevel = ChargePadLevel.One;
        SetMat();

        if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.chargePadData != null)
        {
            CHARGEPAD_LV2_CHARGE_RESET_TIME = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV2_CHARGE_RESET_TIME;
            CHARGEPAD_LV3_CHARGE_RESET_TIME = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV3_CHARGE_RESET_TIME;
            CHARGEPAD_LV1_ANIM_SPEED = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV1_ANIM_SPEED;
            CHARGEPAD_LV2_ANIM_SPEED = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV2_ANIM_SPEED;
            CHARGEPAD_LV3_ANIM_SPEED = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV3_ANIM_SPEED;
        }

        SetAnim();
    }

    public void SetData()
    {
        if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.chargePadData != null)
        {
            CHARGEPAD_LV2_CHARGE_RESET_TIME = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV2_CHARGE_RESET_TIME;
            CHARGEPAD_LV3_CHARGE_RESET_TIME = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV3_CHARGE_RESET_TIME;
            CHARGEPAD_LV1_ANIM_SPEED = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV1_ANIM_SPEED;
            CHARGEPAD_LV2_ANIM_SPEED = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV2_ANIM_SPEED;
            CHARGEPAD_LV3_ANIM_SPEED = DataManager.Instance.basicData.chargePadData.CHARGEPAD_LV3_ANIM_SPEED;
        }
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

                if (pm.isEnteredTheFinishLine)
                    return;

                if (pm.IsMine)
                    networkInGameRPCManager.RPC_SetChargePad(id, (int)currentLevel, false, pm.PlayerID);
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

    private void OnFirebaseBasicValueChangedCallback()
    {
        SetData();
    }
}
