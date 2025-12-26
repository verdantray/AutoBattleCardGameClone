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
            public Vector3 horizontalAlignDirection;
            public Vector3 verticalAlignDirection;
        }

        [SerializeField] private OnboardPoints ownSide;
        [SerializeField] private OnboardPoints otherSide;

        private readonly CardLocator<CardReference> _cardReferenceLocator = new CardLocator<CardReference>();
        private readonly CardLocator<CardOnboard> _cardOnboardLocator = new CardLocator<CardOnboard>();

        private readonly Dictionary<OnboardSide, CardOnboard> _cardObjectsOnDeck = new()
        {
            { OnboardSide.Own, null },
            { OnboardSide.Other, null }
        };

        public async Task SetCardsToDeckPileAsync(IPlayer owner, IReadOnlyList<CardReference> deckCards, ScaledTime delay, ScaledTime duration, CancellationToken token)
        {
            OnboardSide onboardSide = GetOnboardSideOfPlayer(owner);
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(onboardSide);

            if (!_cardReferenceLocator.Contains(owner))
            {
                _cardReferenceLocator.RegisterSideLocator(owner, deckCards);
            }

            if (!_cardOnboardLocator.Contains(owner))
            {
                _cardOnboardLocator.RegisterSideLocator(owner);
            }

            var cardReferenceDeck = _cardReferenceLocator[owner].Deck;
            var cardOnboardDeck = _cardOnboardLocator[owner].Deck;
            
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), token);

                int deckCount = deckCards.Count;
                ScaledTime interval = duration / deckCount;

                var tasks = new List<Task>();

                for (int i = 0; i < deckCount; i++)
                {
                    var cardReference = deckCards[i];
                    cardReferenceDeck.Insert(i, cardReference);

                    CardSpawnArgs args = new CardSpawnArgs(onboardPoints.deckPoint, cardReference);
                    var cardOnboard = SpawnCardOnboard(args);
                    cardOnboardDeck.Insert(i, cardOnboard);

                    float delayPerCard = interval * i;
                    ScaledTime scaledDelay = delayPerCard;

                    var task = cardOnboard.MoveFollowingSplineAsync(onboardPoints.comeToDeckSpline, interval, scaledDelay, token);
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
                for (int i = cardOnboardDeck.Count - 1; i >= 0; i--)
                {
                    var cardOnboard = cardOnboardDeck.Pop(i);
                    DespawnCardObject(cardOnboard);
                }
                
                if (cardReferenceDeck.Count > 0 && _cardObjectsOnDeck[onboardSide] == null)
                {
                    CardSpawnArgs args = new CardSpawnArgs(onboardPoints.deckPoint, null);
                    _cardObjectsOnDeck[onboardSide] = SpawnCardOnboard(args);
                }
            }
        }

        public async Task DrawCardToFieldAsync(IPlayer owner, CardMovementInfo moveInfo, ScaledTime revealDuration, ScaledTime remainDelay, ScaledTime alignDuration, CancellationToken token)
        {
            OnboardSide onboardSide = GetOnboardSideOfPlayer(owner);
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(onboardSide);
            CardReference targetCardRef = moveInfo.PreviousLocation.PopFromLocation(_cardReferenceLocator);

            CardSpawnArgs args = new CardSpawnArgs(onboardPoints.deckPoint, targetCardRef);
            CardOnboard drawnCardOnboard = SpawnCardOnboard(args);
            
            moveInfo.CurrentLocation.InsertToLocation(_cardReferenceLocator, targetCardRef);
            moveInfo.CurrentLocation.InsertToLocation(_cardOnboardLocator, drawnCardOnboard);

            try
            {
                if (_cardReferenceLocator[owner].Deck.Count == 0 && _cardObjectsOnDeck[onboardSide] != null)
                {
                    DespawnCardObject(_cardObjectsOnDeck[onboardSide]);
                    _cardObjectsOnDeck[onboardSide] = null;
                }

                await drawnCardOnboard.MoveToTargetAsync(onboardPoints.revealCardPoint, revealDuration, token);
                await remainDelay.WaitScaledTimeAsync(token);

                var tasks = new List<Task>();
                
                int fieldCount = _cardReferenceLocator[owner].Field.Count;
                for (int i = fieldCount - 1; i >= 0; i--)
                {
                    var cardOnField = _cardOnboardLocator[owner].Field.Peek(i);
                    Vector3 alignedPosition = GetHorizontalAlignedPosition(
                        fieldCount,
                        i,
                        onboardPoints.fieldPoint.position,
                        onboardPoints.horizontalAlignDirection
                    );

                    var task = cardOnField.MoveToTargetAsync(alignedPosition, onboardPoints.fieldPoint.eulerAngles, alignDuration, token);
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
                int fieldCount = _cardReferenceLocator[owner].Field.Count;
                for (int i = fieldCount - 1; i >= 0; i--)
                {
                    var cardOnField = _cardOnboardLocator[owner].Field.Peek(i);
                    Vector3 alignedPosition = GetHorizontalAlignedPosition(
                        fieldCount,
                        i,
                        onboardPoints.fieldPoint.position,
                        onboardPoints.horizontalAlignDirection
                    );

                    cardOnField.MoveToTarget(alignedPosition, onboardPoints.fieldPoint.eulerAngles);
                }
            }
        }

        private static Vector3 GetHorizontalAlignedPosition(int totalCount, int index, Vector3 origin, Vector3 direction)
        {
            int centerIndex = totalCount / 2;
            Vector3 alignedPosition = centerIndex == index
                ? origin
                : origin + (direction * (index - centerIndex));

            return alignedPosition;
        }

        public async Task SendCardToInfirmaryAsync(IPlayer owner, CardMovementInfo moveInfo, ScaledTime moveDuration, CancellationToken token = default)
        {
            OnboardSide onboardSide = GetOnboardSideOfPlayer(owner);
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(onboardSide);
            
            CardReference targetCardRef = moveInfo.PreviousLocation.PopFromLocation(_cardReferenceLocator);
            CardOnboard cardToInfirmary = moveInfo.PreviousLocation.PopFromLocation(_cardOnboardLocator);
            
            moveInfo.CurrentLocation.InsertToLocation(_cardReferenceLocator, targetCardRef);
            moveInfo.CurrentLocation.InsertToLocation(_cardOnboardLocator, cardToInfirmary);

            try
            {
                var tasks =  new List<Task>();

                int indexOfKey = _cardOnboardLocator[owner].Infirmary.IndexOfKey(targetCardRef.CardData.nameKey);
                var cardOnInfirmaryHolder = _cardOnboardLocator[owner].Infirmary[indexOfKey];
                var target = onboardPoints.infirmaryPoints[indexOfKey];
                
                int infirmarySlotCount = cardOnInfirmaryHolder.Count;
                for (int i = 0; i < infirmarySlotCount; i++)
                {
                    var cardOnInfirmary = cardOnInfirmaryHolder.Peek(i);
                    Vector3 alignedPosition = GetVerticalAlignedPosition(i, target.position, onboardPoints.verticalAlignDirection);

                    var task = cardOnInfirmary.MoveToTargetAsync(alignedPosition, target.eulerAngles, moveDuration, token);
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
                int indexOfKey = _cardOnboardLocator[owner].Infirmary.IndexOfKey(targetCardRef.CardData.nameKey);
                var cardOnInfirmaryHolder = _cardOnboardLocator[owner].Infirmary[indexOfKey];
                var target = onboardPoints.infirmaryPoints[indexOfKey];
                
                int infirmarySlotCount = cardOnInfirmaryHolder.Count;
                for (int i = 0; i < infirmarySlotCount; i++)
                {
                    var cardOnInfirmary = cardOnInfirmaryHolder.Peek(i);
                    Vector3 alignedPosition = GetVerticalAlignedPosition(i, target.position, onboardPoints.verticalAlignDirection);

                    cardOnInfirmary.MoveToTarget(alignedPosition, target.eulerAngles);
                }
            }
        }

        private static Vector3 GetVerticalAlignedPosition(int index, Vector3 origin, Vector3 direction)
        {
            return origin + (direction * index);
        }

        private static OnboardSide GetOnboardSideOfPlayer(IPlayer player)
        {
            bool isOwnPlayer = ReferenceEquals(player, Simulator.Model.player);
            return isOwnPlayer ? OnboardSide.Own : OnboardSide.Other;
        }
        
        private OnboardPoints GetOnboardPointsOfSide(OnboardSide side)
        {
            return side == OnboardSide.Own ? ownSide : otherSide;
        }

        private static CardOnboard SpawnCardOnboard(CardSpawnArgs args)
        {
            return Simulator.Model.cardObjectSpawner.Spawn<CardOnboard>(GameConst.AssetPath.CARD_ONBOARD, args);
        }

        private static void DespawnCardObject(CardObject cardOnboard)
        {
            Simulator.Model.cardObjectSpawner.Despawn(cardOnboard);
        }
    }
}
