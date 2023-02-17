using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class UtilityCommon
{
    public static List<T> ShuffleList<T>(ref List<T> list)
    {
        System.Random rnd = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static void DebugColorLog(string log)
    {
        if (string.IsNullOrEmpty(log) == true)
            return;

        Debug.Log("<color=yellow>" + log + "</color>");
    }

    public static void ChangeAllLayers(Transform trans, string name)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in trans)
        {
            if(child != null)
                ChangeAllLayers(child, name);
        }
    }

    public static string GetTimeString_TYPE_1(double totalSeconds)
    {
        // example: 15m 33s

        double mincheck = 60;
        double hourcheck = 60 * 60;  //3600
        double daychek = 60 * 60 * 24;   //84600
        double share = totalSeconds / daychek;
        double rest = 0;
        // Day 
        if (share >= 1)
        {
            rest = totalSeconds - (daychek * (int)share);
            rest /= hourcheck;
            return string.Format("{0}d {1}h", (int)share, (int)rest);
        }
        else
        {
            share = totalSeconds / hourcheck;
            // Hour
            if (share >= 1)
            {
                rest = totalSeconds - (hourcheck * (int)share);
                rest /= mincheck;
                return string.Format("{0}h {1}m", (int)share, (int)rest);
            }
            else
            {
                share = totalSeconds / mincheck;
                // Min sec
                if (share >= 1)
                {
                    rest = totalSeconds - (mincheck * (int)share);
                    return string.Format("{0}m {1}s", (int)share, (int)rest);
                }
                else
                    return string.Format("{0}s", (int)totalSeconds);
            }
        }
    }

    public static string GetTimeString_TYPE_2(double totalSeconds)
    {
        int min = (int)(totalSeconds / 60);
        double sec = totalSeconds - min * 60;

        string minString = "";
        if (min < 10) //1의 자리 수가 존재 안할경우
            minString = "0" + min.ToString();
        else
            minString = min.ToString();

        string secString = "";
        sec = Math.Round(sec, 2);
        if (sec < 10) //1의 자리 수가 존재 안할경우
            secString = "0" + string.Format("{0:f2}", sec);
        else
            secString = string.Format("{0:f2}", sec);

        string txt = minString + ":" + secString;

        //string txt = minString + ":" + string.Format("{0:f2}", Math.Round(sec, 2));
        //return string.Format("{0}:{1}", min, Math.Round(sec, 2));
        return txt;

    }

    #region Mono

    public static void SafeSetActive(this GameObject go, bool isOn)
    {
        if (go != null)
        {
            go.gameObject.SetActive(isOn);
        }
    }

    public static void SafeSetActive(this UnityEngine.UI.Button btn, bool isOn)
    {
        if (btn != null)
        {
            btn.gameObject.SetActive(isOn);
        }
    }

    //UGUI Text
    public static void SafeSetText(this Text txt, string msg)
    {
        if (txt != null)
        {
            txt.text = msg;
        }
    }

    //NGUI Text
    public static void SafeSetText(this UILabel txt, string msg)
    {
        if (txt != null)
        {
            txt.text = msg;
        }
    }

    //UGUI Click Event
    public static void SafeSetButton(this Button btn, UnityAction func)
    {
        if (btn != null && func != null)
        {
            btn.onClick.AddListener(func);
        }
    }

    //UGUI Click Event
    public delegate void VoidBtnDelegate(Button btn);
    public static void SafeSetButton(this Button btn, VoidBtnDelegate callback)
    {
        UnityAction func;
        if (btn != null && callback != null)
        {
            func = () => callback(btn);
            btn.onClick.AddListener(func);
        }
    }

    //NGUI Click Event
    public static void SafeSetButton(this GameObject btn, UIEventListener.VoidDelegate callback)
    {
        if (btn != null)
        {
            UIEventListener.Get(btn).onClick = callback;
        }
    }

    //NGUI Drag Event
    public static void SafeSetDrag(this GameObject go, UIEventListener.VectorDelegate callback)
    {
        if (go != null)
        {
            UIEventListener.Get(go).onDrag = callback;
        }
    }

    //NGUI Drag Event
    public static void SafeSetDrag(this GameObject go, UIEventListener.Custom_DragInfo_Delegate callback)
    {
        if (go != null)
        {
            UIEventListener.Get(go).onCustomDrag = callback;
        }
    }

    //NGUI Drag Start Event
    public static void SafeSetDragStart(this GameObject go, UIEventListener.VoidDelegate callback)
    {
        if (go != null)
        {
            UIEventListener.Get(go).onDragStart = callback;
        }
    }

    //NGUI Drag End Event
    public static void SafeSetDragEnd(this GameObject go, UIEventListener.VoidDelegate callback)
    {
        if (go != null)
        {
            UIEventListener.Get(go).onDragEnd = callback;
        }
    }

    //NGUI Press Event
    public static void SafeSetPressd(this GameObject go, UIEventListener.BoolDelegate callback)
    {
        if (go != null)
        {
            UIEventListener.Get(go).onPress = callback;
        }
    }

    //Animation
    public static void SafeSetBool(this Animator anim, string animName, bool isOn)
    {
        if (anim != null && string.IsNullOrEmpty(animName) == false)
        {
            anim.SetBool(animName, isOn);
        }
    }

    public static void SafeSetTrigger(this Animator anim, string animName)
    {
        if (anim != null && string.IsNullOrEmpty(animName) == false)
        {
            anim.SetTrigger(animName);   
        }
    }


    public enum DebugColor { None, White, Black, Cyan, Red, Yellow, Blue, Magenta }
    public static void ColorLog(string msg, DebugColor color)
    {
        if (string.IsNullOrEmpty(msg) == false)
        {
#if UNITY_EDITOR
            switch (color)
            {
                case DebugColor.None:
                    Debug.Log(msg);
                    break;
                case DebugColor.White:
                    Debug.Log("<color=white>" + msg + "</color>");
                    break;
                case DebugColor.Black:
                    Debug.Log("<color=black>" + msg + "</color>");
                    break;
                case DebugColor.Cyan:
                    Debug.Log("<color=cyan>" + msg + "</color>");
                    break;
                case DebugColor.Red:
                    Debug.Log("<color=red>" + msg + "</color>");
                    break;
                case DebugColor.Yellow:
                    Debug.Log("<color=yellow>" + msg + "</color>");
                    break;
                case DebugColor.Blue:
                    Debug.Log("<color=blue>" + msg + "</color>");
                    break;
                case DebugColor.Magenta:
                    Debug.Log("<color=magenta>" + msg + "</color>");
                    break;
                default:
                    Debug.Log(msg);
                    break;
            }
#else
            Debug.Log(msg);
#endif
        }
        else
            Debug.Log("Debug Log Msg is Null...?");
    }

