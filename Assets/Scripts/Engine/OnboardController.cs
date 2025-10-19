using System;
using UnityEngine;
using UnityEngine.Splines;

namespace ProjectABC.Engine
{
    public sealed class OnboardController : MonoBehaviour
    {
        public enum OnboardSide { Own, Other }
        
        [Serializable]
        private class OnboardPoints
        {
            public Transform deckPoint;
            public Transform revealCardPoint;
            public Transform fieldPoint;
            public Transform[] infirmaryPoints;
            public SplineContainer comeToDeckSpline;
        }

        [SerializeField] private OnboardPoints ownSide;
        [SerializeField] private OnboardPoints otherSide;

        private CardOnboard _ownCardOfDeck = null;
        private CardOnboard _otherCardOfDeck = null;

        public void SetCardToDeck(OnboardSide side, bool active)
        {
            switch (side)
            {
                default:
                case OnboardSide.Own:
                    break;
                case OnboardSide.Other:
                    break;
            }
        }

        public void MoveCardsToCardPile(OnboardSide side, int drawCount = 1, float duration = 0.2f)
        {
            OnboardPoints points = GetSide(side);
        }

        public void MoveCardsToHand(OnboardSide side, int drawCount = 1, float duration = 0.2f)
        {
            
        }

        public void MoveCardsToDeck(OnboardSide side, int drawCount = 1, float duration = 0.2f)
        {
            
        }

        public void MoveCardToField(OnboardSide side, float duration = 0.2f)
        {
            
        }

        private OnboardPoints GetSide(OnboardSide side)
        {
            return side == OnboardSide.Own ? ownSide : otherSide;
        }
    }
}
