using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIComponent_Currency : MonoBehaviour
{
    [SerializeField] RawImage coinImg = null;
    [SerializeField] TextMeshProUGUI coinTxt;

    [SerializeField] RawImage gemImg = null;
    [SerializeField] TextMeshProUGUI gemTxt;

    public RectTransform CoinTrans
    {
        get { if (coinImg != null) return coinImg.rectTransform; else return null; }
    }
    public RectTransform GemTrans
    {
        get { if (gemImg != null) return gemImg.rectTransform; else return null; }
    }

    public bool UseAutoChange = true;
    [Range(0, 10f)]public float changeDelay = 1f;

    private void Awake()
    {
        coinTxt.SafeSetText(StringManager.GetNumberChange(0));
        gemTxt.SafeSetText(StringManager.GetNumberChange(0));
    }

    public void OnEnable()
    {
        AccountManager.Instance.OnValueChangedAccountData += OnValueChangedAccountData;
    }

    public void OnDisable()
    {
        AccountManager.Instance.OnValueChangedAccountData -= OnValueChangedAccountData;
    }

    private void OnValueChangedAccountData()
    {
        if (UseAutoChange)
        {
            UtilityInvoker.Invoke(this, () => SetCurrency(), changeDelay);
        }
    }

    public void SetCurrency()
    {
        if (AccountManager.Instance.AccountData != null)
        {
            var data = AccountManager.Instance.AccountData;
            coinTxt.SafeSetText(StringManager.GetNumberChange(data.coin));
            gemTxt.SafeSetText(StringManager.GetNumberChange(data.gem));
        }
    }

    public void SetCurrency(int coinValue, int gemValue)
    {
        coinTxt.SafeSetText(StringManager.GetNumberChange(coinValue));
        gemTxt.SafeSetText(StringManager.GetNumberChange(gemValue));
    }
}
