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
                    MatchFinishMessageEvent finishByEmptyDeckEvent = new MatchFinishMessageEvent(
                        ownSide.Player,
                        otherSide.Player,
                        MatchEndReason.EndByEmptyDeck
                    );
                    
                    finishByEmptyDeckEvent.RegisterEvent(matchContextEvent);
                    return;
                }

                bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveOpponentFieldEffectArgs, matchContextEvent);
                if (isMovementReplaced)
                {
                    ownSide.CheckApplyCardBuffs(otherSide, gameState);
                    otherSide.CheckApplyCardBuffs(ownSide, gameState);

                    // TODO: register match event if need to announce applying buffs
                    continue;
                }
                
                otherSide.Infirmary.PutCard(cardToMove);

                string putCardToInfirmaryMessage = $"{otherSide.Player.Name}가 카드를 양호실에 넣음. \n{cardToMove}";
                CommonMatchMessageEvent putCardToInfirmaryEvent = new CommonMatchMessageEvent(putCardToInfirmaryMessage);
                putCardToInfirmaryEvent.RegisterEvent(matchContextEvent);

                if (!otherSide.Infirmary.IsSlotRemains)
                {
                    MatchFinishMessageEvent finishByFullOfInfirmaryEvent = new MatchFinishMessageEvent(
                        ownSide.Player,
                        otherSide.Player,
                        MatchEndReason.EndByFullOfInfirmary
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
            return LocalizationHelper.Instance.Localize(DescriptionKey, _cardsAmount);
        }
    } 
}