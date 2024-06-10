using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_Stats_Slot : MonoBehaviour
{
    [SerializeField] RawImage statImg = null;
    [SerializeField] TextMeshProUGUI nameTxt = null;
    [SerializeField] TextMeshProUGUI levelTxt = null;

    [SerializeField] Transform barGrid = null;
    [SerializeField] GameObject bar_on = null;
    [SerializeField] GameObject bar_off = null;

    [SerializeField] Button upgradeBtn = null;
    [SerializeField] RawImage currencyImg = null;
    [SerializeField] TextMeshProUGUI priceTxt = null;
    [SerializeField] TextMeshProUGUI probabilityTxt = null;
    [SerializeField] GameObject deactivatedObj = null;

    [SerializeField] Button maxBtn = null;

    RefData.Ref_PlayerBasicStats.Ref_PlayerUpgradeStatsInfo statsInfo = null;

    private List<GameObject> onBarList = new List<GameObject>();
    private List<GameObject> offBarList = new List<GameObject>();

    private void Awake()
    {
        upgradeBtn.SafeSetButton(OnClickBtn);
        maxBtn.SafeSetButton(OnClickBtn);

        bar_on.SafeSetActive(false);
        bar_off.SafeSetActive(false);
    }

    public void Setup(RefData.Ref_PlayerBasicStats.Ref_PlayerUpgradeStatsInfo info)
    {
        statsInfo = info;

        nameTxt.SafeLocalizeText(info.statsName);
        levelTxt.SafeSetText("Lv." + (info.level + 1));
        statImg.texture = ResourceManager.Instance.GetStatsIcon((CommonDefine.PlayerStatsType)info.type);

        var totalList = ReferenceManager.Instance.PlayerBasicStats.upgradeStatsList;
        short nextLevel = info.level;
        ++nextLevel; //꼭 값복사 하자...

        var nextInfo = totalList.Find(x => x.type.Equals(info.type) && x.level.Equals(nextLevel));

        if (nextInfo != null)
        {
            upgradeBtn.SafeSetActive(true);
            maxBtn.SafeSetActive(false);

            if (IsHaveEnoughCurrency((CommonDefine.CurrencyType)nextInfo.PurchaseCurrencyType, nextInfo.Price))
                deactivatedObj.SafeSetActive(false);
            else
                deactivatedObj.SafeSetActive(true);

            priceTxt.SafeSetText(StringManager.GetNumberChange(nextInfo.Price));
            probabilityTxt.SafeSetText(nextInfo.SuccessProbability + "%");
            currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)nextInfo.PurchaseCurrencyType);
        }
        else //MAX 도달
        {
            upgradeBtn.SafeSetActive(false);
            maxBtn.SafeSetActive(true);
        }

        if (onBarList != null && onBarList.Count == 0 && offBarList != null && offBarList.Count == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                var go_on = Instantiate(bar_on);
                go_on.transform.SetParent(barGrid);
                go_on.transform.localScale = Vector3.one;
                onBarList.Add(go_on);

                var go_off = Instantiate(bar_off);
                go_off.transform.SetParent(barGrid);
                go_off.transform.localScale = Vector3.one;
                offBarList.Add(go_off);
            }
        }

        for (int i = 0; i < onBarList.Count; i++)
        {
            onBarList[i].SafeSetActive(i <= info.level);
        }

        for (int i = 0; i < offBarList.Count; i++)
        {
            offBarList[i].SafeSetActive(i > info.level);
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == upgradeBtn)
        {
            if (statsInfo == null)
                return;

            var totalList = ReferenceManager.Instance.PlayerBasicStats.upgradeStatsList;
            short nextLevel = statsInfo.level;
            ++nextLevel; //꼭 값복사 하자...

            var nextInfo = totalList.Find(x => x.type.Equals(statsInfo.type) && x.level.Equals(nextLevel));
            if (nextInfo != null)
            {
                if (IsHaveEnoughCurrency((CommonDefine.CurrencyType)nextInfo.PurchaseCurrencyType, nextInfo.Price))
                {
                    BillingManager.Instance.PurchaseStats(nextInfo, OnPurchaseCallback);
                }
                else
                {
                    var ui = PrefabManager.Instance.UI_ToastMessage;
                    StringManager.GetLocailzedText(StringManager.GetCurrencyKey((CommonDefine.CurrencyType)nextInfo.PurchaseCurrencyType), (isSuccess, result)=>
                    {
                        if (isSuccess)
                        {
                            ui.SetMessage("OUTGAME_NOT_ENOUGH", result);
                            ui.Show();
                        }
                    });
                }
            }
        }
        else if (btn == maxBtn)
        { 
        
        }
    }

    private void OnPurchaseCallback(BillingManager.StatsPurchaseResult result)
    {
        UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);

        //TODO... 연출 관련...

        switch (result)
        {
            case BillingManager.StatsPurchaseResult.Probability_Success:
                {
                    var ui = PrefabManager.Instance.UI_ToastMessage;
                    ui.SetMessage("Upgrade Success!!");
                    ui.Show();
                }
                break;
            case BillingManager.StatsPurchaseResult.Probability_Fail:
                {
                    var ui = PrefabManager.Instance.UI_ToastMessage;
                    ui.SetMessage("Bad Luck... Upgrade Failed!!");
                    ui.Show();
                }
                break;
            case BillingManager.StatsPurchaseResult.Network_Fail:
                {
                    var commonUI = PrefabManager.Instance.UI_CommonPopup;
                    commonUI.SetUp(UI_CommonPopup.CommonPopupType.Okay_WithExit, "ERROR_TITLE", "ERROR_PURCHASE_FAILED", null);
                    UIManager.Instance.ShowUI(commonUI);
                }
                break;
        }
    }

    private bool IsHaveEnoughCurrency(CommonDefine.CurrencyType type, int price)
    {
        var myData = AccountManager.Instance.AccountData;

        switch (type)
        {
            case CommonDefine.CurrencyType.Coin:
                return (myData.coin >= price);
            case CommonDefine.CurrencyType.Gem:
                return (myData.gem >= price);
            default:
            case CommonDefine.CurrencyType.IAP:
                return true;
        }
    }
}
