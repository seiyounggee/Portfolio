using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager
{
    [Header("Rarity")]
    public Color RarityColor_Common = Color.white;
    public Color RarityColor_Rare = Color.white;
    public Color RarityColor_Epic = Color.white;
    public Color RarityColor_Legend = Color.white;
    public Color RarityColor_Special = Color.white;

    [Header("Skill")]
    public Color SkillTypeColor_Passive = Color.white;
    public Color SkillTypeColor_Active = Color.white;

    [Header("Skin")]
    public Color SkinTypeColor_Character = Color.white;
    public Color SkinTypeColor_Weapon = Color.white;

    #region Get Color

    public static Color GetRarityColor(byte rarity)
    {
        if (Instance == null)
            return Color.white;

        switch (rarity)
        {
            case (byte)CommonDefine.Rarity.Common:
                return Instance.RarityColor_Common;

            case (byte)CommonDefine.Rarity.Rare:
                return Instance.RarityColor_Rare;

            case (byte)CommonDefine.Rarity.Epic:
                return Instance.RarityColor_Epic;

            case (byte)CommonDefine.Rarity.Legendary:
                return Instance.RarityColor_Legend;

            case (byte)CommonDefine.Rarity.Special:
                return Instance.RarityColor_Special;
        }

        return Color.white;
    }

    public static Color GetSkillTypeColor(CommonDefine.SkillType type)
    {
        if (Instance == null)
            return Color.white;

        switch (type)
        {
            case CommonDefine.SkillType.Passive:
                return Instance.SkillTypeColor_Passive;

            case CommonDefine.SkillType.Active:
                return Instance.SkillTypeColor_Active;
        }

        return Color.white;
    }

    public static Color GetSkinTypeColor(CommonDefine.SkinType type)
    {
        if (Instance == null)
            return Color.white;

        switch (type)
        {
            case CommonDefine.SkinType.Character:
                return Instance.SkinTypeColor_Character;

            case CommonDefine.SkinType.Weapon:
                return Instance.SkinTypeColor_Weapon;
        }

        return Color.white;
    }


    #endregion
}
