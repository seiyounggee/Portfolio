using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float destroyTime = 0f;

    private void Awake()
    {
        if (destroyTime <= 0f)
        {
            destroyTime = CommonDefine.DEFAULT_DESTROY_AFTERTIME;
        }
    }

    void OnEnable()
    {
        Invoke("Destroy", destroyTime);
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }
}
