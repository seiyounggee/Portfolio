using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Globalization;
using System.Threading.Tasks;

public static partial class StringManager
{
    public const string LOCALIZATION_TABLE_NAME = "Localization Asset Table";

    #region Localization

    public static void SetInitialLanguge()
    {
        var language = PlayerPrefsManager.Instance.GetSavedLanguageCode();
        if (string.IsNullOrEmpty(language) == false)
        {
            //저장된 언어가 있는 경우
            SetLanguage(language);
        }
        else
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            var locale = LocalizationSettings.AvailableLocales.GetLocale(cultureInfo);
            if (locale != null)
            {
                SetLanguage(locale.Identifier);
            }
            else
            {
                //지원하지 언어인 경우 기본 영어로 세팅하자..
                SetLanguage(CommonDefine.LANGUAGE_ENGLISH);
            }
        }
    }

    public static void SetLanguage(LocaleIdentifier id)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(id);
        if (locale != null)
        {
            Debug.Log("<color=cyan>Setting Language => " + id + "</color>");
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefsManager.Instance.SaveLanguageCode(locale.Identifier);
        }
        else
        {
            Debug.Log("<color=red>Locale not found: " + id + "</color>");
        }
    }

    public static void SetLanguage(string languageCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
        if (locale != null)
        {
            Debug.Log("<color=cyan>Setting Language => " + languageCode + "</color>");
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefsManager.Instance.SaveLanguageCode(locale.Identifier);
        }
        else
        {
            Debug.Log("<color=red>Locale not found: " + languageCode + "</color>");
        }
    }

    public static void SafeLocalizeText(this Text uiTextComponent, string key)
    {
        LocalizeText(uiTextComponent, key);
    }

    private static void LocalizeText(Text uiTextComponent, string key)
    {
        if (uiTextComponent == null)
            return;

        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LOCALIZATION_TABLE_NAME, key).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                uiTextComponent.text = handle.Result; // UI 컴포넌트에 직접 값을 설정
            }
            else
            {
                uiTextComponent.text = "MISSING_" + key; // 실패 시 처리
            }
        };
    }

    public static void SafeLocalizeText(this TextMeshProUGUI uiTextComponent, string key)
    {
        LocalizeText(uiTextComponent, key);
    }

    public static void SafeLocalizeText(this TextMeshProUGUI uiTextComponent, string key, params object[] args)
    {
        LocalizeText(uiTextComponent, key, args);
    }

    private static void LocalizeText(TextMeshProUGUI uiTextComponent, string key)
    {
        if (uiTextComponent == null)
            return;

        if (string.IsNullOrEmpty(key))
        {
            uiTextComponent.SafeSetText(string.Empty);
            return;
        }

        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LOCALIZATION_TABLE_NAME, key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                uiTextComponent.SafeSetText(handle.Result); // UI 컴포넌트에 직접 값을 설정
            }
            else
            {
                uiTextComponent.SafeSetText("MISSING_" + key); // 실패 시 처리
            }
        };
    }

    private static void LocalizeText(TextMeshProUGUI uiTextComponent, string key, params object[] args)
    {
        if (uiTextComponent == null)
            return;

        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LOCALIZATION_TABLE_NAME, key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                uiTextComponent.text = string.Format(handle.Result, args); // UI 컴포넌트에 직접 값을 설정
            }
            else
            {
                uiTextComponent.text = "MISSING_" + key; // 실패 시 처리
            }
        };
    }

    public static void GetLocailzedText(string key, Action<bool, string> callback = null)
    {
        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LOCALIZATION_TABLE_NAME, key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(true, handle.Result);
            }
            else
            {
                callback?.Invoke(true, "MISSING_" + key);
            }
        };
    }


    #endregion

    #region String Format

    public static string GetTimeString_TYPE_1(double totalSeconds)
    {
        // example: 15m 33s

        double mincheck = 60;
        double hourcheck = 60 * 60;  //3600
        double daychek = 60 * 60 * 24;   //84600
        double share = totalSeconds / daychek;
        double rest = 0;
        // Day 
        if (share >= 1)
        {
            rest = totalSeconds - (daychek * (int)share);
            rest /= hourcheck;
            return string.Format("{0}d {1}h", (int)share, (int)rest);
        }
        else
        {
            share = totalSeconds / hourcheck;
            // Hour
            if (share >= 1)
            {
                rest = totalSeconds - (hourcheck * (int)share);
                rest /= mincheck;
                return string.Format("{0}h {1}m", (int)share, (int)rest);
            }
            else
            {
                share = totalSeconds / mincheck;
                // Min sec
                if (share >= 1)
                {
                    rest = totalSeconds - (mincheck * (int)share);
                    return string.Format("{0}m {1}s", (int)share, (int)rest);
                }
                else
                    return string.Format("{0}s", (int)totalSeconds);
            }
        }
    }

    public static string GetTimeString_TYPE_2(double totalSeconds)
    {
        int min = (int)(totalSeconds / 60);
        double sec = totalSeconds - min * 60;

        string minString = "";
        if (min < 10) //1의 자리 수가 존재 안할경우
            minString = "0" + min.ToString();
        else
            minString = min.ToString();

        string secString = "";
        sec = Math.Round(sec, 2);
        if (sec < 10) //1의 자리 수가 존재 안할경우
            secString = "0" + string.Format("{0:f0}", sec);
        else
            secString = string.Format("{0:f0}", sec);

        string txt = minString + ":" + secString;

        //string txt = minString + ":" + string.Format("{0:f2}", Math.Round(sec, 2));
        //return string.Format("{0}:{1}", min, Math.Round(sec, 2));
        return txt;

    }

    /* 인트형을 문자로 변형해주면서 컴마 넣어줌 */
    public static string GetNumberChange(int value)
    {
        return value.ToString("n0", CultureInfo.InvariantCulture);
    }

    /* 인트형을 문자로 변형해주면서 컴마 넣어줌 */
    public static string GetNumberChange(long value)
    {
        return value.ToString("n0", CultureInfo.InvariantCulture);
    }

    public static string GetNumberChange(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            value = 0;
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string GetNumberChange(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            value = 0;
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string GetNumberChange(float value, string strType)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            value = 0;
        return value.ToString(strType, CultureInfo.InvariantCulture);
    }

    public static string GetRankingFormat(int number)
    {
        int lastTwoDigits = number % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return number + "th";
        }

        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }

    public static string GetRankingEndFormat(int number)
    {
        int lastTwoDigits = number % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return "th";
        }

        switch (number % 10)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";
        }
    }

    #endregion
}
