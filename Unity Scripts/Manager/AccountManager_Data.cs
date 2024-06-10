using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class AccountManager
{
    public AccountData AccountData
    {
        get
        {
            if (accountData != null)
                return accountData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! accountData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public string PID
    {
        get
        {
            if (AccountData != null)
                return AccountData.PID;
            else
                return string.Empty;
        }
    }

    public string Nickname
    {
        get
        {
            if (AccountData != null)
            {
                if (AccountData.nickname != string.Empty)
                    return AccountData.nickname;
                else
                    return PID;
            }
            else
                return string.Empty;
        }
    }

    public string CountryCode
    {
        get
        {
            if (AccountData != null)
            {
                if (AccountData.CountryCode != string.Empty)
                    return AccountData.CountryCode;
                else
                    return PID;
            }
            else
                return string.Empty;
        }
    }

    public int RankingPoint
    {
        get
        {
            if (AccountData != null)
            {
                return AccountData.RankingPoint;
            }
            else
                return 0;
        }
    }

    public int CurrentPlayMode
    {
        get
        {
            if (AccountData != null)
            {
                return AccountData.ingameSettings_ingamePlayMode;
            }
            else
                return 0;
        }
    }

    public short GetMatchMakingGroup
    {
        get
        {
            if (AccountData != null)
            {
                var tier = GetRankingTier(AccountData.RankingPoint);
                var group = CommonDefine.MatchMakingGroup.Normal_Group;


                var groupList = ReferenceManager.Instance.MatchMakingData.MatchMakingList
                    .FindAll(group => group.includedTierList.Contains((int)tier) == true);

                if (groupList != null && groupList.Count > 0)
                {
                    int random = UnityEngine.Random.Range(0, groupList.Count);

                    group = (CommonDefine.MatchMakingGroup)groupList[random].groupID;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("<color=red>groupList is empty!!</color>");
#endif
                }

                /*
                switch (tier)
                {
                    case CommonDefine.RankTierType.Bronze:
                        {
                            group = CommonDefine.MatchMakingGroup.Beginner_Group;
                            break;
                        }
                    case CommonDefine.RankTierType.Silver:
                    case CommonDefine.RankTierType.Gold:
                        {
                            int random = UnityEngine.Random.Range(0, 100);
                            if (random > 50)
                                group = CommonDefine.MatchMakingGroup.Beginner_Group;
                            else
                                group = CommonDefine.MatchMakingGroup.Normal_Group;
                        }
                        break;
                    case CommonDefine.RankTierType.Platinum:
                        {
                            int random = UnityEngine.Random.Range(0, 100);
                            if (random > 50)
                                group = CommonDefine.MatchMakingGroup.Normal_Group;
                            else
                                group = CommonDefine.MatchMakingGroup.Expert_Group;
                        }
                        break;
                    case CommonDefine.RankTierType.Diamond:
                        {
                            int random = UnityEngine.Random.Range(0, 100);
                            if (random > 50)
                                group = CommonDefine.MatchMakingGroup.Expert_Group;
                            else
                                group = CommonDefine.MatchMakingGroup.Pro_Group;
                        }
                        break;
                    case CommonDefine.RankTierType.Champion:
                        {
                            group = CommonDefine.MatchMakingGroup.Pro_Group;
                        }
                        break;
                }
                */

                return (short)group;
            }
            else
                return 0;
        }
    }
}

[Serializable]
public class AccountData
{
    public string PID; //firebase userId 사용
    public string nickname;
    public string firebase_email;
    public string firebase_passward;

    public int characterSkinID;
    public int weaponSkinID;

    public int passiveSkillID;
    public int activeSkillID;

    public bool isBanned = false;
    public bool IsVIP = false;
    public int loginCount = 0;
    public bool isBgmOn = true;
    public bool isSoundEffectOn = true;
    public bool isVibration = true;
    public string LanguageCode = "en-US";
    public string CountryCode = "en-US";

    public int coin = 0;    //free currency
    public int gem = 0;     //paid currency

    public int ingameSettings_ingamePlayMode = 0; //0: Solo 1:Team 2:Practice
    public int ingameSettings_cameramode = 0; //0: SimpleLoockAt 1: LockIn
    public float ingameSettings_cameraOffsetDistance = 7f; //0: SimpleLoockAt 1: LockIn

    public int MatchCount_All;
    public int MatchCount_SoloMode;
    public int MatchCount_TeamMode;
    public int MatchCount_PracticeMode;

    public bool RankPointNeedToBeCalculated = false; //매칭 시작해서 RankPoint 정산이 필요 여부
    public int RankingPoint; //랭킹 포인트
    //public string RecentMatchDateTime;

    //기본 스텟 업그레이드 관련 수치들
    public short PlayerStats_MaxHealthPoint_Level = 0;
    public short PlayerStats_AttackDamage_Level = 0;
    public short PlayerStats_AttackRange_Level = 0;
    public short PlayerStats_AttackDuration_Level = 0;
    public short PlayerStats_MaxSpeed_Level = 0;
    public short PlayerStats_AttackCooltime_Level = 0;
    public short PlayerStats_JumpCooltime_Level = 0;

    public int nicknameChangeCount = 0;

    [SerializeField] public List<int> ownedCharacterSkin = new List<int>();
    [SerializeField] public List<int> ownedWeaponSkin = new List<int>();

    [SerializeField] public List<int> ownedActiveSkill = new List<int>();
    [SerializeField] public List<int> ownedPassiveSkill = new List<int>();

    [SerializeField] public List<PurchaseShopData> purchaseShopDataList_All = new List<PurchaseShopData>();

    [SerializeField] public List<PurchaseShopData> purchaseShopDataList_IAP = new List<PurchaseShopData>();

    [SerializeField] public List<MatchData> matchDataList = new List<MatchData>();

    [SerializeField] public List<QuestPassData> questPassData = new List<QuestPassData>();

    [Serializable]
    public class PurchaseShopData
    {
        public int purchaseShopID;
        public int purchaseItemID;
        public short currencyType;
        public string purchaseDate;
    }

    [Serializable]
    public class MatchData
    {
        public short matchType;
        public int mapID;
        public string matchDate;
    }

    [Serializable]
    public class QuestPassData
    {
        public int questPassID;
        public int seasonNumber;
        public int exp;

        [SerializeField] public List<int> obtainedRewardID = new List<int>();
    }
}


