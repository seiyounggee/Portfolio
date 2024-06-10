using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ResourceManager
{
   
    public Texture2D GetOutGameIconByItemType(CommonDefine.ItemType type, int itemId)
    {
        //ItemType -> OutGame Icon ±¸ÇÏ±â

        Texture2D texture2D = null;

        switch (type)
        {
            case CommonDefine.ItemType.CoinBundle_Small:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Coin_Small);
                break;
            case CommonDefine.ItemType.CoinBundle_Mid:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Coin_Mid);
                break;
            case CommonDefine.ItemType.CoinBundle_Large:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Coin_Large);
                break;
            case CommonDefine.ItemType.CoinBundle_Mega:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Coin_Mega);
                break;
            case CommonDefine.ItemType.GemBundle_Small:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Gem_Small);
                break;
            case CommonDefine.ItemType.GemBundle_Mid:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Gem_Mid);
                break;
            case CommonDefine.ItemType.GemBundle_Large:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Gem_Large);
                break;
            case CommonDefine.ItemType.GemBundle_Mega:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Gem_Mega);
                break;

            case CommonDefine.ItemType.CharacterBundle:
                break;
            case CommonDefine.ItemType.WeaponBundle:
                break;
            case CommonDefine.ItemType.SkillBundle:
                break;
            case CommonDefine.ItemType.QuestPassBundle:
                break;
        }

        return texture2D;
    }

    public Texture2D GetCharacterSkinIcon(int skinID)
    {
        Texture2D texture2D = null;

        var characterSkin = CharacterSkinDataList.Find(x => x.id.Equals(skinID));
        if (characterSkin != null)
            texture2D = characterSkin.texture;
        return texture2D;
    }

    public Texture2D GetWeaponSkinIcon(int skinID)
    {
        Texture2D texture2D = null;

        var weaponSkin = WeaponSkinDataList.Find(x => x.id.Equals(skinID));
        if (weaponSkin != null)
            texture2D = weaponSkin.texture;
        return texture2D;
    }

    public Texture2D GetActiveSkillIcon(int skillId)
    {
        Texture2D texture2D = null;

        var skill = ActiveSkillDataList.Find(x => x.id.Equals(skillId));
        if (skill != null)
            texture2D = skill.texture;
        return texture2D;
    }

    public Texture2D GetPassiveSkillIcon(int skillId)
    {
        Texture2D texture2D = null;

        var skill = PassiveSkillDataList.Find(x => x.id.Equals(skillId));
        if (skill != null)
            texture2D = skill.texture;
        return texture2D;
    }

    public Texture2D GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType type)
    {
        Texture2D texture2D = null;

        var data = OutGameCommonIconDataList.Find(x => x.type.Equals(type));
        if (data != null)
            texture2D = data.texture;

        return texture2D;
    }

    public Texture2D GetCurrencyIcon(CommonDefine.CurrencyType type)
    {
        Texture2D texture2D = null;

        OutGameIconData data = null;
        switch (type)
        {
            case CommonDefine.CurrencyType.Coin:
                data = OutGameCommonIconDataList.Find(x => x.type.Equals(CommonDefine.OutGameCommonIconType.Coin_Default));
                break;
            case CommonDefine.CurrencyType.Gem:
                data = OutGameCommonIconDataList.Find(x => x.type.Equals(CommonDefine.OutGameCommonIconType.Gem_Default));
                break;
        }

        if (data != null)
            texture2D = data.texture;

        return texture2D;
    }

    public Texture2D GetOutGameRankIconByTier(CommonDefine.RankTierType type)
    {
        Texture2D texture2D = null;

        switch (type)
        {
            case CommonDefine.RankTierType.Bronze:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Rank_Bronze);
                break;
            case CommonDefine.RankTierType.Silver:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Rank_Silver);
                break;
            case CommonDefine.RankTierType.Gold:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Rank_Gold);
                break;
            case CommonDefine.RankTierType.Platinum:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Rank_Platinum);
                break;
            case CommonDefine.RankTierType.Diamond:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Rank_Diamond);
                break;
            case CommonDefine.RankTierType.Champion:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Rank_Champion);
                break;
        }

        return texture2D;
    }

    public Texture2D GetOutGameRankIconByRP(int rp)
    {
        var tier = AccountManager.Instance.GetRankingTier(rp);

        return GetOutGameRankIconByTier(tier);
    }

    public Texture2D GetStatsIcon(CommonDefine.PlayerStatsType type)
    {
        Texture2D texture2D = null;

        OutGameIconData data = null;
        switch (type)
        {
            case CommonDefine.PlayerStatsType.MaxHealthPoint:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_MaxHealthPoint);
                break;
            case CommonDefine.PlayerStatsType.AttackDamage:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_AttackDamage);
                break;
            case CommonDefine.PlayerStatsType.AttackRange:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_AttackRange);
                break;
            case CommonDefine.PlayerStatsType.AttackDuration:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_AttackDuration);
                break;
            case CommonDefine.PlayerStatsType.MaxSpeed:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_MaxSpeed);
                break;
            case CommonDefine.PlayerStatsType.AttackCooltime:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_AttackCooltime);
                break;
            case CommonDefine.PlayerStatsType.JumpCooltime:
                texture2D = GetOutGameCommonIcon(CommonDefine.OutGameCommonIconType.Stats_JumpCooltime);
                break;
        }

        if (data != null)
            texture2D = data.texture;

        return texture2D;
    }
}
