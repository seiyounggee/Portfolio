using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unity_Skin_Character : MonoBehaviour
{
    public Transform rightHandPosition;
    public Transform leftHandPosition;
    public Transform chestPosition;
    public Transform waistPosition;

    [SerializeField] SkinnedMeshRenderer body_skinnedMesh = null;
    [ReadOnly][SerializeField] Material[] body_originalMaterials = null;
    [ReadOnly][SerializeField] Color[] body_originalColors;

    [SerializeField] MeshRenderer cloakMesh;
    [SerializeField] MeshRenderer bodyArmorMesh;

    [SerializeField] MeshRenderer headMesh;
    [ReadOnly][SerializeField] Material head_originalMaterial = null;
    [ReadOnly][SerializeField] Color head_originalColor;

    public enum SkinColor 
    {
        Normal = 0,
        Red,
    }

    private void Awake()
    {
        this.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);

        if (body_skinnedMesh == null)
            body_skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        if (rightHandPosition == null || leftHandPosition == null || chestPosition == null)
        {
            var childTrans = this.transform.GetComponentsInChildren<Transform>();

            foreach (var i in childTrans)
            {
                if (i.gameObject.name.Contains("RightHand"))
                {
                    rightHandPosition = i;
                }

                if (i.gameObject.name.Contains("LeftHand"))
                {
                    leftHandPosition = i;
                }

                if (i.gameObject.name.Contains("Chest"))
                {
                    chestPosition = i;
                }
            }

            if (chestPosition != null && waistPosition == null)
            {
                var waistObj = new GameObject();
                waistObj.name = "waist";
                waistObj.transform.SetParent(chestPosition);
                waistObj.transform.localPosition = new Vector3(0.334f, -0.455f, 0.563f);
                waistPosition = waistObj.transform;
            }
        }



        body_originalMaterials = GetComponentInChildren<SkinnedMeshRenderer>()?.materials;
        body_originalColors = new Color[body_originalMaterials.Length];
        for (int i = 0; i < body_originalColors.Length; i++)
            body_originalColors[i] = body_originalMaterials[i].color;

        if (headMesh != null)
        {
            head_originalMaterial = headMesh.material;
            head_originalColor = headMesh.material.color;
        }


        ChangeSkinColor(SkinColor.Normal);
    }

    public void ChangeSkinColor(SkinColor color)
    {
        if (body_originalMaterials != null && body_originalMaterials.Length > 0)
        {
            for (int i = 0; i < body_originalMaterials.Length; i++)
            {
                switch (color)
                {
                    case SkinColor.Normal:
                        {
                            body_originalMaterials[i].SetColor("_BaseColor", body_originalColors[i]);
                        }
                        break;

                    case SkinColor.Red:
                        {
                            body_originalMaterials[i].SetColor("_BaseColor", Color.red);
                        }
                        break;
                }
            }
        }

        if (head_originalMaterial != null)
        {
            switch (color)
            {
                case SkinColor.Normal:
                    {
                        head_originalMaterial.SetColor("_BaseColor", head_originalColor);
                    }
                    break;

                case SkinColor.Red:
                    {
                        head_originalMaterial.SetColor("_BaseColor", Color.red);
                    }
                    break;
            }
        }
    }

    public enum LayerType { Default, Player}
    public void ChangeLayer(LayerType type = LayerType.Player)
    {
        if (type == LayerType.Player)
        {
            gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);

            if (body_skinnedMesh)
                body_skinnedMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);

            if (headMesh)
                headMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);

            if (cloakMesh)
                cloakMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);

            if (bodyArmorMesh)
                bodyArmorMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Player);
        }
        else if (type == LayerType.Default)
        {
            gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);

            if (body_skinnedMesh)
                body_skinnedMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);

            if (headMesh)
                headMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);
        
            if(cloakMesh)
                cloakMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);

            if (bodyArmorMesh)
                bodyArmorMesh.gameObject.layer = UnityEngine.LayerMask.NameToLayer(CommonDefine.LayerName_Default);
        }
    }
}
