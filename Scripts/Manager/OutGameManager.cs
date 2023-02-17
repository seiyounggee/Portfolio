using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutGameManager : MonoSingleton<OutGameManager>
{
    [ReadOnly] public OutGamePlayer outGamePlayer = null;
    [ReadOnly] public GameObject outGameFloor = null;

    private void Update()
    {
        if (outGamePlayer != null)
        {
            Vector3 euler = outGamePlayer.transform.rotation.eulerAngles + new Vector3(0f, Time.deltaTime * 20f, 0f);
            Quaternion rot = Quaternion.Euler(euler);
            outGamePlayer.transform.rotation = rot;
        }


        if (PhaseManager.Instance.CurrentPhase == CommonDefine.Phase.Lobby)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var ui = PrefabManager.Instance.UI_PanelCommon;
                string msg = "End Game?";

                ui.SetData(msg, () => { Application.Quit(); });
                ui.Show(UI_PanelBase.Depth.High);
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

        if (outGameFloor == null)
        {
            outGameFloor = Instantiate(PrefabManager.Instance.OutGameFloor, Vector3.zero, Quaternion.identity);
            outGameFloor.transform.position = new Vector3(0f, -0.1f, -1.2f);
        }
    }

    public void SetOutGamePlayerData()
    {
        if (outGamePlayer != null)
        {
            outGamePlayer.SetData();
        }
    }
}
