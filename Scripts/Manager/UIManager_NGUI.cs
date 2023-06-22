using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UI_Base;
using StylizedWater2;

public class UIManager_NGUI : MonoSingleton<UIManager_NGUI>
{
    [ReadOnly] public Transform ui_parent = null;

    [ReadOnly] public List<UI_Base> uiActivated = new List<UI_Base>();

    public enum UIType
    {
        None = 0,
        UI_PanelSprite,
        UI_PanelLobby_TabMenu,
        UI_PanelLobby_Garage,
        UI_PanelLobby_Character,
        UI_PanelLobby_Main,
        UI_PanelLobby_Shop,
        UI_PanelLobby_Quest,
        UI_PanelInGame,
        UI_PanelLoading,
        UI_PanelNickname,
        UI_PanelTouchDefence,

        UI_PopupDefault,
        UI_PopupDrone,
        UI_PopupRewardBoxInfo,
    }

    private void Start()
    {
        if (ui_parent == null)
        {
            ui_parent = UIRoot_Base.Instance.transform;
        }
    }

    public void SetActvatedUIBaseList(UI_Base uiBase, bool isSetActive, UI_Base.Depth depth = UI_Base.Depth.Normal)
    {
        if (isSetActive == true)
        {
            if (uiActivated.Contains(uiBase) == false)
            {
                uiActivated.Add(uiBase);

                int newDepth = uiActivated.Count;
                if (depth == UI_Base.Depth.SuperLow)
                    newDepth -= 2000;
                else if (depth == UI_Base.Depth.Low)
                    newDepth -= 1000;
                else if (depth == UI_Base.Depth.High)
                    newDepth += 1000;
                else if (depth == UI_Base.Depth.SuperHigh)
                    newDepth += 2000;
                uiBase.SetDepth(depth, newDepth);
            }
        }
        else
        {
            if (uiActivated.Contains(uiBase))
            {
                int removedPanelDepth = uiBase.GetDepthNum();
                uiActivated.Remove(uiBase);

                foreach (var p in uiActivated)
                {
                    if (p.GetDepthNum() > removedPanelDepth)
                    {
                        int depthNum = p.GetDepthNum() - 1;
                        if (depth == UI_Base.Depth.SuperLow)
                            depthNum -= 2000;
                        else if (depth == UI_Base.Depth.Low)
                            depthNum -= 1000;
                        else if (depth == UI_Base.Depth.High)
                            depthNum += 1000;
                        else if (depth == UI_Base.Depth.SuperHigh)
                            depthNum += 2000;

                        p.SetDepth(depth, depthNum);
                    }
                }
            }
        }
    }

    public void ActivateUI(UIType type, UI_Base.Depth depth = UI_Base.Depth.Normal)
    {
        switch (type)
        {
            case UIType.None:
                break;
            case UIType.UI_PanelSprite:
                {
                    var ui = PrefabManager.Instance.UI_PanelSprite;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLobby_TabMenu:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_TabMenu;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLobby_Garage:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Garage;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLobby_Character:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Character;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLobby_Main:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Main;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLobby_Shop:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Shop;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLobby_Quest:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Quest;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelInGame:
                {
                    var ui = PrefabManager.Instance.UI_PanelIngame;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelLoading:
                {
                    var ui = PrefabManager.Instance.UI_PanelLoading;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelNickname:
                {
                    var ui = PrefabManager.Instance.UI_PanelNickname;
                    ui.Show(depth);
                }
                break;
            case UIType.UI_PanelTouchDefence:
                {
                    var ui = PrefabManager.Instance.UI_PanelTouchDefence;
                    depth = Depth.SuperHigh;
                    ui.Show(depth);
                }
                break;


            case UIType.UI_PopupDefault:
                {
                    //얘는 밑에 Method 사용하자 (callback 지정해야하니까~)
                    Debug.Log("Use ActivatePanelCommonUI() Method!");
                }
                break;

            case UIType.UI_PopupDrone:
                {
                    var ui = PrefabManager.Instance.UI_PanelPopup_Drone;
                    ui.Show(depth);
                }
                break;

            case UIType.UI_PopupRewardBoxInfo:
                {
                    var ui = PrefabManager.Instance.UI_PanelPopup_RewardBoxInfo;
                    ui.Show(depth);
                }
                break;
        }
    }

    public void ActivateLobbyUI(UIType type)
    {

        for (int i = (int)UIType.UI_PanelLobby_Garage; i <= (int)UIType.UI_PanelLobby_Quest; i++)
        {
            if (i == (int)type)
            {
                ActivateUI((UIType)i);
            }
            else
            {
                DeactivateUI((UIType)i);
            }
        }

        PrefabManager.Instance.UI_PanelLobby_TabMenu.SetDepthToTop();
    }

    public void ActivatePanelDefault_YesNo(string msg = "", Action callback_yes = null, Action callback_no = null)
    {
        var ui = PrefabManager.Instance.UI_PanelPopup_Default;
        ui.SetData_YesNo(msg, string.Empty, () => { callback_yes?.Invoke(); }, () => { callback_no?.Invoke(); });
        ui.Show(Depth.High);
    }

    public void ActivatePanelDefault_Confirm(string msg = "", Action callback_confirm = null)
    {
        var ui = PrefabManager.Instance.UI_PanelPopup_Default;
        ui.SetData_Center(msg, string.Empty, () => { callback_confirm?.Invoke(); });
        ui.Show(Depth.High);
    }

    public void DeactivateUI(UIType type)
    {
        switch (type)
        {
            case UIType.None:
                break;
            case UIType.UI_PanelSprite:
                {
                    var ui = PrefabManager.Instance.UI_PanelSprite;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLobby_TabMenu:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_TabMenu;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLobby_Garage:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Garage;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLobby_Character:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Character;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLobby_Main:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Main;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLobby_Shop:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Shop;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLobby_Quest:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Quest;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelInGame:
                {
                    var ui = PrefabManager.Instance.UI_PanelIngame;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelLoading:
                {
                    var ui = PrefabManager.Instance.UI_PanelLoading;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelNickname:
                {
                    var ui = PrefabManager.Instance.UI_PanelNickname;
                    ui.Close();
                }
                break;
            case UIType.UI_PanelTouchDefence:
                {
                    var ui = PrefabManager.Instance.UI_PanelTouchDefence;
                    ui.Close();
                }
                break;

            case UIType.UI_PopupDefault:
                {
                    var ui = PrefabManager.Instance.UI_PanelPopup_Default;
                    ui.Close();
                }
                break;

            case UIType.UI_PopupDrone:
                {
                    var ui = PrefabManager.Instance.UI_PanelPopup_Drone;
                    ui.Close();
                }
                break;

            case UIType.UI_PopupRewardBoxInfo:
                {
                    var ui = PrefabManager.Instance.UI_PanelPopup_RewardBoxInfo;
                    ui.Close();
                }
                break;
        }
    }

    public void DeactivateLobbyUI()
    {
        DeactivateUI(UIType.UI_PanelLobby_TabMenu);
        DeactivateUI(UIType.UI_PanelLobby_Garage);
        DeactivateUI(UIType.UI_PanelLobby_Character);
        DeactivateUI(UIType.UI_PanelLobby_Main);
        DeactivateUI(UIType.UI_PanelLobby_Shop);
        DeactivateUI(UIType.UI_PanelLobby_Quest);
    }
}
