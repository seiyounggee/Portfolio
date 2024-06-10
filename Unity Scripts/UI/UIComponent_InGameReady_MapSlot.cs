using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_InGameReady_MapSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mapNameTxt;
    [SerializeField] RawImage mapImg;

    public void Setup(RefData.Ref_MapData.Ref_MapInfo info)
    {
        mapNameTxt.SafeLocalizeText(info.MapNameKey);
    }
}
