using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CommonPopup : UIBase
{
    [SerializeField] TextMeshProUGUI titleTxt;
    [SerializeField] TextMeshProUGUI descTxt;
    
    [SerializeField] Button btnCenterOkay = null;
    [SerializeField] Button btnExit = null;

    [SerializeField] Button btnYes = null;
    [SerializeField] Button btnNo = null;

    public Action btnCallback = null;

    public enum CommonPopupType
    { 
        None,
        YesNo,
        Okay_WithExit,
        Okay_WithoutExit,
    }

    private CommonPopupType currentType = CommonPopupType.None;

    private void Awake()
    {
        btnCenterOkay.SafeSetButton(OnClickBtn);
        btnExit.SafeSetButton(OnClickBtn);

        btnYes.SafeSetButton(OnClickBtn);
        btnNo.SafeSetButton(OnClickBtn);

        Clear();
    }

    public override void Show()
    {
        base.Show();

        //혹시나 해서...
        if (currentType == CommonPopupType.None)
            Hide();
    }

    public override void Hide()
    {
        base.Hide();

        Clear();
    }

    private void Clear()
    {
        titleTxt.SafeSetText(string.Empty);
        descTxt.SafeSetText(string.Empty);

        btnCallback = null;

        btnYes.SafeSetActive(false);
        btnNo.SafeSetActive(false);
        btnCenterOkay.SafeSetActive(false);
        btnExit.SafeSetActive(false);
    }

    public void SetUp(CommonPopupType type, string title_key = "", string desc_key = "", Action callback = null)
    {
        currentType = type;

        titleTxt.SafeLocalizeText(title_key);
        descTxt.SafeLocalizeText(desc_key);

        switch (type)
        {
            case CommonPopupType.YesNo:
                SetUp_YesNo();
                break;

            case CommonPopupType.Okay_WithExit:
                SetUp_OkayWithExit();
                break;

            case CommonPopupType.Okay_WithoutExit:
                SetUp_OkayWithoutExit();
                break;
        }

        btnCallback = callback;
    }

    private void SetUp_YesNo()
    {
        btnYes.SafeSetActive(true);
        btnNo.SafeSetActive(true);

        btnCenterOkay.SafeSetActive(false);
        btnExit.SafeSetActive(false);
    }

    private void SetUp_OkayWithExit()
    {
        btnExit.SafeSetActive(true);
        btnCenterOkay.SafeSetActive(true);

        btnYes.SafeSetActive(false);
        btnNo.SafeSetActive(false);
    }

    private void SetUp_OkayWithoutExit()
    {
        btnCenterOkay.SafeSetActive(true);

        btnExit.SafeSetActive(false);
        btnYes.SafeSetActive(false);
        btnNo.SafeSetActive(false);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == btnCenterOkay)
        {
            btnCallback?.Invoke();
            Hide();
        }
        else if (btn == btnExit)
        {
            Hide();
        }
        else if (btn == btnYes)
        {
            btnCallback?.Invoke();
            Hide();
        }
        else if (btn == btnNo)
        {
            Hide();
        }


    }
}
