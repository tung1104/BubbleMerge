using System.Collections.Generic;
using UnityEngine;
using Watermelon.BubbleMerge;

namespace Watermelon.Map
{
    public class MapChankBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] GameObject bottom;

        [SerializeField] List<MapLevelBehavior> levels;
        public int LevelsCount => levels.Count;

        public MapBehavior Map { get; private set; }
        public float Height => background.size.y * transform.localScale.y;

        public float CurrentLevelPosition { get; private set; }
        public float Position { get; private set; }
        public int StartLevelCount { get; private set; }

        public void SetPosition(float y)
        {
            Position = y;
            transform.SetPositionY(y * Map.Height + Height / 2 + Camera.main.transform.position.y - Map.Height / 2);
        }

        public void SetMap(MapBehavior map)
        {
            Map = map;
        }

        public void Init(int startLevelCount)
        {
            StartLevelCount = startLevelCount;

            CurrentLevelPosition = -1;

            transform.localScale = Vector3.one * Map.Width / background.size.x;
            
            for(int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                var levelId = startLevelCount + i;

                level.Init(levelId);

                if(levelId == LevelController.MaxLevelReached)
                {
                    CurrentLevelPosition = (level.transform.position.y + Height / 2);
                }
            }

            background.receiveShadows = true;

            if(bottom != null) bottom.SetActive(startLevelCount == 0);
        }
    }
}

