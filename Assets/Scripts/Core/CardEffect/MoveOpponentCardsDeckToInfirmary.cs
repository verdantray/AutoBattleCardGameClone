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

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
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
                    MatchFinishEvent finishByEmptyDeck = new MatchFinishEvent(
                        ownSide.Player,
                        otherSide.Player,
                        MatchEndReason.EndByEmptyDeck
                    );
                    
                    finishByEmptyDeck.RegisterEvent(matchContextEvent);
                    return;
                }

                bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveOpponentFieldEffectArgs, matchContextEvent);
                if (isMovementReplaced)
                {
                    IMatchContextEvent.CheckApplyBuffs(gameState, matchContextEvent, ownSide, otherSide);
                    continue;
                }
                
                CardLocation prevAppliedCardLocation = new DeckLocation(otherSide.Player, otherSide.Deck.Count);
                otherSide.Infirmary.PutCard(cardToMove, out var infirmaryLocation);

                CallCard.TryGetCardLocation(ownSide, out CardLocation activatedCardLocation);
                CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(prevAppliedCardLocation, activatedCardLocation);
                CardMovementInfo movementInfo = new CardMovementInfo(prevAppliedCardLocation, infirmaryLocation);
                
                MoveCardByEffectEvent moveCardEvent = new MoveCardByEffectEvent(appliedInfo, movementInfo);
                moveCardEvent.RegisterEvent(matchContextEvent);

                if (!otherSide.Infirmary.IsSlotRemains)
                {
                    MatchFinishEvent finishByFullInfirmary = new MatchFinishEvent(
                        ownSide.Player,
                        otherSide.Player,
                        MatchEndReason.EndByFullOfInfirmary
                    );
                    
                    finishByFullInfirmary.RegisterEvent(matchContextEvent);
                    return;
                }
                
                cardToMove.CardEffect.CheckApplyEffect(enterOpponentInfirmaryEffectArgs, matchContextEvent);
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }
    } 
}