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
    private float dragThreshold = 0f; // �巡�׷� �ν��� �ּ� �̵� �Ÿ�

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
        // �巡�� ������ Ȯ��
        if (!isDragging)
        {
            // �巡�װ� ���۵� �� �ּ� �̵� �Ÿ� �̻� ���������� Ȯ��
            if (Vector2.Distance(initialPosition, data.position) > dragThreshold)
                isDragging = true;
        }

        // �巡�� ���� �� �ӵ��� ���
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
