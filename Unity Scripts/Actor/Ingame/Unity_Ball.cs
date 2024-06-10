using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public class Unity_Ball : MonoBehaviour
{
    [ReadOnly] public EntityRef enityRef;
    [SerializeField] MeshRenderer ballMesh;
    Material ballMaterial;

    private Frame frame = null;

    private class ActivatedSkillInfo
    {
        public GameObject effectObj;
        public Unity_Effect effectScript;
        public Quantum.PlayerActiveSkill type;
    }
    private List<ActivatedSkillInfo> activatedSkillInfoList = new List<ActivatedSkillInfo>();


    private void Awake()
    {
        if (ballMesh != null)
            ballMaterial = ballMesh.material;
    }

    public void SetData(EntityRef _enityRef)
    {
        enityRef = _enityRef;
    }
    private void OnEnable()
    {
        frame = NetworkManager_Client.Instance.GetFrameVerified();

        QuantumEvent.Subscribe<EventBallEvents>(this, OnEventBallEvents);
    }

    private void OnDisable()
    {
        frame = null;

        QuantumEvent.UnsubscribeListener<EventBallEvents>(this);
    }

    private unsafe void Update()
    {
        if (NetworkManager_Client.Instance == null)
            return;

        var frame = NetworkManager_Client.Instance.GetFramePredicted();

        if (frame == null)
            return;

        if (enityRef != EntityRef.None && InGame_Quantum.Instance.ball != null && InGameManager.Instance.myPlayer != null)
        {
            if (frame.Unsafe.TryGetPointer<BallRules>(enityRef, out BallRules * br))
            {
                if (br->isActive == false)
                    this.gameObject.SafeSetActive(false);
                else
                    this.gameObject.SafeSetActive(true);

                //타겟의 경우 Skin Color 바꿔서 표시해주자
                if (InGameManager.Instance.myPlayer.entityRef.Equals(br->TargetEntity))
                    ballMaterial?.SetColor("_Color", Color.red);
                else
                    ballMaterial?.SetColor("_Color", Color.black);
            }
        }
    }

    private unsafe void OnEventBallEvents(EventBallEvents _event)
    {
        if (_event.Entity.Equals(enityRef) == false)
            return;

        switch (_event.BallEvent)
        {
            case BallEvent.Event_ResetMovementLogic:
                {
                    if (activatedSkillInfoList != null & activatedSkillInfoList.Count > 0)
                    {
                        foreach (var i in activatedSkillInfoList)
                        {
                            if (i != null && i.effectObj != null)
                            {
                                if (i.effectScript != null)
                                    i.effectScript.Hide();
                                else
                                    i.effectObj.SafeSetActive(false);
                            }
                        }
                        activatedSkillInfoList.Clear();
                    }
                }
                break;
            case BallEvent.Event_Active_FreezeBall:
                {
                    var effect  = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_Frost, transform.position, transform.rotation, 1f, transform);
                    activatedSkillInfoList.Add(new ActivatedSkillInfo() { effectObj = effect, effectScript = effect.GetComponent<Unity_Effect>(), type = PlayerActiveSkill.FreezeBall });
                }
                break;
            case BallEvent.Event_Active_FastBall:
                {
                    var effect = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_LightningOrbSharpPink, transform.position, transform.rotation, 1f, transform);
                    activatedSkillInfoList.Add(new ActivatedSkillInfo() { effectObj = effect, effectScript = effect.GetComponent<Unity_Effect>(), type = PlayerActiveSkill.FastBall });
                }
                break;
            case BallEvent.Event_Active_CurveBall:
                {
                    var effect = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_StormMissile, transform.position, transform.rotation, 1f, transform);
                    activatedSkillInfoList.Add(new ActivatedSkillInfo() { effectObj = effect, effectScript = effect.GetComponent<Unity_Effect>(), type = PlayerActiveSkill.CurveBall });
                }
                break;
            case BallEvent.Event_Active_SkyRocketBall:
                {
                    var effect = InGameManager.Instance.ActivatePooledObj(InGameManager.PooledType.Effect_MysticMissileDark, transform.position, transform.rotation, 1f, transform);
                    activatedSkillInfoList.Add(new ActivatedSkillInfo() { effectObj = effect, effectScript = effect.GetComponent<Unity_Effect>(), type = PlayerActiveSkill.SkyRocketBall });
                }
                break;
        }
    }
}
