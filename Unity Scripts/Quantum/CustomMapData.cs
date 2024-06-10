using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMapData : MonoBehaviour
{
    [SerializeField] public List<GameObject> MapSpawnPointList = new List<GameObject>();
    [SerializeField] public CustomMapDataSettingsAsset CustomMapDataSettingsAsset = null;

}
