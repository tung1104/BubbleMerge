﻿#pragma warning disable 649

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Watermelon.BubbleMerge
{
    public class LevelEditorWindow : LevelEditorBase
    {

        private const string ITEM_ENUM_FILE_PATH = "Assets/Project Data/Game/Scripts/Level/Data/ItemEnum.cs";
        private const string LEVEL_SHAPE_ENUM_FILE_PATH = "Assets/Project Data/Game/Scripts/Level/Data/LevelShapeTypeEnum.cs";
        private const string LEVEL_BACKGROUNDS_ENUM_FILE_PATH = "Assets/Project Data/Game/Scripts/Level/Data/LevelBackgroundType.cs";
        private const string GAME_SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
        private const string EDITOR_SCENE_PATH = "Assets/Project Data/Game/Scenes/LevelEditor.unity";
        private static string EDITOR_SCENE_NAME = "LevelEditor";

        //Window configuration
        private const string TITLE = "Level Editor";
        private const float WINDOW_MIN_WIDTH = 800;
        private const float WINDOW_MIN_HEIGHT = 560;
        private const float WINDOW_MAX_WIDTH = 800;
        private const float WINDOW_MAX_HEIGHT = 700;

        //Level database fields
        private const string LEVELS_PROPERTY_NAME = "levels";
        private const string ITEMS_PROPERTY_NAME = "items";
        private const string BRANCHES_PROPERTY_NAME = "branches";
        private const string LEVEL_SHAPES_PROPERTY_NAME = "levelShapes";
        private const string LEVEL_BACKGROUNDS_PROPERTY_NAME = "levelBackgrounds";
        private const string POTION_SPRITES_PROPERTY_NAME = "potionSprites";
        private SerializedProperty levelsSerializedProperty;
        private SerializedProperty itemsSerializedProperty;
        private SerializedProperty branchesSerializedProperty;
        private SerializedProperty levelShapesSerializedProperty;
        private SerializedProperty levelBackgroundsSerializedProperty;
        private SerializedProperty potionSpritesSerializedProperty;

        //EnumObjectsList 
        private const string TYPE_PROPERTY_PATH = "type";
        private const string PREFAB_PROPERTY_PATH = "prefab";
        private const string EDITOR_TEXTURE_PROPERTY_PATH = "editorTexture";
        private const string LEVELS_SHAPE_ENUM_NAME = "LevelShapeType";
        private const string LEVELS_BACKGROUNDS_ENUM_NAME = "LevelBackgroundType";
        private bool enumCompiling;
        private EnumObjectsList itemsEnumObjectsList;
        private EnumObjectsList levelShapesEnumObjectsList;
        private EnumObjectsList levelSBackgroundsEnumObjectsList;

        //TabHandler
        private TabHandler tabHandler;
        private const string LEVELS_TAB_NAME = "Levels";
        private const string ITEMS_TAB_NAME = "Items";
        private const string LEVEL_SHAPES_TAB_NAME = "Level shapes";
        private const string LEVEL_BACKGROUNDS_TAB_NAME = "Level backgrounds";
        private const string BRANCHES_TAB_NAME = "Branches";
        private const string EDITOR_TAB_NAME = "Editor";

        //sidebar
        private LevelsHandler levelsHandler;
        private LevelRepresentation selectedLevelRepresentation;
        private const int SIDEBAR_WIDTH = 360;
        private const string OPEN_GAME_SCENE_LABEL = "Open \"Game\" scene";

        private const string OPEN_GAME_SCENE_WARNING = "Please make sure you saved changes before swiching scene. Are you ready to proceed?";
        private const string REMOVE_SELECTION = "Remove selection";

        //ItemSave
        private const string POSITION_PROPERTY_PATH = "position";
        private const string ROTATION_PROPERTY_PATH = "rotation";
        private const string SCALE_PROPERTY_PATH = "scale";

        //General
        private const string YES = "Yes";
        private const string CANCEL = "Cancel";
        private const string WARNING_TITLE = "Warning";
        private SerializedProperty tempProperty;
        private string tempPropertyLabel;

        //PlayerPrefs
        private const string PREFS_LEVEL = "editor_level_index";
        private const string PREFS_WIDTH = "editor_sidebar_width";

        //rest of levels tab
        private const string ITEMS_LABEL = "Spawn items:";
        private const string FILE = "file:";
        private const string COMPILING = "Compiling...";
        private const string ITEM_UNASSIGNED_ERROR = "Please assign prefab to this item in \"Items\"  tab.";
        private const string ITEM_ASSIGNED = "This buttton spawns item.";
        private const string TEST_LEVEL = "Test level";

        private const float ITEMS_BUTTON_MAX_WIDTH = 120;
        private const float ITEMS_BUTTON_SPACE = 8;
        private const float ITEMS_BUTTON_WIDTH = 80;
        private const float ITEMS_BUTTON_HEIGHT = 80;
        private const string RENAME_LEVELS = "Rename Levels";
        private const string PLAYMODE_MENU_ITEM_PATH = "Edit/Play";
        private const string PREGENERATE_LEVEL = "Pregenerate Level";
        private const string PREGENERATE_LEVEL_WARNING = "Level fields would be modified. Continue?";
        private const string UPDATE_NOTE = "Update";
        private const string SHAPES_TAB_LABEL = "Shapes:";
        private const string BACKGROUNDS_TAB_LABEL = "Backgrounds:";
        private bool prefabAssigned;
        private GUIContent itemContent;
        private SerializedProperty currentLevelItemProperty;
        private Vector2 levelItemsScrollVector;
        private float itemPosX;
        private float itemPosY;
        private Rect itemsRect;
        private Rect selectedLevelFieldRect;
        private Rect itemRect;
        private int itemsPerRow;
        private int rowCount;
        private GameObject tempGameobject;
        private Item tempItem;
        private Texture2D tempTexture;
        private Vector3 tempPosition;
        private int selectedTab;
        private string[] levelTabs = { "Items", "Fields" };
        private int newLevelIndex;
        private int currentSideBarWidth;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private EditorBehaviorMode backupBehaviourMode;
        private bool lastActiveLevelOpened;
        private Rect levelContentRect;

        protected override string LEVELS_DATABASE_FOLDER_PATH => "Assets/Project Data/Content/Data";

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            builder.KeepWindowOpenOnScriptReload(true);
            builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
            builder.SetContentMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
            builder.SetWindowMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
            return builder.Build();
        }

        public override bool WindowClosedInPlaymode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (window != null)
                {
                    window.Close();
                    OpenScene(GAME_SCENE_PATH);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(Level);
        }

        protected override void ReadLevelDatabaseFields()
        {
            levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            branchesSerializedProperty = levelsDatabaseSerializedObject.FindProperty(BRANCHES_PROPERTY_NAME);
            itemsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(ITEMS_PROPERTY_NAME);
            levelShapesSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVEL_SHAPES_PROPERTY_NAME);
            levelBackgroundsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVEL_BACKGROUNDS_PROPERTY_NAME);
            potionSpritesSerializedProperty = levelsDatabaseSerializedObject.FindProperty(POTION_SPRITES_PROPERTY_NAME);
        }

        protected override void InitialiseVariables()
        {
            enumCompiling = false;
            levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
            levelsHandler.removeElementCallback += RemoveLevelCallback;
            levelsHandler.addElementCallback += AddLevelCallback;
            levelsHandler.CustomList.MinHeight = 400;
            itemsEnumObjectsList = new EnumObjectsList(itemsSerializedProperty, TYPE_PROPERTY_PATH, PREFAB_PROPERTY_PATH, ITEM_ENUM_FILE_PATH, OnBeforeEnumFileupdateCallback);
            itemsEnumObjectsList.EnableTextureField(EDITOR_TEXTURE_PROPERTY_PATH);
            levelShapesEnumObjectsList = new EnumObjectsList(levelShapesSerializedProperty, TYPE_PROPERTY_PATH, PREFAB_PROPERTY_PATH, LEVEL_SHAPE_ENUM_FILE_PATH, OnBeforeEnumFileupdateCallback, LEVELS_SHAPE_ENUM_NAME);
            levelSBackgroundsEnumObjectsList = new EnumObjectsList(levelBackgroundsSerializedProperty, TYPE_PROPERTY_PATH, PREFAB_PROPERTY_PATH, LEVEL_BACKGROUNDS_ENUM_FILE_PATH, OnBeforeEnumFileupdateCallback, LEVELS_BACKGROUNDS_ENUM_NAME);
            levelShapesEnumObjectsList.TabLabel = SHAPES_TAB_LABEL;
            levelSBackgroundsEnumObjectsList.TabLabel = BACKGROUNDS_TAB_LABEL;

            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab(LEVELS_TAB_NAME, DisplayLevelsTab));
            tabHandler.AddTab(new TabHandler.Tab(ITEMS_TAB_NAME, itemsEnumObjectsList.DisplayTab));
            tabHandler.AddTab(new TabHandler.Tab(LEVEL_SHAPES_TAB_NAME, levelShapesEnumObjectsList.DisplayTab));
            tabHandler.AddTab(new TabHandler.Tab(LEVEL_BACKGROUNDS_TAB_NAME, levelSBackgroundsEnumObjectsList.DisplayTab));
            tabHandler.AddTab(new TabHandler.Tab(BRANCHES_TAB_NAME, DisplayBranchesTab));
            tabHandler.AddTab(new TabHandler.Tab(EDITOR_TAB_NAME, DisplayEditorSpritesTab));
            PrefsSettings.InitEditor();
            newLevelIndex = -1;

            currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);
        }

        private void OpenLastActiveLevel()
        {
            if (!lastActiveLevelOpened)
            {
                if ((levelsSerializedProperty.arraySize > 0) && PlayerPrefs.HasKey(PREFS_LEVEL))
                {
                    int levelIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0),0, levelsSerializedProperty.arraySize - 1);
                    levelsHandler.CustomList.SelectedIndex = levelIndex;
                    levelsHandler.OpenLevel(levelIndex);
                }

                lastActiveLevelOpened = true;
            }
        }

        private void AddLevelCallback()
        {
            newLevelIndex = levelsSerializedProperty.arraySize - 1;
        }

        private void RemoveLevelCallback()
        {
            selectedLevelRepresentation = null;
        }

        private void OnBeforeEnumFileupdateCallback()
        {
            enumCompiling = true;
        }

        protected override void Styles()
        {
            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }
        }

        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
            SaveLevelIfPosssible();
            PlayerPrefs.SetInt(PREFS_LEVEL, index);
            PlayerPrefs.Save();
            selectedLevelRepresentation = new LevelRepresentation(levelObject);
            levelsHandler.UpdateCurrentLevelLabel(GetLevelLabel(levelObject, index));
            LoadLevel();
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
            return levelRepresentation.GetLevelLabel(index, stringBuilder);
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
            levelRepresentation.Clear();
        }

        protected override void DrawContent()
        {
            if (EditorSceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                DrawOpenEditorScene();
                return;
            }

            if (enumCompiling)
            {
                EditorGUILayout.LabelField(COMPILING, EditorStylesExtended.label_large_bold);
                return;
            }

            tabHandler.DisplayTab();
        }

        private void DrawOpenEditorScene()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

            if (GUILayout.Button("Open \"" + EDITOR_SCENE_NAME + "\" scene"))
            {
                OpenScene(EDITOR_SCENE_PATH);
            }

            EditorGUILayout.EndVertical();
        }

        private void DisplayLevelsTab()
        {
            OpenLastActiveLevel();
            EditorGUILayout.BeginHorizontal();
            //sidebar 
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(currentSideBarWidth));
            levelsHandler.DisplayReordableList();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            HandleChangingSideBar();

            //level content
            levelContentRect = EditorGUILayout.BeginVertical(GUI.skin.box);
            DisplaySelectedLevel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void HandleChangingSideBar()
        {
            separatorRect = EditorGUILayout.BeginHorizontal(GUI.skin.box,GUILayout.MinWidth(8),GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    levelsHandler.IgnoreDragEvents = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    levelsHandler.IgnoreDragEvents = false;
                    PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
                    PlayerPrefs.Save();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
                    Event.current.Use();
                }
            }
        }

        private void DisplaySidebarButtons()
        {
            if (GUILayout.Button(RENAME_LEVELS, EditorStylesExtended.button_01))
            {
                SaveLevelIfPosssible();
                levelsHandler.RenameLevels();
            }

            if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, EditorStylesExtended.button_01))
            {
                if (EditorUtility.DisplayDialog(WARNING_TITLE, OPEN_GAME_SCENE_WARNING, YES, CANCEL))
                {
                    SaveLevelIfPosssible();
                    OpenScene(GAME_SCENE_PATH);
                }
            }

            if (GUILayout.Button(REMOVE_SELECTION, EditorStylesExtended.button_01))
            {
                SaveLevelIfPosssible();
                levelsHandler.ClearSelection();
                ClearScene();
            }
        }

        private static void ClearScene()
        {
            EditorSceneController.Instance.Clear();
        }

        private void SetAsCurrentLevel()
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();
            SimpleIntSave level = tempSave.GetSaveObject<SimpleIntSave>("levelNumber");
            if (levelsHandler.SelectedLevelIndex > level.Value)
            {
                level.Value = levelsHandler.SelectedLevelIndex;

                tempSave.LevelId = levelsHandler.SelectedLevelIndex;
            }
            else
            {
                tempSave.LevelId = levelsHandler.SelectedLevelIndex;
            }

            SaveController.SaveCustom(tempSave);
        }

        private void DisplaySelectedLevel()
        {
            if (levelsHandler.SelectedLevelIndex == -1)
            {
                return;
            }

            //handle level file field
            EditorGUI.BeginChangeCheck();
            //easy way to know width of available space
            selectedLevelFieldRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.PropertyField(levelsHandler.SelectedLevelProperty, new GUIContent(FILE));
            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                levelsHandler.ReopenLevel();
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(TEST_LEVEL, GUILayout.Width(EditorGUIUtility.labelWidth)))
            {
                SaveLevel();
                SetAsCurrentLevel();
                OpenScene(GAME_SCENE_PATH);
                GameController.AutoRunLevelInEditor = true;
                EditorApplication.ExecuteMenuItem(PLAYMODE_MENU_ITEM_PATH);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(PREGENERATE_LEVEL))
            {
                if (newLevelIndex == levelsHandler.SelectedLevelIndex)
                {
                    PregenerateLevel();
                }
                else if (EditorUtility.DisplayDialog(WARNING_TITLE, PREGENERATE_LEVEL_WARNING, YES, CANCEL))
                {
                    PregenerateLevel();
                }

            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(selectedLevelRepresentation.levelShapeTypeProperty);

            if (EditorGUI.EndChangeCheck())
            {
                LoadLevelShape();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(selectedLevelRepresentation.levelBackTypeProperty);

            if (EditorGUI.EndChangeCheck())
            {
                LoadLevelBackground();
            }

            EditorGUILayout.Space();

            selectedTab = GUILayout.Toolbar(selectedTab, levelTabs);

            if (selectedTab == 0)
            {
                DisplayItemsListSection();
            }
            else
            {
                DisplayLevelFields();
            }

            EditorGUILayout.Space();
            selectedLevelRepresentation.ApplyChanges();
        }



        private void DisplayLevelFields()
        {
            EditorGUILayout.PropertyField(selectedLevelRepresentation.bubblesOnTheFieldAmountProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.turnsLimitProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.coinsRewardProperty);

            selectedLevelRepresentation.DrawRequirementsProperty();
            selectedLevelRepresentation.canBeUsedInRandomizerProperty.boolValue = EditorGUILayout.ToggleLeft(selectedLevelRepresentation.canBeUsedInRandomizerProperty.displayName, selectedLevelRepresentation.canBeUsedInRandomizerProperty.boolValue);
            selectedLevelRepresentation.UpdateNote();
            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
        }

        private void DisplayItemsListSection()
        {
            EditorGUILayout.LabelField(ITEMS_LABEL);
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            //assigning space
            if (itemsSerializedProperty.arraySize != 0)
            {
                itemsPerRow = Mathf.FloorToInt((Screen.width - SIDEBAR_WIDTH - 20) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH));
                rowCount = Mathf.CeilToInt((itemsSerializedProperty.arraySize * 1f) / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }


            backupBehaviourMode = EditorSettings.defaultBehaviorMode;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

            for (int i = 0; i < itemsSerializedProperty.arraySize; i++)
            {
                tempProperty = itemsSerializedProperty.GetArrayElementAtIndex(i);
                tempPropertyLabel = tempProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).enumDisplayNames[tempProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).enumValueIndex];
                prefabAssigned = (tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue != null);
                tempItem = (Item)tempProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).enumValueIndex;
                tempTexture = tempProperty.FindPropertyRelative(EDITOR_TEXTURE_PROPERTY_PATH).objectReferenceValue as Texture2D;

                if(tempTexture == null)
                {
                    if (prefabAssigned)
                    {
                        itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue), ITEM_ASSIGNED);
                    }
                    else
                    {
                        itemContent = new GUIContent(tempPropertyLabel, ITEM_UNASSIGNED_ERROR);
                    }
                }
                else
                {
                    itemContent = new GUIContent(tempTexture);
                }

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > selectedLevelFieldRect.width - 10)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                EditorGUI.BeginDisabledGroup(!prefabAssigned);

                if (GUI.Button(itemRect, itemContent, EditorStylesExtended.button_01))
                {
                    tempGameobject = (GameObject)tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue;
                    

                    EditorSceneController.Instance.Spawn(tempGameobject, Vector3.zero, tempItem);
                }

                EditorGUI.EndDisabledGroup();

                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }

            EditorSettings.defaultBehaviorMode = backupBehaviourMode;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void LoadLevel()
        {
            EditorSceneController.Instance.Clear();

            LoadLevelShape();
            LoadLevelBackground();
            LoadLevelItems();
        }

        private void LoadLevelShape()
        {
            LevelShapeType shapeType = (LevelShapeType)selectedLevelRepresentation.levelShapeTypeProperty.intValue;
            GameObject prefab = GetLevelShapePrefab(shapeType);

            if (prefab != null)
            {
                EditorSceneController.Instance.SpawnLevelShape(prefab);
            }
        }

        private void LoadLevelBackground()
        {
            LevelBackgroundType backgroundType = (LevelBackgroundType)selectedLevelRepresentation.levelBackTypeProperty.intValue;
            GameObject prefab = GetLevelBackgroundPrefab(backgroundType);

            if (prefab != null)
            {
                EditorSceneController.Instance.SpawnLevelBackground(prefab);
            }
        }

        private void LoadLevelItems()
        {
            ItemSave tempItemSave;

            for (int i = 0; i < selectedLevelRepresentation.itemsProperty.arraySize; i++)
            {
                tempItemSave = PropertyToItemSave(i);
                EditorSceneController.Instance.Spawn(tempItemSave, GetItemPrefab(tempItemSave.Type));
            }
        }

        private void SaveLevel()
        {
            SaveLevelItems();

            selectedLevelRepresentation.UpdateNote();
            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
            AssetDatabase.SaveAssets();
        }

        private void SaveLevelItems()
        {
            ItemSave[] levelItems = EditorSceneController.Instance.GetLevelItems();
            selectedLevelRepresentation.itemsProperty.arraySize = levelItems.Length;

            for (int i = 0; i < levelItems.Length; i++)
            {
                ItemSaveToProperty(levelItems[i], i);
            }
        }

        private void ItemSaveToProperty(ItemSave levelItem, int index)
        {
            currentLevelItemProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(index);
            currentLevelItemProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).intValue = (int)levelItem.Type;
            currentLevelItemProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = levelItem.Position;
            currentLevelItemProperty.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value = levelItem.Rotation;
            currentLevelItemProperty.FindPropertyRelative(SCALE_PROPERTY_PATH).vector3Value = levelItem.Scale;
        }

        private ItemSave PropertyToItemSave(int index)
        {
            currentLevelItemProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(index);
            return new ItemSave(
                (Item)currentLevelItemProperty.FindPropertyRelative(TYPE_PROPERTY_PATH).intValue,
                currentLevelItemProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value,
                currentLevelItemProperty.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value,
                currentLevelItemProperty.FindPropertyRelative(SCALE_PROPERTY_PATH).vector3Value);
        }

        private GameObject GetItemPrefab(Item item)
        {
            for (int i = 0; i < itemsSerializedProperty.arraySize; i++)
            {
                if ((Item)itemsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue == item)
                {
                    return (GameObject)itemsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue;
                }
            }

            Debug.LogError("GetItemPrefab element not found");
            return null;
        }

        private GameObject GetLevelShapePrefab(LevelShapeType levelShapeType)
        {
            for (int i = 0; i < levelShapesSerializedProperty.arraySize; i++)
            {
                if ((LevelShapeType)levelShapesSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue == levelShapeType)
                {
                    return (GameObject)levelShapesSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue;
                }
            }

            Debug.LogError("GetLevelShapePrefab element not found");
            return null;
        }

        private GameObject GetLevelBackgroundPrefab(LevelBackgroundType levelBackgroundType)
        {
            for (int i = 0; i < levelShapesSerializedProperty.arraySize; i++)
            {
                if ((LevelBackgroundType)levelBackgroundsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue == levelBackgroundType)
                {
                    return (GameObject)levelBackgroundsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue;
                }
            }

            Debug.LogError("GetLevelShapePrefab element not found");
            return null;
        }

        private void DisplayBranchesTab()
        {
            EditorGUILayout.PropertyField(branchesSerializedProperty);
        }

        private void DisplayEditorSpritesTab()
        {
            EditorGUILayout.PropertyField(potionSpritesSerializedProperty);
        }

        private void SaveLevelIfPosssible()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                return;
            }

            if (selectedLevelRepresentation == null)
            {
                return;
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return;
            }

            try
            {
                SaveLevel();
            }
            catch
            {

            }

            levelsHandler.SetLevelLabels();
        }

        private void OnDestroy()
        {
            SaveLevelIfPosssible();

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                OpenScene(GAME_SCENE_PATH);
            }
        }

        private void PregenerateLevel()
        {

            selectedLevelRepresentation.levelShapeTypeProperty.enumValueIndex = UnityEngine.Random.Range(0, levelShapesSerializedProperty.arraySize);
            selectedLevelRepresentation.levelBackTypeProperty.enumValueIndex = UnityEngine.Random.Range(0, levelBackgroundsSerializedProperty.arraySize);
            selectedLevelRepresentation.bubblesOnTheFieldAmountProperty.intValue = UnityEngine.Random.Range(5, 10);
            selectedLevelRepresentation.turnsLimitProperty.intValue = UnityEngine.Random.Range(32, 45);

            float random = UnityEngine.Random.Range(0, 1f);
            int ingredientsAmount = 1;

            if (random < 0.3f)
            {
                ingredientsAmount = 2;
            }
            else if (random < 0.95f)
            {
                ingredientsAmount = 3;
            }
            else
            {
                ingredientsAmount = 4;
            }

            selectedLevelRepresentation.CreateRandomRecipe(ingredientsAmount);
            int randomIndex = UnityEngine.Random.Range(0, potionSpritesSerializedProperty.arraySize);
            selectedLevelRepresentation.AssignPotionSprite(potionSpritesSerializedProperty.GetArrayElementAtIndex(randomIndex).objectReferenceValue);

            // 30% chance of ice appear
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                selectedLevelRepresentation.SetIce(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(4, 10));
            }
            else
            {
                selectedLevelRepresentation.SetIce(0, 0);
            }

            // 30% chance of box appear
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                selectedLevelRepresentation.SetBoxes(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4));
            }
            else
            {
                selectedLevelRepresentation.SetBoxes(0, 0);
            }

            // 30% chance of bombs appear
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                selectedLevelRepresentation.SetBombs(UnityEngine.Random.Range(2, 5), UnityEngine.Random.Range(0, 8));
            }
            else
            {
                selectedLevelRepresentation.SetBombs(0, 0);
            }

            selectedTab = 1;
            LoadLevelShape();
            LoadLevelBackground();

            selectedLevelRepresentation.UpdateNote();
            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
        }


        protected class LevelRepresentation : LevelRepresentationBase
        {

            private const string NOTE_PROPERTY_NAME = "note";

            private const string BUBBLES_ON_THE_FIELD_PROPERTY_NAME = "bubblesOnTheFieldAmount";
            private const string TURNS_LIMIT_PROPERTY_NAME = "turnsLimit";
            private const string COINS_REWARD_PROPERTY_NAME = "coinsReward";
            private const string REQUIREMENTS_PROPERTY_NAME = "requirements";
            private const string CAN_BE_USED_IN_RANDOMIZER_PROPERTY_NAME = "canBeUsedInRandomizer";

            private const string ITEMS_PROPERTY_NAME = "items";
            private const string BOMBS_DATA_PROPERTY_NAME = "bombsData";
            private const string LEVEL_SHAPE_TYPE_PROPERTY_NAME = "levelShapeType";
            private const string LEVEL_BACK_TYPE_PROPERTY_NAME = "levelBackType";

            private const string RECIPE_PROPERTY_NAME = "recipe";
            private const string RESULT_PREVIEW_PROPERTY_NAME = "ResultPreview";
            private const string INGRIDIENTS_PROPERTY_NAME = "Ingridients";
            private const string ICE_BUBBLES_PER_LEVEL_PROPERTY_NAME = "iceBubblesPerLevel";
            private const string ICE_BUBBLES_HEALTH_PROPERTY_NAME = "iceBubblesHealth";
            private const string BOXES_PER_LEVEL_PROPERTY_NAME = "boxesPerLevel";
            private const string BOXES_HEALTH_PROPERTY_NAME = "boxesHealth";

            private const string BRANCH_PROPERTY_NAME = "branch";
            private const string STAGE_ID_PROPERTY_NAME = "stageId";
            private const string AMOUNT_PROPERTY_NAME = "amount";
            private const string MOVES_TO_SPAWN_PROPERTY = "movesToSpawn";
            private const string BOMBS_LIST_HEADER = "Bombs";
            public SerializedProperty noteProperty;

            public SerializedProperty bubblesOnTheFieldAmountProperty;
            public SerializedProperty turnsLimitProperty;
            public SerializedProperty coinsRewardProperty;
            public SerializedProperty requirementsProperty;
            public SerializedProperty canBeUsedInRandomizerProperty;
            public SerializedProperty itemsProperty;
            public SerializedProperty levelShapeTypeProperty;
            public SerializedProperty levelBackTypeProperty;

            //temp
            SerializedProperty ingredientsProperty;
            SerializedProperty ingredientProperty;
            SerializedProperty spriteProperty;

            ReorderableList bombsList;

            //this empty constructor is nessesary
            public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
            {
            }


            protected override void ReadFields()
            {
                noteProperty = serializedLevelObject.FindProperty(NOTE_PROPERTY_NAME);
                bubblesOnTheFieldAmountProperty = serializedLevelObject.FindProperty(BUBBLES_ON_THE_FIELD_PROPERTY_NAME);
                coinsRewardProperty = serializedLevelObject.FindProperty(COINS_REWARD_PROPERTY_NAME);
                turnsLimitProperty = serializedLevelObject.FindProperty(TURNS_LIMIT_PROPERTY_NAME);
                requirementsProperty = serializedLevelObject.FindProperty(REQUIREMENTS_PROPERTY_NAME);
                canBeUsedInRandomizerProperty = serializedLevelObject.FindProperty(CAN_BE_USED_IN_RANDOMIZER_PROPERTY_NAME);
                itemsProperty = serializedLevelObject.FindProperty(ITEMS_PROPERTY_NAME);
                levelShapeTypeProperty = serializedLevelObject.FindProperty(LEVEL_SHAPE_TYPE_PROPERTY_NAME);
                levelBackTypeProperty = serializedLevelObject.FindProperty(LEVEL_BACK_TYPE_PROPERTY_NAME);

                InitBombsList();
            }

            

            public override void Clear()
            {
                if (!NullLevel)
                {
                    noteProperty.stringValue = string.Empty;
                    bubblesOnTheFieldAmountProperty.intValue = 0;
                    coinsRewardProperty.intValue = 20;
                    turnsLimitProperty.intValue = 0;
                    canBeUsedInRandomizerProperty.boolValue = true;
                    itemsProperty.arraySize = 0;
                    levelShapeTypeProperty.enumValueIndex = 0;
                    levelBackTypeProperty.enumValueIndex = 0;

                    requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(RESULT_PREVIEW_PROPERTY_NAME).objectReferenceValue = null;
                    requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(INGRIDIENTS_PROPERTY_NAME).arraySize = 0;
                    requirementsProperty.FindPropertyRelative(ICE_BUBBLES_PER_LEVEL_PROPERTY_NAME).intValue = 0;
                    requirementsProperty.FindPropertyRelative(ICE_BUBBLES_HEALTH_PROPERTY_NAME).intValue = 0;
                    requirementsProperty.FindPropertyRelative(BOXES_PER_LEVEL_PROPERTY_NAME).intValue = 0;
                    requirementsProperty.FindPropertyRelative(BOXES_HEALTH_PROPERTY_NAME).intValue = 0;
                    requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).arraySize = 0;

                    ApplyChanges();
                }
            }

            public void UpdateNote()
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(levelShapeTypeProperty.enumNames[levelShapeTypeProperty.enumValueIndex]);

                GetItemsString(stringBuilder);

                if (requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).arraySize > 0)
                {
                    stringBuilder.Append(" + x" + requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).arraySize + " ");
                    stringBuilder.Append("Bomb");
                }

                if (requirementsProperty.FindPropertyRelative(ICE_BUBBLES_PER_LEVEL_PROPERTY_NAME).intValue > 0)
                {
                    stringBuilder.Append(" + x" + requirementsProperty.FindPropertyRelative(ICE_BUBBLES_PER_LEVEL_PROPERTY_NAME).intValue + " ");
                    stringBuilder.Append("Ice(");
                    stringBuilder.Append(requirementsProperty.FindPropertyRelative(ICE_BUBBLES_HEALTH_PROPERTY_NAME).intValue);
                    stringBuilder.Append("hp)");
                }

                if (requirementsProperty.FindPropertyRelative(BOXES_PER_LEVEL_PROPERTY_NAME).intValue > 0)
                {
                    stringBuilder.Append(" + x" + requirementsProperty.FindPropertyRelative(BOXES_PER_LEVEL_PROPERTY_NAME).intValue + " ");
                    stringBuilder.Append("Box(");
                    stringBuilder.Append(requirementsProperty.FindPropertyRelative(BOXES_HEALTH_PROPERTY_NAME).intValue);
                    stringBuilder.Append("hp) ");
                }

                noteProperty.stringValue = stringBuilder.ToString();
                ApplyChanges();
            }

            public void GetItemsString(StringBuilder stringBuilder)
            {
                if (itemsProperty.arraySize == 0)
                {
                    return;
                }

                stringBuilder.Append(" + ");

                int currentType;
                int previousType = -1;
                int counter = 0;
                SerializedProperty type = null;

                for (int i = 0; i < itemsProperty.arraySize; i++)
                {
                    type = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH);
                    currentType = type.enumValueIndex;

                    if (currentType == previousType)
                    {
                        counter++;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            previousType = currentType;
                            counter = 1;
                        }
                        else
                        {
                            stringBuilder.Append("x" + counter + " ");
                            stringBuilder.Append(type.enumNames[previousType]);
                            stringBuilder.Append(" + ");
                            previousType = currentType;
                            counter = 1;
                        }
                    }
                }

                stringBuilder.Append("x" + counter + " ");
                stringBuilder.Append(type.enumNames[previousType]);
            }

            public void GetIngredientsString(StringBuilder stringBuilder)
            {
                stringBuilder.Append('(');
                ingredientsProperty = requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(INGRIDIENTS_PROPERTY_NAME);
                SerializedProperty branchProperty;

                for (int i = 0; i < ingredientsProperty.arraySize; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append('+');
                    }

                    branchProperty = ingredientsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(BRANCH_PROPERTY_NAME);
                    string[] words = branchProperty.enumDisplayNames[branchProperty.enumValueIndex].Split('_');

                    for (int j = 0; j < words.Length; j++)
                    {
                        if (j > 0)
                        {
                            stringBuilder.Append('_');
                        }

                        stringBuilder.Append(words[j].Substring(0, 2));
                    }
                }

                stringBuilder.Append(')');
            }

            public void DrawRequirementsProperty()
            {
                ingredientsProperty = requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(INGRIDIENTS_PROPERTY_NAME);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                ingredientsProperty.arraySize = EditorGUILayout.IntField("Ingredients count", ingredientsProperty.arraySize);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(100f));
                EditorGUILayout.LabelField("Sprite", GUILayout.MaxWidth(100f));
                spriteProperty = requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(RESULT_PREVIEW_PROPERTY_NAME);
                spriteProperty.objectReferenceValue = EditorGUILayout.ObjectField(spriteProperty.objectReferenceValue, typeof(Sprite), false, GUILayout.MinHeight(64f));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.MinWidth(150f), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(BRANCH_PROPERTY_NAME, GUILayout.MaxWidth(50f));

                for (int i = 0; i < ingredientsProperty.arraySize; i++)
                {
                    ingredientProperty = ingredientsProperty.GetArrayElementAtIndex(i);
                    ingredientProperty.FindPropertyRelative(BRANCH_PROPERTY_NAME).intValue = (int)((Branch)EditorGUILayout.EnumPopup((Branch)ingredientProperty.FindPropertyRelative(BRANCH_PROPERTY_NAME).intValue));
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(50f));
                EditorGUILayout.LabelField(STAGE_ID_PROPERTY_NAME, GUILayout.MaxWidth(50f));

                for (int i = 0; i < ingredientsProperty.arraySize; i++)
                {
                    ingredientProperty = ingredientsProperty.GetArrayElementAtIndex(i);
                    ingredientProperty.FindPropertyRelative(STAGE_ID_PROPERTY_NAME).intValue = EditorGUILayout.IntField(ingredientProperty.FindPropertyRelative(STAGE_ID_PROPERTY_NAME).intValue);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(50f));
                EditorGUILayout.LabelField(AMOUNT_PROPERTY_NAME, GUILayout.MaxWidth(50f));

                for (int i = 0; i < ingredientsProperty.arraySize; i++)
                {
                    ingredientProperty = ingredientsProperty.GetArrayElementAtIndex(i);
                    ingredientProperty.FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue = EditorGUILayout.IntField(ingredientProperty.FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(requirementsProperty.FindPropertyRelative(ICE_BUBBLES_PER_LEVEL_PROPERTY_NAME));
                EditorGUILayout.PropertyField(requirementsProperty.FindPropertyRelative(ICE_BUBBLES_HEALTH_PROPERTY_NAME));
                EditorGUILayout.PropertyField(requirementsProperty.FindPropertyRelative(BOXES_PER_LEVEL_PROPERTY_NAME));
                EditorGUILayout.PropertyField(requirementsProperty.FindPropertyRelative(BOXES_HEALTH_PROPERTY_NAME));
                bombsList.DoLayoutList();
                EditorGUILayout.EndVertical();
            }

            public void CreateRandomRecipe(int ingredientsAmount)
            {
                ingredientsProperty = requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(INGRIDIENTS_PROPERTY_NAME);
                ingredientsProperty.arraySize = ingredientsAmount;

                int enumSize = Enum.GetValues(typeof(Branch)).Length;
                List<int> list = new List<int>();

                for (int i = 0; i < enumSize; i++)
                {
                    list.Add(i);
                }

                list.Shuffle();


                for (int i = 0; i < ingredientsAmount; i++)
                {
                    ingredientProperty = ingredientsProperty.GetArrayElementAtIndex(i);
                    ingredientProperty.FindPropertyRelative(BRANCH_PROPERTY_NAME).enumValueIndex = list[i];
                    ingredientProperty.FindPropertyRelative(STAGE_ID_PROPERTY_NAME).intValue = 2;
                    ingredientProperty.FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue = UnityEngine.Random.Range(2, 5);
                }
            }

            public void AssignPotionSprite(UnityEngine.Object objectReferenceValue)
            {
                requirementsProperty.FindPropertyRelative(RECIPE_PROPERTY_NAME).FindPropertyRelative(RESULT_PREVIEW_PROPERTY_NAME).objectReferenceValue = objectReferenceValue;
            }

            public void SetIce(int health, int amount)
            {
                requirementsProperty.FindPropertyRelative(ICE_BUBBLES_HEALTH_PROPERTY_NAME).intValue = health;
                requirementsProperty.FindPropertyRelative(ICE_BUBBLES_PER_LEVEL_PROPERTY_NAME).intValue = amount;
            }

            public void SetBoxes(int health, int amount)
            {
                requirementsProperty.FindPropertyRelative(BOXES_HEALTH_PROPERTY_NAME).intValue = health;
                requirementsProperty.FindPropertyRelative(BOXES_PER_LEVEL_PROPERTY_NAME).intValue = amount;
            }

            internal void SetBombs(int amount, int firstBombDelay)
            {
                requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).arraySize = amount;
                int currentDelay = firstBombDelay;

                for (int i = 0; i < requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).arraySize; i++)
                {
                    requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).GetArrayElementAtIndex(i).FindPropertyRelative(MOVES_TO_SPAWN_PROPERTY).intValue = currentDelay;
                    currentDelay += UnityEngine.Random.Range(3, 7);
                }
            }

            private void InitBombsList()
            {
                bombsList = new ReorderableList(serializedLevelObject,requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME),true,true,true,true);
                bombsList.drawElementCallback += BombsDrawElementCallback;
                bombsList.drawHeaderCallback += BombsDrawHeaderCallback;
                bombsList.elementHeight = EditorGUIUtility.singleLineHeight;
            }

            private void BombsDrawHeaderCallback(Rect rect)
            {
                EditorGUI.LabelField(rect, BOMBS_LIST_HEADER);
            }

            private void BombsDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                EditorGUI.PropertyField(rect, requirementsProperty.FindPropertyRelative(BOMBS_DATA_PROPERTY_NAME).GetArrayElementAtIndex(index).FindPropertyRelative(MOVES_TO_SPAWN_PROPERTY));
            }



            public override string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                if (NullLevel)
                {
                    return base.GetLevelLabel(index, stringBuilder);
                }
                else
                {
                    UpdateNote();
                    stringBuilder.Clear();
                    stringBuilder.Append(NUMBER);
                    stringBuilder.Append(index + 1);
                    stringBuilder.Append(SEPARATOR);
                    stringBuilder.Append(noteProperty.stringValue);
                    return stringBuilder.ToString();
                }
            }

            
        }
    }
}

// -----------------
// Scene interraction level editor V1.6
// -----------------

// Changelog
// v 1.6
// • Replaced "Set as current level" function with playtest level
// • Added autosave
// • Updated object preview
// • Added USE_LEVEL_EDITOR_SCENE bool
// • Small fix for EditorSceneController
// v 1.5
// • Updated Spawner tool
// • Updated list
// v 1.5
// • Added Building tool
// • Updated list
// v 1.4
// • Updated EnumObjectlist
// • Updated object preview
// v 1.4
// • Updated EnumObjectlist
// • Fixed bug with window size
// v 1.3
// • Updated EnumObjectlist
// • Added StartPointHandles script that can be added to gameobjects
// v 1.2
// • Reordered some methods
// v 1.1
// • Added spawner tool
// v 1 basic version works
