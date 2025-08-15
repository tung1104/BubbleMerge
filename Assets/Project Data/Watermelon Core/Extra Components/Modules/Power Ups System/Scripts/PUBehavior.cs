using System;
using UnityEngine;

namespace Watermelon
{
    public abstract class PUBehavior : MonoBehaviour
    {
        protected PUSettings settings;
        public PUSettings Settings => settings;

        protected bool isBusy;
        public bool IsBusy => isBusy;

        public void InitialiseSettings(PUSettings settings)
        {
            this.settings = settings;
        }

        public abstract void Initialise();

        public abstract bool Activate();

        public virtual string GetFloatingMessage()
        {
            return settings.FloatingMessage;
        }

        public virtual PUTimer GetTimer()
        {
            return null;
        }

        public virtual void ResetBehavior()
        {

        }
    }
}
