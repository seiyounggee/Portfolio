using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveFalseAfterTime : MonoBehaviour
{
    public float activeFalseTime = 0f;

    private void Awake()
    {
        if (activeFalseTime <= 0f)
        {
            activeFalseTime = CommonDefine.DEFAULT_SETACTIVE_FALSE_AFTERTIME;
        }
    }

    void OnEnable()
    {
        Invoke("Deactivate", activeFalseTime);
    }

    private void Deactivate()
    {
        this.gameObject.SafeSetActive(false);
    }

    public void ResetTimer()
    {
        CancelInvoke();
        Invoke("Deactivate", activeFalseTime);
    }
}
