using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Base : MonoBehaviour
{
    public enum Depth {SuperLow, Low, Normal, High, SuperHigh}
    public Depth currentPanelDepth = Depth.Normal;
    public int GetDepthNum()
    {
        var UIPanel = this.GetComponent<UIPanel>();
        if (UIPanel != null)
        {
            return UIPanel.depth;
        }

        return 0;
    }

    public virtual void Show(Depth depth)
    {
        if (gameObject.activeSelf == true)
        {
#if UNITY_EDITOR
            Debug.Log(gameObject.name + " is already active!");
#endif
            return;
        }

        this.gameObject.SafeSetActive(true);

        UIManager_NGUI.Instance.SetActvatedUIBaseList(this, true, depth);
    }

    public virtual void Close()
    {
        this.gameObject.SafeSetActive(false);

        UIManager_NGUI.Instance.SetActvatedUIBaseList(this, false);
    }

    public virtual void SetDepth(Depth depth, int depthNum)
    {
        var UIPanel = this.GetComponent<UIPanel>();
        if (UIPanel != null)
        {
            UIPanel.depth = depthNum;
            currentPanelDepth = depth;
        }
    }

    public virtual void SetDepthToTop()
    {
        var UIPanel = this.GetComponent<UIPanel>();
        if (UIPanel != null && UIManager_NGUI.Instance.uiActivated != null)
        {
            int newDepth = UIManager_NGUI.Instance.uiActivated.Count + 1;
            if (currentPanelDepth == Depth.SuperLow)
                newDepth -= 2000;
            else if (currentPanelDepth == Depth.Low)
                newDepth -= 1000;
            else if (currentPanelDepth == Depth.High)
                newDepth += 1000;
            else if (currentPanelDepth == Depth.SuperHigh)
                newDepth += 2000;

            UIPanel.depth = newDepth;
        }
    }

}
