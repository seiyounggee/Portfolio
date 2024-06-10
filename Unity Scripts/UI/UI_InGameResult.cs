using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InGameResult : UIBase
{
    [SerializeField] public Button homeBtn = null;

    [Header("Rank Base")]
    [SerializeField] GameObject rankBase = null;
    [SerializeField] TextMeshProUGUI myRankText_left = null;
    [SerializeField] TextMeshProUGUI myRankText_right = null;
    [SerializeField] TextMeshProUGUI winLoseTxt = null;
    [SerializeField] UIComponent_InGameResult_RankingSlot rankSlotTemplate = null;
    [SerializeField] Transform rankSlotParent = null;

    [Header("Tier Base")]
    [SerializeField] GameObject tierBase = null;
    [SerializeField] RawImage rankTexture;
    [SerializeField] TextMeshProUGUI tierNameTxt;
    [SerializeField] TextMeshProUGUI rankPointTxt_current;
    [SerializeField] TextMeshProUGUI rankPointTxt_min;
    [SerializeField] TextMeshProUGUI rankPointTxt_max;
    [SerializeField] Slider rankPointSlider;

    [Header("Reward Base")]
    [SerializeField] GameObject rewardBase = null;

    [Header("Etc")]
    [SerializeField] GameObject confettiParticleEffect = null;

    private InGame_Quantum.PlayerInfo myPlayerInfo = null;
    private List<InGame_Quantum.PlayerInfo> playerInfoList = new List<InGame_Quantum.PlayerInfo>();

    private List<UIComponent_InGameResult_RankingSlot> slotList = new List<UIComponent_InGameResult_RankingSlot>();

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);

        rankSlotTemplate.SafeSetActive(false);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        if (NetworkManager_Client.Instance.CurrentPlayMode == Quantum.InGamePlayMode.PracticeMode)
        {
            rankBase.SafeSetActive(false);
            tierBase.SafeSetActive(false);
            rewardBase.SafeSetActive(false);
        }
        else
        {
            if (myPlayerInfo != null)
            {
                rankBase.SafeSetActive(true);
                tierBase.SafeSetActive(true);
                rewardBase.SafeSetActive(true);

                SetResultBase();
                SetTierBase();
            }
        }
    }

    public void SetInfo()
    {
        Clear();

        RenderTextureManager.Instance.ActivateRenderTexture_Player(-1, -2); //-1: 현재 장착중인거 -2: 표시x

        var gm = InGame_Quantum.Instance;
        if (gm != null)
        {
            if (gm.myPlayer != null)
            {
                myPlayerInfo = gm.myPlayer;
                playerInfoList = gm.listOfPlayers;
            }
        }
    }

    private void Clear()
    {
        myPlayerInfo = null;
        playerInfoList = null;

        foreach (var i in slotList)
            i.SafeSetActive(false);

        confettiParticleEffect.SafeSetActive(false);
    }

    private void SetResultBase()
    {
        var rt = RenderTextureManager.Instance.RenderTexturePlayerCharacter;
        if (myPlayerInfo != null && playerInfoList != null)
        {
            if (myPlayerInfo.inGamePlayMode == Quantum.InGamePlayMode.SoloMode)
            {
                winLoseTxt.gameObject.SafeSetActive(false);
                myRankText_left.gameObject.SafeSetActive(true);
                myRankText_right.gameObject.SafeSetActive(true);

                myRankText_left.SafeSetText(myPlayerInfo.rank_solo.ToString());
                myRankText_right.SafeSetText(StringManager.GetRankingEndFormat(myPlayerInfo.rank_solo));

                if (rt != null)
                {
                    if (myPlayerInfo.rank_solo <= 2) //2등 이내
                    {
                        rt.ForceAnimState(Unity_PlayerCharacter.AnimState.Win);
                        confettiParticleEffect.SafeSetActive(true);
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_Win);
                    }
                    else if (myPlayerInfo.rank_solo <= 4)  // 3~4등
                    {
                        rt.ForceAnimState(Unity_PlayerCharacter.AnimState.Idle);
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_Win);
                    }
                    else // 5~6등
                    {
                        rt.ForceAnimState(Unity_PlayerCharacter.AnimState.Lose);
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_Lose);
                    }
                }
            }
            else if (myPlayerInfo.inGamePlayMode == Quantum.InGamePlayMode.TeamMode)
            {
                winLoseTxt.gameObject.SafeSetActive(true);
                myRankText_left.gameObject.SafeSetActive(false);
                myRankText_right.gameObject.SafeSetActive(false);

                if (myPlayerInfo.rank_team == 1)
                {
                    winLoseTxt.SafeLocalizeText("COMMON_WIN");
                    if (rt != null)
                    {
                        rt.ForceAnimState(Unity_PlayerCharacter.AnimState.Win);
                        confettiParticleEffect.SafeSetActive(true);
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_Win);
                    }
                }
                else
                {
                    winLoseTxt.SafeLocalizeText("COMMON_LOSE");
                    if (rt != null)
                    {
                        rt.ForceAnimState(Unity_PlayerCharacter.AnimState.Lose);
                        SoundManager.Instance.PlaySound(SoundManager.SoundClip.Ingame_Lose);
                    }
                }
            }

            SetRankList();
        }
        else
        {
            winLoseTxt.gameObject.SafeSetActive(false);
            myRankText_left.gameObject.SafeSetActive(false);
            myRankText_right.gameObject.SafeSetActive(false);
        }
    }

    private void SetRankList()
    {
        foreach (var i in slotList)
        {
            i.SafeSetActive(false);
        }

        if (myPlayerInfo == null || playerInfoList == null)
            return;

        for (int i = 0; i < playerInfoList.Count; i++)
        {
            if (i < slotList.Count)
            {
                //데이터 덮어씌우자
                slotList[i].Setup(playerInfoList[i]);
                slotList[i].SafeSetActive(true);
            }
            else //새로운 슬롯 만들자
            {
                var slot = GameObject.Instantiate(rankSlotTemplate);
                slot.transform.SetParent(rankSlotParent);
                slot.transform.localScale = Vector3.one;
                slot.Setup(playerInfoList[i]);
                slot.SafeSetActive(true);
                slotList.Add(slot);
            }
        }

        if (myPlayerInfo.inGamePlayMode == Quantum.InGamePlayMode.SoloMode)
            slotList.Sort((x, y) => x.SortIndex.CompareTo(y.SortIndex));
        else if (myPlayerInfo.inGamePlayMode == Quantum.InGamePlayMode.TeamMode)
            slotList.Sort((x, y) => x.SortIndex.CompareTo(y.SortIndex));

        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].transform.SetSiblingIndex(i);
        }
    }

    private void SetTierBase()
    {
        var rp = AccountManager.Instance.RankingPoint;
        var tier = AccountManager.Instance.GetRankingTier(rp);
        tierNameTxt.SafeLocalizeText(StringManager.GetRankingTierNameKey(rp));
        rankTexture.texture = ResourceManager.Instance.GetOutGameRankIconByTier(tier);

        rankPointTxt_current.SafeSetText(rp.ToString());
        var rankingDataList = ReferenceManager.Instance.RankingData.RankingTierList;
        if (rankingDataList != null)
        {
            var data = rankingDataList.Find(x => x.TierRange_Min <= rp && x.TierRange_Max >= rp);
            if (data != null)
            {
                rankPointTxt_min.SafeSetText(data.TierRange_Min.ToString());
                rankPointTxt_max.SafeSetText(data.TierRange_Max.ToString());

                rankPointSlider.maxValue = data.TierRange_Max;
                rankPointSlider.minValue = data.TierRange_Min;
                rankPointSlider.value = rp;
            }
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            if (NetworkManager_Client.Instance != null)
            {
                UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                NetworkManager_Client.Instance.LeaveRoom();
            }
        }
    }
}
