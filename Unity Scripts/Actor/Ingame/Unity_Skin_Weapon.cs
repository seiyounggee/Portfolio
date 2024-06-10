using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unity_Skin_Weapon : MonoBehaviour
{
    [ReadOnly] Material originalMaterial = null;
    [ReadOnly] MeshRenderer mesh = null;

    private void Awake()
    {
        originalMaterial = GetComponent<MeshRenderer>()?.material;
        mesh = GetComponent<MeshRenderer>();
    }

    public enum LayerType { Default, Player }
    public void ChangeLayer(LayerType type = LayerType.Player)
    {
        if (type == LayerType.Player)
        {
            gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);
            mesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);
        }
        else if (type == LayerType.Default)
        {
            gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);
            mesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);
        }
    }
}
