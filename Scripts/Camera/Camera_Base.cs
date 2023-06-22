using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class Camera_Base : MonoSingleton<Camera_Base>
{
    [SerializeField] public Camera mainCam = null;

    public override void Awake()
    {
        base.Awake();

        if (mainCam == null)
            mainCam = GetComponent<Camera>();

        DontDestroyOnLoad(this);
    }

    public void OnEnable()
    {
        if (UIRoot_Base.Instance.uiCam != null && mainCam != null)
        {
            var cameraData = mainCam.GetUniversalAdditionalCameraData();

            if (cameraData.cameraStack.Contains(UIRoot_Base.Instance.uiCam) == false)
                cameraData.cameraStack.Add(UIRoot_Base.Instance.uiCam);
        }
    }

    public void InitializeSettings()
    {
        TurnOffCameraAllFX();
    }
}
