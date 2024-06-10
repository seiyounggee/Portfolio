using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoSingleton<PrefabManager>
{
    [ReadOnly] public Transform ui_parent = null;

    private const string FILE_PATH_GAME = "UnityPrefabs/Game/";
    private const string FILE_PATH_GAME_EFFECT = "UnityPrefabs/Game/Effect/";
    private const string FILE_PATH_UI = "UnityPrefabs/UI/";

    private void Start()
    {
        if (ui_parent == null)
        {
            ui_parent = UICanvas_Parent.Instance.transform;
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
        go.transform.SetParent(parent);
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
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale = Vector3.one;
        go.SetActive(false);
        return go;
    }

    #endregion //Function


    #region InGame Prefabs

    public GameObject Unity_PlayerCharacter
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME + "Unity_PlayerCharacter") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Unity_PlayerCharacter");

            return null;
        }
    }

    public GameObject Unity_Ball
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME + "Unity_Ball") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Unity_Ball");

            return null;
        }
    }

    #region Effect
    public GameObject Effect_Slash
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_Slash") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_Slash");

            return null;
        }
    }

    public GameObject Effect_PickupExplodeYellow
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_PickupExplodeYellow") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_PickupExplodeYellow");

            return null;
        }
    }



    public GameObject Effect_MagicShieldBlue
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_MagicShieldBlue") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_MagicShieldBlue");

            return null;
        }
    }

    public GameObject Effect_ShieldSoftPurple
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_ShieldSoftPurple") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_ShieldSoftPurple");

            return null;
        }
    }


    public GameObject Effect_SoftRadialPunchMedium
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_SoftRadialPunchMedium") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_SoftRadialPunchMedium");

            return null;
        }
    }

    public GameObject Effect_SoulGenericDeath
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_SoulGenericDeath") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_SoulGenericDeath");

            return null;
        }
    }

    public GameObject Effect_StunExplosion
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_StunExplosion") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_StunExplosion");

            return null;
        }
    }

    public GameObject Effect_SwordHitBlueCritical
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_SwordHitBlueCritical") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_SwordHitBlueCritical");

            return null;
        }
    }
    

    public GameObject Effect_TargetHitExplosion
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_TargetHitExplosion") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_TargetHitExplosion");

            return null;
        }
    }

    public GameObject Effect_Frost
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_Frost") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_Frost");

            return null;
        }
    }

    public GameObject Effect_MagicBuffYellow
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_MagicBuffYellow") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_MagicBuffYellow");

            return null;
        }
    }

    public GameObject Effect_MagicEnchantYellow
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_MagicEnchantYellow") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_MagicEnchantYellow");

            return null;
        }
    }

    public GameObject Effect_SwordWaveWhite
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_SwordWaveWhite") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_SwordWaveWhite");

            return null;
        }
    }

    public GameObject Effect_MagicCircleSimpleYellow
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_MagicCircleSimpleYellow") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_MagicCircleSimpleYellow");

            return null;
        }
    }

    public GameObject Effect_LaserBeamYellow
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_LaserBeamYellow") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_LaserBeamYellow");

            return null;
        }
    }

    public GameObject Effect_LaserBeamRed
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_LaserBeamRed") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_LaserBeamRed");

            return null;
        }
    }

    public GameObject Effect_LightningOrbSharpPink
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_LightningOrbSharpPink") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_LightningOrbSharpPink");

            return null;
        }
    }

    public GameObject Effect_StormMissile
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_StormMissile") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_StormMissile");

            return null;
        }
    }

    public GameObject Effect_MysticMissileDark
    {
        get
        {
            GameObject pObj = UnityEngine.Resources.Load(FILE_PATH_GAME_EFFECT + "Effect_MysticMissileDark") as GameObject;
            if (pObj != null)
                return pObj;
            else
                Debug.LogError("Not Found Effect_LaserBeamRed");

            return null;
        }
    }

    #endregion

    #endregion

    #region UI Prefabs

    public UI_Title UI_Title
    {
        get
        {
            if (ui_title == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Title", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_title = pObj.GetComponent<UI_Title>();

                    if (ui_title == null)
                        Debug.LogError("No UI_Title Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Title");
            }

            return ui_title;
        }
    }

    private UI_Title ui_title = null;

    public UI_Logo UI_Logo
    {
        get
        {
            if (ui_logo == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Logo", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_logo = pObj.GetComponent<UI_Logo>();
                    if (ui_logo == null)
                        Debug.LogError("No UI_Logo Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Logo");
            }

            return ui_logo;
        }
    }

    private UI_Logo ui_logo = null;

    public UI_ToastMessage UI_ToastMessage
    {
        get
        {
            if (ui_ToastMessage == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_ToastMessage", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_ToastMessage = pObj.GetComponent<UI_ToastMessage>();

                    if (ui_ToastMessage == null)
                        Debug.LogError("No UI_ToastMessage Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_ToastMessage");
            }

            return ui_ToastMessage;
        }
    }

    private UI_ToastMessage ui_ToastMessage = null;

    public UI_TouchDefense UI_TouchDefense
    {
        get
        {
            if (ui_TouchDefense == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_TouchDefense", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_TouchDefense = pObj.GetComponent<UI_TouchDefense>();

                    if (ui_TouchDefense == null)
                        Debug.LogError("No UI_TouchDefense Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_TouchDefense");
            }

            return ui_TouchDefense;
        }
    }

    private UI_TouchDefense ui_TouchDefense = null;

    public UI_SceneTransition UI_SceneTransition
    {
        get
        {
            if (ui_SceneTransition == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_SceneTransition", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_SceneTransition = pObj.GetComponent<UI_SceneTransition>();

                    if (ui_SceneTransition == null)
                        Debug.LogError("No UI_SceneTransition Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_SceneTransition");
            }

            return ui_SceneTransition;
        }
    }

    private UI_SceneTransition ui_SceneTransition = null;

    public UI_TweenContainer UI_TweenContainer
    {
        get
        {
            if (ui_TweenContainer == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_TweenContainer", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_TweenContainer = pObj.GetComponent<UI_TweenContainer>();

                    if (ui_TweenContainer == null)
                        Debug.LogError("No UI_TweenContainer Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_TweenContainer");
            }

            return ui_TweenContainer;
        }
    }

    private UI_TweenContainer ui_TweenContainer = null;

    public UI_OutGame UI_OutGame
    {
        get
        {
            if (ui_OutGame == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_OutGame", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_OutGame = pObj.GetComponent<UI_OutGame>();
                    if (ui_OutGame == null)
                        Debug.LogError("No UI_OutGame Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_OutGame");
            }

            return ui_OutGame;
        }
    }

    private UI_OutGame ui_OutGame = null;

    public UI_Skin UI_Skin
    {
        get
        {
            if (ui_Skin == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Skin", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Skin = pObj.GetComponent<UI_Skin>();
                    if (ui_Skin == null)
                        Debug.LogError("No UI_Skin Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Skin");
            }

            return ui_Skin;
        }
    }

    private UI_Skin ui_Skin = null;

    public UI_Rank UI_Rank
    {
        get
        {
            if (ui_Rank == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Rank", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Rank = pObj.GetComponent<UI_Rank>();
                    if (ui_Rank == null)
                        Debug.LogError("No UI_Rank Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Rank");
            }

            return ui_Rank;
        }
    }

    private UI_Rank ui_Rank = null;

    public UI_Quest UI_Quest
    {
        get
        {
            if (ui_Quest == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Quest", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Quest = pObj.GetComponent<UI_Quest>();
                    if (ui_Quest == null)
                        Debug.LogError("No UI_Rank Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Rank");
            }

            return ui_Quest;
        }
    }

    private UI_Quest ui_Quest = null;

    public UI_Skill UI_Skill
    {
        get
        {
            if (ui_Skill == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Skill", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Skill = pObj.GetComponent<UI_Skill>();
                    if (ui_Skill == null)
                        Debug.LogError("No UI_Skill Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Skill");
            }

            return ui_Skill;
        }
    }

    private UI_Skill ui_Skill = null;

    public UI_Shop UI_Shop
    {
        get
        {
            if (ui_Shop == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Shop", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Shop = pObj.GetComponent<UI_Shop>();
                    if (ui_Shop == null)
                        Debug.LogError("No UI_Shop Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Shop");
            }

            return ui_Shop;
        }
    }

    private UI_Shop ui_Shop = null;

    public UI_Stats UI_Stats
    {
        get
        {
            if (ui_Stats == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Stats", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Stats = pObj.GetComponent<UI_Stats>();
                    if (ui_Stats == null)
                        Debug.LogError("No UI_Stats Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Stats");
            }

            return ui_Stats;
        }
    }

    private UI_Stats ui_Stats = null;

    public UI_Setting UI_Setting
    {
        get
        {
            if (ui_Setting == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Setting", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Setting = pObj.GetComponent<UI_Setting>();
                    if (ui_Setting == null)
                        Debug.LogError("No UI_Setting Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Setting");
            }

            return ui_Setting;
        }
    }

    private UI_Setting ui_Setting = null;

    public UI_RewardPopup UI_RewardPopup
    {
        get
        {
            if (ui_RewardPopup == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_RewardPopup", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_RewardPopup = pObj.GetComponent<UI_RewardPopup>();
                    if (ui_RewardPopup == null)
                        Debug.LogError("No UI_RewardPopup Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_RewardPopup");
            }

            return ui_RewardPopup;
        }
    }

    private UI_RewardPopup ui_RewardPopup = null;

    public UI_PlayModePopup UI_PlayModePopup
    {
        get
        {
            if (ui_PlayModePopup == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_PlayModePopup", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_PlayModePopup = pObj.GetComponent<UI_PlayModePopup>();
                    if (ui_PlayModePopup == null)
                        Debug.LogError("No UI_PlayModePopup Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_PlayModePopup");
            }

            return ui_PlayModePopup;
        }
    }

    private UI_PlayModePopup ui_PlayModePopup = null;

    public UI_RankChangePopup UI_RankChangePopup
    {
        get
        {
            if (ui_RankChangePopup == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_RankChangePopup", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_RankChangePopup = pObj.GetComponent<UI_RankChangePopup>();
                    if (ui_RankChangePopup == null)
                        Debug.LogError("No UI_RankChangePopup Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_RankChangePopup");
            }

            return ui_RankChangePopup;
        }
    }

    private UI_RankChangePopup ui_RankChangePopup = null;

    public UI_ChangeNicknamePopup UI_ChangeNicknamePopup
    {
        get
        {
            if (ui_ChangeNicknamePopup == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_ChangeNicknamePopup", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_ChangeNicknamePopup = pObj.GetComponent<UI_ChangeNicknamePopup>();
                    if (ui_ChangeNicknamePopup == null)
                        Debug.LogError("No UI_ChangeNicknamePopup Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_ChangeNicknamePopup");
            }

            return ui_ChangeNicknamePopup;
        }
    }

    private UI_ChangeNicknamePopup ui_ChangeNicknamePopup = null;

    public UI_BuyPopup UI_BuyPopup
    {
        get
        {
            if (ui_BuyPopup == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_BuyPopup", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_BuyPopup = pObj.GetComponent<UI_BuyPopup>();
                    if (ui_BuyPopup == null)
                        Debug.LogError("No UI_BuyPopup Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_BuyPopup");
            }

            return ui_BuyPopup;
        }
    }

    private UI_BuyPopup ui_BuyPopup = null;

    public UI_InGameReady UI_InGameReady
    {
        get
        {
            if (ui_InGameReady == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_InGameReady", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_InGameReady = pObj.GetComponent<UI_InGameReady>();
                    if (ui_InGameReady == null)
                        Debug.LogError("No UI_InGameReady Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_InGameReady");
            }

            return ui_InGameReady;
        }
    }

    private UI_InGameReady ui_InGameReady = null;

    public UI_InGame UI_InGame
    {
        get
        {
            if (ui_InGame == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_InGame", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_InGame = pObj.GetComponent<UI_InGame>();
                    if (ui_InGame == null)
                        Debug.LogError("No UI_InGame Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_InGame");
            }

            return ui_InGame;
        }
    }

    private UI_InGame ui_InGame = null;

    public UI_InGameResult UI_InGameResult
    {
        get
        {
            if (ui_InGameResult == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_InGameResult", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_InGameResult = pObj.GetComponent<UI_InGameResult>();
                    if (ui_InGameResult == null)
                        Debug.LogError("No UI_InGameResult Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_InGameResult");
            }

            return ui_InGameResult;
        }
    }

    private UI_InGameResult ui_InGameResult = null;

    public UI_CommonPopup UI_CommonPopup
    {
        get
        {
            if (ui_CommonPopup == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_CommonPopup", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_CommonPopup = pObj.GetComponent<UI_CommonPopup>();
                    if (ui_CommonPopup == null)
                        Debug.LogError("No UI_CommonPopup Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_CommonPopup");
            }

            return ui_CommonPopup;
        }
    }

    private UI_CommonPopup ui_CommonPopup = null;

    public UI_FadePanel UI_FadePanel
    {
        get
        {
            if (ui_FadePanel == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_FadePanel", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_FadePanel = pObj.GetComponent<UI_FadePanel>();
                    if (ui_FadePanel == null)
                        Debug.LogError("No UI_FadePanel Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_FadePanel");
            }

            return ui_FadePanel;
        }
    }

    private UI_FadePanel ui_FadePanel = null;

    public UI_Debug UI_Debug
    {
        get
        {
            if (ui_Debug == null)
            {
                GameObject pObj = InstantiateUIPrefab(FILE_PATH_UI + "UI_Debug", ui_parent, Vector3.zero);
                if (pObj != null)
                {
                    ui_Debug = pObj.GetComponent<UI_Debug>();
                    if (ui_Debug == null)
                        Debug.LogError("No UI_Debug Script Attached!");
                }
                else
                    Debug.LogError("Not Found UI_Debug");
            }

            return ui_Debug;
        }
    }

    private UI_Debug ui_Debug = null;

    #endregion
}
