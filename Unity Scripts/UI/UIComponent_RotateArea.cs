using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIComponent_RotateArea : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Vector2 prevPointerPosition;
    private Vector2 initialPointerPosition;
    private Vector2 currentSwipe;
    private float dragLength = 0f;

    public delegate void PointerUp(SwipeDirection sd, float strength, float length);
    public PointerUp pointerUpCallback = null;

    public delegate void PointerDown(SwipeDirection sd, float strength);
    public PointerDown pointerDownCallback = null;

    public delegate void DragDel(SwipeDirection sd, float strength);
    public DragDel dragCallback = null;

    public enum SwipeDirection { None , Left, Right}

    public void OnPointerDown(PointerEventData eventData)
    {
        prevPointerPosition = eventData.position;
        initialPointerPosition = eventData.position;
        dragLength = 0f;

        pointerDownCallback?.Invoke(SwipeDirection.None, 0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentSwipe = eventData.position - prevPointerPosition;
        prevPointerPosition = eventData.position;
        dragLength += currentSwipe.magnitude;

        if (currentSwipe.x < 0)
        {
            dragCallback?.Invoke(SwipeDirection.Left, 1f);
        }
        else
        {
            dragCallback?.Invoke(SwipeDirection.Right, 1f);
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        var strength = Vector2.SqrMagnitude(eventData.position - initialPointerPosition);

        dragCallback?.Invoke(SwipeDirection.None, strength);
        pointerUpCallback?.Invoke(SwipeDirection.None, strength, dragLength);
    }
}
