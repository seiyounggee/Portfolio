using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoSingleton<PrefabManager>
{
    [ReadOnly] public Transform ngui_parent = null;

    private void Start()
    {
        if (ngui_parent == null)
        {
            ngui_parent = UIRoot_Base.Instance.transform;
        }
    }

    #region Functions

    public static GameObject InstantiateInGamePrefab(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("obj is null");
            return null;
        }

        GameObject go = Instantiate(obj);
        return go;
    }

    public static GameObject InstantiateInGamePrefab(GameObject obj, Vector3 pos, Transform parent = null)
    {
        if (obj == null)
        {
            Debug.LogError("obj is null");
            return null;
        }

        GameObject go = Instantiate(obj, parent);
        go.transform.position = pos;
        return go;
    }


    public static GameObject InstantiateInGamePrefab(GameObject obj, Vector3 pos, Vector3 rot, Transform parent = null)
    {
        if (obj == null)
        {
            Debug.LogError("obj is null");
            return null;
        }

        GameObject go = Instantiate(obj, parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(rot);
        return go;
    }


    public static GameObject InstantiateInGamePrefab(string path, Transform parent, Vector3 pos)
    {
        UnityEngine.Object obj = UnityEngine.Resources.Load(path);
        if (obj == null)
        {
            Debug.LogError("load failed : " + path);
            return null;
        }

        GameObject go = (GameObject)UnityEngine.Object.Instantiate(obj, parent);
        go.transform.parent = parent;
        go.transform.localPosition = pos;
        return go;
    }

    public static GameObject InstantiateUIPrefab(string path, Transform parent, Vector3 pos)
    {
        UnityEngine.Object obj = UnityEngine.Resources.Load(path);
        if (obj == null)
        {
            Debug.LogError("load failed : " + path);
            return null;
        }

        GameObject go = (GameObject)UnityEngine.Object.Instantiate(obj, parent);
        go.transform.parent = parent;
        go.transform.localPosition = pos;
        go.transform.localScale = Vector3.one;
        go.SetActive(false);
        return go;
    }

    #endregion //Function


    #region InGame Prefabs

    public GameObject NetworkPlayer
    {
        get
        {
            string path = "Prefabs/InGame/NetworkPlayer";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject NetworkPlayer_AI
    {
        get
        {
            string path = "Prefabs/InGame/NetworkPlayer_AI";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject PlayerCar_FX
    {
        get
        {
            string path = "Prefabs/InGame/PlayerCar_FX";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject NetworkInGameRPCManager
    {
        get
        {
            string path = "Prefabs/InGame/NetworkInGameRPCManager";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject OutGamePlayer
    {
        get
        {
            string path = "Prefabs/OutGame/OutGamePlayer";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject DummyPlayer
    {
        get
        {
            string path = "Prefabs/Common/DummyPlayer";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject OutGameBackground
    {
        get
        {
            string path = "Prefabs/OutGame/OutGameBackground";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject NetworkFoodTruck
    {
        get
        {
            string path = "Prefabs/InGame/NetworkFoodTruck";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    #region FX
    public GameObject FX_Hit_01
    {
        get
        {
            string path = "Prefabs/FX/FX_Hit_01";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject FX_Hit_02
    {
        get
        {
            string path = "Prefabs/FX/FX_Hit_02";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject FX_Hit_03
    {
        get
        {
            string path = "Prefabs/FX/FX_Hit_03";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject FX_Charging_Once_Blue
    {
        get
        {
            string path = "Prefabs/FX/FX_Charging_Once_Blue";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject FX_Charging_Once_Red
    {
        get
        {
            string path = "Prefabs/FX/FX_Charging_Once_Red";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject FX_Charging_Once_Yellow
    {
        get
        {
            string path = "Prefabs/FX/FX_Charging_Once_Yellow";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }


    public GameObject FX_Waterdrowning
    {
        get
        {
            string path = "Prefabs/FX/FX_Waterdrowning";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }
    #endregion

    public GameObject MapObject_ChargePad
    {
        get
        {
            string path = "Prefabs/InGame/MapObject_ChargePad";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject MapObject_Obstacle
    {
        get
        {
            string path = "Prefabs/InGame/MapObject_Obstacle";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject MapObject_TimingPad
    {
        get
        {
            string path = "Prefabs/InGame/MapObject_TimingPad";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject MiniMap_LineRenderer
    {
        get
        {
            string path = "Prefabs/InGame/MiniMap_LineRenderer";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject MiniMap_Player
    {
        get
        {
            string path = "Prefabs/InGame/MiniMap_Player";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    public GameObject MiniMap_Camera
    {
        get
        {
            string path = "Prefabs/InGame/MiniMap_Camera";
            UnityEngine.Object obj = UnityEngine.Resources.Load(path);
            if (obj == null)
            {
                Debug.LogError("load failed : " + path);
                return null;
            }

            return (GameObject)obj;
        }
    }

    #endregion

    #region NGUI UI Prefabs

    public UI_PanelIngame UI_PanelIngame
    {
        get
        {
            if (ui_PanelIngame == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelIngame", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelIngame = pObj.GetComponent<UI_PanelIngame>();
                    if (ui_PanelIngame == null)
                        Debug.LogError("No UI_PanelIngame Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelIngame");
            }

            return ui_PanelIngame;
        }
    }

    private UI_PanelIngame ui_PanelIngame = null;

    public UI_PanelLobby_TabMenu UI_PanelLobby_TabMenu
    {
        get
        {
            if (ui_PanelLobby_TabMenu == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/New/PanelLobby_TabMenu", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLobby_TabMenu = pObj.GetComponent<UI_PanelLobby_TabMenu>();
                    if (ui_PanelLobby_TabMenu == null)
                        Debug.LogError("No UI_PanelLobby_TabMenu Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelLobby_TabMenu");
            }

            return ui_PanelLobby_TabMenu;
        }
    }

    private UI_PanelLobby_TabMenu ui_PanelLobby_TabMenu = null;

    public UI_PanelLobby_Garage UI_PanelLobby_Garage
    {
        get
        {
            if (ui_PanelLobby_Garage == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelLobby_Garage", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLobby_Garage = pObj.GetComponent<UI_PanelLobby_Garage>();
                    if (ui_PanelLobby_Garage == null)
                        Debug.LogError("No UI_PanelLobby_Garage Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelLobby_Garage");
            }

            return ui_PanelLobby_Garage;
        }
    }

    private UI_PanelLobby_Garage ui_PanelLobby_Garage = null;

    public UI_PanelLobby_Character UI_PanelLobby_Character
    {
        get
        {
            if (ui_PanelLobby_Character == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelLobby_Character", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLobby_Character = pObj.GetComponent<UI_PanelLobby_Character>();
                    if (ui_PanelLobby_Character == null)
                        Debug.LogError("No UI_PanelLobby_Character Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelLobby_Character");
            }

            return ui_PanelLobby_Character;
        }
    }

    private UI_PanelLobby_Character ui_PanelLobby_Character = null;

    public UI_PanelLobby_Main UI_PanelLobby_Main
    {
        get
        {
            if (ui_PanelLobby_Main == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/New/PanelLobby_Main", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLobby_Main = pObj.GetComponent<UI_PanelLobby_Main>();
                    if (ui_PanelLobby_Main == null)
                        Debug.LogError("No PanelLobby_Main Script Attached!");
                }
                else
                    Debug.LogError("Not Found PanelLobby_Main");
            }

            return ui_PanelLobby_Main;
        }
    }

    private UI_PanelLobby_Main ui_PanelLobby_Main = null;

    public UI_PanelLobby_Shop UI_PanelLobby_Shop
    {
        get
        {
            if (ui_PanelLobby_Shop == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelLobby_Shop", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLobby_Shop = pObj.GetComponent<UI_PanelLobby_Shop>();
                    if (ui_PanelLobby_Shop == null)
                        Debug.LogError("No UI_PanelLobby_Shop Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelLobby_Shop");
            }

            return ui_PanelLobby_Shop;
        }
    }

    private UI_PanelLobby_Shop ui_PanelLobby_Shop = null;

    public UI_PanelLobby_Quest UI_PanelLobby_Quest
    {
        get
        {
            if (ui_PanelLobby_Quest == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelLobby_Quest", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLobby_Quest = pObj.GetComponent<UI_PanelLobby_Quest>();
                    if (ui_PanelLobby_Quest == null)
                        Debug.LogError("No UI_PanelLobby_Quest Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelLobby_Quest");
            }

            return ui_PanelLobby_Quest;
        }
    }

    private UI_PanelLobby_Quest ui_PanelLobby_Quest = null;

    public UI_PanelSprite UI_PanelSprite
    {
        get
        {
            if (ui_PanelSprite == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelSprite", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelSprite = pObj.GetComponent<UI_PanelSprite>();
                    if (ui_PanelSprite == null)
                        Debug.LogError("No UI_PanelSprite Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelSprite");
            }

            return ui_PanelSprite;
        }
    }

    private UI_PanelSprite ui_PanelSprite = null;

    public UI_PanelLoading UI_PanelLoading
    {
        get
        {
            if (ui_PanelLoading == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelLoading", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelLoading = pObj.GetComponent<UI_PanelLoading>();
                    if (ui_PanelLoading == null)
                        Debug.LogError("No UI_PanelLoading Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelLoading");
            }

            return ui_PanelLoading;
        }
    }

    private UI_PanelLoading ui_PanelLoading = null;

    public UI_PanelPopup_Default UI_PanelPopup_Default
    {
        get
        {
            if (ui_PanelPopup_Default == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/New/PanelPopup_Default", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelPopup_Default = pObj.GetComponent<UI_PanelPopup_Default>();
                    if (ui_PanelPopup_Default == null)
                        Debug.LogError("No UI_PanelPopup_Default Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelPopup_Default");
            }

            return ui_PanelPopup_Default;
        }
    }

    private UI_PanelPopup_Default ui_PanelPopup_Default = null;

    public UI_PanelDebugTool UI_PanelDebugTool
    {
        get
        {
            if (ui_PanelDebugTool == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelDebugTool", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelDebugTool = pObj.GetComponent<UI_PanelDebugTool>();
                    if (ui_PanelDebugTool == null)
                        Debug.LogError("No UI_PanelDebugTool Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelDebugTool");
            }

            return ui_PanelDebugTool;
        }
    }

    private UI_PanelDebugTool ui_PanelDebugTool = null;


    public UI_PanelNickname UI_PanelNickname
    {
        get
        {
            if (ui_PanelNickname == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelNickname", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelNickname = pObj.GetComponent<UI_PanelNickname>();
                    if (ui_PanelNickname == null)
                        Debug.LogError("No UI_PanelNickname Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelNickname");
            }

            return ui_PanelNickname;
        }
    }

    private UI_PanelNickname ui_PanelNickname = null;

    public UI_PanelTouchDefence UI_PanelTouchDefence
    {
        get
        {
            if (ui_PanelTouchDefence == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/PanelTouchDefence", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelTouchDefence = pObj.GetComponent<UI_PanelTouchDefence>();
                    if (ui_PanelTouchDefence == null)
                        Debug.LogError("No UI_PanelTouchDefence Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelTouchDefence");
            }

            return ui_PanelTouchDefence;
        }
    }

    private UI_PanelTouchDefence ui_PanelTouchDefence = null;


    public UI_PanelPopup_Drone UI_PanelPopup_Drone
    {
        get
        {
            if (ui_PanelPopup_Drone == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/New/PanelPopup_Drone", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelPopup_Drone = pObj.GetComponent<UI_PanelPopup_Drone>();
                    if (ui_PanelPopup_Drone == null)
                        Debug.LogError("No UI_PanelPopup_Drone Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelPopup_Drone");
            }

            return ui_PanelPopup_Drone;
        }
    }

    private UI_PanelPopup_Drone ui_PanelPopup_Drone = null;

    public UI_PanelPopup_RewardBoxInfo UI_PanelPopup_RewardBoxInfo
    {
        get
        {
            if (ui_PanelPopup_RewardBoxInfo == null)
            {
                GameObject pObj = InstantiateUIPrefab("UI/New/PanelPopup_RewardBoxInfo", ngui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PanelPopup_RewardBoxInfo = pObj.GetComponent<UI_PanelPopup_RewardBoxInfo>();
                    if (ui_PanelPopup_RewardBoxInfo == null)
                        Debug.LogError("No UI_PanelPopup_RewardBoxInfo Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PanelPopup_RewardBoxInfo");
            }

            return ui_PanelPopup_RewardBoxInfo;
        }
    }

    private UI_PanelPopup_RewardBoxInfo ui_PanelPopup_RewardBoxInfo = null;

    #endregion
}
