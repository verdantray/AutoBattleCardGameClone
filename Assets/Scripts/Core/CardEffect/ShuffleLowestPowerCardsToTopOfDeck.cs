using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 덱에서 가장 낮은 파워 카드들을 덱 맨 위로 정렬
    /// </summary>
    public class ShuffleLowestPowerCardsToTopOfDeck : CardEffect
    {
        private readonly string _failureDescKey;
        
        private readonly int _cardsAmount;

        public ShuffleLowestPowerCardsToTopOfDeck(Card card, JsonObject json) : base(card, json)
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
        
        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;
            
            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }
            
            // if amount of remain hands less than _cardsAmount, then no need to move cards to top of deck...
            if (ownSide.Deck.Count < _cardsAmount)
            {
                var failedEffectEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                failedEffectEvent.RegisterEvent(matchContextEvent);
                
                return;
            }

            HashSet<int> rangeToMoveCardPowers = new HashSet<int>(ownSide.Deck
                .OrderBy(card => card.BasePower)
                .Take(_cardsAmount)
                .Select(card => card.BasePower)
                .Distinct());
            
            int moveCount = 0;
            
            for (int i = 0; i < ownSide.Deck.Count; i++)
            {
                if (_cardsAmount == moveCount)
                {
                    break;
                }

                if (!rangeToMoveCardPowers.Contains(ownSide.Deck[i].BasePower))
                {
                    continue;
                }

                Card cardToShuffle = ownSide.Deck[i];

                ownSide.Deck.Remove(cardToShuffle);
                ownSide.Deck.AddToTop(cardToShuffle);

                var moveCardsEffectEvent = new MoveCardsToTopOfDeckEvent(cardToShuffle, new MatchSnapshot(ownSide, otherSide));
                moveCardsEffectEvent.RegisterEvent(matchContextEvent);
                
                moveCount++;
            }
        }

        protected override string GetDescription()
        {
            // TODO : use localization and format
            return DescriptionKey;
        }
    }

    public class MoveCardsToTopOfDeckEvent : MatchEventBase
    {
        public MoveCardsToTopOfDeckEvent(Card movedCard, MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }
}
