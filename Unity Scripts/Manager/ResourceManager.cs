using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ResourceManager : MonoSingleton<ResourceManager>
{
    public override void Awake()
    {
        base.Awake();
    }

    public void LoadResourceData()
    {
        SetCharacterSkin();
        SetWeaponSkin();
        SetPassiveSkill();
        SetActiveSkill();
        SetMap();
        SetOutGameCommonIcon();
    }
}
