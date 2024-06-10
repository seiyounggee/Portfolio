using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControl_InGameRotateArea : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private bool isDragging = false;
    private Vector2 initialPosition;
    private Vector2 lastPosition;
    private float dragThreshold = 0f; // 드래그로 인식할 최소 이동 거리

    public Action<Vector2> areaDragCallback = null;
    public Action dragEndCallback = null;

    public void OnPointerDown(PointerEventData data)
    {
        initialPosition = data.position;
        lastPosition = data.position;
        isDragging = false;
    }

    public void OnDrag(PointerEventData data)
    {
        // 드래그 중인지 확인
        if (!isDragging)
        {
            // 드래그가 시작된 후 최소 이동 거리 이상 움직였는지 확인
            if (Vector2.Distance(initialPosition, data.position) > dragThreshold)
                isDragging = true;
        }

        // 드래그 중일 때 속도를 계산
        if (isDragging)
        {
            Vector2 currentPosition = data.position;
            float distance = Vector2.Distance(lastPosition, currentPosition);
            float deltaTime = Time.deltaTime;
            float dragSpeed = distance / deltaTime;

            areaDragCallback?.Invoke((currentPosition - lastPosition).normalized);

            lastPosition = currentPosition;
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        isDragging = false;

        dragEndCallback?.Invoke();
    }
}
