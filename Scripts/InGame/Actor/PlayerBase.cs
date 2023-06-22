using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class PlayerBase : NetworkBehaviour
{
    [HideInInspector] public GameObject player = null;
    [HideInInspector] public PlayerBase playerBase = null;
    [HideInInspector] public PlayerMovement playerMovement = null;

    [HideInInspector] public PlayerCar playerCar = null;
    [HideInInspector] public PlayerCharacter playerCharacter = null;

    public PlayerRef playerRef;

    protected virtual void Awake()
    {
        player = gameObject;
        playerBase = gameObject.GetComponent<PlayerBase>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();

        playerCar = gameObject.GetComponentInChildren<PlayerCar>();
        playerCharacter = gameObject.GetComponentInChildren<PlayerCharacter>();
    }

    public NetworkId networkPlayerID
    {
        get 
        {
            if (networkObject != null)
                return networkObject.Id;
            else 
                return new NetworkId(); 
        }
    }


    public NetworkObject networkObject
    {
        get 
        { 
            return
                Object; 
        }
    }
}
