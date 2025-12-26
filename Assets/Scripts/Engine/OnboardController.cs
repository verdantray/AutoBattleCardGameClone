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

        private readonly Dictionary<OnboardSide, CardReferenceLocator> _cardReferenceLocators = new()
        {
            { OnboardSide.Own, new CardReferenceLocator() },
            { OnboardSide.Other, new CardReferenceLocator() },
        };

        private readonly Dictionary<OnboardSide, CardOnboardLocator> _cardOnboardLocators = new()
        {
            { OnboardSide.Own, new CardOnboardLocator() },
            { OnboardSide.Other, new CardOnboardLocator() },
        };

        private readonly Dictionary<OnboardSide, CardOnboard> _cardObjectsOnDeck = new()
        {
            { OnboardSide.Own, null },
            { OnboardSide.Other, null }
        };

        public async Task SetCardsToDeckPileAsync(OnboardSide side, IReadOnlyList<CardReference> deckCards, ScaledTime delay, ScaledTime duration, CancellationToken token)
        {
            OnboardPoints boardPoints = GetOnboardPointsOfSide(side);
            CardReferenceLocator cardReferenceLocator = _cardReferenceLocators[side];
            CardOnboardLocator cardOnboardLocator = _cardOnboardLocators[side];
            
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
                    var cardOnboard = SpawnCardOnboard(args);
                    cardOnboardLocator.Deck.Insert(i, cardOnboard);

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
                for (int i = cardOnboardLocator.Deck.Count - 1; i >= 0; i--)
                {
                    var cardOnboard = cardOnboardLocator.Deck.Pop(i);
                    DespawnCardObject(cardOnboard);
                }

                if (cardReferenceLocator.Deck.Count > 0 && _cardObjectsOnDeck[side] == null)
                {
                    CardSpawnArgs args = new CardSpawnArgs(boardPoints.deckPoint, null);
                    _cardObjectsOnDeck[side] = SpawnCardOnboard(args);
                }
            }
        }

        public async Task DrawCardToFieldAsync(OnboardSide side, CardMovementInfo moveInfo, ScaledTime revealDuration, ScaledTime remainDelay, ScaledTime alignDuration, CancellationToken token)
        {
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(side);
            CardReference targetCardRef = moveInfo.PreviousLocation.PopFromLocation(_cardReferenceLocators[side]);

            CardSpawnArgs args = new CardSpawnArgs(onboardPoints.deckPoint, targetCardRef);
            CardOnboard drawnCardOnboard = SpawnCardOnboard(args);
            
            moveInfo.CurrentLocation.InsertToLocation(_cardReferenceLocators[side], targetCardRef);
            moveInfo.CurrentLocation.InsertToLocation(_cardOnboardLocators[side], drawnCardOnboard);

            try
            {
                if (_cardReferenceLocators[side].Deck.Count == 0 && _cardObjectsOnDeck[side] != null)
                {
                    DespawnCardObject(_cardObjectsOnDeck[side]);
                    _cardObjectsOnDeck[side] = null;
                }

                await drawnCardOnboard.MoveToTargetAsync(onboardPoints.revealCardPoint, revealDuration, token);
                await remainDelay.WaitScaledTimeAsync(token);

                var tasks = new List<Task>();
                
                int fieldCount = _cardReferenceLocators[side].Field.Count;
                for (int i = fieldCount - 1; i >= 0; i--)
                {
                    var cardOnField = _cardOnboardLocators[side].Field.Peek(i);
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
                int fieldCount = _cardReferenceLocators[side].Field.Count;
                for (int i = fieldCount - 1; i >= 0; i--)
                {
                    var cardOnField = _cardOnboardLocators[side].Field.Peek(i);
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

        private Vector3 GetHorizontalAlignedPosition(int totalCount, int index, Vector3 origin, Vector3 direction)
        {
            int centerIndex = totalCount / 2;
            Vector3 alignedPosition = centerIndex == index
                ? origin
                : origin + (direction * (index - centerIndex));

            return alignedPosition;
        }

        public async Task SendCardToInfirmaryAsync(OnboardSide side, CardMovementInfo moveInfo, ScaledTime moveDuration, CancellationToken token = default)
        {
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(side);
            
            CardReference targetCardRef = moveInfo.PreviousLocation.PopFromLocation(_cardReferenceLocators[side]);
            CardOnboard cardToInfirmary = moveInfo.PreviousLocation.PopFromLocation(_cardOnboardLocators[side]);
            
            moveInfo.CurrentLocation.InsertToLocation(_cardReferenceLocators[side], targetCardRef);
            moveInfo.CurrentLocation.InsertToLocation(_cardOnboardLocators[side], cardToInfirmary);

            try
            {
                var tasks =  new List<Task>();

                int indexOfKey = _cardOnboardLocators[side].Infirmary.IndexOfKey(targetCardRef.CardData.nameKey);
                var cardOnInfirmaryHolder = _cardOnboardLocators[side].Infirmary[indexOfKey];
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
                int indexOfKey = _cardOnboardLocators[side].Infirmary.IndexOfKey(targetCardRef.CardData.nameKey);
                var cardOnInfirmaryHolder = _cardOnboardLocators[side].Infirmary[indexOfKey];
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

        private Vector3 GetVerticalAlignedPosition(int index, Vector3 origin, Vector3 direction)
        {
            return origin + (direction * index);
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
