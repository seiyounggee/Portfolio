using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UI_Skin : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;
    [SerializeField] UIComponent_Currency currency;

    [Header("Left Group")]
    [SerializeField] TextMeshProUGUI skinNameTxt = null;
    [SerializeField] TextMeshProUGUI skinDescTxt = null;
    [SerializeField] TextMeshProUGUI skinRarityTxt = null;
    [SerializeField] Image skinRarityImg = null;
    [SerializeField] TextMeshProUGUI skinTypeTxt = null;
    [SerializeField] Image skinTypeImg = null;

    [SerializeField] GameObject stats_attackObj = null;
    [SerializeField] TextMeshProUGUI stats_attackTxt = null;
    [SerializeField] GameObject stats_hpObj = null;
    [SerializeField] TextMeshProUGUI stats_hpTxt = null;
    [SerializeField] GameObject stats_speedObj = null;
    [SerializeField] TextMeshProUGUI stats_speedTxt = null;


    [Header("Right Group")]
    [SerializeField] Button characterBtn = null;
    [SerializeField] Button weaponBtn = null;
    [SerializeField] UIButton_StateVisualChanger characterBtn_stateChanger = null;
    [SerializeField] UIButton_StateVisualChanger weaponBtn_stateChanger = null;


    [SerializeField] GameObject skinListParentObj = null;
    [SerializeField] UIComponent_SkinSlot skinSlotTemplate = null;

    private List<UIComponent_SkinSlot> skinSlotList = new List<UIComponent_SkinSlot>();

    private CommonDefine.SkinType currentSelectedSkinType = CommonDefine.SkinType.None;
    private int currentSelectedSkinID = 0;

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);
        characterBtn.SafeSetButton(OnClickBtn);
        weaponBtn.SafeSetButton(OnClickBtn);

        skinSlotTemplate.SafeSetActive(false);
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

        RenderTextureManager.Instance.DeactivateRenderTexture_All();

        AccountManager.Instance.OnValueChangedAccountData -= OnValueChangedAccountData;

        UIManager.Instance.UnregisterBackButton(this);
    }

    public void OnBackButton()
    {
        Hide();
    }

    public override void Show()
    {
        base.Show();

        if (currentSelectedSkinType == CommonDefine.SkinType.None)
            SetSelectedData(CommonDefine.SkinType.Character);

        SetSelectedSkin();
        SetSkinList();
        SetCurrency();
        SetTabMenuBtn();
        SetStats();
    }

    public override void Hide()
    {
        base.Hide();

        ClearData();
    }

    private void ClearData()
    {
        currentSelectedSkinType = CommonDefine.SkinType.None;
        currentSelectedSkinID = 0;
    }

    public void SetSelectedData(CommonDefine.SkinType type, int skillId = -1)
    {
        currentSelectedSkinType = type;

        if (skillId >= 0)
        {
            currentSelectedSkinID = skillId;
        }
        else
        {
            var accData = AccountManager.Instance.AccountData;
            if (accData != null)
            {
                switch (currentSelectedSkinType)
                {
                    case CommonDefine.SkinType.Character:
                        {
                            currentSelectedSkinID = accData.characterSkinID;
                        }
                        break;
                    case CommonDefine.SkinType.Weapon:
                        {
                            currentSelectedSkinID = accData.weaponSkinID;
                        }
                        break;
                }
            }
        }
    }

    private void SetSelectedSkin()
    {
        var skinData = ReferenceManager.Instance.SkinData;
        var skinCharList = ResourceManager.Instance.CharacterSkinDataList;
        var skinWeaponList = ResourceManager.Instance.WeaponSkinDataList;
        var account = AccountManager.Instance.AccountData;

        if (skinData != null && skinCharList != null && skinWeaponList != null && account != null)
        {
            switch (currentSelectedSkinType)
            {
                case CommonDefine.SkinType.Character:
                    {
                        var list_character = skinData.CharacterSkinList;
                        var ref_data = list_character.Find(x => x.SkinID.Equals(currentSelectedSkinID));
                        var resource_data = skinCharList.Find(x => x.id.Equals(currentSelectedSkinID));

                        if (ref_data != null && resource_data != null)
                        {
                            skinNameTxt.SafeLocalizeText(ref_data.SkinName);
                            skinDescTxt.SafeLocalizeText(StringManager.GetCharacterSkinDescKey(ref_data.SkinID));
                            skinRarityTxt.SafeLocalizeText(StringManager.GetRarityKey(ref_data.Rarity));
                            skinRarityImg.SafeColor(UIManager.GetRarityColor(ref_data.Rarity));
                            skinTypeTxt.SafeLocalizeText(StringManager.GetSkinTypeKey(CommonDefine.SkinType.Character));
                            skinTypeImg.SafeColor(UIManager.GetSkinTypeColor(CommonDefine.SkinType.Character));
                            RenderTextureManager.Instance.ActivateRenderTexture_Player(currentSelectedSkinID, account.weaponSkinID);
                        }
                    }
                    break;

                case CommonDefine.SkinType.Weapon:
                    {
                        var list_weapon = skinData.WeaponSkinList;
                        var ref_data = list_weapon.Find(x => x.SkinID.Equals(currentSelectedSkinID));
                        var resource_data = skinWeaponList.Find(x => x.id.Equals(currentSelectedSkinID));

                        if (ref_data != null && resource_data != null)
                        {
                            skinNameTxt.SafeLocalizeText(ref_data.SkinName);
                            skinDescTxt.SafeLocalizeText(StringManager.GetWeaponSkinDescKey(ref_data.SkinID));
                            skinRarityTxt.SafeLocalizeText(StringManager.GetRarityKey(ref_data.Rarity));
                            skinRarityImg.SafeColor(UIManager.GetRarityColor(ref_data.Rarity));
                            skinTypeTxt.SafeLocalizeText(StringManager.GetSkinTypeKey(CommonDefine.SkinType.Weapon));
                            skinTypeImg.SafeColor(UIManager.GetSkinTypeColor(CommonDefine.SkinType.Weapon));
                            RenderTextureManager.Instance.ActivateRenderTexture_Weapon(currentSelectedSkinID);
                        }
                    }
                    break;
            }
        }
    }

    private void SetSkinList()
    {
        var rt = skinListParentObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            //맨위 쪽으로 고정...?
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0f);
        }

        foreach (var i in skinSlotList)
        {
            i.SafeSetActive(false);
        }

        var skinData = ReferenceManager.Instance.SkinData;
        if (skinData != null)
        {
            switch (currentSelectedSkinType)
            {
                case CommonDefine.SkinType.Character:
                    {
                        var skinListToShow = skinData.CharacterSkinList;
                        for (int i = 0; i < skinListToShow.Count; i++)
                        {
                            if (i < skinSlotList.Count)
                            {
                                skinSlotList[i].SetData_Character(skinListToShow[i]);
                                skinSlotList[i].SafeSetActive(true);
                            }
                            else
                            {
                                var slot = GameObject.Instantiate(skinSlotTemplate);
                                slot.transform.SetParent(skinListParentObj.transform);
                                slot.transform.localScale = Vector3.one;
                                slot.SetData_Character(skinListToShow[i]);
                                slot.SafeSetActive(true);
                                skinSlotList.Add(slot);
                            }
                        }
                    }
                    break;

                case CommonDefine.SkinType.Weapon:
                    {
                        var skillListToShow = skinData.WeaponSkinList;
                        for (int i = 0; i < skillListToShow.Count; i++)
                        {
                            if (i < skinSlotList.Count)
                            {
                                skinSlotList[i].SetData_Weapon(skillListToShow[i]);
                                skinSlotList[i].SafeSetActive(true);
                            }
                            else
                            {
                                var slot = GameObject.Instantiate(skinSlotTemplate);
                                slot.transform.SetParent(skinListParentObj.transform);
                                slot.transform.localScale = Vector3.one;
                                slot.SetData_Weapon(skillListToShow[i]);
                                slot.SafeSetActive(true);
                                skinSlotList.Add(slot);
                            }
                        }
                    }
                    break;
            }
        }
    }

    public void OnClickSkinSlot_Focus(CommonDefine.SkinType type, int skinId)
    {
        SetSelectedData(type, skinId);

        SetSelectedSkin();
    }

    public void OnClickSkinSlot_Select(CommonDefine.SkinType type, int skinId)
    {
        AccountManager.Instance.Save_SkinData(type, skinId, (isSuccess)=>
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
                ui.SetUp(UI_CommonPopup.CommonPopupType.Okay_WithExit, "ERROR_TITLE", "ERROR_SKIN_SELECTION", null);
                UIManager.Instance.ShowUI(ui);
            }
        });

    }

    private void SetTabMenuBtn()
    {
        switch (currentSelectedSkinType)
        {
            case CommonDefine.SkinType.Character:
                {
                    characterBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Focused);
                    weaponBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Unfocused);
                }
                break;
            case CommonDefine.SkinType.Weapon:
                {
                    characterBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Unfocused);
                    weaponBtn_stateChanger.SetState(UIButton_StateVisualChanger.State.Focused);
                }
                break;
        }
    }

    private void SetStats()
    {
        var skinData = ReferenceManager.Instance.SkinData;

        if (skinData == null || skinData.CharacterSkinList == null || skinData.WeaponSkinList == null)
            return;

        stats_attackObj.SafeSetActive(false);
        stats_speedObj.SafeSetActive(false);
        stats_hpObj.SafeSetActive(false);

        switch (currentSelectedSkinType)
        {
            case CommonDefine.SkinType.Character:
                {
                    var ref_data = skinData.CharacterSkinList.Find(x => x.SkinID.Equals(currentSelectedSkinID));
                    if (ref_data != null)
                    {
                        if (ref_data.increase_attackDamage > 0)
                        {
                            stats_attackObj.SafeSetActive(true);
                            stats_attackTxt.SafeSetText("+" + ref_data.increase_attackDamage);
                        }

                        if (ref_data.increase_maxhp > 0)
                        {
                            stats_hpObj.SafeSetActive(true);
                            stats_hpTxt.SafeSetText("+" + ref_data.increase_maxhp);
                        }

                        if (ref_data.increase_moveSpeed > 0)
                        {
                            stats_speedObj.SafeSetActive(true);
                            stats_speedTxt.SafeSetText("+" + ref_data.increase_moveSpeed);
                        }
                    }
                }
                break;
            case CommonDefine.SkinType.Weapon:
                {
                    var ref_data = skinData.WeaponSkinList.Find(x => x.SkinID.Equals(currentSelectedSkinID));
                    if (ref_data != null)
                    {
                        if (ref_data.increase_attackDamage > 0)
                        {
                            stats_attackObj.SafeSetActive(true);
                            stats_attackTxt.SafeSetText("+" + ref_data.increase_attackDamage);
                        }

                        if (ref_data.increase_maxhp > 0)
                        {
                            stats_hpObj.SafeSetActive(true);
                            stats_hpTxt.SafeSetText("+" + ref_data.increase_maxhp);
                        }

                        if (ref_data.increase_moveSpeed > 0)
                        {
                            stats_speedObj.SafeSetActive(true);
                            stats_speedTxt.SafeSetText("+" + ref_data.increase_moveSpeed);
                        }
                    }
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
        else if (btn == characterBtn)
        {
            SetSelectedData(CommonDefine.SkinType.Character);
            SetSelectedSkin();
            SetSkinList();
            SetTabMenuBtn();
        }
        else if (btn == weaponBtn)
        {
            SetSelectedData(CommonDefine.SkinType.Weapon);
            SetSelectedSkin();
            SetSkinList();
            SetTabMenuBtn();
        }
    }

    public void SetCurrency()
    {
        currency?.SetCurrency();
    }

    private void OnValueChangedAccountData()
    {
        foreach (var i in skinSlotList)
        {
            if (i != null)
                i.Refresh();
        }
    }
}
