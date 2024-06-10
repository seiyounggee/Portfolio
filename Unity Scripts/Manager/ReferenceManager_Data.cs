using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RefData;

public partial class ReferenceManager
{

    public AppDefines AppDefines
    {
        get
        {
            if (referenceData != null)
                return referenceData.appDefines;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_PlayerBasicStats PlayerBasicStats
    {
        get
        {
            if (referenceData != null && referenceData.playerData != null)
                return referenceData.playerData.playerBasicStats;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Quantum.PlayerDefaultData PlayerDefaultStat_Quantum
    {
        get 
        {
            var data = new Quantum.PlayerDefaultData();

            if (PlayerBasicStats != null)
            {
                data.MaxHealthPoint = PlayerBasicStats.defaultStats.MaxHealthPoint;
                data.AttackDamage = PlayerBasicStats.defaultStats.AttackDamage;
                data.AttackRange = PlayerBasicStats.defaultStats.AttackRange;
                data.AttackDuration = PlayerBasicStats.defaultStats.AttackDuration;
                data.MaxSpeed = PlayerBasicStats.defaultStats.MaxSpeed;

                data.Input_AttackCooltime = PlayerBasicStats.defaultStats.Input_AttackCooltime;
                data.Input_JumpCooltime = PlayerBasicStats.defaultStats.Input_JumpCooltime;
                data.Input_SkillCooltime = PlayerBasicStats.defaultStats.Input_SkillCooltime;
            }

            return data;
        }
    }

    public Quantum.PlayerDefaultData PlayerAdditionalStat_Quantum(CommonDefine.MatchMakingGroup group) //AI용 Stats 에 의한 추가 수치
    {
        //AI용 추가 Stat 수치
        var data = new Quantum.PlayerDefaultData();

        short aiStatsLevel = 0;

        switch (group)
        {
            default:
            case CommonDefine.MatchMakingGroup.Beginner_Group:
                aiStatsLevel = 0;
                break;
            case CommonDefine.MatchMakingGroup.Normal_Group:
                aiStatsLevel = 3;
                break;
            case CommonDefine.MatchMakingGroup.Expert_Group:
                aiStatsLevel = 6;
                break;
            case CommonDefine.MatchMakingGroup.Pro_Group:
                aiStatsLevel = 9;
                break;
        }


        var refList = PlayerBasicStats.upgradeStatsList;
        var data_MaxHealthPoint = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.MaxHealthPoint) && x.level.Equals(aiStatsLevel));
        var data_AttackDamage = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackDamage) && x.level.Equals(aiStatsLevel));
        var data_AttackRange = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackRange) && x.level.Equals(aiStatsLevel));
        var data_AttackDuration = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackDuration) && x.level.Equals(aiStatsLevel));
        var data_MaxSpeed = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.MaxSpeed) && x.level.Equals(aiStatsLevel));
        var data_AttackCooltime = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackCooltime) && x.level.Equals(aiStatsLevel));
        var data_JumpCooltime = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.JumpCooltime) && x.level.Equals(aiStatsLevel));

        if (data_MaxHealthPoint != null)
            data.MaxHealthPoint = data_MaxHealthPoint.param_1;
        if (data_AttackDamage != null)
            data.AttackDamage = data_AttackDamage.param_1;
        if (data_AttackRange != null)
            data.AttackRange = data_AttackRange.param_1;
        if (data_AttackDuration != null)
            data.AttackDuration = data_AttackDuration.param_1;
        if (data_MaxSpeed != null)
            data.MaxSpeed = data_MaxSpeed.param_1;
        if (data_AttackCooltime != null)
            data.Input_AttackCooltime = data_AttackCooltime.param_1;
        if (data_JumpCooltime != null)
            data.Input_JumpCooltime = data_JumpCooltime.param_1;

        return data;
    }

    public Ref_BallDefaultStat BallDefaultStat
    {
        get
        {
            if (referenceData != null && referenceData.ballData != null)
                return referenceData.ballData.ballDefaultStats;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Quantum.BallDefaultData BallDefaultStat_Quantum
    {
        get
        {
            var data = new Quantum.BallDefaultData();

            if (BallDefaultStat != null)
            {
                data.BallStartSpeed = BallDefaultStat.BallStartSpeed;
                data.BallMinSpeed = BallDefaultStat.BallMinSpeed;
                data.BallIncreaseSpeed = BallDefaultStat.BallIncreaseSpeed;
                data.BallMaxSpeed = BallDefaultStat.BallMaxSpeed;

                data.BallRotationSpeed = BallDefaultStat.BallRotationSpeed;
                data.BallRotationMinSpeed = BallDefaultStat.BallRotationMinSpeed;
                data.BallRotationIncreaseSpeed = BallDefaultStat.BallRotationIncreaseSpeed;
                data.BallRotationMaxSpeed = BallDefaultStat.BallRotationMaxSpeed;

                data.DefaultAttackDamage = BallDefaultStat.DefaultAttackDamage;

                data.BallCollisionCooltime = BallDefaultStat.BallCollisionCooltime;
            }

            return data;
        }
    }
    public Quantum.AIDefaultData AIDefaultData_Quantum(CommonDefine.MatchMakingGroup group)
    {
        //Client -> Quantum 으로 보내고 싶은 추가적인 AI Data들...
        var data = new Quantum.AIDefaultData();

        //아래 두개 데이터 합산해서 계산중
        data.PlayerDafaultData = PlayerDefaultStat_Quantum;
        data.PlayerAdditionalStatsData = PlayerAdditionalStat_Quantum(group); //AI용

        if (SkillData != null && SkillData.ActiveSkillInfoList != null)
        {
            foreach (var i in SkillData.ActiveSkillInfoList)
            {
                var skillType = (Quantum.PlayerActiveSkill)i.SkillID;
                switch (skillType)
                {

                    case Quantum.PlayerActiveSkill.Dash:
                        data.CoolTime_Dash = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.FreezeBall:
                        data.CoolTime_FreezeBall = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.FastBall:
                        data.CoolTime_FastBall = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.Shield:
                        data.CoolTime_Shield = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.TakeBallTarget:
                        data.CoolTime_TakeBallTarget = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.BlindZone:
                        data.CoolTime_BlindZone = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.ChangeBallTarget:
                        data.CoolTime_ChangeBallTarget = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.CurveBall:
                        data.CoolTime_CurveBall = i.defaultCoolTime;
                        break;
                    case Quantum.PlayerActiveSkill.SkyRocketBall:
                        data.CoolTime_SkyRocketBall = i.defaultCoolTime;
                        break;

                }
            }
        }

        return data;
    }

    public Quantum.AdditionalReferenceData AdditionalReferenceData_Quantum
    {
        //Client -> Quantum 으로 보내고 싶은 추가적인 RefData들...

        get
        {
            var data = new Quantum.AdditionalReferenceData();


            return data;
        }
    }

    public Ref_SkinData SkinData
    {
        get
        {
            if (referenceData != null && referenceData.playerData != null)
                return referenceData.playerData.skinData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_AIData AIData
    {
        get
        {
            if (referenceData != null && referenceData.playerData != null)
                return referenceData.playerData.aiData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_SkillData SkillData
    {
        get
        {
            if (referenceData != null && referenceData.playerData != null)
                return referenceData.playerData.skillData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_MapData MapData
    {
        get
        {
            if (referenceData != null && referenceData.mapData != null)
                return referenceData.mapData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_ShopData ShopData
    {
        get
        {
            if (referenceData != null && referenceData.shopData != null)
                return referenceData.shopData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_MissionData MissionData
    {
        get
        {
            if (referenceData != null && referenceData.missionData != null)
                return referenceData.missionData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_QuestPassData QuestPassData
    {
        get
        {
            if (referenceData != null && referenceData.questPassData != null)
                return referenceData.questPassData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_RankingData RankingData
    {
        get
        {
            if (referenceData != null && referenceData.rankingData != null)
                return referenceData.rankingData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }

    public Ref_MatchMakingData MatchMakingData
    {
        get
        {
            if (referenceData != null && referenceData.matchMakingData != null)
                return referenceData.matchMakingData;
            else
            {
                UtilityCommon.ColorLog("ERROR...! referenceData is null!", UtilityCommon.DebugColor.Red);
                return null;
            }
        }
    }
}



[Serializable]
public class RefData
{
    #region AppDefines
    public AppDefines appDefines = new AppDefines();
    [Serializable]
    public class AppDefines
    {
        public string ClientVersion = "1.0.0";
        public string PhotonQuantumVersion = "1.0";

        public bool IsMaintenanceTime = false; //유지보수 시간

        public string termsOfServiceUrl = "";
        public string privacyPolicyUrl = "";
        public string contactUrl = "";
    }
    #endregion

    #region Player Data
    public Ref_PlayerData playerData = new Ref_PlayerData();
    [Serializable]
    public class Ref_PlayerData
    {
        [SerializeField] public Ref_PlayerBasicStats playerBasicStats = new Ref_PlayerBasicStats();
        [SerializeField] public Ref_SkinData skinData = new Ref_SkinData();
        [SerializeField] public Ref_AIData aiData = new Ref_AIData();
        [SerializeField] public Ref_SkillData skillData = new Ref_SkillData();
    }

    [Serializable]
    public class Ref_PlayerBasicStats
    {
        [SerializeField] public Ref_PlayerDefaultStatsInfo defaultStats = new Ref_PlayerDefaultStatsInfo();
        [SerializeField] public List<Ref_PlayerUpgradeStatsInfo> upgradeStatsList = new List<Ref_PlayerUpgradeStatsInfo>();

        public Ref_PlayerBasicStats()
        {
            defaultStats = new Ref_PlayerDefaultStatsInfo()
            {
                MaxHealthPoint = 20,
                AttackDamage = 0,
                AttackRange = 2_500,  //millimeter 기준 (2.5미터)
                AttackDuration = 750,  //millisecond 기준 (0.75초)
                MaxSpeed = 8_000,  //millisecond 기준
                Input_AttackCooltime = 2_000, //millisecond 기준 (2초)
                Input_JumpCooltime = 1_000, //millisecond 기준 (1초)
                Input_SkillCooltime = 10_000, //millisecond 기준 (10초)
            };

            upgradeStatsList = new List<Ref_PlayerUpgradeStatsInfo>()
            {
                //MaxHealthPoint
                new Ref_PlayerUpgradeStatsInfo(){ id = 100, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0 , SuccessProbability = 100 , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 101, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 5 , SuccessProbability = 100 , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 102, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 10, SuccessProbability = 80  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 103, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 15, SuccessProbability = 70  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 104, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 20, SuccessProbability = 60  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 105, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 25, SuccessProbability = 50  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 106, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 30, SuccessProbability = 40  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 107, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 35, SuccessProbability = 30  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 108, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 40, SuccessProbability = 20  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 109, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 45, SuccessProbability = 15  , statsName = "STATS_MAXHEALTHPOINT"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 110, type = (short)CommonDefine.PlayerStatsType.MaxHealthPoint, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 50, SuccessProbability = 10  , statsName = "STATS_MAXHEALTHPOINT"},

                //AttackDamage
                new Ref_PlayerUpgradeStatsInfo(){ id = 200, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0 , SuccessProbability = 100 , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 201, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 3 , SuccessProbability = 100 , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 202, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 6 , SuccessProbability = 80  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 203, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 8 , SuccessProbability = 70  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 204, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 10, SuccessProbability = 60  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 205, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 12, SuccessProbability = 50  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 206, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 15, SuccessProbability = 40  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 207, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 18, SuccessProbability = 30  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 208, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 21, SuccessProbability = 20  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 209, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 25, SuccessProbability = 15  , statsName = "STATS_ATTACKDAMAGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 210, type = (short)CommonDefine.PlayerStatsType.AttackDamage, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 30, SuccessProbability = 10  , statsName = "STATS_ATTACKDAMAGE"},
            
                //AttackRange
                new Ref_PlayerUpgradeStatsInfo(){ id = 300, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0  , SuccessProbability = 100 , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 301, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 100, SuccessProbability = 100 , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 302, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 150, SuccessProbability = 80  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 303, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 200, SuccessProbability = 70  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 304, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 250, SuccessProbability = 60  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 305, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 300, SuccessProbability = 50  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 306, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 350, SuccessProbability = 40  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 307, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 400, SuccessProbability = 30  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 308, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 450, SuccessProbability = 20  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 309, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 500, SuccessProbability = 15  , statsName = "STATS_ATTACKRANGE"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 310, type = (short)CommonDefine.PlayerStatsType.AttackRange, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 600, SuccessProbability = 10  , statsName = "STATS_ATTACKRANGE"},
            
                //AttackDuration
                new Ref_PlayerUpgradeStatsInfo(){ id = 400, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0  , SuccessProbability = 100 , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 401, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 20 , SuccessProbability = 100 , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 402, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 40 , SuccessProbability = 80  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 403, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 60 , SuccessProbability = 70  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 404, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 80 , SuccessProbability = 60  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 405, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 100, SuccessProbability = 50  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 406, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 120, SuccessProbability = 40  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 407, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 140, SuccessProbability = 30  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 408, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 160, SuccessProbability = 20  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 409, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 180, SuccessProbability = 15  , statsName = "STATS_ATTACKDURATION"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 410, type = (short)CommonDefine.PlayerStatsType.AttackDuration, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 200, SuccessProbability = 10  , statsName = "STATS_ATTACKDURATION"},

                //MaxSpeed
                new Ref_PlayerUpgradeStatsInfo(){ id = 500, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0    , SuccessProbability = 100 , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 501, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 200  , SuccessProbability = 100 , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 502, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 400  , SuccessProbability = 80  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 503, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 600  , SuccessProbability = 70  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 504, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 800  , SuccessProbability = 60  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 505, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 1_000, SuccessProbability = 50  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 506, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 1_200, SuccessProbability = 40  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 507, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 1_400, SuccessProbability = 30  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 508, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 1_600, SuccessProbability = 20  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 509, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 1_800, SuccessProbability = 15  , statsName = "STATS_MAXSPEED"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 510, type = (short)CommonDefine.PlayerStatsType.MaxSpeed, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 2_000, SuccessProbability = 10  , statsName = "STATS_MAXSPEED"},

                //AttackCooltime
                new Ref_PlayerUpgradeStatsInfo(){ id = 600, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0  , SuccessProbability = 100 , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 601, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 20 , SuccessProbability = 100 , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 602, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 40 , SuccessProbability = 80  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 603, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 60 , SuccessProbability = 70  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 604, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 80 , SuccessProbability = 60  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 605, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 100, SuccessProbability = 50  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 606, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 120, SuccessProbability = 40  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 607, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 140, SuccessProbability = 30  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 608, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 160, SuccessProbability = 20  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 609, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 180, SuccessProbability = 15  , statsName = "STATS_ATTACKCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 610, type = (short)CommonDefine.PlayerStatsType.AttackCooltime, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 200, SuccessProbability = 10  , statsName = "STATS_ATTACKCOOLTIME"},

                //JumpCooltime
                new Ref_PlayerUpgradeStatsInfo(){ id = 700, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 0,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 0,     param_1 = 0  , SuccessProbability = 100 , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 701, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 1,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2000,  param_1 = 10 , SuccessProbability = 100 , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 702, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 2,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 2500,  param_1 = 20 , SuccessProbability = 80  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 703, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 3,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 3000,  param_1 = 30 , SuccessProbability = 70  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 704, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 4,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 4000,  param_1 = 40 , SuccessProbability = 60  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 705, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 5,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 5000,  param_1 = 50 , SuccessProbability = 50  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 706, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 6,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 6000,  param_1 = 60 , SuccessProbability = 40  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 707, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 7,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 7000,  param_1 = 70 , SuccessProbability = 30  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 708, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 8,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 8000,  param_1 = 80 , SuccessProbability = 20  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 709, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 9,  PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 9000,  param_1 = 90 , SuccessProbability = 15  , statsName = "STATS_JUMPCOOLTIME"},
                new Ref_PlayerUpgradeStatsInfo(){ id = 710, type = (short)CommonDefine.PlayerStatsType.JumpCooltime, level = 10, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 10000, param_1 = 100, SuccessProbability = 10  , statsName = "STATS_JUMPCOOLTIME"},
            };
        }

        [Serializable]
        public class Ref_PlayerDefaultStatsInfo
        {
            public int MaxHealthPoint;
            public int AttackDamage;
            public int AttackRange;
            public int AttackDuration;

            public int MaxSpeed;

            public int Input_AttackCooltime;
            public int Input_JumpCooltime;
            public int Input_SkillCooltime;
        }

        [Serializable]
        public class Ref_PlayerUpgradeStatsInfo
        {
            public int id;
            public short type;
            public short level;
            public short PurchaseCurrencyType; //Coin, Gem, IAP
            public int Price;
            public int SuccessProbability; //강화 확률

            public int param_1;
            public int param_2;
            public int param_3;

            public string statsName; //Name Key
            public string statIconName;
        }
    }

    [Serializable]
    public class Ref_SkinData
    {
        [SerializeField] public List<Ref_SkinInfo> CharacterSkinList = new List<Ref_SkinInfo>();
        [SerializeField] public List<Ref_SkinInfo> WeaponSkinList = new List<Ref_SkinInfo>();

        [Serializable]
        public class Ref_SkinInfo
        {
            public int SkinID = 0;
            public string SkinAssetName = "";
            public string SkinIconAssetName = "";
            public bool IsEnabled = true;
            public string SkinName = "";
            public byte Rarity = 0;

            public int increase_attackDamage = 0;
            public int increase_maxhp = 0;
            public int increase_moveSpeed = 0;

            public short PurchaseCurrencyType; //Coin, Gem, IAP
            public int Price;
        }

        public Ref_SkinData()
        {
            CharacterSkinList = new List<Ref_SkinInfo> {
                new Ref_SkinInfo(){ SkinID = 0, SkinAssetName = "Skin_Character_Shadow", SkinIconAssetName = "Icon_Character_Shadow", IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_SHADOW", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Unavailable, Price = 99999},
                new Ref_SkinInfo(){ SkinID = 1, SkinAssetName = "Skin_Character_Cisab", SkinIconAssetName = "Icon_Character_Cisab", IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_CISAB", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 2, SkinAssetName = "Skin_Character_Jocker", SkinIconAssetName = "Icon_Character_Jocker",IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_JOCKER", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 3, SkinAssetName = "Skin_Character_Dronax", SkinIconAssetName = "Icon_Character_Dronax",IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_DRONAX", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 4, SkinAssetName = "Skin_Character_Fazzle", SkinIconAssetName = "Icon_Character_Fazzle",IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_FAZZLE", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 5, SkinAssetName = "Skin_Character_Zorple", SkinIconAssetName = "Icon_Character_Zorple",IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_ZORPLE", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 6, SkinAssetName = "Skin_Character_Vexlo", SkinIconAssetName = "Icon_Character_Vexlo",IsEnabled = true  , SkinName = "SKIN_CHARACTER_NAME_VEXLO", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 7, SkinAssetName = "Skin_Character_Flowey", SkinIconAssetName = "Icon_Character_Flowey", IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_FLOWEY", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 8, SkinAssetName = "Skin_Character_Davill", SkinIconAssetName = "Icon_Character_Davill", IsEnabled = true, SkinName = "SKIN_CHARACTER_NAME_DAVILL", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
            };

            WeaponSkinList = new List<Ref_SkinInfo> {
                new Ref_SkinInfo(){ SkinID = 0,  SkinAssetName = "Sword0_Blue", SkinIconAssetName = "icon_weapon_0",IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_0", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 1,  SkinAssetName = "Sword1_Silver", SkinIconAssetName = "icon_weapon_1",IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_1", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 100},
                new Ref_SkinInfo(){ SkinID = 2,  SkinAssetName = "Sword2_Green", SkinIconAssetName = "icon_weapon_2",IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_2", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 200},
                new Ref_SkinInfo(){ SkinID = 3,  SkinAssetName = "Sword3_Red", SkinIconAssetName = "icon_weapon_3",IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_3", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Coin, Price = 300},
                new Ref_SkinInfo(){ SkinID = 4,  SkinAssetName = "Sword4_Red", SkinIconAssetName = "icon_weapon_4", IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_4", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 5,  SkinAssetName = "Sword5_Gold",SkinIconAssetName = "icon_weapon_5", IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_5", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 6,  SkinAssetName = "Sword6_Red", SkinIconAssetName = "icon_weapon_6", IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_6", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 7,  SkinAssetName = "Sword7_Green", SkinIconAssetName = "icon_weapon_7",IsEnabled = true, SkinName = "SKIN_WEAPON_NAME_7", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 8,  SkinAssetName = "Sword8_Corrupted", SkinIconAssetName = "icon_weapon_8", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_8", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 9,  SkinAssetName = "Sword9_Purple", SkinIconAssetName = "icon_weapon_9", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_9", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 10, SkinAssetName = "Sword10_Brown", SkinIconAssetName = "icon_weapon_10", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_10", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 11, SkinAssetName = "Sword11_Blood", SkinIconAssetName = "icon_weapon_11", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_11", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 12, SkinAssetName = "Sword12_Purple", SkinIconAssetName = "icon_weapon_12", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_12", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 13, SkinAssetName = "Sword13_Green", SkinIconAssetName = "icon_weapon_13", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_13", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 14, SkinAssetName = "Sword14_Blue", SkinIconAssetName = "icon_weapon_14", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_14", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Unavailable, Price = 99999},
                new Ref_SkinInfo(){ SkinID = 15, SkinAssetName = "Sword15_Lava", SkinIconAssetName = "icon_weapon_15", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_15", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_SkinInfo(){ SkinID = 16, SkinAssetName = "Sword16_Frost", SkinIconAssetName = "icon_weapon_16", IsEnabled = true  , SkinName = "SKIN_WEAPON_NAME_16", Rarity =(byte) CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
            };
        }
    }

    [Serializable]
    public class Ref_AIData
    {
        [SerializeField] public List<int> AvailableCharacterSkinIDList = new List<int>();
        [SerializeField] public List<int> AvailableWeaponSkinIDList = new List<int>();
        [SerializeField] public List<int> AvailablePassiveSkillList = new List<int>();
        [SerializeField] public List<int> AvailableActiveSkillList = new List<int>();

        public Ref_AIData()
        {
            AvailableCharacterSkinIDList = new List<int> {
                0, 1, 2 , 3, 4, 5, 6, 7, 8
            };

            AvailableWeaponSkinIDList = new List<int> {
                0, 1, 2 , 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            };

            AvailablePassiveSkillList = new List<int> {
                1, 2 , 3
            };

            AvailableActiveSkillList = new List<int> {
                1, 2 , 3, 4, 5, 6, 7, 8, 9
            };
        }
    }

    #endregion

    #region Ball Data
    public Ref_BallData ballData = new Ref_BallData();
    [Serializable]
    public class Ref_BallData
    {
        [SerializeField] public Ref_BallDefaultStat ballDefaultStats = new Ref_BallDefaultStat();
    }

    [Serializable]
    public class Ref_BallDefaultStat
    {
        public int BallStartSpeed = 10_000;  //millisecond 기준 (=10) 
        public int BallMinSpeed = 5_000;  //millisecond 기준 (=5)
        public int BallIncreaseSpeed = 200;  //millisecond 기준 (=0.2)
        public int BallMaxSpeed = 100_000;  //millisecond 기준 (=100)

        public int BallRotationSpeed = 5_000;  //millisecond 기준 (=5) 
        public int BallRotationMinSpeed = 5_000;  //millisecond 기준 (=5)
        public int BallRotationIncreaseSpeed = 100;  //millisecond 기준 (=0.1)
        public int BallRotationMaxSpeed = 50_000;  //millisecond 기준 (=50)

        public int DefaultAttackDamage = 5;

        public int BallCollisionCooltime = 200;  //millisecond 기준 (=0.2)

    }
    #endregion

    #region Skill Data

    [Serializable]
    public class Ref_SkillData
    {
        [SerializeField] public List<Ref_PassiveSkillInfo> PassiveSkillInfoList = new List<Ref_PassiveSkillInfo>();
        [SerializeField] public List<Ref_ActiveSkillInfo> ActiveSkillInfoList = new List<Ref_ActiveSkillInfo>();

        [Serializable]
        public class Ref_PassiveSkillInfo
        {
            public int SkillID = 0;
            public int type = 0;
            public string SkinAssetName = "";
            public string skillName = "";
            public byte Rarity = 0;

            public int param_1 = 0;
            public int param_2 = 0;
            public int param_3 = 0;

            public int defaultCoolTime = 0; //millisecond 기준 (0초)

            public short PurchaseCurrencyType; //Coin, Gem, IAP
            public int Price;
        }

        [Serializable]
        public class Ref_ActiveSkillInfo
        {
            public int SkillID = 0;
            public int type = 0;
            public string SkinAssetName = "";
            public string skillName = "";
            public byte Rarity = 0;

            public int param_1 = 0;
            public int param_2 = 0;
            public int param_3 = 0;

            public int defaultCoolTime;

            public short PurchaseCurrencyType; //Coin, Gem, IAP
            public int Price;

            /*
            //쿨타임 메모...
            defaultCoolTIme >> Dash = 5_000; //millisecond 기준 (5초)
            defaultCoolTIme >> FreezeBall = 20_000; //millisecond 기준 (20초)
            defaultCoolTIme >> FastBall = 15_000; //millisecond 기준 (15초)
            defaultCoolTIme >> Shield = 15_000; //millisecond 기준 (15초)
            defaultCoolTIme >> TakeBallTarget = 10_000; //millisecond 기준 (10초)
            defaultCoolTIme >> BlindZone = 20_000; //millisecond 기준 (20초)
            defaultCoolTIme >> ChangeBallTarget = 20_000; //millisecond 기준 (20초)
            */
        }

        public Ref_SkillData()
        {
            PassiveSkillInfoList = new List<Ref_PassiveSkillInfo>()
            {
                new Ref_PassiveSkillInfo(){ SkillID = 0, type = (int)Quantum.PlayerPassiveSkill.None, SkinAssetName = "icon_none" , skillName = "SKILL_PASSIVE_NAME_NONE", Rarity = (byte)CommonDefine.Rarity.Common, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_PassiveSkillInfo(){ SkillID = 1, type = (int)Quantum.PlayerPassiveSkill.IncreaseMaxHp, SkinAssetName = "icon_increaseHp", skillName = "SKILL_PASSIVE_NAME_HPBOOST", Rarity = (byte)CommonDefine.Rarity.Common, param_1 = 5, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_PassiveSkillInfo(){ SkillID = 2, type = (int)Quantum.PlayerPassiveSkill.IncreaseSpeed, SkinAssetName = "icon_increaseSpeed" , skillName = "SKILL_PASSIVE_NAME_SPEEDBOOST", Rarity = (byte)CommonDefine.Rarity.Common, param_1 = 1_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_PassiveSkillInfo(){ SkillID = 3, type = (int)Quantum.PlayerPassiveSkill.IncreaseAttackDamage , SkinAssetName = "icon_increaseAttack", skillName = "SKILL_PASSIVE_NAME_ATTACKBOOST", Rarity = (byte)CommonDefine.Rarity.Common, param_1 = 2, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
            };

            ActiveSkillInfoList = new List<Ref_ActiveSkillInfo>()
            {
                new Ref_ActiveSkillInfo(){ SkillID = 0, type = (int)Quantum.PlayerActiveSkill.None, SkinAssetName = "icon_none", skillName = "SKILL_ACTIVE_NAME_NONE", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 0, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 1, type = (int)Quantum.PlayerActiveSkill.Dash, SkinAssetName = "icon_dash", skillName = "SKILL_ACTIVE_NAME_DASH", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 5_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 2, type = (int)Quantum.PlayerActiveSkill.FreezeBall, SkinAssetName = "icon_freezeball", skillName = "SKILL_ACTIVE_NAME_FREEZEBALL", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 20_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 3, type = (int)Quantum.PlayerActiveSkill.FastBall, SkinAssetName = "icon_fastball", skillName = "SKILL_ACTIVE_NAME_FASTBALL", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 15_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 4, type = (int)Quantum.PlayerActiveSkill.Shield, SkinAssetName = "icon_shield", skillName = "SKILL_ACTIVE_NAME_SHEILD", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 15_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 5, type = (int)Quantum.PlayerActiveSkill.TakeBallTarget, SkinAssetName = "icon_takeballtarget", skillName = "SKILL_ACTIVE_NAME_TAKEBALLTARGET", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 10_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 6, type = (int)Quantum.PlayerActiveSkill.BlindZone, SkinAssetName = "icon_blindzone", skillName = "SKILL_ACTIVE_NAME_BLINDZONE", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 20_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 7, type = (int)Quantum.PlayerActiveSkill.ChangeBallTarget, SkinAssetName = "icon_changeballtarget", skillName = "SKILL_ACTIVE_NAME_CHANGEBALLTARGET", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 20_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 8, type = (int)Quantum.PlayerActiveSkill.CurveBall, SkinAssetName = "icon_curveball", skillName = "SKILL_ACTIVE_NAME_CURVEBALL", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 20_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
                new Ref_ActiveSkillInfo(){ SkillID = 9, type = (int)Quantum.PlayerActiveSkill.SkyRocketBall, SkinAssetName = "icon_skyrocketball", skillName = "SKILL_ACTIVE_NAME_SKYROCKETBALL", Rarity = (byte)CommonDefine.Rarity.Common, defaultCoolTime = 20_000, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.Gem, Price = 10},
            };
        }
    }


    #endregion

    #region Account Data
    public Ref_AccountData accountData;
    [Serializable]
    public class Ref_AccountData
    {
        [SerializeField] public Ref_AccountDefaultInfo accountDefaultInfo = new Ref_AccountDefaultInfo();
    }

    [Serializable]
    public class Ref_AccountDefaultInfo
    {
        //최초 계정 생성시 시작되는 id

        public int default_characterSkinID = 0;
        public int default_weaponSkinID = 0;

        public int defulat_passiveSkillID = 0;
        public int defulat_activeSkillID = 0;

        public Ref_AccountDefaultInfo()
        {
            default_characterSkinID = 0;
            default_weaponSkinID = 0;

            defulat_passiveSkillID = 0;
            defulat_activeSkillID = 0;
        }
    }

    #endregion

    #region Map Data

    public Ref_MapData mapData = new Ref_MapData();
    [Serializable]
    public class Ref_MapData
    {
        [SerializeField] public List<Ref_MapInfo> MapInfoList = new List<Ref_MapInfo>();

        [Serializable]
        public class Ref_MapInfo
        {
            public int MapID = 0;
            public int GroupID = 0; //0: Practice Mode Maps, 1: Solo,Team Match Maps
            public bool isOpen = true; //isOpen == false 인 경우 사용x
            public string MapSceneName = "";
            public string QuantumMapAssetName = ""; //runtime config에 필요한 asset (지형 mesh info 담고 있는 asset)
            public string QuantumCustomMapDataAssetName = ""; //runtime config에 필요한 custom data asset (Spawn Point)
            public string MapNameKey = "";

            public string MapAddressableKey()
            {
                return "MapScene/" + MapSceneName;
            }
        }

        public Ref_MapData()
        {
            MapInfoList = new List<Ref_MapInfo>()
            {
                new Ref_MapInfo(){ MapID = 0, isOpen = true, MapSceneName = "MapScene_Practice_01", QuantumMapAssetName = "MapAsset_Practice_01", QuantumCustomMapDataAssetName = "MapCustomData_Practice_01", GroupID = CommonDefine.MAP_GROUP_ID_PRACTICE_MODE, MapNameKey = "MAP_NAME_PRACTICE"},

                new Ref_MapInfo(){ MapID = 1, isOpen = true, MapSceneName = "MapScene_Arena77_01", QuantumMapAssetName ="MapAsset_Arena77_01", QuantumCustomMapDataAssetName = "MapCustomData_Arena77_01",GroupID = CommonDefine.MAP_GROUP_ID_SOLO_OR_TEAM_MODE, MapNameKey = "MAP_NAME_ARENA77"},
                new Ref_MapInfo(){ MapID = 2, isOpen = true, MapSceneName = "MapScene_ChessBoard_01", QuantumMapAssetName ="MapAsset_ChessBoard_01", QuantumCustomMapDataAssetName = "MapCustomData_ChessBoard_01",GroupID = CommonDefine.MAP_GROUP_ID_SOLO_OR_TEAM_MODE, MapNameKey = "MAP_NAME_CHESSBOARD"},
                new Ref_MapInfo(){ MapID = 3, isOpen = true, MapSceneName = "MapScene_WhimsyWoods_01", QuantumMapAssetName ="MapAsset_WhimsyWoods_01", QuantumCustomMapDataAssetName = "MapCustomData_WhimsyWoods_01",GroupID = CommonDefine.MAP_GROUP_ID_SOLO_OR_TEAM_MODE, MapNameKey = "MAP_NAME_WHIMSYWOODS"},
                new Ref_MapInfo(){ MapID = 4, isOpen = true, MapSceneName = "MapScene_RuinedCastle_01", QuantumMapAssetName = "MapAsset_RuinedCastle_01", QuantumCustomMapDataAssetName = "MapCustomData_RuinedCastle_01",GroupID = CommonDefine.MAP_GROUP_ID_SOLO_OR_TEAM_MODE, MapNameKey = "MAP_NAME_RUINEDCASTLE"},
            };
        }
    }

    #endregion

    #region Shop Data

    public Ref_ShopData shopData = new Ref_ShopData();
    [Serializable]
    public class Ref_ShopData
    {
        [SerializeField] public List<Ref_ShopInfo> ShopInfoList = new List<Ref_ShopInfo>();

        [Serializable]
        public class Ref_ShopInfo
        {
            public int ShopID; //겹치면 안됨!!!!!
            public int ShopType;
            [SerializeField] public List<ItemDataInfo> ItemList = new List<ItemDataInfo>();

            [Serializable]
            public class ItemDataInfo
            {
                public int ItemId; //겹치면 안됨!!!!! 
                public short PurchaseCurrencyType; //Coin, Gem, IAP
                public string IAP_Id; //겹치면 안됨!!!!!  IBillingProduct Id 와 동일
                public string IAP_PlatformId_AOS;   //IBillingProduct PlatformId 와 동일
                public string IAP_PlatformId_IOS;   //IBillingProduct PlatformId 와 동일
                public string ItemName;
                public string ItemDesc;
                public short ItemType;
                public int Price;
                public int UI_GroupID;
                public short UI_Sort;
                public string SaleStartTime;    //DateTime.Parse 필요 (일반 DateTime은 Serialize이 안됨)
                public string SaleEndTime;      //DateTime.Parse 필요 (일반 DateTime은 Serialize이 안됨)

                [SerializeField] public List<RewardData> RewardList = new List<RewardData>();
            }

            [Serializable]
            public class RewardData
            {
                public int id;
                public int RewardID;
                public short RewardType;
                public int Quantity;

                public string RewardName; //확인용...?
            }
        }

        public Ref_ShopData()
        {
            ShopInfoList = new List<Ref_ShopInfo>()
            {
                new Ref_ShopInfo(){ ShopID = 0, ShopType = (int)CommonDefine.ShopType.Common, 
                    ItemList = new List<Ref_ShopInfo.ItemDataInfo>()
                    { 
                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 0, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Coin_Bundle_0", ItemName = "Coin_Bundle_0_name", ItemType = (short)CommonDefine.ItemType.CoinBundle_Small, Price = 0, UI_GroupID = 1, UI_Sort = 5,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){ id = 0, RewardID = 1,  RewardType = (short)CommonDefine.RewardType.Coin , Quantity = 500}
                            } },
                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 1, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Coin_Bundle_1", ItemName = "Coin_Bundle_1_name", ItemType = (short)CommonDefine.ItemType.CoinBundle_Mid, Price = 0, UI_GroupID = 1, UI_Sort = 5,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){ id = 0, RewardID = 1,  RewardType = (short)CommonDefine.RewardType.Coin , Quantity = 2000}
                            } },
                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 2, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Coin_Bundle_2", ItemName = "Coin_Bundle_2_name", ItemType = (short)CommonDefine.ItemType.CoinBundle_Large, Price = 0, UI_GroupID = 1, UI_Sort = 5,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){  id = 0,RewardID = 1,  RewardType = (short)CommonDefine.RewardType.Coin , Quantity = 5000}
                            } },
                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 3, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Coin_Bundle_3", ItemName = "Coin_Bundle_3_name", ItemType = (short)CommonDefine.ItemType.CoinBundle_Mega, Price = 0, UI_GroupID = 1, UI_Sort = 5,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){  id = 0,RewardID = 1,  RewardType = (short)CommonDefine.RewardType.Coin , Quantity = 10000}
                            } },


                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 4, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Gem_Bundle_0", ItemName = "Gem_Bundle_0_name", ItemType = (short)CommonDefine.ItemType.GemBundle_Small, Price = 0, UI_GroupID = 2, UI_Sort = 6,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){ id = 0, RewardID = 2,  RewardType = (short)CommonDefine.RewardType.Gem , Quantity = 50}
                            } },
                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 5, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Gem_Bundle_1", ItemName = "Gem_Bundle_1_name",  ItemType = (short)CommonDefine.ItemType.GemBundle_Mid, Price = 0, UI_GroupID = 2, UI_Sort = 6,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){ id = 0, RewardID = 2,  RewardType = (short)CommonDefine.RewardType.Gem , Quantity = 200}
                            } },
                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 6, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Gem_Bundle_2", ItemName = "Gem_Bundle_2_name",  ItemType = (short)CommonDefine.ItemType.GemBundle_Large, Price = 0, UI_GroupID = 2, UI_Sort = 6,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){ id = 0, RewardID = 2,  RewardType = (short)CommonDefine.RewardType.Gem , Quantity = 500}
                            } },
                        new Ref_ShopInfo.ItemDataInfo(){ItemId = 7, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Gem_Bundle_3", ItemName = "Gem_Bundle_3_name",  ItemType = (short)CommonDefine.ItemType.GemBundle_Mega, Price = 0, UI_GroupID = 2, UI_Sort = 6,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){  id = 0,RewardID = 2,  RewardType = (short)CommonDefine.RewardType.Gem , Quantity = 1000}
                            } },

                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 8, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Quest_Pass_Bundle", ItemName = "Quest_Pass_name", ItemType = (short)CommonDefine.ItemType.QuestPassBundle, Price = 0, UI_GroupID = 3, UI_Sort = 1,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){  id = 0,RewardID = 0 , RewardType = (short)CommonDefine.RewardType.QuestPass , Quantity = 1, RewardName = CommonDefine.RewardType.QuestPass.ToString()},
                            } },

                        new Ref_ShopInfo.ItemDataInfo(){ ItemId = 9, PurchaseCurrencyType = (short)CommonDefine.CurrencyType.IAP, IAP_Id = "Skill_Bundle_0", ItemName = "Skill_Bundle_0_name", ItemType = (short)CommonDefine.ItemType.SkillBundle, Price = 0, UI_GroupID = 4, UI_Sort = 2,
                            RewardList = new List<Ref_ShopInfo.RewardData>()
                            {
                                new Ref_ShopInfo.RewardData(){ id = 0,  RewardID = (int)Quantum.PlayerActiveSkill.FreezeBall, RewardType = (short)CommonDefine.RewardType.ActiveSkill , Quantity = 1, RewardName = Quantum.PlayerActiveSkill.FreezeBall.ToString()},
                                new Ref_ShopInfo.RewardData(){ id = 1, RewardID = (int)Quantum.PlayerActiveSkill.FastBall, RewardType = (short)CommonDefine.RewardType.ActiveSkill , Quantity = 1, RewardName = Quantum.PlayerActiveSkill.FastBall.ToString()},
                                new Ref_ShopInfo.RewardData(){ id = 2, RewardID = (int)Quantum.PlayerActiveSkill.Shield, RewardType = (short)CommonDefine.RewardType.ActiveSkill , Quantity = 1, RewardName = Quantum.PlayerActiveSkill.Shield.ToString()},
                            } },
                    }
                },
            };
        }
    }


    #endregion

    #region Mission Data

    public Ref_MissionData missionData = new Ref_MissionData();
    [Serializable]
    public class Ref_MissionData
    {
        [SerializeField] public List<Ref_MissionInfo> MissionList = new List<Ref_MissionInfo>();

        public class Ref_MissionInfo
        {
            public int Id;
        }
    }
    #endregion

    #region Quest Pass Data

    public Ref_QuestPassData questPassData = new Ref_QuestPassData();
    [Serializable]
    public class Ref_QuestPassData
    {
        public List<Ref_QuestPassInfo> questPassList_free = new List<Ref_QuestPassInfo>();
        public List<Ref_QuestPassInfo> questPassList_paid = new List<Ref_QuestPassInfo>();

        [Serializable]
        public class Ref_QuestPassInfo
        {
            public int QuestPassID;
            public int SeasonNumber;
            public string SeasonStartTimeUTC;
            public string SeasonEndTimeUTC;
            [SerializeField] public List<Ref_QuestPassReward> RewardList = new List<Ref_QuestPassReward>();
        }

        [SerializeField]
        public class Ref_QuestPassReward
        {
            public int id;
            public int RewardID;
            public int RequiredExp;
            public short RewardType;
            public int Quantity;
        }

        public Ref_QuestPassData()
        {
            questPassList_free = new List<Ref_QuestPassInfo>()
            {
                new Ref_QuestPassInfo()
                {
                    QuestPassID = 1,
                    SeasonNumber = 1,
                    SeasonStartTimeUTC = "2024-06-04 02:04:23",
                    SeasonEndTimeUTC = "2024-10-04 02:04:23",
                    RewardList = new List<Ref_QuestPassReward>()
                    { 
                        new Ref_QuestPassReward(){ id = 0, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 10, RewardType =  (short)CommonDefine.RewardType.Gem,           Quantity = 5},
                        new Ref_QuestPassReward(){ id = 1, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 20, RewardType =  (short)CommonDefine.RewardType.Coin,          Quantity = 100},
                        new Ref_QuestPassReward(){ id = 2, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 30, RewardType =  (short)CommonDefine.RewardType.Gem,           Quantity = 5},
                        new Ref_QuestPassReward(){ id = 3, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 40, RewardType =  (short)CommonDefine.RewardType.Coin,          Quantity = 100},
                        new Ref_QuestPassReward(){ id = 4, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 50, RewardType =  (short)CommonDefine.RewardType.Gem,           Quantity = 5},
                        new Ref_QuestPassReward(){ id = 5, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 60, RewardType =  (short)CommonDefine.RewardType.Coin,          Quantity = 100},
                        new Ref_QuestPassReward(){ id = 6, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 70, RewardType =  (short)CommonDefine.RewardType.Gem,           Quantity = 5},
                        new Ref_QuestPassReward(){ id = 7, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 80, RewardType =  (short)CommonDefine.RewardType.Coin,          Quantity = 100},
                        new Ref_QuestPassReward(){ id = 8, RewardID = (int)2,                            RequiredExp = 90, RewardType =  (short)CommonDefine.RewardType.CharacterSkin, Quantity = 1},

                    },
                },
            };

            questPassList_paid = new List<Ref_QuestPassInfo>()
            {
                new Ref_QuestPassInfo()
                {
                    QuestPassID = 1,
                    SeasonNumber = 1,
                    SeasonStartTimeUTC = "2024-06-04 02:04:23",
                    SeasonEndTimeUTC = "2024-10-04 02:04:23",
                    RewardList = new List<Ref_QuestPassReward>()
                    {
                        new Ref_QuestPassReward(){ id = 0, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 10, RewardType =  (short)CommonDefine.RewardType.Gem,         Quantity = 20},
                        new Ref_QuestPassReward(){ id = 1, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 20, RewardType =  (short)CommonDefine.RewardType.Coin,        Quantity = 500},
                        new Ref_QuestPassReward(){ id = 2, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 30, RewardType =  (short)CommonDefine.RewardType.Gem,         Quantity = 5},
                        new Ref_QuestPassReward(){ id = 3, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 40, RewardType =  (short)CommonDefine.RewardType.Coin,        Quantity = 100},
                        new Ref_QuestPassReward(){ id = 4, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 50, RewardType =  (short)CommonDefine.RewardType.Gem,           Quantity = 5},
                        new Ref_QuestPassReward(){ id = 5, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 60, RewardType =  (short)CommonDefine.RewardType.Coin,          Quantity = 100},
                        new Ref_QuestPassReward(){ id = 6, RewardID = (int)CommonDefine.RewardType.Gem,  RequiredExp = 70, RewardType =  (short)CommonDefine.RewardType.Gem,           Quantity = 5},
                        new Ref_QuestPassReward(){ id = 7, RewardID = (int)CommonDefine.RewardType.Coin, RequiredExp = 80, RewardType =  (short)CommonDefine.RewardType.Coin,          Quantity = 100},
                        new Ref_QuestPassReward(){ id = 8, RewardID = 14,                                RequiredExp = 90, RewardType =  (short)CommonDefine.RewardType.WeaponSkin,  Quantity = 1},
                    },
                },
            };
        }
    }

    #endregion

    #region MatchMaking Data

    public Ref_MatchMakingData matchMakingData = new Ref_MatchMakingData();
    [Serializable]
    public class Ref_MatchMakingData
    {
        [SerializeField] public List<Ref_MatchMakingInfo> MatchMakingList = new List<Ref_MatchMakingInfo>();

        [Serializable]
        public class Ref_MatchMakingInfo
        {
            public int groupID; //MatchMakingGroup ID
            public bool isAiIncluded;   //AI 포함여부
            public int waitTime; //매칭 대기시간(초)
            [SerializeField] public List<int> includedTierList = new List<int>();
        }

        public Ref_MatchMakingData()
        {
            MatchMakingList = new List<Ref_MatchMakingInfo>()
            {
                new Ref_MatchMakingInfo(){ 
                    groupID = (int)CommonDefine.MatchMakingGroup.Beginner_Group,
                    isAiIncluded = true,
                    waitTime = 10,
                    includedTierList = new List<int>() 
                    {
                        (int)CommonDefine.RankTierType.Bronze,
                        (int)CommonDefine.RankTierType.Silver,
                        (int)CommonDefine.RankTierType.Gold,
                    }
                },

                new Ref_MatchMakingInfo(){
                    groupID = (int)CommonDefine.MatchMakingGroup.Normal_Group,
                    isAiIncluded = true,
                    waitTime = 20,
                    includedTierList = new List<int>()
                    {
                        (int)CommonDefine.RankTierType.Silver,
                        (int)CommonDefine.RankTierType.Gold,
                        (int)CommonDefine.RankTierType.Platinum,
                    }
                },

               new Ref_MatchMakingInfo(){
                    groupID = (int)CommonDefine.MatchMakingGroup.Expert_Group,
                    isAiIncluded = true,
                    waitTime = 30,
                    includedTierList = new List<int>()
                    {
                        (int)CommonDefine.RankTierType.Platinum,
                        (int)CommonDefine.RankTierType.Diamond,
                    }
                },

                new Ref_MatchMakingInfo(){
                    groupID = (int)CommonDefine.MatchMakingGroup.Pro_Group,
                    isAiIncluded = false,   //AI 포함x
                    waitTime = 100,
                    includedTierList = new List<int>()
                    {
                        (int)CommonDefine.RankTierType.Diamond,
                        (int)CommonDefine.RankTierType.Champion,
                    }
                },
            };
        }
    }
    #endregion

    #region Ranking Data
    //랭킹관련된 정의

    public Ref_RankingData rankingData = new Ref_RankingData();
    [Serializable]
    public class Ref_RankingData
    {
        [SerializeField] public List<Ref_RankingTierInfo> RankingTierList = new List<Ref_RankingTierInfo>();

        [Serializable]
        public class Ref_RankingTierInfo
        {
            public int Id;
            public short TierType; //CommonDefine RankTierType;
            public int TierRange_Min;
            public int TierRange_Max;

            public short RankingPoints_SoloMode_1st;
            public short RankingPoints_SoloMode_2nd;
            public short RankingPoints_SoloMode_3rd;
            public short RankingPoints_SoloMode_4th;
            public short RankingPoints_SoloMode_5th;
            public short RankingPoints_SoloMode_6th;
            //public short RankingPoints_SoloMode_7th;
            //public short RankingPoints_SoloMode_8th;
            //public short RankingPoints_SoloMode_9th;
            //public short RankingPoints_SoloMode_10th;

            public short RankingPoints_TeamMode_1st;
            public short RankingPoints_TeamMode_2nd;
            //public short RankingPoints_TeamMode_3rd;
            //public short RankingPoints_TeamMode_4th;
            //public short RankingPoints_TeamMode_5th;
        }

        public Ref_RankingData()
        {
            RankingTierList = new List<Ref_RankingTierInfo>()
            {
                //Bronze
                new Ref_RankingTierInfo() { Id = 0, TierType = (short)CommonDefine.RankTierType.Bronze, TierRange_Min = 0, TierRange_Max = 1499,
                RankingPoints_SoloMode_1st = 50,
                RankingPoints_SoloMode_2nd = 40,
                RankingPoints_SoloMode_3rd = 30,
                RankingPoints_SoloMode_4th = 20,
                RankingPoints_SoloMode_5th = 10,
                RankingPoints_SoloMode_6th = 0,

                RankingPoints_TeamMode_1st = 20,
                RankingPoints_TeamMode_2nd = 0,
                },
                
                //Silver
                new Ref_RankingTierInfo() { Id = 1, TierType = (short)CommonDefine.RankTierType.Silver, TierRange_Min = 1500, TierRange_Max = 2999,
                RankingPoints_SoloMode_1st = 45,
                RankingPoints_SoloMode_2nd = 35,
                RankingPoints_SoloMode_3rd = 25,
                RankingPoints_SoloMode_4th = 15,
                RankingPoints_SoloMode_5th = 0,
                RankingPoints_SoloMode_6th = -10,

                RankingPoints_TeamMode_1st = 20,
                RankingPoints_TeamMode_2nd = -10,
                },
                
                //Gold
                new Ref_RankingTierInfo() { Id = 2, TierType = (short)CommonDefine.RankTierType.Gold, TierRange_Min = 3000, TierRange_Max = 4499,
                RankingPoints_SoloMode_1st = 35,
                RankingPoints_SoloMode_2nd = 20,
                RankingPoints_SoloMode_3rd = 10,
                RankingPoints_SoloMode_4th = 0,
                RankingPoints_SoloMode_5th = -10,
                RankingPoints_SoloMode_6th = -15,

                RankingPoints_TeamMode_1st = 13,
                RankingPoints_TeamMode_2nd = -15,
                },
               
                //Platinum
                new Ref_RankingTierInfo() { Id = 3, TierType = (short)CommonDefine.RankTierType.Platinum, TierRange_Min = 4500, TierRange_Max = 5999,
                RankingPoints_SoloMode_1st = 25,
                RankingPoints_SoloMode_2nd = 15,
                RankingPoints_SoloMode_3rd = 5,
                RankingPoints_SoloMode_4th = -5,
                RankingPoints_SoloMode_5th = -15,
                RankingPoints_SoloMode_6th = -20,

                RankingPoints_TeamMode_1st = 9,
                RankingPoints_TeamMode_2nd = -20,
                },
                
                //Diamond
                new Ref_RankingTierInfo() { Id = 4, TierType = (short)CommonDefine.RankTierType.Diamond, TierRange_Min = 6000, TierRange_Max = 7499,
                RankingPoints_SoloMode_1st = 20,
                RankingPoints_SoloMode_2nd = 10,
                RankingPoints_SoloMode_3rd = 0,
                RankingPoints_SoloMode_4th = -10,
                RankingPoints_SoloMode_5th = -15,
                RankingPoints_SoloMode_6th = -20,

                RankingPoints_TeamMode_1st = 8,
                RankingPoints_TeamMode_2nd = -15,
                },
               
                //Champion
                new Ref_RankingTierInfo() { Id = 5, TierType = (short)CommonDefine.RankTierType.Champion, TierRange_Min = 7500, TierRange_Max = 20000,
                RankingPoints_SoloMode_1st = 10,
                RankingPoints_SoloMode_2nd = 5,
                RankingPoints_SoloMode_3rd = 0,
                RankingPoints_SoloMode_4th = -5,
                RankingPoints_SoloMode_5th = -10,
                RankingPoints_SoloMode_6th = -20,

                RankingPoints_TeamMode_1st = 4,
                RankingPoints_TeamMode_2nd = -10,
                },
            };
        }
    }

    #endregion
}

