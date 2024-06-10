using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

/* ������ ����
 * Observer Pattern
 * Attach�� �����ڿ��� notification�� �����ִ� ����
 * AttachObserver: ������ ���
 * DetachObserver: ������ ����
 * NotifyToObservers: �����ڿ��� Notify
 */

public interface IObserver<T> : IObserverBase<T>
{
    new public void AttachObserver(MonoBehaviour observer, T key, Action ac)
    {
        if (notify.ContainsKey(observer) == false)
        {
            var l = new List<Action>();
            l.Add(ac);
            var notify = new Dictionary<T, List<Action>>();
            notify.Add(key, l);
            IObserverBase<T>.notify.Add(observer, notify);
        }
        else
        {
            if (notify[observer].ContainsKey(key) == false)
            {
                var l = new List<Action>();
                l.Add(ac);
                notify[observer].Add(key, l);
            }
            else
            {
                notify[observer][key].Add(ac);
            }
        }
    }

    new public void DetachObserver(MonoBehaviour observer)
    {
        if (notify.ContainsKey(observer) == true)
            notify.Remove(observer);
    }

    new public void NotifyToObservers(T key)
    {
        foreach (var i in notify)
        {
            if (i.Value.ContainsKey(key))
            {
                foreach (var j in i.Value[key])
                {
                    j?.Invoke();
                }
            }
        }
    }
}


public interface IObserver<T, U> : IObserverBase<T, U>
{
    new public void AttachObserver(MonoBehaviour observer, T key, Action<U> ac)
    {
        if (notify.ContainsKey(observer) == false)
        {
            var l = new List<Action<U>>();
            l.Add(ac);
            var notify = new Dictionary<T, List<Action<U>>>();
            notify.Add(key, l);
            IObserverBase<T, U>.notify.Add(observer, notify);
        }
        else
        {
            if (notify[observer].ContainsKey(key) == false)
            {
                var l = new List<Action<U>>();
                l.Add(ac);
                notify[observer].Add(key, l);
            }
            else
            {
                notify[observer][key].Add(ac);
            }
        }
    }

    new public void DetachObserver(MonoBehaviour observer)
    {
        if (notify.ContainsKey(observer) == true)
            notify.Remove(observer);
    }

    new public void NotifyToObservers(T key, U param)
    {
        foreach (var i in notify)
        {
            if (i.Value.ContainsKey(key))
            {
                foreach (var j in i.Value[key])
                {
                    j?.Invoke(param);
                }
            }
        }
    }
}