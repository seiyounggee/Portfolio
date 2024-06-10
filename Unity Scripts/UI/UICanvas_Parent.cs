using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas_Parent : MonoSingleton<UICanvas_Parent>
{
    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);
    }
}
