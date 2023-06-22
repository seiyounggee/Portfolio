using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class OutGameBackground : MonoBehaviour
{
    [SerializeField] public List<OutGameBackgroundParts> partsList = new List<OutGameBackgroundParts>();
    private Queue<OutGameBackgroundParts> partsQueue = new Queue<OutGameBackgroundParts>();

    private Vector3 respwanPosition_road;
    private Vector3 respwanPosition_etc;
    private float currentMoveLength = 0f;
    public float moveSpeed = 10f;
    private float moveTargetLength = 0f;
    private Vector3 moveDirection = Vector3.zero;


    [Space(10)]
    [SerializeField] private GameObject skyDome = null;
    [SerializeField] private float skyDomeMoveSpeed = 1f;

    [Serializable]
    public class OutGameBackgroundParts
    {
        public GameObject roadObj = null;
    }

    private void Start()
    {
        currentMoveLength = 0f;

        if (partsList != null && partsList.Count > 0)
        {
            foreach (var i in partsList)
            {
                if (i.roadObj == null)
                {
                    Debug.Log("Error!!!!! Fix your ougamebackground!");
                    return;
                }
            }

            respwanPosition_road = partsList[partsList.Count - 1].roadObj.transform.position;

            partsQueue.Clear();
            foreach (var i in partsList)
            {
                if (i.roadObj != null)
                    partsQueue.Enqueue(i);
            }


            var meshFilter = partsList[0].roadObj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.mesh != null)
            {
                moveTargetLength = meshFilter.mesh.bounds.size.z * partsList[0].roadObj.transform.localScale.z;
                moveDirection = partsList[0].roadObj.transform.TransformDirection(Vector3.back).normalized;
            }
            else
            {
                moveTargetLength = 40f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (partsList != null && partsList.Count > 0)
        {
            foreach (var i in partsList)
            {
                if (i.roadObj != null)
                    i.roadObj.transform.position += moveDirection * Time.fixedDeltaTime * moveSpeed;
            }

            currentMoveLength += (moveDirection * Time.fixedDeltaTime * moveSpeed).magnitude;
        }

        if (partsQueue != null && partsQueue.Count > 0)
        {
            if (currentMoveLength >= moveTargetLength)
            {
                var go = partsQueue.Peek();
                go.roadObj.transform.position = respwanPosition_road;

                partsQueue.Dequeue();
                partsQueue.Enqueue(go);

                currentMoveLength = 0f;
            }
        }

        if (skyDome != null)
        {
            skyDome.transform.RotateAround(skyDome.transform.position, Vector3.up, Time.fixedDeltaTime * skyDomeMoveSpeed);
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (true)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(respwanPosition_road, 1.3f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(respwanPosition_etc, 1.3f);
        }
    }

#endif
}
