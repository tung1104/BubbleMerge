
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.IAPStore
{
    public class PowerUpsOffer : IAPStoreOffer
    {
        [SerializeField] List<PowerUpDeal> powerUps;

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < powerUps.Count; i++)
            {
                var data = powerUps[i];

                data.text.text = $"x{data.amount}";
            }
        }

        protected override void ApplyOffer()
        { 
            for (int i = 0; i < powerUps.Count; i++)
            {
                var data = powerUps[i];

                PUController.AddPowerUp(data.type, data.amount);
            }
        }

        protected override void ReapplyOffer()
        {
            
        }

        [System.Serializable]
        private class PowerUpDeal
        {
            public TMP_Text text;
            public PUType type;
            public int amount;
        }
    }
}