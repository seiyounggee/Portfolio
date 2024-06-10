using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIControl_InGameButton : MonoBehaviour
{
    public enum ButtonType
    { 
        None,
        Jump,
        Attack,
        Skill
    }
    ButtonType CurrentButtonType = ButtonType.None;
    [ReadOnly] public bool isActive = true;
    [SerializeField] Image coolTimeImg = null;

    public Action OnClickButton;

    private Button btn;
    public Button Button { get { return btn; } }

    private float coolTimeLeft = 0f;

    protected void Awake()
    {
        btn = GetComponent<Button>();

        btn.onClick.AddListener(OnClickBtn);
    }

    private void Update()
    {
        Update_CheckActiveState();

        if (coolTimeImg != null)
        {
            if (isActive)
            {
                coolTimeImg.fillAmount = 0f;
            }
            else
            {
                coolTimeImg.fillAmount = coolTimeLeft;
            }
        }
    }

    public void SetButtonType(ButtonType type)
    {
        CurrentButtonType = type;
    }

    public void OnClickBtn()
    {
        OnClickButton?.Invoke();
    }

    private unsafe void Update_CheckActiveState()
    {
        var frame = NetworkManager_Client.Instance.GetFramePredicted();

        if (frame == null)
            return;

        var myPlayer = InGame_Quantum.Instance.myPlayer;

        if (myPlayer == null)
            return;


        if (frame.Unsafe.TryGetPointer<Quantum.PlayerRules>(myPlayer.entityRef, out Quantum.PlayerRules* pr))
        {
            switch (CurrentButtonType)
            {
                case ButtonType.Attack:
                    {
                        isActive = (pr->inputAttackCooltimeCounter.AsFloat <= 0f) ? true : false;

                        if (isActive == false)
                        {
                            if (pr->attackDuration.RawValue > 0)
                                coolTimeLeft = pr->inputAttackCooltimeCounter.AsFloat / pr->inputAttackCooltime.AsFloat;
                        }
                    }
                    break;

                case ButtonType.Jump:
                    {
                        isActive = (pr->inputJumpCooltimeCounter.AsFloat <= 0f) ? true : false;

                        if (isActive == false)
                        {
                            if (pr->attackDuration.RawValue > 0)
                                coolTimeLeft = pr->inputJumpCooltimeCounter.AsFloat / pr->inputJumpCooltime.AsFloat;
                        }
                    }
                    break;
                case ButtonType.Skill:
                    {
                        isActive = (pr->inputSkillCooltimeCounter.AsFloat <= 0f) ? true : false;

                        if (isActive == false)
                        {
                            if (pr->attackDuration.RawValue > 0)
                                coolTimeLeft = pr->inputSkillCooltimeCounter.AsFloat / pr->inputSkillCooltime.AsFloat;
                        }
                    }
                    break;
            }
        }
    }


}
