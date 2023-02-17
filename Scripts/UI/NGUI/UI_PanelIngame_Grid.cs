using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
                i.gameObject.SafeSetActive(false);
        }
        else
            listOfStandingInfos = new List<UI_PanelIngame_StandingInfo>();

        infoSlotTemplate.gameObject.SafeSetActive(false);
    }

    public void SetStandingInfo()
    {
        var list = InGameManager.Instance.ListOfPlayers;

        for (int i = 0; i < list.Count; i++)
        {
            CreateSlots(i, list[i]);
        }

        foreach (var i in listOfStandingInfos)
            i.gameObject.SafeSetActive(false);

        UtilityCoroutine.StartCoroutine(ref showInfos, ShowInfos(), this);
    }

    public void CreateSlots(int index, InGameManager.PlayerInfo info)
    {
        if (index < listOfStandingInfos.Count)
        {
            listOfStandingInfos[index].SetInfo(info, index);
        }
        else
        {
            GameObject slot = Instantiate(infoSlotTemplate.gameObject, grid.transform);
            slot.transform.localScale = Vector3.one;
            var script = slot.GetComponent<UI_PanelIngame_StandingInfo>();
            if (script != null)
            {
                listOfStandingInfos.Add(slot.GetComponent<UI_PanelIngame_StandingInfo>());
                script.SetInfo(info, index);
            }
        }

        if (listOfStandingInfos != null)
        {
            listOfStandingInfos.Sort((x, y) => x.rank.CompareTo(y.rank));
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

        for (int i = 0; i < list.Count; i++)
        {
            if (i < listOfStandingInfos.Count)
            {
                listOfStandingInfos[i].gameObject.SafeSetActive(true);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
