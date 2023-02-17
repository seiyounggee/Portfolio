using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutGamePlayer : MonoBehaviour
{
    [SerializeField] PlayerCar car = null;
    [SerializeField] PlayerCharacter character = null;

    public void SetData()
    {
        //Set Car
        //Set Character ...
        if (DataManager.Instance.userData != null && car != null && character != null)
        {
            var carID = DataManager.Instance.userData.MyCarID;
            var charID = DataManager.Instance.userData.MyCharacterID;

            car.SetCar((DataManager.CAR_DATA.CarID)carID);
            character.SetCharacter((DataManager.CHARACTER_DATA.CharacterType)charID);
            character.SetHeadObj(carID);
        }

    }
}
