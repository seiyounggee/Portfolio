using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_SwitchingPad : MonoBehaviour
{
    [ReadOnly] public int id = -1;

    public NetworkInGameRPCManager networkInGameRPCManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

    public void SetData(int id)
    {
        this.id = id;
    }


    [SerializeField]
    public class SwitchPad
    {
        public int subId = -1;
        public int nearestWayPointIndex = -1;
        public PlayerMovement.LaneType nearestWayPointLaneType = PlayerMovement.LaneType.None;
    }
}
