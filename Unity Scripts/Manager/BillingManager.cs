using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.EssentialKit;
using static RefData.Ref_ShopData;
using static RefData.Ref_PlayerBasicStats;
using static RefData.Ref_SkinData;
using static RefData.Ref_SkillData;

public class BillingManager : MonoSingleton<BillingManager>
{
    private IBillingProduct[] billingProducts;

    Action purchaseCallback_Shop = null;

    private delegate void Purchase_IAP(Ref_ShopInfo shopInfo, Ref_ShopInfo.ItemDataInfo itemInfo, Action callback = null);
    private Purchase_IAP purchase_iap = null;
    private delegate void Purchase_Coin(Ref_ShopInfo shopInfo, Ref_ShopInfo.ItemDataInfo itemInfo, Action callback = null);
    private Purchase_Coin purchase_coin = null;
    private delegate void Purchase_Gem(Ref_ShopInfo shopInfo, Ref_ShopInfo.ItemDataInfo itemInfo, Action callback = null);
    private Purchase_Gem purchase_gem = null;
    
    

    private void OnEnable()
    {
        // register for events
        BillingServices.OnInitializeStoreComplete += OnInitializeStoreComplete;
        BillingServices.OnTransactionStateChange += OnTransactionStateChange;
        BillingServices.OnRestorePurchasesComplete += OnRestorePurchasesComplete;
    }

    private void OnDisable()
    {
        // unregister from events
        BillingServices.OnInitializeStoreComplete -= OnInitializeStoreComplete;
        BillingServices.OnTransactionStateChange -= OnTransactionStateChange;
        BillingServices.OnRestorePurchasesComplete -= OnRestorePurchasesComplete;
    }

    #region Shop Related
    public void InitializeBilling()
    {
        BillingServices.InitializeStore();
    }

    public void PurchaseItem(int shopID, int itemID, Action callback = null)
    {
        //초기화...
        purchaseCallback_Shop = null; 
        purchase_coin = null;
        purchase_gem = null;
        purchase_iap = null;

        var shopData = ReferenceManager.Instance.ShopData;
        if (shopData != null)
        {
            var shop = shopData.ShopInfoList.Find(x => x.ShopID.Equals(shopID));
            if (shop != null)
            {
                var item = shop.ItemList.Find(x => x.ItemId.Equals(itemID));
                if (item != null)
                {
                    switch ((CommonDefine.CurrencyType)item.PurchaseCurrencyType)
                    {
                        case CommonDefine.CurrencyType.IAP:
                            {
                                purchase_iap = new Purchase_IAP(PurchaseItem_IAP);
                                purchaseCallback_Shop = () => purchase_iap(shop, item, callback);
                                AccountManager.Instance.LoadMyData_FirebaseServer(LoadMyDataCallback_Shop);

                                UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                            }
                            break;

                        case CommonDefine.CurrencyType.Coin:
                            {
                                purchase_coin = new Purchase_Coin(PurchaseItem_Coin);
                                purchaseCallback_Shop = () => purchase_coin(shop, item, callback);
                                AccountManager.Instance.LoadMyData_FirebaseServer(LoadMyDataCallback_Shop);

                                UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                            }
                            break;
                        case CommonDefine.CurrencyType.Gem:
                            {
                                purchase_gem = new Purchase_Gem(PurchaseItem_Gem);
                                purchaseCallback_Shop = () => purchase_gem(shop, item, callback);
                                AccountManager.Instance.LoadMyData_FirebaseServer(LoadMyDataCallback_Shop);

                                UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
                            }
                            break;
                    }
                }
            }
        }
    }

    private void LoadMyDataCallback_Shop(bool isSuccess)
    {
        UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);

        if (isSuccess)
        {
            //AccountData 한번 가져온 후에 구매처리하자

            purchaseCallback_Shop?.Invoke();
        }
        else
        {
            var commonUI = PrefabManager.Instance.UI_CommonPopup;
            commonUI.SetUp(UI_CommonPopup.CommonPopupType.Okay_WithExit, "ERROR_TITLE", "ERROR_PURCHASE_FAILED", null);
            UIManager.Instance.ShowUI(commonUI);

            Debug.Log("Error >>> Firebase Account Data Loading Error");
        }
    }

    private void PurchaseItem_IAP(Ref_ShopInfo shopInfo, Ref_ShopInfo.ItemDataInfo itemInfo, Action callback)
    {
        if (BillingServices.CanMakePayments() == false)
        {
            Debug.Log("BillingServices.CanMakePayments() == false");
            return;
        }

        //todo..
        //BillingServices.BuyProduct(product);

#if UNITY_EDITOR || SERVERTYPE_DEV
        //일단 임시로.....?
        //Account Data 수정
        AccountManager.Instance.SetPurchaseShopData(shopInfo.ShopID, itemInfo.ItemId, itemInfo.PurchaseCurrencyType);
        AccountManager.Instance.GainAndSave_ShopRewardList(itemInfo.RewardList, (isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke();

                var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                rewardUI.Setup_ShopReward(itemInfo);
                UIManager.Instance.ShowUI(rewardUI);

#if UNITY_EDITOR
                Debug.Log("<color=white>PurchaseItem_IAP Success</color>");
#endif
            }
        });
