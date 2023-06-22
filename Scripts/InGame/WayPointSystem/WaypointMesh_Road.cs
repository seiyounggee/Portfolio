using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WayPointSystem;

public class WaypointMesh_Road : MonoBehaviour
{
    [HideInInspector] public MeshFilter meshFilter = null;
    [HideInInspector] public MeshRenderer meshRenderer = null;
    [HideInInspector] public MeshCollider meshCollider = null;
    [HideInInspector] public CollisionChecker collisionChecker = null;
    [HideInInspector] public Track_Basic ground_Basic = null;

    [SerializeField] public List<List<List<Waypoint>>> generatedMeshWaypoints = new List<List<List<Waypoint>>>();

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        collisionChecker = GetComponent<CollisionChecker>();
        ground_Basic = GetComponent<Track_Basic>();
    }

    public void SetRoad()
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

        if (gameObject.GetComponent<Track_Basic>() == null)
            ground_Basic = gameObject.AddComponent<Track_Basic>();
        else
            ground_Basic = gameObject.GetComponent<Track_Basic>();

        ground_Basic.collisionChecker = collisionChecker;
    }
}
