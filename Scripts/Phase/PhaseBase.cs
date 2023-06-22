using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static InGameManager;

public class PhaseBase : MonoBehaviour, IEventBus<PhaseBase.EventBusKey>
{
    protected virtual void Awake()
    {

    }

    protected virtual void OnEnable()
    { 
        
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    { 
        
    }

    protected virtual void OnDisable()
    { 
    
    }

    public enum EventBusKey 
    { 
        None, 
        PhaseInitialize, 
        Lobby,
        InGameReady,
        InGame,
        InGameResult 
    }

    static Dictionary<EventBusKey, List<Action>> events => IEventBus<EventBusKey>.events;

    public void SubscribeEvent(EventBusKey key, Action ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    public void UnSubscribeEvent(EventBusKey key, System.Action ac)
    {
        if (events.ContainsKey(key) == true)
        {
            var l = events[key];
            if (l != null)
            {
                foreach (var i in l)
                {
                    if (i.Equals(ac))
                    {
                        l.Remove(i);
                        break;
                    }
                }
            }
        }
    }

    public void UnSubscribeAllEvent(EventBusKey key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    public void ExcecuteEvent(EventBusKey key)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke();
        }
    }
}
