using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player_AI : MonoBehaviour
{
    public PlayerMovement pm = null;
    public void Awake()
    {
        if (pm == null)
            pm = GetComponent<PlayerMovement>();

        pm.IsAI = true;
    }
}
