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
            public Transform spawnByEffectPoint;
            public Transform fieldPoint;
            public Transform[] infirmaryPoints;
            public SplineContainer comeToDeckSpline;
            public Vector3 horizontalAlignDirection;
            public Vector3 verticalAlignDirection;
        }

        [SerializeField] private OnboardPoints ownSide;
        [SerializeField] private OnboardPoints otherSide;
        [SerializeField] private Vector3 cardOverlapDirection;

        private readonly CardLocator<CardReference> _cardReferenceLocator = new CardLocator<CardReference>();
        private readonly CardLocator<CardOnboard> _cardOnboardLocator = new CardLocator<CardOnboard>();

        private readonly Dictionary<OnboardSide, CardOnboard> _cardObjectsOnDeck = new()
        {
            { OnboardSide.Own, null },
            { OnboardSide.Other, null }
        };

        public void RegisterOnboard(IPlayer player, MatchPosition position, IReadOnlyList<CardReference> deckCards)
        {
            _cardReferenceLocator.RegisterSideLocator(player, position, deckCards);
            _cardOnboardLocator.RegisterSideLocator(player, position);
        }

        public void SwitchPosition(IPlayer attacker, IPlayer defender)
        {
            _cardReferenceLocator[attacker].SetPosition(MatchPosition.Attacking);
            _cardOnboardLocator[defender].SetPosition(MatchPosition.Defending);
        }

        public void RefreshOnboard(MatchSnapshot matchSnapshot)
        {
            // TODO : implements
        }

        public async Task InitializeOnboardAsync(ScaledTime delay, ScaledTime duration, CancellationToken token)
        {
            try
            {
                await delay.WaitScaledTimeAsync(token);

                var tasks = new List<Task>();
                
                foreach (var owner in _cardReferenceLocator.Participants)
                {
                    OnboardPoints onboardPoints = GetOnboardPointsOfSide(GetOnboardSideOfPlayer(owner));
                    
                    var cardReferenceDeck = _cardReferenceLocator[owner].Deck;
                    var cardOnboardDeck = _cardOnboardLocator[owner].Deck;

                    int deckCount = cardReferenceDeck.Count;
                    ScaledTime interval = duration / deckCount;
                    
                    for (int i = 0; i < deckCount; i++)
                    {
                        var cardReference = cardReferenceDeck.Peek(i);
                        ScaledTime scaledDelay = (interval * i);

                        var cardOnboard = SpawnCardOnboard(new CardSpawnArgs(onboardPoints.deckPoint, cardReference));
                        cardOnboardDeck.Insert(i, cardOnboard);

                        var task = cardOnboard.MoveFollowingSplineAsync(onboardPoints.comeToDeckSpline, scaledDelay, interval, token);
                        tasks.Add(task);
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // pass
            }
            finally
            {
                foreach (var owner in _cardReferenceLocator.Participants)
                {
                    OnboardSide onboardSide =  GetOnboardSideOfPlayer(owner);
                    OnboardPoints onboardPoints = GetOnboardPointsOfSide(onboardSide);

                    var cardReferenceDeck = _cardReferenceLocator[owner].Deck;
                    var cardOnboardDeck = _cardOnboardLocator[owner].Deck;
                    
                    int despawnCount = cardOnboardDeck.Count;
                    for (int i = 0; i < despawnCount; i++)
                    {
                        var cardToDespawn = cardOnboardDeck.Pop(0);
                        DespawnCardObject(cardToDespawn);
                    }

                    if (cardReferenceDeck.Count <= 0 || _cardObjectsOnDeck[onboardSide] != null)
                    {
                        continue;
                    }
                    
                    CardSpawnArgs args = new CardSpawnArgs(onboardPoints.deckPoint, null);
                    _cardObjectsOnDeck[onboardSide] = SpawnCardOnboard(args);
                }
            }
        }
        
        public async Task SetOverlapFieldCardsAsync(IPlayer owner, ScaledTime overlapDuration, ScaledTime alignDuration, CancellationToken token)
        {
            OnboardSide onboardSide = GetOnboardSideOfPlayer(owner);
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(onboardSide);
            
            // last card will place at first index
            CardOnboard lastCardOfField = _cardOnboardLocator[owner].Field.Peek(0);
                
            Vector3 lastCardPosition = lastCardOfField.transform.position;
            Vector3 lastCardAngles = lastCardOfField.transform.eulerAngles;
            
            lastCardOfField.MoveToTarget(lastCardPosition + cardOverlapDirection, lastCardAngles);
            
            try
            {
                int overlapCount = _cardOnboardLocator[owner].Field.Count - 1;
                ScaledTime interval = overlapDuration /  overlapCount;
                
                for (int i = overlapCount; i > 0; i--)
                {
                    var cardOnField = _cardOnboardLocator[owner].Field.Peek(i);
                    // ScaledTime scaledDelay = (interval * (i - overlapCount - 1));

                    await cardOnField.MoveToTargetAsync(lastCardPosition, lastCardAngles, interval, token);
                }
                
                int despawnCount = _cardOnboardLocator[owner].Field.Count - 1;
                for (int i = 1; i <= despawnCount; i++)
                {
                    CardOnboard cardOnField = _cardOnboardLocator[owner].Field.Pop(1);
                    DespawnCardObject(cardOnField);
                }
                
                lastCardOfField.MoveToTarget(lastCardPosition, lastCardAngles);
                await lastCardOfField.MoveToTargetAsync(onboardPoints.fieldPoint, alignDuration, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // pass
            }
            finally
            {
                int despawnCount = _cardOnboardLocator[owner].Field.Count - 1;
                for (int i = 1; i <= despawnCount; i++)
                {
                    CardOnboard cardOnField = _cardOnboardLocator[owner].Field.Pop(1);
                    DespawnCardObject(cardOnField);
                }
                
                lastCardOfField.MoveToTarget(onboardPoints.fieldPoint);
            }
        }

        public async Task ShuffleDeckAsync(CardMovementInfo movementInfo, ScaledTime remainDelay, ScaledTime inoutDuration, CancellationToken token)
        {
            var targetCardRef = movementInfo.PreviousLocation.PopFromLocation(_cardReferenceLocator);
            movementInfo.CurrentLocation.InsertToLocation(_cardReferenceLocator, targetCardRef);
            
            OnboardSide onboardSide = GetOnboardSideOfPlayer(movementInfo.PreviousLocation.Owner);
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(onboardSide);
            CardOnboard overturnedFromDeck = SpawnCardOnboard(new CardSpawnArgs(onboardPoints.deckPoint, null));
            
            try
            {
                Vector3 targetPosition = onboardPoints.deckPoint.position + onboardPoints.horizontalAlignDirection;
                Vector3 targetAngles = onboardPoints.deckPoint.transform.eulerAngles;

                await overturnedFromDeck.MoveToTargetAsync(targetPosition, targetAngles, inoutDuration, token);
                await remainDelay.WaitScaledTimeAsync(token);
                await overturnedFromDeck.MoveToTargetAsync(onboardPoints.deckPoint, inoutDuration, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // pass
            }
            finally
            {
                DespawnCardObject(overturnedFromDeck);
            }
        }

        public async Task MoveCardAsync(CardMovementInfo movementInfo, ScaledTime delay, ScaledTime duration, CancellationToken token)
        {
            IPlayer prevLocationOwner = movementInfo.PreviousLocation.Owner;
            OnboardSide prevOnboardSide = GetOnboardSideOfPlayer(prevLocationOwner);
            OnboardPoints prevOnboardPoints = GetOnboardPointsOfSide(prevOnboardSide);
            
            CardReference targetCardRef = movementInfo.PreviousLocation.PopFromLocation(_cardReferenceLocator);

            CardOnboard cardOnboard = null;
            switch (movementInfo.PreviousLocation.CardZone)
            {
                case CardZone.CardPile:
                {
                    cardOnboard = SpawnCardOnboard(new CardSpawnArgs(prevOnboardPoints.spawnByEffectPoint, targetCardRef));
                    break;
                }
                case CardZone.Deck:
                {
                    // hide cardOnboard of deck pile
                    if (_cardReferenceLocator[prevLocationOwner].Deck.Count == 0 && _cardObjectsOnDeck[prevOnboardSide] != null)
                    {
                        DespawnCardObject(_cardObjectsOnDeck[prevOnboardSide]);
                        _cardObjectsOnDeck[prevOnboardSide] = null;
                    }
                    
                    cardOnboard = SpawnCardOnboard(new CardSpawnArgs(prevOnboardPoints.deckPoint, targetCardRef));
                    break;
                }
                case CardZone.Field:
                {
                    bool isLastCardOfField = _cardReferenceLocator[prevLocationOwner].Field.Count == 0;
                    cardOnboard = isLastCardOfField
                        ? movementInfo.PreviousLocation.PopFromLocation(_cardOnboardLocator)
                        : SpawnCardOnboard(new CardSpawnArgs(prevOnboardPoints.fieldPoint, targetCardRef));
                    break;
                }
                case CardZone.Infirmary:
                {
                    cardOnboard = movementInfo.PreviousLocation.PopFromLocation(_cardOnboardLocator);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            IPlayer curLocationOwner = movementInfo.CurrentLocation.Owner;
            OnboardSide curOnboardSide = GetOnboardSideOfPlayer(curLocationOwner);
            OnboardPoints curOnboardPoints = GetOnboardPointsOfSide(curOnboardSide);
            
            movementInfo.CurrentLocation.InsertToLocation(_cardReferenceLocator, targetCardRef);

            bool isMoveToFieldOrInfirmary = movementInfo.CurrentLocation.CardZone is CardZone.Field or CardZone.Infirmary;
            if (isMoveToFieldOrInfirmary)
            {
                movementInfo.CurrentLocation.InsertToLocation(_cardOnboardLocator, cardOnboard);
            }

            try
            {
                if (movementInfo.PreviousLocation.CardZone == CardZone.Deck)
                {
                    ScaledTime revealDuration = delay * 0.5f;
                    await RevealDeckCardAsync(cardOnboard, prevOnboardPoints.revealCardPoint, revealDuration, revealDuration, token);
                }
                else
                {
                    await delay.WaitScaledTimeAsync(token);
                }

                string infirmarySlotKey = targetCardRef.CardData.nameKey;
                var task = movementInfo.CurrentLocation.CardZone switch
                {
                    CardZone.CardPile => cardOnboard.MoveToTargetAsync(curOnboardPoints.spawnByEffectPoint, duration, token),
                    CardZone.Deck => cardOnboard.MoveToTargetAsync(curOnboardPoints.deckPoint, duration, token),
                    CardZone.Field => AlignCardsOfFieldAfterAddedTask(curLocationOwner, duration, token),
                    CardZone.Infirmary => AlignCardsOfInfirmarySlotTask(curLocationOwner, infirmarySlotKey, duration, token),
                    _ => throw new ArgumentOutOfRangeException()
                };

                await task;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // pass
            }
            finally
            {
                switch (movementInfo.CurrentLocation.CardZone)
                {
                    case CardZone.CardPile:
                    {
                        DespawnCardObject(cardOnboard);
                        break;
                    }
                    case CardZone.Deck:
                    {
                        DespawnCardObject(cardOnboard);
                        
                        if (_cardReferenceLocator[curLocationOwner].Deck.Count > 0 && _cardObjectsOnDeck[curOnboardSide] == null)
                        {
                            CardSpawnArgs args = new CardSpawnArgs(curOnboardPoints.deckPoint, null);
                            _cardObjectsOnDeck[curOnboardSide] = SpawnCardOnboard(args);
                        }
                        break;
                    }
                    case CardZone.Field:
                    {
                        AlignCardsOfField(curLocationOwner);
                        break;
                    }
                    case CardZone.Infirmary:
                    {
                        string infirmarySlotKey = targetCardRef.CardData.nameKey;
                        AlignCardsOfInfirmarySlot(curLocationOwner, infirmarySlotKey);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static async Task RevealDeckCardAsync(CardOnboard cardOnboard, Transform revealPoint, ScaledTime revealDuration, ScaledTime remainDelay, CancellationToken token)
        {
            await cardOnboard.MoveToTargetAsync(revealPoint, revealDuration, token);
            await remainDelay.WaitScaledTimeAsync(token);
        }

        // align cards after newly added to field
        private Task AlignCardsOfFieldAfterAddedTask(IPlayer owner, ScaledTime duration, CancellationToken token)
        {
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(GetOnboardSideOfPlayer(owner));
            var cardReferenceField = _cardReferenceLocator[owner].Field;
            var cardOnboardField = _cardOnboardLocator[owner].Field;

            // attacking
            if (_cardReferenceLocator[owner].Position == MatchPosition.Attacking)
            {
                var tasks = new List<Task>();
                
                int fieldCount = cardReferenceField.Count;
                for (int i = 0; i < fieldCount; i++)
                {
                    var fieldCard = cardOnboardField.Peek(i);

                    Vector3 targetPos = GetHorizontalAlignedPosition(
                        fieldCount,
                        i,
                        onboardPoints.fieldPoint.position,
                        onboardPoints.horizontalAlignDirection
                    );
                
                    var task = fieldCard.MoveToTargetAsync(targetPos, onboardPoints.fieldPoint.eulerAngles, duration, token);
                    tasks.Add(task);
                }
                
                return Task.WhenAll(tasks);
            }
            // defending
            else
            {
                Vector3 targetPosition = onboardPoints.fieldPoint.position + cardOverlapDirection;
                Vector3 targetAngles = onboardPoints.fieldPoint.eulerAngles;

                int lastFieldIndex = cardOnboardField.Count - 1;
                var lastCardOfField = cardOnboardField.Peek(lastFieldIndex);

                return lastCardOfField.MoveToTargetAsync(targetPosition, targetAngles, duration, token);
            }
        }

        private void AlignCardsOfField(IPlayer owner)
        {
            OnboardPoints onboardPoints = GetOnboardPointsOfSide(GetOnboardSideOfPlayer(owner));
            var cardReferenceField = _cardReferenceLocator[owner].Field;
            var cardOnboardField = _cardOnboardLocator[owner].Field;

            if (_cardReferenceLocator[owner].Position == MatchPosition.Attacking)
            {
                int fieldCount = cardReferenceField.Count;
                for (int i = 0; i < fieldCount; i++)
                {
                    var fieldCard = cardOnboardField.Peek(i);

                    Vector3 targetPos = GetHorizontalAlignedPosition(
                        fieldCount,
                        i,
                        onboardPoints.fieldPoint.position,
                        onboardPoints.horizontalAlignDirection
                    );
                
                    fieldCard.MoveToTarget(targetPos, onboardPoints.fieldPoint.eulerAngles);
                }
            }
            else
            {
                int despawnCount = cardOnboardField.Count - 1;
                for (int i = 0; i < despawnCount; i++)
                {
                    var cardOnboard =  cardOnboardField.Peek(0);
                    DespawnCardObject(cardOnboard);
                }

                var lastCardOfField = cardOnboardField.Peek(0);
                lastCardOfField.MoveToTarget(onboardPoints.fieldPoint);
            }
        }

        private Task AlignCardsOfInfirmarySlotTask(IPlayer owner, string slotKey, ScaledTime moveDuration, CancellationToken token)
        {
            int indexOfSlot = _cardOnboardLocator[owner].Infirmary.IndexOfKey(slotKey);
            var cardHolderOfSlot = _cardOnboardLocator[owner].Infirmary[indexOfSlot];

            OnboardPoints onboardPoints = GetOnboardPointsOfSide(GetOnboardSideOfPlayer(owner));
            var target = onboardPoints.infirmaryPoints[indexOfSlot];

            var tasks = new List<Task>();

            int alignCount = cardHolderOfSlot.Count;
            for (int i = 0; i < alignCount; i++)
            {
                var cardOnInfirmary = cardHolderOfSlot.Peek(i);
                Vector3 alignDirection = onboardPoints.verticalAlignDirection + cardOverlapDirection;
                Vector3 alignedPosition = GetVerticalAlignedPosition(i, target.position, alignDirection);
                
                var task = cardOnInfirmary.MoveToTargetAsync(alignedPosition, target.eulerAngles, moveDuration, token);
                tasks.Add(task);
            }
            
            return Task.WhenAll(tasks);
        }

        private void AlignCardsOfInfirmarySlot(IPlayer owner, string slotKey)
        {
            int indexOfSlot = _cardOnboardLocator[owner].Infirmary.IndexOfKey(slotKey);
            var cardHolderOfSlot = _cardOnboardLocator[owner].Infirmary[indexOfSlot];

            OnboardPoints onboardPoints = GetOnboardPointsOfSide(GetOnboardSideOfPlayer(owner));
            var target = onboardPoints.infirmaryPoints[indexOfSlot];

            int alignCount = cardHolderOfSlot.Count;
            for (int i = 0; i < alignCount; i++)
            {
                var cardOnInfirmary = cardHolderOfSlot.Peek(i);
                Vector3 alignDirection = onboardPoints.verticalAlignDirection + cardOverlapDirection;
                Vector3 alignedPosition = GetVerticalAlignedPosition(i, target.position, alignDirection);
                
                cardOnInfirmary.MoveToTarget(alignedPosition, target.eulerAngles);
            }
        }
        
        private static Vector3 GetHorizontalAlignedPosition(int totalCount, int index, Vector3 origin, Vector3 direction)
        {
            float centerIndex = (totalCount - 1) * 0.5f;
            float offset = index - centerIndex;

            return origin + (direction * offset);
        }

        private static Vector3 GetVerticalAlignedPosition(int index, Vector3 origin, Vector3 direction)
        {
            return origin + (direction * index);
        }

        private static OnboardSide GetOnboardSideOfPlayer(IPlayer player)
        {
            return player.IsLocalPlayer ? OnboardSide.Own : OnboardSide.Other;
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
