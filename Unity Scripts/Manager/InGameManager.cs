using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InGameManager : MonoSingleton<InGameManager>
{
    public List<InGame_Quantum.PlayerInfo> listOfPlayers { get { return InGame_Quantum.Instance?.listOfPlayers; } }
    public InGame_Quantum.PlayerInfo myPlayer { get { return InGame_Quantum.Instance?.myPlayer; } }
    public InGame_Quantum.BallInfo ball { get { return InGame_Quantum.Instance?.ball; } }

    public void StartGame_Unity()
    {
        UtilityCoroutine.StartCoroutine(ref gameRoutine, GameRoutine(), this);
    }

    private IEnumerator gameRoutine = null;
    private IEnumerator GameRoutine()
    {
        ClearStuffs();

        SetupInGameUI();
        SetupSound();
        SetInput();
        SetPoolGameObjects();

        yield break;
    }

    private void ClearStuffs()
    {
        //���ʿ��� Render Cam ������ (���ɿ� ū ���� ��ħ... �Ⱦ��� �׻� ������...)
        RenderTextureManager.Instance.DeactivateRenderTexture_All();
    }

    private void SetupInGameUI()
    {
        UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);
        UIManager.Instance.HideGrouped_Outgame();
        UIManager.Instance.ShowUI(UIManager.UIType.UI_InGame);
    }

    private void SetupSound()
    {
        SoundManager.Instance.PlaySound_BGM(SoundManager.SoundClip.BGM_InGame_01);
    }
}
