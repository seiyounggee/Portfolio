using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Burst.Intrinsics;

#if UNITY_EDITOR
[CustomEditor(typeof(MapObjectManager))]
public class MapObjectManagerEditor : Editor
{
    MapObjectManager mapObjectManager;

    private void OnEnable()
    {
        mapObjectManager = target as MapObjectManager;
    }

    [MenuItem(CommonDefine.ProjectName + "/Custom Editor/Setup Empty Map Scene")]
    public static void SetupMapScene()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CommonDefine.InGameScene
            || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CommonDefine.OutGameScene)
        {
            Debug.Log("<color=red>Invalid Scene!! Waypoint Generator are set in seperate maps not on ingame or outgame scene</color>");
            return;
        }

        var wpg = FindObjectOfType<WayPointSystem.WaypointsGroup>();
        if (wpg == null)
            WayPointSystem.WaypointsGroupEditor.CreateWaypoint();

        var wmsg = FindObjectOfType<WayPointSystem.WaypointMeshGenerator>();
        if (wmsg == null)
            WayPointSystem.WaypointMeshGeneratorEditor.CreateMeshGenerator();

        var mom = FindObjectOfType<MapObjectManager>();
        if (mom == null)
        {
            GameObject go = new GameObject("MapObject Manager");
            go.AddComponent<MapObjectManager>();
        }

        var light = FindObjectOfType<Light>();
        if (light == null)
        {
            GameObject go = new GameObject("Direction Light");
            var li = go.AddComponent<Light>();
            li.type = LightType.Directional;
            li.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            li.transform.position = Vector3.zero;
            li.shadows = LightShadows.Soft;
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Saved Scene");
    }

    private void OnSceneGUI()
    { 
    
    }

    override public void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (mapObjectManager == null)
            return;

        if (GUILayout.Button("Find And Set All Objects"))
        {
            FindAndSetAllObjects();
        }
    }

    private void SaveScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Saved Scene");
    }

    private void FindAndSetAllObjects()
    {
        if (mapObjectManager == null)
            return;

        SetChargePad();

        SetTimingPad();

        SetChargeZone();

        SetContainerBox();

        SetGround();

        SetObstacle();

        SetCatapult();

        SetPodiumBoard();

        Debug.Log("<color=cyan>Map Objects Are Set!!</color>");
        SaveScene();
    }

    private void SetChargePad()
    {
        if (mapObjectManager.chargePadBase == null)
        {
            mapObjectManager.chargePadBase = new GameObject();
        }

        mapObjectManager.chargePadBase.name = "[MapObject] ChargePadBase";
        mapObjectManager.chargePadList = new List<MapObject_ChargePad>();

        var arr = FindObjectsOfType<MapObject_ChargePad>();
        int cnt = 0;
        foreach (var i in arr)
        {
            if (mapObjectManager.chargePadList.Contains(i) == false)
            {
                mapObjectManager.chargePadList.Add(i);
            }
            i.transform.parent = mapObjectManager.chargePadBase.transform;

            i.transform.name = "ChargePad_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();
            if (child != null && child.Length > 0)
            { 
                foreach(var j in child)
                    j.transform.name = "ChargePad_" + cnt;
            }

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }

            ++cnt;
        }

        if (arr == null || arr.Length == 0)
        {
            DestroyImmediate(mapObjectManager.chargePadBase);
            mapObjectManager.chargePadBase = null;
        }

        Debug.Log("Charge Pad List Count: " + mapObjectManager.chargePadList.Count);
    }

    private void SetTimingPad()
    {
        if (mapObjectManager.timingPadBase == null)
        {
            mapObjectManager.timingPadBase = new GameObject();
        }

        mapObjectManager.timingPadBase.name = "[MapObject] TimingPadBase";
        mapObjectManager.timingPadList = new List<MapObject_TimingPad>();

        var arr = FindObjectsOfType<MapObject_TimingPad>();
        int cnt = 0;
        foreach (var i in arr)
        {
            if (mapObjectManager.timingPadList.Contains(i) == false)
            {
                mapObjectManager.timingPadList.Add(i);
            }
            i.transform.parent = mapObjectManager.timingPadBase.transform;

            i.transform.name = "TimingPad_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();
            if (child != null && child.Length > 0)
            {
                foreach (var j in child)
                    j.transform.name = "TimingPad_" + cnt;
            }

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }

            ++cnt;
        }

        if (arr == null || arr.Length == 0)
        {
            DestroyImmediate(mapObjectManager.timingPadBase);
            mapObjectManager.timingPadBase = null;
        }

        Debug.Log("Timing Pad List Count: " + mapObjectManager.timingPadList.Count);
    }


    private void SetChargeZone()
    {
        if (mapObjectManager.charZoneBase == null)
        {
            mapObjectManager.charZoneBase = new GameObject();
        }

        mapObjectManager.charZoneBase.name = "[MapObject] ChargeZoneBase";
        mapObjectManager.chargeZoneList = new List<MapObject_ChargeZone>();

        var arr2 = FindObjectsOfType<MapObject_ChargeZone>();
        int cnt = 0;
        foreach (var i in arr2)
        {
            if (mapObjectManager.chargeZoneList.Contains(i) == false)
            {
                mapObjectManager.chargeZoneList.Add(i);
            }
            i.transform.parent = mapObjectManager.charZoneBase.transform;

            i.transform.name = "ChargeZone_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();
            if (child != null && child.Length > 0)
            {
                foreach (var j in child)
                    j.transform.name = "ChargeZone_" + cnt;
            }

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }
            ++cnt;
        }

        if (arr2 == null || arr2.Length == 0)
        {
            DestroyImmediate(mapObjectManager.charZoneBase);
            mapObjectManager.charZoneBase = null;
        }

        Debug.Log("Charge Zone List Count: " + mapObjectManager.chargeZoneList.Count);
    }

    private void SetContainerBox()
    {
        if (mapObjectManager.containerBase == null)
        {
            mapObjectManager.containerBase = new GameObject();
        }

        mapObjectManager.containerBase.name = "[MapObject] ContainerBase";
        mapObjectManager.containerBoxList = new List<MapObject_ContainerBox>();

        var arr3 = FindObjectsOfType<MapObject_ContainerBox>();
        int cnt = 0;
        foreach (var i in arr3)
        {
            if (mapObjectManager.containerBoxList.Contains(i) == false)
            {
                mapObjectManager.containerBoxList.Add(i);
            }
            i.transform.parent = mapObjectManager.containerBase.transform;

            i.transform.name = "ContainerBox_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();
            if (child != null && child.Length > 0)
            {
                foreach (var j in child)
                    j.transform.name = "ContainerBox_" + cnt;
            }

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }

            ++cnt;
        }

        if (arr3 == null || arr3.Length == 0)
        {
            DestroyImmediate(mapObjectManager.containerBase);
            mapObjectManager.containerBase = null;
        }

        Debug.Log("ContainerBox List Count: " + mapObjectManager.containerBoxList.Count);
    }

    private void SetGround()
    {
        if (mapObjectManager.groundBase == null)
        {
            mapObjectManager.groundBase = new GameObject();
        }

        mapObjectManager.groundBase.name = "[MapObject] GroundBase";
        mapObjectManager.groundList = new List<MapObject_Ground>();

        var arr4 = FindObjectsOfType<MapObject_Ground>();
        int cnt = 0;
        foreach (var i in arr4)
        {
            if (mapObjectManager.groundList.Contains(i) == false)
            {
                mapObjectManager.groundList.Add(i);
            }
            i.transform.parent = mapObjectManager.groundBase.transform;

            i.transform.name = "Ground_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();
            if (child != null && child.Length > 0)
            {
                foreach (var j in child)
                    j.transform.name = "Ground_" + cnt;
            }

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }

            ++cnt;
        }

        if (arr4 == null || arr4.Length == 0)
        {
            DestroyImmediate(mapObjectManager.groundBase);
            mapObjectManager.groundBase = null;
        }

        Debug.Log("Ground List Count: " + mapObjectManager.groundList.Count);
    }

    private void SetObstacle()
    {
        if (mapObjectManager.obstacleBase == null)
        {
            mapObjectManager.obstacleBase = new GameObject();
        }

        mapObjectManager.obstacleBase.name = "[MapObject] ObstacleBase";
        var arr5 = FindObjectsOfType<MapObject_Obstacle>();
        int cnt = 0;
        foreach (var i in arr5)
        {
            if (mapObjectManager.obstacleList.Contains(i) == false)
            {
                mapObjectManager.obstacleList.Add(i);
            }
            i.transform.parent = mapObjectManager.obstacleBase.transform;

            i.transform.name = "Obstacle_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();
            if (child != null && child.Length > 0)
            {
                foreach (var j in child)
                    j.transform.name = "Obstacle_" + cnt;
            }

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }

            ++cnt;
        }

        if (arr5 == null || arr5.Length == 0)
        {
            DestroyImmediate(mapObjectManager.obstacleBase);
            mapObjectManager.obstacleBase = null;
        }

        Debug.Log("Ostacle List Count: " + mapObjectManager.obstacleList.Count);
    }

    private void SetCatapult()
    {
        if (mapObjectManager.catapultBase == null)
        {
            mapObjectManager.catapultBase = new GameObject();
        }


        mapObjectManager.catapultBase.name = "[MapObject] CatapultBase";
        var arr = FindObjectsOfType<MapObject_Catapult>();
        int cnt = 0;
        foreach (var i in arr)
        {
            if (mapObjectManager.catapultList.Contains(i) == false)
            {
                mapObjectManager.catapultList.Add(i);
            }
            i.transform.parent = mapObjectManager.catapultBase.transform;

            i.transform.name = "Catapult_" + cnt;
            var child = i.GetComponentsInChildren<Transform>();

            var sortedChild = new List<Transform>(child.OrderBy(x => x.name));
            int index = 0;
            foreach (var j in sortedChild)
            {
                j.SetSiblingIndex(index++);
            }

            ++cnt;
        }

        if (arr == null || arr.Length == 0)
        {
            DestroyImmediate(mapObjectManager.catapultBase);
            mapObjectManager.catapultBase = null;
        }

        Debug.Log("Catapult List Count: " + mapObjectManager.catapultList.Count);
    }

    private void SetPodiumBoard()
    {
        if (mapObjectManager.podiumBase == null)
        {
            mapObjectManager.podiumBase = new GameObject();
        }

        var pd = FindObjectOfType<MapObject_Podium>();
        if (pd != null)
        {
            mapObjectManager.podium = pd;
            mapObjectManager.podiumBase.gameObject.name = "[MapObject] PodiumBase";
            mapObjectManager.podium.transform.parent = mapObjectManager.podiumBase.transform;
        }

        if (pd == null )
        {
            DestroyImmediate(mapObjectManager.podiumBase);
            mapObjectManager.podiumBase = null;
        }
    }
}
#endif
