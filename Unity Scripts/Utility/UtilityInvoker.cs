using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityInvoker
{
    private static Dictionary<MonoBehaviour, Dictionary<string, Coroutine>> activeCoroutines = new Dictionary<MonoBehaviour, Dictionary<string, Coroutine>>();

    public static void Invoke(MonoBehaviour script, Action action, float time, string key = "")
    {
        CancelInvoke(script, key);  // �̹� ���� ���� ���� Ű�� �ڷ�ƾ�� �ִٸ� ����

        if (script.SafeIsActive())
        {
            if (!activeCoroutines.ContainsKey(script))
                activeCoroutines[script] = new Dictionary<string, Coroutine>();

            var coroutine = script.StartCoroutine(Invoker(script, action, time));
            activeCoroutines[script][key] = coroutine;
        }
    }

    private static IEnumerator Invoker(MonoBehaviour script, Action action, float time = 0f, string key = "")
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();

        // Invoker ���� �Ϸ� �� ����
        if (script && activeCoroutines.ContainsKey(script) && activeCoroutines[script].ContainsKey(key))
        {
            activeCoroutines[script].Remove(key);
            if (activeCoroutines[script].Count == 0)
                activeCoroutines.Remove(script);
        }

    }

    public static void CancelInvoke(MonoBehaviour script, string key = "")
    {
        if (script.SafeIsActive() && activeCoroutines.ContainsKey(script) && activeCoroutines[script].ContainsKey(key))
        {
            script.StopCoroutine(activeCoroutines[script][key]);
            activeCoroutines[script].Remove(key);
        }
    }

    public static void CancelAllInvoke(MonoBehaviour script)
    {
        if (script.SafeIsActive() && activeCoroutines.ContainsKey(script))
        {
            foreach (var coroutine in activeCoroutines[script].Values)
            {
                script.StopCoroutine(coroutine);
            }
            activeCoroutines[script].Clear();
        }
    }
}
