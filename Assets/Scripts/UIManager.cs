using UnityEngine;

namespace ProjectABC.InGame
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public LayoutInGame DEBUGLayoutInGame;

        private void Awake()
        {
            Instance = this;
        }
    }
}