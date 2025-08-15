﻿using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "PU Bombs Settings", menuName = "Content/Power Ups/PU Bombs Settings")]
    public class PUBombsSettings : PUCustomSettings
    {
        [LineSpacer]
        [SerializeField] int bombsAmount = 3;
        public int BombsAmount => bombsAmount;

        public override void Initialise()
        {

        }
    }
}
