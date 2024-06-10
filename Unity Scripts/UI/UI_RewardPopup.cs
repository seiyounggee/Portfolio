using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_RewardPopup : UIBase
{
    [SerializeField] TextMeshProUGUI descTxt;
    [SerializeField] RawImage normalImg = null;
    [SerializeField] RawImage renderImg = null;
    [SerializeField] Button btnOkay = null;
    [SerializeField] UIComponent_Currency currency = null;

    private bool isTweening = false;

    public int currentIndex = 0;
    public int lastIndex = 0;

    //Shop
    RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo shopItemInfo = null;
    List<RefData.Ref_ShopData.Ref_ShopInfo.RewardData> shopDataList = null;

    //Simple


    public enum ShowType
    {
        None,
        Custom,
        ShopReward,
    }
    private ShowType showType;

    private void Awake()
    {
        btnOkay.SafeSetButton(OnClickBtn);
    }

    public override void Show()
    {
        base.Show();

        switch (showType)
        {
            case ShowType.None:
                {
#if UNITY_EDITOR
                    Debug.Log("<color=red>Error...!Show Type is None??</color>");
#endif
                    Hide();
                }
                break;

            case ShowType.Custom:
            case ShowType.ShopReward:
                {
                    ShowData(currentIndex);
                }
                break;
        }
    }

    private void Clear()
    {
        showType = ShowType.None;

        isTweening = false;
        currentIndex = 0;
        lastIndex = 0;
        shopDataList = null;
        shopItemInfo = null;

        btnOkay.SafeSetActive(true);
        currency.SafeSetActive(false);
    }


    public void Setup_Custom(int _rewardID, short _rewardType, int _quantity)
    {
        Clear();

        showType = ShowType.Custom;

        var customData = new RefData.Ref_ShopData.Ref_ShopInfo.RewardData();
        customData.RewardID = _rewardID;
        customData.RewardType = _rewardType;
        customData.Quantity = _quantity;

        shopItemInfo = new RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo();
        shopItemInfo.RewardList = new List<RefData.Ref_ShopData.Ref_ShopInfo.RewardData>();
        shopDataList = shopItemInfo.RewardList;
        shopDataList.Add(customData);
    }

    public void Setup_ShopReward(RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo itemData)
    {
        Clear();

        showType = ShowType.ShopReward;

        shopItemInfo = itemData;
        shopDataList = itemData.RewardList;
        currentIndex = 0;
        lastIndex = shopDataList.Count - 1;
    }

    private void ShowData(int dataIndex)
    {
        if (shopDataList != null && dataIndex < shopDataList.Count)
        {
            var uiTween = PrefabManager.Instance.UI_TweenContainer;
            var singleData = shopDataList[dataIndex];
            descTxt.SafeSetText("x" + StringManager.GetNumberChange(singleData.Quantity));

            switch ((CommonDefine.RewardType)singleData.RewardType)
            {
                case CommonDefine.RewardType.Coin:
                    {
                        //코인하고 Gem은 예외적으로 iteminfo의 아이콘 보여주자..
                        if (shopItemInfo != null)
                        {
                            normalImg.texture = ResourceManager.Instance.GetOutGameIconByItemType((CommonDefine.ItemType)shopItemInfo.ItemType, shopItemInfo.ItemId);
                            normalImg.SetNativeSize();
                            normalImg.SafeSetActive(true);
                            renderImg.SafeSetActive(false);
                        }

                        isTweening = true;
                        currency.UseAutoChange = false;
                        currency.SafeSetActive(true);
                        var texture = ResourceManager.Instance.GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Coin_Default);
                        var startValue = AccountManager.Instance.AccountData.coin - singleData.Quantity;
                        var endValue = AccountManager.Instance.AccountData.coin;
                        currency.SetCurrency(startValue, AccountManager.Instance.AccountData.gem);
                        uiTween.Tween(texture, normalImg.transform, currency.CoinTrans,
                            startValue, endValue,
                            (value) => {
                                currency.SetCurrency(value, AccountManager.Instance.AccountData.gem);
                            },
                            null);
                    }
                    break;
                case CommonDefine.RewardType.Gem:
                    {
                        //코인하고 Gem은 예외적으로 iteminfo의 아이콘 보여주자..
                        if (shopItemInfo != null)
                        {
                            normalImg.texture = ResourceManager.Instance.GetOutGameIconByItemType((CommonDefine.ItemType)shopItemInfo.ItemType, shopItemInfo.ItemId);
                            normalImg.SetNativeSize();
                            normalImg.SafeSetActive(true);
                            renderImg.SafeSetActive(false);
                        }

                        isTweening = true;
                        currency.UseAutoChange = false;
                        currency.SafeSetActive(true);
                        var texture = ResourceManager.Instance.GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Gem_Default);
                        var startValue = AccountManager.Instance.AccountData.gem - singleData.Quantity;
                        var endValue = AccountManager.Instance.AccountData.gem;
                        currency.SetCurrency(AccountManager.Instance.AccountData.coin, startValue);
                        uiTween.Tween(texture, normalImg.transform, currency.GemTrans,
                            startValue, endValue,
                            (value) => {
                                currency.SetCurrency(AccountManager.Instance.AccountData.coin, value);
                            },
                            null);

                    }
                    break;

                case CommonDefine.RewardType.CharacterSkin:
                    {
                        //normalImg.texture = ResourceManager.Instance.GetCharacterSkinIcon(singleData.RewardID);
                        normalImg.SafeSetActive(false);

                        RenderTextureManager.Instance.ActivateRenderTexture_Player(singleData.RewardID);
                        renderImg.SafeSetActive(true);
                    }
                    break;

                case CommonDefine.RewardType.WeaponSkin:
                    {
                        //normalImg.texture = ResourceManager.Instance.GetWeaponSkinIcon(singleData.RewardID);
                        normalImg.SafeSetActive(false);

                        RenderTextureManager.Instance.ActivateRenderTexture_Weapon(singleData.RewardID);
                        renderImg.SafeSetActive(true);
                    }
                    break;

                case CommonDefine.RewardType.ActiveSkill:
                    {
                        normalImg.texture = ResourceManager.Instance.GetActiveSkillIcon(singleData.RewardID);
                        normalImg.SetNativeSize();
                        normalImg.SafeSetActive(true);

                        renderImg.SafeSetActive(false);
                    }
                    break;

                case CommonDefine.RewardType.PassiveSkill:
                    {
                        normalImg.texture = ResourceManager.Instance.GetPassiveSkillIcon(singleData.RewardID);
                        normalImg.SetNativeSize();
                        normalImg.SafeSetActive(true);

                        renderImg.SafeSetActive(false);
                    }
                    break;

                default:
                    {
                        normalImg.SafeSetActive(false);
                        renderImg.SafeSetActive(false);
                    }
                    break;
            }
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == btnOkay)
        {
            switch (showType)
            {
                case ShowType.Custom:
                    {
                        PrefabManager.Instance.UI_TweenContainer.StopTween();
                        Clear();
                        Hide();
                    }
                    break;

                case ShowType.ShopReward:
                    {
                        if (currentIndex < lastIndex)
                        {
                            ++currentIndex;
                            ShowData(currentIndex);
                        }
                        else
                        {
                            PrefabManager.Instance.UI_TweenContainer.StopTween();
                            Clear();
                            Hide();
                        }
                    }
                    break;
            }


        }
    }
}
