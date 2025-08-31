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
        
        public override bool TryApplyEffect(EffectTriggerEvent trigger, MatchSide mySide, MatchSide otherSide, out IMatchEvent matchEvent)
        {
            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                matchEvent = new FailToApplyCardEffectEvent(
                    FailToApplyCardEffectEvent.FailureReason.TriggerNotMatch,
                    "",
                    new MatchSnapshot(mySide, otherSide)
                );

                return false;
            }
            
            // if amount of remain hands less then _cardsAmount, then no need to move cards to top of deck...
            if (mySide.Deck.Count < _cardsAmount)
            {
                matchEvent = new FailToApplyCardEffectEvent(
                    FailToApplyCardEffectEvent.FailureReason.FailureMeetCondition,
                    "",
                    new MatchSnapshot(mySide, otherSide)
                );
                
                return false;
            }
            
            var toMove = mySide.Deck.OrderBy(card => card.BasePower).Take(_cardsAmount).ToList();
            foreach (var card in toMove)
            {
                mySide.Deck.Remove(card);
            }
            
            mySide.Deck.AddToTopRange(toMove);
            
            MatchSnapshot matchSnapshot = new MatchSnapshot(mySide, otherSide);
            matchEvent = new MoveCardsToTopOfDeck(toMove, matchSnapshot);
            
            return true;
        }

        protected override string GetDescription()
        {
            // TODO : use localization and format
            return DescriptionKey;
        }
    }

    public class MoveCardsToTopOfDeck : MatchEventBase
    {
        public readonly IReadOnlyList<CardSnapshot> MovedCardsToTopOfDeck;
        
        public MoveCardsToTopOfDeck(IEnumerable<Card> movedCards, MatchSnapshot snapshot) : base(snapshot)
        {
            MovedCardsToTopOfDeck = movedCards.Select(card => new CardSnapshot(card)).ToList();
        }
    }
}
