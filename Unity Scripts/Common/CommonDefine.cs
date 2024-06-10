using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonDefine
{
    public const string ProjectName = "Blader";

    public const string OutGameScene = "OutGameScene";
    public const string InGameScene = "InGameScene";

    public const float Gravity = 9.8f;

    //Layer Name
    public const string LayerName_Default = "Default";
    public const string LayerName_Hidden = "Hidden";
    public const string LayerName_Player = "Unity_PlayerCharacter";
    public const string LayerName_Ball = "Unity_Ball";

    public const string LANGUAGE_ENGLISH = "en-US";
    public const string LANGUAGE_KOREAN = "ko-kr";

    public static string QuantumVersion
    {
        get { return PhotonServerSettings.Instance.AppSettings.AppVersion; }
    }

    public static string ClientVersion
    {
        get { return Application.version; }
    }

    //Ingame Related
    public const int DEFAULT_SOLO_PLAYER = 1; //이건 당연히 1명...
    public const int DEFAULT_MAX_PLAYER = 6; //한 게임에 기본 참여자 수 (6명)
    public const int DEFAULT_PLAYER_NUM_EACH_TEAM = 3;  //한 팀에 기본 수 (3명씩 총 2팀 = 6명) //만약 2로 설정하면 3팀 //memo 현재 3:3 만약 2:2:2 로 바꾸거나 할거면 세팅해줘야할게 많음!

    public const int MAP_GROUP_ID_PRACTICE_MODE = 0;
    public const int MAP_GROUP_ID_SOLO_OR_TEAM_MODE = 1;

    public enum Phase
    { 
        None = -1,
        Initialize,
        OutGame,
        InGameReady,
        InGame,
        InGameResult
    }

    public enum SkinType
    {
        None,
        Character,
        Weapon,
    }

    public enum SkillType
    {
        None,
        Passive,
        Active,
    }

    public enum Rarity : byte
    {
        Common,
        Rare,
        Epic,
        Legendary,
        Special,

        Max,
    }

    public enum ShopType
    { 
        Common, //일반 샵 (UI_Shop)
    }

    public enum CurrencyType
    { 
        Coin,
        Gem,
        IAP,
        Unavailable, //구매할 수 없는 아이템
    }

    public enum ItemType
    { 
        CoinBundle_Small,
        CoinBundle_Mid,
        CoinBundle_Large,
        CoinBundle_Mega,

        GemBundle_Small,
        GemBundle_Mid,
        GemBundle_Large,
        GemBundle_Mega,

        CharacterBundle,

        WeaponBundle,

        SkillBundle,

        QuestPassBundle
    }

    public enum RewardType
    {
        None,
        Coin,
        Gem,
        CharacterSkin,
        WeaponSkin,
        ActiveSkill,
        PassiveSkill,
        QuestPass,

        Max,
    }

    public enum OutGameCommonIconType
    {
        None,
        Coin_Default,
        Coin_Small,
        Coin_Mid,
        Coin_Large,
        Coin_Mega,

        Gem_Default,
        Gem_Small,
        Gem_Mid,
        Gem_Large,
        Gem_Mega,

        Rank_Bronze,
        Rank_Silver,
        Rank_Gold,
        Rank_Platinum,
        Rank_Diamond,
        Rank_Champion,

        Stats_AttackCooltime,
        Stats_AttackDamage,
        Stats_AttackDuration,
        Stats_AttackRange,
        Stats_JumpCooltime,
        Stats_MaxHealthPoint,
        Stats_MaxSpeed,

    }

    public enum RankTierType
    { 
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Champion,
    }

    public enum MatchMakingGroup
    { 
        Beginner_Group = 0,     //AI 매치 허용 o
        Normal_Group = 1,       //AI 매치 허용 o
        Expert_Group = 2,       //AI 매치 허용 o
        Pro_Group = 3,       //AI 매치 허용 x
    }

    public enum PlayerStatsType
    { 
        None = 0,
        MaxHealthPoint, //최대체력
        AttackDamage,   //공격력
        AttackRange,    //공격 범위
        AttackDuration, //공격 지속시간
        MaxSpeed,       //이동속도
        AttackCooltime, //공격 쿨타임
        JumpCooltime,   //점프 쿨타임
    }

    public enum MissionType
    { 
        PlayGameCount,
        KillPlayerCount,
        AttackSuccessCount,
        UseSkillCount,
        FinalRank1stCount,
        FinalRankInside3rdCount,
    }
}