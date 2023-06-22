using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

/* 옵저버 패턴
 * Observer Pattern
 * Attach된 관찰자에서 notification을 보내주는 형식
 * AttachObserver: 관찰자 등록
 * DetachObserver: 관찰자 해제
 * NotifyToObservers: 관찰자에게 Notify
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
