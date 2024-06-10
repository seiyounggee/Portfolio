using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileWorkspace : MonoBehaviour
{
    // 원본 파일 경로와 새 파일 경로를 설정하세요
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
        // 새 위치에 파일이 이미 존재하는지 확인합니다.
        if (File.Exists(newPath))
        {
            Debug.Log("File already exists at the new location.");
            return; // 이미 파일이 있으면 작업을 중단합니다.
        }

        try
        {
            // 파일을 새 위치와 새 이름으로 복사합니다.
            File.Copy(sourcePath, newPath);
            Debug.Log("File copied and renamed successfully!");
        }
        catch (IOException copyError)
        {
            Debug.LogError("Failed to copy and rename file: " + copyError.Message);
        }
    }
}
