using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RefData.Ref_ShopData;

public partial class AccountManager
{
    #region Set Data
    //재화 소모
    public void Use_Currency(CommonDefine.CurrencyType type, int quantity, Action callback = null)
    {
        if (accountData != null)
        {
            switch (type)
            {
                case CommonDefine.CurrencyType.Coin:
                    accountData.coin -= quantity;
                    break;
                case CommonDefine.CurrencyType.Gem:
                    accountData.gem -= quantity;
                    break;
                case CommonDefine.CurrencyType.IAP:
                    break;
            }
        }
    }

    //재화 소모 + 저장
    public void UseAndSave_Currency(CommonDefine.CurrencyType type, int quantity, Action callback = null)
    {
        if (accountData != null)
        {
            switch (type)
            {
                case CommonDefine.CurrencyType.Coin:
                    accountData.coin -= quantity;
                    break;
                case CommonDefine.CurrencyType.Gem:
                    accountData.gem -= quantity;
                    break;
                case CommonDefine.CurrencyType.IAP:
                    break;
            }
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            callback?.Invoke();
        });
    }

    public void SetPurchaseShopData(int shopId, int itemID, short currencyType)
    {
        AccountData.PurchaseShopData data = new AccountData.PurchaseShopData();
        data.purchaseShopID = shopId;
        data.purchaseItemID = itemID;
        data.currencyType = currencyType;
        data.purchaseDate = NetworkManager_Client.Instance.ServerTime_UTC_String;

        //temp... 가장 최근 30개의 구매정보만 기록하자...
        if (accountData.purchaseShopDataList_All.Count > 30)
            accountData.purchaseShopDataList_All.RemoveAt(0);

        accountData.purchaseShopDataList_All.Add(data);

        //IAP 구매내역은 계속 누적시키자 (중요한 데이터니까 삭제x)
        if(currencyType == (short)CommonDefine.CurrencyType.IAP)
            accountData.purchaseShopDataList_IAP.Add(data);
    }

    public void SetMatchData(NetworkManager_Client.RoomData roomData, Quantum.InGamePlayStatisticsArray statistics)
    {
        AccountData.MatchData data = new AccountData.MatchData();
        data.matchType = (short)roomData.ingamePlayMode;
        data.mapID = roomData.mapID;

        //이거 수정해야함... 하나로 통일이 필요... 안그럼 다 다르게 나올듯
        data.matchDate = NetworkManager_Client.Instance.ServerTime_UTC_String;

        //temp... 가장 최근 30개의 구매정보만 기록하자...
        if (accountData.matchDataList.Count > 30)
            accountData.matchDataList.RemoveAt(0);

        accountData.matchDataList.Add(data);
    }

    //List보상 수령
    public void GainAndSave_ShopRewardList(List<Ref_ShopInfo.RewardData> list, Action<bool> callback = null)
    {
        foreach (var i in list)
        {
            Gain_RewardSingle((CommonDefine.RewardType)i.RewardType, i.RewardID, i.Quantity);
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            callback?.Invoke(isSuccess);
        });
    }

    //단일보상 수형
    public void Gain_RewardSingle(CommonDefine.RewardType type, int rewardID, int quantity)
    {
        switch (type)
        {
            case CommonDefine.RewardType.Coin:
                {
                    accountData.coin += quantity;
                }
                break;
            case CommonDefine.RewardType.Gem:
                {
                    accountData.gem += quantity;
                }
                break;
            case CommonDefine.RewardType.CharacterSkin:
                {
                    if (accountData.ownedCharacterSkin.Contains(rewardID) == false)
                        accountData.ownedCharacterSkin.Add(rewardID);
                }
                break;
            case CommonDefine.RewardType.WeaponSkin:
                {
                    if (accountData.ownedWeaponSkin.Contains(rewardID) == false)
                        accountData.ownedWeaponSkin.Add(rewardID);
                }
                break;
            case CommonDefine.RewardType.ActiveSkill:
                {
                    if (accountData.ownedActiveSkill.Contains(rewardID) == false)
                        accountData.ownedActiveSkill.Add(rewardID);
                }
                break;
            case CommonDefine.RewardType.PassiveSkill:
                {
                    if (accountData.ownedPassiveSkill.Contains(rewardID) == false)
                        accountData.ownedPassiveSkill.Add(rewardID);
                }
                break;
            case CommonDefine.RewardType.QuestPass:
                {
                    //TODO...
#if UNITY_EDITOR
                    Debug.Log("<color=red>TODO >>> Quest Pass </color>");
#endif
                }
                break;
        }

    }

    public void Save_OwnedSkinData(CommonDefine.SkinType type, int skinId, Action<bool> callback = null)
    {
        switch (type)
        {
            case CommonDefine.SkinType.Character:
                {
                    if (accountData.ownedCharacterSkin.Contains(skinId) == false)
                        accountData.ownedCharacterSkin.Add(skinId);
                }
                break;

            case CommonDefine.SkinType.Weapon:
                {
                    if (accountData.ownedWeaponSkin.Contains(skinId) == false)
                        accountData.ownedWeaponSkin.Add(skinId);
                }
                break;
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke(isSuccess);
            }
        });
    }

    public void Save_SkinData(CommonDefine.SkinType type, int skinId, Action<bool> callback = null)
    {

        switch (type)
        {
            case CommonDefine.SkinType.Character:
                {
                    if (accountData.ownedCharacterSkin.Contains(skinId) == false)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    accountData.characterSkinID = skinId;
                }
                break;
            case CommonDefine.SkinType.Weapon:
                {
                    if (accountData.ownedWeaponSkin.Contains(skinId) == false)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    accountData.weaponSkinID = skinId;
                }
                break;
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke(isSuccess);
            }
        });
    }

    public void Save_OwnedSkillData(CommonDefine.SkillType type, int skillId, Action<bool> callback = null)
    {
        switch (type)
        {
            case CommonDefine.SkillType.Passive:
                {
                    if (accountData.ownedPassiveSkill.Contains(skillId) == false)
                        accountData.ownedPassiveSkill.Add(skillId);
                }
                break;

            case CommonDefine.SkillType.Active:
                {
                    if (accountData.ownedActiveSkill.Contains(skillId) == false)
                        accountData.ownedActiveSkill.Add(skillId);
                }
                break;
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke(isSuccess);
            }
        });
    }

    public void Save_SkillData(CommonDefine.SkillType type, int skillId, Action<bool> callback = null)
    {
        var data = AccountManager.Instance.AccountData;

        switch (type)
        {
            case CommonDefine.SkillType.Passive:
                {
                    if (accountData.ownedPassiveSkill.Contains(skillId) == false)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    accountData.passiveSkillID = skillId;
                }
                break;

            case CommonDefine.SkillType.Active:
                {
                    if (accountData.ownedActiveSkill.Contains(skillId) == false)
                    {
                        callback?.Invoke(false);
                        return;

                    }
                    accountData.activeSkillID = skillId;
                }
                break;
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke(isSuccess);
            }
        });
    }

    public void SetIngamePlayMode(Quantum.InGamePlayMode mode)
    {
        if (AccountData != null)
        {
            AccountData.ingameSettings_ingamePlayMode = (int)mode;
        }
    }

    public void SetCameraMode(CameraManager.InGameCameraMode mode)
    {
        if (AccountData != null)
        {
            AccountData.ingameSettings_cameramode = (int)mode;
        }
    }

    public void SetCameraOffsetDist(float dist)
    {
        if (AccountData != null)
        {
            AccountData.ingameSettings_cameraOffsetDistance = dist;
        }
    }

    public void Save_StatsData(CommonDefine.PlayerStatsType type, short level, Action<bool> callback = null)
    {
        if (AccountData == null)
            return;
            
        switch (type)
        {
            case CommonDefine.PlayerStatsType.MaxHealthPoint:
                AccountData.PlayerStats_MaxHealthPoint_Level = level;
                break;
            case CommonDefine.PlayerStatsType.AttackDamage:
                AccountData.PlayerStats_AttackDamage_Level = level;
                break;
            case CommonDefine.PlayerStatsType.AttackRange:
                AccountData.PlayerStats_AttackRange_Level = level;
                break;
            case CommonDefine.PlayerStatsType.AttackDuration:
                AccountData.PlayerStats_AttackDuration_Level = level;
                break;
            case CommonDefine.PlayerStatsType.MaxSpeed:
                AccountData.PlayerStats_MaxSpeed_Level = level;
                break;
            case CommonDefine.PlayerStatsType.AttackCooltime:
                AccountData.PlayerStats_AttackCooltime_Level = level;
                break;
            case CommonDefine.PlayerStatsType.JumpCooltime:
                AccountData.PlayerStats_JumpCooltime_Level = level;
                break;
        }

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke(isSuccess);
            }
        });
    }

    public void Save_Nickname(string newNickname, Action<bool> callback = null)
    {
        if (AccountData == null)
            return;

        accountData.nickname = newNickname;
        accountData.nicknameChangeCount += 1;

        SaveMyData_FirebaseServer_All((isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke(isSuccess);
            }
        });
    }

    #endregion


    #region Get Data

    public (Quantum.PlayerActiveSkill, int, int, int) GetCurrentActiveSkill()
    {
        if (AccountData != null && ReferenceManager.Instance.SkillData != null)
        {
            var skillList = ReferenceManager.Instance.SkillData.ActiveSkillInfoList;
            var skill = skillList.Find(x => x.SkillID.Equals(accountData.activeSkillID));
            if (skill != null)
                return ((Quantum.PlayerActiveSkill)accountData.activeSkillID, skill.param_1, skill.param_2, skill.param_3);
        }

        return (Quantum.PlayerActiveSkill.None, 0, 0, 0);
    }

    public (Quantum.PlayerPassiveSkill, int, int, int) GetCurrentPassiveSkill()
    {
        if (AccountData != null && ReferenceManager.Instance.SkillData != null)
        {
            var skillList = ReferenceManager.Instance.SkillData.PassiveSkillInfoList;
            var skill = skillList.Find(x => x.SkillID.Equals(accountData.passiveSkillID));
            if (skill != null)
                return ((Quantum.PlayerPassiveSkill)accountData.passiveSkillID, skill.param_1, skill.param_2, skill.param_3);
        }

        return (Quantum.PlayerPassiveSkill.None, 0, 0, 0);
    }

    public CommonDefine.RankTierType GetRankingTier(int rp)
    {
        CommonDefine.RankTierType tier = CommonDefine.RankTierType.Bronze;

        var rankingDataList = ReferenceManager.Instance.RankingData.RankingTierList;
        var data = rankingDataList.Find(x => x.TierRange_Min <= rp && x.TierRange_Max >= rp);
        if (data != null)
        {
            tier = (CommonDefine.RankTierType)data.TierType;
        }

        return tier;
    }

    public CommonDefine.RankTierType GetNextHigherRankingTier(int rp)
    {
        CommonDefine.RankTierType currTier = GetRankingTier(rp);
        CommonDefine.RankTierType nextTier = CommonDefine.RankTierType.Bronze;

        if (currTier < CommonDefine.RankTierType.Champion)
            nextTier = currTier + 1;
        else
            nextTier = CommonDefine.RankTierType.Champion; //마지막 챔피온은 다음 티어가 없음

        return nextTier;
    }

    public CommonDefine.RankTierType GetNextLowerRankingTier(int rp)
    {
        CommonDefine.RankTierType currTier = GetRankingTier(rp);
        CommonDefine.RankTierType nextTier = CommonDefine.RankTierType.Bronze;

        if (currTier > CommonDefine.RankTierType.Bronze)
            nextTier = currTier - 1;
        else
            nextTier = CommonDefine.RankTierType.Bronze; //브론즈 밑은 없음

        return nextTier;
    }

    public int GetCurrentStats(CommonDefine.PlayerStatsType type)
    {
        var refList = ReferenceManager.Instance.PlayerBasicStats.upgradeStatsList;
        var data_MaxHealthPoint = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.MaxHealthPoint) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_MaxHealthPoint_Level));
        var data_AttackDamage = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackDamage) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackDamage_Level));
        var data_AttackRange = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackRange) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackRange_Level));
        var data_AttackDuration = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackDuration) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackDuration_Level));
        var data_MaxSpeed = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.MaxSpeed) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_MaxSpeed_Level));
        var data_AttackCooltime = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.AttackCooltime) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_AttackCooltime_Level));
        var data_JumpCooltime = refList.Find(x => x.type.Equals((short)CommonDefine.PlayerStatsType.JumpCooltime) && x.level.Equals(AccountManager.Instance.AccountData.PlayerStats_JumpCooltime_Level));


        switch (type)
        {
            case CommonDefine.PlayerStatsType.MaxHealthPoint:
                if (data_MaxHealthPoint != null)
                    return data_MaxHealthPoint.param_1;
                else
                    return 0;
            case CommonDefine.PlayerStatsType.AttackDamage:
                if (data_AttackDamage != null)
                    return data_AttackDamage.param_1;
                else
                    return 0;
            case CommonDefine.PlayerStatsType.AttackRange:
                if (data_AttackRange != null)
                    return data_AttackRange.param_1;
                else
                    return 0;
            case CommonDefine.PlayerStatsType.AttackDuration:
                if (data_AttackDuration != null)
                    return data_AttackDuration.param_1;
                else
                    return 0;
            case CommonDefine.PlayerStatsType.MaxSpeed:
                if (data_MaxSpeed != null)
                    return data_MaxSpeed.param_1;
                else
                    return 0;
            case CommonDefine.PlayerStatsType.AttackCooltime:
                if (data_AttackCooltime != null)
                    return data_AttackCooltime.param_1;
                else
                    return 0;
            case CommonDefine.PlayerStatsType.JumpCooltime:
                if (data_JumpCooltime != null)
                    return data_JumpCooltime.param_1;
                else
                    return 0;
        }

        return 0;
    }

    #endregion
}
