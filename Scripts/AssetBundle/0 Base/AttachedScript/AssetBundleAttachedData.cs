using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleAttachedData : MonoBehaviour
{
    [SerializeField] Object[] objAttached = null;

#if UNITY_EDITOR
    public void UpdateAttachedAssetData( Object[] data )
    {
        objAttached = data;
    }
#endif

    public Object[] GetAttachedAssetData()
    {
        return objAttached;
    }
}
