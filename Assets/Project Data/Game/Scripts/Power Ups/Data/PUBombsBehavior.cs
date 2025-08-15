﻿using Watermelon.BubbleMerge;

namespace Watermelon
{
    public class PUBombsBehavior : PUBehavior
    {
        private PUBombsSettings bombsSettings;

        private TweenCase[] bombDelaysCases;
        private TweenCase disableTweenCase;

        public override void Initialise()
        {
            bombsSettings = (PUBombsSettings)settings;
        }

        public override bool Activate()
        {
            isBusy = true;

            bombDelaysCases = new TweenCase[bombsSettings.BombsAmount];
            for (int i = 0; i < bombDelaysCases.Length; i++)
            {
                int bombIndex = i;

                bombDelaysCases[bombIndex] = Tween.DelayedCall(0.2f * i, () =>
                {
                    BombBehavior bombBehavior = LevelController.LevelBehavior.SpawnPUBomb();

                    bombDelaysCases[bombIndex] = Tween.DelayedCall(0.15f, () =>
                    {
                        bombBehavior.ForceExplode(0.8f);
                    });
                });
            }

            disableTweenCase = Tween.DelayedCall(2.0f, () =>
            {
                isBusy = false;
            });

            return true;
        }

        public override void ResetBehavior()
        {
            isBusy = false;

            disableTweenCase.KillActive();

            if(!bombDelaysCases.IsNullOrEmpty())
            {
                for (int i = 0; i < bombDelaysCases.Length; i++)
                {
                    bombDelaysCases[i].KillActive();
                }
            }
        }
    }
}
