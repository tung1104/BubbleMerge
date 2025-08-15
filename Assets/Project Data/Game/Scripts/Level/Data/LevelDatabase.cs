using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon.BubbleMerge
{
    [CreateAssetMenu(fileName = "Level Database", menuName = "Content/Data/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] Level[] levels;
        [SerializeField] EvolutionBranch[] branches;
        [SerializeField] LevelItem[] items;
        [SerializeField] LevelShape[] levelShapes;
        [SerializeField] LevelBackground[] levelBackgrounds;
        [SerializeField] Sprite[] potionSprites; //used for generation in level editor

        public LevelItem[] Items => items;
        public LevelShape[] LevelShapes => levelShapes;
        public LevelBackground[] LevelBackgrounds => levelBackgrounds;

        public Level GetLevel(int levelId)
        {
            if (levelId < levels.Length)
            {
                return levels[levelId % levels.Length];
            }

            bool found = false;
            int result = 0;

            while (!found)
            {
                result = Random.Range(0, levels.Length);

                if (levels[result].CanBeUsedInRandomizer)
                {
                    return levels[result];
                }
            }

            return levels[0];
        }

        public EvolutionBranch GetBranch(Branch branchType)
        {
            for (int i = 0; i < branches.Length; i++)
            {
                if (branches[i].branch == branchType)
                    return branches[i];
            }

            return null;
        }

        public LevelItem GetItem(Item itemType)
        {
            foreach (LevelItem item in Items)
            {
                if (item.Type == itemType)
                {
                    return item;
                }
            }

            return null;
        }

        public LevelShape GetShape(LevelShapeType levelShapeType)
        {
            foreach (LevelShape item in levelShapes)
            {
                if (item.Type == levelShapeType)
                {
                    return item;
                }
            }

            return null;
        }

        public LevelBackground GetBackground(LevelBackgroundType levelBackType)
        {
            foreach (LevelBackground item in levelBackgrounds)
            {
                if (item.Type == levelBackType)
                {
                    return item;
                }
            }

            return null;
        }
    }
}