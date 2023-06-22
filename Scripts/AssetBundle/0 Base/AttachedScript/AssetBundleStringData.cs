using UnityEngine;

// script for saving asset bundle script data
public class AssetBundleStringData : MonoBehaviour 
{
    [SerializeField]  string m_stringData = string.Empty;

#if UNITY_EDITOR
    public void UpdateAttachedAssetData( string data )
    {
        m_stringData = data;
    }
#endif

    public string GetAttachedAssetData()
    {
        return m_stringData;
    }
}
