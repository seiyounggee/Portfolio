using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutGameManager : MonoSingleton<OutGameManager>
{
    public Unity_PlayerCharacter playerChar = null;

    public void StartOutGame()
    {
        Clear();

        SetupOutGameUI();
        SetupSound();
        SpawnPlayer();

        //���ʿ��� Render Cam ������ (���ɿ� ū ���� ��ħ... �Ⱦ��� �׻� ������...)
        RenderTextureManager.Instance.DeactivateRenderTexture_All();
    }

    public void Clear()
    {
        if (playerChar != null)
            Destroy(playerChar.gameObject);
    }

    public void SpawnPlayer()
    {
        var prefab = PrefabManager.Instance.Unity_PlayerCharacter;
        GameObject playerObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.Euler(new Vector3(0f, -200f, 0f)));
        playerChar = playerObj.GetComponent<Unity_PlayerCharacter>();
        playerObj.name = "OutGame_Player";
        playerChar.SetData_OutGame();
    }

    public void SetupOutGameUI()
    {
        UIManager.Instance.ShowUI(UIManager.UIType.UI_OutGame);

        //��ŷ �±� ���� �˾� ǥ���ؾ��ϸ� ǥ���ϱ�
        var rankPopup = PrefabManager.Instance.UI_RankChangePopup;
        if (rankPopup.IsReady)
            UIManager.Instance.ShowUI(rankPopup);
    }

    private void SetupSound()
    {
        SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.BGM_OutGame_01);
    }

    public void ChangeSpawnedPlayerData()
    {
        if (playerChar != null)
            playerChar.SetData_OutGame();
    }

    public void ChangeSpawnedPlayerAnim(Unity_PlayerCharacter.AnimState type)
    {
        if (playerChar != null)
            playerChar.ForceAnimState(type);
    }
}
