using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_ShopItem_Slot : MonoBehaviour
{
    [SerializeField] RawImage image = null;
    [SerializeField] Button buyBtn = null;

    [SerializeField] TextMeshProUGUI nameTxt = null;
    [SerializeField] TextMeshProUGUI quantityTxt = null;
    [SerializeField] TextMeshProUGUI priceTxt = null;

    private RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo dataInfo = null;

    private void Awake()
    {
        buyBtn.SafeSetButton(OnClickBtn);
    }

    public void SetData(RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo data)
    {
        if (data == null)
            return;

        dataInfo = data;

        nameTxt.SafeSetText(((CommonDefine.ItemType)data.ItemType).ToString() + "_" + data.ItemId);

        priceTxt.SafeSetText(GetPrice());

        string rewardTxt = string.Empty;
        for (int i = 0; i < data.RewardList.Count; i++)
        {
            rewardTxt += (CommonDefine.RewardType)data.RewardList[i].RewardType + "  x" + StringManager.GetNumberChange(data.RewardList[i].Quantity);

            if (i < data.RewardList.Count - 1)
                rewardTxt += "\n";
        }
        quantityTxt.SafeSetText(rewardTxt);
        image.texture = ResourceManager.Instance.GetOutGameIconByItemType((CommonDefine.ItemType)data.ItemType, data.ItemId);
    }

    private void OnClickBtn(Button btn)
    {
        if (btn == buyBtn)
        {
            UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
            BillingManager.Instance.PurchaseItem((int)CommonDefine.ShopType.Common, dataInfo.ItemId);
        }
    }

    private string GetPrice()
    {
        //TODO юс╫ц...!

        string price = dataInfo.Price.ToString();

        switch ((CommonDefine.CurrencyType)dataInfo.PurchaseCurrencyType)
        {
            case CommonDefine.CurrencyType.Coin:
                {
                    price += " coin";
                }
                break;
            case CommonDefine.CurrencyType.Gem:
                {
                    price += " gem";
                }
                break;
            case CommonDefine.CurrencyType.IAP:
                {
                    price += " IAP";
                }
                break;
        }

        return price;
    }
}
