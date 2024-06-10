using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UI_Stats : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;
    [SerializeField] UIComponent_Currency currency;

    [SerializeField] Transform statsSlotParent = null;
    [SerializeField] UIComponent_Stats_Slot statsSlotTemplate = null;

    private List<UIComponent_Stats_Slot> slotList = new List<UIComponent_Stats_Slot>();

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);

        statsSlotTemplate.SafeSetActive(false);
    }

    internal override void OnEnable()
    {
        base.OnEnable();

        AccountManager.Instance.OnValueChangedAccountData += OnValueChangedAccountData;

        UIManager.Instance.RegisterBackButton(this);
    }

    internal override void OnDisable()
    {
        base.OnDisable();

        AccountManager.Instance.OnValueChangedAccountData -= OnValueChangedAccountData;

        UIManager.Instance.UnregisterBackButton(this);
    }

    public override void Show()
    {
        base.Show();

        currency?.SetCurrency();

        SetStatsList();
    }

    public override void Hide()
    {
        base.Hide();

        Clear();
    }

    private void Clear()
    {
        foreach (var i in slotList)
            i.SafeSetActive(false);
    }

    private void SetStatsList()
    {
        foreach (var i in slotList)
        {
            i.SafeSetActive(false);
        }

        var list = ReferenceManager.Instance.GetMyPlayerUpgradeStatsInfo();

        //Debug.Log("list count >> " + list.Count);

        if (list == null || list.Count <= 0)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (i < slotList.Count)
            {
                slotList[i].Setup(list[i]);
                slotList[i].SafeSetActive(true);
            }
            else
            {
                var slot = GameObject.Instantiate(statsSlotTemplate);
                slot.transform.SetParent(statsSlotParent);
                slot.transform.localScale = Vector3.one;
                slot.Setup(list[i]);
                slot.SafeSetActive(true);
                slotList.Add(slot);
            }
        }
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == homeBtn)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundClip.Common_UIClick_Close);
            Hide();
        }
    }

    private void OnValueChangedAccountData()
    {
        SetStatsList();
    }

    public void OnBackButton()
    {
        Hide();
    }
}
