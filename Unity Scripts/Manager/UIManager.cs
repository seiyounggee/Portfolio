using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : MonoSingleton<UIManager>
{
    private List<UIBase> activatedUIList = new List<UIBase>();

    public enum UIType
    {
        None,

        //Common UI
        UI_Logo,
        UI_CommonPopup,
        UI_FadePanel,
        UI_ToastMessage,
        UI_TouchDefense,
        UI_SceneTransition,
        UI_TweenContainer,

        //PreOutgame UI
        UI_Title,

        //Outgame UI
        UI_OutGame,
        UI_Skin,
        UI_Rank,
        UI_Quest,
        UI_Skill,
        UI_Shop,
        UI_Stats,
        UI_Setting,
        UI_RewardPopup,
        UI_PlayModePopup,
        UI_RankChangePopup,
        UI_ChangeNicknamePopup,
        UI_BuyPopup,

        //Ingame UI
        UI_InGameReady,
        UI_InGame,
        UI_InGameResult,

        //Etc
        UI_Debug,

    }

    public void ShowUI(UIType type)
    {
        UIBase ui = GetUI(type);

        if (ui == null)
        {
            Debug.Log("<color=red>Error...!" + type + "is null</color>");
            return;
        }

        if (ui.SafeIsActive())
            ui.Hide();
        ui.Show();

        if (activatedUIList.Contains(ui) == false)
        {
            activatedUIList.Add(ui);
        }
    }

    public void ShowUI(UIBase ui)
    {
        if (ui == null)
        {
            Debug.Log("<color=red>Error...! ui is null</color>");
            return;
        }

        if (ui.SafeIsActive())
            ui.Hide();
        ui.Show();

        if (activatedUIList.Contains(ui) == false)
        {
            activatedUIList.Add(ui);
        }
    }

    public void HideUI(UIType type)
    {
        UIBase ui = GetUI(type);

        if (ui == null)
        {
            Debug.Log("<color=red>Error...!" + type + "is null</color>");
            return;
        }

        ui.Hide();

        if (activatedUIList.Contains(ui))
            activatedUIList.Remove(ui);
    }

    public void HideUI(UIBase ui)
    {
        if (ui == null)
        {
            Debug.Log("<color=red>Error...! ui is null</color>");
            return;
        }

        ui.Hide();

        if (activatedUIList.Contains(ui))
            activatedUIList.Remove(ui);
    }

    private UIBase GetUI(UIType type)
    {
        UIBase ui = null;
        switch (type)
        {
            case UIType.None:
            default:
                break;

            case UIType.UI_Logo:
                ui = PrefabManager.Instance.UI_Logo;
                break;
            case UIType.UI_Title:
                ui = PrefabManager.Instance.UI_Title;
                break;
            case UIType.UI_ToastMessage:
                ui = PrefabManager.Instance.UI_ToastMessage;
                break;
            case UIType.UI_TouchDefense:
                ui = PrefabManager.Instance.UI_TouchDefense;
                break;
            case UIType.UI_SceneTransition:
                ui = PrefabManager.Instance.UI_SceneTransition;
                break;
            case UIType.UI_TweenContainer:
                ui = PrefabManager.Instance.UI_TweenContainer;
                break;
            case UIType.UI_OutGame:
                ui = PrefabManager.Instance.UI_OutGame;
                break;
            case UIType.UI_Skin:
                ui = PrefabManager.Instance.UI_Skin;
                break;
            case UIType.UI_Rank:
                ui = PrefabManager.Instance.UI_Rank;
                break;
            case UIType.UI_Quest:
                ui = PrefabManager.Instance.UI_Quest;
                break;
            case UIType.UI_Skill:
                ui = PrefabManager.Instance.UI_Skill;
                break;
            case UIType.UI_Shop:
                ui = PrefabManager.Instance.UI_Shop;
                break;
            case UIType.UI_Stats:
                ui = PrefabManager.Instance.UI_Stats;
                break;
            case UIType.UI_Setting:
                ui = PrefabManager.Instance.UI_Setting;
                break;
            case UIType.UI_RewardPopup:
                ui = PrefabManager.Instance.UI_RewardPopup;
                break;
            case UIType.UI_PlayModePopup:
                ui = PrefabManager.Instance.UI_PlayModePopup;
                break;
            case UIType.UI_RankChangePopup:
                ui = PrefabManager.Instance.UI_RankChangePopup;
                break;
            case UIType.UI_ChangeNicknamePopup:
                ui = PrefabManager.Instance.UI_ChangeNicknamePopup;
                break;
            case UIType.UI_BuyPopup:
                ui = PrefabManager.Instance.UI_BuyPopup;
                break;
            case UIType.UI_InGameReady:
                ui = PrefabManager.Instance.UI_InGameReady;
                break;
            case UIType.UI_InGame:
                ui = PrefabManager.Instance.UI_InGame;
                break;
            case UIType.UI_InGameResult:
                ui = PrefabManager.Instance.UI_InGameResult;
                break;
            case UIType.UI_CommonPopup:
                ui = PrefabManager.Instance.UI_CommonPopup;
                break;
            case UIType.UI_FadePanel:
                ui = PrefabManager.Instance.UI_FadePanel;
                break;
            case UIType.UI_Debug:
#if UNITY_EDITOR || SERVERTYPE_DEV
                ui = PrefabManager.Instance.UI_Debug;
#endif
                break;
        }

        return ui;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ActiveBackButton();
        }
    }

    #region BackButton Handler

    public List<IBackButtonHandler> registerdBackButtonHandlerList = new List<IBackButtonHandler>();

    public void RegisterBackButton(IBackButtonHandler ui)
    {
        if (registerdBackButtonHandlerList.Contains(ui) == false)
            registerdBackButtonHandlerList.Add(ui);
    }

    public void UnregisterBackButton(IBackButtonHandler ui)
    {
        if (registerdBackButtonHandlerList.Contains(ui) == true)
            registerdBackButtonHandlerList.Remove(ui);
    }

    public void ActiveBackButton()
    {
        var currPhase = PhaseManager.Instance.CurrentPhase;

        var backBtnHandler = registerdBackButtonHandlerList.FindLast(x => true);
        if (backBtnHandler != null)
        {
            if (currPhase == CommonDefine.Phase.OutGame
                || currPhase == CommonDefine.Phase.InGame)
            {
                backBtnHandler.OnBackButton();
            }
        }
        else
        {
            if (currPhase == CommonDefine.Phase.OutGame)
            {
                var commonUI = PrefabManager.Instance.UI_CommonPopup;
                //commonUI.ActivatePanelYesNo(EndApp, "END APP?", "Do you want to Completely Quit?", true);
            }
            else if (currPhase == CommonDefine.Phase.InGame)
            {

            }
        }
    }

    private void EndApp()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif

    }


    #endregion

    #region Methods
    public void HideGroup_All()
    {
        HideGrouped_Outgame();
        HideGrouped_Ingame();
    }

    public void HideGrouped_PreOutgame()
    {
        HideUI(UIType.UI_Logo);
        HideUI(UIType.UI_Title);
    }

    public void HideGrouped_Outgame()
    {
        HideUI(UIType.UI_CommonPopup);

        HideUI(UIType.UI_OutGame);
        HideUI(UIType.UI_Skin);
        HideUI(UIType.UI_Rank);
        HideUI(UIType.UI_Quest);
        HideUI(UIType.UI_Skill);
        HideUI(UIType.UI_Shop);
        HideUI(UIType.UI_Stats);
        HideUI(UIType.UI_Setting);
        HideUI(UIType.UI_RewardPopup);
        HideUI(UIType.UI_BuyPopup);
        HideUI(UIType.UI_PlayModePopup);
        HideUI(UIType.UI_RankChangePopup);
        HideUI(UIType.UI_ChangeNicknamePopup);
        HideUI(UIType.UI_InGameReady);
    }

    public void HideGrouped_Ingame()
    {
        HideUI(UIType.UI_CommonPopup);

        HideUI(UIType.UI_InGameReady);
        HideUI(UIType.UI_InGame);
        HideUI(UIType.UI_InGameResult);
    }
    #endregion
}
