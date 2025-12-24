using System;
using System.Collections.Generic;
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

        public async Task SetCardsToDeckPileAsync(OnboardSide side, IReadOnlyList<CardReference> deckCards, ScaledTime delay, ScaledTime duration, CancellationToken token = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay), token);

            var boardPoints = GetSide(side);

            string assetPath = GameConst.AssetPath.CARD_ONBOARD;
            
            int deckCount = deckCards.Count;
            ScaledTime interval = duration / deckCount;
            for (int i = 0; i < deckCount; i++)
            {
                CardSpawnArgs args = new CardSpawnArgs(boardPoints.comeToDeckSpline.transform, deckCards[i]);
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
                            _ownCardOfDeck.MoveToTransform(boardPoints.deckPoint);
                            break;
                        case OnboardSide.Other when _otherCardOfDeck == null:
                            _otherCardOfDeck = card;
                            _otherCardOfDeck.MoveToTransform(boardPoints.deckPoint);
                            break;
                        default:
                            Simulator.Model.cardObjectSpawner.Despawn(card);
                            break;
                    }
                }
            }
        }

        public async Task MoveCardsAsync(OnboardSide side, CardMovementInfo moveInfo, ScaledTime duration, CancellationToken token = default)
        {
            try
            {
                OnboardPoints onboardPoints = GetSide(side);
                
                if (moveInfo.PreviousLocation.CardZone == CardZone.Deck)
                {
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private OnboardPoints GetSide(OnboardSide side)
        {
            return side == OnboardSide.Own ? ownSide : otherSide;
        }
    }
}
