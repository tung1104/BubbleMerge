using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleMerge;

namespace Watermelon
{
    public class PUUIPurchasePanel : MonoBehaviour
    {
        [SerializeField] GameObject powerUpPurchasePanel;

        [Space(5)]
        [SerializeField] Image powerUpPurchasePreview;
        [SerializeField] TMP_Text powerUpPurchaseAmountText;
        [SerializeField] TMP_Text powerUpPurchaseDescriptionText;
        [SerializeField] TMP_Text powerUpPurchasePriceText;

        [Space(5)]
        [SerializeField] Button smallCloseButton;
        [SerializeField] Button bigCloseButton;
        [SerializeField] Button purchaseButton;

        [Space(5)]
        [SerializeField] CurrencyUIPanelSimple currencyPanel;

        private PUSettings settings;

        private void Awake()
        {
            smallCloseButton.onClick.AddListener(ClosePurchasePUPanel);
            bigCloseButton.onClick.AddListener(ClosePurchasePUPanel);
            purchaseButton.onClick.AddListener(PurchasePUButton);

            currencyPanel.Initialise();
            currencyPanel.AddButton.onClick.AddListener(() => UIController.ShowPage<IAPStore.IAPStoreUI>());
        }

        public void Show(PUSettings settings)
        {
            this.settings = settings;

            powerUpPurchasePanel.SetActive(true);

            powerUpPurchasePreview.sprite = settings.Icon;
            powerUpPurchaseDescriptionText.text = settings.Description;
            powerUpPurchasePriceText.text = settings.Price.ToString();
            powerUpPurchaseAmountText.text = string.Format("x{0}", settings.PurchaseAmount);

            LevelController.OnGamePopupOpened();
        }

        public void PurchasePUButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            bool purchaseSuccessful = PUController.PurchasePowerUp(settings.Type);

            if (purchaseSuccessful)
                ClosePurchasePUPanel();
        }

        public void ClosePurchasePUPanel()
        {
            powerUpPurchasePanel.SetActive(false);

            LevelController.OnGamePopupClosed();
        }
    }
}