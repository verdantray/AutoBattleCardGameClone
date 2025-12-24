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

        private readonly Dictionary<OnboardSide, CardReferenceLocator> _cardReferenceLocators =
            new Dictionary<OnboardSide, CardReferenceLocator>()
            {
                { OnboardSide.Own, new CardReferenceLocator() },
                { OnboardSide.Other, new CardReferenceLocator() },
            };

        private readonly Dictionary<OnboardSide, CardObjectLocator> _cardObjectLocators =
            new Dictionary<OnboardSide, CardObjectLocator>()
            {
                { OnboardSide.Own, new CardObjectLocator() },
                { OnboardSide.Other, new CardObjectLocator() },
            };

        private readonly Dictionary<OnboardSide, CardOnboard> _cardObjectsOnDeck =
            new Dictionary<OnboardSide, CardOnboard>()
            {
                { OnboardSide.Own, null },
                { OnboardSide.Other, null }
            };

        public async Task SetCardsToDeckPileAsync(OnboardSide side, IReadOnlyList<CardReference> deckCards, ScaledTime delay, ScaledTime duration, CancellationToken token = default)
        {
            string assetPath = GameConst.AssetPath.CARD_ONBOARD;
            OnboardPoints boardPoints = GetOnboardPointsOfSide(side);
            CardReferenceLocator cardReferenceLocator = _cardReferenceLocators[side];
            CardObjectLocator cardObjectLocator = _cardObjectLocators[side];
            
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), token);

                int deckCount = deckCards.Count;
                ScaledTime interval = duration / deckCount;

                var tasks = new List<Task>();

                for (int i = 0; i < deckCount; i++)
                {
                    var cardReference = deckCards[i];
                    cardReferenceLocator.Deck.Insert(i, cardReference);

                    CardSpawnArgs args = new CardSpawnArgs(boardPoints.deckPoint, cardReference);
                    var cardOnboard = Simulator.Model.cardObjectSpawner.Spawn<CardOnboard>(assetPath, args);
                    cardObjectLocator.Deck.Insert(i, cardOnboard);

                    float delayPerCard = interval * i;
                    ScaledTime scaledDelay = delayPerCard;

                    var task = cardOnboard.MoveFollowingSplineAsync(boardPoints.comeToDeckSpline, interval, scaledDelay, token);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // pass
            }
            finally
            {
                for (int i = cardObjectLocator.Deck.Count - 1; i >= 0; i--)
                {
                    var cardOnboard = cardObjectLocator.Deck.Pop(i);
                    Simulator.Model.cardObjectSpawner.Despawn(cardOnboard);
                }

                if (cardReferenceLocator.Deck.Count > 0 && _cardObjectsOnDeck[side] == null)
                {
                    CardSpawnArgs args = new CardSpawnArgs(boardPoints.deckPoint, null);
                    _cardObjectsOnDeck[side] = Simulator.Model.cardObjectSpawner.Spawn<CardOnboard>(assetPath, args);
                }
            }
        }

        public async Task MoveCardsAsync(OnboardSide side, CardMovementInfo moveInfo, ScaledTime duration, CancellationToken token = default)
        {
            try
            {
                OnboardPoints onboardPoints = GetOnboardPointsOfSide(side);
                
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

        private OnboardPoints GetOnboardPointsOfSide(OnboardSide side)
        {
            return side == OnboardSide.Own ? ownSide : otherSide;
        }
    }
}
