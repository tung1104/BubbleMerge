#pragma warning disable 649

using UnityEngine;

namespace Watermelon.BubbleMerge
{
    [System.Serializable]
    public class LevelShape
    {
        [SerializeField] private LevelShapeType type;
        [SerializeField] private GameObject prefab;

        public LevelShapeType Type => type;
        public GameObject Prefab => prefab;
    }
}