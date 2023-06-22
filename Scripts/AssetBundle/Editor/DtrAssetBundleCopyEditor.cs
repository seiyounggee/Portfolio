using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class DtrAssetBundleCopyEditor : BaseAssetBundleCopyEditor
{
    public DtrAssetBundleCopyEditor()
    {
        MakeServerNameLists(typeof(DtrServerEnum));
    }
    

    [MenuItem(CommonDefine.ProjectName + "/AssetBundle/Copy to StreamingAsset Folder", false, 2)]
    static public void CopyAssets()
    {
        ShowCopyAssetBundles(typeof(DtrAssetBundleCopyEditor), DtrServerEnum.Dev);
    }
}