using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UI_PanelCommon : UI_PanelBase
{
    [SerializeField] UILabel msgTxt = null;

    [SerializeField] GameObject yesBtn;
    [SerializeField] GameObject noBtn;

    private Action yesCallback = null;
    private Action noCallback = null;

    private void Awake()
    {
        yesBtn.SafeSetButton(OnClickBtn);
        noBtn.SafeSetButton(OnClickBtn);
    }

    public void SetData(string msg, Action yesCallback = null, Action noCallback = null)
    {
        msgTxt.SafeSetText(msg);

        this.yesCallback = yesCallback;
        this.noCallback = noCallback;
    }

    public void OnClickBtn(GameObject go)
    {
        if (go == yesBtn)
        {
            yesCallback?.Invoke();
            base.Close();

            yesCallback = null;
        }
        else if (go == noBtn)
        {
            noCallback?.Invoke();
            base.Close();

            noCallback = null;
        }
    }
}
