using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon.IAPStore
{
    public abstract class IAPStoreOffer : MonoBehaviour
    {
        [SerializeField] ProductKeyType productKey;

        [Space]
        [SerializeField] IAPButton purchaseButton;

        private RectTransform rect;
        public float Height => rect.sizeDelta.y;

        private SimpleBoolSave save;
        protected bool Bought => !save.Value;

#if MODULE_IAP
        private Product product;
#endif

        protected virtual void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Init()
        {
            save = SaveController.GetSaveObject<SimpleBoolSave>($"Product_{productKey}");

#if MODULE_IAP
            product = IAPManager.GetProduct(productKey);

            if (product.receipt != null || product.definition.type == UnityEngine.Purchasing.ProductType.NonConsumable && save.Value)
            {
                ReapplyOffer();
                if (product.definition.type == UnityEngine.Purchasing.ProductType.NonConsumable)
                    Disable();
                else
                    purchaseButton.Init(productKey);
            }
            else
            {
                purchaseButton.Init(productKey);
                IAPManager.OnPurchaseComplete += OnPurchaseComplete;
            }
#endif
        }

        public void Disable()
        {
#if MODULE_IAP
            IAPManager.OnPurchaseComplete -= OnPurchaseComplete;
#endif

            gameObject.SetActive(false);
        }

        private void OnPurchaseComplete(ProductKeyType key)
        {
#if MODULE_IAP
            if (productKey == key)
            {
                ApplyOffer();
                if (product.definition.type == UnityEngine.Purchasing.ProductType.NonConsumable) Disable();

                save.Value = true;
            }
#endif
        }

        protected abstract void ApplyOffer();
        protected abstract void ReapplyOffer();
    }
}