using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public sealed class MoveRandomCardsToField : CardEffect
    {
        private readonly int _cardsAmount;
        
        public MoveRandomCardsToField(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }
            
            // TODO: PCG32
            Random random = new Random();
            var cardsInInfirmary = ownSide.Infirmary.GetAllCards()
                .OrderBy(_ => random.Next())
                .Take(_cardsAmount)
                .ToArray();

            CallCard.TryGetCardLocation(ownSide, out var activatedCardLocation);
            
            if (cardsInInfirmary.Length == 0)
            {
                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    CallCard.Id,
                    activatedCardLocation,
                    FailToActivateEffectReason.NoInfirmaryRemains
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // var failEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoInfirmaryRemains);
                // failEffectEvent.RegisterEvent(matchContextEvent);
                return;
            }
            
            CardEffectArgs onLeaveInfirmaryArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveInfirmary,
                ownSide,
                otherSide,
                gameState
            );

            CardEffectArgs onEnterFieldArgs = new CardEffectArgs(
                ownSide.IsAttacking
                    ? EffectTriggerEvent.OnEnterFieldAsAttacker
                    : EffectTriggerEvent.OnRemainField,
                ownSide,
                otherSide,
                gameState
            );
            
            for (int i = 0; i < _cardsAmount; i++)
            {
                if (cardsInInfirmary.Length <= i)
                {
                    break;
                }
                
                Card cardToMove = cardsInInfirmary[i];

                string slotKey = cardToMove.CardData.nameKey;
                if (!ownSide.Infirmary[slotKey].Contains(cardToMove))
                {
                    throw new KeyNotFoundException($"Can't find slot {slotKey} from {ownSide.Player.Name}'s infirmary");
                }

                int indexOfInfirmarySlot = ownSide.Infirmary[slotKey].IndexOf(cardToMove);
                
                ownSide.Infirmary[slotKey].Remove(cardToMove);
                if (ownSide.Infirmary[slotKey].Count == 0)
                {
                    ownSide.Infirmary.Remove(slotKey);
                }
                
                
                cardToMove.CardEffect.CheckApplyEffect(onLeaveInfirmaryArgs, matchContextEvent);
                
                int targetFieldIndex = ownSide.Field.IndexOf(CallCard);
                ownSide.Field.Insert(targetFieldIndex, cardToMove);
                
                CardLocation prevLocation = new InfirmaryLocation(ownSide.Player, slotKey, indexOfInfirmarySlot);
                CardLocation curLocation = new FieldLocation(ownSide.Player, targetFieldIndex);

                CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(prevLocation, activatedCardLocation);
                CardMovementInfo movementInfo = new CardMovementInfo(prevLocation, curLocation);
                
                MoveCardByEffectEvent moveCardEvent = new MoveCardByEffectEvent(appliedInfo, movementInfo);
                moveCardEvent.RegisterEvent(matchContextEvent);
                
                cardToMove.CardEffect.CheckApplyEffect(onEnterFieldArgs, matchContextEvent);
                
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }
    }
}