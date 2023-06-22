using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DtrPreloadAssetListEditor : BasePreloadAssetListEditor
{
    [MenuItem(CommonDefine.ProjectName + "/AssetBundle/Edit Preload Asset List", false, 3)]
    static public void EditAssetList()
    {
        var _window = EditorWindow.GetWindow<DtrPreloadAssetListEditor>();
        _window.Initialize(DtrServerEnum.Dev);
    }
}