#endif


    }

    private void PurchaseItem_Coin(Ref_ShopInfo shopInfo, Ref_ShopInfo.ItemDataInfo itemInfo, Action callback)
    {
        var myData = AccountManager.Instance.AccountData;
        if (myData.coin < itemInfo.Price)
        {
#if UNITY_EDITOR
            Debug.Log("Not Enough Coin!!");
#endif
            return;
        }

        AccountManager.Instance.SetPurchaseShopData(shopInfo.ShopID, itemInfo.ItemId, itemInfo.PurchaseCurrencyType);
        AccountManager.Instance.Use_Currency((CommonDefine.CurrencyType)itemInfo.PurchaseCurrencyType, itemInfo.Price);
        AccountManager.Instance.GainAndSave_ShopRewardList(itemInfo.RewardList, (isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke();

                var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                rewardUI.Setup_ShopReward(itemInfo);
                UIManager.Instance.ShowUI(rewardUI);

#if UNITY_EDITOR
                Debug.Log("<color=white>PurchaseItem_Coin Success</color>");
#endif
            }
        });

    }

    private void PurchaseItem_Gem(Ref_ShopInfo shopInfo, Ref_ShopInfo.ItemDataInfo itemInfo, Action callback)
    {
        var myData = AccountManager.Instance.AccountData;
        if (myData.gem < itemInfo.Price)
        {
#if UNITY_EDITOR
            Debug.Log("Not Enough Gem!!");
#endif
            return;
        }

        AccountManager.Instance.SetPurchaseShopData(shopInfo.ShopID, itemInfo.ItemId, itemInfo.PurchaseCurrencyType);
        AccountManager.Instance.Use_Currency((CommonDefine.CurrencyType)itemInfo.PurchaseCurrencyType, itemInfo.Price);
        AccountManager.Instance.GainAndSave_ShopRewardList(itemInfo.RewardList, (isSuccess) =>
        {
            if (isSuccess)
            {
                callback?.Invoke();

                var rewardUI = PrefabManager.Instance.UI_RewardPopup;
                rewardUI.Setup_ShopReward(itemInfo);
                UIManager.Instance.ShowUI(rewardUI);

#if UNITY_EDITOR
                Debug.Log("<color=white>PurchaseItem_Gem Success</color>");
#endif
            }
        });


    }

    private void OnInitializeStoreComplete(BillingServicesInitializeStoreResult result, Error error)
    {
        if (error != null)
        {
            Debug.Log("Error >>> BillingManager.OnInitializeStoreComplete()");
        }
        else
        {
            // show console messages
            billingProducts = result.Products;
            Debug.Log("Store initialized successfully.");
            Debug.Log("Total products fetched: " + billingProducts.Length);
            Debug.Log("Below are the available products:");
            for (int iter = 0; iter < billingProducts.Length; iter++)
            {
                var product = billingProducts[iter];
                Debug.Log(string.Format("[{0}]: {1}", iter, product));
            }
        }
    }

    private void OnTransactionStateChange(BillingServicesTransactionStateChangeResult result)
    {
        var transactions = result.Transactions;
        for (int iter = 0; iter < transactions.Length; iter++)
        {
            var transaction = transactions[iter];
            switch (transaction.TransactionState)
            {
                case BillingTransactionState.Purchased:
                    purchaseCallback_Shop?.Invoke();
                    Debug.Log(string.Format("Buy product with id:{0} finished successfully.", transaction.Payment.ProductId));
                    break;

                case BillingTransactionState.Failed:
                    Debug.Log(string.Format("Buy product with id:{0} failed with error. Error: {1}", transaction.Payment.ProductId, transaction.Error));
                    break;
            }
        }
    }

    private void OnRestorePurchasesComplete(BillingServicesRestorePurchasesResult result, Error error)
    {
        if (error != null)
        {

        }
        else
        {
            Debug.Log("Error >>> BillingManager.OnRestorePurchasesComplete()");
        }
    }
    #endregion


    #region Stats Related

    public enum StatsPurchaseResult { Probability_Success, Probability_Fail, Network_Fail, }

    private delegate void Purchase_Stat(Ref_PlayerUpgradeStatsInfo statsInfo, Action<StatsPurchaseResult> callback = null);
    private Purchase_Stat purchase_stat = null;
    private Action purchaseStatCallback = null;

    public void PurchaseStats(Ref_PlayerUpgradeStatsInfo info, Action<StatsPurchaseResult> callback = null)
    {
        purchase_stat = null;
        purchaseStatCallback = null;

        if (IsHaveEnoughCurrency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price) == false)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("Not Enough " + (CommonDefine.CurrencyType)info.PurchaseCurrencyType + " !!");
