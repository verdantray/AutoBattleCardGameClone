using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에서 기본 파워가 기준 n 이하인 카드 m장을 덱 아래로 넣음
    /// </summary>
    public sealed class MoveLowBasePowerCardsFromInfirmaryToDeck : CardEffect
    {
        private readonly int _cardsAmount;
        private readonly int _powerCriteria;
        
        public MoveLowBasePowerCardsFromInfirmaryToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                    case "power_criteria":
                        _powerCriteria = field.value.intValue;
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

            var cardsInInfirmary = ownSide.Infirmary.GetAllCards();
            if (!cardsInInfirmary.Any(card => card.BasePower <= _powerCriteria))
            {
                var failedCard = new CardReference(CallCard, new CardBuffArgs(ownSide, otherSide, gameState));

                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    failedCard,
                    FailToActivateEffectReason.NoMeetCondition
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // var failEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoMeetCondition);
                // failEffectEvent.RegisterEvent(matchContextEvent);
                return;
            }
            
            CardEffectArgs onLeaveInfirmaryArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveInfirmary,
                ownSide,
                otherSide,
                gameState
            );

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

                int indexOfSlot = ownSide.Infirmary[i].IndexOf(cardToMove);
                string slotKey = cardToMove.CardData.nameKey;
                CardLocation prevLocation = new InfirmaryLocation(ownSide.Player, slotKey, indexOfSlot);
                
                ownSide.Infirmary[i].Remove(cardToMove);
                if (ownSide.Infirmary[i].Count == 0)
                {
                    ownSide.Infirmary.RemoveByIndex(i);
                }

                ownSide.Deck.Add(cardToMove);
                cardToMove.CardEffect.CheckApplyEffect(onLeaveInfirmaryArgs, matchContextEvent);
                
                moveCount++;

                CardBuffArgs buffArgs = new CardBuffArgs(ownSide, otherSide, gameState);
                
                var sentCard = new CardReference(cardToMove, buffArgs);
                var activatedCard = new CardReference(CallCard, buffArgs);
                CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(sentCard, activatedCard);

                int indexOfDeck = ownSide.Deck.IndexOf(cardToMove);
                CardLocation curLocation = new DeckLocation(ownSide.Player, indexOfDeck);

                CardMovementInfo movementInfo = new CardMovementInfo(prevLocation, curLocation);

                SendToDeckFromInfirmaryEvent sendCardEvent = new SendToDeckFromInfirmaryEvent(appliedInfo, movementInfo);
                sendCardEvent.RegisterEvent(matchContextEvent);

                // string moveCardToBottomOfDeckMessage = $"{ownSide.Player.Name}가 카드를 덱 맨 아래로 보냄\n{cardToMove}";
                // var moveCardEffectEvent = new CommonMatchMessageEvent(moveCardToBottomOfDeckMessage);
                // moveCardEffectEvent.RegisterEvent(matchContextEvent);
                
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _powerCriteria, _cardsAmount);
        }
    }
}
