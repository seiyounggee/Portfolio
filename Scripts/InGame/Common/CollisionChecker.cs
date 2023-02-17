using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CollisionChecker : MonoBehaviour
{
    public Action<Collision> _OnCollisionEnter = null;
    public Action<Collision> _OnCollisionStay = null;
    public Action<Collision> _OnCollisionExit = null;

    private void OnCollisionEnter(Collision collision)
    {
        _OnCollisionEnter?.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        _OnCollisionStay?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        _OnCollisionExit?.Invoke(collision);
    }
}
