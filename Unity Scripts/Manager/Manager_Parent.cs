using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Parent : MonoSingleton<Manager_Parent>
{
    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);
    }
}