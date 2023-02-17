using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityCoroutine
{
    public static void StartCoroutine(ref IEnumerator variable, IEnumerator func, MonoBehaviour script)
    {
        if (variable != null)
            script.StopCoroutine(variable);

        variable = func;
        script.StartCoroutine(variable);
    }

    public static void StopCoroutine(ref IEnumerator variable, MonoBehaviour script)
    {
        if (variable != null)
        {
            script.StopCoroutine(variable);
            variable = null;
        }
    }

    public static IEnumerator StartCoroutine(this IEnumerator variable, IEnumerator func, MonoBehaviour script)
    {
        variable.StopCoroutine(script);

        script.StartCoroutine(func);
        return func;
    }

    public static IEnumerator StopCoroutine(this IEnumerator variable, MonoBehaviour script)
    {
        if (variable != null)
        {
            script.StopCoroutine(variable);
        }

        return null;
    }
}