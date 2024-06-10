using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System.Globalization;

public class PlayerPrefsManager : Singleton<PlayerPrefsManager>
{
    public const string PLAYERPREFS_LANGUAGE_CODE = "PLAYERPREFS_LANGUAGE_CODE";

    public const string PLAYERPREFS_SETTINGS_SFX_ONOFF = "PLAYERPREFS_SETTINGS_SFX_ONOFF";
    public const string PLAYERPREFS_SETTINGS_MUSIC_ONOFF = "PLAYERPREFS_SETTINGS_MUSIC_ONOFF";

    public void SaveLanguageCode(LocaleIdentifier id)
    {
        PlayerPrefs.SetString(PLAYERPREFS_LANGUAGE_CODE, id.Code);
    }

    public string GetSavedLanguageCode()
    {
        if (PlayerPrefs.HasKey(PLAYERPREFS_LANGUAGE_CODE))
        {
            return PlayerPrefs.GetString(PLAYERPREFS_LANGUAGE_CODE);
        }
        else
        {
            return string.Empty;
        }
    }

    public void SaveSettingsSFX(bool isOn)
    {
        if (isOn)
            PlayerPrefs.SetInt(PLAYERPREFS_SETTINGS_SFX_ONOFF, 1);
        else
            PlayerPrefs.SetInt(PLAYERPREFS_SETTINGS_SFX_ONOFF, 0);
    }

    public bool GetSettingsSFX_IsOn()
    {
        if (PlayerPrefs.HasKey(PLAYERPREFS_SETTINGS_SFX_ONOFF))
        {
            var onOff = PlayerPrefs.GetInt(PLAYERPREFS_SETTINGS_SFX_ONOFF);

            return onOff == 1;
        }
        else
        {
            SaveSettingsSFX(true);
            return true;
        }
    }

    public void SaveSettingsMusic(bool isOn)
    {
        if (isOn)
            PlayerPrefs.SetInt(PLAYERPREFS_SETTINGS_MUSIC_ONOFF, 1);
        else
            PlayerPrefs.SetInt(PLAYERPREFS_SETTINGS_MUSIC_ONOFF, 0);
    }

    public bool GetMusicSFX_IsOn()
    {
        if (PlayerPrefs.HasKey(PLAYERPREFS_SETTINGS_MUSIC_ONOFF))
        {
            var onOff = PlayerPrefs.GetInt(PLAYERPREFS_SETTINGS_MUSIC_ONOFF);

            return onOff == 1;
        }
        else
        {
            SaveSettingsMusic(true);
            return true;
        }
    }
}
