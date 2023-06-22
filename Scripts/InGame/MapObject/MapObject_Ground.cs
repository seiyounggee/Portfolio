using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_Ground : MonoBehaviour
{
    [ReadOnly] public int id = -1;

    public enum GroundType { None, Mud, Water}
    public GroundType currentGroundType = GroundType.None;

    [SerializeField] List<TriggerChecker> triggerChecker = new List<TriggerChecker>();

    [ReadOnly] public List<GroundInfo> groundStayList = new List<GroundInfo>();

    [ReadOnly] public float GROUND_MUD_DECREASE_SPEED = 10f; //머드 지형의 감속 속도

    [System.Serializable]
    public class GroundInfo
    {
        public PlayerMovement pm;
    }

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

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }

    public void SetData(int id)
    {
        this.id = id;

        GROUND_MUD_DECREASE_SPEED = DataManager.Instance.GetGameConfig<float>("mudDecreaseSpeed");
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

                var p = groundStayList.Find(x => x.pm.Equals(pm));
                if (p == null)
                    groundStayList.Add(new GroundInfo() { pm = pm });

                if (currentGroundType == GroundType.Mud)
                {
                    if (!pm.isFlipped && !pm.isOutOfBoundary)
                    {
                        pm.SetGroundSpeed(GroundType.Mud, GROUND_MUD_DECREASE_SPEED * -1f);

                        if (pm.playerCar != null)
                            pm.playerCar.ActivateMudFX();
                    }
                }
                else if (currentGroundType == GroundType.Water)
                {
                    InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Waterdrowning, pm.transform.position);

                    if (pm.IsMineAndNotAI)
                    {
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Water);
                        Camera_Base.Instance.FX_Water.SafeSetActive(false);
                        Camera_Base.Instance.FX_Water.SafeSetActive(true);
                    }
                }
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

                var p = groundStayList.Find(x => x.pm.Equals(pm));
                if (p == null)
                {
                    groundStayList.Add(new GroundInfo() { pm = pm });
                }

                if (currentGroundType == GroundType.Mud)
                {
                    if (!pm.isFlipped && !pm.isOutOfBoundary)
                    {
                        pm.SetGroundSpeed(GroundType.Mud, GROUND_MUD_DECREASE_SPEED * -1f);
                    }
                }
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

                var p = groundStayList.Find(x => x.pm.Equals(pm));
                if (p != null)
                {
                    groundStayList.Remove(p);
                }

                if (currentGroundType == GroundType.Mud)
                {
                    pm.SetGroundSpeed(GroundType.None);

                    if (pm.playerCar != null)
                        pm.playerCar.DeactivateMudFX();
                }
            }
        }
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
