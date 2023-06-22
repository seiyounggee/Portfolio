using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using WayPointSystem;

#if UNITY_EDITOR
[CustomEditor(typeof(FlowNGUILabel))]
public class FlowNGUILabelEditor : Editor
{
    FlowNGUILabel flowNGUILabel = null;

    private void OnEnable()
    {
        flowNGUILabel = target as FlowNGUILabel;

        if (flowNGUILabel != null)
        {
            if (flowNGUILabel.uILabel == null)
            {
                flowNGUILabel.uILabel = flowNGUILabel.GetComponent<UILabel>();
            }
        }
    }

    override public void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (flowNGUILabel == null)
            return;

        GUILayoutOption[] btnOptions = new[] {
                    GUILayout.Height (40),
                    };

        if (flowNGUILabel.parentPanel == null)
        {
            if (Application.isPlaying == false)
            {
                if (flowNGUILabel != null && flowNGUILabel.uILabel != null)
                    flowNGUILabel.uILabel.overflowMethod = UILabel.Overflow.ClampContent;
            }

            if (GUILayout.Button("Set Up Flow NGUI", btnOptions))
            {
                var root = flowNGUILabel.transform.parent;
                GameObject panelGo = new GameObject("Flow Label Parent");
                panelGo.layer = LayerMask.NameToLayer("UI");
                var panel = panelGo.AddComponent<UIPanel>();
                panel.clipping = UIDrawCall.Clipping.SoftClip;
                Vector3 originalPos = Vector3.zero;
                if (flowNGUILabel.uILabel != null)
                {
                    originalPos = flowNGUILabel.uILabel.transform.position;
                    var h = flowNGUILabel.uILabel.height;
                    var w = flowNGUILabel.uILabel.width;
                    panel.baseClipRegion = new Vector4(w / 2f, 0f, w, h);

                    flowNGUILabel.uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
                }

                UtilityCommon.SetParent(root.gameObject, panelGo.gameObject, true);

                flowNGUILabel.parentPanel = panel;

                if (flowNGUILabel.uILabel != null && flowNGUILabel.parentPanel != null)
                {
                    UtilityCommon.SetParent(flowNGUILabel.parentPanel.gameObject, flowNGUILabel.uILabel.gameObject, true);

                    flowNGUILabel.parentPanel.transform.position = originalPos;
                }
            }
        }
        else
        {
            if (Application.isPlaying == false)
            {
                if (flowNGUILabel != null)
                    flowNGUILabel.transform.localPosition = Vector3.zero;
            }
        }
    }

    
}
#endif