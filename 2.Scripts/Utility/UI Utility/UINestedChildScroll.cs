using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UINestedChildScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// Nested Child Scroll (스크롤 안에 스크롤)
    /// Scroll Rect 안에 또 다른 자식 Scroll Rect가 있는 경우 그 자식의 Rect안에서 Drag할때
    /// 부모의 Scroll Rect도 Drag하고 싶을때 사용
    /// </summary>

    [SerializeField] private ScrollRect parentScroll = null;
    private ScrollRect childScroll = null;

    [Range(0f, 1f)]
    public float minThrehold = 0.3f;
    [Range(0f, 1f)]
    public float maxThrehold = 0.9f;

    private bool isDragable = false;

    private void OnEnable()
    {
        SetScroll();
    }

    public void SetScroll()
    {
        if (childScroll == null)
            childScroll = this.GetComponent<ScrollRect>();

        if (this.parentScroll == null)
        {
            /* TODO
            if (LobbyUIManager.Instance.uILobby != null &&
                LobbyUIManager.Instance.uILobby.uILobbyScroll != null &&
                LobbyUIManager.Instance.uILobby.uILobbyScroll.uIScrollRect != null)
            {
                parentScroll = LobbyUIManager.Instance.uILobby.uILobbyScroll.uIScrollRect;
            }
            */
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScroll == null || childScroll == null)
            return;

        isDragable = IsDragable(eventData);
        if (isDragable)
        {
            parentScroll.OnBeginDrag(eventData);
            parentScroll.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScroll == null || childScroll == null)
            return;

        if (isDragable)
        {
            childScroll.StopMovement();
            parentScroll.OnDrag(eventData);
            parentScroll.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScroll == null || childScroll == null)
            return;

        if (isDragable)
        {
            parentScroll.OnEndDrag(eventData);
            parentScroll.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }

    private bool IsDragable(PointerEventData eventData)
    {
        if (parentScroll == null || childScroll == null)
            return false;

        if (!childScroll.vertical && !childScroll.horizontal)
        {
            return true;
        }
        else if (childScroll.horizontal)
        {
            return
                (Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x)) || // Main direction
                (eventData.delta.x < 0f && childScroll.horizontalNormalizedPosition < minThrehold) || // Left
                (eventData.delta.x > 0f && childScroll.horizontalNormalizedPosition > maxThrehold); // Right
        }
        else // if (childScroll.vertical)
        {
            return
                (Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y)) || // Main direction
                (eventData.delta.y > 0f && childScroll.verticalNormalizedPosition < minThrehold) || // Bottom
                (eventData.delta.y < 0f && childScroll.verticalNormalizedPosition > maxThrehold); // Top
        }
    }
}
