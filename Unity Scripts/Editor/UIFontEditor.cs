using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class UIFontEditor : EditorWindow
{
    private TMP_FontAsset selectedFont;
    private bool isChangeColor = false;
    private Color textColor = Color.white;

    [MenuItem(CommonDefine.ProjectName + "/Custom Editor/Font Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(UIFontEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a TMP Font Asset", EditorStyles.boldLabel);
        // ��Ʈ ���� â�� ǥ���մϴ�.
        selectedFont = EditorGUILayout.ObjectField("Font", selectedFont, typeof(TMP_FontAsset), false) as TMP_FontAsset;
        isChangeColor = EditorGUILayout.Toggle("Change Text Color", isChangeColor);
        textColor = EditorGUILayout.ColorField("Text Color", textColor);

        // ��ư�� Ŭ���Ǹ� ��Ʈ ���� ����
        if (GUILayout.Button("Change Font"))
        {
            ChangeTMPFont();
        }
    }

    private void ChangeTMPFont()
    {
        if (selectedFont == null)
        {
            Debug.LogError("No font selected!");
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("No GameObject selected!");
            return;
        }

        var textComponents = selectedObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        Undo.RecordObjects(textComponents, "Change TMP Font");  // ���� ��ü�� ���� Undo ���
        foreach (var textComp in textComponents)
        {
            //Undo.RecordObject(textComp, "Change TMP Font");
            textComp.font = selectedFont;

            if(isChangeColor)
                textComp.color = textColor;

            EditorUtility.SetDirty(textComp);
        }

        Debug.Log("Font changed for all TextMeshPro components.");
    }
}
