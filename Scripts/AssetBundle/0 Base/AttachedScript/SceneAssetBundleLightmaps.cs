using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAssetBundleLightmaps : MonoBehaviour 
{
    [System.Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public int lightmapIndex;
    //  public Vector4 lightmapOffsetScale;
    }


    public RendererInfo[] m_RendererInfo;
    public UnityEngine.LightmapsMode m_LightingMode;

    //
    void Awake()
    {
       Activate();
    }

    public void Activate()
    {
        if (m_RendererInfo == null || m_RendererInfo.Length == 0)
            return;
        
        UnityEngine.LightmapSettings.lightmapsMode = m_LightingMode;


        ApplyRendererInfo(m_RendererInfo);
    }


    static void ApplyRendererInfo( RendererInfo[] infos )
    {
        Debug.Log(" ApplyRendererInfo !! " + infos.Length);

        for( int i=0, _max = infos.Length; i < _max; ++i )
        {
            RendererInfo _info = infos[i];
            if( _info.renderer == null )
                continue;

            _info.renderer.gameObject.isStatic = true;
            _info.renderer.lightmapIndex = _info.lightmapIndex;
     //       _info.renderer.lightmapScaleOffset = _info.lightmapOffsetScale;
        }
    }

#if UNITY_EDITOR
    //[UnityEditor.MenuItem("Assets/Assign Scene Assetbundle Lightmaps")]
    //static void GenerateLightmapInfo()
    //{
    //    if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
    //    {
    //        return;
    //    }
    //
    //    SceneAssetBundleLightmaps[] prefabs = FindObjectsOfType<SceneAssetBundleLightmaps>();
    //    foreach (var instance in prefabs)
    //    {
    //        var gameObject = instance.gameObject;
    //        var rendererInfos = new List<RendererInfo>();
    //        var lightmaps = new List<Texture2D>();
    //
    //        GenerateLightmapInfo(gameObject, rendererInfos, lightmaps);
    //
    //        instance.m_RendererInfo = rendererInfos.ToArray();
    //    }
    //}
    //
    //static void GenerateLightmapInfo( GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps )
    //{
    //    var _renderers = root.GetComponentsInChildren<MeshRenderer>();
    //    foreach (MeshRenderer renderer in _renderers )
    //    {
    //        if (renderer.lightmapIndex != -1)
    //        {
    //            RendererInfo info = new RendererInfo();
    //            info.renderer = renderer;
    //            info.lightmapOffsetScale = renderer.lightmapScaleOffset;
    //            info.lightmapIndex = renderer.lightmapIndex;
    //            rendererInfos.Add(info);
    //        }
    //    }
    //}

    public static void GenerateLightmapInfo( UnityEngine.LightmapsMode mode, GameObject target )
    {
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("??? GenerateLightmapInfo Error");
            return;
        }

        var rendererInfos = new List<RendererInfo>();
     
        var _script = target.GetComponent<SceneAssetBundleLightmaps>();
        if (_script == null)
        {
            _script = target.AddComponent<SceneAssetBundleLightmaps>();
        }

        var _renderers = target.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in _renderers )
        {
            if (renderer.lightmapIndex != -1)
            {
                RendererInfo info = new RendererInfo();
                info.renderer = renderer;
        //        info.lightmapOffsetScale = renderer.lightmapScaleOffset;
                info.lightmapIndex = renderer.lightmapIndex;
                rendererInfos.Add(info);
            }
        }

        _script.m_RendererInfo = rendererInfos.ToArray();
        _script.m_LightingMode = mode;
    }
#endif

}
