using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.IAPStore
{
    public class IAPStoreUI : UIPage
    {
        [SerializeField] VerticalLayoutGroup layout;
        [SerializeField] RectTransform content;
        [SerializeField] Button closeButton;
        [SerializeField] CurrencyUIPanelSimple coinsUI;

        public static bool IsStoreAvailable { get; private set; } = false;

        private List<IAPStoreOffer> offers = new List<IAPStoreOffer>();

        private void Awake()
        {
            content.GetComponentsInChildren(offers);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void Start()
        {
            IAPManager.SubscribeOnPurchaseModuleInitted(InitOffers);

            Tween.NextFrame(coinsUI.Initialise);
        }

        public override void Initialise()
        {
            
        }

        private void InitOffers()
        {
            foreach (var offer in offers)
            {
                offer.Init();
            }

            IsStoreAvailable = true;

            Tween.NextFrame(() => {

                float height = layout.padding.top + layout.padding.bottom;

                for (int i = 0; i < offers.Count; i++)
                {
                    var offer = offers[i];
                    if (offer.gameObject.activeSelf)
                    {
                        height += offer.Height;
                        if(i != offers.Count - 1) height += layout.spacing;
                    }
                }

                content.sizeDelta = content.sizeDelta.SetY(height);
            });
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);
        }

        public void Hide()
        {
            UIController.HidePage<IAPStoreUI>();
        }

        private void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<IAPStoreUI>();
        }
    }
}