#endregion

#region Action Invoke

    public static void SafeInvoke<T>(this Action<T> action, T t)
    {
        if (action != null) action(t);
    }

    public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2)
    {
        if (action != null) action(t1, t2);
    }

    public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
    {
        if (action != null) action(t1, t2, t3);
    }

    public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
    {
        if (action != null) action(t1, t2, t3, t4);
    }

    public static void Invoke(this MonoBehaviour mono, Action action, float delay)
    {
        mono.StartCoroutine(InvokeCoroutine(action, delay));
    }

    private static IEnumerator InvokeCoroutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public static void Invoke<T>(this MonoBehaviour mono, Action<T> action, float delay, T t)
    {
        mono.StartCoroutine(InvokeCoroutine(action, delay, t));
    }

    private static IEnumerator InvokeCoroutine<T>(Action<T> action, float delay, T t)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke(t);
    }

    public static void Invoke<T1, T2>(this MonoBehaviour mono, Action<T1, T2> action, float delay, T1 t1, T2 t2)
    {
        mono.StartCoroutine(InvokeCoroutine(action, delay, t1, t2));
    }

    private static IEnumerator InvokeCoroutine<T1, T2>(Action<T1, T2> action, float delay, T1 t1, T2 t2)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke(t1, t2);
    }

    #endregion

    #region Math

    //각도로 좌우 방향구하기
    public static Vector3 GetDirectionByAngle_XZ(float angleInDegrees, Vector3 dir)
    {
        angleInDegrees += dir.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    //각도로 높이 방향구하기
    public static Vector3 GetDirectionByAngle_YZ(float angleInDegrees, Vector3 dir)
    {
        Vector3 axis = Vector3.Cross(dir, Vector3.up);

        // handle case where start is colinear with up
        if (axis == Vector3.zero) axis = Vector3.right;

        return Quaternion.AngleAxis(angleInDegrees, axis) * dir;        
    }

    //시야각안에 들어왔을 경우
    public static bool IsInsideViewAngle(Transform target, Transform point, float viewAngle)
    {
        if (target == null || point == null)
        {
            Debug.Log("<color=red>ERROR: IsInsideViewAngle target or point is NULL</color>");
            return false;
        }

        Vector3 dirToTarget = (target.position - point.position).normalized;
        if (Vector3.Dot(point.forward, dirToTarget) > Mathf.Cos((viewAngle / 2) * Mathf.Deg2Rad))
        {
            return true;
        }
        else
            return false;
    }

    //앞 뒤 확인 me:기준 
    public static bool IsFront(Vector3 dir, Vector3 me, Vector3 other)
    {
        bool isFront;

        Vector3 directionToTarget = me - other;
        float angel = Vector3.Angle(dir, directionToTarget);
        if (Mathf.Abs(angel) < 90)
            isFront = false;
        else
            isFront = true;

        return isFront;
    }

    //오른쪽 왼쪽 판별
    public static bool IsRight(Vector3 dir, Vector3 me, Vector3 other)
    {
        bool isRight = false;

        Vector3 toOther = (other - me).normalized;

        if (Vector3.Cross(dir, toOther).y < 0)
            isRight = false;
        else
            isRight = true;

        return isRight;
    }


    //360 기준 각도
    public static float Calculate360Angle(Vector3 v1, Vector3 v2)
    {
        float angle = Vector3.SignedAngle(v1, v2, Vector3.down); //Returns the angle between -180 and 180.
        if (angle < 0)
        {
            angle = 360 - angle * -1;
        }

        return angle;
    }


    /// <summary>
    /// Calculates the intersection of two given lines
    /// </summary>
    /// <param name="intersection">returned intersection</param>
    /// <param name="linePoint1">start location of the line 1</param>
    /// <param name="lineDirection1">direction of line 1</param>
    /// <param name="linePoint2">start location of the line 2</param>
    /// <param name="lineDirection2">direction of line2</param>
    /// <returns>true: lines intersect, false: lines do not intersect</returns>
    public static bool LineLineIntersection(out Vector3 intersection,
        Vector3 linePoint1, Vector3 lineDirection1,
        Vector3 linePoint2, Vector3 lineDirection2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineDirection1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

#endregion

#region Animation

    class CoMonoBehaviour : MonoBehaviour { }

    public static Coroutine StartCoroutine(this GameObject go, IEnumerator routine)
    {
        if (null == go || !go.activeSelf) return null;

        CoMonoBehaviour mono = go.GetComponent<CoMonoBehaviour>();
        if (null == mono)
            mono = go.AddComponent<CoMonoBehaviour>();

        return mono.StartCoroutine(routine);
    }

    public static Coroutine PlayCallback(this Animation anim, string clip, Action callback)
    {
        return anim.gameObject.StartCoroutine(CoPlayCallback(anim, clip, 1f, callback));
    }

    public static IEnumerator CoPlayCallback(Animation anim, string aniName, float speed, Action callback)
    {
        if (anim.isPlaying) anim.Stop();
        if (anim.HasClip(aniName))
        {
            anim.Play(aniName);
            anim[aniName].speed = speed;
            while (anim.isPlaying) yield return null;
        }

        if(callback != null)
            callback();

        if (anim.HasClip(aniName))
        {
            anim[aniName].speed = 1f;
        }
        yield break;
    }

    public static bool HasClip(this Animation anim, string clip)
    {
        return (anim.GetClip(clip) != null);
    }

    public static void Reset(this Animation anim)
    {
        if (anim == null)
            return;

        anim.Rewind();
        anim.Play();
        anim.Sample();
        anim.Stop();
    }

    public static float SafePlay(this Animation anim, string ani_name)
    {
        if (anim == null)
        {
            return 0.0f;
        }

        string targetClipName = null;
        if (anim.clip != null)
        {
            targetClipName = anim.clip.name;
        }

        var _enum = anim.GetEnumerator();
        while (_enum.MoveNext())
        {
            AnimationState state = _enum.Current as AnimationState;
            if (state.name == ani_name)
            {
                targetClipName = ani_name;
                break;
            }
        }

        if (string.IsNullOrEmpty(targetClipName) == true)
        {
            Debug.Log("<color==red>No Existing Ani Name >> " + ani_name + "</color>");
            return 0.0f;
        }

#if UNITY_EDITOR
        if (targetClipName != ani_name)
        {
            Debug.Log("<color==red>Clip Name Changed " + ani_name + " To " + targetClipName + "</color>");
        }
#endif

        ResetPlay(anim, targetClipName);

        return anim[targetClipName].length;
    }

    public static void ResetPlay(this Animation anim, string aniName)
    {
        if (anim == null)
            return;

        anim.Rewind(aniName);
        anim.Play(aniName);
        anim.Sample();
        anim.Stop();

        anim.Play(aniName);

        //------------------------------------------------------------
        // ...unity update bug??
        anim.cullingType = AnimationCullingType.BasedOnRenderers;
        anim.cullingType = AnimationCullingType.AlwaysAnimate;
        //------------------------------------------------------------
    }

    #endregion

    #region Camera

    public static bool IsObjectVisible(this UnityEngine.Camera @this, Renderer renderer)
    {
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(@this), renderer.bounds);
    }

    public static bool IsObjectVisible(this UnityEngine.Camera @this, Vector3 pos)
    {
        Plane[] _planes = GeometryUtility.CalculateFrustumPlanes(@this);
        for (int i = 0; i < _planes.Length; i++)
        {
            if (_planes[i].GetSide(pos) == false)
                return false;
        }

        return true;
    }

    #endregion
}
