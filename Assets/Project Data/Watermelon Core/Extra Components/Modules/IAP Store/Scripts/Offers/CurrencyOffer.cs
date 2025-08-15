using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.IAPStore
{
    public class CurrencyOffer : IAPStoreOffer
    {
        [SerializeField] int coinsAmount;

        [SerializeField] TMP_Text currencyAmountText;

        protected override void Awake()
        {
            base.Awake();
            currencyAmountText.text = $"x{coinsAmount}";
        }

        protected override void ApplyOffer()
        {
            CurrenciesController.Add(CurrencyType.Coin, coinsAmount);
        }

        protected override void ReapplyOffer()
        {
            
        }
    }
}