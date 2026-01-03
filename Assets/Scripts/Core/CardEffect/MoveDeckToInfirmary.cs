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

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
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
                    MatchFinishEvent finishByEmptyDeck = new MatchFinishEvent(otherSide.Player, ownSide.Player, MatchEndReason.EndByEmptyDeck);
                    finishByEmptyDeck.RegisterEvent(matchContextEvent);
                    
                    // MatchFinishMessageEvent finishByEmptyDeckEvent = new MatchFinishMessageEvent(otherSide.Player, ownSide.Player, MatchEndReason.EndByEmptyDeck);
                    // finishByEmptyDeckEvent.RegisterEvent(matchContextEvent);
                    return;
                }
                
                // replace movement instead put to infirmary
                bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, matchContextEvent);
                if (isMovementReplaced)
                {
                    IMatchContextEvent.CheckApplyBuffs(gameState, matchContextEvent, ownSide, otherSide);
                    continue;
                }
                
                // put cards from defender field to infirmary
                ownSide.Infirmary.PutCard(cardToMove, out var infirmaryLocation);
                
                CardLocation prevAppliedCardLocation = new DeckLocation(ownSide.Player, ownSide.Deck.Count);
                CallCard.TryGetCardLocation(ownSide, out CardLocation activatedCardLocation);
                
                CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(prevAppliedCardLocation, activatedCardLocation);
                CardMovementInfo movementInfo = new CardMovementInfo(prevAppliedCardLocation, infirmaryLocation);
                
                MoveCardByEffectEvent moveCardEvent = new MoveCardByEffectEvent(appliedInfo, movementInfo);
                moveCardEvent.RegisterEvent(matchContextEvent);
                
                // string putCardToInfirmaryMessage = $"{ownSide.Player.Name}가 카드를 양호실에 넣음. \n{cardToMove}";
                // CommonMatchMessageEvent putCardToInfirmaryEvent = new CommonMatchMessageEvent(putCardToInfirmaryMessage);
                // putCardToInfirmaryEvent.RegisterEvent(matchContextEvent);

                if (!ownSide.Infirmary.IsSlotRemains)
                {
                    MatchFinishEvent finishByFullInfirmary = new MatchFinishEvent(
                        otherSide.Player,
                        ownSide.Player,
                        MatchEndReason.EndByFullOfInfirmary
                    );
                    
                    finishByFullInfirmary.RegisterEvent(matchContextEvent);
                    
                    // MatchFinishMessageEvent finishByFullOfInfirmaryEvent = new MatchFinishMessageEvent(
                    //     otherSide.Player,
                    //     ownSide.Player,
                    //     MatchEndReason.EndByFullOfInfirmary
                    // );
                    //
                    // finishByFullOfInfirmaryEvent.RegisterEvent(matchContextEvent);
                    return;
                }
                
                cardToMove.CardEffect.CheckApplyEffect(enterInfirmaryEffectArgs, matchContextEvent);
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }
    }
}