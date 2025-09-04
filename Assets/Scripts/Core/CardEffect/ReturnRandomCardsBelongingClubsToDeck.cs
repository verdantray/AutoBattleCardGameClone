using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에 있는 특정 소속 카드 중 무작위 n장을 덱 맨 아래로 넣음
    /// </summary>
    public class ReturnRandomCardsBelongingClubsToDeck : CardEffect
    {
        private readonly string _failureDescKey;
        private readonly int _cardsAmount;
        private readonly ClubType _includedClubFlag;
        
        public ReturnRandomCardsBelongingClubsToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_FAILURE_DESC_KEY:
                        _failureDescKey = field.value.strValue;
                        break;
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                    case "club_includes":

                        ClubType includeFlag = 0;
                        
                        foreach (var element in field.value.arr)
                        {
                            includeFlag |= Enum.Parse<ClubType>(element.strValue, true);
                        }

                        _includedClubFlag = includeFlag;
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

            Random random = new Random();
            var cardsBelongClubsInInfirmary = ownSide.Infirmary.GetAllCards()
                .Where(card => _includedClubFlag.HasFlag(card.ClubType))
                .OrderBy(_ => random.Next())
                .ToArray();

            if (!cardsBelongClubsInInfirmary.Any())
            {
                var failEffectEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                failEffectEvent.RegisterEvent(matchContextEvent);
                
                return;
            }

            List<Card> cardsToMove = new List<Card>();
            
            for (int i = 0; i < _cardsAmount; i++)
            {
                for (int j = ownSide.Infirmary.Count; j >= 0; j--)
                {
                    if (!ownSide.Infirmary[j].Contains(cardsBelongClubsInInfirmary[i]))
                    {
                        continue;
                    }
                    
                    ownSide.Infirmary[j].Remove(cardsBelongClubsInInfirmary[i]);
                    cardsToMove.Add(cardsBelongClubsInInfirmary[i]);

                    if (ownSide.Infirmary[j].Count > 0)
                    {
                        continue;
                    }
                    
                    ownSide.Infirmary.RemoveByIndex(j);
                }
            }

            CardEffectArgs onLeaveInfirmaryArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveInfirmary,
                ownSide,
                otherSide,
                gameState
            );
            
            foreach (var card in cardsToMove)
            {
                ownSide.Deck.Add(card);
                card.CardEffect.CheckApplyEffect(onLeaveInfirmaryArgs, matchContextEvent);
            }

            var moveCardEffectEvent = new MoveCardsToBottomOfDeckEvent(cardsToMove, new MatchSnapshot(ownSide, otherSide));
            moveCardEffectEvent.RegisterEvent(matchContextEvent);
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
    }
}
