using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleMerge;

namespace Watermelon
{
    public abstract class MapLevelAbstractBehavior : MonoBehaviour
    {
        protected static UIMainMenu MainMenu { get; set; }

        [SerializeField] protected Button button;
        [SerializeField] protected TMP_Text levelNumber;

        public int LevelId { get; protected set; }

        protected virtual void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        protected virtual void Start()
        {
            if (MainMenu == null) MainMenu = UIController.GetPage<UIMainMenu>();
        }

        public virtual void Init(int id)
        {
            LevelId = id;
            levelNumber.text = $"{id + 1}";

            if (id < LevelController.MaxLevelReached)
            {
                InitOpen();
            }
            else if (id == LevelController.MaxLevelReached)
            {
                InitCurrent();
            }
            else
            {
                InitClose();
            }
        }

        protected virtual void OnButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (LivesManager.Lives > 0)
            {
                MainMenu.ShowLevelPopup(LevelId);
            }
            else
            {
                UIController.GetPage<UIMainMenu>().ShowAddLivesPopup();
            }

        }

        protected abstract void InitOpen();
        protected abstract void InitClose();
        protected abstract void InitCurrent();
    }
}