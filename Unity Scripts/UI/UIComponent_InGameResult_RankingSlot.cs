using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_InGameResult_RankingSlot : MonoBehaviour
{
    [SerializeField] RawImage rankTexture;
    [SerializeField] TextMeshProUGUI nickname;
    [SerializeField] TextMeshProUGUI rankTxt;

    [SerializeField] Image winnerBG;
    [SerializeField] Image mineBG;

    [ReadOnly] private InGame_Quantum.PlayerInfo playerInfo;

    public int SortIndex
    {
        get
        {
            if (playerInfo != null)
            {
                if (playerInfo.inGamePlayMode == Quantum.InGamePlayMode.SoloMode)
                {
                    if (playerInfo.rank_solo > 0)
                        return playerInfo.rank_solo;
                    else //랭킹 부여 안받은 사람들
                        return int.MaxValue;
                }
                else if (playerInfo.inGamePlayMode == Quantum.InGamePlayMode.TeamMode)
                {
                    if (playerInfo.rank_team > 0)
                        return playerInfo.rank_team;
                    else //랭킹 부여 안받은 사람들
                        return int.MaxValue;
                }
            }

            return 0;
        }
    }

    public void Setup(InGame_Quantum.PlayerInfo info)
    {
        playerInfo = info;

        var tier = AccountManager.Instance.GetRankingTier(info.rankingPoint);
        rankTexture.texture = ResourceManager.Instance.GetOutGameRankIconByTier(tier);
        nickname.SafeSetText(info.nickname);

        if (info.inGamePlayMode == Quantum.InGamePlayMode.SoloMode)
        {
            if (info.rank_solo > 0)
                rankTxt.SafeSetText(StringManager.GetRankingFormat(info.rank_solo));
            else
                rankTxt.SafeSetText("-");

            mineBG.SafeSetActive(AccountManager.Instance.PID.Equals(info.pid));
            winnerBG.SafeSetActive(false);
        }
        else if (info.inGamePlayMode == Quantum.InGamePlayMode.TeamMode)
        {
            if (info.rank_team > 0)
            {
                if (info.rank_team == 1) //승리
                {
                    rankTxt.SafeLocalizeText("COMMON_WIN");
                }
                else //2등 이후는 패배로 표시
                {
                    rankTxt.SafeLocalizeText("COMMON_LOSE");
                }
            }
            else
                rankTxt.SafeSetText("-");

            mineBG.SafeSetActive(false);
            winnerBG.SafeSetActive(info.rank_team == 1);
        }
    }
}
