using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_RankChangePopup : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;

    [Header("Demotion")]
    [SerializeField] GameObject demotionBase = null;
    [SerializeField] RawImage demotion_prevRankImg = null;
    [SerializeField] RawImage demotion_currRankImg = null;
    [SerializeField] TextMeshProUGUI demotion_prevRankTxt = null;
    [SerializeField] TextMeshProUGUI demotion_currRankTxt = null;
    [SerializeField] TextMeshProUGUI demotion_descTxt = null;

    [Header("Promotion")]
    [SerializeField] GameObject promotionBase = null;
    [SerializeField] RawImage promotion_currRankImg = null;
    [SerializeField] TextMeshProUGUI promotion_currRankTxt = null;
    [SerializeField] TextMeshProUGUI promotion_descTxt = null;

    private bool isPromoted = false;

    [ReadOnly] public bool IsReady = false;

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);

        demotionBase.SafeSetActive(false);
        promotionBase.SafeSetActive(false);
    }
    internal override void OnEnable()
    {
        base.OnEnable();

        UIManager.Instance.RegisterBackButton(this);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        UIManager.Instance.UnregisterBackButton(this);
    }

    public override void Hide()
    {
        base.Hide();

        IsReady = false;
    }

    public void Setup(bool _isPromoted)
    {
        isPromoted = _isPromoted;
        IsReady = true;

        if (isPromoted)
        {
            promotionBase.SafeSetActive(true);
            demotionBase.SafeSetActive(false);

            SetUI_Promoted();
        }
        else
        {
            demotionBase.SafeSetActive(true);
            promotionBase.SafeSetActive(false);

            SetUI_Demoted();
        }
    }

    private void SetUI_Promoted()
    {
        var currentRP = AccountManager.Instance.RankingPoint;
        var currentTier = AccountManager.Instance.GetRankingTier(currentRP);

        promotion_currRankTxt.SafeLocalizeText(StringManager.GetRankingTierNameKey(currentTier));
        promotion_currRankImg.texture = ResourceManager.Instance.GetOutGameRankIconByTier(currentTier);

        promotion_descTxt.SafeLocalizeText("RANK_PROMOTION_DESC");
    }

    private void SetUI_Demoted()
    {
        var currentRP = AccountManager.Instance.RankingPoint;
        var currentTier = AccountManager.Instance.GetRankingTier(currentRP);
        var previousTier = AccountManager.Instance.GetNextHigherRankingTier(currentRP);

        demotion_prevRankTxt.SafeLocalizeText(StringManager.GetRankingTierNameKey(previousTier));
        demotion_currRankTxt.SafeLocalizeText(StringManager.GetRankingTierNameKey(currentTier));

        demotion_prevRankImg.texture = ResourceManager.Instance.GetOutGameRankIconByTier(previousTier);
        demotion_currRankImg.texture = ResourceManager.Instance.GetOutGameRankIconByTier(currentTier);

        demotion_descTxt.SafeLocalizeText("RANK_DEMOTION_DESC");
    }

    public void OnBackButton()
    {
        Hide();
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            Hide();
        }
    }
}
