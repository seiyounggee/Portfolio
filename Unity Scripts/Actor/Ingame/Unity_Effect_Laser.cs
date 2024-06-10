using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unity_Effect_Laser : Unity_Effect
{
    [SerializeField] LineRenderer lineRenderer;

    private Transform fromTrans;
    private Transform toTrans;

    public void Setup(Transform _fromTrans, Transform _toTrans)
    {
        lineRenderer.positionCount = 2;

        fromTrans = _fromTrans;
        toTrans = _toTrans;
    }

    private void LateUpdate()
    {
        if (lineRenderer != null && lineRenderer.positionCount >= 2
            && fromTrans != null && toTrans != null)
        {
            lineRenderer.SetPosition(0, fromTrans.transform.position + Vector3.up);
            lineRenderer.SetPosition(1, toTrans.transform.position);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
}
