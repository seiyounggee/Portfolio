using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
public class UIEditor : EditorWindow
{
    private Font ugui_font;
    private TMP_FontAsset textMeshPro_font;

    [MenuItem(CommonDefine.ProjectName + "/Custom Editor/UI Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(UIEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("UGUI Font");
        GUILayout.BeginHorizontal();
        ugui_font = (Font)EditorGUILayout.ObjectField(ugui_font, typeof(Font), false);
        if (GUILayout.Button("Apply"))
        {
            if (ugui_font != null)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    AssignFontToSelection_UGUI(go);
                    // This resets the element so it updates the colors in the editor
                    go.SetActive(false);
                    go.SetActive(true);
                }

                Debug.Log("<color=cyan>Font Changed... to: " + ugui_font.name + "</color>");
            }
            else
            {
                Debug.Log("<color=red>ERROR...! No font selected!</color>");
            }
        }
        GUILayout.EndHorizontal();




        GUILayout.Label("TextMeshPro Font");
        GUILayout.BeginHorizontal();
        textMeshPro_font = (TMP_FontAsset)EditorGUILayout.ObjectField(textMeshPro_font, typeof(TMP_FontAsset), false);
        if (GUILayout.Button("Apply"))
        {
            if (textMeshPro_font != null)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    AssignFontToSelection_TextMeshPro(go);
                    // This resets the element so it updates the colors in the editor

                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                        go.SetActive(true);
                    }
                }

                Debug.Log("<color=cyan>Font Changed... to: " + textMeshPro_font.name + "</color>");
            }
            else
            {
                Debug.Log("<color=red>ERROR...! No font selected!</color>");
            }
        }
        GUILayout.EndHorizontal();
    }

    private void AssignFontToSelection_UGUI(GameObject gameObject)
    {
        if (gameObject.GetComponent<UnityEngine.UI.Text>())
        {
            UnityEngine.UI.Text text = gameObject.GetComponent<UnityEngine.UI.Text>();
            if (ugui_font)
            {
                Undo.RecordObject(text, "Change Font");
                text.font = ugui_font;
                EditorUtility.SetDirty(text);
            }
        }
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            AssignFontToSelection_UGUI(gameObject.transform.GetChild(i).gameObject);
        }
    }

    private void AssignFontToSelection_TextMeshPro(GameObject gameObject)
    {
        if (gameObject.GetComponent<UnityEngine.UI.Text>())
        {
            var text = gameObject.GetComponent<TextMeshProUGUI>();
            if (textMeshPro_font)
            {
                Undo.RecordObject(text, "Change Font");
                text.font = textMeshPro_font;
                text.UpdateFontAsset();
                EditorUtility.SetDirty(text);
            }
        }
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            AssignFontToSelection_TextMeshPro(gameObject.transform.GetChild(i).gameObject);
        }
    }
}
#endif