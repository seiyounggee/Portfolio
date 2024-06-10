using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public partial class ResourceManager
{
    private const string FILE_PATH_INGAME_CHARACTERSKIN_PREFAB = "UnityPrefabs/Game/Skin/CharacterSkin_Prefab";
    private const string FILE_PATH_INGAME_WEAPONSKIN_PREFAB = "UnityPrefabs/Game/Skin/WeaponSkin_Prefab";
    private const string FILE_PATH_INGAME_CHARACTERSKIN_ICON = "UnityPrefabs/Game/Skin/CharacterSkin_Icon";
    private const string FILE_PATH_INGAME_WEAPONSKIN_ICON = "UnityPrefabs/Game/Skin/WeaponSkin_Icon";

    private const string FILE_PATH_INGAME_PASSIVESKILL_ICON = "UnityPrefabs/Game/Skill/PassiveSkill_Icon";
    private const string FILE_PATH_INGAME_ACTIVESKILL_ICON = "UnityPrefabs/Game/Skill/ActiveSkill_Icon";

    private const string FILE_PATH_INGAME_MAP = "MapScene/";

    private const string FILE_PATH_OUTGAME_COMMON_ICONS = "UnityPrefabs/OutGame/Common_Icon";
    private const string FILE_PATH_OUTGAME_FLAG_ICONS = "UnityPrefabs/OutGame/Flag";

    [Serializable]
    public class Resource_SkinData
    {
        public int id;
        public GameObject obj;
        public CommonDefine.SkinType skinType;
        public Texture2D texture;
    }

    [Serializable]
    public class Resource_SkillData
    {
        public int id;
        public Texture2D texture;
        public CommonDefine.SkillType skillType;
    }

    [ReadOnly] public List<Resource_SkinData> CharacterSkinDataList = new List<Resource_SkinData>();
    [ReadOnly] public List<Resource_SkinData> WeaponSkinDataList = new List<Resource_SkinData>();

    [ReadOnly] public List<Resource_SkillData> PassiveSkillDataList = new List<Resource_SkillData>();
    [ReadOnly] public List<Resource_SkillData> ActiveSkillDataList = new List<Resource_SkillData>();

    [Serializable]
    public class OutGameIconData
    {
        public CommonDefine.OutGameCommonIconType type;
        public Texture2D texture;
    }

    [ReadOnly] public List<OutGameIconData> OutGameCommonIconDataList = new List<OutGameIconData>();

    public void SetCharacterSkin()
    {
        CharacterSkinDataList.Clear();

        var allResources_prefab = UnityEngine.Resources.LoadAll(FILE_PATH_INGAME_CHARACTERSKIN_PREFAB);
        var allResources_icon = UnityEngine.Resources.LoadAll(FILE_PATH_INGAME_CHARACTERSKIN_ICON);
        var allRefData = ReferenceManager.Instance.SkinData?.CharacterSkinList;

        if (allRefData == null || allResources_prefab == null)
            return;

        int index = 0;
        foreach (var resource in allResources_prefab)
        {
            var gameObj = resource as GameObject;
            if (gameObj != null)
            {
                var refData = allRefData.Find(x => x.SkinAssetName.Equals(gameObj.name));
                if (refData != null)
                {
                    var data = new Resource_SkinData();
                    data.obj = gameObj;
                    data.skinType = CommonDefine.SkinType.Character;
                    data.id = refData.SkinID;

                    CharacterSkinDataList.Add(data);

                    ++index;
                }
            }
        }

        //update Texture
        foreach (var i in CharacterSkinDataList)
        {
            foreach (var resource in allResources_icon)
            {
                var texture = resource as Texture2D;
                if (texture != null)
                {
                    var refData = allRefData.Find(x => x.SkinIconAssetName.Equals(texture.name) && i.id.Equals(x.SkinID));
                    if (refData != null)
                    {
                        i.texture = texture;
                        break;
                    }

                }
            }
        }


        CharacterSkinDataList.Sort((x, y) => x.id.CompareTo(y.id));

        Debug.Log("<color=cyan>[RESOURCE] Character Skin Loaded Count >>> " + index + "</color>");
    }

    public void SetWeaponSkin()
    {
        WeaponSkinDataList.Clear();

        var allResources = UnityEngine.Resources.LoadAll(FILE_PATH_INGAME_WEAPONSKIN_PREFAB);
        var allResources_icon = UnityEngine.Resources.LoadAll(FILE_PATH_INGAME_WEAPONSKIN_ICON);
        var allRefData = ReferenceManager.Instance.SkinData?.WeaponSkinList;

        if (allRefData == null || allResources == null)
            return;

        int index = 0;
        foreach (var resource in allResources)
        {
            var gameObj = resource as GameObject;
            if (gameObj != null)
            {
                var refData = allRefData.Find(x => x.SkinAssetName.Equals(gameObj.name));
                if (refData != null)
                {
                    var data = new Resource_SkinData();
                    data.obj = gameObj;
                    data.skinType = CommonDefine.SkinType.Weapon;
                    data.id = refData.SkinID;

                    WeaponSkinDataList.Add(data);

                    ++index;
                }
            }
        }

        //update Texture
        foreach (var i in WeaponSkinDataList)
        {
            foreach (var resource in allResources_icon)
            {
                var texture = resource as Texture2D;
                if (texture != null)
                {
                    var refData = allRefData.Find(x => x.SkinIconAssetName.Equals(texture.name) && i.id.Equals(x.SkinID));
                    if (refData != null)
                    {
                        i.texture = texture;
                        break;
                    }

                }
            }
        }

        WeaponSkinDataList.Sort((x, y) => x.id.CompareTo(y.id));

        Debug.Log("<color=cyan>[RESOURCE] Weapon Skin Loaded Count >>> " + index + "</color>");
    }

    public void SetPassiveSkill()
    {
        PassiveSkillDataList.Clear();

        var allResources = UnityEngine.Resources.LoadAll(FILE_PATH_INGAME_PASSIVESKILL_ICON);
        var allRefData = ReferenceManager.Instance.SkillData?.PassiveSkillInfoList;

        if (allRefData == null || allResources == null)
            return;

        int index = 0;
        foreach (var resource in allResources)
        {
            var texture = resource as Texture2D;
            if (texture != null)
            {
                var refData = allRefData.Find(x => x.SkinAssetName.Equals(texture.name));
                if (refData != null)
                {
                    var data = new Resource_SkillData();
                    data.texture = texture;
                    data.skillType = CommonDefine.SkillType.Passive;
                    data.id = refData.SkillID;

                    PassiveSkillDataList.Add(data);

                    ++index;
                }
            }
        }

        PassiveSkillDataList.Sort((x, y) => x.id.CompareTo(y.id));

        Debug.Log("<color=cyan>[RESOURCE] Passive Skill Loaded Count >>> " + index + "</color>");
    }

    public void SetActiveSkill()
    {
        ActiveSkillDataList.Clear();

        var allResources = UnityEngine.Resources.LoadAll(FILE_PATH_INGAME_ACTIVESKILL_ICON);
        var allRefData = ReferenceManager.Instance.SkillData?.ActiveSkillInfoList;

        if (allRefData == null || allResources == null)
            return;

        int index = 0;
        foreach (var resource in allResources)
        {
            var texture = resource as Texture2D;
            if (texture != null)
            {
                var refData = allRefData.Find(x => x.SkinAssetName.Equals(texture.name));
                if (refData != null)
                {
                    var data = new Resource_SkillData();
                    data.texture = texture;
                    data.skillType = CommonDefine.SkillType.Active;
                    data.id = refData.SkillID;

                    ActiveSkillDataList.Add(data);

                    ++index;
                }
            }
        }

        ActiveSkillDataList.Sort((x, y) => x.id.CompareTo(y.id));

        Debug.Log("<color=cyan>[RESOURCE] Active Skill Loaded Count >>> " + index + "</color>");
    }

    public void SetMap()
    {
        var allRefData = ReferenceManager.Instance.MapData?.MapInfoList;

        if (allRefData == null)
            return;

        foreach (var i in allRefData)
        {
            Addressables.GetDownloadSizeAsync(i.MapAddressableKey()).Completed += (size) => 
            {
                if (size.Status == AsyncOperationStatus.Succeeded && size.Result > 0)
                    Addressables.DownloadDependenciesAsync(i.MapAddressableKey(), true).Completed += (download) =>
                    {
                        if (download.Status != AsyncOperationStatus.Succeeded)
                            return;

                        OnDownloadFinished_Map(i.MapAddressableKey(), size.Result);
                    };
                else
                {
                    //size가 0이면 이미 다운로드가 완료된 상태
                    OnDownloadFinished_Map(i.MapAddressableKey(), size.Result);
                }
            };
        }
    }

    private void OnDownloadFinished_Map(string key, long size)
    {
#if SERVERTYPE_DEV || UNITY_EDITOR
        Debug.Log("<color=white>[ADDRESSABLE] OnDownloadFinished_Map >> " + key + " | size >> " + size + "</color>");
#endif
    }

    public void SetOutGameCommonIcon()
    {
        OutGameCommonIconDataList.Clear();

        var allResources = UnityEngine.Resources.LoadAll(FILE_PATH_OUTGAME_COMMON_ICONS);

        if (allResources == null)
            return;

        int index = 0;
        foreach (var resource in allResources)
        {
            var texture = resource as Texture2D;
            if (texture != null)
            {
                var data = new OutGameIconData();

                switch (texture.name)
                {
                    case "Coin_Default":
                        data.type = CommonDefine.OutGameCommonIconType.Coin_Default;
                        data.texture = texture;
                        break;
                    case "Coin_Small":
                        data.type = CommonDefine.OutGameCommonIconType.Coin_Small;
                        data.texture = texture;
                        break;
                    case "Coin_Mid":
                        data.type = CommonDefine.OutGameCommonIconType.Coin_Mid;
                        data.texture = texture;
                        break;
                    case "Coin_Large":
                        data.type = CommonDefine.OutGameCommonIconType.Coin_Large;
                        data.texture = texture;
                        break;
                    case "Coin_Mega":
                        data.type = CommonDefine.OutGameCommonIconType.Coin_Mega;
                        data.texture = texture;
                        break;


                    case "Gem_Default":
                        data.type = CommonDefine.OutGameCommonIconType.Gem_Default;
                        data.texture = texture;
                        break;
                    case "Gem_Small":
                        data.type = CommonDefine.OutGameCommonIconType.Gem_Small;
                        data.texture = texture;
                        break;
                    case "Gem_Mid":
                        data.type = CommonDefine.OutGameCommonIconType.Gem_Mid;
                        data.texture = texture;
                        break;
                    case "Gem_Large":
                        data.type = CommonDefine.OutGameCommonIconType.Gem_Large;
                        data.texture = texture;
                        break;
                    case "Gem_Mega":
                        data.type = CommonDefine.OutGameCommonIconType.Gem_Mega;
                        data.texture = texture;
                        break;


                    case "Rank_Bronze":
                        data.type = CommonDefine.OutGameCommonIconType.Rank_Bronze;
                        data.texture = texture;
                        break;
                    case "Rank_Silver":
                        data.type = CommonDefine.OutGameCommonIconType.Rank_Silver;
                        data.texture = texture;
                        break;
                    case "Rank_Gold":
                        data.type = CommonDefine.OutGameCommonIconType.Rank_Gold;
                        data.texture = texture;
                        break;
                    case "Rank_Platinum":
                        data.type = CommonDefine.OutGameCommonIconType.Rank_Platinum;
                        data.texture = texture;
                        break;
                    case "Rank_Diamond":
                        data.type = CommonDefine.OutGameCommonIconType.Rank_Diamond;
                        data.texture = texture;
                        break;
                    case "Rank_Champion":
                        data.type = CommonDefine.OutGameCommonIconType.Rank_Champion;
                        data.texture = texture;
                        break;

                    case "Stats_AttackCooltime":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_AttackCooltime;
                        data.texture = texture;
                        break;
                    case "Stats_AttackDamage":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_AttackDamage;
                        data.texture = texture;
                        break;
                    case "Stats_AttackDuration":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_AttackDuration;
                        data.texture = texture;
                        break;
                    case "Stats_AttackRange":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_AttackRange;
                        data.texture = texture;
                        break;
                    case "Stats_JumpCooltime":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_JumpCooltime;
                        data.texture = texture;
                        break;
                    case "Stats_MaxHealthPoint":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_MaxHealthPoint;
                        data.texture = texture;
                        break;
                    case "Stats_MaxSpeed":
                        data.type = CommonDefine.OutGameCommonIconType.Stats_MaxSpeed;
                        data.texture = texture;
                        break;
                }

                if (data.texture != null)
                {
                    OutGameCommonIconDataList.Add(data);
                    index++;
                }
            }
        }

        Debug.Log("<color=cyan>[RESOURCE] Icon Loaded Count >>> " + index + "</color>");
    }

}
