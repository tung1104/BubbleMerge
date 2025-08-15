using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.BubbleMerge
{
    public class UILevelQuitPopUp : MonoBehaviour
    {
        [SerializeField] Button closeSmallButton;
        [SerializeField] Button closeBigButton;
        [SerializeField] Button confirmButton;

        private void Awake()
        {
            closeSmallButton.onClick.AddListener(UIController.GamePage.ExitPopCloseButton);
            closeBigButton.onClick.AddListener(UIController.GamePage.ExitPopCloseButton);
            confirmButton.onClick.AddListener(UIController.GamePage.ExitPopUpConfirmExitButton);
        }

    }
}
