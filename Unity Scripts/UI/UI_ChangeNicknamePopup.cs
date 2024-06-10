using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UI_ChangeNicknamePopup : UIBase, IBackButtonHandler
{
    [SerializeField] Button exitBtn = null;
    [SerializeField] Button okayBtn = null;
    [SerializeField] TMP_InputField inputField = null;

    private string newNickname = null;

    private void Awake()
    {
        exitBtn.SafeSetButton(OnClickBtn);
        okayBtn.SafeSetButton(OnClickBtn);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        UIManager.Instance.RegisterBackButton(this);
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        UIManager.Instance.UnregisterBackButton(this);
        inputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == exitBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
        else if (btn == okayBtn)
        {
            if (string.IsNullOrEmpty(newNickname) == false
                && newNickname.Length >= 3 && newNickname.Length <= 15
                && newNickname.Equals(AccountManager.Instance.Nickname) == false)
            {
                UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                AccountManager.Instance.Save_Nickname(newNickname, (isSuccess) =>
                {
                    UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
                    Hide();

                    if (isSuccess)
                    {
                        var ui = PrefabManager.Instance.UI_ToastMessage;
                        ui.SetMessage("Nickname Successfully Changed!");
                        ui.Show();
                    }
                    else
                    { 
                    
                    }
                });
            }
            else
            {
                var ui = PrefabManager.Instance.UI_ToastMessage;
                ui.SetMessage("Not Valid Nickname");
                ui.Show();
            }
        }
    }

    private void OnValueChanged(string _newNickname)
    {
        newNickname = _newNickname;
    }

    public void OnBackButton()
    {
        Hide();
    }
}
