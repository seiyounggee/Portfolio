using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionChecker : MonoBehaviour
{
    public enum ColliderType { None, Player, Ground }
    public ColliderType currentType = ColliderType.None;
    [SerializeField] public Collider coll = null;
    [SerializeField] public PlayerMovement pm = null;

    //OnCollisionEnter OnCollisionStay OnCollisionExit 
    //���� 3�� callback�� PlayerMovement.cs ���� �����!


    private void Awake()
    {
        if (pm == null)
            pm = GetComponentInParent<PlayerMovement>();

        if (coll == null)
            coll = GetComponent<Collider>();
    }
}
