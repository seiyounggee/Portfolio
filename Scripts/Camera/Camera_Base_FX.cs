using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Camera_Base
{
    [Header("Camera FX")]
    [SerializeField] public GameObject FX_Line_0 = null;
    [SerializeField] public GameObject FX_Line_1 = null;
    [SerializeField] public GameObject FX_Line_2 = null;
    [SerializeField] public GameObject FX_Line_3 = null;
    [SerializeField] public GameObject FX_Water = null;



    public void TurnOffCameraBoosterFX()
    {
        FX_Line_0.SafeSetActive(false);
        FX_Line_1.SafeSetActive(false);
        FX_Line_2.SafeSetActive(false);
        FX_Line_3.SafeSetActive(false);
    }

    public void TurnOffWaterFX()
    {
        FX_Water.SafeSetActive(false);
    }


    public void TurnOffCameraAllFX()
    {
        TurnOffCameraBoosterFX();
        TurnOffWaterFX();
    }
}
