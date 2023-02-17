using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(CanvasGroup))]
public class UISnapScroll : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Snap Scroll Settings")]
    private int startingIndex = 0;
    [SerializeField] public bool wrapAround = false;
    [SerializeField] public float lerpTimeMilliSeconds = 200f;
    [SerializeField] public float triggerPercent = 5f;
    [Range(0f, 10f)] public float triggerAcceleration = 1f;

    [Header("UI Components")]
    public ScrollRect scrollRect;
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform content;
    public CanvasGroup canvasGroup;

    [HideInInspector] public int totalCellCount;
    private Vector2 slotSize;
    private Vector2 spacing;
    private int slotIndex;
    private bool indexChangeTriggered = false;
    private bool isLerping = false;
    private DateTime lerpStartedAt;
    private Vector2 releasedPosition;
    private Vector2 targetPosition;

    public Action<int> IndexChangedCallback = null;
    public Action OnDragSnapScroll = null;

    protected override void Awake()
    {
        base.Awake();

        if (scrollRect == null)        
            scrollRect = this.GetComponent<ScrollRect>();

        scrollRect.inertia = false;

        if (canvasGroup == null)
            canvasGroup = this.GetComponent<CanvasGroup>();

        slotIndex = startingIndex;

        if (gridLayoutGroup != null)
        {
            slotSize = gridLayoutGroup.cellSize;
            spacing = gridLayoutGroup.spacing;
        }

        this.gameObject.SetActive(false);
    }

    public void Activate()
    {
        /*
        //로비로 들어올때 마지막으로 플레이한 맵 보여주기 (or 처음 플레이시 가장 첫번째 맵)
        if (DataManager.Instance.gameData.userAccountData.userBasicData.lastPlayedMapSlotIndex > 0)
        {
            int index = DataManager.Instance.gameData.userAccountData.userBasicData.lastPlayedMapSlotIndex;
            if (LobbyUIManager.Instance.uILobby.uILobbyScroll.CheckIfVaildMapSlotIndex(index) == false)
                index = LobbyUIManager.Instance.uILobby.uILobbyScroll.GetInitialMapSlotIndex();

            slotIndex = index;
            startingIndex = index;
        }
        else
        {
            var lobbySlotList = LobbyUIManager.Instance.lobbySlotList;
            if (lobbySlotList != null)
            {
                for (int i = 0; i < lobbySlotList.Count; i++)
                {
                    var slot = lobbySlotList[i].gameObject.GetComponent<UIBattleSlot>();
                    if (slot != null)
                    {
                        slotIndex = i;
                        startingIndex = i;
                        break;
                    }
                }
            }
        }
        */

        totalCellCount = content.childCount;
        SetContentSize(totalCellCount);
        MoveToIndex(startingIndex);
    }

    void LateUpdate()
    {
        if (isLerping)
        {
            // Lerp
            float t = (float)((DateTime.Now - lerpStartedAt).TotalMilliseconds / lerpTimeMilliSeconds);
            float newX = Mathf.Lerp(releasedPosition.x, targetPosition.x, t);
            content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);

            // Stop Lerp
            if (Mathf.Abs(content.anchoredPosition.x - targetPosition.x) < 0.001)
            {
                isLerping = false;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 셀이 추가되면 ContentSize을 재설정 해줘야함
        totalCellCount = content.childCount;
        SetContentSize(totalCellCount);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dx = eventData.delta.x;
        float dt = Time.deltaTime * 1000f;
        float acceleration = Mathf.Abs(dx / dt);
        if (acceleration > triggerAcceleration && acceleration != Mathf.Infinity)
        {
            indexChangeTriggered = true;

            OnDragSnapScroll?.Invoke();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IndexShouldChangeFromDrag(eventData))
        {
            int direction = (eventData.pressPosition.x - eventData.position.x) > 0f ? 1 : -1;
            SnapToIndex(slotIndex + direction * CalculateScrollingAmount(eventData));
        }
        else
        {
            StartLerping();
        }
    }

    public int CalculateScrollingAmount(PointerEventData data)
    {
        var offset = scrollRect.content.anchoredPosition.x + slotIndex * slotSize.x;
        var normalizedOffset = Mathf.Abs(offset / slotSize.x);
        var skipping = (int)Mathf.Floor(normalizedOffset);
        if (skipping == 0)
            return 1;
        if ((normalizedOffset - skipping) * 100f > triggerPercent)
        {
            return skipping + 1;
        }
        else
        {
            return skipping;
        }
    }

    public void SnapToIndex(int newCellIndex) // Scroll 하면서 가기
    {
        /*
        if (newCellIndex >= LobbyUIManager.Instance.lobbySlotList.Count)
            return;
        */

        int maxIndex = CalculateMaxIndex();
        if (wrapAround && maxIndex > 0)
        {
            slotIndex = newCellIndex;
        }
        else
        {
            newCellIndex = Mathf.Clamp(newCellIndex, 0, maxIndex);
            slotIndex = newCellIndex;
        }

        IndexChangedCallback?.Invoke(slotIndex);

        StartLerping();
    }

    public void SnapToNext()
    {
        SnapToIndex(slotIndex + 1);
    }

    public void SnapToPrev()
    {
        SnapToIndex(slotIndex - 1);
    }

    public void MoveToIndex(int newCellIndex) // Scroll 없이 바로가기
    {
        if (newCellIndex >= totalCellCount)
            return;

        int maxIndex = CalculateMaxIndex();
        if (newCellIndex >= 0 && newCellIndex <= maxIndex)
        {
            slotIndex = newCellIndex;
        }

        content.anchoredPosition = CalculateTargetPoisition(slotIndex);

        IndexChangedCallback?.Invoke(slotIndex);
    }

    Vector2 CalculateTargetPoisition(int index)
    {
        return new Vector2(-slotSize.x * index, content.anchoredPosition.y);
    }

    void StartLerping()
    {
        releasedPosition = content.anchoredPosition;
        targetPosition = CalculateTargetPoisition(slotIndex);
        lerpStartedAt = DateTime.Now;
        canvasGroup.blocksRaycasts = false;
        isLerping = true;
    }

    int CalculateMaxIndex()
    {
        int cellPerFrame = Mathf.FloorToInt(scrollRect.GetComponent<RectTransform>().rect.size.x / slotSize.x);
        return totalCellCount - cellPerFrame;
    }

    bool IndexShouldChangeFromDrag(PointerEventData data)
    {
        // acceleration was above threshold
        if (indexChangeTriggered) //인덱스가 바뀔경우
        {
            indexChangeTriggered = false;
            return true;
        }
        // dragged beyond trigger threshold
        var offset = scrollRect.content.anchoredPosition.x + slotIndex * slotSize.x;
        var normalizedOffset = Mathf.Abs(offset / slotSize.x);
        return normalizedOffset * 100f > triggerPercent;
    }


    void SetContentSize(int elementCount)
    {
        content.sizeDelta = new Vector2(slotSize.x * elementCount, content.rect.height);
    }
}
