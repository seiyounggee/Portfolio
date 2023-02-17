using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager_NGUI : MonoSingleton<UIManager_NGUI>
{
    [ReadOnly] public Transform ui_parent = null;

    [ReadOnly] public List<UI_PanelBase> panelActivated = new List<UI_PanelBase>();

    public enum UIType
    {
        None = 0,
        UI_PanelSprite,
        UI_PanelLobby_Menu,
        UI_PanelLobby_Garage,
        UI_PanelLobby_Character,
        UI_PanelLobby_Main,
        UI_PanelLobby_Shop,
        UI_PanelLobby_Quest,
        UI_PanelInGame,
        UI_PanelCommon,
        UI_PanelLoading,
        UI_PanelNickname,
    }

    private void Start()
    {
        if (ui_parent == null)
        {
            ui_parent = UIRoot_Base.Instance.transform;
        }
    }

    public void SetActvatedPanelList(UI_PanelBase panel, bool isSetActive, UI_PanelBase.Depth depth = UI_PanelBase.Depth.Normal)
    {
        if (isSetActive == true)
        {
            if (panelActivated.Contains(panel) == false)
            {
                panelActivated.Add(panel);

                int newDepth = panelActivated.Count;
                if (depth == UI_PanelBase.Depth.SuperLow)
                    newDepth -= 2000;
                else if (depth == UI_PanelBase.Depth.Low)
                    newDepth -= 1000;
                else if (depth == UI_PanelBase.Depth.High)
                    newDepth += 1000;
                else if (depth == UI_PanelBase.Depth.SuperHigh)
                    newDepth += 2000;
                panel.SetDepth(depth, newDepth);
            }
        }
        else
        {
            if (panelActivated.Contains(panel))
            {
                int removedPanelDepth = panel.GetDepthNum();
                panelActivated.Remove(panel);

                foreach (var p in panelActivated)
                {
                    if (p.GetDepthNum() > removedPanelDepth)
                    {
                        int depthNum = p.GetDepthNum() - 1;
                        if (depth == UI_PanelBase.Depth.SuperLow)
                            depthNum -= 2000;
                        else if (depth == UI_PanelBase.Depth.Low)
                            depthNum -= 1000;
                        else if (depth == UI_PanelBase.Depth.High)
                            depthNum += 1000;
                        else if (depth == UI_PanelBase.Depth.SuperHigh)
                            depthNum += 2000;

                        p.SetDepth(depth, depthNum);
                    }
                }
            }
        }
    }

    public void ActivateUI(UIType type, UI_PanelBase.Depth depth = UI_PanelBase.Depth.Normal)
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
            case UIType.UI_PanelLobby_Menu:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Menu;
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


            case UIType.UI_PanelCommon:
                {
                    //얘는 따로 Callback 지정해서...
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

        if (PrefabManager.Instance.UI_PanelLobby_Menu.gameObject.activeSelf == false)
            PrefabManager.Instance.UI_PanelLobby_Menu.Show();
        else
            PrefabManager.Instance.UI_PanelLobby_Menu.SetDepthToTop();
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
            case UIType.UI_PanelLobby_Menu:
                {
                    var ui = PrefabManager.Instance.UI_PanelLobby_Menu;
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

            case UIType.UI_PanelCommon:
                {

                }
                break;
        }
    }

    public void DeactivateLobbyUI()
    {
        DeactivateUI(UIType.UI_PanelLobby_Menu);
        DeactivateUI(UIType.UI_PanelLobby_Garage);
        DeactivateUI(UIType.UI_PanelLobby_Character);
        DeactivateUI(UIType.UI_PanelLobby_Main);
        DeactivateUI(UIType.UI_PanelLobby_Shop);
        DeactivateUI(UIType.UI_PanelLobby_Quest);
    }
}
