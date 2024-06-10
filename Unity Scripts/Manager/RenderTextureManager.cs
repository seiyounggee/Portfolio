using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureManager : MonoSingleton<RenderTextureManager>
{
    [SerializeField] Camera renderCam;

    Unity_PlayerCharacter playerChar = null;
    Unity_Skin_Weapon weapon = null;

    public Unity_PlayerCharacter RenderTexturePlayerCharacter { get { return playerChar; } } //character + weapon
    public Unity_Skin_Weapon RenderTextureWeapon { get { return weapon; } } //weapon only


    public override void Awake()
    {
        base.Awake();

        playerChar = null;
    }

    public void ActivateRenderTexture_Player(int _characterSkinID = -1, int _weaponSkinID = -1) //-1 현재 끼고 있는 거, -2 표시x
    {
        DeactivateRenderTexture_Weapon();

        if (playerChar == null)
        {
            var prefab = PrefabManager.Instance.Unity_PlayerCharacter;
            GameObject playerObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.Euler(new Vector3(0f, -200f, 0f)));
            playerObj.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);
            playerObj.name = "RenderTexture_Player";

            playerChar = playerObj.GetComponent<Unity_PlayerCharacter>();
            playerChar.SetData_RenderTexture(_characterSkinID, _weaponSkinID);

            playerChar.transform.position = renderCam.transform.position + renderCam.transform.forward * 2.8f;
            playerChar.transform.position -= Vector3.up * 1f;
        }
        else
        {
            playerChar.SetData_RenderTexture(_characterSkinID, _weaponSkinID);
        }

        renderCam.SafeSetActive(true);
    }

    public void DeactivateRenderTexture_Player()
    {
        if (playerChar != null)
            Destroy(playerChar.gameObject);

        playerChar = null;
    }

    public void ActivateRenderTexture_Weapon(int _weaponSkinID = -1)
    {
        DeactivateRenderTexture_Player();

        if (weapon != null)
            Destroy(weapon.gameObject);

        var weaponlist = ResourceManager.Instance.WeaponSkinDataList;
        var weaponSkin = weaponlist.Find(x => x.id.Equals(_weaponSkinID));
        if (weaponSkin != null)
        {
            var weaponObj = GameObject.Instantiate(weaponSkin.obj);
            weaponObj.transform.position = renderCam.transform.position + renderCam.transform.forward * 2.8f;
            weaponObj.transform.position += Vector3.down * 0.348f; ;
            weaponObj.transform.position += Vector3.left * 0.3f;
            weaponObj.transform.rotation = Quaternion.Euler(-180f, 0f, 216f);
            weapon = weaponObj.GetComponent<Unity_Skin_Weapon>();
        }
    }

    public void DeactivateRenderTexture_Weapon()
    {
        if (weapon != null)
            Destroy(weapon.gameObject);

        weapon = null;
    }

    //성능에 큰 영향 미침... 안쓰면 항상 꺼두자...!!!! 꼭 
    public void DeactivateRenderTexture_All()
    {
        renderCam.SafeSetActive(false);

        DeactivateRenderTexture_Player();
        DeactivateRenderTexture_Weapon();
    }
}
