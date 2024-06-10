using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileWorkspace : MonoBehaviour
{
    // ���� ���� ��ο� �� ���� ��θ� �����ϼ���
    public string sourceFilePath = "Assets/Resources/UnityPrefabs/Game/Skin/CharacterSkin_Icon/icon_character_0.png";
    public string newFilePathFormat = "Assets/Resources/UnityPrefabs/Game/Skin/CharacterSkin_Icon/icon_character_{0}.png";

    public int number = 50;

    void Start()
    {
        for (int i = 0; i < number; i++)
        {
            CopyAndRenameFile(sourceFilePath, string.Format(newFilePathFormat, i));
        }
    }

    void CopyAndRenameFile(string sourcePath, string newPath)
    {
        // �� ��ġ�� ������ �̹� �����ϴ��� Ȯ���մϴ�.
        if (File.Exists(newPath))
        {
            Debug.Log("File already exists at the new location.");
            return; // �̹� ������ ������ �۾��� �ߴ��մϴ�.
        }

        try
        {
            // ������ �� ��ġ�� �� �̸����� �����մϴ�.
            File.Copy(sourcePath, newPath);
            Debug.Log("File copied and renamed successfully!");
        }
        catch (IOException copyError)
        {
            Debug.LogError("Failed to copy and rename file: " + copyError.Message);
        }
    }
}
