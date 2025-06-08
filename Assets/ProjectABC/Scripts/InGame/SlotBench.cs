using UnityEngine;

namespace ProjectABC.Core
{
    public class SlotBench : MonoBehaviour
    {
        [SerializeField] private Vector3 _cardSize;
        [SerializeField] private float _cardSpace;

        public Vector3 GetCardSize()
        {
            return _cardSize;
        }

        public float GetCardSpace()
        {
            return _cardSpace;
        }
    }
}