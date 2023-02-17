using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class UIEditor : EditorWindow
{
    private Font font;

    [MenuItem(CommonDefine.ProjectName + "/Custom Editor/UI Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(UIEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Font");
        GUILayout.BeginHorizontal();
        font = (Font)EditorGUILayout.ObjectField(font, typeof(Font), false);
        if (GUILayout.Button("Apply"))
        {
            if (font != null)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    AssignFontToSelection(go);
                    // This resets the element so it updates the colors in the editor
                    go.SetActive(false);
                    go.SetActive(true);
                }

                Debug.Log("<color=cyan>Font Changed... to: " + font.name + "</color>");
            }
            else
            {
                Debug.Log("<color=red>ERROR...! No font selected!</color>");
            }
        }
        GUILayout.EndHorizontal();
    }

    private void AssignFontToSelection(GameObject gameObject)
    {
        if (gameObject.GetComponent<UnityEngine.UI.Text>())
        {
            UnityEngine.UI.Text text = gameObject.GetComponent<UnityEngine.UI.Text>();
            if (font)
            {
                Undo.RecordObject(text, "Change Font");
                text.font = font;
                EditorUtility.SetDirty(text);
            }
        }
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            AssignFontToSelection(gameObject.transform.GetChild(i).gameObject);
        }
    }
}
#endif