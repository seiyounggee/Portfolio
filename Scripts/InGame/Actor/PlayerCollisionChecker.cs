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
    //위에 3개 callback은 PlayerMovement.cs 에서 담당함!


    private void Awake()
    {
        if (pm == null)
            pm = GetComponentInParent<PlayerMovement>();

        if (coll == null)
            coll = GetComponent<Collider>();
    }
}
