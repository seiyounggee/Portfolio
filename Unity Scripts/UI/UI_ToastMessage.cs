using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_ToastMessage : UIBase
{
    [SerializeField] TextMeshProUGUI msgText = null;

    public override void Show()
    {
        base.Show();

        UtilityInvoker.Invoke(this, () => Hide(), 1.5f);
    }

    public void SetMessage(string msg_key)
    {
        msgText.SafeLocalizeText(msg_key);
    }

    public void SetMessage(string msg_key, params object[] args)
    {
        msgText.SafeLocalizeText(msg_key, args);
    }

    public void SetMessage_NoLocalization(string msg)
    {
        msgText.SafeSetText(msg);
    }
}
