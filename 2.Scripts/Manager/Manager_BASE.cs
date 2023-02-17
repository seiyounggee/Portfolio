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
            //게임에 다시 들어왔을 경우

        }
        else
        { 
            //Focus를 잃었을 경우

        }
    }

}
