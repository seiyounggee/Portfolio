using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class MapObject_DirectionalLight : MonoBehaviour
{
    private Light dirLight = null;

    private void Awake()
    {
        if (dirLight == null)
            dirLight = GetComponent<Light>();
    }

    private void SetData()
    {
        if (dirLight == null)
            return;
        dirLight.intensity = 1f;
        dirLight.shadowStrength = 1f;
    }

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }


}
