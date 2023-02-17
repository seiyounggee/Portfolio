using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Fusion;

public class UI_PanelIngame : UI_PanelBase
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

    [SerializeField] GameObject popupResultBase = null;
    [SerializeField] GameObject raceOverObj = null;
    [SerializeField] UI_PanelIngame_Grid standingInfoGrid = null;
    [SerializeField] GameObject raceOverExitBtnBase = null;
    [SerializeField] GameObject raceOverExitBtn = null;

    [SerializeField] GameObject popupTextBase = null;
    [SerializeField] UILabel textBase_label = null;

    [SerializeField] UILabel pingTxt = null;
    [SerializeField] UILabel avgPingTxt = null;

    [SerializeField] GameObject rank_1_Txt, rank_2_Txt, rank_3_Txt, rank_4_Txt, rank_5_Txt;
    private List<GameObject> rankGoTxtList = new List<GameObject>();


    [SerializeField] public Color color_normal;
    [SerializeField] public Color color_booster_2;
    [SerializeField] public Color color_booster_3;

    [SerializeField] public UILabel labelDebug = null;

    [ReadOnly] public bool isIngameInputSet_Drag = false;

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

    public void Initialize()
    {
        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            ingameControl_touchSwipe.gameObject.SafeSetActive(true);
            ingameControl_virtualPad.gameObject.SafeSetActive(false);
        }
        else if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.VirtualPad)
        {
            ingameControl_touchSwipe.gameObject.SafeSetActive(false);
            ingameControl_virtualPad.gameObject.SafeSetActive(true);
        }
            
        isTouch = false;
        dragStartPos = null;
        dragCurrPos = null;
        isIngameInputSet_Drag = false;

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

        if (CommonDefine.GAME_MINIMAP_ACTIVATE == false)
            minimapBase.SafeSetActive(false);
        else
            minimapBase.SafeSetActive(true);
    }

    private void OnEnable()
    {
        PhotonNetworkManager.Instance.PhotonCallback_OnInput += PhotonCallback_OnInput;
    }

    private void OnDisable()
    {
        PhotonNetworkManager.Instance.PhotonCallback_OnInput -= PhotonCallback_OnInput;
    }

    public void OnDragStartCallback()
    {
        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            isTouch = true;
            isIngameInputSet_Drag = false;

            dragStartPos = null;
            dragCurrPos = null;
        }
    }

    public void OnDragCallback(Vector3 posi)
    {
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
                    if (mPlayer.isDecelerating == false)
                    {
                        if (dragStartPos.HasValue && dragCurrPos.HasValue)
                        {
                            var diff_leftRight = dragCurrPos.Value.x - dragStartPos.Value.x;
                            float length_leftRight = CommonDefine.ScreenSwipeLength(diff_leftRight);

                            var diff_topDown = dragCurrPos.Value.y - dragStartPos.Value.y;
                            float length_topDown = CommonDefine.ScreenSwipeLength(diff_topDown);

                            if (Mathf.Abs(length_leftRight) >= Mathf.Abs(length_topDown))
                            {
                                if (Mathf.Abs(length_leftRight) > CommonDefine.GAME_INPUT_SWIPE_MIN_LENGHT)
                                {
                                    //diff 따라 왼쪽 오른쪽 판별  
                                    if (length_leftRight < 0)
                                    {
                                        myNetworkInGameRPCManager.RPC_ChangeLane_Left(mPlayer.PlayerID);
                                        isIngameInputSet_Drag = true;
                                    }
                                    else
                                    {
                                        myNetworkInGameRPCManager.RPC_ChangeLane_Right(mPlayer.PlayerID);
                                        isIngameInputSet_Drag = true;
                                    }
                                }
                            }
                            else
                            {
                                if (Mathf.Abs(length_topDown) > CommonDefine.GAME_INPUT_SWIPE_MIN_LENGHT)
                                {
                                    if (length_topDown >= 0)
                                    {
                                        PlayerMovement.CarBoosterLevel boosterLv = mPlayer.GetAvailableInputBooster();
                                        if (boosterLv != PlayerMovement.CarBoosterLevel.None)
                                        {
                                            myNetworkInGameRPCManager.RPC_BoostPlayer(mPlayer.PlayerID, (int)boosterLv);
                                        }

                                        isIngameInputSet_Drag = true;
                                        dragStartPos = posi;
                                    }
                                    else
                                    {
                                        myNetworkInGameRPCManager.RPC_Deceleration_Start(mPlayer.PlayerID);
                                        isIngameInputSet_Drag = true;
                                        dragStartPos = posi;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                    }
                }
            }
        }
    }

    public void OnDragEndCallback()
    {
        if (CommonDefine.CurrentControlType() == CommonDefine.ControlType.TouchSwipe)
        {
            var mPlayer = InGameManager.Instance.myPlayer;
            if (mPlayer != null)
            {
                if (mPlayer.isDecelerating == false)
                {
                    if (dragStartPos.HasValue && dragCurrPos.HasValue)
                    {
                        var diff_leftRight = dragCurrPos.Value.x - dragStartPos.Value.x;
                        float length_leftRight = CommonDefine.ScreenSwipeLength(diff_leftRight);

                        var diff_topDown = dragCurrPos.Value.y - dragStartPos.Value.y;
                        float length_topDown = CommonDefine.ScreenSwipeLength(diff_topDown);

                        if (Mathf.Abs(length_leftRight) >= Mathf.Abs(length_topDown))
                        {
                            if (Mathf.Abs(length_leftRight) > CommonDefine.GAME_INPUT_SWIPE_MIN_LENGHT)
                            {
                                //diff 따라 왼쪽 오른쪽 판별  
                                if (length_leftRight < 0)
                                {

                                }
                                else
                                {

                                }
                            }
                        }
                        else
                        {
                            if (Mathf.Abs(length_topDown) > CommonDefine.GAME_INPUT_SWIPE_MIN_LENGHT)
                            {
                                if (length_topDown >= 0)
                                {

                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
                else
                {
                    myNetworkInGameRPCManager.RPC_Deceleration_End(mPlayer.PlayerID);
                }
            }

            isIngameInputSet_Drag = false;
            isTouch = false;
            dragStartPos = null;
            dragCurrPos = null;
        }
    }


    public void DeactiveHUDBase()
    {
        popupHUDBase.SafeSetActive(false);
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
                    if (CommonDefine.GetFinalLapCount() != -1)
                    {
                        lapCurrTxt.SafeSetText((InGameManager.Instance.myPlayer.currentLap + 1).ToString());
                        lapTotalTxt.SafeSetText("/" + CommonDefine.GetFinalLapCount().ToString());
                    }
                    else
                    {
                        lapCurrTxt.SafeSetText((InGameManager.Instance.myPlayer.currentLap + 1).ToString());
                        lapTotalTxt.SafeSetText("/-");
                    }
                }
            }

            if (lapTimeTxt != null)
            {
                lapTimeTxt.SafeSetText(UtilityCommon.GetTimeString_TYPE_2(InGameManager.Instance.myTimeRecordElapsed));
            }

            if (pingTxt != null)
            {
                pingTxt.SafeSetText(string.Format("ping: {0} [+/-{1}]ms",
                    PhotonNetworkManager.Instance.GetPing(),
                    PhotonNetworkManager.Instance.GetPingVariance()));
            }

            if (avgPingTxt != null)
            {
                avgPingTxt.SafeSetText(string.Format("avg ping: {0} ms",
                    PhotonNetworkManager.Instance.GetAveragePing()));
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
                        case PlayerMovement.CarBoosterLevel.None:
                        default:
                            batteryProgressSprite.color = color_normal;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(false);
                            batteryProgressThumb.SafeSetActive(false);
                            break;
                        case PlayerMovement.CarBoosterLevel.Car_One:
                            batteryProgressSprite.color = color_normal;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(false);
                            batteryProgressThumb.SafeSetActive(false);
                            break;
                        case PlayerMovement.CarBoosterLevel.Car_Two:
                            batteryProgressSprite.color = color_booster_2;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(true);
                            batteryProgressThumb.SafeSetActive(true);
                            break;
                        case PlayerMovement.CarBoosterLevel.Car_Three:
                            batteryProgressSprite.color = color_booster_3;
                            batteryProgressBar_MovingBack.gameObject.SafeSetActive(true);
                            batteryProgressThumb.SafeSetActive(true);
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

        previousTargetRate = (float)p.previousBatteryAtBooster / p.PLAYER_BATTERY_MAX;
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


    public void ActivateCountDownTxt(int startTime, int delayTime = 0)
    {
        UtilityCoroutine.StartCoroutine(ref updateCountDownText, UpdateCountDownTxt(startTime, delayTime), this);
    }

    public void DeactivateCountDownTxt()
    {
        UtilityCoroutine.StopCoroutine(ref updateCountDownText, this);
        popupCountDownBase.SafeSetActive(false);
    }

    private IEnumerator updateCountDownText = null;
    private IEnumerator UpdateCountDownTxt(int startTime, int delayTime = 0)
    {
        int timeCounter = startTime;

        if (delayTime > 0f)
        {
            yield return new WaitForSeconds(delayTime);

            timeCounter = startTime - delayTime;
        }

        countDown_Go.SafeSetActive(false);

        while (true)
        {
            //애니메이션 때문에 false -> true
            countDown_number.SafeSetActive(false);
            countDown_number.SafeSetActive(true);

            if (popupFinishBase.activeSelf == true)
                popupCountDownBase.SafeSetActive(false);
            else
                popupCountDownBase.SafeSetActive(true);

            if (timeCounter <= 0)
            {
                break;
            }

            SoundManager.Instance.PlaySound(SoundManager.SoundClip.CountDown_Beep);

            countDown_numberTxt.SafeSetText(timeCounter.ToString());

            yield return new WaitForSeconds(1f);

            timeCounter -= 1;
        }

        countDown_numberTxt.SafeSetText("");
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


        foreach (var i in rankGoTxtList)
            i.SafeSetActive(false);
    }

    private IEnumerator updateEndFinishText = null;
    private IEnumerator UpdateEndFinishText()
    {
        popupFinishBase.SafeSetActive(true);

        yield return new WaitForSeconds(3f);

        popupFinishBase.SafeSetActive(false);
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
        standingInfoGrid.gameObject.SafeSetActive(true);
        standingInfoGrid.SetStandingInfo();
        if (InGameManager.Instance.ListOfPlayers != null && InGameManager.Instance.ListOfPlayers.Count > 0)
            yield return new WaitForSeconds(InGameManager.Instance.ListOfPlayers.Count + 1f);
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

            if (myP.isParryingCooltime)
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

        DeactivateParryingCooltimeGO();
    }



    public void DeactivateParryingCooltimeGO()
    {
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
            //if (Photon.Pun.PhotonNetwork.OfflineMode || Photon.Pun.PhotonNetwork.CurrentRoom == null)
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
        PhaseManager.Instance.ChangePhase(CommonDefine.Phase.Lobby);
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

            if (Vector3.Distance(newPos, checkPos) > CommonDefine.PLAYER_INDICATOR_CHECK_BEHIND_DIST)
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
                    if (i.playerInfo.pm.PlayerID.Equals(InGameManager.Instance.myPlayer.PlayerID))
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


    private void PhotonCallback_OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.InGame)
        {
            var data = new TouchSwipe_NetworkInputData();

            var ts = PrefabManager.Instance.UI_PanelIngame.ingameControl_touchSwipe;
            if (ts != null)
            {
                if (UI_IngameControl_TouchSwipe.OnDragStart)
                {
                    data.currentEventType = TouchSwipe_NetworkInputData.EventType.StartDrag;
                }
                else if (UI_IngameControl_TouchSwipe.OnDrag)
                {
                    data.currentEventType = TouchSwipe_NetworkInputData.EventType.Drag;
                    data.position = UI_IngameControl_TouchSwipe.OnDragPositionInfo;
                }
                else if (UI_IngameControl_TouchSwipe.OnDragEnd)
                {
                    data.currentEventType = TouchSwipe_NetworkInputData.EventType.EndDrag;
                }

                input.Set(data);
            }
        }
    }
}
