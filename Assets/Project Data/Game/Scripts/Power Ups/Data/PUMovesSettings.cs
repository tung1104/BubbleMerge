﻿using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "PU Moves Settings", menuName = "Content/Power Ups/PU Moves Settings")]
    public class PUMovesSettings : PUCustomSettings
    {
        [LineSpacer]
        [SerializeField] int extraTurns = 3;
        public int ExtraTurns => extraTurns;

        public override void Initialise()
        {

        }
    }
}
