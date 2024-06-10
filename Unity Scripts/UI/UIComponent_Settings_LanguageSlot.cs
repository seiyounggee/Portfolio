using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


public class UIComponent_Settings_LanguageSlot : MonoBehaviour
{
    [SerializeField] GameObject selectedObj = null;
    [SerializeField] TextMeshProUGUI languageTxt = null;
    [SerializeField] Button button;

    private Locale local = null ;

    private void Awake()
    {
        button.SafeSetButton(OnClickBtn);
    }

    private void OnEnable()
    {
        LocalizationSettings.Instance.OnSelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.Instance.OnSelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    public void Setup(Locale _local)
    {
        local = _local;
        languageTxt.SafeSetText(local.LocaleName);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == button)
        {
            if (local != null)
            {
                StringManager.SetLanguage(local.Identifier);
            }
        }
    }

    private void OnSelectedLocaleChanged(Locale loc)
    {
        selectedObj.SafeSetActive(loc.Equals(local));
    }
}
