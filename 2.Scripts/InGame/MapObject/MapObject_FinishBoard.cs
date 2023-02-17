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
        //�̷��� �������� 2�ʸ��� üũ�ϴ°� �ʹ� ����....
        //TODO ���߿� IngameManger PlayerCompletedLap Ȱ���ؼ� ��������
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
