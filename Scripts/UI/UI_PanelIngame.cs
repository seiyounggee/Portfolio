using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Fusion;

public class UI_PanelIngame : UI_Base
{
    [SerializeField] public UI_IngameControl_TouchSwipe ingameControl_touchSwipe = null;
    [SerializeField] public UI_IngameControl_VirtualPad ingameControl_virtualPad = null;

    private bool isTouch = false;
    private Vector2? dragStartPos = null;
    private Vector2? dragCurrPos = null;

    [SerializeField] GameObject popupHUDBase = null;

    [SerializeField] UILabel speedTxt = null;

    [SerializeField] UILabel lapCurrTxt = null;
    [SerializeField] UILabel lapTotalTxt = null;
    [SerializeField] UILabel lapTimeTxt = null;
    [SerializeField] UILabel batteryTxt = null;

    [SerializeField] UIProgressBar batteryProgressBar = null;
    [SerializeField] UISprite batteryProgressSprite = null;
    [SerializeField] GameObject batteryProgressThumb = null;
    [SerializeField] UIProgressBar batteryProgressBar_Moving = null;
    [SerializeField] UIProgressBar batteryProgressBar_MovingBack = null;
    [SerializeField] UISprite batteryProgress_MovingSprite = null;

    [SerializeField] GameObject popupCountDownBase = null;
    [SerializeField] GameObject countDown_number = null;
    [SerializeField] UILabel countDown_numberTxt = null;
    [SerializeField] GameObject countDown_Go = null;

    [SerializeField] GameObject popupFinishBase = null;
    [SerializeField] GameObject finishLabelGo = null;

    [SerializeField] GameObject popupResultBase = null;
    [SerializeField] GameObject raceOverObj = null;
    [SerializeField] UI_PanelIngame_Grid standingInfoGrid = null;
    [SerializeField] GameObject raceOverExitBtnBase = null;
    [SerializeField] GameObject raceOverExitBtn = null;

    [SerializeField] GameObject popupTextBase = null;
    [SerializeField] UILabel textBase_label = null;

    [SerializeField] UILabel fpsTxt = null;
    [SerializeField] UILabel pingTxt = null;
    [SerializeField] UILabel avgPingTxt = null;
    [SerializeField] UILabel etcDebugTxt = null;

    [SerializeField] GameObject rank_1_Txt, rank_2_Txt, rank_3_Txt, rank_4_Txt, rank_5_Txt;
    private List<GameObject> rankGoTxtList = new List<GameObject>();


    [SerializeField] public Color color_normal;
    [SerializeField] public Color color_booster_2;
    [SerializeField] public Color color_booster_3;

    [SerializeField] public UILabel labelDebug = null;

    [ReadOnly] public bool isIngameInputSet_Drag = false;
    [ReadOnly] public float ingameInput_resetCooltimer = 0f;
    [ReadOnly] public bool isIngameInput_continuousInputReady = false;
    [ReadOnly] public bool isIngameInput_continuousInput = false;
    private Vector2? startPosi_continuousInput = null;

    [SerializeField] public GameObject warning_Go = null;

    [SerializeField] public GameObject minimapBase = null;

    [SerializeField] public GameObject shieldBase = null;
    [SerializeField] public UISprite shieldSprite = null;

    [SerializeField] public UI_PanelIngame_Nickname nicknameGo = null;
    [SerializeField] public GameObject nicknameBase = null;
    [SerializeField] public List<UI_PanelIngame_Nickname> nicknameUIList = null;

    private float PLAYER_NICKNAME_SHOW_DIST = 55f;

    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkInGameRPCManager myNetworkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    private void Awake()
    {
        raceOverExitBtn.SafeSetButton(OnClickBtn);
    }

