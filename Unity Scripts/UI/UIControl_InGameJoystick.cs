using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControl_InGameJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform rectTransform = null;
    private Vector3 initialPosition = Vector3.zero;

    [ReadOnly] public Vector3 inputDirection = new Vector3();

    [ReadOnly] public float x = 0f;
    [ReadOnly] public float y = 0f;
    [ReadOnly] public Vector2 normalizedInput = new Vector2();

    [ReadOnly] public bool isJoystickActive;
    [ReadOnly] public bool isBlockJoystickInput = false;

    public delegate void PointerUp(Vector2 v);
    public PointerUp pointerUpCallback = null;

    public delegate void PointerDown(Vector2 v);
    public PointerDown pointerDownCallback = null;
    private Vector3 stickInitialLocalPosition = Vector3.zero;

    public delegate void DragDel(Vector2 v);
    public DragDel dragCallback = null;
    private Vector3 stickDragLocalPosition = Vector3.zero;

    public Image joystickBase = null;

    public Image joystickCircle = null;
    public Image joystickStick = null;

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        InitializeSettings();
    }

    private void Update()
    {
        isBlockJoystickInput = false;
        joystickBase.raycastTarget = true;
    }

    public void InitializeSettings()
    {
        inputDirection = Vector3.zero;
        isJoystickActive = false;
        initialPosition = rectTransform.anchoredPosition;

        x = 0f;
        y = 0f;
        joystickStick.rectTransform.anchoredPosition = Vector3.zero;

        isBlockJoystickInput = false;
        joystickBase.raycastTarget = true;
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (joystickCircle == null || joystickStick == null)
            return;

        if (isBlockJoystickInput == true)
            return;

        //Set Joystick Position
        transform.position = data.position;
        stickInitialLocalPosition = joystickStick.transform.localPosition;
        Vector2 position = Vector2.zero;

        //To get InputDirection
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (joystickCircle.rectTransform,
        data.position,
        data.pressEventCamera,
        out position);

        position.x = (position.x / joystickCircle.rectTransform.sizeDelta.x);
        position.y = (position.y / joystickCircle.rectTransform.sizeDelta.y);

        x = position.x * 2;
        y = position.y * 2;
        normalizedInput = new Vector2(x, y).normalized;

        inputDirection = new Vector3(x, y, 0);
        inputDirection = (inputDirection.magnitude > 1) ? inputDirection.normalized : inputDirection;

        //to define the area in which joystick can move around
        joystickStick.rectTransform.anchoredPosition = new Vector3(inputDirection.x * (joystickCircle.rectTransform.sizeDelta.x / 2)
        , inputDirection.y * (joystickCircle.rectTransform.sizeDelta.y) / 2);

        isJoystickActive = true;
        pointerDownCallback?.Invoke(normalizedInput);
    }


    public void OnDrag(PointerEventData data)
    {
        if (joystickCircle == null || joystickStick == null)
            return;

        if (isBlockJoystickInput == true)
            return;

        stickDragLocalPosition = joystickStick.transform.localPosition;

        Vector2 position = Vector2.zero;
        //To get InputDirection
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (joystickCircle.rectTransform,
        data.position,
        data.pressEventCamera,
        out position);

        position.x = (position.x / joystickCircle.rectTransform.sizeDelta.x);
        position.y = (position.y / joystickCircle.rectTransform.sizeDelta.y);

        x = position.x * 2;
        y = position.y * 2;
        normalizedInput = new Vector2(x, y).normalized;

        inputDirection = new Vector3(x, y, 0);
        inputDirection = (inputDirection.magnitude > 1) ? inputDirection.normalized : inputDirection;

        //to define the area in which joystick can move around
        joystickStick.rectTransform.anchoredPosition = new Vector3(inputDirection.x * (joystickCircle.rectTransform.sizeDelta.x / 2)
        , inputDirection.y * (joystickCircle.rectTransform.sizeDelta.y) / 2);

        isJoystickActive = true;
        dragCallback?.Invoke(normalizedInput);
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (joystickCircle == null || joystickStick == null)
            return;

        if (isBlockJoystickInput == true)
            return;

        inputDirection = Vector3.zero;
        x = 0f;
        y = 0f;
        joystickStick.rectTransform.anchoredPosition = Vector3.zero;
        isJoystickActive = false;

        pointerUpCallback?.Invoke(normalizedInput);

        //Reset Joystick Position
        rectTransform.anchoredPosition = initialPosition;
    }

}
