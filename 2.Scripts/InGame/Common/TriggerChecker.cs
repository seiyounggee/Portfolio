using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TriggerChecker : MonoBehaviour
{
    public Action<Collider> _OnTriggerEnter = null;
    public Action<Collider> _OnTriggerStay = null;
    public Action<Collider> _OnTriggerExit = null;

    private void OnTriggerEnter(Collider other)
    {
        _OnTriggerEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        _OnTriggerStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _OnTriggerExit?.Invoke(other);
    }
}
