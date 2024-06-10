using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BuyPopup : UIBase, IBackButtonHandler
{
    [SerializeField] Button exitBtn = null;
    [SerializeField] Button buyBtn = null;

    [SerializeField] GameObject disabledBg = null;

    [SerializeField] RawImage itemImg = null;
    [SerializeField] TextMeshProUGUI descTxt = null;

    [SerializeField] TextMeshProUGUI costTxt = null;
    [SerializeField] RawImage currencyImg = null;

    public Action buyCallback = null;

    CommonDefine.CurrencyType currencytype;
    int itemCost;

    private void Awake()
    {
        exitBtn.SafeSetButton(OnClickBtn);
        buyBtn.SafeSetButton(OnClickBtn);
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

    public void Setup(string descMsg, Texture2D itemTexture, Texture2D currencyTexture, int cost, CommonDefine.CurrencyType type, Action _buyCallback)
    {
        currencytype = type;
        itemCost = cost;

        descTxt.SafeLocalizeText(descMsg);

        itemImg.texture = itemTexture;
        currencyImg.texture = currencyTexture;

        costTxt.SafeSetText(StringManager.GetNumberChange(cost));

        disabledBg.SafeSetActive(false); //ÀÏ´ÜÀº..
        buyCallback = _buyCallback;
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == exitBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
        else if (btn == buyBtn)
        {
            if (IsHaveEnoughCurrency(currencytype, itemCost) == false)
            {
                var ui = PrefabManager.Instance.UI_ToastMessage;
                StringManager.GetLocailzedText(StringManager.GetCurrencyKey(currencytype), (isSuccess, result) =>
                {
                    if (isSuccess)
                    {
                        ui.SetMessage("OUTGAME_NOT_ENOUGH", result);
                        ui.Show();
                    }
                });
                return;
            }

            buyCallback?.Invoke();
            Hide();
        }
    }

    public void OnBackButton()
    {
        Hide();
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
