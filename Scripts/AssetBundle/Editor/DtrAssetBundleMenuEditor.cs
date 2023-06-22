using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DtrAssetBundleMenuEditor : BaseAssetBundleMenuEditor
{
    bool m_bNeedRestore = false;

    public DtrAssetBundleMenuEditor()
    {
        AssetBundleEditorUtil.COMMON_PATH = "Assets/DownloadAsset";

        MakeServerNameLists(typeof(DtrServerEnum));
    }

    protected override void UpdateServerData(AssetDefines.ServerEnum type)
    {
        if (type == DtrServerEnum.Dev)
        {
            UpdateServerData_MakeMode();
        }
        else if (type == DtrServerEnum.QA)
        {
            UpdateServerData_EditMode(DtrServerEnum.Dev);
        }
        else if (type == DtrServerEnum.Review)
        {
            UpdateServerData_EditMode(DtrServerEnum.QA);
        }
        else if (type == DtrServerEnum.Release)
        {
            UpdateServerData_CopyMode(DtrServerEnum.Review, DtrServerEnum.Release);
        }
        else
        {
            base.UpdateServerData(type);
        }
    }

    public override void Initialize(System.Type menu_type, AssetDefines.MakeAssetMenuEnum type)
    {
        base.Initialize(menu_type, type);

        m_dicTypePath.Add(DtrAssetMenuEnum.Car, new string[] { "Car" });
        m_dicTypePath.Add(DtrAssetMenuEnum.Character, new string[] { "Character" });
        m_dicTypePath.Add(DtrAssetMenuEnum.Map, new string[] { "Map" });


        //에러 때문에 아래 dependency는 제외하자
        // , 로 구분하자
        m_dicDependencyNotCheckKeyword.Add(DtrAssetMenuEnum.All, IGNORE_DEPENDENCY_KEYWORD);
        m_dicDependencyNotCheckKeyword.Add(DtrAssetMenuEnum.Car, IGNORE_DEPENDENCY_KEYWORD);
        m_dicDependencyNotCheckKeyword.Add(DtrAssetMenuEnum.Character, IGNORE_DEPENDENCY_KEYWORD);
        m_dicDependencyNotCheckKeyword.Add(DtrAssetMenuEnum.Map, IGNORE_DEPENDENCY_KEYWORD);
    }

    const string IGNORE_DEPENDENCY_KEYWORD = 
        "Editor," +
        "com.unity.render-pipelines.universal," +
        "VolumeProfile," +
        "Volume";

    protected override void MakeAssetLists(AssetDefines.MakeAssetMenuEnum type, List<AssetState> list)
    {
        if (type == DtrAssetMenuEnum.All)
        {
            MakeAssetLists(DtrAssetMenuEnum.Car, list);
            MakeAssetLists(DtrAssetMenuEnum.Character, list);
            MakeAssetLists(DtrAssetMenuEnum.Map, list);

        }
        else if (type == DtrAssetMenuEnum.Car)
        {
            AddToAssetStateList(type, ref list, BaseAssetBundleMaker.FindAssetFiles.OnlyFiles, AssetFilesData.ScriptExtractionStep.None);
        }
        else if (type == DtrAssetMenuEnum.Character)
        {
            AddToAssetStateList(type, ref list, BaseAssetBundleMaker.FindAssetFiles.OnlyFiles, AssetFilesData.ScriptExtractionStep.None);
        }
        else if (type == DtrAssetMenuEnum.Map)
        {
            AddToAssetStateList(type, ref list, BaseAssetBundleMaker.FindAssetFiles.OnlyFiles, AssetFilesData.ScriptExtractionStep.None);
        }

    }

    protected override void ShowCopyEditor(AssetDefines.ServerEnum server_type)
    {
        BaseAssetBundleCopyEditor.ShowCopyAssetBundles(typeof(DtrAssetBundleCopyEditor), server_type);
    }

    void OnGUI()
    {
        OnGuiDraw_First();

        var _server_type = m_TargetData.serverType;
        if (_server_type == DtrServerEnum.None)
        {
        }
        else if (_server_type == DtrServerEnum.Dev)
        {
            OnGuiDraw_MakeAssetsAndAssetVersion();
        }
        else if (_server_type == DtrServerEnum.Release)
        {
            OnGuiDraw_MakeAssetVersionReal();
        }
        else
        {
            OnGuiDraw_MakeAssetVersion();
        }
    }

    protected override void OnPrevMakeBundle(AssetDefines.MakeAssetMenuEnum menu_type)
    {
        base.OnPrevMakeBundle(menu_type);
    }

    protected override void OnPostMakeBundle(AssetDefines.MakeAssetMenuEnum menu_type)
    {
        base.OnPostMakeBundle(menu_type);
    }

    //
    [MenuItem(CommonDefine.ProjectName + "/AssetBundle/Make AssetBundles", false, 1)]
    static void CreateAssetBundleMaker()
    {
        AssetBundleMaker.InitializeData();

        var _popup = EditorWindow.GetWindow<DtrAssetBundleMenuEditor>();
        _popup.Initialize(typeof(DtrAssetMenuEnum), DtrAssetMenuEnum.None);
    }
}
