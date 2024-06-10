using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_Rank_RankingSlot : MonoBehaviour, IInfiniteScrollContent
{
    [SerializeField] RawImage rankTexture;
    [SerializeField] TextMeshProUGUI nickname;
    [SerializeField] TextMeshProUGUI rankTxt;
    [SerializeField] TextMeshProUGUI rpTxt;

    private bool isUpdate;

    public void HideSlot()
    {
        gameObject.SafeSetActive(false);
    }

    public void Setup(RankingManager.RankData.RankingUserInfo info, int index)
    {
        var tier = AccountManager.Instance.GetRankingTier(info.RankingPoint);
        rankTexture.texture = ResourceManager.Instance.GetOutGameRankIconByTier(tier);
        nickname.SafeSetText(info.Nickname);
        rpTxt.SafeSetText("RP " + info.RankingPoint.ToString());
        rankTxt.SafeSetText((index + 1).ToString());

        gameObject.SafeSetActive(true);
    }

    bool IInfiniteScrollContent.Update(int index)
    {
        if (RankingManager.Instance.RankingData == null)
            return false;

        var list = RankingManager.Instance.RankingData.TotalRP_RankingUserList;

        if (list == null || list.Count <= 0)
            return false ;

        if (index < list.Count)
            Setup(list[index], index);
        else
            HideSlot();

        return true;
    }

    private void Update()
    {

    }
}
