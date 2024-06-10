using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_Menu : MonoBehaviour
{
    [SerializeField] Button menuBtn = null;

    private void Awake()
    {
        menuBtn.SafeSetButton(OnClickBtn);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == menuBtn)
        {
            UIManager.Instance.ShowUI(UIManager.UIType.UI_Setting);
        }
    }
}
