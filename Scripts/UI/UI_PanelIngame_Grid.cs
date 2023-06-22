using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static InGameManager;
using System;

public class UI_PanelIngame_Grid : MonoBehaviour
{
    [SerializeField] UIGrid grid = null;
    [SerializeField] UI_PanelIngame_StandingInfo infoSlotTemplate = null;

    private List<UI_PanelIngame_StandingInfo> listOfStandingInfos = new List<UI_PanelIngame_StandingInfo>();

    public void Initialize()
    {
        if (listOfStandingInfos != null)
        {
            foreach (var i in listOfStandingInfos)
            {
                Destroy(i.gameObject);
            }

            listOfStandingInfos.Clear();
        }
        else
            listOfStandingInfos = new List<UI_PanelIngame_StandingInfo>();

        infoSlotTemplate.gameObject.SafeSetActive(false);
    }

    public void CreateStandingSlots()
    {
        var list = InGameManager.Instance.ListOfPlayers;

        for (int i = 0; i < list.Count; i++)
        {
            CreateSlots(i, list[i]);
        }
    }

    public void SetAndShowStandingInfo()
    {
        UtilityCoroutine.StartCoroutine(ref showInfos, ShowInfos(), this);
    }

    public void CreateSlots(int index, InGameManager.PlayerInfo info)
    {
        if (index < listOfStandingInfos.Count)
        {
            listOfStandingInfos[index].SetInfo(info);
        }
        else
        {
            GameObject slot = Instantiate(infoSlotTemplate.gameObject, grid.transform);
            slot.transform.localScale = Vector3.one;
            var script = slot.GetComponent<UI_PanelIngame_StandingInfo>();
            if (script != null)
            {
                listOfStandingInfos.Add(slot.GetComponent<UI_PanelIngame_StandingInfo>());
                script.SetInfo(info);
            }
        }

        UpdateListOrder();
    }

    private void UpdateListOrder()
    {
        if (listOfStandingInfos != null)
        {
            var result = listOfStandingInfos
                .OrderByDescending(x => x.playerInfo != null ? x.playerInfo.isEnterFinishLine_Network : false)
                .ThenBy(x => x.playerInfo != null ? x.playerInfo.enterFinishLineTime_Network : 0f)
                .ThenBy(x => x.playerInfo != null ? x.playerInfo.distLeft : 0f);
            listOfStandingInfos = result.ToList();

            for (int i = 0; i < listOfStandingInfos.Count; i++)
            {
                listOfStandingInfos[i].SetRank(i + 1);
                listOfStandingInfos[i].name = "standingInfo_" + i;
            }
        }

        grid.Reposition();
    }

    IEnumerator showInfos = null;

    private IEnumerator ShowInfos()
    {
        yield return null;
        grid.Reposition();
        yield return null;

        if (listOfStandingInfos == null || listOfStandingInfos.Count <= 0)
            yield break;

        var list = InGameManager.Instance.ListOfPlayers;

        while (true)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < listOfStandingInfos.Count; j++)
                {
                    if (list[i] == null || list[i].data == null)
                        continue;

                    if (listOfStandingInfos[i] == null || listOfStandingInfos[i].isActive == true)
                        continue;

                    if (listOfStandingInfos[i].isActive == false 
                        && listOfStandingInfos[i].IsShow == true)
                        listOfStandingInfos[i].Activate();
                }

                if (InGameManager.Instance.isGameEnded == false)
                    yield return null;
                else
                    yield return new WaitForSeconds(0.2f);
            }

            yield return null;

            bool isAllStandingInfoActive = false;
            int activeCount = 0;
            foreach (var i in listOfStandingInfos)
            {
                if (i != null && i.isActive)
                {
                    ++activeCount;
                }

                if (activeCount == InGameManager.Instance.ListOfPlayers.Count)
                    isAllStandingInfoActive = true;
            }

            if (isAllStandingInfoActive)
                break;
        }
    }

    public void UpdateInfo() //Info °»½Å
    {
        if (listOfStandingInfos == null || listOfStandingInfos.Count <= 0)
        {
            CreateStandingSlots();
        }

        var list = InGameManager.Instance.ListOfPlayers;

        foreach (var i in listOfStandingInfos)
        {
            var info = list.Find(x =>
            {
                if (x.photonNetworkID != null && i.playerInfo != null && i.playerInfo.photonNetworkID != null)
                    return x.photonNetworkID.Equals(i.playerInfo.photonNetworkID);
                else
                    return false;
            }
            );

            if (info != null)
            {
                if (i != null && i.playerInfo != null)
                {
                    i.UpdateInfo(info);
                }
            }
        }

        UpdateListOrder();
    }
}
