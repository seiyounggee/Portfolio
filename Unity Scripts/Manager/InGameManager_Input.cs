using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Deterministic;
using Quantum;

public partial class InGameManager
{
    public Vector2 inputVector;
    public bool input_jump;
    public bool input_attack;
    public bool input_skill;

    public Action btnInput_Jump = null;
    public Action btnInput_Attack = null;
    public Action btnInput_Skill = null;

    private DispatcherSubscription subscription;

    private void Update()
    {
#if UNITY_EDITOR || KEYBOARD_INPUT
        ActivateKeyboardInput();
#endif
    }

    private void OnEnable()
    {
        subscription = QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    private void OnDisable()
    {
        QuantumCallback.Unsubscribe(subscription);
    }

    public unsafe void PollInput(CallbackPollInput callback)
    {
        Quantum.Input input = new Quantum.Input();

        var x = inputVector.x;
        var y = inputVector.y;

        if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f))
        {
            input.horizontal = FP._0;
            input.vertical = FP._0;
        }
        else
        {
            input.horizontal = x.ToFP();
            input.vertical = y.ToFP();
        }

        if (input_jump)
        {
            input.Jump = true;
            input_jump = false;
        }

        if (input_attack)
        {
            input.Attack = true;
            input_attack = false;
        }

        if (input_skill)
        {
            input.Skill = true;
            input_skill = false;
        }

        if (CameraManager.Instance.MainCamera)
        {
            input.cameraDirection = CameraManager.Instance.MainCamera.transform.forward.ToFPVector3();
        }

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }


    public void SetInput()
    {
        var ui = PrefabManager.Instance.UI_InGame;
        ui.joystick.pointerDownCallback = JoystickInput_PointerDown;
        ui.joystick.dragCallback = JoystickInput_Drag;
        ui.joystick.pointerUpCallback = JoystickInput_PointerUp;

        ui.rotateArea.areaDragCallback = RotateArea_Drag;
        ui.rotateArea.dragEndCallback = RotateArea_EndDrag;

        ui.btn_jump.Button.SafeSetButton(OnClickBtn_Jump);
        ui.btn_attack.Button.SafeSetButton(OnClickBtn_Attack);
        ui.btn_skill.Button.SafeSetButton(OnClickBtn_Skill);

        inputVector = Vector3.zero; //√ ±‚»≠
    }

    private void JoystickInput_PointerDown(Vector2 v)
    {
        inputVector = v;
    }

    private void JoystickInput_Drag(Vector2 v)
    {
        inputVector = v;
    }

    private void JoystickInput_PointerUp(Vector2 v)
    {
        inputVector = Vector2.zero;
    }

    private void RotateArea_Drag(Vector2 v)
    {
        CameraManager.Instance.RotateCamera_Ingame(v);
    }

    private void RotateArea_EndDrag()
    {
        CameraManager.Instance.EndRotateCamera_Ingame();
    }

    private void OnClickBtn_Jump()
    {
        btnInput_Jump?.Invoke();

        input_jump = true;
    }

    private void OnClickBtn_Attack()
    {
        btnInput_Attack?.Invoke();

        input_attack = true;
    }

    private void OnClickBtn_Skill()
    {
        btnInput_Skill?.Invoke();

        input_skill = true;
    }


    private void ActivateKeyboardInput()
    {
#if UNITY_EDITOR || KEYBOARD_INPUT

        //for keyboard

        float h = UnityEngine.Input.GetAxisRaw("Horizontal");
        float v = UnityEngine.Input.GetAxisRaw("Vertical");
        Vector2 input = Vector2.zero;
        input = new Vector2(h, v).normalized;

        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S)
            || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W)
            || UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D)
            || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
        {
            JoystickInput_PointerDown(input);
        }

        if (UnityEngine.Input.GetKey(KeyCode.DownArrow) || UnityEngine.Input.GetKey(KeyCode.S)
            || UnityEngine.Input.GetKey(KeyCode.UpArrow) || UnityEngine.Input.GetKey(KeyCode.W)
            || UnityEngine.Input.GetKey(KeyCode.RightArrow) || UnityEngine.Input.GetKey(KeyCode.D)
            || UnityEngine.Input.GetKey(KeyCode.LeftArrow) || UnityEngine.Input.GetKey(KeyCode.A))
        {
            JoystickInput_Drag(input);
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.DownArrow) || UnityEngine.Input.GetKeyUp(KeyCode.S)
            || UnityEngine.Input.GetKeyUp(KeyCode.UpArrow) || UnityEngine.Input.GetKeyUp(KeyCode.W)
            || UnityEngine.Input.GetKeyUp(KeyCode.RightArrow) || UnityEngine.Input.GetKeyUp(KeyCode.D)
            || UnityEngine.Input.GetKeyUp(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyUp(KeyCode.A))
        {
            JoystickInput_PointerUp(input);
        }

        if (!UnityEngine.Input.GetKey(KeyCode.DownArrow) && !UnityEngine.Input.GetKey(KeyCode.S)
            && !UnityEngine.Input.GetKey(KeyCode.UpArrow) && !UnityEngine.Input.GetKey(KeyCode.W)
            && !UnityEngine.Input.GetKey(KeyCode.RightArrow) && !UnityEngine.Input.GetKey(KeyCode.D)
            && !UnityEngine.Input.GetKey(KeyCode.LeftArrow) && !UnityEngine.Input.GetKey(KeyCode.A))
        {
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.J))
        {
            OnClickBtn_Jump();
        }
#endif
    }
}
