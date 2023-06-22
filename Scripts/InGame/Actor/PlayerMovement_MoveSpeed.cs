using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement
{
    [ReadOnly] public float peakMaxSpeed = 0f;
    [ReadOnly] public float currentMaxSpeed = 0f;

    [ReadOnly] public float timingBoosterBatteryCost = 0f;
    [ReadOnly] public int timingBoosterCurrentCombo = 0;
    private int timingBoosterMaxCombo = 5;
    public enum TimingBoosterSuccessType { None, Great, Perfect};
    [ReadOnly] public TimingBoosterSuccessType currentTimingBoosterSuccessType = TimingBoosterSuccessType.None;

    public enum SpeedState { Increase, Constant, Decrease}
    public SpeedState currentSpeedState = SpeedState.Constant;

    public enum SpeedSection { None, 
        Increase_One, Increase_Two, Increase_Three, Increase_Four, Increase_Five, Increase_End,
        Decrease_One, Decrease_Two, Decrease_Three, Decrease_Four, Decrease_Five, Decrease_End
    }
    public SpeedSection currentSpeedSection = SpeedSection.None;

    private float GetCurrentMoveSpeed()
    {
        if (network_isMoving && !isFlipped && !isOutOfBoundary)
        {
            SetCurrentMaxSpeed();
            SetCurrentSpeedState();
            SetCurrentSpeedSection();

            if (currentSpeedState == SpeedState.Increase)
            {
                if (client_currentMoveSpeed <= currentMaxSpeed)
                    return client_currentMoveSpeed + Runner.DeltaTime * GetSpeed() * GetSpeedConstant();
                else
                    return currentMaxSpeed;
            }
            else if (currentSpeedState == SpeedState.Decrease)
            {
                if (client_currentMoveSpeed >= 0f)
                    return client_currentMoveSpeed + Runner.DeltaTime * GetSpeed() * GetSpeedConstant();
                else
                    return 0f;
            }
            else if (currentSpeedState == SpeedState.Constant)
            {
                return client_currentMoveSpeed;
            }
        }
        else if (isOutOfBoundary)
        {
            peakMaxSpeed = ref_defaultMaxSpeed;
            currentMaxSpeed = ref_defaultMaxSpeed;
            client_currentMoveSpeed = ref_defaultMaxSpeed;
            currentSpeedSection = SpeedSection.None;

            return client_currentMoveSpeed;
        }
        else if (isFlipped)
        {
            peakMaxSpeed = ref_defaultMaxSpeed;
            currentMaxSpeed = ref_defaultMaxSpeed;
            client_currentMoveSpeed = 0f;
            currentSpeedSection = SpeedSection.None;

            return 0f;
        }
        else
        {
            peakMaxSpeed = ref_defaultMaxSpeed;
            currentMaxSpeed = ref_defaultMaxSpeed;
            client_currentMoveSpeed = ref_defaultMaxSpeed;
            currentSpeedSection = SpeedSection.None;
            currentSpeedState = SpeedState.Constant;
        }

        return 0f;
    }

    public void SetCurrentMaxSpeed()
    {
        if (network_isMoving == false)
            return;

        var speed = 0f;

        if (isBoosting)
        {
            speed = ref_defaultMaxSpeed;

            switch (client_currentCarBoosterLv)
            {
                case CarBoosterType.None:
                    break;
                case CarBoosterType.CarBooster_Starting:
                    speed += ref_boosterStartingMaxSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelOne:
                    speed += ref_boosterLv1MaxSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelTwo:
                    speed += ref_boosterLv2MaxSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelThree:
                    speed += ref_boosterLv3MaxSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelFour_Timing:
                    {
                        if (currentTimingBoosterSuccessType == TimingBoosterSuccessType.Great)
                            speed += ref_boosterTimingMaxSpeed + ref_boosterTimingMaxSpeed * (ref_boosterTimingMaxSpeedComboEfficiency - 1) * timingBoosterCurrentCombo + timingBoosterBatteryCost * ref_boosterGreatTimingMaxSpeedConsumedGaugeEfficiency;
                        else if (currentTimingBoosterSuccessType == TimingBoosterSuccessType.Perfect)
                            speed += ref_boosterTimingMaxSpeed + ref_boosterTimingMaxSpeed * (ref_boosterTimingMaxSpeedComboEfficiency - 1) * timingBoosterCurrentCombo + timingBoosterBatteryCost * ref_boosterPerfectTimingMaxSpeedConsumedGaugeEfficiency;
                    }
                    break;
            }

            switch (client_currentChargeBoosterLv)
            {
                case ChargePadBoosterLevel.None:
                    break;
                case ChargePadBoosterLevel.ChargePadBooster_One:
                    speed += ref_chargePadLv1MaxSpeed;
                    break;
                case ChargePadBoosterLevel.ChargePadBooster_Two:
                    speed += ref_chargePadLv2MaxSpeed;
                    break;
                case ChargePadBoosterLevel.ChargePadBooster_Three:
                    speed += ref_chargePadLv3MaxSpeed;
                    break;
            }
        }
        else
        {
            speed = ref_defaultMaxSpeed;
        }

        if (isDrafting)
        {
            speed += ref_draftMaxSpeed;
        }

        if (isFullBattery)
        {
            speed += ref_fullBusterSpeedBuffMaxSpeed;
        }

        if (ref_playerRankingBuffOnOff == 1)
            speed += (network_Rank - 1) * ref_playerRankingmaxSpeed;

        if (isMoveLine)
        {
            speed += ref_moveLineMaxSpeedDown;
        }

        if (isStunned)
        {
            speed += ref_stunMaxSpeedDown;
        }
        else if (isShield)
        {
            speed += ref_shieldMaxSpeedDown;

            if(speed < ref_shieldMinSpeed)
                speed = ref_shieldMinSpeed;
        }

        currentMaxSpeed = speed;

        if (isBoosting || isDrafting)
        {
            peakMaxSpeed = speed;
        }
        else
        {
            if (client_currentMoveSpeed <= ref_defaultMaxSpeed)
                peakMaxSpeed = ref_defaultMaxSpeed;
        }
    }

    public void SetCurrentSpeedState()
    {
        if (IsEqual(client_currentMoveSpeed, currentMaxSpeed))
            currentSpeedState = SpeedState.Constant;
        else
        {
            if (client_currentMoveSpeed < currentMaxSpeed)
                currentSpeedState = SpeedState.Increase;
            else
                currentSpeedState = SpeedState.Decrease;
        }
    }

    //속도구간 지정
    private void SetCurrentSpeedSection()
    {
        if (network_isMoving == false)
            return;

        //증속
        if (currentSpeedState == SpeedState.Increase)
        {
            if (client_currentMoveSpeed < currentMaxSpeed * ref_increaseSpeedRange1stReferenceValue)
            {
                currentSpeedSection = SpeedSection.Increase_One;
            }
            else if (client_currentMoveSpeed < currentMaxSpeed * ref_increaseSpeedRange2ndReferenceValue)
            {
                currentSpeedSection = SpeedSection.Increase_Two;
            }
            else if (client_currentMoveSpeed < currentMaxSpeed * ref_increaseSpeedRange3rdReferenceValue)
            {
                currentSpeedSection = SpeedSection.Increase_Three;
            }
            else if (client_currentMoveSpeed < currentMaxSpeed * ref_increaseSpeedRange4thReferenceValue)
            {
                currentSpeedSection = SpeedSection.Increase_Four;
            }
            else if (client_currentMoveSpeed < currentMaxSpeed * ref_increaseSpeedRange5thReferenceValue)
            {
                currentSpeedSection = SpeedSection.Increase_Five;
            }
            else
            {
                currentSpeedSection = SpeedSection.Increase_End;
            }
        }
        //감속
        else if (currentSpeedState == SpeedState.Decrease)
        {
            if (client_currentMoveSpeed > ref_defaultMaxSpeed * (1f + ref_decreaseSpeedRange1stReferenceValue))
            {
                currentSpeedSection = SpeedSection.Decrease_One;
            }
            else if (client_currentMoveSpeed > ref_defaultMaxSpeed * (1f + ref_decreaseSpeedRange2ndReferenceValue))
            {
                currentSpeedSection = SpeedSection.Decrease_Two;
            }
            else if (client_currentMoveSpeed > ref_defaultMaxSpeed * (1f + ref_decreaseSpeedRange3rdReferenceValue))
            {
                currentSpeedSection = SpeedSection.Decrease_Three;
            }
            else if (client_currentMoveSpeed > ref_defaultMaxSpeed * (1f + ref_decreaseSpeedRange4thReferenceValue))
            {
                currentSpeedSection = SpeedSection.Decrease_Four;
            }
            else if (client_currentMoveSpeed > ref_defaultMaxSpeed * (1f + ref_decreaseSpeedRange5thReferenceValue))
            {
                currentSpeedSection = SpeedSection.Decrease_Five;
            }
            else
            {
                currentSpeedSection = SpeedSection.Decrease_End;
            }
        }
    }

    //속도
    private float GetSpeed()
    {
        float speed = 0f;

        if (currentSpeedState == SpeedState.Increase)
            speed = ref_defaultIncreaseSpeed;
        else if (currentSpeedState == SpeedState.Decrease)
            speed = ref_defaultDecreaseSpeed;
        else if (currentSpeedState == SpeedState.Constant)
            speed = 0f;

        if (isCarBoosting && currentSpeedState == SpeedState.Increase)
        {
            switch (client_currentCarBoosterLv)
            {
                case CarBoosterType.CarBooster_Starting:
                    speed += ref_boosterStartingIncreaseSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelOne:
                    speed += ref_boosterLv1IncreaseSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelTwo:
                    speed += ref_boosterLv2IncreaseSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelThree:
                    speed += ref_boosterLv3IncreaseSpeed;
                    break;
                case CarBoosterType.CarBooster_LevelFour_Timing:
                    {
                        if (currentTimingBoosterSuccessType == TimingBoosterSuccessType.Great)
                            speed += ref_boosterTimingIncreaseSpeed + ref_boosterTimingIncreaseSpeed * (ref_boosterTimingIncreaseSpeedComboEfficiency - 1f) * timingBoosterCurrentCombo + timingBoosterBatteryCost * ref_boosterGreatTimingIncreaseSpeedConsumedGaugeEfficiency;
                        else if (currentTimingBoosterSuccessType == TimingBoosterSuccessType.Perfect)
                            speed += ref_boosterTimingIncreaseSpeed + ref_boosterTimingIncreaseSpeed * (ref_boosterTimingIncreaseSpeedComboEfficiency - 1f) * timingBoosterCurrentCombo + timingBoosterBatteryCost * ref_boosterPerfectTimingIncreaseSpeedConsumedGaugeEfficiency;
                    }
                    break;
            }
        }

        if (isChargePadBoosting && currentSpeedState == SpeedState.Increase)
        {
            switch (client_currentChargeBoosterLv)
            {
                case ChargePadBoosterLevel.ChargePadBooster_One:
                    speed += ref_chargePadLv1IncreaseSpeed;
                    break;
                case ChargePadBoosterLevel.ChargePadBooster_Two:
                    speed += ref_chargePadLv2IncreaseSpeed;
                    break;
                case ChargePadBoosterLevel.ChargePadBooster_Three:
                    speed += ref_chargePadLv3IncreaseSpeed;
                    break;
            }
        }

        if (isStunned)
        {
            speed += ref_stunDecreaseSpeed;
        }
        else if (isShield)
        {
            speed += ref_shieldDecreaseSpeed;
        }

        if (isMoveLine)
        {
            speed += ref_moveLineDecreaseSpeed;
        }

        if (isDrafting)
        {
            speed += ref_draftIncreaseSpeed;
        }

        if (ref_playerRankingBuffOnOff == 1)
        {
            speed += (inGameManager.myCurrentRank - 1) * ref_playerRankingIncreaseSpeed;
        }

        if (isFullBattery)
        {
            speed += ref_fullBusterSpeedBuffIncreaseSpeed;
        }


        if (currentSpeedState == SpeedState.Increase)
        {
            if (speed <= 0)
                speed = 0f;
        }
        else if (currentSpeedState == SpeedState.Decrease)
        {
            if (speed >= 0)
                speed = 0f;
        }

        return speed;
    }

    //속도 계수
    private float GetSpeedConstant()
    {
        float speed = 0f;
        switch (currentSpeedSection)
        {
            case SpeedSection.None:
            default:
                break;
            case SpeedSection.Increase_One:
                speed = ref_increaseSpeedRange1stDepreciationValue;
                break;
            case SpeedSection.Increase_Two:
                speed = ref_increaseSpeedRange2ndDepreciationValue;
                break;
            case SpeedSection.Increase_Three:
                speed = ref_increaseSpeedRange3rdDepreciationValue;
                break;
            case SpeedSection.Increase_Four:
                speed = ref_increaseSpeedRange4thDepreciationValue;
                break;
            case SpeedSection.Increase_Five:
                speed = ref_increaseSpeedRange5thDepreciationValue;
                break;
            case SpeedSection.Increase_End:
                speed = ref_increaseSpeedRangeEndDepreciationValue;
                break;

            case SpeedSection.Decrease_One:
                speed = ref_decreaseSpeedRange1stDepreciationValue;
                break;
            case SpeedSection.Decrease_Two:
                speed = ref_decreaseSpeedRange2ndDepreciationValue;
                break;
            case SpeedSection.Decrease_Three:
                speed = ref_decreaseSpeedRange3rdDepreciationValue;
                break;
            case SpeedSection.Decrease_Four:
                speed = ref_decreaseSpeedRange4thDepreciationValue;
                break;
            case SpeedSection.Decrease_Five:
                speed = ref_decreaseSpeedRange5thDepreciationValue;
                break;
            case SpeedSection.Decrease_End:
                speed = ref_decreaseSpeedRangeEndDepreciationValue;
                break;
        }


        return speed;
    }

    const float EPSILON = 0.1f; // 허용오차

    private bool IsEqual(float x, float y) // 비교 함수.
    {
        return (((x - EPSILON) < y) && (y < (x + EPSILON)));
    }
}
