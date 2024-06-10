using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_OutGame : UIBase
{
    [Header("Top Left")]
    [SerializeField] GameObject topLeftBarObj;
    [SerializeField] Button nameBtn;
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] TextMeshProUGUI pidTxt;
    [Header("Top Right")]
    [SerializeField] GameObject topRightBarObj;
    [SerializeField] UIComponent_Currency currency;

    [Header("Center")]
    [SerializeField] GameObject centerGroupObj;
    [SerializeField] UIComponent_RotateArea rotateArea;
    [SerializeField] Button activeSkillBtn;
    [SerializeField] RawImage activeSkillImage;
    [SerializeField] GameObject activeSkillNoneImage;
    [SerializeField] Button passiveSkillBtn;
    [SerializeField] RawImage passiveSkillImage;
    [SerializeField] GameObject passiveSkillNoneImage;

    [Header("Bottom Right")]
    [SerializeField] GameObject bottomRightGroupObj;
    [SerializeField] GameObject playBtnBase;
    [SerializeField] Button playBtn_Live;
    [SerializeField] Button playModeBtn;
    [SerializeField] GameObject playMode_solo;
    [SerializeField] GameObject playMode_team;
    [SerializeField] GameObject playMode_practice;
    [SerializeField] TextMeshProUGUI tierNameTxt;
    [SerializeField] TextMeshProUGUI rankPointTxt;
    [SerializeField] RawImage rankTexture;

    [Header("Bottom Center")]
    [SerializeField] GameObject bottomCenterGroupObj;

    [Header("Bottom Left")]
    [SerializeField] GameObject bottomLeftGroupObj;
    [SerializeField] Button shopBtn;
    [SerializeField] Button skinBtn;
    [SerializeField] Button rankBtn;
    [SerializeField] Button skillBtn;
    [SerializeField] Button missionBtn;
    [SerializeField] Button questBtn;
    [SerializeField] Button statsBtn;

    [Header("Etc")]
    [SerializeField] TextMeshProUGUI versionTxt;

    private void Awake()
    {
        playBtn_Live.SafeSetButton(OnClickBtn);
        playModeBtn.SafeSetButton(OnClickBtn);

        shopBtn.SafeSetButton(OnClickBtn);
        skinBtn.SafeSetButton(OnClickBtn);
        rankBtn.SafeSetButton(OnClickBtn);
        skillBtn.SafeSetButton(OnClickBtn);
        missionBtn.SafeSetButton(OnClickBtn);
        questBtn.SafeSetButton(OnClickBtn);
        statsBtn.SafeSetButton(OnClickBtn);

        activeSkillBtn.SafeSetButton(OnClickBtn);
        passiveSkillBtn.SafeSetButton(OnClickBtn);

        nameBtn.SafeSetButton(OnClickBtn);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        rotateArea.dragCallback += OnDragRotateArea;
        rotateArea.pointerUpCallback += OnPointerUpRotateArea;

        AccountManager.Instance.OnValueChangedAccountData += OnValueChangedAccountData;
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        rotateArea.dragCallback -= OnDragRotateArea;
        rotateArea.pointerUpCallback -= OnPointerUpRotateArea;

        AccountManager.Instance.OnValueChangedAccountData -= OnValueChangedAccountData;
    }

    public override void Show()
    {
        base.Show();

        SetTxt();
        ShowPlayBtn();
        SetPlayModeBtn();
        SetCurrency();
        SetSkillInfo();
    }

    private void RefreshUIData()
    {
        SetTxt();
        SetCurrency();
        SetSkillInfo();
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == playBtn_Live)
        {
            if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.OutGame)
                return;

            switch (NetworkManager_Client.Instance.CurrentPlayMode)
            {
                case Quantum.InGamePlayMode.SoloMode:
                    {
                        NetworkManager_Client.Instance.SetRoomMaxPlayer(CommonDefine.DEFAULT_MAX_PLAYER);
                    }
                    break;
                case Quantum.InGamePlayMode.TeamMode:
                    {
                        NetworkManager_Client.Instance.SetRoomMaxPlayer(CommonDefine.DEFAULT_MAX_PLAYER);
                        //TODO...
                    }
                    break;
                case Quantum.InGamePlayMode.PracticeMode:
                    {
                        NetworkManager_Client.Instance.SetRoomMaxPlayer(CommonDefine.DEFAULT_SOLO_PLAYER);
                    }
                    break;
            }

            PhaseManager.Instance.ChangePhase(CommonDefine.Phase.InGameReady);
        }
        else if (btn == playModeBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_PlayModePopup);
            }
        }
        else if (btn == shopBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Shop);
            }
        }
        else if (btn == skinBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Skin);
            }
        }
        else if (btn == rankBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                RankingManager.Instance.LoadData_FirebaseServer((isComplete) =>
                {
                    if (isComplete)
                        UIManager.Instance.ShowUI(UIManager.UIType.UI_Rank);

                    UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
                });
            }
        }
        else if (btn == skillBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Skill);
            }
        }
        else if (btn == missionBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
            }
        }
        else if (btn == questBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Quest);
            }
        }
        else if (btn == statsBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Stats);
            }
        }

        if (btn == activeSkillBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                PrefabManager.Instance.UI_Skill.SetSelectedData(CommonDefine.SkillType.Active);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Skill);
            }

        }
        else if (btn == passiveSkillBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Open);
                PrefabManager.Instance.UI_Skill.SetSelectedData(CommonDefine.SkillType.Passive);
                UIManager.Instance.ShowUI(UIManager.UIType.UI_Skill);
            }
        }
        else if (btn == nameBtn)
        {
            if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.OutGame)
                UIManager.Instance.ShowUI(UIManager.UIType.UI_ChangeNicknamePopup);
        }
    }

    public void SetTxt()
    {
        nameTxt.SafeSetText(AccountManager.Instance.Nickname);
        pidTxt.SafeSetText(AccountManager.Instance.PID);

        var msg = "Client Ver : " + CommonDefine.ClientVersion + "\n" + "Quantum Ver : " + CommonDefine.QuantumVersion + "\nRegion : " + NetworkManager_Client.Instance.BestRegion;
#if UNITY_EDITOR || SERVERTYPE_DEV
        msg += "\nServerType : Dev";
#elif SERVERTYPE_LIVE
        msg += "\nServerType : Live";
#endif

        versionTxt.SafeSetText(msg);

        var rp = AccountManager.Instance.RankingPoint;
        var tier = AccountManager.Instance.GetRankingTier(rp);
        tierNameTxt.SafeLocalizeText(StringManager.GetRankingTierNameKey(rp));
        rankPointTxt.SafeSetText("RP " + rp);
        rankTexture.texture = ResourceManager.Instance.GetOutGameRankIconByTier(tier);
    }

    public void SetCurrency()
    {
        currency?.SetCurrency();
    }

    public void SetSkillInfo()
    {
        var resourceList_passive = ResourceManager.Instance.PassiveSkillDataList;
        var resouceList_active = ResourceManager.Instance.ActiveSkillDataList;

        if (AccountManager.Instance.AccountData != null && resourceList_passive != null && resouceList_active != null)
        {
            var skill_passive = resourceList_passive.Find(x => x.id.Equals(AccountManager.Instance.AccountData.passiveSkillID));
            var skill_active = resouceList_active.Find(x => x.id.Equals(AccountManager.Instance.AccountData.activeSkillID));

            if (skill_passive != null)
            {
                if (skill_passive.id.Equals((int)Quantum.PlayerPassiveSkill.None))
                {
                    passiveSkillNoneImage.SafeSetActive(true);
                }
                else
                {
                    passiveSkillNoneImage.SafeSetActive(false);
                    passiveSkillImage.texture = skill_passive.texture;
                }
            }

            if (skill_active != null)
            {
                if (skill_active.id.Equals((int)Quantum.PlayerActiveSkill.None))
                {
                    activeSkillNoneImage.SafeSetActive(true);
                }
                else
                {
                    activeSkillNoneImage.SafeSetActive(false);
                    activeSkillImage.texture = skill_active.texture;
                }
            }
        }
    }

    private void ShowPlayBtn()
    {
        playBtnBase.SafeSetActive(true);

        topLeftBarObj.SafeSetActive(true);
        topRightBarObj.SafeSetActive(true);
        bottomCenterGroupObj.SafeSetActive(false);
        bottomLeftGroupObj.SafeSetActive(true);
        centerGroupObj.SafeSetActive(true);

        OutGameManager.Instance.ChangeSpawnedPlayerAnim(Unity_PlayerCharacter.AnimState.Idle);
    }

    public void SetPlayModeBtn()
    {
        switch (NetworkManager_Client.Instance.CurrentPlayMode)
        {
            case Quantum.InGamePlayMode.SoloMode:
                {
                    playMode_solo.SafeSetActive(true);
                    playMode_team.SafeSetActive(false);
                    playMode_practice.SafeSetActive(false);
                }
                break;
            case Quantum.InGamePlayMode.TeamMode:
                {
                    playMode_team.SafeSetActive(true);
                    playMode_solo.SafeSetActive(false);
                    playMode_practice.SafeSetActive(false);
                }
                break;
            case Quantum.InGamePlayMode.PracticeMode:
                {
                    playMode_practice.SafeSetActive(true);
                    playMode_team.SafeSetActive(false);
                    playMode_solo.SafeSetActive(false);
                }
                break;
        }
    }

    private void OnDragRotateArea(UIComponent_RotateArea.SwipeDirection dir, float strength)
    {
        var player = OutGameManager.Instance.playerChar;
        if (player != null)
        {
            if (dir == UIComponent_RotateArea.SwipeDirection.Left)
                player.transform.Rotate(0f, Time.deltaTime * 600f, 0f);
            else if (dir == UIComponent_RotateArea.SwipeDirection.Right)
                player.transform.Rotate(0f, -Time.deltaTime * 600f, 0f);
        }
    }

    private void OnPointerUpRotateArea(UIComponent_RotateArea.SwipeDirection dir, float strength, float length)
    {
        if (strength < 100 && length < 5)
        {
            OnClickBtn(skinBtn);
        }
    }

    private void OnValueChangedAccountData()
    {
        OutGameManager.Instance.ChangeSpawnedPlayerData();
        RefreshUIData();
    }
}
