using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class WaypointMesh_Boundary : MonoBehaviour
{
    [HideInInspector] public MeshFilter meshFilter = null;
    [HideInInspector] public MeshRenderer meshRenderer = null;
    [HideInInspector] public MeshCollider meshCollider = null;
    [HideInInspector] public CollisionChecker collisionChecker = null;


    public void SetBoundary()
    {
        if (gameObject.GetComponent<MeshFilter>() == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        else
            meshFilter = gameObject.GetComponent<MeshFilter>();

        if (gameObject.GetComponent<MeshRenderer>() == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        else
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (gameObject.GetComponent<MeshCollider>() == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        else
            meshCollider = gameObject.GetComponent<MeshCollider>();

        this.gameObject.tag = CommonDefine.TAG_ROAD_Normal;
        int layerValue = LayerMask.NameToLayer("Ground");
        gameObject.layer = layerValue;

        if (gameObject.GetComponent<CollisionChecker>() == null)
            collisionChecker = gameObject.AddComponent<CollisionChecker>();
        else
            collisionChecker = gameObject.GetComponent<CollisionChecker>();
    }


}
