using System;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class MoveSpecificGradeCardsFromPilesToDeck : CardEffect
    {
        private readonly GradeType _targetGrade;
        private readonly int _cardsAmount;
        
        public MoveSpecificGradeCardsFromPilesToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "target_grade":
                        _targetGrade = field.value.strValue.ParseGradeType();
                        break;
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

            IPlayer ownPlayer = ownSide.Player;
            PlayerState ownPlayerState = gameState.GetPlayerState(ownPlayer);
            GradeCardPiles gradeCardPiles = ownPlayerState.GradeCardPiles;

            // TODO : use PCG32
            Random random = new Random();
            
            for (int i = 0; i < _cardsAmount; i++)
            {
                if (!gradeCardPiles[_targetGrade].TryDraw(out Card drawnCard))
                {
                    FailToApplyCardEffectEvent failEffectEvent = new FailToApplyCardEffectEvent(
                        FailToApplyCardEffectEvent.FailReason.NoCardPileRemains,
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
            string gradeText = LocalizationHelper.Instance.Localize(_targetGrade.GetLocalizationKey());

            return LocalizationHelper.Instance.Localize(DescriptionKey, gradeText, _cardsAmount);
        }
    }

    public class MoveCardToRandomOfDeck : MatchEventBase
    {
        public MoveCardToRandomOfDeck(Card card, MatchSnapshot snapshot) : base(snapshot)
        {
        }
    }
}