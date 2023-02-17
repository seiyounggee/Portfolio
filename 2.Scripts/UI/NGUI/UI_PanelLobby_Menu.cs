using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PanelLobby_Menu : UI_PanelBase
{
    [SerializeField] GameObject garageBtn = null;
    [SerializeField] GameObject characterBtn = null;
    [SerializeField] GameObject trackBtn = null;
    [SerializeField] GameObject shopBtn = null;
    [SerializeField] GameObject questBtn = null;

    private void Awake()
    {
        garageBtn.SafeSetButton(OnClickBtn);
        characterBtn.SafeSetButton(OnClickBtn);
        trackBtn.SafeSetButton(OnClickBtn);
        shopBtn.SafeSetButton(OnClickBtn);
        questBtn.SafeSetButton(OnClickBtn);
    }

    private void OnClickBtn(GameObject go)
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.Lobby)
            return;

        if (go == garageBtn)
        {
            UIManager_NGUI.Instance.ActivateLobbyUI(UIManager_NGUI.UIType.UI_PanelLobby_Garage);
        }
        else if (go == characterBtn)
        {
            UIManager_NGUI.Instance.ActivateLobbyUI(UIManager_NGUI.UIType.UI_PanelLobby_Character);
        }
        else if (go == trackBtn)
        {
            UIManager_NGUI.Instance.ActivateLobbyUI(UIManager_NGUI.UIType.UI_PanelLobby_Main);
        }
        else if (go == shopBtn)
        {
            UIManager_NGUI.Instance.ActivateLobbyUI(UIManager_NGUI.UIType.UI_PanelLobby_Shop);
        }
        else if (go == questBtn)
        {
            UIManager_NGUI.Instance.ActivateLobbyUI(UIManager_NGUI.UIType.UI_PanelLobby_Quest);
        }
    }
}
