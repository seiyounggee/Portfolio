using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutGameManager : MonoSingleton<OutGameManager>
{
    [ReadOnly] public OutGamePlayer outGamePlayer = null;
    [ReadOnly] public OutGameBackground outGameBackground = null;

    private void Update()
    {
        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                string msg = "Outgame_EndApp".Localize();
                UIManager_NGUI.Instance.ActivatePanelDefault_YesNo(msg, () => { Application.Quit(); });
            }
        }
    }

    public void SpawnOutGameObjects()
    {
        if (outGamePlayer == null)
        {
            GameObject p = Instantiate(PrefabManager.Instance.OutGamePlayer, Vector3.zero, Quaternion.identity);
            outGamePlayer = p.GetComponent<OutGamePlayer>();
        }

        if (outGameBackground == null)
        {
            GameObject p = Instantiate(PrefabManager.Instance.OutGameBackground, Vector3.zero, Quaternion.identity);
            outGameBackground  = p.GetComponent<OutGameBackground>();
        }
    }

    public void SetOutGamePlayerData()
    {
        if (outGamePlayer != null)
        {
            outGamePlayer.SetData();
        }
    }

    public void InitializeOutGameUI()
    {
        PrefabManager.Instance.UI_PanelLobby_Main.Initialize();
        PrefabManager.Instance.UI_PanelLobby_TabMenu.Initialize();
        PrefabManager.Instance.UI_PanelLobby_Garage.Initialize();
        PrefabManager.Instance.UI_PanelLobby_Character.Initialize();
        PrefabManager.Instance.UI_PanelLobby_Shop.Initialize();
        PrefabManager.Instance.UI_PanelLobby_Quest.Initialize();
    }

    public void ActivateOutGamePlayerAndCam()
    {
        PrefabManager.Instance.UI_PanelLobby_Main.SetOutGameData();
        if (outGamePlayer != null)
            outGamePlayer.ChangeMovementType(OutGamePlayer.MovementType.Move);
        CameraManager.Instance.ChangeCamType(CameraManager.CamType.OutGame_MainCam_LookAt);

    }
}
