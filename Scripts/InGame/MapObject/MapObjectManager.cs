using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectManager : MonoSingleton<MapObjectManager>
{
    [SerializeField] public List<MapObject_ChargePad> chargePadList = new List<MapObject_ChargePad>();
    [SerializeField] public List<MapObject_TimingPad> timingPadList = new List<MapObject_TimingPad>();
    [SerializeField] public List<MapObject_ChargeZone> chargeZoneList = new List<MapObject_ChargeZone>();
    [SerializeField] public List<MapObject_ContainerBox> containerBoxList = new List<MapObject_ContainerBox>();
    [SerializeField] public List<MapObject_Ground> groundList = new List<MapObject_Ground>();
    [SerializeField] public List<MapObject_Obstacle> obstacleList = new List<MapObject_Obstacle>();
    [SerializeField] public List<MapObject_Catapult> catapultList = new List<MapObject_Catapult>();
    [SerializeField] public MapObject_Podium podium = null;

    [ReadOnly] public GameObject chargePadBase = null;
    [ReadOnly] public GameObject timingPadBase = null;
    [ReadOnly] public GameObject charZoneBase = null;
    [ReadOnly] public GameObject containerBase = null;
    [ReadOnly] public GameObject groundBase = null;
    [ReadOnly] public GameObject obstacleBase = null;
    [ReadOnly] public GameObject catapultBase = null;
    [ReadOnly] public GameObject podiumBase = null;


    private PhotonNetworkManager photonNetworkManager => PhotonNetworkManager.Instance;
    private NetworkInGameRPCManager networkInGameRPCManager => photonNetworkManager.MyNetworkInGameRPCManager;

    public void Initialize()
    {
        if (chargePadBase == null)
        {
            chargePadBase = new GameObject();
            chargePadBase.name = "[MapObject] ChargePadBase";
        }

        if (chargePadList != null && chargePadList.Count > 0)
        {
            for (int i = 0; i < chargePadList.Count; i++)
            {
                if (chargePadList[i] != null)
                {
                    chargePadList[i].SetData(i);
                    chargePadList[i].transform.parent = chargePadBase.transform;
                }
            }
        }

        if (timingPadBase == null)
        {
            timingPadBase = new GameObject();
            timingPadBase.name = "[MapObject] TimingPadBase";
        }

        if (timingPadList != null && timingPadList.Count > 0)
        {
            for (int i = 0; i < timingPadList.Count; i++)
            {
                if (timingPadList[i] != null)
                {
                    timingPadList[i].SetData(i);
                    timingPadList[i].transform.parent = timingPadBase.transform;
                }
            }
        }

        if (charZoneBase == null)
        {
            charZoneBase = new GameObject();
            charZoneBase.name = "[MapObject] ChargeZoneBase";
        }

        if (chargeZoneList != null && chargeZoneList.Count > 0)
        {
            for (int i = 0; i < chargeZoneList.Count; i++)
            {
                if (chargeZoneList[i] != null)
                {
                    chargeZoneList[i].SetData(i);
                    chargeZoneList[i].transform.parent = charZoneBase.transform;
                }
            }
        }

        if (containerBase == null)
        {
            containerBase = new GameObject();
            containerBase.name = "[MapObject] ContainerBase";
        }

        if (containerBoxList != null && containerBoxList.Count > 0)
        {
            for (int i = 0; i < containerBoxList.Count; i++)
            {
                if (containerBoxList[i] != null)
                {
                    containerBoxList[i].SetData(i);
                    containerBoxList[i].transform.parent = containerBase.transform;
                }
            }
        }

        if (groundBase == null)
        {
            groundBase = new GameObject();
            groundBase.name = "[MapObject] GroundBase";
        }


        if (groundList != null && groundList.Count > 0)
        {
            for (int i = 0; i < groundList.Count; i++)
            {
                if (groundList[i] != null)
                {
                    groundList[i].SetData(i);
                    groundList[i].transform.parent = groundBase.transform;
                }
            }
        }

        if (obstacleBase == null)
        {
            obstacleBase = new GameObject();
            obstacleBase.name = "[MapObject] ObstacleBase";
        }

        if (obstacleList != null && obstacleList.Count > 0)
        {
            for (int i = 0; i < obstacleList.Count; i++)
            {
                if (obstacleList[i] != null)
                {
                    obstacleList[i].SetData(i);
                    obstacleList[i].transform.parent = obstacleBase.transform;
                }
            }
        }

        if (catapultBase == null)
        {
            catapultBase = new GameObject();
            catapultBase.name = "[MapObject] CatapultBase";
        }

        if (catapultList != null && catapultList.Count > 0)
        {
            for (int i = 0; i < catapultList.Count; i++)
            {
                if (catapultList[i] != null)
                {
                    catapultList[i].SetData(i);
                    catapultList[i].transform.parent = catapultBase.transform;
                }
            }
        }

        if (podiumBase == null)
        {
            podiumBase = new GameObject();
            podiumBase.name = "[MapObject] PodiumBase";
        }

        if (podium != null && podiumBase != null)
        {
            podium.DeactivatePodium();
            podium.transform.parent = podiumBase.transform;
        }
    }


    public void ContainerLoop()
    {
        foreach (var i in containerBoxList)
        {
            if (i != null)
                StartCoroutine(ContainerCoroutine(i));
        }
    }


    private IEnumerator ContainerCoroutine(MapObject_ContainerBox box)
    {
        float counter = 0f;
        float coolTime = box.coolTime;
        float initialDelay = box.startDelay;

        yield return new WaitForSecondsRealtime(initialDelay);

        var myPlayer = InGameManager.Instance.myPlayer;

        while (true)
        {
            counter += Time.fixedDeltaTime;

            if (counter >= coolTime)
            {
                if (myPlayer != null && box != null)
                    networkInGameRPCManager.RPC_ActivateContainerBox(myPlayer.networkPlayerID, box.id);

                counter = 0f;
            }

            yield return new WaitForFixedUpdate();


            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                break;

            if (PhotonNetworkManager.Instance.IsRoomMasterClient == false)
                break;
        }
    }

    public void CatapultLoop()
    {
        foreach (var i in catapultList)
        {
            if (i != null)
                StartCoroutine(CatapultCoroutine(i));
        }
    }


    private IEnumerator CatapultCoroutine(MapObject_Catapult catapult)
    {
        float coolTime = catapult.coolTime;
        float initialDelay = catapult.startDelay;

        yield return new WaitForSecondsRealtime(initialDelay);

        var myPlayer = InGameManager.Instance.myPlayer;

        while (true)
        {
            if (catapult == null)
                break;

            while (catapult != null && catapult.isMoving == true)
                yield return null;

            if (catapult != null && catapult.isMoving == false)
            {
                if (networkInGameRPCManager != null && myPlayer != null && myPlayer.networkPlayerID != null && catapult != null)
                {
                    networkInGameRPCManager.RPC_ActivateCatapult(myPlayer.networkPlayerID, catapult.id);
                }
            }

            yield return new WaitForSeconds(coolTime);

            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
                break;

            if (PhotonNetworkManager.Instance.IsRoomMasterClient == false)
                break;
        }
    }






    public void OnRPCEvent_ActivateChargePad(Fusion.NetworkId networkID, int chargePadID, MapObject_ChargePad.ChargePadLevel currentLv)
    {
        var cp = chargePadList.Find(x => 
        {
            if (x != null)
                return x.id.Equals(chargePadID);
            else
                return false;
        });

        if (cp != null)
        {
            cp.ActivateChargePad(currentLv);
        }
    }

    public void OnRPCEvent_ResetChargePad(int chargePadID, MapObject_ChargePad.ChargePadLevel currentLv)
    {
        var cp = chargePadList.Find(x => {
            if (x != null)
                return x.id.Equals(chargePadID);
            else
                return false;
        });

        if (cp != null)
        {
            cp.ResetChargePad(currentLv);
        }
    }

    public void OnRPCEvent_ActivateContainerBox(int id)
    {
        var cb = containerBoxList.Find(x => {
            if (x != null)
                return x.id.Equals(id);
            else
                return false;
        });

        if (cb != null)
        {
            cb.ActivateContainerBox();
        }
    }

    public void OnRPCEvent_ActivateChargeZone(int chargeZoneID, bool isActive)
    {
        var cz = chargeZoneList.Find(x => {
            if (x != null)
                return x.id.Equals(chargeZoneID);
            else
                return false;
        });

        if (cz != null)
        {
            cz.ActivateChargeZone(isActive);
        }
    }

    public void OnRPCEvent_ActivateCatapult(int id)
    {
        var c = catapultList.Find(x => {
            if (x != null)
                return x.id.Equals(id);
            else
                return false;
        });

        if (c != null)
        {
            c.ActivateCatapult();
        }
    }
}
