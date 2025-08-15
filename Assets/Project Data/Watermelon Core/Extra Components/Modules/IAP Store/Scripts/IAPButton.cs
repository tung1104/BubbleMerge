using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon.IAPStore
{
    public class IAPButton : MonoBehaviour
    {
        [SerializeField] Image backImage;
        [SerializeField] Button button;
        [SerializeField] TMP_Text priceText;
        [SerializeField] GameObject loadingObject;

        [Space]
        [SerializeField] Sprite activeBackSprite;
        [SerializeField] Sprite unactiveBackSprite;

        private ProductKeyType key;

#if MODULE_IAP
        private Product product;
#endif

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public void Init(ProductKeyType key)
        {
            Debug.Log("start of update " + key);
            this.key = key;

#if MODULE_IAP
            product = IAPManager.GetProduct(key);
#endif

            UpdateState();
        }

        private void UpdateState()
        {
#if MODULE_IAP
            if (product != null)
            {
                loadingObject.SetActive(false);
                priceText.gameObject.SetActive(true);

                backImage.sprite = activeBackSprite;

                priceText.text = IAPManager.GetProductLocalPriceString(key);
            }
            else
            {
                SetDisabledState();
            }
#else
            SetDisabledState();
#endif
            Debug.Log("end of update " + key);
        }

        private void SetDisabledState()
        {
            loadingObject.SetActive(true);
            priceText.gameObject.SetActive(false);

            backImage.sprite = unactiveBackSprite;
        }

        private void OnButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            IAPManager.BuyProduct(key);
        }
    }
}