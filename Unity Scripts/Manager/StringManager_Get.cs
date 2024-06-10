using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class StringManager
{
    public static string GetRarityKey(byte rarity)
    {
        switch (rarity)
        {
            case (byte)CommonDefine.Rarity.Common:
                return "COMMON_RARITY_COMMON";

            case (byte)CommonDefine.Rarity.Rare:
                return "COMMON_RARITY_RARE";

            case (byte)CommonDefine.Rarity.Epic:
                return "COMMON_RARITY_EPIC";

            case (byte)CommonDefine.Rarity.Legendary:
                return "COMMON_RARITY_LEGEND";

            case (byte)CommonDefine.Rarity.Special:
                return "COMMON_RARITY_SPECIAL";
        }

        return string.Empty;
    }

    public static string GetSkillTypeKey(CommonDefine.SkillType type)
    {
        switch (type)
        {
            case CommonDefine.SkillType.Passive:
                return "SKILL_TYPE_PASSIVE";

            case CommonDefine.SkillType.Active:
                return "SKILL_TYPE_ACTIVE";
        }

        return string.Empty;
    }

    public static string GetSkinTypeKey(CommonDefine.SkinType type)
    {
        switch (type)
        {
            case CommonDefine.SkinType.Character:
                return "SKIN_TYPE_CHARACTER";

            case CommonDefine.SkinType.Weapon:
                return "SKIN_TYPE_WEAPON";
        }

        return string.Empty;
    }

    public static string GetPassiveSkillDescKey(int type)
    {
        switch (type)
        {
            case (int)Quantum.PlayerPassiveSkill.IncreaseMaxHp:
                return "SKILL_PASSIVE_DESC_HPBOOST";

            case (int)Quantum.PlayerPassiveSkill.IncreaseSpeed:
                return "SKILL_PASSIVE_DESC_SPEEDBOOST";

            case (int)Quantum.PlayerPassiveSkill.IncreaseAttackDamage:
                return "SKILL_PASSIVE_DESC_ATTACKBOOST";
        }

        return string.Empty;
    }

    public static string GetActiveSkillDescKey(int type)
    {
        switch (type)
        {
            case (int)Quantum.PlayerActiveSkill.Dash:
                return "SKILL_ACTIVE_DESC_DASH";

            case (int)Quantum.PlayerActiveSkill.FreezeBall:
                return "SKILL_ACTIVE_DESC_FREEZEBALL";

            case (int)Quantum.PlayerActiveSkill.FastBall:
                return "SKILL_ACTIVE_DESC_FASTBALL";

            case (int)Quantum.PlayerActiveSkill.Shield:
                return "SKILL_ACTIVE_DESC_SHEILD";

            case (int)Quantum.PlayerActiveSkill.TakeBallTarget:
                return "SKILL_ACTIVE_DESC_TAKEBALLTARGET";

            case (int)Quantum.PlayerActiveSkill.BlindZone:
                return "SKILL_ACTIVE_DESC_BLINDZONE";

            case (int)Quantum.PlayerActiveSkill.ChangeBallTarget:
                return "SKILL_ACTIVE_DESC_CHANGEBALLTARGET";

            case (int)Quantum.PlayerActiveSkill.CurveBall:
                return "SKILL_ACTIVE_DESC_CURVEBALL";

            case (int)Quantum.PlayerActiveSkill.SkyRocketBall:
                return "SKILL_ACTIVE_DESC_SKYROCKETBALL";
        }

        return string.Empty;
    }

    public static string GetCharacterSkinDescKey(int skinID)
    {
        switch (skinID)
        {
            case 0:
                return "SKIN_CHARACTER_DESC_SHADOW";

            case 1:
                return "SKIN_CHARACTER_DESC_CISAB";

            case 2:
                return "SKIN_CHARACTER_DESC_JOCKER";

            case 3:
                return "SKIN_CHARACTER_DESC_DRONAX";

            case 4:
                return "SKIN_CHARACTER_DESC_FAZZLE";

            case 5:
                return "SKIN_CHARACTER_DESC_ZORPLE";

            case 6:
                return "SKIN_CHARACTER_DESC_VEXLO";

            case 7:
                return "SKIN_CHARACTER_DESC_FLOWEY";

            case 8:
                return "SKIN_CHARACTER_DESC_DAVILL";
        }

        return string.Empty;
    }

    public static string GetWeaponSkinDescKey(int skinID)
    {
        //юс╫ц..?
        return "SKIN_WEAPON_DESC_" + skinID;
    }

    public static string GetRankingTierNameKey(int rp)
    {
        string key = string.Empty;

        var tier = AccountManager.Instance.GetRankingTier(rp);
        switch (tier)
        {
            case CommonDefine.RankTierType.Bronze:
                key = "COMMON_TIER_BRONZE";
                break;
            case CommonDefine.RankTierType.Silver:
                key = "COMMON_TIER_SILVER";
                break;
            case CommonDefine.RankTierType.Gold:
                key = "COMMON_TIER_GOLD";
                break;
            case CommonDefine.RankTierType.Platinum:
                key = "COMMON_TIER_PLATINUM";
                break;
            case CommonDefine.RankTierType.Diamond:
                key = "COMMON_TIER_DIAMOND";
                break;
            case CommonDefine.RankTierType.Champion:
                key = "COMMON_TIER_CHAMPION";
                break;
        }

        return key;
    }

    public static string GetRankingTierNameKey(CommonDefine.RankTierType type)
    {
        string key = string.Empty;

        switch (type)
        {
            case CommonDefine.RankTierType.Bronze:
                key = "COMMON_TIER_BRONZE";
                break;
            case CommonDefine.RankTierType.Silver:
                key = "COMMON_TIER_SILVER";
                break;
            case CommonDefine.RankTierType.Gold:
                key = "COMMON_TIER_GOLD";
                break;
            case CommonDefine.RankTierType.Platinum:
                key = "COMMON_TIER_PLATINUM";
                break;
            case CommonDefine.RankTierType.Diamond:
                key = "COMMON_TIER_DIAMOND";
                break;
            case CommonDefine.RankTierType.Champion:
                key = "COMMON_TIER_CHAMPION";
                break;
        }

        return key;
    }

    public static string GetIngameTeamName(int teamID)
    {
        string key = string.Empty;

        switch (teamID)
        {
            case 0:
                key = "INGAME_TEAM_RED";
                break;
            case 1:
                key = "INGAME_TEAM_BLUE";
                break;
            case 2:
                key = "INGAME_TEAM_GREEN";
                break;

            default:
                key = teamID.ToString();
                break;

        }

        return key;
    }

    public static Color GetIngameTeamColor(int teamID)
    {
        switch (teamID)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.blue;
            case 2:
                return Color.green;

            default:
                return Color.white;

        }
        return Color.white;
    }

    public static string GetInGameReadyTip_Random()
    {
        var index = Random.Range(1, 5);
        switch (index)
        {
            case 1:
                return "INGAMEREADY_TIP_1";

            case 2:
                return "INGAMEREADY_TIP_2";

            case 3:
                return "INGAMEREADY_TIP_3";

            case 4:
                return "INGAMEREADY_TIP_4";

            default:
                return "INGAMEREADY_TIP_1";
        }
    }

    public static string GetCurrencyKey(CommonDefine.CurrencyType type)
    {
        switch (type)
        {
            case CommonDefine.CurrencyType.Coin:
                return "COMMON_CURRENCY_COIN";
            case CommonDefine.CurrencyType.Gem:
                return "COMMON_CURRENCY_GEM";
            case CommonDefine.CurrencyType.IAP:
                return "COMMON_CURRENCY_IAP";
            default:
                return "";
        }
    }
}
