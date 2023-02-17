using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRoot_Base : MonoSingleton<UIRoot_Base>
{

    [SerializeField] public Camera uiCam = null;

    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);
    }
}
