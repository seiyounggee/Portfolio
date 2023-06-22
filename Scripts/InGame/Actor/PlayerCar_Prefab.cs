using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PNIX.ReferenceTable;

public class PlayerCar_Prefab : MonoBehaviour
{
    [ReadOnly] public CRefCar dataInfo = null;

    public int carID { get { return dataInfo.ID; } }
    public int carAnimationID { get { return dataInfo.ID; } }
    public string carPrefabName { get { return dataInfo.prefabID; } }
    public string carMaterialName { get { return dataInfo.materialID; } }

    #region Components Linked in inspector
    public GameObject go = null;
    public Animator animator = null;

    public SkinnedMeshRenderer bodyMesh = null;

    public SkinnedMeshRenderer[] mesh;

    public GameObject dollyRoot = null;
    #endregion

    public void SetMaterial()
    {
        if (mesh != null && mesh.Length > 0)
        {
            foreach (var i in mesh)
            {
                if (i != null)
                {
                    foreach (var j in i.materials)
                    {
                        if (j != null)
                        {
                            if (j.shader.name.Contains(CommonDefine.ShaderName_DTRBasicLitShader))
                                j.shader = Shader.Find("Shader Graphs/" + CommonDefine.ShaderName_DTRBasicLitShader);
                            else if (j.shader.name.Contains(CommonDefine.ShaderName_DTRStandardLit))
                                j.shader = Shader.Find("Universal Render Pipeline/" + CommonDefine.ShaderName_DTRStandardLit);
                        }
                    }
                }
            }
        }
    }
}
