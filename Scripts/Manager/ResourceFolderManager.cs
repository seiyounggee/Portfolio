using PNIX.ReferenceTable;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceFolderManager : MonoSingleton<ResourceFolderManager>
{
    [SerializeField] Texture CoinIcon = null;
    [SerializeField] Texture GemIcon = null;
    [SerializeField] Texture GearIcon = null;
    [SerializeField] Texture RpIcon = null;

    [SerializeField] Texture boxNormalIcon = null;
    [SerializeField] Texture boxRareIcon = null;
    [SerializeField] Texture boxEpicIcon = null;

    public Texture GetItemTexture(DTR.Shared.EItemType type, int itemID = 0)
    {
        string iconName = string.Empty;
        switch (type)
        {
            case DTR.Shared.EItemType.None:
                break;
            case DTR.Shared.EItemType.Coin:
                {
                    if (CoinIcon != null)
                        return CoinIcon;
                    iconName = "CoinIcon_128";
                }
                break;
            case DTR.Shared.EItemType.FreeGem:
            case DTR.Shared.EItemType.PaidGem:
                {
                    if (GemIcon != null)
                        return GemIcon;
                    iconName = "GemIcon_128";
                }
                break;
            case DTR.Shared.EItemType.RacingPoint:
                {
                    if (RpIcon != null)
                        return RpIcon;
                }
                break;
            case DTR.Shared.EItemType.Energy:
                break;
            case DTR.Shared.EItemType.Car:
                {
                    iconName = "CarIcon_128";
                }
                break;
            case DTR.Shared.EItemType.Character:
                {
                    iconName = "CharacterIcon_128";
                }
                break;
            case DTR.Shared.EItemType.Gear:
            case DTR.Shared.EItemType.UnidentifiedGear:
                {
                    if (GearIcon != null)
                        return GearIcon;

                    iconName = "GearIcon_128";
                }
                break;
            case DTR.Shared.EItemType.Box:
                {
                    var refBox = CReferenceManager.Instance.FindRefBox(itemID);

                    if (refBox != null)
                    {
                        if (refBox.icon == 0 || refBox.icon == 1)
                        {
                            if (boxNormalIcon != null)
                                return boxNormalIcon;
                        }
                        else if (refBox.icon == 2)
                        {
                            if (boxRareIcon != null)
                                return boxRareIcon;
                        }
                        else if (refBox.icon == 3)
                        {
                            if (boxEpicIcon != null)
                                return boxEpicIcon;
                        }
                    }
                    else
                    {
                        if (boxNormalIcon != null)
                            return boxNormalIcon;
                    }
                }
                break;
            default:
                break;
        }

        return GetTexture(iconName);
    }
     
    public Texture GetTexture(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return null;

        string path = "UI/UITexture/" + itemID;

        var obj = UnityEngine.Resources.Load<Texture>(path);
        if (obj == null)
        {
            Debug.LogError("load failed : " + path);
            return null;
        }

        return obj;
    }
}