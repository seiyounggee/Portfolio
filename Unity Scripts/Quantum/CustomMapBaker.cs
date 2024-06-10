using Photon.Deterministic;
using Quantum;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Assert = Quantum.Assert;
using UnityEngine.SceneManagement;
using Photon.Analyzer;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[MapDataBakerCallback(invokeOrder: 2)]
public class CustomMapBaker : MapDataBakerCallback
{
    public override void OnBeforeBake(MapData data)
    {
        //Debug.Log("<color=cyan>OnBeforeBake Map</color>");
    }

    public override void OnBake(MapData data)
    {
        //Debug.Log("<color=cyan>OnBake Map</color>");

        Scene currScene = SceneManager.GetActiveScene();

        if (currScene.name.Equals(CommonDefine.OutGameScene) || currScene.name.Equals(CommonDefine.InGameScene))
            return;
            
        var spawnData = GameObject.FindObjectOfType<CustomMapData>();

        if (spawnData == null)
        {
            Debug.Log("<color=red>Spawn Data is null in your scene...</color>");
            return;
        }

        var customData = spawnData.CustomMapDataSettingsAsset;
        var spawnPoints = spawnData.MapSpawnPointList;

        if (customData == null || spawnPoints.Count == 0)
        {
            if (customData == null)
                Debug.Log("<color=red>CustomMapDataSettingsAsset is null in your scene...</color>");

            if (spawnPoints == null || spawnPoints.Count == 0)
                Debug.Log("<color=red>MapSpawnPointList is null in your scene...</color>");

            return;
        }

        var defaultSpawnPoint = spawnPoints[0];
        if (customData.Settings.DefaultSpawnPoint.Equals(default(CustomMapDataSettings.SpawnPointData)))
        {
            customData.Settings.DefaultSpawnPoint.Position = defaultSpawnPoint.transform.position.ToFPVector3();
            customData.Settings.DefaultSpawnPoint.Rotation = defaultSpawnPoint.transform.rotation.ToFPQuaternion();
        }

        customData.Settings.SpawnPoints = new CustomMapDataSettings.SpawnPointData[spawnPoints.Count];
        for (var i = 0; i < spawnPoints.Count; i++)
        {
            customData.Settings.SpawnPoints[i].Position = spawnPoints[i].transform.position.ToFPVector3();
            customData.Settings.SpawnPoints[i].Rotation = spawnPoints[i].transform.rotation.ToFPQuaternion();
        }

        Debug.Log("<color=cyan>CustomMapDataSettingsAsset Successfully Set!</color>");

#if UNITY_EDITOR
        EditorUtility.SetDirty(customData);
#endif
    }
}