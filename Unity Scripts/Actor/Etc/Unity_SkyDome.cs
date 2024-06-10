using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unity_SkyDome : MonoBehaviour
{
    [SerializeField] bool useChangeColor = false;
    [ReadOnly] public Material material;
    [ReadOnly] public float currentValue = 1f;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<MeshRenderer>() != null)
        {
            material = GetComponent<MeshRenderer>().material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (useChangeColor == true && material != null)
        {
            Vector2 tiling = material.mainTextureScale;
            tiling.x = currentValue;
            material.mainTextureScale = tiling;

            currentValue += Time.deltaTime * 0.01f;

            if (currentValue >= 3)
                currentValue = 1;
        }
    }
}
