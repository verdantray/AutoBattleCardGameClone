using System;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class MoveSpecificGradeCardsFromPilesToDeck : CardEffect
    {
        private readonly string _failureDescriptionKey;
        private readonly GradeType _targetGrade;
        private readonly int _cardAmount;
        
        public MoveSpecificGradeCardsFromPilesToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "fail_desc_key":
                        _failureDescriptionKey = field.value.strValue;
                        break;
                    case "target_grade":
                        _targetGrade = field.value.strValue.ParseGradeType();
                        break;
                    case "card_amount":
                        _cardAmount = field.value.intValue;
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

            IPlayer ownPlayer = ownSide.Player;
            PlayerState ownPlayerState = gameState.GetPlayerState(ownPlayer);
            GradeCardPiles gradeCardPiles = ownPlayerState.GradeCardPiles;

            // TODO : use PCG32
            Random random = new Random();
            
            for (int i = 0; i < _cardAmount; i++)
            {
                if (!gradeCardPiles[_targetGrade].TryDraw(out Card drawnCard))
                {
                    FailToApplyCardEffectEvent failEffectEvent = new FailToApplyCardEffectEvent(
                        _failureDescriptionKey,
                        new MatchSnapshot(ownSide, otherSide)
                    );
                    
                    failEffectEvent.RegisterEvent(matchContextEvent);
                    break;
                }
                
                ownPlayerState.Deck.Add(drawnCard);
                
                int randomIndex = Enumerable.Range(0, ownSide.Deck.Count - 1).OrderBy(_ => random.Next()).First();
                ownSide.Deck.Insert(randomIndex, drawnCard);
                
                // additive-recruit is not trigger card effect of drawn card...
                // drawnCard.CardEffect.TryApplyEffectOnRecruit(ownPlayer, gameState, out var contextEvent);

                MoveCardToRandomOfDeck moveCardEvent = new MoveCardToRandomOfDeck(drawnCard, new MatchSnapshot(ownSide, otherSide));
                moveCardEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            // localization
            return DescriptionKey;
        }
    }

    public class MoveCardToRandomOfDeck : MatchEventBase
    {
        public MoveCardToRandomOfDeck(Card card, MatchSnapshot snapshot) : base(snapshot)
        {
        }
    }
}