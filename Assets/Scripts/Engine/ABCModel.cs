using System;

namespace ProjectABC.Engine
{
    [Serializable]
    public class ABCModel
    {
        public CardObjectSpawner cardObjectSpawner;
        public OnboardController onboardController;
        public PlayerController player;
    }
}
