using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
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

        public async Task SetCardsToDeckPileAsync(OnboardSide side, int cardsAmount, float delay, float duration, CancellationToken token = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay), token);

            var boardPoints = GetSide(side);

            string assetPath = GameConst.AssetPath.CARD_ONBOARD;
            CardSpawnArgs args = new CardSpawnArgs(boardPoints.comeToDeckSpline.transform);

            float interval = duration / cardsAmount;
            for (int i = 0; i < cardsAmount; i++)
            {
                var card = Simulator.Model.cardObjectSpawner.Spawn<CardOnboard>(assetPath, args);

                try
                {
                    await card.MoveFollowingSplineAsync(boardPoints.comeToDeckSpline, interval, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // pass
                }
                finally
                {
                    switch (side)
                    {
                        case OnboardSide.Own when _ownCardOfDeck == null:
                            _ownCardOfDeck = card;
                            _ownCardOfDeck.MoveTo(boardPoints.deckPoint);
                            break;
                        case OnboardSide.Other when _otherCardOfDeck == null:
                            _otherCardOfDeck = card;
                            _otherCardOfDeck.MoveTo(boardPoints.deckPoint);
                            break;
                        default:
                            Simulator.Model.cardObjectSpawner.Despawn(card);
                            break;
                    }
                }
            }
        }

        private OnboardPoints GetSide(OnboardSide side)
        {
            return side == OnboardSide.Own ? ownSide : otherSide;
        }
    }
}
