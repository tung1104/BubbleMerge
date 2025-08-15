using UnityEngine;
using Watermelon.BubbleMerge;
using System;
using Watermelon.Map;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [SerializeField] UIController uiController;

        private ParticlesController particlesController;
        private FloatingTextController floatingTextController;
        private CurrenciesController currenciesController;
        private LevelController levelController;
        private TrajectoryController trajectoryController;
        private TutorialController tutorialController;
        private MapBehavior mapBehavior;
        private PUController powerUpController;

        public static event SimpleCallback OnLevelChanged;

        private void Awake()
        {
            instance = this;

            SaveController.Initialise(useAutoSave: false);

            // Cache components
            CacheComponent(out particlesController);
            CacheComponent(out floatingTextController);
            CacheComponent(out currenciesController);
            CacheComponent(out levelController);
            CacheComponent(out trajectoryController);
            CacheComponent(out tutorialController);
            CacheComponent(out mapBehavior);
            CacheComponent(out powerUpController);
        }

        private void Start()
        {
            InitialiseGame();
        }

        public void InitialiseGame()
        {
            uiController.Initialise();

            particlesController.Initialise();
            floatingTextController.Inititalise();
            currenciesController.Initialise();
            trajectoryController.Init();
            powerUpController.Initialise();
            tutorialController.Initialise();

            uiController.InitialisePages();

            // Display default page
            UIController.ShowPage<UIMainMenu>();

            levelController.Init();
            mapBehavior.Init();

            // Move this method to the point when the game is fully loaded
            GameLoading.MarkAsReadyToHide();

#if UNITY_EDITOR
            CheckIfNeedToAutoRunLevel();
#endif
        }

        public static void OnLevelStart(int levelId)
        {
            SaveController.LevelId = levelId;
            OnLevelChanged?.Invoke();
            LevelController.LoadLevel(levelId);

            instance.mapBehavior.Disable();

            UIController.HidePage<UIMainMenu>();
            UIController.ShowPage<UIGame>();
        }

        public static void OnLevelCompleted()
        {
            UIController.ShowPage<UIComplete>();
            UIController.HidePage<UIGame>();

            SaveController.LevelId++;

            if (SaveController.LevelId > LevelController.MaxLevelReached)
                LevelController.MaxLevelReached = SaveController.LevelId;
        }

        public static void NextLevel()
        {
            SaveController.MarkAsSaveIsRequired();
            LevelController.LoadLevel(SaveController.LevelId);

            OnLevelChanged?.Invoke();
        }

        public static void OnLevelFailed()
        {
            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIGameOver>();

            LivesManager.RemoveLife();
        }

        public static void ReplayLevel()
        {
            UIController.HidePage<UIGameOver>();
            UIController.ShowPage<UIGame>();
            LevelController.LoadLevel(SaveController.LevelId);
        }

        public static void CloseLevel()
        {
            LevelController.CloseLevel();
            instance.mapBehavior.Init();
            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIMainMenu>();
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SaveController.LevelId++;
                if (SaveController.LevelId > LevelController.MaxLevelReached)
                    LevelController.MaxLevelReached = SaveController.LevelId;

                GameController.OnLevelManuallyChanged();
                LevelController.LoadLevel(SaveController.LevelId);

                SaveController.MarkAsSaveIsRequired();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SaveController.LevelId = Mathf.Clamp(SaveController.LevelId - 1, 0, int.MaxValue);
                GameController.OnLevelManuallyChanged();
                LevelController.LoadLevel(SaveController.LevelId);

                SaveController.MarkAsSaveIsRequired();
            }
        }
#endif

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion

        #region Dev

        public static void OnLevelManuallyChanged()
        {
            OnLevelChanged?.Invoke();
        }

#if UNITY_EDITOR

        private static readonly string AUTO_RUN_LEVEL_SAVE_NAME = "auto run level editor";

        public static bool AutoRunLevelInEditor
        {
            get { return EditorPrefs.GetBool(AUTO_RUN_LEVEL_SAVE_NAME, false); }
            set { EditorPrefs.SetBool(AUTO_RUN_LEVEL_SAVE_NAME, value); }
        }

        private void CheckIfNeedToAutoRunLevel()
        {
            if (AutoRunLevelInEditor)
                OnLevelStart(SaveController.LevelId);

            AutoRunLevelInEditor = false;
        }
#endif

        #endregion
    }
}