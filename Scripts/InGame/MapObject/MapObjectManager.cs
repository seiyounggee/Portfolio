using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectManager : MonoSingleton<MapObjectManager>
{
    [SerializeField] public List<MapObject_ChargePad> chargePadList = new List<MapObject_ChargePad>();
    [SerializeField] public List<MapObject_ChargeZone> chargeZoneList = new List<MapObject_ChargeZone>();
    [SerializeField] public List<MapObject_ContainerBox> containerBoxList = new List<MapObject_ContainerBox>();
    [SerializeField] public List<MapObject_Ground> groundList = new List<MapObject_Ground>();
    //[SerializeField] public List<FoodTruck_Movement> FoodTrucks { get { return InGameManager.Instance.ListOfFoodTrucks; } }

    [SerializeField] public MapObject_FinishBoard finishBoard = null;

    public void Start()
    {
        if (chargePadList != null && chargePadList.Count > 0)
        {
            for (int i = 0; i < chargePadList.Count; i++)
            {
                if (chargePadList[i] != null)
                    chargePadList[i].SetData(i);
            }
        }

        if (chargeZoneList != null && chargeZoneList.Count > 0)
        {
            for (int i = 0; i < chargeZoneList.Count; i++)
            {
                if (chargeZoneList[i] != null)
                    chargeZoneList[i].SetData(i);
            }
        }

        if (containerBoxList != null && containerBoxList.Count > 0)
        {
            for (int i = 0; i < containerBoxList.Count; i++)
            {
                if (containerBoxList[i] != null)
                    containerBoxList[i].SetData(i);
            }
        }

        if (groundList != null && groundList.Count > 0)
        {
            for (int i = 0; i < groundList.Count; i++)
            {
                if (groundList[i] != null)
                    groundList[i].SetData(i);
            }
        }
    }

    public void OnEvent_ActivateChargePad(int viewID, int id, MapObject_ChargePad.ChargePadLevel currentLv)
    {
        var cp = chargePadList.Find(x => x.id.Equals(id));
        if (cp != null)
        {
            cp.ActivateChargePad(currentLv);
        }
    }

    public void OnEvent_ResetChargePad(int id, MapObject_ChargePad.ChargePadLevel currentLv)
    {
        var cp = chargePadList.Find(x => x.id.Equals(id));
        if (cp != null)
        {
            cp.ResetChargePad(currentLv);
        }
    }

    public void OnEvent_ActivateContainerBox(int id)
    {
        var cb = containerBoxList.Find(x => x.id.Equals(id));
        if (cb != null)
        {
            cb.ActivateContainerBox();
        }
    }
}
