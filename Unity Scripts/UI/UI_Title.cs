using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class UI_Title : UIBase
{
    [SerializeField] TextMeshProUGUI currentStatusTxt = null;


    public static void Activate(string msg, bool forceShow = true)
    {
        var ui = PrefabManager.Instance.UI_Title;
        if (forceShow)
            ui.Show();

        ui.SetText(msg);
    }

    public static void Deactivate()
    {
        var ui = PrefabManager.Instance.UI_Title;
        ui.Hide();
    }

    public void SetText(string msg)
    {
        currentStatusTxt.SafeSetText(msg);
    }
}
