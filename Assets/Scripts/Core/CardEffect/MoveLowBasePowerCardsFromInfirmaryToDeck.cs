using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에서 기본 파워가 기준 n 이하인 카드 m장을 덱 아래로 넣음
    /// </summary>
    public class MoveLowBasePowerCardsFromInfirmaryToDeck : CardEffect
    {
        private readonly string _failureDescKey;

        private readonly int _cardsAmount;
        private readonly int _powerCriteria;
        
        public MoveLowBasePowerCardsFromInfirmaryToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_CANCEL_TRIGGERS_KEY:
                        _failureDescKey = field.value.strValue;
                        break;
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                    case "power_criteria":
                        _powerCriteria = field.value.intValue;
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

            var cardsInInfirmary = ownSide.Infirmary.GetAllCards();
            if (!cardsInInfirmary.Any(card => card.BasePower <= _powerCriteria))
            {
                var failEffectEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                failEffectEvent.RegisterEvent(matchContextEvent);
                
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

                ownSide.Infirmary[i].Remove(cardToMove);
                if (ownSide.Infirmary[i].Count == 0)
                {
                    ownSide.Infirmary.RemoveByIndex(i);
                }

                ownSide.Deck.Add(cardToMove);
                cardToMove.CardEffect.CheckApplyEffect(onLeaveInfirmaryArgs, matchContextEvent);
                
                moveCount++;
                
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }

                var moveCardEffectEvent = new MoveCardToBottomOfDeckEvent(cardToMove, new MatchSnapshot(ownSide, otherSide));
                moveCardEffectEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            // TODO : use localization and format
            return DescriptionKey;
        }
    }

    public class MoveCardToBottomOfDeckEvent : MatchEventBase
    {
        public MoveCardToBottomOfDeckEvent(Card movedCard, MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }
}
