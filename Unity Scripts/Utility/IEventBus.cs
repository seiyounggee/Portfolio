using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

/* 이벤트 버스 패턴
 * Event Buss Pattern
 * 객체(게시자)가 이벤트를 발생하게 하면 다른 객체(구독자)가 받을 수 있는 신호를 보낸다. 신호는 작업이 생겼다는 것을 알리는 알림 형식.
 */

public interface IEventBus<T> : IEventBusBase<T>
{
    new public void SubscribeEvent(T key, Action ac)
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

    new public void UnSubscribeEvent(T key, System.Action ac)
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

    new public void UnSubscribeAllEvent(T key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    new public void ExcecuteEvent(T key)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke();
        }
    }
}

public interface IEventBus<T, U> : IEventBusBase<T, U>
{
    new public void SubscribeEvent(T key, Action<U> ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action<U>>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    new public void UnSubscribeEvent(T key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    new public void ExcecuteEvent(T key, U param)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke(param);
        }
    }
}

public interface IEventBus<T, U, V> : IEventBusBase<T, U, V>
{
    new public void SubscribeEvent(T key, Action<U, V> ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action<U, V>>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    new public void UnSubscribeEvent(T key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    new public void ExcecuteEvent(T key, U param, V parm_2)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke(param, parm_2);
        }
    }
}

public interface IEventBus<T, U, V, W> : IEventBusBase<T, U, V, W>
{
    new public void SubscribeEvent(T key, Action<U, V, W> ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action<U, V, W>>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    new public void UnSubscribeEvent(T key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    new public void ExcecuteEvent(T key, U param, V parm_2, W param_3)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke(param, parm_2, param_3);
        }
    }
}