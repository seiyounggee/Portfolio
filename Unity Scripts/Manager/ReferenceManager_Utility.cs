using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ReferenceManager
{
    public int GetMaxHealthPoint()
    {
        int maxHp = PlayerDefaultStat_Quantum.MaxHealthPoint;

        (var type, var param_1, var param_2, var param_3) = AccountManager.Instance.GetCurrentPassiveSkill();
        if (type == Quantum.PlayerPassiveSkill.IncreaseMaxHp)
            maxHp += param_1;

        maxHp += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.MaxHealthPoint);

        return maxHp;
    }

    public int GetAttackDamage()
    {
        int dmg = PlayerDefaultStat_Quantum.AttackDamage;

        (var type, var param_1, var param_2, var param_3) = AccountManager.Instance.GetCurrentPassiveSkill();
        if (type == Quantum.PlayerPassiveSkill.IncreaseAttackDamage)
            dmg += param_1;

        dmg += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.AttackDamage);

        return dmg;
    }

    public int GetAttackRange()
    {
        int range = PlayerDefaultStat_Quantum.AttackRange;

        range += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.AttackRange);

        return range;
    }

    public int GetAttackDuration()
    {
        int duration = PlayerDefaultStat_Quantum.AttackDuration;

        duration += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.AttackDuration);

        return duration;
    }

    public int GetMaxSpeed()
    {
        int speed = PlayerDefaultStat_Quantum.MaxSpeed;

        (var type, var param_1, var param_2, var param_3) = AccountManager.Instance.GetCurrentPassiveSkill();
        if (type == Quantum.PlayerPassiveSkill.IncreaseSpeed)
            speed += param_1;

        speed += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.MaxSpeed);

        return speed;
    }

    public int GetInputAttackCootime()
    {
        int coolTime = PlayerDefaultStat_Quantum.Input_AttackCooltime;

        coolTime += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.AttackCooltime);

        return coolTime;
    }

    public int GetInputJumpCootime()
    {
        int coolTime = PlayerDefaultStat_Quantum.Input_JumpCooltime;

        coolTime += AccountManager.Instance.GetCurrentStats(CommonDefine.PlayerStatsType.JumpCooltime);

        return coolTime;
    }

    public int GetInputSkillCootime()
    {
        int coolTime = PlayerDefaultStat_Quantum.Input_SkillCooltime;

        var accountData = AccountManager.Instance.AccountData;
        if (accountData != null)
        {
            var skillList = SkillData.ActiveSkillInfoList;
            var skill = skillList.Find(x => x.SkillID.Equals(accountData.activeSkillID));

            if (skill != null)
            {
                switch ((Quantum.PlayerActiveSkill)accountData.activeSkillID)
                {
                    case Quantum.PlayerActiveSkill.None:
                    default:
                        break;

                    case Quantum.PlayerActiveSkill.Dash:
                    case Quantum.PlayerActiveSkill.FreezeBall:
                    case Quantum.PlayerActiveSkill.FastBall:
                    case Quantum.PlayerActiveSkill.Shield:
                    case Quantum.PlayerActiveSkill.TakeBallTarget:
                    case Quantum.PlayerActiveSkill.BlindZone:
                        {
                            coolTime = skill.defaultCoolTime;
                        }
                        break;

                }
            }

        }

        return coolTime;
    }


    public Quantum.AdditionalReferenceData GetAdditionalReferenceData()
    {
        var data = new Quantum.AdditionalReferenceData();

        return data;
    }

    public bool IsAIMatchAvailable(CommonDefine.MatchMakingGroup id)
    {
        bool isAIMatch = false;

        if (MatchMakingData == null || MatchMakingData.MatchMakingList == null)
            return false;

        var data = MatchMakingData.MatchMakingList.Find(x => x.groupID.Equals((int)id));
        if (data != null)
        {
            isAIMatch = data.isAiIncluded;
        }

        return isAIMatch;
    }

    public float GetMatchRoomWaitTime(CommonDefine.MatchMakingGroup id)
    {
        float roomWaitTime = 10f;

        if (MatchMakingData == null || MatchMakingData.MatchMakingList == null)
            return 10f;

        var data = MatchMakingData.MatchMakingList.Find(x => x.groupID.Equals((int)id));
        if (data != null)
        {
            roomWaitTime = data.waitTime;
        }

        return roomWaitTime;
    }

    public List<RefData.Ref_PlayerBasicStats.Ref_PlayerUpgradeStatsInfo> GetMyPlayerUpgradeStatsInfo()
    {
        //현재 기준 Upgrade Stat Info List

        var list = new List<RefData.Ref_PlayerBasicStats.Ref_PlayerUpgradeStatsInfo>();

        var refList = PlayerBasicStats.upgradeStatsList;
        var data_MaxHealthPoint = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.MaxHealthPoint) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_MaxHealthPoint_Level));
        var data_AttackDamage = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackDamage) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackDamage_Level));
        var data_AttackRange = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackRange) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackRange_Level));
        var data_AttackDuration = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackDuration) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackDuration_Level));
        var data_MaxSpeed = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.MaxSpeed) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_MaxSpeed_Level));
        var data_AttackCooltime = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackCooltime) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackCooltime_Level));
        var data_JumpCooltime = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.JumpCooltime) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_JumpCooltime_Level));

        if (data_MaxHealthPoint != null)
            list.Add(data_MaxHealthPoint);
        if (data_AttackDamage != null)
            list.Add(data_AttackDamage);
        if (data_AttackRange != null)
            list.Add(data_AttackRange);
        if (data_AttackDuration != null)
            list.Add(data_AttackDuration);
        if (data_MaxSpeed != null)
            list.Add(data_MaxSpeed);
        if (data_AttackCooltime != null)
            list.Add(data_AttackCooltime);
        if (data_JumpCooltime != null)
            list.Add(data_JumpCooltime);

        return list;
    }
}
