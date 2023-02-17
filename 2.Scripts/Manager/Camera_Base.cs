using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Camera_Base : MonoSingleton<Camera_Base>
{
    [SerializeField] public Camera mainCam = null;
    [SerializeField] public UniversalAdditionalCameraData camData = null;

    [SerializeField] public GameObject FX_0 = null;
    [SerializeField] public GameObject FX_1 = null;
    [SerializeField] public GameObject FX_2 = null;

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
        TurnOffCameraFX();
    }

    public void TurnOffCameraFX()
    {
        FX_0.SafeSetActive(false);
        FX_1.SafeSetActive(false);
        FX_2.SafeSetActive(false);
    }
}
