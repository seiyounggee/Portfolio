using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public class MapWorkspace : EditorWindow
{
    private GameObject selectedTilePrefab;

    public int counter = 20;

    private GameObject[] grassObjects;
    private int grassArraySize = 1;
    private int grassScale = 2;
    private int grassDensity = 2;

    [MenuItem(CommonDefine.ProjectName + "/Custom Editor/Map Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MapWorkspace));
    }

    private void OnEnable()
    {
        grassArraySize = 1;
        grassObjects = new GameObject[grassArraySize];
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Tile Prefab:", EditorStyles.boldLabel);

        selectedTilePrefab = EditorGUILayout.ObjectField(selectedTilePrefab, typeof(GameObject), true) as GameObject;
        counter = EditorGUILayout.IntField("Count Value:", counter);
        var scene = SceneManager.GetActiveScene();

        GUILayout.Space(30);


        GUILayout.Label("Floor Creater", EditorStyles.boldLabel);
        if (GUILayout.Button("Create Floor"))
        {
            if (scene.name != "Map_Workspace")
            {
                Debug.Log("<color=red>FORBIDDEN!! Go to Map_Workspace scene to Create!</color>");
                return;
            }

            bool isSuccess = CreateFloor();

            if (isSuccess == false)
            {
                Debug.Log("<color=red>FAILED to create floor</color>");
            }
        }

        GUILayout.Space(30);


        GUILayout.Label("Grass Creater", EditorStyles.boldLabel);

        int newSize = EditorGUILayout.IntField("Number of Grass Prefabs", grassArraySize);
        if (newSize != grassArraySize)
        {
            grassArraySize = newSize;
            System.Array.Resize(ref grassObjects, grassArraySize);
        }
        for (int i = 0; i < grassObjects.Length; i++)
        {
            grassObjects[i] = (GameObject)EditorGUILayout.ObjectField($"Grass Prefab {i + 1}", grassObjects[i], typeof(GameObject), true);
        }


        grassScale = EditorGUILayout.IntField("Scale", grassScale);
        grassDensity = EditorGUILayout.IntSlider("Density", grassDensity, 1, 5);

        if (GUILayout.Button("Create Grass"))
        {
            if (scene.name != "Map_Workspace")
            {
                Debug.Log("<color=red>FORBIDDEN!! Go to Map_Workspace scene to Create!</color>");
                return;
            }

            bool isSuccess = CreateGrass();

            if (isSuccess == false)
            {
                Debug.Log("<color=red>FAILED to create grass</color>");
            }
        }
    }

    private bool CreateFloor()
    {
        if (selectedTilePrefab != null && counter > 0)
        {
            GameObject floor = new GameObject();
            floor.name = "floor base_" + selectedTilePrefab.name;
            Vector3 tileSize = selectedTilePrefab.GetComponent<Renderer>().bounds.size; // 타일의 크기
            Vector3 mapCenter = new Vector3((counter - 1) * tileSize.x / 2f, 0, (counter - 1) * tileSize.z / 2f); // 맵의 중심 위치

            int cnt = 0;
            for (int i = 0; i < counter; i++)
            {
                for (int j = 0; j < counter; j++)
                {
                    var tile = Instantiate(selectedTilePrefab);
                    tile.name = "Tile_" + cnt;
                    Vector3 tilePosition = new Vector3(i * tileSize.x, 0, j * tileSize.z); // 타일의 위치
                    tile.transform.position = tilePosition - mapCenter; // 타일의 위치를 맵의 중심으로 이동
                    tile.transform.SetParent(floor.transform);


                    ++cnt;
                }
            }
            //floor.transform.position = Vector3.up * 0.6f;

            Debug.Log("<color=cyan>Successfully Created Floor!!!!</color>");
            return true;
        }

        return false;
    }

    private bool CreateGrass()
    {
        if (selectedTilePrefab != null && counter > 0)
        {
            GameObject grass = new GameObject();
            grass.name = "grass base_" + selectedTilePrefab.name;
            Vector3 tileSize = selectedTilePrefab.GetComponent<Renderer>().bounds.size; // 타일의 크기
            Vector3 mapCenter = new Vector3((counter - 1) * tileSize.x / 2f, 0, (counter - 1) * tileSize.z / 2f); // 맵의 중심 위치

            int cnt = 0;
            for (int i = 0; i < counter; i++)
            {
                for (int j = 0; j < counter; j++)
                {
                    for (int d = 0; d < grassDensity; d++)
                    {
                        var randomGrass = UnityEngine.Random.Range(0, grassObjects.Length);
                        var grassObj = Instantiate(grassObjects[randomGrass]);
                        grassObj.name = "Grass_" + cnt;

                        // 위치에 약간의 랜덤 변동을 주어 겹치지 않게 함
                        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-tileSize.x / 2, tileSize.x / 2),
                                                          0,
                                                          UnityEngine.Random.Range(-tileSize.z / 2, tileSize.z / 2));
                        Vector3 tilePosition = new Vector3(i * tileSize.x, 0, j * tileSize.z) + randomOffset - mapCenter;
                        grassObj.transform.position = tilePosition;
                        grassObj.transform.localScale = grassScale * Vector3.one;
                        grassObj.transform.SetParent(grass.transform);

                        ++cnt;
                    }
                }
            }


            Debug.Log("<color=cyan>Successfully Created Grass!!!!</color>");
            return true;
        }

        return false;
    }
}
#endif