using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UI_IngameControl_TouchSwipe : MonoBehaviour
{
    public Action onDragStartEvent;
    public Action<UICamera.DragInfo> onCustomDragEvent;
    public Action onDragEndEvent;

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


    //UI_PanelIngmae.cs   PhotonCallback_OnInput 에서 network input을 넣어주고 있음...!

    private void OnDragStartCallback(GameObject go)
    {
       //onDragStartEvent?.Invoke();

        OnDragStart = true;
        OnDragEnd = false;
    }

    private void OnDragCallback(GameObject go, UICamera.DragInfo info )
    {
       //onCustomDragEvent?.Invoke(info);

        OnDragStart = false;
        OnDrag = true;
        OnDragPositionInfo = info.position;
    }

    private void OnDragEndCallback(GameObject go)
    {
        //onDragEndEvent?.Invoke();

        OnDragEnd = true;
        OnDrag = false;
    }
}
