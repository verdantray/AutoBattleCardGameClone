using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class MoveDeckToInfirmary : CardEffect
    {
        private readonly int _cardsAmount;
        
        public MoveDeckToInfirmary(Card card, JsonObject json) : base(card, json)
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

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }
            
            CardEffectArgs leaveFieldEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveField,
                ownSide,
                otherSide,
                gameState
            );

            CardEffectArgs enterInfirmaryEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnEnterInfirmary,
                ownSide,
                otherSide,
                gameState
            );

            for (int i = 0; i < _cardsAmount; i++)
            {
                if (!ownSide.Deck.TryDraw(out Card cardToMove))
                {
                    MatchFinishEvent finishByEmptyDeckEvent = new MatchFinishEvent(
                        otherSide.Player,
                        MatchEndReason.EndByEmptyDeck,
                        new MatchSnapshot(ownSide, otherSide)
                    );
                    
                    finishByEmptyDeckEvent.RegisterEvent(matchContextEvent);
                    return;
                }
                
                // replace movement instead put to infirmary
                bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, matchContextEvent);
                if (isMovementReplaced)
                {
                    MatchContextEvent.CheckApplyBuffs(ownSide, otherSide, gameState, matchContextEvent);
                    continue;
                }
                
                // put cards from defender field to infirmary
                ownSide.Infirmary.PutCard(cardToMove);
                
                TryPutCardInfirmaryEvent putCardInfirmaryEvent = new TryPutCardInfirmaryEvent(
                    ownSide.Player,
                    cardToMove,
                    new MatchSnapshot(ownSide, otherSide)
                );
                
                putCardInfirmaryEvent.RegisterEvent(matchContextEvent);

                if (!ownSide.Infirmary.IsSlotRemains)
                {
                    MatchFinishEvent finishByFullOfInfirmaryEvent = new MatchFinishEvent(
                        otherSide.Player,
                        MatchEndReason.EndByFullOfInfirmary,
                        new MatchSnapshot(ownSide, otherSide)
                    );
                    
                    finishByFullOfInfirmaryEvent.RegisterEvent(matchContextEvent);
                    return;
                }
                
                cardToMove.CardEffect.CheckApplyEffect(enterInfirmaryEffectArgs, matchContextEvent);
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