using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR

public class GameObjectEditor : EditorWindow
{
    [MenuItem(CommonDefine.ProjectName + "/Custom Editor/GameObject Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GameObjectEditor));
    }

    private List<Collider> selectedColliderLists = new List<Collider>();

    private void OnEnable()
    {
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Clear All Child Collider");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Select"))
        {
            foreach (GameObject go in Selection.gameObjects)
                SelectAllGameObject(go);
        }

        if (GUILayout.Button("Remove"))
        {
            if (selectedColliderLists != null)
            {
                foreach (Collider co in selectedColliderLists)
                {
                    if (co != null && co.gameObject != null)
                        RemoveSelectedCollider(co.gameObject);
                }
            }

            if (selectedColliderLists.Count == 0)
                Debug.Log("<color=red>ERROR...! No collider selected!</color>");
        }

        GUILayout.EndHorizontal();
        GUIUtility.ExitGUI();
    }

    private void RemoveSelectedCollider(GameObject gameObject)
    {
        Collider col = gameObject.GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log("<color=cyan>Collider removed!... : " + col.name + "</color>");
            DestroyImmediate(col);
        }
    }

    private void SelectAllGameObject(GameObject gameObj)
    {
        Collider col = gameObj.GetComponent<Collider>();
        if (col != null)
        {
            selectedColliderLists.Add(col);
        }

        for (int i = 0; i < gameObj.transform.childCount; i++)
        {
            SelectAllGameObject(gameObj.transform.GetChild(i).gameObject);
        }
    }
}
#endif
