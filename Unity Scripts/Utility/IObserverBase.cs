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

public interface IObserverBase<T>
{
    static Dictionary<MonoBehaviour, Dictionary<T, List<Action>>> notify = new Dictionary<MonoBehaviour, Dictionary<T, List<Action>>>();

    public abstract void AttachObserver(MonoBehaviour observer, T key, Action ac);

    public abstract void DetachObserver(MonoBehaviour observer);

    public abstract void NotifyToObservers(T key);
}

public interface IObserverBase<T, U>
{
    static Dictionary<MonoBehaviour, Dictionary<T, List<Action<U>>>> notify = new Dictionary<MonoBehaviour, Dictionary<T, List<Action<U>>>>();

    public abstract void AttachObserver(MonoBehaviour observer, T key, Action<U> ac);

    public abstract void DetachObserver(MonoBehaviour observer);

    public abstract void NotifyToObservers(T key, U param);
}
