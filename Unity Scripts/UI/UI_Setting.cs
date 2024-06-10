using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UI_Setting : UIBase, IBackButtonHandler
{
    [SerializeField] Button exitBtn = null;

    [SerializeField] Toggle sfxToggle_On = null;
    [SerializeField] Toggle sfxToggle_Off = null;
    [SerializeField] Toggle musicToggle_On = null;
    [SerializeField] Toggle musicToggle_Off = null;
    [SerializeField] Toggle grahpicToggle_Low = null;
    [SerializeField] Toggle grahpicToggle_Best = null;
    [SerializeField] TextMeshProUGUI clientInfoText = null;
    [SerializeField] Button languageBtn = null;
    [SerializeField] Button termsOfServiceBtn = null;
    [SerializeField] Button privacyPolicyBtn = null;
    [SerializeField] Button contactBtn = null;
    [SerializeField] Button signInBtn = null;
    [SerializeField] Button signOutBtn = null;

    [SerializeField] GameObject languagePopupObj = null;
    [SerializeField] Button languagePopupExitBtn = null;
    [SerializeField] Transform languageSlotParent = null;
    [SerializeField] UIComponent_Settings_LanguageSlot languageSlotTemplate = null;

    private List<UIComponent_Settings_LanguageSlot> slotList = new List<UIComponent_Settings_LanguageSlot>();

    private void Awake()
    {
        exitBtn.SafeSetButton(OnClickBtn);

        languageBtn.SafeSetButton(OnClickBtn);
        languagePopupExitBtn.SafeSetButton(OnClickBtn);

        termsOfServiceBtn.SafeSetButton(OnClickBtn);
        privacyPolicyBtn.SafeSetButton(OnClickBtn);
        contactBtn.SafeSetButton(OnClickBtn);
        signInBtn.SafeSetButton(OnClickBtn);
        signOutBtn.SafeSetButton(OnClickBtn);

        sfxToggle_On.SafeSetToggle(OnClickToggle);
        sfxToggle_Off.SafeSetToggle(OnClickToggle);
        musicToggle_On.SafeSetToggle(OnClickToggle);
        musicToggle_Off.SafeSetToggle(OnClickToggle);

        languageSlotTemplate.SafeSetActive(false);
    }
    internal override void OnEnable()
    {
        base.OnEnable();

        UIManager.Instance.RegisterBackButton(this);

        Setup();
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        UIManager.Instance.UnregisterBackButton(this);
    }

    public void OnBackButton()
    {
        Hide();
    }

    public void Setup()
    {
        languagePopupObj.SafeSetActive(false);

        string info = string.Format("Client Version : {0} \nServer Version : {1} \nRegion : {2}",
            CommonDefine.ClientVersion,
            CommonDefine.QuantumVersion,
            NetworkManager_Client.Instance.BestRegion);
        clientInfoText.SafeSetText(info);

        sfxToggle_On.isOn = PlayerPrefsManager.Instance.GetSettingsSFX_IsOn();
        sfxToggle_Off.isOn = !PlayerPrefsManager.Instance.GetSettingsSFX_IsOn();

        musicToggle_On.isOn = PlayerPrefsManager.Instance.GetMusicSFX_IsOn();
        musicToggle_Off.isOn = !PlayerPrefsManager.Instance.GetMusicSFX_IsOn();

        SetLanguageSlotList();
    }

    private void SetLanguageSlotList()
    {
        foreach (var i in slotList)
        {
            i.SafeSetActive(false);
        }

        var list = LocalizationSettings.AvailableLocales.Locales;

        if (list == null || list.Count <= 0)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (i < slotList.Count)
            {
                slotList[i].Setup(list[i]);
                slotList[i].SafeSetActive(true);
            }
            else
            {
                var slot = GameObject.Instantiate(languageSlotTemplate);
                slot.transform.SetParent(languageSlotParent);
                slot.transform.localScale = Vector3.one;
                slot.Setup(list[i]);
                slot.SafeSetActive(true);
                slotList.Add(slot);
            }
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == exitBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
        else if (btn == languageBtn)
        {
            languagePopupObj.SafeSetActive(true);
        }
        else if (btn == languagePopupExitBtn)
        {
            languagePopupObj.SafeSetActive(false);
        }
    }

    private void OnClickToggle(Toggle toggle)
    {
        if (toggle == sfxToggle_On)
        {
            PlayerPrefsManager.Instance.SaveSettingsSFX(sfxToggle_On.isOn);
            sfxToggle_Off.isOn = !sfxToggle_On.isOn;
        }
        else if (toggle == sfxToggle_Off)
        {
            PlayerPrefsManager.Instance.SaveSettingsSFX(!sfxToggle_Off.isOn);
            sfxToggle_On.isOn = !sfxToggle_Off.isOn;
        }
        else if (toggle == musicToggle_On)
        {
            PlayerPrefsManager.Instance.SaveSettingsMusic(musicToggle_On.isOn);
            musicToggle_Off.isOn = !musicToggle_On.isOn;

            if (PlayerPrefsManager.Instance.GetMusicSFX_IsOn() == false)
                SoundManager.Instance.StopAllBgmPlaying();
            else
                SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.BGM_OutGame_01);
        }
        else if (toggle == musicToggle_Off)
        {
            PlayerPrefsManager.Instance.SaveSettingsMusic(!musicToggle_Off.isOn);
            musicToggle_On.isOn = !musicToggle_Off.isOn;

            if (PlayerPrefsManager.Instance.GetMusicSFX_IsOn() == false)
                SoundManager.Instance.StopAllBgmPlaying();
            else
                SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.BGM_OutGame_01);

        }
    }
}
