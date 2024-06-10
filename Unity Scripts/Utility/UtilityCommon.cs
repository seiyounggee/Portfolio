using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Linq;

public static class UtilityCommon
{
    public static T[] ShuffleArray<T>(T[] array)
    {
        System.Random rnd = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            int k = rnd.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }

        return array;
    }

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

    public static void ChangeAllLayers(Transform trans, string name)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in trans)
        {
            if(child != null)
                ChangeAllLayers(child, name);
        }
    }

    public static string GetTimeString(double totalSeconds)
    {
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

    public static string GetCurrentDateTime(string languageType = "en-US")
    {
        System.DateTime dt = System.DateTime.Now;
        System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(languageType);
        string timeString = dt.ToString(provider: culture);
        return timeString;
    }

    #region Mono

    public static void SafeSetActive(this GameObject gameObject, bool isActive)
    {
        if (gameObject == null) 
            return;

        gameObject.SetActive(isActive);
    }

    public static void SafeSetActive<T>(this T component, bool isOn) where T: Component
    {
        var go = component.gameObject;
        if (go != null)
        {
            go.gameObject.SetActive(isOn);
        }
    }

    public static void SafeSetActive(this Button btn, bool isActive)
    {
        if (btn == null)
            return;

        btn.gameObject.SetActive(isActive);
    }

    public static bool SafeIsActive(this GameObject gameObject)
    {
        if (gameObject == null) 
            return false;

        return gameObject.activeSelf;
    }

    public static bool SafeIsActive<T>(this T component) where T : Component
    {
        if (component == null) 
            return false;

        return component.gameObject.SafeIsActive();
    }

    public static void SafeSetButton(this Button btn, UnityAction func)
    {
        if (btn != null && func != null)
        {
            btn.onClick.AddListener(func);
        }
    }

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

    public delegate void VoidToggleDelegate(Toggle toggle);
    public static void SafeSetToggle(this Toggle toggle, VoidToggleDelegate callback)
    {
        UnityAction<bool> func;
        if (toggle != null && callback != null)
        {
            func = (isTrue) => callback(toggle);
            toggle.onValueChanged.AddListener(func);
        }
    }

    public static void SafeSetText(this Text txt, string msg)
    {
        if (txt != null)
        {
            txt.text = msg;
        }
    }

    public static void SafeSetText(this TextMeshProUGUI txt, string msg)
    {
        if (txt != null)
        {
            txt.text = msg;
        }
    }

    public static void SafeSetTexture(this RawImage image, Texture2D texture )
    {
        if (image != null)
        {
            image.texture = texture;
        }
    }

    public static void SafeSetSprite(this Image image, Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
        }
    }

    public static void SafeColor(this Image img, Color color)
    {
        if (img != null)
        {
            img.color = color;
        }
    }

    public static void SafeColor(this Image img, Color32 color)
    {
        if (img != null)
        {
            img.color = color;
        }
    }

    public enum DebugColor { None, White, Black, Cyan, Red, Yellow , Blue}

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void ColorLog(string msg, DebugColor color)
    {
        if (string.IsNullOrEmpty(msg) == false)
        {
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
                default:
                    Debug.Log(msg);
                    break;
            }
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

    #endregion

    #region Math & Physics

    //각도로 방향구하기
    public static Vector3 GetDirectionByAngle(float angleInDegrees, Transform point)
    {
        if (point == null)
        {
            Debug.Log("<color=red>ERROR: GetDirectionByAngle point is NULL</color>");
            return Vector3.zero;
        }

        angleInDegrees += point.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
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

    // 정사면체
    public static Vector3[] CalculatePyramidVertices(float size)
    {
        Vector3[] vertices = new Vector3[5];

        // 정사면체의 바닥의 네 꼭지점 계산
        float halfBaseSideLength = size / 2.0f;

        Vector3 bottomLeft = new Vector3(-halfBaseSideLength, 0.0f, -halfBaseSideLength);
        Vector3 bottomRight = new Vector3(halfBaseSideLength, 0.0f, -halfBaseSideLength);
        Vector3 topLeft = new Vector3(-halfBaseSideLength, 0.0f, halfBaseSideLength);
        Vector3 topRight = new Vector3(halfBaseSideLength, 0.0f, halfBaseSideLength);

        // 정사면체의 정점(피라미드의 정상) 계산
        double d = Mathf.Pow(size, 2) - Mathf.Pow(halfBaseSideLength, 2);
        float height = (float)Math.Sqrt(d);
        Vector3 apex = new Vector3(0.0f, height, 0.0f);

        //중앙(?) 다시 설정
        bottomLeft.y -= height / 2f;
        bottomRight.y -= height / 2f;
        topLeft.y -= height / 2f;
        topRight.y -= height / 2f;
        apex.y -= height / 2f;


        // 리스트에 꼭지점 추가
        vertices[0] = bottomLeft;
        vertices[1] = bottomRight;
        vertices[2] = topLeft;
        vertices[3] = topRight;
        vertices[4] = apex;

        return vertices;
    }

    // 정육면체
    public static Vector3[] CalculateCubeVertices(float size)
    {
        // 정육면체의 꼭지점 배열
        Vector3[] vertices = new Vector3[8];

        // 정육면체의 꼭지점 계산
        float halfSize = size / 2f;
        vertices[0] = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[1] = new Vector3(halfSize, -halfSize, -halfSize);
        vertices[2] = new Vector3(halfSize, -halfSize, halfSize);
        vertices[3] = new Vector3(-halfSize, -halfSize, halfSize);
        vertices[4] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[5] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[6] = new Vector3(halfSize, halfSize, halfSize);
        vertices[7] = new Vector3(-halfSize, halfSize, halfSize);

        return vertices;
    }

    // 정팔면체
    public static Vector3[] CalculateOctahedronVertices(float size)
    {
        // 정팔면체의 꼭지점 배열
        Vector3[] vertices = new Vector3[6];

        // 정팔면체의 꼭지점 계산
        float halfSize = size / 2f;
        vertices[0] = new Vector3(0f, -size, 0f);
        vertices[1] = new Vector3(size, 0f, 0f);
        vertices[2] = new Vector3(0f, 0f, -size);
        vertices[3] = new Vector3(-size, 0f, 0f);
        vertices[4] = new Vector3(0f, 0f, size);
        vertices[5] = new Vector3(0f, size, 0f);

        return vertices;
    }

    // 정십이면체
    public static Vector3[] CalculateIcosahedronVertices(float size)
    {
        // 정십이면체의 꼭지점 배열
        Vector3[] vertices = new Vector3[12];

        // 정십이면체의 꼭지점 계산
        float phi = (1 + Mathf.Sqrt(5)) / 2; // 황금비

        vertices[0] = new Vector3(0, 1, phi);
        vertices[1] = new Vector3(0, 1, -phi);
        vertices[2] = new Vector3(0, -1, phi);
        vertices[3] = new Vector3(0, -1, -phi);
        vertices[4] = new Vector3(1, phi, 0);
        vertices[5] = new Vector3(1, -phi, 0);
        vertices[6] = new Vector3(-1, phi, 0);
        vertices[7] = new Vector3(-1, -phi, 0);
        vertices[8] = new Vector3(phi, 0, 1);
        vertices[9] = new Vector3(phi, 0, -1);
        vertices[10] = new Vector3(-phi, 0, 1);
        vertices[11] = new Vector3(-phi, 0, -1);

        // 크기 조절
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= size;
        }

        return vertices;
    }

    // 정이십면체
    public static Vector3[] CalculateDodecahedronVertices(float size)
    {
        // 정이십면체의 꼭지점 배열
        Vector3[] vertices = new Vector3[20];

        // 정이십면체의 꼭지점 계산
        float phi = (1 + Mathf.Sqrt(5)) / 2; // 황금비
        float a = size / Mathf.Sqrt(3);
        float b = size * Mathf.Sqrt((3 + Mathf.Sqrt(5)) / 6);

        vertices[0] = new Vector3(0, a, -b);
        vertices[1] = new Vector3(0, a, b);
        vertices[2] = new Vector3(0, -a, -b);
        vertices[3] = new Vector3(0, -a, b);
        vertices[4] = new Vector3(-b, 0, -a);
        vertices[5] = new Vector3(-b, 0, a);
        vertices[6] = new Vector3(b, 0, -a);
        vertices[7] = new Vector3(b, 0, a);
        vertices[8] = new Vector3(-a, -b, 0);
        vertices[9] = new Vector3(-a, b, 0);
        vertices[10] = new Vector3(a, -b, 0);
        vertices[11] = new Vector3(a, b, 0);
        vertices[12] = new Vector3(-phi * a, -phi * a, -phi * a);
        vertices[13] = new Vector3(-phi * a, -phi * a, phi * a);
        vertices[14] = new Vector3(-phi * a, phi * a, -phi * a);
        vertices[15] = new Vector3(-phi * a, phi * a, phi * a);
        vertices[16] = new Vector3(phi * a, -phi * a, -phi * a);
        vertices[17] = new Vector3(phi * a, -phi * a, phi * a);
        vertices[18] = new Vector3(phi * a, phi * a, -phi * a);
        vertices[19] = new Vector3(phi * a, phi * a, phi * a);

        return vertices;
    }

    public static Vector3[] CalculateTruncatedCuboctahedronVertices(float size)
    {
        Vector3[] vertices;

        float a = size / Mathf.Sqrt(2.0f);
        float b = a / 2.0f;

        // 큐보옥타헤드론의 꼭지점 계산
        vertices = new Vector3[]
        {
            new Vector3(a, 0, 0),
            new Vector3(-a, 0, 0),
            new Vector3(0, a, 0),
            new Vector3(0, -a, 0),
            new Vector3(0, 0, a),
            new Vector3(0, 0, -a),

            // 정사각형 면의 꼭지점 추가
            new Vector3(b, b, b),
            new Vector3(b, b, -b),
            new Vector3(b, -b, b),
            new Vector3(b, -b, -b),
            new Vector3(-b, b, b),
            new Vector3(-b, b, -b),
            new Vector3(-b, -b, b),
            new Vector3(-b, -b, -b)
        };

        return vertices;
    }

    public static Vector3[] CalculateSphereVertices(float size)
    {
        List<Vector3> vertices = new List<Vector3>();

        var sphere_radius = size;
        int latitudeSegments = 30;
        int longitudeSegments = latitudeSegments / 2;

        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            float normalizedLatitude = lat / (float)latitudeSegments;
            float theta = normalizedLatitude * Mathf.PI;

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                float normalizedLongitude = lon / (float)longitudeSegments;
                float phi = normalizedLongitude * 2f * Mathf.PI;

                float x = Mathf.Sin(theta) * Mathf.Cos(phi) * sphere_radius;
                float y = Mathf.Cos(theta) * sphere_radius;
                float z = Mathf.Sin(theta) * Mathf.Sin(phi) * sphere_radius;

                vertices.Add(new Vector3(x, y, z));
            }
        }

        return vertices.ToArray();
    }

    public static List<Vector3> ReturnSubDevidedVerticies(Vector3[] vertices, int devidedCount)
    {
        if (devidedCount <= 0)
            return vertices.ToList();

        List<Vector3> subdividedVertices = new List<Vector3>();

        var minDist = GetMinDist(vertices);
        var interval = minDist / (float)devidedCount;

        // Calculate subdivided vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = i; j < vertices.Length; j++)
            {
                if (i == j)
                    continue;

                Vector3 currentPoint = vertices[i];
                Vector3 nextPoint = vertices[j];

                int subdivisions = Mathf.CeilToInt(Vector3.Distance(currentPoint, nextPoint) / interval);

                // Calculate and add subdivided vertices
                for (int k = 0; k <= subdivisions; k++)
                {
                    float t = k / (float)subdivisions;
                    Vector3 newVertex = Vector3.Lerp(currentPoint, nextPoint, t);
                    subdividedVertices.Add(newVertex);
                }
            }
        }

        return subdividedVertices;
    }

    private static float GetMinDist(Vector3[] vertices)
    {
        float minDist = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = i; j < vertices.Length; j++)
            {
                if (i == j)
                    continue;

                var dist = Vector3.Distance(vertices[i], vertices[j]);
                if (dist < minDist)
                    minDist = dist;
            }
        }

        return minDist;
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

        anim.cullingType = AnimationCullingType.BasedOnRenderers;
        anim.cullingType = AnimationCullingType.AlwaysAnimate;
    }

    #endregion


}
