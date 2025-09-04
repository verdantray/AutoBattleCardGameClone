using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
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

        public override bool TryApplyEffect(CardEffectArgs args, out IMatchEvent matchEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                matchEvent = null;
                return false;
            }

            Random random = new Random();
            var cardsBelongClubsInInfirmary = ownSide.Infirmary.GetAllCards()
                .Where(card => _includedClubFlag.HasFlag(card.ClubType))
                .OrderBy(_ => random.Next())
                .ToArray();

            if (!cardsBelongClubsInInfirmary.Any())
            {
                matchEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                return false;
            }

            List<Card> cardsToMove = new List<Card>();
            
            for (int i = 0; i < _cardsAmount; i++)
            {
                foreach (var (key, cardPile) in ownSide.Infirmary)
                {
                    if (!cardPile.Contains(cardsBelongClubsInInfirmary[i]))
                    {
                        continue;
                    }

                    cardPile.Remove(cardsBelongClubsInInfirmary[i]);
                    cardsToMove.Add(cardsBelongClubsInInfirmary[i]);
                    
                    if (cardPile.Count == 0)
                    {
                        ownSide.Infirmary.Remove(key);
                    }
                    
                    break;
                }
            }
            
            ownSide.Deck.AddRange(cardsToMove);

            matchEvent = new MoveCardsToBottomOfDeckEvent(cardsToMove, new MatchSnapshot(ownSide, otherSide));
            return true;
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
    }
}
