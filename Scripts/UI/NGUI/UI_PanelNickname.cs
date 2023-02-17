using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelNickname : UI_PanelBase
{
    [SerializeField] GameObject okBtn = null;
    [SerializeField] GameObject cancelBtn = null;

    [SerializeField] UIInput inputField = null;

    private void Awake()
    {
        okBtn.SafeSetButton(OnClickBtn);
        cancelBtn.SafeSetButton(OnClickBtn);
    }

    private void OnEnable()
    {
        inputField.value = "";
        inputField.defaultText = "Click Here To Change";
    }

    private void OnClickBtn(GameObject go)
    {
        if (go == okBtn)
        {
            if (inputField.value != null && string.IsNullOrEmpty(inputField.value) == false)
            {
                var ud = DataManager.Instance.userData;
                if (ud != null)
                {
                    inputField.value = inputField.value.Replace("\n", "");

                    if (string.IsNullOrEmpty(inputField.value))
                    {
                        Debug.Log("Error... Null input");
                        Close();
                        return;
                    }

                    ud.NickName = inputField.value;

                    DataManager.Instance.SaveUserData();

                    var lobbyUI = PrefabManager.Instance.UI_PanelLobby_Main;
                    lobbyUI.SetOutGameData();
                }
            }

            Close();
        }
        else if (go == cancelBtn)
        {
            Close();
        }
    }
}
