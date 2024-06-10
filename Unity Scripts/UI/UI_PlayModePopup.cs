using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_PlayModePopup : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;

    [SerializeField] GameObject objBase_SoloMode;
    [SerializeField] Button btn_SoloMode = null;

    [SerializeField] GameObject objBase_TeamMode;
    [SerializeField] Button btn_TeamMode = null;

    [SerializeField] GameObject objBase_PracticeMode;
    [SerializeField] Button btn_PracticeMode = null;

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);

        btn_SoloMode.SafeSetButton(OnClickBtn);
        btn_TeamMode.SafeSetButton(OnClickBtn);
        btn_PracticeMode.SafeSetButton(OnClickBtn);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        UIManager.Instance.RegisterBackButton(this);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        UIManager.Instance.UnregisterBackButton(this);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
        else if (btn == btn_SoloMode)
        {
            AccountManager.Instance.SetIngamePlayMode(Quantum.InGamePlayMode.SoloMode);
            PrefabManager.Instance.UI_OutGame.SetPlayModeBtn();
            Hide();
        }
        else if (btn == btn_TeamMode)
        {
            AccountManager.Instance.SetIngamePlayMode(Quantum.InGamePlayMode.TeamMode);
            PrefabManager.Instance.UI_OutGame.SetPlayModeBtn();
            Hide();
        }
        else if (btn == btn_PracticeMode)
        {
            AccountManager.Instance.SetIngamePlayMode(Quantum.InGamePlayMode.PracticeMode);
            PrefabManager.Instance.UI_OutGame.SetPlayModeBtn();
            Hide();
        }
    }

    public void OnBackButton()
    {
        Hide();
    }
}
