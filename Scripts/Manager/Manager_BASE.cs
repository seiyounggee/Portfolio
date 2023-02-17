using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_BASE : MonoSingleton<Manager_BASE>
{
    private bool isPaused = false;

    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;

        if (isPaused == false)
        {
            //���ӿ� �ٽ� ������ ���

        }
        else
        { 
            //Focus�� �Ҿ��� ���

        }
    }

}
