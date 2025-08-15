using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using Watermelon.BubbleMerge;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [SerializeField] UIFade backgroundFade;

        [Space]
        [SerializeField] UIScalableObject levelCompleteLabel;

        [Space]
        [SerializeField] CurrencyUIPanelSimple currencyPanel;
        [SerializeField] UIScalableObject rewardLabel;
        [SerializeField] TextMeshProUGUI rewardAmountText;

        [Space]
        [SerializeField] Image resultImage;

        [Space]
        [SerializeField] UIFade multiplyRewardButtonFade;
        [SerializeField] Button multiplyRewardButton;
        [SerializeField] UIFade noThanksButtonText;
        [SerializeField] Button noThanksButton;
        [SerializeField] UIFade continueButtonFade;
        [SerializeField] Button continueButton;
        [SerializeField] Button quitInMenuButton;

        public static float HideDuration => 0.25f;

        private TweenCase noThanksAppearTween;
        private int coinsHash = FloatingCloud.StringToHash("Coins");

        private int currentReward = 0;

        public override void Initialise()
        {
            multiplyRewardButton.onClick.AddListener(MultiplyRewardButton);
            noThanksButton.onClick.AddListener(ContinueButton);
            continueButton.onClick.AddListener(ContinueButton);
            quitInMenuButton.onClick.AddListener(QuitInMenuButton);

            currencyPanel.Initialise();
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            multiplyRewardButton.interactable = true;
            noThanksButtonText.Hide(immediately: true);
            noThanksButton.interactable = false;
            continueButtonFade.Hide(immediately: true);

            backgroundFade.Show(duration: 0.3f);
            levelCompleteLabel.Show();

            resultImage.sprite = LevelController.Level.Requirements.Recipe.ResultPreview;

            continueButtonFade.Show(0.3f);

            currentReward = LevelController.Level.CoinsReward;

            ShowRewardLabel(currentReward, false, 0.3f, delegate
            {
                rewardLabel.RectTransform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                {
                    FloatingCloud.SpawnCurrency(coinsHash, rewardLabel.RectTransform, currencyPanel.RectTransform, 10, "", () =>
                    {
                        CurrenciesController.Add(CurrencyType.Coin, currentReward);

                        multiplyRewardButtonFade.Show();
                    });
                });
            });
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(HideDuration, false);

            Tween.DelayedCall(HideDuration, delegate
            {
                canvas.enabled = false;
                isPageDisplayed = false;

                UIController.OnPageClosed(this);
            });
        }
        #endregion

        #region RewardLabel

        public void ShowRewardLabel(float rewardAmounts, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately);

            if (immediately)
            {
                rewardAmountText.text = "+" + rewardAmounts;
                onComplted?.Invoke();

                return;
            }

            rewardAmountText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmounts, duration, (float value) =>
            {

                rewardAmountText.text = "+" + (int)value;
            }).OnComplete(delegate
            {

                onComplted?.Invoke();
            });
        }

        #endregion

        #region Buttons

        public void MultiplyRewardButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (noThanksAppearTween != null && noThanksAppearTween.isActive)
            {
                noThanksAppearTween.Kill();
            }

            AdsManager.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    int rewardMult = 3;

                    noThanksButton.interactable = false;
                    noThanksButtonText.Hide(immediately: true);
                    multiplyRewardButtonFade.Hide(immediately: true);
                    multiplyRewardButton.interactable = false;

                    ShowRewardLabel(currentReward * rewardMult, false, 0.3f, delegate
                    {
                        FloatingCloud.SpawnCurrency(coinsHash, rewardLabel.RectTransform, currencyPanel.RectTransform, 10, "", () =>
                        {
                            CurrenciesController.Add(CurrencyType.Coin, currentReward * rewardMult);

                            noThanksButton.interactable = true;
                            continueButton.gameObject.SetActive(true);
                            continueButtonFade.Show();
                        });
                    });
                }
                else
                {
                    ContinueButton();
                }
            });
        }

        public void ContinueButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            GameController.NextLevel();

            UIController.HidePage<UIComplete>();
        }

        public void QuitInMenuButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            GameController.CloseLevel();
            UIController.HidePage<UIComplete>();
            UIController.ShowPage<UIMainMenu>();

            LivesManager.AddLife();
        }

        #endregion
    }
}
