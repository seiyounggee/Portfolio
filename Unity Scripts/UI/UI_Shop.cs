using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Shop : UIBase, IBackButtonHandler
{
    [SerializeField] Button homeBtn = null;
    [SerializeField] UIComponent_Currency currency;

    [SerializeField] Transform groupListParent = null;
    [SerializeField] UIComponent_ShopItem_Group groupTemplate_big = null;
    [SerializeField] UIComponent_ShopItem_Group groupTemplate_small = null;

    private List<UIComponent_ShopItem_Group> shopGroupList = new List<UIComponent_ShopItem_Group>();

    private void Awake()
    {
        homeBtn.SafeSetButton(OnClickBtn);

        groupTemplate_big.SafeSetActive(false);
        groupTemplate_small.SafeSetActive(false);
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

        CreateList();
        SetCurrency();
    }

    private void CreateList()
    {
        var shopData = ReferenceManager.Instance.ShopData;
        if (shopData != null && shopData.ShopInfoList != null)
        {
            var shop = shopData.ShopInfoList.Find(x => x.ShopType.Equals((int)CommonDefine.ShopType.Common));
            if (shop != null && shop.ItemList != null)
            {
                //¾È¾²´Â ³ðµéÀº ²¨ÁÖÀÚ...
                for (int i = 0; i < shopGroupList.Count; i++)
                { 
                    if(i >= shop.ItemList.Count)
                        shopGroupList[i].SafeSetActive(false);
                }

                shop.ItemList.Sort((x, y) => x.UI_Sort.CompareTo(y.UI_Sort));

                for (int i = 0; i < shop.ItemList.Count; i++)
                {
                    var item = shopGroupList.Find(x => x.GroupID.Equals(shop.ItemList[i].UI_GroupID));
                    var groupItemList = shop.ItemList.FindAll(x => x.UI_GroupID.Equals(shop.ItemList[i].UI_GroupID));
                    if (item != null)
                    {
                        item.SetData(groupItemList, shop.ItemList[i].UI_GroupID);
                        item.SafeSetActive(true);
                    }
                    else
                    {
                        UIComponent_ShopItem_Group template;
                        switch ((CommonDefine.ItemType)shop.ItemList[i].ItemType)
                        {
                            case CommonDefine.ItemType.CoinBundle_Small:
                            case CommonDefine.ItemType.CoinBundle_Mid:
                            case CommonDefine.ItemType.CoinBundle_Large:
                            case CommonDefine.ItemType.CoinBundle_Mega:
                            case CommonDefine.ItemType.GemBundle_Small:
                            case CommonDefine.ItemType.GemBundle_Mid:
                            case CommonDefine.ItemType.GemBundle_Large:
                            case CommonDefine.ItemType.GemBundle_Mega:
                                {
                                    template = groupTemplate_small;
                                }
                                break;

                            default:
                            case CommonDefine.ItemType.CharacterBundle:
                            case CommonDefine.ItemType.WeaponBundle:
                            case CommonDefine.ItemType.SkillBundle:
                            case CommonDefine.ItemType.QuestPassBundle:
                                {
                                    template = groupTemplate_big;
                                }
                                break;
                        }

                        var newSlot = Instantiate(template);
                        newSlot.transform.SetParent(groupListParent);
                        newSlot.transform.localScale= Vector3.one;
                        newSlot.SafeSetActive(true);
                        newSlot.SetData(groupItemList, shop.ItemList[i].UI_GroupID);
                        shopGroupList.Add(newSlot);
                    }
                }
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

    public void OnBackButton()
    {
        Hide();
    }

    public void SetCurrency()
    {
        currency?.SetCurrency();
    }

    public UIComponent_Currency GetCurrency()
    {
        return currency;
    }
}
