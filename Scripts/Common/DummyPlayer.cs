using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    [SerializeField] PlayerCar playerCar = null;
    [SerializeField] PlayerCharacter playerCharacter = null;

    public enum MovementType { None, Idle, Move, Victory, Complete }
    public MovementType currentMovementType = MovementType.None;
    public enum AnimationState { Idle, Drive, Booster_1, Booster_2, Brake, MoveLeft, MoveRight, Flip, Victory, Complete, Retire, Spin }
    [ReadOnly] public AnimationState currentCarAnimationState = AnimationState.Idle;
    [ReadOnly] public AnimationState currentCharAnimationState = AnimationState.Idle;

    private void Awake()
    {
        if (playerCar == null)
            playerCar.GetComponentInChildren<PlayerCar>();

        if (playerCharacter == null)
            playerCharacter.GetComponentInChildren<PlayerCharacter>();
    }

    public void SetPlayer_Car(int carID)
    {
        if (playerCar == null)
            return;

        var carRef = DataManager.Instance.GetCarRef(carID);
        if (carRef != null)
        {
            playerCar.SetCar(carRef);
        }
    }

    public void SetPlayer_Character(int charID)
    {
        if (playerCharacter == null)
            return;

        var charRef = DataManager.Instance.GetCharacterRef(charID);
        if (charRef != null)
        {
            playerCharacter.SetCharacter(charRef);
        }

    }

    public void SetPlayer_ToMyData()
    {
        //Set Car
        //Set Character ...
        if (playerCar != null && playerCharacter != null)
        {
            var carID = AccountManager.Instance.Equipped_CarID;
            var charID = AccountManager.Instance.Equipped_CharacterID;

            var carRef = DataManager.Instance.GetCarRef(carID);
            if (carRef != null)
            {
                playerCar.SetCar(carRef);
            }

            var charRef = DataManager.Instance.GetCharacterRef(charID);
            if (charRef != null)
            {
                playerCharacter.SetCharacter(charRef);
            }

            transform.rotation = Quaternion.Euler(0f, -135f, 0f);
        }
    }

    public void ChangeMovementType(MovementType type)
    {
        currentMovementType = type;

        if (playerCar == null || playerCharacter == null)
            return;

        switch (currentMovementType)
        {
            case MovementType.None:
                {
                    SetAnimation_Both(AnimationState.Idle, true);
                    SetAnimation_Both(AnimationState.Drive, false);
                    SetAnimation_Both(AnimationState.Complete, false);
                    SetAnimation_Both(AnimationState.Victory, false);
                }
                break;
            case MovementType.Idle:
                {
                    SetAnimation_Both(AnimationState.Idle, true);
                    SetAnimation_Both(AnimationState.Drive, false);
                    SetAnimation_Both(AnimationState.Complete, false);
                    SetAnimation_Both(AnimationState.Victory, false);
                }
                break;
            case MovementType.Move:
                {
                    SetAnimation_Both(AnimationState.Idle, false);
                    SetAnimation_Both(AnimationState.Drive, true);
                    SetAnimation_Both(AnimationState.Complete, false);
                    SetAnimation_Both(AnimationState.Victory, false);
                }
                break;
            case MovementType.Complete:
                {
                    SetAnimation_Both(AnimationState.Idle, false);
                    SetAnimation_Both(AnimationState.Drive, false);
                    SetAnimation_Both(AnimationState.Complete, true);
                    SetAnimation_Both(AnimationState.Victory, false);
                }
                break;
            case MovementType.Victory:
                {
                    SetAnimation_Both(AnimationState.Idle, false);
                    SetAnimation_Both(AnimationState.Drive, false);
                    SetAnimation_Both(AnimationState.Complete, false);
                    SetAnimation_Both(AnimationState.Victory, true);
                }
                break;
        }
    }


    public void FixedUpdate()
    {

    }



    #region Animation

    private void SetAnimation_Both(AnimationState state, bool isOn)
    {
        SetAnimation_CarOnly(state, isOn);
        SetAnimation_CharacterOnly(state, isOn);
    }

    private void SetAnimation_CarOnly(AnimationState state, bool isOn)
    {
        if (playerCar == null || playerCar.currentCar == null || playerCar.currentCar.animator == null)
            return;

        currentCarAnimationState = state;

        var carAnim = playerCar.currentCar.animator;

        switch (state)
        {
            case AnimationState.Idle:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_IDLE(), isOn);
                }
                break;
            case AnimationState.Drive:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_DRIVE(), isOn);
                }
                break;
            case AnimationState.Booster_1:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_BOOSTER_1());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_BOOSTER_1());
                    }
                }
                break;
            case AnimationState.Booster_2:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_BOOSTER_2());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_BOOSTER_2());
                    }
                }
                break;
            case AnimationState.Brake:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_BRAKE(), isOn);
                }
                break;
            case AnimationState.MoveLeft:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_LEFT(), isOn);
                }
                break;
            case AnimationState.MoveRight:
                {
                    carAnim.SafeSetBool(playerCar.GetString_ANIM_RIGHT(), isOn);
                }
                break;

            case AnimationState.Flip:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_FLIP());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_FLIP());
                    }
                }
                break;
            case AnimationState.Spin:
                {
                    if (isOn)
                    {
                        carAnim.SafeResetTrigger(playerCar.GetString_ANIM_SPIN());
                        carAnim.SafeSetTrigger(playerCar.GetString_ANIM_SPIN());
                    }
                }
                break;
        }
    }

    private void SetAnimation_CharacterOnly(AnimationState state, bool isOn)
    {
        if (playerCharacter == null || playerCharacter.currentCharacter == null || playerCharacter.currentCharacter.animator == null)
            return;

        currentCharAnimationState = state;

        var charAnim = playerCharacter.currentCharacter.animator;

        switch (state)
        {
            case AnimationState.Idle:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_IDLE(), isOn);
                }
                break;
            case AnimationState.Drive:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_DRIVE(), isOn);
                }
                break;
            case AnimationState.Booster_1:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_BOOSTER_1());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_BOOSTER_1());
                    }
                }
                break;
            case AnimationState.Booster_2:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_BOOSTER_2());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_BOOSTER_2());
                    }
                }
                break;
            case AnimationState.Brake:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_BRAKE(), isOn);
                }
                break;
            case AnimationState.MoveLeft:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_LEFT(), isOn);
                }
                break;
            case AnimationState.MoveRight:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_RIGHT(), isOn);
                }
                break;
            case AnimationState.Victory:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_VICTORY(), isOn);
                }
                break;
            case AnimationState.Complete:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_COMPLETE(), isOn);
                }
                break;
            case AnimationState.Retire:
                {
                    charAnim.SafeSetBool(playerCharacter.GetString_ANIM_RETIRE(), isOn);
                }
                break;
            case AnimationState.Flip:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_FLIP());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_FLIP());
                    }
                }
                break;
            case AnimationState.Spin:
                {
                    if (isOn)
                    {
                        charAnim.SafeResetTrigger(playerCharacter.GetString_ANIM_SPIN());
                        charAnim.SafeSetTrigger(playerCharacter.GetString_ANIM_SPIN());
                    }
                }
                break;
        }
    }

    #endregion
}
