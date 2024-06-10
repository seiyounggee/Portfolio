using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UI_Rank : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;
    [SerializeField] UIComponent_Currency currency;

    [SerializeField] InfiniteScrollRect infiniteScroll = null;

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        UIManager.Instance.RegisterBackButton(this);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        UIManager.Instance.UnregisterBackButton(this);
    }

    public override void Show()
    {
        base.Show();

        SetupScroll();
    }

    private void SetupScroll()
    {
        if (RankingManager.Instance.RankingData == null)
        {
#if UNITY_EDITOR
            Debug.Log("<color==red>RankingData is null</color>");
#endif
            return;
        }

        var list = RankingManager.Instance.RankingData.TotalRP_RankingUserList;

        if (list == null || list.Count <= 0)
        {
#if UNITY_EDITOR
            Debug.Log("<color==red>TotalRP_RankingUserList is null or 0</color>");
#endif
            return;
        }


        infiniteScroll.onVerifyIndex = (index =>
        { 
            return index >= 0 && index < list.Count;
        });
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
    }

    public void OnBackButton()
    {
        Hide();
    }
}
