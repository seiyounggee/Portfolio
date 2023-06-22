using PNIX.ReferenceTable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter_Prefab : MonoBehaviour
{
    [ReadOnly] public CRefCharacter dataInfo = null;

    public int charID { get { return dataInfo.ID; } }
    public int charAnimationID { get { return dataInfo.ID; } }
    public string charPrefabName { get { return dataInfo.prefabID; } }
    public string charMatName { get { return dataInfo.materialID; } }

    public GameObject go = null;
    public Animator animator = null;

    public SkinnedMeshRenderer[] mesh;

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
