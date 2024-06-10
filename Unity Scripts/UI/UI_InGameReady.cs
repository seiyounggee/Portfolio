using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_InGameReady : UIBase
{
    [SerializeField] Button cancelBtn;
    [SerializeField] TextMeshProUGUI roomStatusTxt;
    [SerializeField] TextMeshProUGUI timerTxt;
    [SerializeField] GameObject mapSlotBase;
    [SerializeField] Transform mapSlotParent;
    [SerializeField] UIComponent_InGameReady_MapSlot mapSlotTemplate;

    [SerializeField] GameObject finalSelectedSlotBase;
    [SerializeField] UIComponent_InGameReady_MapSlot finalSelectedSlot;

    [SerializeField] TextMeshProUGUI tipTxt;

    private List<UIComponent_InGameReady_MapSlot> slotList = new List<UIComponent_InGameReady_MapSlot>();

    private void Awake()
    {
        cancelBtn.SafeSetButton(OnClickBtn);

        mapSlotTemplate.SafeSetActive(false);
    }

    public override void Show()
    {
        base.Show();

        ShowOrHideCancelButton(true);
        SetupMapSlot();
        UtilityCoroutine.StartCoroutine(ref updateRoomStatusText, UpdateRoomStatusText(), this);
        UtilityCoroutine.StartCoroutine(ref updateTimerText, UpdateTimerText(), this);
        UtilityCoroutine.StartCoroutine(ref updateTipText, UpdateTipText(), this);
    }

    public override void Hide()
    {
        base.Hide();

        UtilityCoroutine.StopCoroutine(ref updateRoomStatusText, this);
        UtilityCoroutine.StopCoroutine(ref updateTimerText, this);
        UtilityCoroutine.StopCoroutine(ref updateTipText, this);

        timerTxt.SafeSetText(string.Empty);
    }

    private void SetupMapSlot()
    {
        foreach (var i in slotList)
            i.SafeSetActive(false);

        mapSlotBase.SafeSetActive(true);
        finalSelectedSlotBase.SafeSetActive(false);

        if (ReferenceManager.Instance.MapData != null && ReferenceManager.Instance.MapData.MapInfoList != null)
        {
            var md = ReferenceManager.Instance.MapData;
            var list = ReferenceManager.Instance.MapData.MapInfoList;
            var mapListToPlay = list.FindAll(x => x.isOpen.Equals(true) && x.GroupID.Equals(CommonDefine.MAP_GROUP_ID_SOLO_OR_TEAM_MODE));
            mapListToPlay.AddRange(mapListToPlay); //List 

            if (mapListToPlay != null && mapListToPlay.Count > 0)
            {
                for (int i = 0; i < mapListToPlay.Count; i++)
                {
                    if (i < slotList.Count)
                    {
                        slotList[i].Setup(mapListToPlay[i]);
                        slotList[i].SafeSetActive(true);
                    }
                    else
                    {
                        var newSlot = Instantiate(mapSlotTemplate);
                        newSlot.transform.SetParent(mapSlotParent);
                        newSlot.transform.localScale = Vector3.one;
                        newSlot.SafeSetActive(true);
                        newSlot.Setup(mapListToPlay[i]);
                        slotList.Add(newSlot);
                    }
                }
            }

        }

        if (mapSlotParent != null)
        {
            int activeCount = 0;
            foreach (var i in slotList)
                if (i.SafeIsActive())
                    ++activeCount;

            if (activeCount > 1)
            {
                //시작 위치 초기화
                var initialPos = mapSlotParent.GetComponent<RectTransform>().anchoredPosition;
                initialPos.x = 0;
                mapSlotParent.GetComponent<RectTransform>().anchoredPosition = initialPos;

                //300은 각 cell width size
                var targetPos = new Vector3(-300 * (activeCount - 1), mapSlotParent.transform.localPosition.y, 0f);
                var duration = 0.5f * (activeCount - 1);
                mapSlotParent.DOKill();
                mapSlotParent.DOLocalMove(targetPos, duration, false).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            }
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == cancelBtn)
        {
            if (NetworkManager_Client.Instance != null)
            {
                if (NetworkManager_Client.Instance.Quantum_IsInRoom)
                {
                    UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                    NetworkManager_Client.Instance.LeaveRoom();
                }
            }
        }
    }

    private IEnumerator updateRoomStatusText;
    private IEnumerator UpdateRoomStatusText()
    {
        roomStatusTxt.SafeSetText(string.Empty);
        while (true)
        {
            //룸에 입장할때까지 기다려주자...!
            if (NetworkManager_Client.Instance.Quantum_IsInRoom)
                break;

            yield return null;
        }

        while (true)
        {
            if (NetworkManager_Client.QuantumClient == null || NetworkManager_Client.QuantumClient.CurrentRoom == null)
                break;

            roomStatusTxt.SafeLocalizeText("OUTGAME_FINDING_PLAYERS", NetworkManager_Client.QuantumClient.CurrentRoom.PlayerCount, NetworkManager_Client.QuantumClient.CurrentRoom.MaxPlayers);

            yield return null;
        }

        roomStatusTxt.SafeSetText(string.Empty);
    }

    private IEnumerator updateTimerText;
    private IEnumerator UpdateTimerText()
    {
        var timer_seconds = 0;
        while (true)
        {
            timerTxt.SafeSetText(StringManager.GetTimeString_TYPE_2(timer_seconds));
            yield return new WaitForSecondsRealtime(1f);
            ++timer_seconds;
        }
    }

    private IEnumerator updateTipText;
    private IEnumerator UpdateTipText()
    {
        tipTxt.SafeLocalizeText(StringManager.GetInGameReadyTip_Random());

        var refreshTimer = 0;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            ++refreshTimer;

            if (refreshTimer > 5)
            {
                tipTxt.SafeLocalizeText(StringManager.GetInGameReadyTip_Random());
                refreshTimer = 0;
            }
        }
    }

    public void ShowOrHideCancelButton(bool isShow)
    {
        cancelBtn.SafeSetActive(isShow);
    }

    public void MakeFullCount() //강제로 StatusText FullCount로 갱신
    {
        if (NetworkManager_Client.QuantumClient == null || NetworkManager_Client.QuantumClient.CurrentRoom == null)
            return;

        UtilityCoroutine.StopCoroutine(ref updateRoomStatusText, this);
        roomStatusTxt.SafeLocalizeText("OUTGAME_FINDING_PLAYERS", NetworkManager_Client.QuantumClient.CurrentRoom.MaxPlayers, NetworkManager_Client.QuantumClient.CurrentRoom.MaxPlayers);

        UtilityCoroutine.StopCoroutine(ref updateTimerText, this);
        timerTxt.SafeSetText(string.Empty);
    }

    public void SetFinalSelectMap()
    {
        //최종 맵 선택 완료

        if (NetworkManager_Client.Instance != null && NetworkManager_Client.Instance.Quantum_RoomData != null
            && NetworkManager_Client.Instance.Quantum_RoomData.mapID != -1)
        {
            int mapID = NetworkManager_Client.Instance.Quantum_RoomData.mapID;

            if (ReferenceManager.Instance.MapData != null && ReferenceManager.Instance.MapData.MapInfoList != null)
            {
                var list = ReferenceManager.Instance.MapData.MapInfoList;
                var mapInfo = list.Find(x => x.MapID.Equals(mapID));
                if (mapInfo != null)
                {
                    mapSlotBase.SafeSetActive(false);

                    finalSelectedSlotBase.SafeSetActive(true);
                    finalSelectedSlot.Setup(mapInfo);
                }
            }
        }
    }
}
