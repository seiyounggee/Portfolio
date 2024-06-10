using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Quest : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;
    [SerializeField] UIComponent_Currency currency;

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
    }

    public void OnBackButton()
    {
        Hide();
    }
}
