using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[CustomEditor(typeof(MapObjectManager))]
public class MapObjectManagerEditor : Editor
{
    MapObjectManager mapObjectManager;

    private void OnEnable()
    {
        mapObjectManager = target as MapObjectManager;
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

            mapObjectManager.chargePadList = new List<MapObject_ChargePad>();

            var arr = FindObjectsOfType<MapObject_ChargePad>();
            foreach (var i in arr)
            {
                mapObjectManager.chargePadList.Add(i);
            }

            mapObjectManager.chargeZoneList = new List<MapObject_ChargeZone>();

            var arr2 = FindObjectsOfType<MapObject_ChargeZone>();
            foreach (var i in arr2)
            {
                mapObjectManager.chargeZoneList.Add(i);
            }

            mapObjectManager.containerBoxList = new List<MapObject_ContainerBox>();

            var arr3 = FindObjectsOfType<MapObject_ContainerBox>();
            foreach (var i in arr3)
            {
                mapObjectManager.containerBoxList.Add(i);
            }

            mapObjectManager.groundList = new List<MapObject_Ground>();

            var arr4 = FindObjectsOfType<MapObject_Ground>();
            foreach (var i in arr4)
            {
                mapObjectManager.groundList.Add(i);
            }

            var fb = FindObjectOfType<MapObject_FinishBoard>();
            if (fb != null)
                mapObjectManager.finishBoard = fb;


            Debug.Log("<color=cyan>Map Objects Are Set!!</color>");
            Debug.Log("Charge Pad List Count: " + mapObjectManager.chargePadList.Count);
            Debug.Log("Charge Zone List Count: " + mapObjectManager.chargeZoneList.Count);
            Debug.Log("ContainerBox List Count: " + mapObjectManager.containerBoxList.Count);
            Debug.Log("Ground List Count: " + mapObjectManager.groundList.Count);

            SaveScene();
        }
    }

    private void SaveScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Saved Scene");
    }
}
#endif
