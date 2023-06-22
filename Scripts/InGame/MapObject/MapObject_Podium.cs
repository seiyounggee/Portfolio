using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_Podium : MonoBehaviour
{
    [SerializeField] public List<GameObject> objToHide = new List<GameObject>();

    [SerializeField] public List<SpawnListInfo> spawnList = new List<SpawnListInfo>();

    [Serializable]
    public class SpawnListInfo
    {
        public int rankNumber = 0;
        public Transform spawnPosition = null;
    }

    public void ActivatePodium()
    {
        foreach (var i in objToHide)
        {
            if (i != null)
                i.SafeSetActive(true);
        }

        var list = InGameManager.Instance.ListOfPlayers;
        if (list != null && list.Count > 0)
        {
            foreach (var i in list)
            {
                if (i == null)
                    continue;

                if (i.isRetire)
                    continue;

                var p = list.Find(x => x.currentRank.Equals(i.currentRank));

                if (p != null)
                {
                    var dummyPlayer = SpawnDummyPlayer(i.photonNetworkID, p.data.carID, p.data.characterID, i.currentRank);
                    if (dummyPlayer != null)
                    {
                        if (i.currentRank == 1)
                            dummyPlayer.ChangeMovementType(DummyPlayer.MovementType.Victory);
                        else
                            dummyPlayer.ChangeMovementType(DummyPlayer.MovementType.Idle);
                    }
                }
            }
        }
    }

    private DummyPlayer SpawnDummyPlayer(Fusion.NetworkId playerID, int carID, int charID, int rank)
    {
        GameObject go = Instantiate(PrefabManager.Instance.DummyPlayer, Vector3.zero, Quaternion.identity);
        var dp = go.GetComponent<DummyPlayer>();

        if (dp != null)
        {
            dp.SetPlayer_Car(carID);
            dp.SetPlayer_Character(charID);
        }

        go.transform.localScale = Vector3.one;
        if (playerID != null)
            go.transform.name = "DummyPlayer_" + playerID;
        var sInfo = spawnList.Find(x => x.rankNumber.Equals(rank));
        if (sInfo != null && sInfo.spawnPosition != null)
        {
            go.SafeSetActive(true);
            go.transform.position = sInfo.spawnPosition.position;
            go.transform.rotation = sInfo.spawnPosition.rotation;
        }
        else
        {
            go.SafeSetActive(false);
        }

        return dp;
    }

    public void DeactivatePodium() 
    {
        foreach (var i in objToHide)
        {
            if (i != null)
                i.SafeSetActive(false);
        }
    }

}
