using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCar : MonoBehaviour
{
    #region ANIMATION PARAMETERS
    public const string ANIM_CAR_01_IDLE = "ANIM_CAR_01_IDLE";
    public const string ANIM_CAR_01_DRIVE = "ANIM_CAR_01_DRIVE";
    public const string ANIM_CAR_01_LEFT = "ANIM_CAR_01_LEFT";
    public const string ANIM_CAR_01_RIGHT = "ANIM_CAR_01_RIGHT";
    public const string ANIM_CAR_01_BOOSTER_1 = "ANIM_CAR_01_BOOSTER_1";
    public const string ANIM_CAR_01_BOOSTER_2 = "ANIM_CAR_01_BOOSTER_2";
    public const string ANIM_CAR_01_BRAKE = "ANIM_CAR_01_BRAKE";
    public const string ANIM_CAR_01_FLIP = "ANIM_CAR_01_FLIP";
    public const string ANIM_CAR_01_SPIN = "ANIM_CAR_01_SPIN";

    public const string ANIM_CAR_02_IDLE = "ANIM_CAR_02_IDLE";
    public const string ANIM_CAR_02_DRIVE = "ANIM_CAR_02_DRIVE";
    public const string ANIM_CAR_02_LEFT = "ANIM_CAR_02_LEFT";
    public const string ANIM_CAR_02_RIGHT = "ANIM_CAR_02_RIGHT";
    public const string ANIM_CAR_02_BOOSTER_1 = "ANIM_CAR_02_BOOSTER_1";
    public const string ANIM_CAR_02_BOOSTER_2 = "ANIM_CAR_02_BOOSTER_2";
    public const string ANIM_CAR_02_BRAKE = "ANIM_CAR_02_BRAKE";
    public const string ANIM_CAR_02_FLIP = "ANIM_CAR_02_FLIP";
    public const string ANIM_CAR_02_SPIN = "ANIM_CAR_02_SPIN";

    public const string ANIM_CAR_03_IDLE = "ANIM_CAR_01_IDLE";
    public const string ANIM_CAR_03_DRIVE = "ANIM_CAR_01_DRIVE";
    public const string ANIM_CAR_03_LEFT = "ANIM_CAR_01_LEFT";
    public const string ANIM_CAR_03_RIGHT = "ANIM_CAR_01_RIGHT";
    public const string ANIM_CAR_03_BOOSTER_1 = "ANIM_CAR_01_BOOSTER_1";
    public const string ANIM_CAR_03_BOOSTER_2 = "ANIM_CAR_01_BOOSTER_2";
    public const string ANIM_CAR_03_BRAKE = "ANIM_CAR_01_BRAKE";
    public const string ANIM_CAR_03_FLIP = "ANIM_CAR_01_FLIP";
    public const string ANIM_CAR_03_SPIN = "ANIM_CAR_01_SPIN";

    public const string ANIM_CAR_04_IDLE = "ANIM_CAR_02_IDLE";
    public const string ANIM_CAR_04_DRIVE = "ANIM_CAR_02_DRIVE";
    public const string ANIM_CAR_04_LEFT = "ANIM_CAR_02_LEFT";
    public const string ANIM_CAR_04_RIGHT = "ANIM_CAR_02_RIGHT";
    public const string ANIM_CAR_04_BOOSTER_1 = "ANIM_CAR_02_BOOSTER_1";
    public const string ANIM_CAR_04_BOOSTER_2 = "ANIM_CAR_02_BOOSTER_2";
    public const string ANIM_CAR_04_BRAKE = "ANIM_CAR_02_BRAKE";
    public const string ANIM_CAR_04_FLIP = "ANIM_CAR_02_FLIP";
    public const string ANIM_CAR_04_SPIN = "ANIM_CAR_02_SPIN";

    #endregion
    [System.Serializable]
    public class Car
    {
        public DataManager.CAR_DATA.CarID carType;
        public GameObject go = null;
        public Animator animator = null;

        public GameObject dollyRoot = null;

        public GameObject FX_Booster_1 = null;
        public GameObject FX_Booster_2 = null;
        public GameObject FX_Booster_3 = null;
        public GameObject FX_Shield = null;

        public GameObject FX_WheelBase = null;
        public Animation FX_Wheel_BackRight = null;
        public Animation FX_Wheel_FrontRight = null;
        public Animation FX_Wheel_BackLeft = null;
        public Animation FX_Wheel_FrontLeft = null;

        public GameObject FX_Stun = null;

        public GameObject FX_ChargeZone_Green = null;

        public GameObject FX_Mud = null;
    }

    [SerializeField] public List<Car> CarList = new List<Car>();
    [ReadOnly] public Car currentCar = new Car();

    public DataManager.CAR_DATA.CarID Type;

    public void SetCar(DataManager.CAR_DATA.CarID type)
    {
        var c = CarList.Find(x => x.carType.Equals(type));
        if (c != null)
        {
            Type = type;

            currentCar = c;

            foreach (var i in CarList)
            {
                if (i.go.Equals(currentCar.go))
                    i.go.SafeSetActive(true);
                else
                    i.go.SafeSetActive(false);
            }
        }
    }

    public void ActivateBoosterFx(PlayerMovement.CarBoosterLevel lv)
    {
        if (currentCar == null)
        {
            Debug.Log("<color=red>Error...! Current Car Does not Exist?</color>");
            return;
        }

        DeactivateAllBoosterFX();

        switch (lv)
        {
            case PlayerMovement.CarBoosterLevel.None:
                break;
            case PlayerMovement.CarBoosterLevel.Car_One:
                {
                    currentCar.FX_Booster_1.SafeSetActive(true);
                }
                break;
            case PlayerMovement.CarBoosterLevel.Car_Two:
                {
                    currentCar.FX_Booster_2.SafeSetActive(true);
                }
                break;
            case PlayerMovement.CarBoosterLevel.Car_Three:
                {
                    currentCar.FX_Booster_3.SafeSetActive(true);
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
            case PlayerMovement.ChargePadBoosterLevel.ChargePad_One:
            case PlayerMovement.ChargePadBoosterLevel.ChargePad_Two:
            case PlayerMovement.ChargePadBoosterLevel.ChargePad_Three:
                {
                    currentCar.FX_Booster_1.SafeSetActive(true);
                }
                break;
        }
    }

    public void DeactivateAllBoosterFX()
    {
        if (currentCar == null)
            return;

        currentCar.FX_Booster_1.SafeSetActive(false);
        currentCar.FX_Booster_2.SafeSetActive(false);
        currentCar.FX_Booster_3.SafeSetActive(false);

        DeactivateWheelFX();
    }

    public void DeactivateBoosterFX_2and3()
    {
        if (currentCar == null)
            return;

        currentCar.FX_Booster_2.SafeSetActive(false);
        currentCar.FX_Booster_3.SafeSetActive(false);

        DeactivateWheelFX();
    }

    public void ActivateWheelFX(PlayerMovement.CarBoosterLevel lv)
    {
        if (currentCar == null)
            return;

        currentCar.FX_WheelBase.SafeSetActive(true);

        switch (lv)
        {
            case PlayerMovement.CarBoosterLevel.None:
                break;
            case PlayerMovement.CarBoosterLevel.Car_One:
                {
                    currentCar.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine1");
                    currentCar.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine1");
                    currentCar.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine1");
                    currentCar.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine1");
                }
                break;
            case PlayerMovement.CarBoosterLevel.Car_Two:
                {
                    currentCar.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine2");
                    currentCar.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine2");
                    currentCar.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine2");
                    currentCar.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine2");
                }
                break;
            case PlayerMovement.CarBoosterLevel.Car_Three:
                {
                    currentCar.FX_Wheel_FrontLeft.SafePlay("Wheel_WhidLine3");
                    currentCar.FX_Wheel_FrontRight.SafePlay("Wheel_WhidLine3");
                    currentCar.FX_Wheel_BackLeft.SafePlay("Wheel_WhidLine3");
                    currentCar.FX_Wheel_BackRight.SafePlay("Wheel_WhidLine3");
                }
                break;
        }
    }

    public void DeactivateWheelFX()
    {
        if (currentCar == null)
            return;

        currentCar.FX_WheelBase.SafeSetActive(false);
    }

    public void ActivateShieldFX()
    {
        if (currentCar == null || currentCar.FX_Shield == null)
            return;

        currentCar.FX_Shield.SafeSetActive(true);
        var ani = currentCar.FX_Shield.GetComponent<Animation>();
        ani.PlayCallback("Shield_On",
            ()=> 
            {
                var trans = currentCar.FX_Shield.gameObject.GetComponentsInChildren<Transform>();
                if (trans.Length > 0)
                {
                    foreach (var i in trans)
                    {
                        if (i.gameObject.name.Contains("Shield") && i.gameObject != currentCar.FX_Shield)
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
        if (currentCar == null || currentCar.FX_Shield == null)
            return;

        currentCar.FX_Shield.SafeSetActive(true);
        var ani = currentCar.FX_Shield.GetComponent<Animation>();
        ani.PlayCallback("Shield_Off",
        () =>
        {
            var trans = currentCar.FX_Shield.gameObject.GetComponentsInChildren<Transform>();
            if (trans.Length > 0)
            {
                foreach (var i in trans)
                {
                    if (i.gameObject.name.Contains("Shield") && i.gameObject != currentCar.FX_Shield)
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
        if (currentCar == null || currentCar.FX_Stun == null)
            return;

        currentCar.FX_Stun.SafeSetActive(true);
    }

    public void DeactivateStunFX()
    {
        if (currentCar == null || currentCar.FX_Stun == null)
            return;

        currentCar.FX_Stun.SafeSetActive(false);
    }

    SetActiveFalseAfterTime setActiveFalseAfterTime_ChargeZoneFX = null;
    public void ActivateChargeZoneFX() //OnTriggerStay에 의해 계속 호출됨!
    {
        if (currentCar == null || currentCar.FX_ChargeZone_Green == null)
            return;

        if (currentCar.FX_ChargeZone_Green.activeSelf == false)
        {
            currentCar.FX_ChargeZone_Green.SafeSetActive(true);
            setActiveFalseAfterTime_ChargeZoneFX = currentCar.FX_ChargeZone_Green.GetComponent<SetActiveFalseAfterTime>();
        }
        else
        {
            if (setActiveFalseAfterTime_ChargeZoneFX != null)
                setActiveFalseAfterTime_ChargeZoneFX.ResetTimer();
            else
                setActiveFalseAfterTime_ChargeZoneFX = currentCar.FX_ChargeZone_Green.GetComponent<SetActiveFalseAfterTime>();
        }

    }

    public void DeactivateChargeZoneFX()
    {
        if (currentCar == null || currentCar.FX_ChargeZone_Green == null)
            return;

        if (currentCar.FX_ChargeZone_Green.activeSelf == true)
            currentCar.FX_ChargeZone_Green.SafeSetActive(false);
    }

    public void ActivateMudFX()
    {
        if (currentCar == null || currentCar.FX_Mud == null)
            return;

        if (currentCar.FX_Mud.activeSelf == false)
        {
            currentCar.FX_Mud.SafeSetActive(true);
        }

        var ps = currentCar.FX_Mud.GetComponentsInChildren<ParticleSystem>();
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
        if (currentCar == null || currentCar.FX_Mud == null)
            return;

        var ps = currentCar.FX_Mud.GetComponentsInChildren<ParticleSystem>();
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


    public string GetString_ANIM_IDLE()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_IDLE;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_IDLE;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_IDLE;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_IDLE;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_DRIVE()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_DRIVE;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_DRIVE;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_DRIVE;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_DRIVE;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_LEFT()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_LEFT;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_LEFT;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_LEFT;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_LEFT;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_RIGHT()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_RIGHT;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_RIGHT;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_RIGHT;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_RIGHT;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_1()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_BOOSTER_1;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_BOOSTER_1;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_BOOSTER_1;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_BOOSTER_1;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_BOOSTER_2()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_BOOSTER_2;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_BOOSTER_2;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_BOOSTER_2;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_BOOSTER_2;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_BRAKE()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_BRAKE;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_BRAKE;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_BRAKE;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_BRAKE;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_FLIP()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_FLIP;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_FLIP;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_FLIP;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_FLIP;
                    break;
            }
        }

        return animName;
    }

    public string GetString_ANIM_SPIN()
    {
        string animName = "";

        if (currentCar != null)
        {
            switch (currentCar.carType)
            {
                case DataManager.CAR_DATA.CarID.One:
                    animName = ANIM_CAR_01_SPIN;
                    break;
                case DataManager.CAR_DATA.CarID.Two:
                    animName = ANIM_CAR_02_SPIN;
                    break;
                case DataManager.CAR_DATA.CarID.Three:
                    animName = ANIM_CAR_03_SPIN;
                    break;
                case DataManager.CAR_DATA.CarID.Four:
                    animName = ANIM_CAR_04_SPIN;
                    break;
            }
        }

        return animName;
    }
}
