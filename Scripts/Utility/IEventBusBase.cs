using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

/* �̺�Ʈ ���� ����
 * Event Buss Pattern
 * ��ü(�Խ���)�� �̺�Ʈ�� �߻��ϰ� �ϸ� �ٸ� ��ü(������)�� ���� �� �ִ� ��ȣ�� ������. ��ȣ�� �۾��� ����ٴ� ���� �˸��� �˸� ����.
 */

public interface IEventBusBase<T>
{
    static Dictionary<T, List<Action>> events = new Dictionary<T, List<Action>>();

    public abstract void SubscribeEvent(T key, Action ac);
    public abstract void UnSubscribeEvent(T key, System.Action ac);
    public abstract void UnSubscribeAllEvent(T key);
    public abstract void ExcecuteEvent(T key);
}

public interface IEventBusBase<T, U>
{
    static Dictionary<T, List<Action<U>>> events = new Dictionary<T, List<Action<U>>>();

    public abstract void SubscribeEvent(T key, Action<U> ac);
    public abstract void UnSubscribeEvent(T key);
    public abstract void ExcecuteEvent(T key, U param);
}

public interface IEventBusBase<T, U, V>
{
    static Dictionary<T, List<Action<U, V>>> events = new Dictionary<T, List<Action<U, V>>>();

    public abstract void SubscribeEvent(T key, Action<U, V> ac);
    public abstract void UnSubscribeEvent(T key);
    public abstract void ExcecuteEvent(T key, U param_1, V param_2);
}

public interface IEventBusBase<T, U, V, W>
{
    static Dictionary<T, List<Action<U, V, W>>> events = new Dictionary<T, List<Action<U, V, W>>>();

    public abstract void SubscribeEvent(T key, Action<U, V, W> ac);
    public abstract void UnSubscribeEvent(T key);
    public abstract void ExcecuteEvent(T key, U param, V parm_2, W param_3);
}