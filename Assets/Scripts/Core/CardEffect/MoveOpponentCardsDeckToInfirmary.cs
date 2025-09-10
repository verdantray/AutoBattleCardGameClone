using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 상대의 덱에서 카드 n장을 양호실로 보냄
    /// </summary>
    public sealed class MoveOpponentCardsDeckToInfirmary : CardEffect
    {
        private readonly int _cardsAmount;
        
        public MoveOpponentCardsDeckToInfirmary(Card card, JsonObject json) : base(card, json)
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

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            CardEffectArgs leaveOpponentFieldEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveField,
                otherSide,
                ownSide,
                gameState
            );

            CardEffectArgs enterOpponentInfirmaryEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnEnterInfirmary,
                otherSide,
                ownSide,
                gameState
            );

            for (int i = 0; i < _cardsAmount; i++)
            {
                if (!otherSide.Deck.TryDraw(out Card cardToMove))
                {
                    MatchFinishEvent finishByEmptyDeckEvent = new MatchFinishEvent(
                        ownSide.Player,
                        MatchEndReason.EndByEmptyDeck,
                        new MatchSnapshot(ownSide, otherSide)
                    );
                    
                    finishByEmptyDeckEvent.RegisterEvent(matchContextEvent);
                    return;
                }

                bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveOpponentFieldEffectArgs, matchContextEvent);
                if (isMovementReplaced)
                {
                    MatchContextEvent.CheckApplyBuffs(ownSide, otherSide, gameState, matchContextEvent);
                    continue;
                }
                
                otherSide.Infirmary.PutCard(cardToMove);

                TryPutCardInfirmaryEvent putCardInfirmaryEvent = new TryPutCardInfirmaryEvent(
                    otherSide.Player,
                    cardToMove,
                    new MatchSnapshot(ownSide, otherSide)
                );
                
                putCardInfirmaryEvent.RegisterEvent(matchContextEvent);

                if (!otherSide.Infirmary.IsSlotRemains)
                {
                    MatchFinishEvent finishByFullOfInfirmaryEvent = new MatchFinishEvent(
                        ownSide.Player,
                        MatchEndReason.EndByFullOfInfirmary,
                        new MatchSnapshot(ownSide, otherSide)
                    );
                    
                    finishByFullOfInfirmaryEvent.RegisterEvent(matchContextEvent);
                    return;
                }
                
                cardToMove.CardEffect.CheckApplyEffect(enterOpponentInfirmaryEffectArgs, matchContextEvent);
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
    } 
}