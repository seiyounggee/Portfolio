using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseInGameResult : PhaseBase
{
    IEnumerator phaseInGameResultCoroutine;

    //Note : IngameResult 상태일때 Scene은 InGameScene 유지하고 있음

    protected override void OnEnable()
    {
        base.OnEnable();

        UtilityCoroutine.StartCoroutine(ref phaseInGameResultCoroutine, PhaseInGameResultCoroutine(), this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    IEnumerator PhaseInGameResultCoroutine()
    {
        yield return null;

        AccountManager.Instance.SaveMyData_FirebaseServer_All();

        UIManager.Instance.HideGrouped_Ingame();
        PrefabManager.Instance.UI_SceneTransition.Show_In();
        PrefabManager.Instance.UI_InGameResult.SetInfo();

        yield return new WaitForSeconds(1f);
        UIManager.Instance.ShowUI(UIManager.UIType.UI_InGameResult);
        PrefabManager.Instance.UI_SceneTransition.Show_Out();

        float timer = 60f;
        while (true)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                if (NetworkManager_Client.Instance != null)
                {
                    UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                    NetworkManager_Client.Instance.LeaveRoom();
                }

                break;
            }

            yield return null;
        }
    }
}
