using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UI_Skill : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;
    [SerializeField] UIComponent_Currency currency;

    [Header("Left Group")]
    [SerializeField] RawImage currentSkillImage = null;
    [SerializeField] TextMeshProUGUI skillNameTxt = null;
    [SerializeField] TextMeshProUGUI skillDescTxt = null;
    [SerializeField] TextMeshProUGUI skillRarityTxt = null;
    [SerializeField] Image skillRarityImg = null;
    [SerializeField] TextMeshProUGUI skillTypeTxt = null;
    [SerializeField] Image skillTypeImg = null;
    [SerializeField] Button buyBtn = null;
    [SerializeField] GameObject buyDisabledObj = null;
    [SerializeField] TextMeshProUGUI costTxt = null;
    [SerializeField] RawImage currencyImg = null;

    [Header("Right Group")]
    [SerializeField] Button passiveSkillBtn = null;
    [SerializeField] Button activeSkillBtn = null;
    [SerializeField] UIButton_StateVisualChanger passiveSkillBtn_stateChanger = null;
    [SerializeField] UIButton_StateVisualChanger activeSkillBtn_stateChanger = null;

    [SerializeField] GameObject skillListParentObj = null;
    [SerializeField] UIComponent_SkillSlot skillSlotTemplate = null;

    private List<UIComponent_SkillSlot> skillSlotList = new List<UIComponent_SkillSlot>();

    private CommonDefine.SkillType currentSelectedSkillType = CommonDefine.SkillType.None;
    private int currentSelectedSkillID = 0;

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);
        passiveSkillBtn.SafeSetButton(OnClickBtn);
        activeSkillBtn.SafeSetButton(OnClickBtn);
        buyBtn.SafeSetButton(OnClickBtn);

        skillSlotTemplate.SafeSetActive(false);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        AccountManager.Instance.OnValueChangedAccountData += OnValueChangedAccountData;

        UIManager.Instance.RegisterBackButton(this);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        AccountManager.Instance.OnValueChangedAccountData -= OnValueChangedAccountData;

        UIManager.Instance.UnregisterBackButton(this);
    }

    public override void Show()
    {
        base.Show();

        if (currentSelectedSkillType == CommonDefine.SkillType.None)
            SetSelectedData(CommonDefine.SkillType.Active);

        SetSelectedSkill();
        SetSkillList();
        SetCurrency();
        SetTabMenuBtn();
    }

    public override void Hide()
    {
        base.Hide();

        ClearData();
    }

    private void ClearData()
    {
        currentSelectedSkillType = CommonDefine.SkillType.None;
        currentSelectedSkillID = 0;
    }

    public void SetSelectedData(CommonDefine.SkillType type, int skillId = -1)
    {
        currentSelectedSkillType = type;

        if (skillId >= 0)
        {
            currentSelectedSkillID = skillId;
        }
        else
        {
            var accData = AccountManager.Instance.AccountData;
            var skillData = ReferenceManager.Instance.SkillData;
            if (accData != null && skillData != null 
                && skillData.ActiveSkillInfoList != null && skillData.ActiveSkillInfoList.Count >= 2
                && skillData.PassiveSkillInfoList != null && skillData.PassiveSkillInfoList.Count >= 2)
            {
                switch (currentSelectedSkillType)
                {
                    case CommonDefine.SkillType.Active:
                        {
                            currentSelectedSkillID = accData.activeSkillID;

                            if (currentSelectedSkillID.Equals((int)Quantum.PlayerActiveSkill.None))
                                currentSelectedSkillID = (int)Quantum.PlayerActiveSkill.None + 1;
                            
                        }
                        break;
                    case CommonDefine.SkillType.Passive:
                        {
                            currentSelectedSkillID = accData.passiveSkillID;

                            if (currentSelectedSkillID.Equals((int)Quantum.PlayerPassiveSkill.None))
                                currentSelectedSkillID = (int)Quantum.PlayerPassiveSkill.None + 1;
                        }
                        break;
                }
            }
        }
    }

    private void SetSelectedSkill()
    {
        var accountData = AccountManager.Instance.AccountData;
        var skillData = ReferenceManager.Instance.SkillData;
        if (skillData != null && accountData != null)
        {
            switch (currentSelectedSkillType)
            {
                case CommonDefine.SkillType.Active:
                    {
                        var list_active = ResourceManager.Instance.ActiveSkillDataList;
                        var resource_data = list_active.Find(x => x.id.Equals(currentSelectedSkillID));
                        var ref_data = skillData.ActiveSkillInfoList.Find(x => x.SkillID.Equals(currentSelectedSkillID));

                        if (resource_data != null && ref_data != null)
                        {
                            currentSkillImage.texture = resource_data.texture;
                            skillNameTxt.SafeLocalizeText(ref_data.skillName);
                            skillDescTxt.SafeLocalizeText(StringManager.GetActiveSkillDescKey(ref_data.type));
                            skillRarityTxt.SafeLocalizeText(StringManager.GetRarityKey(ref_data.Rarity));
                            skillRarityImg.SafeColor(UIManager.GetRarityColor(ref_data.Rarity));
                            skillTypeTxt.SafeLocalizeText(StringManager.GetSkillTypeKey(CommonDefine.SkillType.Active));
                            skillTypeImg.SafeColor(UIManager.GetSkillTypeColor(CommonDefine.SkillType.Active));

                            /*
                            buyBtn.SafeSetActive(accountData.ownedActiveSkill.Contains(currentSelectedSkillID) == false);
                            costTxt.SafeSetText(StringManager.GetNumberChange(ref_data.Price));
                            currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType);
                            */                            
                        }
                    }
                    break;

                case CommonDefine.SkillType.Passive:
                    {
                        var list_passive = ResourceManager.Instance.PassiveSkillDataList;
                        var resource_data = list_passive.Find(x => x.id.Equals(currentSelectedSkillID));
                        var ref_data = skillData.PassiveSkillInfoList.Find(x => x.SkillID.Equals(currentSelectedSkillID));

                        if (resource_data != null && ref_data != null)
                        {
                            currentSkillImage.texture = resource_data.texture;
                            skillNameTxt.SafeLocalizeText(ref_data.skillName);
                            skillDescTxt.SafeLocalizeText(StringManager.GetPassiveSkillDescKey(ref_data.type));
                            skillRarityTxt.SafeLocalizeText(StringManager.GetRarityKey(ref_data.Rarity));
                            skillRarityImg.SafeColor(UIManager.GetRarityColor(ref_data.Rarity));
                            skillTypeTxt.SafeLocalizeText(StringManager.GetSkillTypeKey(CommonDefine.SkillType.Passive));
                            skillTypeImg.SafeColor(UIManager.GetSkillTypeColor(CommonDefine.SkillType.Passive));

                            /*
                            buyBtn.SafeSetActive(accountData.ownedPassiveSkill.Contains(currentSelectedSkillID) == false);
                            costTxt.SafeSetText(StringManager.GetNumberChange(ref_data.Price));
                            currencyImg.texture = ResourceManager.Instance.GetCurrencyIcon((CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType);
                            */                            
                        }
                    }
                    break;
            }
        }
    }

    private void SetSkillList()
    {
        var rt = skillListParentObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            //맨위 쪽으로 고정...?
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0f);
        }

        foreach (var i in skillSlotList)
        {
            i.gameObject.SafeSetActive(false);
        }

        var skillData = ReferenceManager.Instance.SkillData;
        if (skillData != null)
        {
            switch(currentSelectedSkillType)
            {
                case CommonDefine.SkillType.Active:
                    {
                        var skillListToShow = skillData.ActiveSkillInfoList;
                        for (int i = 0; i < skillListToShow.Count; i++)
                        {
                            if (skillListToShow[i].type == (int)Quantum.PlayerActiveSkill.None)
                                continue;

                            if (i < skillSlotList.Count)
                            {
                                skillSlotList[i].SetData_ActiveSkillInfo(skillListToShow[i]);
                                skillSlotList[i].SafeSetActive(true);
                            }
                            else
                            {
                                var slot = GameObject.Instantiate(skillSlotTemplate);
                                slot.transform.SetParent(skillListParentObj.transform);
                                slot.transform.localScale = Vector3.one;
                                slot.SetData_ActiveSkillInfo(skillListToShow[i]);
                                slot.SafeSetActive(true);
                                skillSlotList.Add(slot);
                            }
                        }
                    }
                    break;

                case CommonDefine.SkillType.Passive:
                    {
                        var skillListToShow = skillData.PassiveSkillInfoList;
                        for (int i = 0; i < skillListToShow.Count; i++)
                        {
                            if (skillListToShow[i].type == (int)Quantum.PlayerPassiveSkill.None)
                                continue;

                            if (i < skillSlotList.Count)
                            {
                                skillSlotList[i].SetData_PassiveSkillInfo(skillListToShow[i]);
                                skillSlotList[i].SafeSetActive(true);
                            }
                            else
                            {
                                var slot = GameObject.Instantiate(skillSlotTemplate);
                                slot.transform.SetParent(skillListParentObj.transform);
                                slot.transform.localScale = Vector3.one;
                                slot.SetData_PassiveSkillInfo(skillListToShow[i]);
                                slot.SafeSetActive(true);
                                skillSlotList.Add(slot);
                            }
                        }
                    }
                    break;
            }
        }
    }

    public void OnClickSkillSlot_Focus(CommonDefine.SkillType type, int skillID)
    {
        SetSelectedData(type, skillID);

        SetSelectedSkill();
    }

    public void OnClickSkillSlot_Select(CommonDefine.SkillType type, int skillID)
    {
        AccountManager.Instance.Save_SkillData(type, skillID, (isSuccess)=>
        {
            if (isSuccess)
            {
                /*
                var ui = PrefabManager.Instance.UI_ToastMessage;
                ui.SetMessage("Succesfully Equipped!");
                ui.Show();
                */
            }
            else
            {
                var ui = PrefabManager.Instance.UI_CommonPopup;
                ui.SetUp(UI_CommonPopup.CommonPopupType.Okay_WithExit, "ERROR_TITLE", "ERROR_SKILL_SELECTION", null);
                UIManager.Instance.ShowUI(ui);
            }
        });
    }

    private void SetTabMenuBtn()
    {
        switch (currentSelectedSkillType)
        {
            case CommonDefine.SkillType.Passive:
                {
                    passiveSkillBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Focused);
                    activeSkillBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Unfocused);
                }
                break;
            case CommonDefine.SkillType.Active:
                {
                    passiveSkillBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Unfocused);
                    activeSkillBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Focused);
                }
                break;
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
        else if (btn == passiveSkillBtn)
        {
            SetSelectedData(CommonDefine.SkillType.Passive);
            SetSelectedSkill();
            SetSkillList();
            SetTabMenuBtn();
        }
        else if (btn == activeSkillBtn)
        {
            SetSelectedData(CommonDefine.SkillType.Active);
            SetSelectedSkill();
            SetSkillList();
            SetTabMenuBtn();
        }
        else if (btn == buyBtn)
        {
            return;
            //잠시 사용x...
            /*
            var skillData = ReferenceManager.Instance.SkillData;

            if (skillData == null)
                return;

            var uiBuy = PrefabManager.Instance.UI_BuyPopup;

            switch (currentSelectedSkillType)
            {
                case CommonDefine.SkillType.Active:
                    {
                        var list_active = ResourceManager.Instance.ActiveSkillDataList;
                        var resource_data = list_active.Find(x => x.id.Equals(currentSelectedSkillID));
                        var ref_data = skillData.ActiveSkillInfoList.Find(x => x.SkillID.Equals(currentSelectedSkillID));
                        if (resource_data != null && ref_data != null)
                        {
                            var desc = StringManager.GetActiveSkillDescKey(ref_data.SkillID);
                            var currencyType = (CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType;
                            var currencyIcon = ResourceManager.Instance.GetCurrencyIcon(currencyType);
                            uiBuy.Setup(desc, resource_data.texture, currencyIcon, ref_data.Price, currencyType);
                        }
                    }
                    break;
                case CommonDefine.SkillType.Passive:
                    {
                        var list_passive = ResourceManager.Instance.PassiveSkillDataList;
                        var resource_data = list_passive.Find(x => x.id.Equals(currentSelectedSkillID));
                        var ref_data = skillData.PassiveSkillInfoList.Find(x => x.SkillID.Equals(currentSelectedSkillID));
                        if (resource_data != null && ref_data != null)
                        {
                            var desc = StringManager.GetPassiveSkillDescKey(ref_data.SkillID);
                            var currencyType = (CommonDefine.CurrencyType)ref_data.PurchaseCurrencyType;
                            var currencyIcon = ResourceManager.Instance.GetCurrencyIcon(currencyType);
                            uiBuy.Setup(desc, resource_data.texture, currencyIcon, ref_data.Price, currencyType);
                        }
                    }
                    break;
            }
           
            UIManager.Instance.ShowUI(uiBuy);
            */
        }
    }

    public void SetCurrency()
    {
        currency?.SetCurrency();
    }

    private void OnValueChangedAccountData()
    {
        foreach (var i in skillSlotList)
        {
            if (i != null)
                i.Refresh();
        }
    }

    public void OnBackButton()
    {
        Hide();
    }
}
