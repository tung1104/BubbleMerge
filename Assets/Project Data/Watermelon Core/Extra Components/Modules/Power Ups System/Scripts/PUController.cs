using System.Collections.Generic;
using UnityEngine;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class PUController : MonoBehaviour
    {
        [DrawReference]
        [SerializeField] PUDatabase database;

        private static PUBehavior[] activePowerUps;
        public static PUBehavior[] ActivePowerUps => activePowerUps;

        private static Dictionary<PUType, PUBehavior> powerUpsLink;

        private static PUUIController powerUpsUIController;
        public static PUUIController PowerUpsUIController => powerUpsUIController;

        private Transform behaviorsContainer;

        public static event OnPowerUpUsedCallback OnPowerUpUsed;

        public void Initialise()
        {
            behaviorsContainer = new GameObject("[POWER UPS]").transform;
            behaviorsContainer.gameObject.isStatic = true;

            PUSettings[] powerUpSettings = database.PowerUps;
            activePowerUps = new PUBehavior[powerUpSettings.Length];
            powerUpsLink = new Dictionary<PUType, PUBehavior>();

            for (int i = 0; i < activePowerUps.Length; i++)
            {
                // Initialise power ups
                powerUpSettings[i].InitialiseSave();
                powerUpSettings[i].Initialise();

                // Spawn behavior object 
                GameObject powerUpBehaviorObject = Instantiate(powerUpSettings[i].BehaviorPrefab, behaviorsContainer);
                powerUpBehaviorObject.transform.ResetLocal();

                PUBehavior powerUpBehavior = powerUpBehaviorObject.GetComponent<PUBehavior>();
                powerUpBehavior.InitialiseSettings(powerUpSettings[i]);
                powerUpBehavior.Initialise();

                activePowerUps[i] = powerUpBehavior;

                // Add power up to dictionary
                powerUpsLink.Add(activePowerUps[i].Settings.Type, activePowerUps[i]);
            }

            UIGame gameUI = UIController.GetPage<UIGame>();

            powerUpsUIController = gameUI.PowerUpsUIController;
            powerUpsUIController.Initialise(this);
        }

        public static bool PurchasePowerUp(PUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                if(powerUpBehavior.Settings.HasEnoughCurrency())
                {
                    CurrenciesController.Substract(powerUpBehavior.Settings.CurrencyType, powerUpBehavior.Settings.Price);

                    powerUpBehavior.Settings.Save.Amount += powerUpBehavior.Settings.PurchaseAmount;

                    powerUpsUIController.RedrawPanels();

                    return true;
                }
                else
                {
                    UIController.ShowPage<IAPStoreUI>();

                    return false;
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }

            return false;
        }

        public static void AddPowerUp(PUType powerUpType, int amount)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount += amount;

                powerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void UsePowerUp(PUType powerUpType)
        {
            if(powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                if(!powerUpBehavior.IsBusy)
                {
                    if(powerUpBehavior.Activate())
                    {
                        AudioController.PlaySound(AudioController.Sounds.powerupSound);

                        powerUpBehavior.Settings.Save.Amount--;

                        powerUpsUIController.OnPowerUpUsed(powerUpBehavior);

                        OnPowerUpUsed?.Invoke(powerUpType);
                    }
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void ResetBehaviors()
        {
            for(int i = 0; i < activePowerUps.Length; i++)
            {
                activePowerUps[i].ResetBehavior();
            }
        }

        [Button]
        public void GiveDebugAmount()
        {
            if (!Application.isPlaying) return;

            for(int i = 0; i < activePowerUps.Length; i++)
            {
                activePowerUps[i].Settings.Save.Amount = 5;
            }

            powerUpsUIController.RedrawPanels();
        }

        [Button]
        public void ResetDebugAmount()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < activePowerUps.Length; i++)
            {
                activePowerUps[i].Settings.Save.Amount = 0;
            }

            powerUpsUIController.RedrawPanels();
        }

        public delegate void OnPowerUpUsedCallback(PUType powerUpType);
    }
}
