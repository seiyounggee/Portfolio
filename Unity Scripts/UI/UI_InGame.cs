using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_InGame : UIBase
{
    [Header("Player Info List")]
    [SerializeField] public Transform playerInfoListParentTrans = null;
    [SerializeField] public UIComponent_PlayerInfo playerInfoTemplate = null;
    private List<UIComponent_PlayerInfo> listOfPlayerInfoUI = new List<UIComponent_PlayerInfo>();

    [Header("Etc")]
    [SerializeField] GameObject ballTargetImg = null;

    [Header("Top Right Group")]
    [SerializeField] GameObject topRightGroupObj;
    [SerializeField] Button menuBtn = null;
    [SerializeField] Button cameraBtn = null;
    [SerializeField] GameObject cameraActivationIndicator = null;
    [SerializeField] Slider cameraSlider = null;
    [SerializeField] Button practiceModeExitBtn;

    [Header("Top Left Group")]
    [SerializeField] GameObject topLeftGroupObj;
    [SerializeField] TextMeshProUGUI aliveTxt;
    [SerializeField] TextMeshProUGUI killsTxt;
    [SerializeField] TextMeshProUGUI pingTxt;
    [SerializeField] TextMeshProUGUI fpsTxt;

    [Header("Bottom Right Group")]
    [SerializeField] GameObject bottomRightGroupObj;
    [SerializeField] public UIControl_InGameJoystick joystick = null;
    [SerializeField] public UIControl_InGameRotateArea rotateArea = null;
    [SerializeField] public UIControl_InGameButton btn_jump = null;
    [SerializeField] public UIControl_InGameButton btn_attack = null;
    [SerializeField] public UIControl_InGameButton btn_skill = null;

    [SerializeField] RawImage activeSkillImage = null;

    [Header("Bottom Center Group")]
    [SerializeField] GameObject bottomCenterGroupObj;
    [SerializeField] GameObject spectateObj;
    [SerializeField] Button spectateLeftBtn;
    [SerializeField] Button spectateRightBtn;
    [SerializeField] Button spectateExitBtn;

    [Header("Ingame Message")]
    [SerializeField] GameObject ingameMessageObj;
    [SerializeField] TextMeshProUGUI ingameMessageTxt;

    private float fpsDeltaTime = 0f;
    private Queue<float> fpsQueue = new Queue<float>();

    private void Awake()
    {
        menuBtn.SafeSetButton(OnClickBtn);
        cameraBtn.SafeSetButton(OnClickBtn);
        spectateLeftBtn.SafeSetButton(OnClickBtn);
        spectateRightBtn.SafeSetButton(OnClickBtn);
        spectateExitBtn.SafeSetButton(OnClickBtn);
        practiceModeExitBtn.SafeSetButton(OnClickBtn);

        btn_jump?.SetButtonType(UIControl_InGameButton.ButtonType.Jump);
        btn_attack?.SetButtonType(UIControl_InGameButton.ButtonType.Attack);
        btn_skill?.SetButtonType(UIControl_InGameButton.ButtonType.Skill);

        playerInfoTemplate.SafeSetActive(false);
    }

    public override void Show()
    {
        base.Show();

        ClearUI();
        DeactiavteGamePlayUI();
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        if (InGame_Quantum.Instance)
            InGame_Quantum.Instance.OnEventGamePlayStateChangedAction += OnEventGamePlayStateChanged;

        cameraSlider?.onValueChanged.AddListener(CamSlider_OnValueChanged);

        QuantumEvent.Subscribe<Quantum.EventPlayerEvents>(this, OnEventPlayerEvents);
        QuantumEvent.Subscribe<Quantum.EventBallTargetChanged>(this, OnEventBallTargetChanged);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        if (InGame_Quantum.Instance)
            InGame_Quantum.Instance.OnEventGamePlayStateChangedAction -= OnEventGamePlayStateChanged;

        cameraSlider?.onValueChanged.RemoveListener(CamSlider_OnValueChanged);

        QuantumEvent.UnsubscribeListener<Quantum.EventPlayerEvents>(this);
        QuantumEvent.UnsubscribeListener<Quantum.EventBallTargetChanged>(this);
    }

    private void LateUpdate()
    {
        LateUpdate_UI();
    }

    private void ClearUI()
    {
        ballTargetImg.SafeSetActive(false);
        playerInfoTemplate.SafeSetActive(false);
    }

    private void DeactiavteGamePlayUI()
    {
        joystick.SafeSetActive(false);
        topLeftGroupObj.SafeSetActive(false);
        topRightGroupObj.SafeSetActive(false);
        bottomRightGroupObj.SafeSetActive(false);
        bottomCenterGroupObj.SafeSetActive(false);

        ingameMessageObj.SafeSetActive(false);
    }

    private void ActiavteGamePlayUI_Alive()
    {
        topLeftGroupObj.SafeSetActive(true);
        topRightGroupObj.SafeSetActive(true);
        bottomRightGroupObj.SafeSetActive(true);
        bottomCenterGroupObj.SafeSetActive(true);

        joystick.SafeSetActive(true);
        joystick.InitializeSettings();

        btn_jump.SafeSetActive(true);
        btn_attack.SafeSetActive(true);
        btn_skill.SafeSetActive(true);
        cameraBtn.SafeSetActive(true);

        spectateObj.SafeSetActive(false);
        spectateExitBtn.SafeSetActive(false);

        practiceModeExitBtn.SafeSetActive(NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.PracticeMode);

        cameraActivationIndicator.SafeSetActive(CameraManager.Instance.CurrentIngameCameraMode == CameraManager.InGameCameraMode.LockIn);

        cameraSlider.SafeSetActive(true);
        cameraSlider.value = (CameraManager.Instance.CameraOffsetDistance - CameraManager.MIN_OFFSET_DISTANCE) / (CameraManager.MAX_OFFSET_DISTANCE - CameraManager.MIN_OFFSET_DISTANCE);

        SetUpSkillImage();
    }

    public void ActiavteGamePlayUI_Dead()
    {
        joystick.SafeSetActive(false);
        btn_jump.SafeSetActive(false);
        btn_attack.SafeSetActive(false);
        btn_skill.SafeSetActive(false);
        cameraBtn.SafeSetActive(false);
        cameraSlider.SafeSetActive(false);

        if (NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.SoloMode)
        {
            spectateObj.SafeSetActive(true);
            spectateObj.transform.localPosition = new Vector3(129f, 57.3f, 0f);
            spectateExitBtn.SafeSetActive(true);
        }
        else if (NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.TeamMode)
        {
            spectateObj.SafeSetActive(true);
            spectateObj.transform.localPosition = new Vector3(0f, 57.3f, 0f);
            spectateExitBtn.SafeSetActive(false);
        }

    }

    private void OnClickBtn(Button btn)
    {
        if (btn == menuBtn)
        {

        }
        else if (btn == cameraBtn)
        {
            if (CameraManager.Instance.CurrentIngameCameraMode == CameraManager.InGameCameraMode.LockIn)
            {
                CameraManager.Instance.ChangeIngameCameraMode(CameraManager.InGameCameraMode.SimpleLookAt);
                cameraActivationIndicator.SafeSetActive(false);
            }
            else if (CameraManager.Instance.CurrentIngameCameraMode == CameraManager.InGameCameraMode.SimpleLookAt)
            {
                CameraManager.Instance.ChangeIngameCameraMode(CameraManager.InGameCameraMode.LockIn);
                cameraActivationIndicator.SafeSetActive(true);
            }
        }
        else if (btn == spectateLeftBtn)
        {
            CameraManager.Instance.ChangeIngameCameraMode(CameraManager.InGameCameraMode.Spectator, -1);
        }
        else if (btn == spectateRightBtn)
        {
            CameraManager.Instance.ChangeIngameCameraMode(CameraManager.InGameCameraMode.Spectator, 1);
        }
        else if (btn == spectateExitBtn)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                return;

            if (NetworkManager_Client.Instance != null
                && PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame
                && InGame_Quantum.Instance.GamePlayState == Quantum.GamePlayState.Play)
            {
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameResult);
            }
        }
        else if (btn == practiceModeExitBtn)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                return;

            if (NetworkManager_Client.Instance != null
                && PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame
                && InGame_Quantum.Instance.GamePlayState == Quantum.GamePlayState.Play)
            {
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameResult);
            }
        }
    }

    private void CamSlider_OnValueChanged(float sliderValue)
    {
        CameraManager.Instance.ZoomInOutCamera_Ingame(sliderValue);
    }

    private void OnEventGamePlayStateChanged(Quantum.EventGamePlayStateChanged _event)
    {
        switch (_event.StateType)
        {
            case Quantum.GamePlayState.Wait:
                {

                }
                break;

            case Quantum.GamePlayState.Play:
                {
                    ActiavteGamePlayUI_Alive();
                    SetPlayerInfoList();
                }
                break;

            case Quantum.GamePlayState.End:
                {

                }
                break;
        }
    }

    public void SetPlayerInfoList()
    {
        foreach (var i in listOfPlayerInfoUI)
        {
            Destroy(i.gameObject);
        }
        listOfPlayerInfoUI.Clear();

        var list = InGame_Quantum.Instance.listOfPlayers;
        for (int i = 0; i < list.Count; i++)
        {
            var uiGO = GameObject.Instantiate(playerInfoTemplate);
            uiGO.transform.SetParent(playerInfoListParentTrans);
            uiGO.transform.localScale = Vector3.one;

            var uiScript = uiGO.GetComponent<UIComponent_PlayerInfo>();
            uiScript.SetData(list[i]);
            listOfPlayerInfoUI.Add(uiScript);
        }
    }

    private unsafe void LateUpdate_UI()
    {
        var frame = NetworkManager_Client.Instance.GetFramePredicted();

        if (listOfPlayerInfoUI != null && listOfPlayerInfoUI.Count > 0)
        {
            foreach (var i in listOfPlayerInfoUI)
            {
                if (i == null || i.info == null || i.info.enityGameObj == null || InGame_Quantum.Instance == null)
                {
                    i.SafeSetActive(false);
                    continue;
                }

                Vector3 addtionalPos = (i.info == InGame_Quantum.Instance.myPlayer) ? Vector3.up * 2.5f : Vector3.up * 3f;
                (bool isInside, Vector3 viewPoint) = CameraManager.Instance.IsInsideCameraView(i.info.enityGameObj.transform.position + addtionalPos);
                if (isInside && i.state == UIComponent_PlayerInfo.UIState.Show)
                {
                    i.gameObject.SafeSetActive(true);

                    i.transform.position = viewPoint;
                }
                else
                {
                    i.gameObject.SafeSetActive(false);
                }

            }

            //정렬 순서...
            var targetObj = CameraManager.Instance.InGameCamera_TargetPlayerGameObj;
            if (targetObj != null)
            {
                listOfPlayerInfoUI = listOfPlayerInfoUI.OrderByDescending(x => (x.PositionData() - targetObj.transform.position).sqrMagnitude).ToList();
                for (int i =0; i< listOfPlayerInfoUI.Count; i++)
                {
                    listOfPlayerInfoUI[i].transform.SetSiblingIndex(i);
                }

                for (int i = 0; i < listOfPlayerInfoUI.Count; i++)
                {
                    if (listOfPlayerInfoUI[i].info.entityRef.Equals(InGame_Quantum.Instance.myPlayer.entityRef))
                    {
                        listOfPlayerInfoUI[i].transform.SetAsLastSibling();  //내껀 항상 앞쪽에 보이도록...
                        break;
                    }
                }
            }
        }


        if (InGame_Quantum.Instance != null && InGame_Quantum.Instance.GameManager.HasValue)
        {
            var gm = InGame_Quantum.Instance.GameManager.Value;

            aliveTxt.SafeSetText(gm.alivePlayerCount.ToString());

            if (ballTargetImg != null && frame != null && listOfPlayerInfoUI != null && listOfPlayerInfoUI.Count > 0)
            {
                if (frame.Unsafe.TryGetPointer<Quantum.BallRules>(gm.ball, out var br))
                {
                    var target = br->TargetEntity;
                    if (target != Quantum.EntityRef.None)
                    {
                        foreach (var i in listOfPlayerInfoUI)
                        {
                            if (i.info != null && i.info.entityRef.Equals(target) && i.info.enityGameObj != null)
                            {
                                (bool isInside, Vector3 viewPoint) = CameraManager.Instance.IsInsideCameraView(i.info.enityGameObj.transform.position + Vector3.up);

                                if (isInside)
                                {
                                    ballTargetImg.transform.position = viewPoint;
                                }
                                else
                                {
                                    ballTargetImg.SafeSetActive(false);
                                }

                                break;
                            }
                        }
                    }
                    else
                        ballTargetImg.SafeSetActive(false);
                }
            }

        }


        fpsDeltaTime += (Time.unscaledDeltaTime - fpsDeltaTime) * 0.1f;
        if (fpsDeltaTime > 0)
        {
            float fps = 1.0f / fpsDeltaTime;

            fpsQueue.Enqueue(fps);

            var averageFPS = fpsQueue.Average();
            fpsTxt.SafeSetText(Mathf.Ceil(averageFPS).ToString());

            if (fpsQueue.Count > 100)
                fpsQueue.Dequeue();
        }

        pingTxt.SafeSetText(NetworkManager_Client.Instance.GetQuantumGamePing().ToString());
    }

    public void ActivateIngameMessage_Unlocalized(string msg, float delayTime = 1.5f)
    {
        if (gameObject.SafeIsActive())
        {
            ingameMessageTxt.SafeSetText(msg);
            ingameMessageObj.SafeSetActive(true);
            UtilityInvoker.Invoke(this, () => ingameMessageObj.SafeSetActive(false), delayTime, "ActivateIngameMessage_Unlocalized");
        }
    }

    public void ActivateIngameMessage_Localized(string msg_key, float delayTime = 1.5f, params object[] arg)
    {
        if (gameObject.SafeIsActive())
        {
            ingameMessageTxt.SafeLocalizeText(msg_key, arg);
            ingameMessageObj.SafeSetActive(true);
            UtilityInvoker.Invoke(this, () => ingameMessageObj.SafeSetActive(false), delayTime, "ActivateIngameMessage_Localized");
        }
    }

    private void SetUpSkillImage()
    {
        var list_passive = ResourceManager.Instance.PassiveSkillDataList;
        var list_active = ResourceManager.Instance.ActiveSkillDataList;

        if (AccountManager.Instance.AccountData != null && list_passive != null && list_active != null)
        {
            var skill_passive = list_passive.Find(x => x.id.Equals(AccountManager.Instance.AccountData.passiveSkillID));
            var skill_active = list_active.Find(x => x.id.Equals(AccountManager.Instance.AccountData.activeSkillID));

            if (skill_passive != null)
            {
                //passiveSkillImage.texture = skill_passive.texture;
            }

            if (skill_active != null)
            {
                activeSkillImage.texture = skill_active.texture;
            }
        }
    }

    private unsafe void OnEventPlayerEvents(Quantum.EventPlayerEvents _event)
    {
        switch (_event.PlayerEvent)
        {
            case Quantum.PlayerEvent.Event_Die:
                {
               
                }
                break;
        }
    }

    private unsafe void OnEventBallTargetChanged(Quantum.EventBallTargetChanged _event)
    {
        if (InGame_Quantum.Instance == null || InGame_Quantum.Instance.myPlayer == null)
            return;

        //내가 공격한 케이스에만 Target UI 보여주자...
        if (_event.TargetEntity != InGame_Quantum.Instance.myPlayer.entityRef
            && _event.PrevEntity == InGame_Quantum.Instance.myPlayer.entityRef)
        {
            ballTargetImg.SafeSetActive(true);
            UtilityInvoker.Invoke(this, () => ballTargetImg.SafeSetActive(false), 0.5f, "OnEventBallTargetChanged");
        }
    }
}
