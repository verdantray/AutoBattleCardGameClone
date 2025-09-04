using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 덱에서 가장 낮은 파워 카드들을 덱 맨 위로 정렬
    /// </summary>
    public class LowestPowerCardsToTopOfDeck : CardEffect
    {
        private readonly string _failureDescKey;
        
        private readonly int _cardsAmount;

        public LowestPowerCardsToTopOfDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_FAILURE_DESC_KEY :
                        _failureDescKey = field.value.strValue;
                        break;
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
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
            
            // if amount of remain hands less than _cardsAmount, then no need to move cards to top of deck...
            if (ownSide.Deck.Count < _cardsAmount)
            {
                matchEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                return false;
            }
            
            var toMove = ownSide.Deck.OrderBy(card => card.BasePower).Take(_cardsAmount).ToList();
            foreach (var card in toMove)
            {
                ownSide.Deck.Remove(card);
            }
            
            ownSide.Deck.AddToTopRange(toMove);
            
            MatchSnapshot matchSnapshot = new MatchSnapshot(ownSide, otherSide);
            matchEvent = new MoveCardsToTopOfDeckEvent(toMove, matchSnapshot);
            
            return true;
        }

        protected override string GetDescription()
        {
            // TODO : use localization and format
            return DescriptionKey;
        }
    }

    public class MoveCardsToTopOfDeckEvent : MatchEventBase
    {
        public readonly IReadOnlyList<CardSnapshot> MovedCardsToTopOfDeck;
        
        public MoveCardsToTopOfDeckEvent(IEnumerable<Card> movedCards, MatchSnapshot snapshot) : base(snapshot)
        {
            MovedCardsToTopOfDeck = movedCards.Select(card => new CardSnapshot(card)).ToList();
        }
    }
}
