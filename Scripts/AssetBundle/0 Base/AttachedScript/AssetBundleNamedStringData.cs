using UnityEngine;

public class AssetBundleNamedStringData : MonoBehaviour
{
    [SerializeField]  string m_strName = string.Empty;
    [SerializeField]  string m_stringData = string.Empty;

#if UNITY_EDITOR
    public void UpdateAttachedAssetData( string name, string data )
    {
        m_strName = name;
        m_stringData = data;
    }
#endif

    public string GetName()
    {
        return m_strName;
    }

    public string GetAttachedAssetData()
    {
        return m_stringData;
    }
}
