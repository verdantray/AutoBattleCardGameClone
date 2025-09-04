using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class ReturnLowBasePowerCardsToDeck : CardEffect
    {
        private readonly string _failureDescKey;

        private readonly int _cardsAmount;
        private readonly int _powerCriteria;
        
        public ReturnLowBasePowerCardsToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_CANCEL_TRIGGERS_KEY:
                        _failureDescKey = field.value.strValue;
                        break;
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                    case "power_criteria":
                        _powerCriteria = field.value.intValue;
                        break;
                }
            }
        }

        public override bool TryApplyEffect(CardEffectArgs args, out IMatchEvent matchEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                matchEvent = null;
                return false;
            }

            var cardsInInfirmary = ownSide.Infirmary.GetAllCards();
            if (!cardsInInfirmary.Any(card => card.BasePower <= _powerCriteria))
            {
                matchEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                return false;
            }
            
            List<Card> cardsToMove = new List<Card>();
            int moveCount = 0;
            
            for (int i = ownSide.Infirmary.Count - 1; i >= 0; i--)
            {
                if (_cardsAmount == moveCount)
                {
                    break;
                }
                
                Card cardToMove = ownSide.Infirmary[i].LastOrDefault(card => card.BasePower <= _powerCriteria);
                if (cardToMove == null)
                {
                    continue;
                }
                
                cardsToMove.Add(cardToMove);

                ownSide.Infirmary[i].Remove(cardToMove);
                if (ownSide.Infirmary[i].Count == 0)
                {
                    ownSide.Infirmary.RemoveByIndex(i);
                }

                moveCount++;
            }
            
            ownSide.Deck.AddRange(cardsToMove);

            matchEvent = new MoveCardsToBottomOfDeckEvent(cardsToMove, new MatchSnapshot(ownSide, otherSide));
            return true;
        }

        protected override string GetDescription()
        {
            // TODO : use localization and format
            return DescriptionKey;
        }
    }

    public class MoveCardsToBottomOfDeckEvent : MatchEventBase
    {
        public readonly IReadOnlyList<CardSnapshot> MoveCardsToBottomOfDeck;
        
        public MoveCardsToBottomOfDeckEvent(IEnumerable<Card> movedCards, MatchSnapshot snapshot) : base(snapshot)
        {
            MoveCardsToBottomOfDeck = movedCards.Select(card => new CardSnapshot(card)).ToList();
        }
    }
}
