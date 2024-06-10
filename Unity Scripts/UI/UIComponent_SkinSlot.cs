using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_SkinSlot : MonoBehaviour
{
    [SerializeField] RawImage skinImage = null;
    [SerializeField] Button skinBtn = null;
    [SerializeField] Button selectBtn = null;
    [SerializeField] Button selectedBtn = null;
    [SerializeField] Button lockedBtn = null;

    [SerializeField] Button buyBtn = null;
    [SerializeField] GameObject buyDisabledObj = null;
    [SerializeField] TextMeshProUGUI costTxt = null;
    [SerializeField] RawImage currencyImg = null;

    private int skinID = 0;
    private CommonDefine.SkinType skinType = CommonDefine.SkinType.None;
    RefData.Ref_SkinData.Ref_SkinInfo skinInfo = null;

    private void Awake()
    {
        skinBtn.SafeSetButton(OnClickBtn);
        selectBtn.SafeSetButton(OnClickBtn);
        selectedBtn.SafeSetButton(OnClickBtn);
        lockedBtn.SafeSetButton(OnClickBtn);
        buyBtn.SafeSetButton(OnClickBtn);
    }

    public void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        SetBtn();
    }

    public void SetData_Character(RefData.Ref_SkinData.Ref_SkinInfo info)
    {
        skinInfo = info;
        skinType = CommonDefine.SkinType.Character;
        skinID = info.SkinID;

        skinImage.texture = ResourceManager.Instance.GetCharacterSkinIcon(info.SkinID);

        costTxt.SafeSetText(StringManager.GetNumberChange(info.Price));
        currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)info.PurchaseCurrencyType);
    }

    public void SetData_Weapon(RefData.Ref_SkinData.Ref_SkinInfo info)
    {
        skinInfo = info;
        skinType = CommonDefine.SkinType.Weapon;
        skinID = info.SkinID;

        skinImage.texture = ResourceManager.Instance.GetWeaponSkinIcon(info.SkinID);

        costTxt.SafeSetText(StringManager.GetNumberChange(info.Price));
        currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)info.PurchaseCurrencyType);
    }

    private void SetBtn()
    {
        var acc = AccountManager.Instance.AccountData;
        switch (skinType)
        {
            case CommonDefine.SkinType.Character:
                {
                    lockedBtn.SafeSetActive(acc.ownedCharacterSkin.Contains(skinID) == false);
                    selectBtn.SafeSetActive(acc.characterSkinID != skinID && acc.ownedCharacterSkin.Contains(skinID));
                    selectedBtn.SafeSetActive(acc.characterSkinID == skinID);
                    buyBtn.SafeSetActive(acc.ownedCharacterSkin.Contains(skinID) == false && skinInfo.PurchaseCurrencyType != (int)CommonDefine.CurrencyType.Unavailable);

                }
                break;

            case CommonDefine.SkinType.Weapon:
                {
                    lockedBtn.SafeSetActive(acc.ownedWeaponSkin.Contains(skinID) == false);
                    selectBtn.SafeSetActive(acc.weaponSkinID != skinID && acc.ownedWeaponSkin.Contains(skinID));
                    selectedBtn.SafeSetActive(acc.weaponSkinID == skinID);
                    buyBtn.SafeSetActive(acc.ownedWeaponSkin.Contains(skinID) == false && skinInfo.PurchaseCurrencyType != (int)CommonDefine.CurrencyType.Unavailable);
                }
                break;
        }
    }

    private void OnClickBtn(Button btn)
    {
        var skinUI = PrefabManager.Instance.UI_Skin;

        if (btn == skinBtn || btn == lockedBtn)
        {
            skinUI.OnClickSkinSlot_Focus(skinType, skinID);
            Refresh();
        }
        else if (btn == selectBtn)
        {
            skinUI.OnClickSkinSlot_Focus(skinType, skinID);
            skinUI.OnClickSkinSlot_Select(skinType, skinID);
            Refresh();
        }
        else if (btn == selectedBtn)
        {
            skinUI.OnClickSkinSlot_Focus(skinType, skinID);
            Refresh();
        }
        else if (btn == buyBtn)
        {
            var skinData = ReferenceManager.Instance.SkinData;

            if (skinData == null)
                return;

            var uiBuy = PrefabManager.Instance.UI_BuyPopup;

            switch (skinType)
            {
                case CommonDefine.SkinType.Character:
                    {
                        var list_char = ResourceManager.Instance.CharacterSkinDataList;
                        var resource_data = list_char.Find(x => x.id.Equals(skinID));
                        var ref_data = skinData.CharacterSkinList.Find(x => x.SkinID.Equals(skinID));
                        if (resource_data != null && ref_data != null)
                        {
                            var desc = ref_data.SkinName;
                            var currencyType = (CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType;
                            var currencyIcon = ResourceManager.Instance.GetCurrencyIcon(currencyType);
                            uiBuy.Setup(desc, resource_data.texture, currencyIcon, ref_data.Price, currencyType, () =>
                            {
                                BillingManager.Instance.PurchaseSkin(skinType, ref_data, (isSuccess) =>
                                {
                                    UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
                                    if (isSuccess)
                                    {
                                        //备概己傍
                                        var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                                        rewardUI.Setup_Custom(ref_data.SkinID, (short)CommonDefine.RewardType.CharacterSkin, 1);
                                        UIManager.Instance.ShowUI(rewardUI);
                                    }
                                });
                            });
                        }
                    }
                    break;
                case CommonDefine.SkinType.Weapon:
                    {
                        var list_weapon = ResourceManager.Instance.WeaponSkinDataList;
                        var resource_data = list_weapon.Find(x => x.id.Equals(skinID));
                        var ref_data = skinData.WeaponSkinList.Find(x => x.SkinID.Equals(skinID));
                        if (resource_data != null && ref_data != null)
                        {
                            var desc = ref_data.SkinName;
                            var currencyType = (CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType;
                            var currencyIcon = ResourceManager.Instance.GetCurrencyIcon(currencyType);
                            uiBuy.Setup(desc, resource_data.texture, currencyIcon, ref_data.Price, currencyType, () =>
                            {
                                BillingManager.Instance.PurchaseSkin(skinType, ref_data, (isSuccess) =>
                                {
                                    UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
                                    if (isSuccess)
                                    {
                                        //备概己傍
                                        var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                                        rewardUI.Setup_Custom(ref_data.SkinID, (short)CommonDefine.RewardType.WeaponSkin, 1);
                                        UIManager.Instance.ShowUI(rewardUI);
                                    }
                                });
                            });
                        }
                    }
                    break;
            }

            UIManager.Instance.ShowUI(uiBuy);
            skinUI.OnClickSkinSlot_Focus(skinType, skinID);
            Refresh();
        }
    }
}
