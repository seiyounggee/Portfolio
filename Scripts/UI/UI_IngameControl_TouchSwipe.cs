using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UI_IngameControl_TouchSwipe : MonoBehaviour
{
    [SerializeField] GameObject controlArea = null;

    private void Awake()
    {
        controlArea.SafeSetDragStart(OnDragStartCallback);
        controlArea.SafeSetDrag(OnDragCallback);
        controlArea.SafeSetDragEnd(OnDragEndCallback);
    }

    public static bool OnDragStart = false;
    public static bool OnDrag = false;
    public static bool OnDragEnd = false;
    public static Vector3 OnDragPositionInfo = Vector3.zero;


    //PlayerMovement.cs   OnInput
    //UI_PanelIngame.cs   StartDrag_ObservedNotification  Drag_ObservedNotification  EndDrag_ObservedNotification

    private void OnDragStartCallback(GameObject go)
    {
        OnDragStart = true;
        OnDragEnd = false;
    }

    private void OnDragCallback(GameObject go, UICamera.DragInfo info )
    {
        OnDragStart = false;
        OnDrag = true;
        OnDragPositionInfo = info.position;
    }

    private void OnDragEndCallback(GameObject go)
    {
        OnDragEnd = true;
        OnDrag = false;
    }
}
