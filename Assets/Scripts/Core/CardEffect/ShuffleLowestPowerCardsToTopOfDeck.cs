using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 덱에서 가장 낮은 파워 카드들을 덱 맨 위로 정렬
    /// </summary>
    public sealed class ShuffleLowestPowerCardsToTopOfDeck : CardEffect
    {
        private readonly int _cardsAmount;

        public ShuffleLowestPowerCardsToTopOfDeck(Card card, JsonObject json) : base(card, json)
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
            
            // if amount of remain hands less than _cardsAmount, then no need to move cards to top of deck...
            if (ownSide.Deck.Count < _cardsAmount)
            {
                CallCard.TryGetCardLocation(ownSide, out var currentLocation);

                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    currentLocation,
                    FailToActivateEffectReason.NoDeckRemains
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // var failedEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoDeckRemains);
                // failedEffectEvent.RegisterEvent(matchContextEvent);
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
                cardToShuffle.TryGetCardLocation(ownSide, out var prevAppliedCardLocation);

                ownSide.Deck.Remove(cardToShuffle);
                ownSide.Deck.AddToTop(cardToShuffle);
                
                cardToShuffle.TryGetCardLocation(ownSide, out var curAppliedCardLocation);
                CallCard.TryGetCardLocation(ownSide, out var activatedCardLocation);
                
                CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(prevAppliedCardLocation, activatedCardLocation);
                CardMovementInfo movementInfo = new CardMovementInfo(prevAppliedCardLocation, curAppliedCardLocation);
                ShuffleDeckEvent shuffleDeckEvent = new ShuffleDeckEvent(appliedInfo, movementInfo);
                shuffleDeckEvent.RegisterEvent(matchContextEvent);
                
                // string moveCardToTopOfDeckMessage = $"{ownSide.Player.Name}가 카드를 덱 맨 위로 보냄\n{cardToShuffle}";
                // var moveCardEffectEvent = new CommonMatchMessageEvent(moveCardToTopOfDeckMessage);
                // moveCardEffectEvent.RegisterEvent(matchContextEvent);
                
                moveCount++;
            }
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _cardsAmount);
        }
    }
}
