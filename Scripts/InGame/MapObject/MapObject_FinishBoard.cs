using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_FinishBoard : MonoBehaviour
{
    [SerializeField] MeshRenderer mr = null;

    void Start()
    {
        if (mr != null)
        {
            mr.enabled = false;
        }
        else
        {
            mr = GetComponent<MeshRenderer>();
            if (mr != null)
                mr.enabled = false;
        }

        UtilityCoroutine.StartCoroutine(ref checkForActivation, CheckForActivation(), this);
    }

    private IEnumerator checkForActivation = null;
    private IEnumerator CheckForActivation()
    {
        //이렇게 무한으로 2초마다 체크하는거 너무 구림....
        //TODO 나중에 IngameManger PlayerCompletedLap 활용해서 수정하자
        while (true)
        {
            if (InGameManager.Instance.gameState == InGameManager.GameState.PlayGame)
            {
                if (InGameManager.Instance.myPlayer != null && mr != null)
                {
                    if (InGameManager.Instance.myPlayer.IsCurrentLastLap())
                    {
                        yield return new WaitForSeconds(2f);
                        mr.enabled = true;
                        break;
                    }
                }
            }


            yield return new WaitForSeconds(1f);
        }
    }

}
