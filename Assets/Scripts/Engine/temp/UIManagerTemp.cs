using UnityEngine;

namespace ProjectABC.Engine
{
    public class UIManagerTemp : MonoBehaviour
    {
        public static UIManagerTemp Instance;

        public LayoutInGame DEBUGLayoutInGame;

        private void Awake()
        {
            Instance = this;
        }
    }
}