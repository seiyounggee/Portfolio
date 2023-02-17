using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class MonoSingletonNetworkBehaviour<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }
            return instance;
        }
    }

    public virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            if (instance != this as T)
            {
                Debug.Log("ERROR......! Same Instance is existing...! Destroy...");
                Destroy(gameObject);
                return;
            }
        }
    }

    public NetworkObject networkObject { get { return Object; } }
}
