using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;


public class UIComponent_SkillSlot : MonoBehaviour
{
    [SerializeField] RawImage skillImage = null;
    [SerializeField] Button skillBtn = null;
    [SerializeField] Button selectBtn = null;
    [SerializeField] Button selectedBtn = null;
    [SerializeField] Button lockedBtn = null;

    [SerializeField] Button buyBtn = null;
    [SerializeField] GameObject buyDisabledObj = null;
    [SerializeField] TextMeshProUGUI costTxt = null;
    [SerializeField] RawImage currencyImg = null;

    private int skillID = 0;
    private CommonDefine.SkillType skillType = CommonDefine.SkillType.None;
    RefData.Ref_SkillData.Ref_ActiveSkillInfo skillInfo_active = null;
    RefData.Ref_SkillData.Ref_PassiveSkillInfo skillInfo_passive = null;

    private void Awake()
    {
        skillBtn.SafeSetButton(OnClickBtn);
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

    public void SetData_ActiveSkillInfo(RefData.Ref_SkillData.Ref_ActiveSkillInfo info)
    {
        skillInfo_active = info;
        skillType = CommonDefine.SkillType.Active;
        skillID = info.SkillID;

        skillImage.texture = ResourceManager.Instance.GetActiveSkillIcon(info.SkillID);

        costTxt.SafeSetText(StringManager.GetNumberChange(info.Price));
        currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)info.PurchaseCurrencyType);
    }

    public void SetData_PassiveSkillInfo(RefData.Ref_SkillData.Ref_PassiveSkillInfo info)
    {
        skillInfo_passive = info;
        skillType = CommonDefine.SkillType.Passive;
        skillID = info.SkillID;

        skillImage.texture = ResourceManager.Instance.GetPassiveSkillIcon(info.SkillID);

        costTxt.SafeSetText(StringManager.GetNumberChange(info.Price));
        currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)info.PurchaseCurrencyType);
    }

    private void SetBtn()
    {
        var acc = AccountManager.Instance.AccountData;
        switch (skillType)
        {
            case CommonDefine.SkillType.Active:
                {
                    lockedBtn.SafeSetActive(acc.ownedActiveSkill.Contains(skillID) == false);
                    selectBtn.SafeSetActive(acc.activeSkillID != skillID && acc.ownedActiveSkill.Contains(skillID));
                    selectedBtn.SafeSetActive(acc.activeSkillID == skillID);
                    buyBtn.SafeSetActive(acc.ownedActiveSkill.Contains(skillID) == false && skillInfo_active.PurchaseCurrencyType != (int)CommonDefine.CurrencyType.Unavailable);
                }
                break;

            case CommonDefine.SkillType.Passive:
                {
                    lockedBtn.SafeSetActive(acc.ownedPassiveSkill.Contains(skillID) == false);
                    selectBtn.SafeSetActive(acc.passiveSkillID != skillID && acc.ownedPassiveSkill.Contains(skillID));
                    selectedBtn.SafeSetActive(acc.passiveSkillID == skillID);
                    buyBtn.SafeSetActive(acc.ownedPassiveSkill.Contains(skillID) == false && skillInfo_passive.PurchaseCurrencyType != (int)CommonDefine.CurrencyType.Unavailable);
                }
                break;
        }
    }

    private void OnClickBtn(Button btn)
    {
        var skillUI = PrefabManager.Instance.UI_Skill;

        if (btn == skillBtn || btn == lockedBtn)
        {
            skillUI.OnClickSkillSlot_Focus(skillType, skillID);
            Refresh();
        }
        else if (btn == selectBtn)
        {
            skillUI.OnClickSkillSlot_Focus(skillType, skillID);
            skillUI.OnClickSkillSlot_Select(skillType, skillID);
            Refresh();
        }
        else if (btn == selectedBtn)
        {
            skillUI.OnClickSkillSlot_Focus(skillType, skillID);
            Refresh();
        }
        else if (btn == buyBtn)
        {
            var skillData = ReferenceManager.Instance.SkillData;

            if (skillData == null)
                return;

            var uiBuy = PrefabManager.Instance.UI_BuyPopup;

            switch (skillType)
            {
                case CommonDefine.SkillType.Active:
                    {
                        var list_active = ResourceManager.Instance.ActiveSkillDataList;
                        var resource_data = list_active.Find(x => x.id.Equals(skillID));
                        var ref_data = skillData.ActiveSkillInfoList.Find(x => x.SkillID.Equals(skillID));
                        if (resource_data != null && ref_data != null)
                        {
                            var desc = ref_data.skillName;
                            var currencyType = (CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType;
                            var currencyIcon = ResourceManager.Instance.GetCurrencyIcon(currencyType);
                            uiBuy.Setup(desc, resource_data.texture, currencyIcon, ref_data.Price, currencyType, ()=>
                            {
                                BillingManager.Instance.PurchaseSkill_Active(CommonDefine.SkillType.Active, ref_data, (isSuccess)=>
                                {
                                    UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
                                    if (isSuccess)
                                    {
                                        //备概己傍
                                        var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                                        rewardUI.Setup_Custom(ref_data.SkillID, (short)CommonDefine.RewardType.ActiveSkill, 1);
                                        UIManager.Instance.ShowUI(rewardUI);
                                    }
                                });
                            });
                        }
                    }
                    break;
                case CommonDefine.SkillType.Passive:
                    {
                        var list_passive = ResourceManager.Instance.PassiveSkillDataList;
                        var resource_data = list_passive.Find(x => x.id.Equals(skillID));
                        var ref_data = skillData.PassiveSkillInfoList.Find(x => x.SkillID.Equals(skillID));
                        if (resource_data != null && ref_data != null)
                        {
                            var desc = ref_data.skillName;
                            var currencyType = (CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType;
                            var currencyIcon = ResourceManager.Instance.GetCurrencyIcon(currencyType);
                            uiBuy.Setup(desc, resource_data.texture, currencyIcon, ref_data.Price, currencyType, () =>
                            {
                                BillingManager.Instance.PurchaseSkill_Passive(CommonDefine.SkillType.Passive, ref_data, (isSuccess) =>
                                {
                                    UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
                                    if (isSuccess)
                                    {
                                        //备概己傍
                                        var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                                        rewardUI.Setup_Custom(ref_data.SkillID, (short)CommonDefine.RewardType.PassiveSkill, 1);
                                        UIManager.Instance.ShowUI(rewardUI);
                                    }
                                });
                            });
                        }
                    }
                    break;
            }

            UIManager.Instance.ShowUI(uiBuy);
            skillUI.OnClickSkillSlot_Focus(skillType, skillID);
            Refresh();
        }
    }

}