#endif
            return;
        }

        if ((CommonDefine.CurrencyType)info.PurchaseCurrencyType == CommonDefine.CurrencyType.IAP)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("IAP Purchase Blocked");
#endif
            return;
        }

        purchase_stat = new Purchase_Stat(PurchaseStatsUp);
        purchaseStatCallback = () => purchase_stat(info, callback);

        UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);
        AccountManager.Instance.LoadMyData_FirebaseServer(LoadMyDataCallback_Stats);
    }

    private void LoadMyDataCallback_Stats(bool isSuccess)
    {
        UIManager.Instance.HideUI(UIManager.UIType.UI_TouchDefense);

        if (isSuccess)
        {
            purchaseStatCallback?.Invoke();
        }
        else
        {
            //네트워크 오류 실패...
            var commonUI = PrefabManager.Instance.UI_CommonPopup;
            commonUI.SetUp(UI_CommonPopup.CommonPopupType.Okay_WithExit, "ERROR_TITLE", "ERROR_PURCHASE_FAILED", null);
            UIManager.Instance.ShowUI(commonUI);
        }
    }

    public void PurchaseStatsUp(Ref_PlayerUpgradeStatsInfo statsInfo, Action<StatsPurchaseResult> callback = null)
    {
        UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);

        //todo... 해킹에 취약
        var random = UnityEngine.Random.Range(1, 101);
        if (random <= statsInfo.SuccessProbability)
        {
            //강화 성공...
            AccountManager.Instance.Use_Currency((CommonDefine.CurrencyType)statsInfo.PurchaseCurrencyType, statsInfo.Price);
            AccountManager.Instance.Save_StatsData((CommonDefine.PlayerStatsType)statsInfo.type, statsInfo.level, (isSuccess) =>
            {
                if (isSuccess)
                    callback?.Invoke(StatsPurchaseResult.Probability_Success);
                else
                    callback?.Invoke(StatsPurchaseResult.Network_Fail);
            });
        }
        else
        {
            //강화 실패
            AccountManager.Instance.UseAndSave_Currency((CommonDefine.CurrencyType)statsInfo.PurchaseCurrencyType, statsInfo.Price);
            callback?.Invoke(StatsPurchaseResult.Probability_Fail);
        }
    }

    #endregion

    #region Skin Related

    //스킨 구매
    public void PurchaseSkin(CommonDefine.SkinType type, Ref_SkinInfo info, Action<bool> callback = null)
    {
        if (IsHaveEnoughCurrency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price) == false)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("Not Enough " + (CommonDefine.CurrencyType)info.PurchaseCurrencyType + " !!");
#endif
            return;
        }

        UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);

        AccountManager.Instance.Use_Currency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price);
        AccountManager.Instance.Save_OwnedSkinData(type, info.SkinID, callback);
    }

    #endregion

    #region Skill Related

    //액티브 스킬 구매
    public void PurchaseSkill_Active(CommonDefine.SkillType type, Ref_ActiveSkillInfo info, Action<bool> callback = null)
    {
        if (IsHaveEnoughCurrency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price) == false)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("Not Enough " + (CommonDefine.CurrencyType)info.PurchaseCurrencyType + " !!");
#endif
            return;
        }


        UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);

        AccountManager.Instance.Use_Currency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price);
        AccountManager.Instance.Save_OwnedSkillData(type, info.SkillID, callback);
    }

    //패시브 스킬구매
    public void PurchaseSkill_Passive(CommonDefine.SkillType type, Ref_PassiveSkillInfo info, Action<bool> callback = null)
    {
        if (IsHaveEnoughCurrency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price) == false)
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("Not Enough " + (CommonDefine.CurrencyType)info.PurchaseCurrencyType + " !!");
#endif
            return;
        }


        UIManager.Instance.ShowUI(UIManager.UIType.UI_TouchDefense);

        AccountManager.Instance.Use_Currency((CommonDefine.CurrencyType)info.PurchaseCurrencyType, info.Price);
        AccountManager.Instance.Save_OwnedSkillData(type, info.SkillID, callback);
    }

    #endregion


    private bool IsHaveEnoughCurrency(CommonDefine.CurrencyType type, int price)
    {
        var myData = AccountManager.Instance.AccountData;

        switch (type)
        {
            case CommonDefine.CurrencyType.Coin:
                return (myData.coin >= price);
            case CommonDefine.CurrencyType.Gem:
                return (myData.gem >= price);
            default:
            case CommonDefine.CurrencyType.IAP:
                return true;
        }
    }
}
