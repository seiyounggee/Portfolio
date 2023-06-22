using PNIX.ReferenceTable;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerCar : MonoBehaviour
{
    #region ANIMATION PARAMETERS

    public const string ANIM_CAR_IDLE = "ANIM_CAR_IDLE";
    public const string ANIM_CAR_DRIVE = "ANIM_CAR_DRIVE";
    public const string ANIM_CAR_LEFT = "ANIM_CAR_LEFT";
    public const string ANIM_CAR_RIGHT = "ANIM_CAR_RIGHT";
    public const string ANIM_CAR_BOOSTER_1 = "ANIM_CAR_BOOSTER_1";
    public const string ANIM_CAR_BOOSTER_2 = "ANIM_CAR_BOOSTER_2";
    public const string ANIM_CAR_BRAKE = "ANIM_CAR_BRAKE";
    public const string ANIM_CAR_FLIP = "ANIM_CAR_FLIP";
    public const string ANIM_CAR_SPIN = "ANIM_CAR_SPIN";
    #endregion

    [SerializeField] public List<PlayerCar_Prefab> CarList = new List<PlayerCar_Prefab>(); //임시...
    [ReadOnly] public PlayerCar_Prefab currentCar = null;
    [ReadOnly] public PlayerCar_FX currentCarFx = null;


    //NetworkPlayer.prefab, OutGamePlayer.prefab, DummyPlayer.prefab 모두 사용되는 공통 스크립트에 해당

    public void SetCar(CRefCar refCar)
    {
        foreach (var i in CarList)
            Destroy(i.gameObject);
        CarList.Clear();
        if (currentCarFx != null)
            Destroy(currentCarFx.gameObject);

        var carPrefabID = refCar.prefabID;

        var asset = AssetManager.Instance.loadedPrefabAssets.Find(x => x.prefab != null && x.prefab.name.Contains(carPrefabID));
        if (asset != null)
        {
#if UNITY_EDITOR
            GameObject asssetGo = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/DownloadAsset/Car/" + asset.prefab.name + ".prefab", typeof(GameObject));
            var go = Instantiate(asssetGo, this.transform);
#else
            var go = Instantiate(asset.prefab, this.transform);
#endif
            go.transform.localScale = Vector3.one;
            var script = go.GetComponent<PlayerCar_Prefab>();
            CarList.Add(script);
            currentCar = script;

            if (go != null)
            {
                currentCarFx = go.GetComponent<PlayerCar_FX>();
            }
        }


        if (currentCar != null)
        {
            currentCar.dataInfo = refCar;
            currentCar.SetMaterial();

            currentCar.go.gameObject.SafeSetActive(true);

            DeactivateAllCarFX();
        }

    }

    public void DeactivateAllCarFX()
    {
        if (currentCarFx == null)
        {
            Debug.Log("<color=red>Error...! Current CarFX Does not Exist?</color>");
            return;
        }

        DeactivateAllBoosterFX();
        DeactivateWheelFX();

        currentCarFx.FX_Shield.SafeSetActive(false);
        DeactivateStunFX();
        DeactivateChargeZoneFX();
        DeactivateMudFX();
    }

    public void ActivateBoosterFx(PlayerMovement.CarBoosterType lv)
    {
        if (currentCar == null)
        {
            Debug.Log("<color=red>Error...! Current Car Does not Exist?</color>");
            return;
        }

        if (currentCarFx == null)
        {
            Debug.Log("<color=red>Error...! Current CarFX Does not Exist?</color>");
            return;
        }

        DeactivateAllBoosterFX();

        switch (lv)
        {
            case PlayerMovement.CarBoosterType.None:
                break;

            case PlayerMovement.CarBoosterType.CarBooster_Starting:
                {
                    foreach (var i in currentCarFx.FX_Booster_Array3)
                    {
                        if (i != null)
                            i.SafeSetActive(true);
                    }
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelOne:
                {
                    foreach (var i in currentCarFx.FX_Booster_Array1)
                    {
                        if (i != null)
                            i.SafeSetActive(true);
                    }
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelTwo:
                {
                    foreach (var i in currentCarFx.FX_Booster_Array2)
                    {
                        if (i != null)
                            i.SafeSetActive(true);
                    }
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelThree:
                {
                    foreach (var i in currentCarFx.FX_Booster_Array3)
                    {
                        if (i != null)
                            i.SafeSetActive(true);
                    }
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelFour_Timing:
                {
                    foreach (var i in currentCarFx.FX_Booster_Array4)
                    {
                        if (i != null)
                            i.SafeSetActive(true);
                    }
                }
                break;
        }

        ActivateWheelFX(lv);
    }

    public void ActivateBoosterFx(PlayerMovement.ChargePadBoosterLevel lv)
    {
        if (currentCar == null)
        {
            Debug.Log("<color=red>Error...! Current Car Does not Exist?</color>");
            return;
        }

        DeactivateAllBoosterFX();

        switch (lv)
        {
            case PlayerMovement.ChargePadBoosterLevel.None:
                break;
            case PlayerMovement.ChargePadBoosterLevel.ChargePadBooster_One:
            case PlayerMovement.ChargePadBoosterLevel.ChargePadBooster_Two:
            case PlayerMovement.ChargePadBoosterLevel.ChargePadBooster_Three:
                {
                    foreach (var i in currentCarFx.FX_Booster_Array1)
                    {
                        if (i != null)
                            i.SafeSetActive(true);
                    }
                }
                break;
        }
    }

    public void DeactivateAllBoosterFX()
    {
        if (currentCar == null)
            return;

        foreach (var i in currentCarFx.FX_Booster_Array1)
        {
            if (i != null)
                i.SafeSetActive(false);
        }

        foreach (var i in currentCarFx.FX_Booster_Array2)
        {
            if (i != null)
                i.SafeSetActive(false);
        }

        foreach (var i in currentCarFx.FX_Booster_Array3)
        {
            if (i != null)
                i.SafeSetActive(false);
        }

        foreach (var i in currentCarFx.FX_Booster_Array4)
        {
            if (i != null)
                i.SafeSetActive(false);
        }


        DeactivateWheelFX();
    }

    public void ActivateWheelFX(PlayerMovement.CarBoosterType lv)
    {
        if (currentCar == null)
            return;

        foreach (var i in currentCarFx.FX_WheelBase_Array)
            i.SafeSetActive(true);

        switch (lv)
        {
            case PlayerMovement.CarBoosterType.None:
                break;
            case PlayerMovement.CarBoosterType.CarBooster_Starting:
                {
                    currentCarFx.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine3");
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelOne:
                {
                    currentCarFx.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine1");
                    currentCarFx.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine1");
                    currentCarFx.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine1");
                    currentCarFx.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine1");
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelTwo:
                {
                    currentCarFx.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine2");
                    currentCarFx.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine2");
                    currentCarFx.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine2");
                    currentCarFx.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine2");
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelThree:
                {
                    currentCarFx.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine3");
                }
                break;
            case PlayerMovement.CarBoosterType.CarBooster_LevelFour_Timing:
                {
                    currentCarFx.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine3");
                    currentCarFx.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine3");
                }
                break;
        }
    }

    public void DeactivateWheelFX()
    {
        if (currentCar == null)
            return;

        foreach(var i in currentCarFx.FX_WheelBase_Array)
            i.SafeSetActive(false);
    }

    public void ActivateShieldFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Shield == null)
            return;

        currentCarFx.FX_Shield.SafeSetActive(true);
        var ani = currentCarFx.FX_Shield.GetComponent<Animation>();
        ani.PlayCallback("Shield_On",
            () =>
            {
                var trans = currentCarFx.FX_Shield.gameObject.GetComponentsInChildren<Transform>();
                if (trans.Length > 0)
                {
                    foreach (var i in trans)
                    {
                        if (i.gameObject.name.Contains("Shield") && i.gameObject != currentCarFx.FX_Shield)
                        {
                            i.gameObject.SafeSetActive(true);
                            break;
                        }
                    }
                }
            });
    }

    public void DeactivateShieldFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Shield == null)
            return;

        currentCarFx.FX_Shield.SafeSetActive(true);
        var ani = currentCarFx.FX_Shield.GetComponent<Animation>();
        ani.PlayCallback("Shield_Off",
        () =>
        {
            var trans = currentCarFx.FX_Shield.gameObject.GetComponentsInChildren<Transform>();
            if (trans.Length > 0)
            {
                foreach (var i in trans)
                {
                    if (i.gameObject.name.Contains("Shield") && i.gameObject != currentCarFx.FX_Shield)
                    {
                        i.gameObject.SafeSetActive(false);
                        break;
                    }
                }
            }
        });
    }

    public void ActivateStunFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Stun == null)
            return;

        currentCarFx.FX_Stun.SafeSetActive(true);
    }

    public void DeactivateStunFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Stun == null)
            return;

        currentCarFx.FX_Stun.SafeSetActive(false);
    }

    SetActiveFalseAfterTime setActiveFalseAfterTime_ChargeZoneFX = null;
    public void ActivateChargeZoneFX() //OnTriggerStay에 의해 계속 호출됨!
    {
        if (currentCarFx == null || currentCarFx.FX_ChargeZone_Green == null)
            return;

        if (currentCarFx.FX_ChargeZone_Green.activeSelf == false)
        {
            currentCarFx.FX_ChargeZone_Green.SafeSetActive(true);
            setActiveFalseAfterTime_ChargeZoneFX = currentCarFx.FX_ChargeZone_Green.GetComponent<SetActiveFalseAfterTime>();
        }
        else
        {
            if (setActiveFalseAfterTime_ChargeZoneFX != null)
                setActiveFalseAfterTime_ChargeZoneFX.ResetTimer();
            else
                setActiveFalseAfterTime_ChargeZoneFX = currentCarFx.FX_ChargeZone_Green.GetComponent<SetActiveFalseAfterTime>();
        }

    }

    public void DeactivateChargeZoneFX()
    {
        if (currentCarFx == null || currentCarFx.FX_ChargeZone_Green == null)
            return;

        if (currentCarFx.FX_ChargeZone_Green.activeSelf == true)
            currentCarFx.FX_ChargeZone_Green.SafeSetActive(false);
    }

    public void ActivateMudFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Mud == null)
            return;

        if (InGameManager.Instance.isGameEnded)
            return;

        if (currentCarFx.FX_Mud.activeSelf == false)
        {
            currentCarFx.FX_Mud.SafeSetActive(true);
        }

        var ps = currentCarFx.FX_Mud.GetComponentsInChildren<ParticleSystem>();
        if (ps != null && ps.Length > 0)
        {
            foreach (var i in ps)
            {
                var em = i.emission;
                em.rateOverTime = 50;
                em.rateOverDistance = 0;
            }
        }
    }

    public void DeactivateMudFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Mud == null)
            return;

        var ps = currentCarFx.FX_Mud.GetComponentsInChildren<ParticleSystem>();
        if (ps != null && ps.Length > 0)
        {
            foreach (var i in ps)
            {
                var em = i.emission;
                em.rateOverTime = 0;
                em.rateOverDistance = 0;
            }
        }
    }

    public void ActivateDraftFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Draft == null)
            return;

        currentCarFx.FX_Draft.SafeSetActive(true);
    }

    public void DeactivateDraftFX()
    {
        if (currentCarFx == null || currentCarFx.FX_Draft == null)
            return;

        currentCarFx.FX_Draft.SafeSetActive(false);
    }

    public string GetString_ANIM_IDLE()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_IDLE;
        }

        return animName;
    }

    public string GetString_ANIM_DRIVE()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_DRIVE;
        }

        return animName;
    }

    public string GetString_ANIM_LEFT()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_LEFT;
        }

        return animName;
    }

    public string GetString_ANIM_RIGHT()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_RIGHT;
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_1()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_BOOSTER_1;
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_2()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_BOOSTER_2;
        }

        return animName;
    }

    public string GetString_ANIM_BRAKE()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_BRAKE;
        }

        return animName;
    }

    public string GetString_ANIM_FLIP()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_FLIP;
        }

        return animName;
    }

    public string GetString_ANIM_SPIN()
    {
        string animName = "";

        if (currentCar != null)
        {
            animName = ANIM_CAR_SPIN;
        }

        return animName;
    }
}