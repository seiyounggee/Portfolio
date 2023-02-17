using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_Ground : MonoBehaviour
{
    [ReadOnly] public int id = -1;

    public enum GroundType { None, Mud, Ocean}
    public GroundType currentGroundType = GroundType.None;

    [SerializeField] List<TriggerChecker> triggerChecker = new List<TriggerChecker>();

    [ReadOnly] public List<GroundInfo> groundStayList = new List<GroundInfo>();

    public float GROUND_MUD_DECREASE_SPEED = 10f;

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
        DataManager.Instance.firebaseBasicValueChangedCallback += OnFirebaseBasicValueChangedCallback;
    }

    public void OnDisable()
    {

        DataManager.Instance.firebaseBasicValueChangedCallback -= OnFirebaseBasicValueChangedCallback;
    }

    public void SetData(int id)
    {
        this.id = id;


        if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.groundData != null)
        {
            GROUND_MUD_DECREASE_SPEED = DataManager.Instance.basicData.groundData.GROUND_MUD_DECREASE_SPEED;
        }
    }

    public void SetData()
    {
        if (DataManager.Instance.basicData != null && DataManager.Instance.basicData.groundData != null)
        {
            GROUND_MUD_DECREASE_SPEED = DataManager.Instance.basicData.groundData.GROUND_MUD_DECREASE_SPEED;
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

                if (pm.isEnteredTheFinishLine)
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
                else if (currentGroundType == GroundType.Ocean)
                {
                    InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Waterdrowning, pm.transform.position);

                    if (pm.IsMine)
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Water);
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

                if (pm.isEnteredTheFinishLine)
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
                if (pm.isEnteredTheFinishLine)
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
