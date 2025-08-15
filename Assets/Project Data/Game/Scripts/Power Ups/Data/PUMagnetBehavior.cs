using Watermelon.BubbleMerge;

namespace Watermelon
{
    public class PUMagnetBehavior : PUBehavior
    {
        private PUTimer timer;
        private PUMagnetSettings powerUpMagnetSettings;

        public override void Initialise()
        {
            powerUpMagnetSettings = (PUMagnetSettings)settings;

            timer = null;
        }

        public override bool Activate()
        {
            isBusy = true;

            LevelController.SetAttractionSettings(powerUpMagnetSettings.AttractionSettings);

            timer = new PUTimer(powerUpMagnetSettings.MagnetDuration, () =>
            {
                timer = null;
                isBusy = false;

                LevelController.ResetAttractionSettings();
            });

            return true;
        }

        public override PUTimer GetTimer()
        {
            return timer;
        }

        public override void ResetBehavior()
        {
            isBusy = false;

            if(timer != null)
            {
                timer.Disable();
                timer = null;
            }

            LevelController.ResetAttractionSettings();
        }
    }
}
