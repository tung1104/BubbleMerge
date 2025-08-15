using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.IAPStore
{
    public class StartedPackOffer : IAPStoreOffer
    {
                [SerializeField, Tooltip("In hours")] int infiniteLifeDuration;
                [Space]
                [SerializeField] List<PUType> powerUps;
                [SerializeField] int powerUpsAmount;
                [SerializeField] int coinsAmount;

                [Space]
                [SerializeField] TMP_Text powerUpsText;
                [SerializeField] TMP_Text livesText;
                [SerializeField] TMP_Text coinsText;

                protected override void Awake()
                {
                    base.Awake();

                    powerUpsText.text = $"x{powerUpsAmount}";
                    coinsText.text = $"x{coinsAmount}";
                    livesText.text = $"{infiniteLifeDuration}hrs";
                }

                protected override void ApplyOffer()
                {
                    LivesManager.StartInfiniteLives(infiniteLifeDuration * 60 * 60);

                    for(int i = 0; i < powerUps.Count; i++)
                    {
                        var type = powerUps[i];

                        PUController.AddPowerUp(type, powerUpsAmount);
                    }

                    CurrenciesController.Add(CurrencyType.Coin, coinsAmount);

                    AdsManager.DisableForcedAd();
                }

                protected override void ReapplyOffer()
                {
                    AdsManager.DisableForcedAd();
                }
    }
}