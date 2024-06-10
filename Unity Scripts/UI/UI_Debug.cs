using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Quantum;
using UnityEngine.SceneManagement;

public class UI_Debug : UIBase
{
    [Header("Outgame Debug")]
    [SerializeField] GameObject outgameDebugBase = null;
    [SerializeField] UnityEngine.UI.Button btn_StartGameImmediately = null;

    [Header("Ingame Debug")]
    [SerializeField] GameObject ingameDebugBase = null;
    [SerializeField] UnityEngine.UI.Button btn_KillSelf = null;
    [SerializeField] UnityEngine.UI.Button btn_InvincibleSelf = null;
    [SerializeField] UnityEngine.UI.Button btn_AutoAttack = null;
    [SerializeField] UnityEngine.UI.Button btn_HealHP = null;
    [SerializeField] UnityEngine.UI.Button btn_BallDamageZero = null;
    [SerializeField] UnityEngine.UI.Button btn_BallMaxSpeed = null;
    [SerializeField] UnityEngine.UI.Button btn_AllPlayerHpToOne = null;

    private void Awake()
    {
        btn_KillSelf.SafeSetButton(OnClickBtn_Ingame);
        btn_InvincibleSelf.SafeSetButton(OnClickBtn_Ingame);
        btn_AutoAttack.SafeSetButton(OnClickBtn_Ingame);
        btn_HealHP.SafeSetButton(OnClickBtn_Ingame);
        btn_BallDamageZero.SafeSetButton(OnClickBtn_Ingame);
        btn_BallMaxSpeed.SafeSetButton(OnClickBtn_Ingame);
        btn_AllPlayerHpToOne.SafeSetButton(OnClickBtn_Ingame);
        btn_StartGameImmediately.SafeSetButton(OnClickBtn_Outgame);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void Show()
    {
        base.Show();

#if SERVERTYPE_RELEASE
        Hide();
#endif

        SetUI();
    }

    private void LateUpdate()
    {
        transform.SetAsLastSibling();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUI();
    }

    public void SetUI()
    {
        var scene = SceneManager.GetActiveScene();

        if (scene.name.Equals(CommonDefine.OutGameScene))
        {
            outgameDebugBase.SafeSetActive(true);
            ingameDebugBase.SafeSetActive(false);
        }
        else if (scene.name.Equals(CommonDefine.InGameScene))
        {
            outgameDebugBase.SafeSetActive(false);
            ingameDebugBase.SafeSetActive(true);
        }
    }

    private void OnClickBtn_Ingame(UnityEngine.UI.Button btn)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        var cmd = new Quantum.CommandCheat();

        if (InGame_Quantum.QuantumGame == null)
            return;

        if (btn == btn_KillSelf)
        {
            if (InGame_Quantum.Instance.myPlayer != null)
            {
                cmd.cheatType = (int)CheatType.KillSelf;
                cmd.targetEntity = InGame_Quantum.Instance.myPlayer.entityRef;

                var ui = PrefabManager.Instance.UI_ToastMessage;
                ui.SetMessage_NoLocalization("Cheat_KillSelf");
                ui.Show();
            }
        }
        else if (btn == btn_InvincibleSelf)
        {
            if (InGame_Quantum.Instance.myPlayer != null)
            {
                cmd.cheatType = (int)CheatType.InvincibleSelf;
                cmd.targetEntity = InGame_Quantum.Instance.myPlayer.entityRef;

                var ui = PrefabManager.Instance.UI_ToastMessage;
                ui.SetMessage_NoLocalization("InvincibleSelf");
                ui.Show();
            }
        }
        else if (btn == btn_AutoAttack)
        {
            if (InGame_Quantum.Instance.myPlayer != null)
            {
                cmd.cheatType = (int)CheatType.AutoAttack;
                cmd.targetEntity = InGame_Quantum.Instance.myPlayer.entityRef;

                var ui = PrefabManager.Instance.UI_ToastMessage;
                ui.SetMessage_NoLocalization("AutoAttack");
                ui.Show();
            }
        }
        else if (btn == btn_HealHP)
        {
            if (InGame_Quantum.Instance.myPlayer != null)
            {
                cmd.cheatType = (int)CheatType.HealHP;
                cmd.targetEntity = InGame_Quantum.Instance.myPlayer.entityRef;

                var ui = PrefabManager.Instance.UI_ToastMessage;
                ui.SetMessage_NoLocalization("HealHP");
                ui.Show();
            }
        }
        else if (btn == btn_BallDamageZero)
        {
            cmd.cheatType = (int)CheatType.BallDamageZero;

            var ui = PrefabManager.Instance.UI_ToastMessage;
            ui.SetMessage_NoLocalization("BallDamageZero");
            ui.Show();
        }
        else if (btn == btn_BallMaxSpeed)
        {
            cmd.cheatType = (int)CheatType.BallMaxSpeed;

            var ui = PrefabManager.Instance.UI_ToastMessage;
            ui.SetMessage_NoLocalization("BallMaxSpeed");
            ui.Show();
        }
        else if (btn == btn_AllPlayerHpToOne)
        {
            cmd.cheatType = (int)CheatType.AllPlayerHpToOne;

            var ui = PrefabManager.Instance.UI_ToastMessage;
            ui.SetMessage_NoLocalization("AllPlayerHpToOne");
            ui.Show();
        }

        InGame_Quantum.QuantumGame.SendCommand(cmd);
#endif
    }

    private void OnClickBtn_Outgame(UnityEngine.UI.Button btn)
    {
#if UNITY_EDITOR || SERVERTYPE_DEV
        if (btn == btn_StartGameImmediately)
        {
            var phaseInGameReady = PhaseManager.Instance.GetPhase<PhaseInGameReady>(CommonDefine.Phase.InGameReady);
            if (phaseInGameReady != null)
            {
                phaseInGameReady.SetRoomWaitTime(0);
            }
        }
#endif
    }
}