    private void Update()
    {
        if (warning_Go != null && warning_Go.activeSelf)
        {
            if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame || InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown)
            {
                var list = InGameManager.Instance.ListOfPlayerBehind;
                if (list != null && list.Count > 0 && list[0].pm != null)
                {
                    var pos = GetWarningIndicatorLocalPosition(list[0].pm.transform.position);
                    warning_Go.transform.localPosition = pos;
                }
            }
            else
            {
                warning_Go.SafeSetActive(false);
            }
        }
    }

    private void FixedUpdate()
    {
        if (DataManager.GAME_INPUT_USE_CONTINUOUSINPUT == true)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
            {
                if (isIngameInput_continuousInputReady)
                {
                    ingameInput_resetCooltimer += Time.fixedDeltaTime;

                    if (ingameInput_resetCooltimer > DataManager.GAME_INPUT_RESET_COOLTIME)
                    {
                        isIngameInput_continuousInput = true;
                        isIngameInputSet_Drag = false;
                        dragCurrPos = null;
                        ingameInput_resetCooltimer = 0f;

                        if (startPosi_continuousInput.HasValue)
                            dragStartPos = startPosi_continuousInput.Value;
                    }
                }
                else
                {
                    ingameInput_resetCooltimer = 0f;
                }
            }
        }
    }

    public void Initialize()
    {
        ingameControl_touchSwipe.gameObject.SafeSetActive(true);

        isTouch = false;
        dragStartPos = null;
        dragCurrPos = null;
        isIngameInputSet_Drag = false;
        isIngameInput_continuousInputReady = false;
        isIngameInput_continuousInput = false;

        speedTxt.SafeSetText("");
        lapCurrTxt.SafeSetText("");
        lapTotalTxt.SafeSetText("");
        lapTimeTxt.SafeSetText("");
        batteryTxt.SafeSetText("");
        countDown_numberTxt.SafeSetText("");
        textBase_label.SafeSetText("");

        popupCountDownBase.SafeSetActive(false);
        countDown_number.SafeSetActive(false);
        countDown_Go.SafeSetActive(false);
        popupFinishBase.SafeSetActive(false);
        popupTextBase.SafeSetActive(false);

        rank_1_Txt.SafeSetActive(false);
        rank_2_Txt.SafeSetActive(false);
        rank_3_Txt.SafeSetActive(false);
        rank_4_Txt.SafeSetActive(false);
        rank_5_Txt.SafeSetActive(false);

        rankGoTxtList.Clear();
        rankGoTxtList.Add(rank_1_Txt);
        rankGoTxtList.Add(rank_2_Txt);
        rankGoTxtList.Add(rank_3_Txt);
        rankGoTxtList.Add(rank_4_Txt);
        rankGoTxtList.Add(rank_5_Txt);

        foreach (var i in rankGoTxtList)
            i.SafeSetActive(false);

        popupHUDBase.SafeSetActive(false);
        popupResultBase.SafeSetActive(false);
        raceOverObj.SafeSetActive(false);
        raceOverExitBtnBase.SafeSetActive(false);
        standingInfoGrid.gameObject.SafeSetActive(false);
        standingInfoGrid.Initialize();

        warning_Go.SetActive(false);

        if (shieldBase != null)
        {
            shieldBase.transform.SetParent(nicknameBase.transform);
            shieldBase.SafeSetActive(false);
        }

        nicknameBase.SafeSetActive(false);
        foreach (var i in nicknameUIList)
            Destroy(i.gameObject);
        nicknameUIList.Clear();

        minimapBase.SafeSetActive(true);

        ingameInput_resetCooltimer = 0f;
    }

    public void StartDrag_ObservedNotification(PlayerMovement.ObserverData_input data)
    {
        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            var mPlayer = InGameManager.Instance.myPlayer;
            if (mPlayer != null)
            {
                if ((InGameManager.Instance.gameState == InGameManager.GameState.EndCountDown
                    || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
                    && mPlayer.network_isEnteredTheFinishLine)
                {
                    myNetworkInGameRPCManager.RPC_LightUpHeadlightsAndHonkHorn(mPlayer.networkPlayerID, true);
                }
            }

            isTouch = true;
            isIngameInput_continuousInputReady = false;
            isIngameInputSet_Drag = false;
            startPosi_continuousInput = null;
            dragStartPos = null;
            dragCurrPos = null;
        }
    }

    public void Drag_ObservedNotification(PlayerMovement.ObserverData_input data)
    {
        Vector3 posi = data.inputPosition;

        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            if (dragStartPos == null)
                dragStartPos = posi;

            dragCurrPos = posi;


            if (isIngameInputSet_Drag == false)
            {
                var mPlayer = InGameManager.Instance.myPlayer;
                if (mPlayer != null)
                {
                    if (dragStartPos.HasValue && dragCurrPos.HasValue)
                    {
                        var diff_leftRight = dragCurrPos.Value.x - dragStartPos.Value.x;
                        float length_leftRight = CommonDefine.ScreenSwipeLength(diff_leftRight);

                        var diff_topDown = dragCurrPos.Value.y - dragStartPos.Value.y;
                        float length_topDown = CommonDefine.ScreenSwipeLength(diff_topDown);

                        if (Mathf.Abs(length_leftRight) >= Mathf.Abs(length_topDown))
                        {
                            if (Mathf.Abs(length_leftRight) > DataManager.GAME_INPUT_SWIPE_MIN_LENGHT)
                            {
                                switch (InGameManager.Instance.gameState)
                                {
                                    case InGameManager.GameState.PlayGame:
                                    case InGameManager.GameState.EndCountDown:
                                        {
                                            if (InGameManager.Instance.isInputDelay)
                                                return;

                                            if (mPlayer.isGrounded == false)
                                                return;

                                            if (mPlayer.isOutOfBoundary)
                                                return;

                                            if (mPlayer.isFlipped)
                                                return;

                                            if (mPlayer.network_isEnteredTheFinishLine)
                                                return;

                                            if (mPlayer.IsStopInputIfSheild())
                                                return;

                                            //diff 따라 왼쪽 오른쪽 판별  
                                            if (length_leftRight < 0)
                                            {
                                                myNetworkInGameRPCManager.RPC_ChangeLane_Left(mPlayer.networkPlayerID);
                                                isIngameInputSet_Drag = true;
                                                dragStartPos = posi;
                                            }
                                            else
                                            {
                                                myNetworkInGameRPCManager.RPC_ChangeLane_Right(mPlayer.networkPlayerID);
                                                isIngameInputSet_Drag = true;
                                                dragStartPos = posi;
                                            }

                                            if (isIngameInput_continuousInput)
                                            {
                                                ingameInput_resetCooltimer = 0f;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (Mathf.Abs(length_topDown) > DataManager.GAME_INPUT_SWIPE_MIN_LENGHT)
                            {
                                if (length_topDown >= 0)
                                {
                                    switch (InGameManager.Instance.gameState)
                                    {
                                        case InGameManager.GameState.StartCountDown:
                                            {
                                                mPlayer.ActivateStartingBooster();
                                            }
                                            break;

                                        case InGameManager.GameState.PlayGame:
                                        case InGameManager.GameState.EndCountDown:
                                            {
                                                PlayerMovement.CarBoosterType boosterLv = mPlayer.GetAvailableInputBooster();

                                                if (InGameManager.Instance.isInputDelay)
                                                    return;

                                                if (mPlayer.isGrounded == false)
                                                    return;

                                                if (mPlayer.isShield == true)
                                                    return;

                                                if (mPlayer.isOutOfBoundary)
                                                    return;

                                                if (mPlayer.isStunned)
                                                    return;

                                                if (mPlayer.network_isEnteredTheFinishLine)
                                                    return;

                                                if (boosterLv != PlayerMovement.CarBoosterType.None)
                                                {
                                                    myNetworkInGameRPCManager.RPC_BoostPlayer(mPlayer.networkPlayerID, (int)boosterLv, (int)mPlayer.currentTimingBoosterSuccessType);
                                                }
                                                else // boosterLv == PlayerMovement.CarBoosterLevel.None
                                                {
                                                    if (mPlayer.IsTimingBoosterReady)
                                                        mPlayer.ResetTimingBooster();
                                                }


                                                isIngameInputSet_Drag = true;
                                                dragStartPos = posi;
                                            }
                                            break;
                                    }

                                }
                                else
                                {
                                    switch (InGameManager.Instance.gameState)
                                    {
                                        case InGameManager.GameState.PlayGame:
                                        case InGameManager.GameState.EndCountDown:
                                            {
                                                if (InGameManager.Instance.isInputDelay)
                                                    return;

                                                if (mPlayer.isGrounded == false)
                                                    return;

                                                if (mPlayer.isOutOfBoundary)
                                                    return;

                                                if (mPlayer.isStunned)
                                                    return;

                                                if (mPlayer.isFlipped)
                                                    return;

                                                if (mPlayer.network_isEnteredTheFinishLine)
                                                    return;

                                                if (mPlayer.isShield || mPlayer.isShieldCooltime)
                                                    return;

                                                myNetworkInGameRPCManager.RPC_Shield(mPlayer.networkPlayerID);
                                                isIngameInputSet_Drag = true;
                                                dragStartPos = posi;
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (DataManager.GAME_INPUT_USE_CONTINUOUSINPUT == true)
                {
                    startPosi_continuousInput = posi;

                    if (dragStartPos.HasValue && dragCurrPos.HasValue)
                    {
                        switch (InGameManager.Instance.gameState)
                        {
                            case InGameManager.GameState.PlayGame:
                            case InGameManager.GameState.EndCountDown:
                                {
                                    if (Vector3.Distance(dragStartPos.Value, dragCurrPos.Value) < DataManager.GAME_INPUT_RESET_STAYDIST_RADIUS)
                                    {
                                        isIngameInput_continuousInputReady = true;
                                    }
                                    else
                                    {
                                        isIngameInput_continuousInputReady = false;
                                        dragStartPos = posi;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    public void EndDrag_ObservedNotification(PlayerMovement.ObserverData_input data)
    {
        Vector3 posi = data.inputPosition;

        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            var mPlayer = InGameManager.Instance.myPlayer;
            if (mPlayer != null)
            {
                switch (InGameManager.Instance.gameState)
                {
                    case InGameManager.GameState.EndCountDown:
                    case InGameManager.GameState.EndGame:
                        {
                            if (mPlayer.network_isEnteredTheFinishLine)
                            {
                                myNetworkInGameRPCManager.RPC_LightUpHeadlightsAndHonkHorn(mPlayer.networkPlayerID, false);
                            }
                        }
                        break;
                }
            }

            startPosi_continuousInput = null;
            isIngameInput_continuousInputReady = false;
            isIngameInput_continuousInput = false;
            isIngameInputSet_Drag = false;
            isTouch = false;
            dragStartPos = null;
            dragCurrPos = null;
        }
    }


    public void DeactiveHUDBase()
    {
        popupHUDBase.SafeSetActive(false);

        foreach (var i in rankGoTxtList)
            i.SafeSetActive(false);
    }

    public void ActivateIngamePlayTxt()
    {
        StartCoroutine(UpdateIngamePlayTxt());
    }

    private IEnumerator UpdateIngamePlayTxt()
    {
        popupHUDBase.SafeSetActive(true);

        float progressBarValueCounter = 0f;

        while (true)
        {
            if (speedTxt != null)
            {
                if (InGameManager.Instance.myPlayer != null)
                {
                    var speed = InGameManager.Instance.myPlayer.GetShowMoveSpeed();

                    if (Math.Abs(speed % 1) <= (Double.Epsilon * 100)) //소수점 존재x
                    {
                        speedTxt.text = speed.ToString() + ".0";
                    }
                    else
                    {
                        speedTxt.text = speed.ToString();
                    }
                }
            }

            if (rank_1_Txt != null && rank_2_Txt != null && rank_3_Txt != null && rank_4_Txt != null && rank_5_Txt != null)
            {
                if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame
                    && InGameManager.Instance.myPlayerPassedFinishLine == false)
                {
                    if (InGameManager.Instance.myCurrentRank >= 1)
                    {
                        for (int i = 0; i < rankGoTxtList.Count; i++)
                        {
                            if (i == InGameManager.Instance.myCurrentRank - 1)
                                rankGoTxtList[i].SafeSetActive(true);
                            else
                                rankGoTxtList[i].SafeSetActive(false);
                        }
                    }
                    else
                    {
                        foreach (var i in rankGoTxtList)
                            i.SafeSetActive(false);
                    }
                }
            }

            if (lapCurrTxt != null && lapTotalTxt != null)
            {
                if (InGameManager.Instance.myPlayer != null)
                {
                    if (DataManager.FinalLapCount != -1)
                    {
                        lapCurrTxt.SafeSetText((InGameManager.Instance.myPlayer.network_currentLap + 1).ToString());
                        lapTotalTxt.SafeSetText("/" + DataManager.FinalLapCount.ToString());
                    }
                    else
                    {
                        lapCurrTxt.SafeSetText((InGameManager.Instance.myPlayer.network_currentLap + 1).ToString());
                        lapTotalTxt.SafeSetText("/-");
                    }
                }
            }

            if (lapTimeTxt != null)
            {
                lapTimeTxt.SafeSetText(UtilityCommon.GetTimeString_TYPE_2(InGameManager.Instance.totalTimeGameElapsed));
            }

            if (pingTxt != null)
            {
                pingTxt.SafeSetText(string.Format("Ping: {0} ms",
                    PhotonNetworkManager.Instance.GetPing()));
            }

            if (avgPingTxt != null)
            {
                avgPingTxt.SafeSetText(string.Format("avg Ping: {0} ms",
                    PhotonNetworkManager.Instance.GetAveragePing()));
            }

            if (fpsTxt != null)
            {
                fpsTxt.SafeSetText("FPS: " + Mathf.Round(FpsCounter.fps));
            }

            if (etcDebugTxt != null)
            {
                if (PhotonNetworkManager.Instance.MyRegion != null)
                    etcDebugTxt.SafeSetText("Region: " + PhotonNetworkManager.Instance.MyRegion);
            }

            if (batteryTxt != null && batteryProgressBar != null && batteryProgressSprite != null)
            {
                if (InGameManager.Instance.myPlayer != null)
                {
                    batteryTxt.SafeSetText(((int)InGameManager.Instance.myPlayer.GetCurrentBatteryPercent()).ToString() + "%");

                    batteryProgressBar.value = InGameManager.Instance.myPlayer.GetCurrentBatteryRate();

                    var boosterLv = InGameManager.Instance.myPlayer.GetAvailableInputBooster();
                    switch (boosterLv)
                    {
                        case PlayerMovement.CarBoosterType.None:
                        default:
                            batteryProgressSprite.color = color_normal;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(false);
                            batteryProgressThumb.SafeSetActive(false);
                            break;
                        case PlayerMovement.CarBoosterType.CarBooster_LevelOne:
                            batteryProgressSprite.color = color_normal;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(false);
                            batteryProgressThumb.SafeSetActive(false);
                            break;
                        case PlayerMovement.CarBoosterType.CarBooster_LevelTwo:
                            batteryProgressSprite.color = color_booster_2;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(true);
                            batteryProgressThumb.SafeSetActive(true);
                            break;
                        case PlayerMovement.CarBoosterType.CarBooster_LevelThree:
                            batteryProgressSprite.color = color_booster_3;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(true);
                            batteryProgressThumb.SafeSetActive(true);
                            break;
                        case PlayerMovement.CarBoosterType.CarBooster_LevelFour_Timing:
                            break;
                    }

                    var tick = Time.fixedDeltaTime * progressBarSpeed_1_increase;

                    if (progressBarValueCounter < InGameManager.Instance.myPlayer.GetCurrentBatteryRate() - tick)
                        progressBarValueCounter += Time.fixedDeltaTime * progressBarSpeed_1_increase;
                    else if (progressBarValueCounter > InGameManager.Instance.myPlayer.GetCurrentBatteryRate() + tick)
                        progressBarValueCounter -= Time.fixedDeltaTime * progressBarSpeed_1_decrease;

                    progressBarValueCounter = Math.Clamp(progressBarValueCounter, 0, 1);
                    batteryProgressBar.value = progressBarValueCounter;

                    if (batteryProgressBar_MovingBack.gameObject.activeSelf)
                        batteryProgressBar_MovingBack.value = progressBarValueCounter;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void ActivateMovingProgressBar()
    {
        UtilityCoroutine.StopCoroutine(ref updateMovingProgressBar, this);
        UtilityCoroutine.StartCoroutine(ref updateMovingProgressBar, UpdateMovingProgressBar(), this);
    }

    public void DeactivateMovingProgressBar()
    {
        UtilityCoroutine.StopCoroutine(ref updateMovingProgressBar, this);
    }


    private float previousTargetRate = 0f;
    //private float targetRate = 0f;

    public float progressBarSpeed_1_increase = 1f;
    public float progressBarSpeed_1_decrease = 3f;
    public float progressBarSpeed_2 = 0.5f;
    private IEnumerator updateMovingProgressBar;
    private IEnumerator UpdateMovingProgressBar()
    {
        if (batteryProgressBar_Moving == null || batteryProgress_MovingSprite == null || batteryProgressBar_MovingBack == null)
        {
            Debug.Log("<colore=red>Error...! ProgressBar is missing... Check your inspector!</color>");
            yield break;
        }

        var p = InGameManager.Instance.myPlayer;
        if (p == null)
            yield break;

        previousTargetRate = (float)p.previousBatteryAtBooster / p.ref_batteryMax;
        batteryProgressBar_Moving.gameObject.SafeSetActive(true);
        batteryProgressBar_Moving.value = previousTargetRate;

        float valueCounter = previousTargetRate;
        while (true)
        {
            if (p == null)
                break;

            valueCounter -= Time.fixedDeltaTime * progressBarSpeed_2;
            valueCounter = Math.Clamp(valueCounter, 0, 1);
            batteryProgressBar_Moving.value = valueCounter;

            if (valueCounter < p.GetCurrentBatteryRate())
                break;

            yield return new WaitForFixedUpdate();
        }

        batteryProgressBar_Moving.gameObject.SafeSetActive(false);
    }


    public enum CountDownType { Start, End}
    public void ActivateCountDownTxt(CountDownType type, DateTime endTime)
    {
        UtilityCoroutine.StartCoroutine(ref updateCountDownText, UpdateCountDownTxt(type, endTime), this);
    }

    public void DeactivateCountDownTxt()
    {
        UtilityCoroutine.StopCoroutine(ref updateCountDownText, this);
        popupCountDownBase.SafeSetActive(false);
    }

    private IEnumerator updateCountDownText = null;
    private IEnumerator UpdateCountDownTxt(CountDownType type, DateTime endTime)
    {
        if (endTime < PnixNetworkManager.Instance.ServerTime)
            yield break;

        countDown_Go.SafeSetActive(false);

        while (true)
        {
            var ts = (endTime - PnixNetworkManager.Instance.ServerTime);

            if ((int)ts.TotalSeconds <= 0)
            {
                break;
            }

            if (type == CountDownType.Start)
            {

            }
            else if (type == CountDownType.End)
            { 
            
            }

            countDown_number.SafeSetActive(true);
            popupCountDownBase.SafeSetActive(true);
            countDown_numberTxt.SafeSetText(((int)ts.TotalSeconds).ToString());

            yield return null;
        }


        countDown_numberTxt.SafeSetText("");
        countDown_number.SafeSetActive(false);
        popupCountDownBase.SafeSetActive(false);
    }

    public void ActivateGoTxt()
    {
        UtilityCoroutine.StartCoroutine(ref updateGoText, UpdateGoTxt(), this);
    }

    private IEnumerator updateGoText = null;
    private IEnumerator UpdateGoTxt()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundClip.CountDown_Go);
        popupCountDownBase.SafeSetActive(true);
        countDown_Go.SafeSetActive(true);
        yield return new WaitForSeconds(3f);
        countDown_Go.SafeSetActive(false);
        popupCountDownBase.SafeSetActive(false);
    }

    public void ActivatePassedFinishLineTxt()
    {
        UtilityCoroutine.StartCoroutine(ref updateEndFinishText, UpdateEndFinishText(), this);
    }

    private IEnumerator updateEndFinishText = null;
    private IEnumerator UpdateEndFinishText()
    {
        popupFinishBase.SafeSetActive(true);
        finishLabelGo.SafeSetActive(true);
        yield return new WaitForSeconds(2f);

        finishLabelGo.SafeSetActive(false);
        yield return null;

        ActivateStandingInfo();
    }

    public void ActivateStandingInfo()
    {
        popupFinishBase.SafeSetActive(true);
        finishLabelGo.SafeSetActive(false);

        if (popupFinishBase.activeSelf)
        {
            if (standingInfoGrid.gameObject.activeSelf == true)
                return;

            standingInfoGrid.gameObject.SafeSetActive(true);
            standingInfoGrid.SetAndShowStandingInfo();
        }
    }

    private void UpdateStandingInfo()
    {
        if (standingInfoGrid != null)
        {
            standingInfoGrid.UpdateInfo();
        }
    }

    public void ActivateEndGameResultTxt()
    {
        UtilityCoroutine.StartCoroutine(ref updateEndGameResultTxt, UpdateEndGameResultTxt(), this);
    }

    private IEnumerator updateEndGameResultTxt = null;
    private IEnumerator UpdateEndGameResultTxt()
    {
        popupHUDBase.SafeSetActive(false);
        popupCountDownBase.SafeSetActive(false);
        popupResultBase.SafeSetActive(true);
        raceOverObj.SafeSetActive(true);
        yield return new WaitForSeconds(1f);
        raceOverExitBtnBase.SafeSetActive(true);
    }

    public void ActivateWarningGO()
    {
        warning_Go.SafeSetActive(true);
    }

    public void DeactivateWarningGO()
    {
        warning_Go.SafeSetActive(false);
    }

    public void ActivateParryingCooltimeGO()
    {
        UtilityCoroutine.StartCoroutine(ref updateParryingCooltimeGO, UpdateParryingCooltimeGO(), this);
    }

    private IEnumerator updateParryingCooltimeGO = null;
    private IEnumerator UpdateParryingCooltimeGO()
    {
        var myP = InGameManager.Instance.myPlayer;

        while (true)
        {
            if (myP == null)
                break;

            if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
                break;

            if (myP.isShieldCooltime)
            {
                shieldBase.SafeSetActive(true);
                shieldSprite.fillAmount = myP.GetParryingCoolTimeLeftRate();
            }
            else
            {
                shieldBase.SafeSetActive(false);
            }

            yield return new WaitForFixedUpdate();
        }

        shieldBase.SafeSetActive(false);
    }

    public void ActivateTextBase(string msg)
    {
        textBase_label.SafeSetText(msg);
        popupTextBase.SafeSetActive(true);
        this.Invoke(() => { if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame) popupTextBase.SafeSetActive(false); }, 2.5f);
    }


    private void OnClickBtn(GameObject btn)
    {
        if (btn == raceOverExitBtn)
        {
            popupResultBase.SafeSetActive(true);

            //이미 자동으로 Room에서 Leave한 경우 그냥 Phase만 Change해주자...!
            if(PhotonNetworkManager.Instance.MyNetworkRunner == null ||
                PhotonNetworkManager.Instance.MyNetworkRunner.IsConnectedToServer == false)
            {
                PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);
            }
            else //보통 아래의 경우로 leave함!
            {
                PhotonNetworkManager.Instance.LeaveSession(LeaveRoomCallback);
            }
        }
    }

    private void LeaveRoomCallback()
    {
        PnixNetworkManager.Instance.SendLeaveRaceReq();
    }




    private Vector3 GetWarningIndicatorLocalPosition(Vector3 targetPos)
    {
        var ingameCam = Camera_Base.Instance.mainCam;
        var uiCam = UIRoot_Base.Instance.uiCam;

        if (ingameCam == null || uiCam == null || InGameManager.Instance.myPlayer == null)
            return Vector3.zero;

        var pos = Vector3.zero;

        var myPos = InGameManager.Instance.myPlayer.transform.position;
        var otherPos = targetPos;

        var direction = (otherPos - myPos).normalized;

        var checkPos = InGameManager.Instance.myPlayer.transform.position;
        int counter = 0;
        var newPos = checkPos;
        while (true)
        {
            newPos = checkPos + direction * counter;
            if (UtilityCommon.IsObjectVisible(ingameCam, newPos) == false)
            {
                break;
            }

            counter++;

            if (Vector3.Distance(newPos, checkPos) > DataManager.PLAYER_INDICATOR_CHECK_BEHIND_DIST)
                break;
        }

        pos = NGUIMath.WorldToLocalPoint(newPos, ingameCam, uiCam, warning_Go.transform);

        //TODO... 이렇게 숫자 박아 넣는데 맞는 건가>? 기기별로 다를수 있을듯....

        pos.z = 0f;
        pos.y = -580f;

        if (pos.x >= 300f)
            pos.x = 300f;
        else if (pos.x < -300f)
            pos.x = -300f;

        return pos;
    }


    public void ActivateNickname()
    {
        nicknameBase.SafeSetActive(true);

        if (InGameManager.Instance.ListOfPlayers != null && nicknameGo != null)
        {
            for (int i = 0; i < InGameManager.Instance.ListOfPlayers.Count; i++)
            {
                var go = Instantiate(nicknameGo.gameObject);
                go.transform.SetParent(nicknameBase.transform);
                go.transform.localScale = Vector3.one;
                if (go.TryGetComponent<UI_PanelIngame_Nickname>(out UI_PanelIngame_Nickname uiNickname))
                {
                    nicknameUIList.Add(uiNickname);
                    uiNickname.SetData(InGameManager.Instance.ListOfPlayers[i]);
                    go.SafeSetActive(false);
                }
            }
        }

        if (InGameManager.Instance.myPlayer != null)
        {
            foreach (var i in nicknameUIList)
            {
                if (i.playerInfo != null && i.playerInfo.pm != null)
                {
                    if (i.playerInfo.pm.networkPlayerID.Equals(InGameManager.Instance.myPlayer.networkPlayerID))
                    {
                        if (shieldBase != null)
                        {
                            shieldBase.transform.SetParent(i.transform);
                            shieldBase.transform.localPosition = new Vector3(0f, 60f, 0f);
                            shieldBase.transform.localScale = Vector3.one;
                        }
                        break;
                    }
                }
            }
        }

        UtilityCoroutine.StartCoroutine(ref updateNickname, UpdateNickname(), this);
    }

    private IEnumerator updateNickname = null;
    private IEnumerator UpdateNickname()
    {
        yield return null;

        if (nicknameUIList == null || nicknameUIList.Count <= 0)
            yield break;

        while (true)
        {
            for (int i = 0; i < nicknameUIList.Count; i++)
            {
                if (nicknameUIList[i].playerInfo == null || nicknameUIList[i].playerInfo.pm == null)
                {
                    if (nicknameUIList[i].gameObject.activeSelf == true)
                        nicknameUIList[i].gameObject.SafeSetActive(false);
                    continue;
                }

                if (UtilityCommon.IsObjectVisible(Camera_Base.Instance.mainCam, nicknameUIList[i].playerInfo.pm.transform.position)
                    && Vector3.Distance(InGameManager.Instance.myPlayer.transform.position, nicknameUIList[i].playerInfo.pm.transform.position) < PLAYER_NICKNAME_SHOW_DIST)
                {
                    if (nicknameUIList[i].gameObject.activeSelf == false)
                        nicknameUIList[i].gameObject.SafeSetActive(true);
                }
                else
                {
                    if (nicknameUIList[i].gameObject.activeSelf == true)
                        nicknameUIList[i].gameObject.SafeSetActive(false);
                }

            }

            if (InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
                break;

                
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < nicknameUIList.Count; i++)
            nicknameUIList[i].gameObject.SafeSetActive(false);
    }


    public void PlayerPassedFinishLine_ObservedNotification(NetworkId? id)
    {
        UpdateStandingInfo();
    }
}
