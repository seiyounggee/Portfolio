using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerBase : NetworkBehaviour
{
    [HideInInspector] public GameObject player = null;
    [HideInInspector] public PlayerBase playerBase = null;
    [HideInInspector] public PlayerMovement playerMovement = null;

    [HideInInspector] public PlayerCar playerCar = null;
    [HideInInspector] public PlayerCharacter playerCharacter = null;
 
    protected virtual void Awake()
    {
        player = gameObject;
        playerBase = gameObject.GetComponent<PlayerBase>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();

        playerCar = gameObject.GetComponentInChildren<PlayerCar>();
        playerCharacter = gameObject.GetComponentInChildren<PlayerCharacter>();
    }

    public int PlayerID
    {
        get { return Id.Behaviour; }
    }


    public NetworkObject networkObject
    {
        get { return Object; }
    }


}
