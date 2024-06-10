using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponent_ShopItem_Group : MonoBehaviour
{
    public int GroupID { get; set; }

    [SerializeField] GroupType groupType = GroupType.None;

    [SerializeField] Transform slotParent = null;
    [SerializeField] UIComponent_ShopItem_Slot slotTemplate;

    private List<UIComponent_ShopItem_Slot> slotList = new List<UIComponent_ShopItem_Slot>();

    private List<RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo> dataInfo;

    public enum GroupType
    { 
        None,
        Big,
        Small
    }

    private void Awake()
    {
        dataInfo = null;
        GroupID = -1;

        slotTemplate.SafeSetActive(false);

        if (slotParent == null)
            slotParent = transform;
    }

    public void SetData(List<RefData.Ref_ShopData.Ref_ShopInfo.ItemDataInfo> infoList, int groupID)
    {
        GroupID = groupID;
        dataInfo = infoList;

        CreateList();
    }

    private void CreateList()
    {
        if (dataInfo == null)
            return;

        //¾È¾²´Â ³ðµéÀº ²¨ÁÖÀÚ...
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i >= dataInfo.Count)
                slotList[i].SafeSetActive(false);
        }

        for (int i = 0; i < dataInfo.Count; i++)
        {
            if (i < slotList.Count)
            {
                slotList[i].SetData(dataInfo[i]);
                slotList[i].SafeSetActive(true);
            }
            else
            {
                var newSlot = Instantiate(slotTemplate);
                newSlot.transform.SetParent(slotParent);
                newSlot.transform.localScale = Vector3.one;
                newSlot.SafeSetActive(true);
                newSlot.SetData(dataInfo[i]);
                slotList.Add(newSlot);
            }
        }
    }
}
