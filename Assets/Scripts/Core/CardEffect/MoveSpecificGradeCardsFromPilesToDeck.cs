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

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }

            IPlayer ownPlayer = ownSide.Player;
            PlayerState ownPlayerState = gameState.GetPlayerState(ownPlayer);
            GradeCardIdQueue gradeCardPiles = ownPlayerState.GradeCardPiles;

            // TODO : use PCG32
            Random random = new Random();
            
            for (int i = 0; i < _cardsAmount; i++)
            {
                if (!gradeCardPiles[_targetGrade].TryDraw(ownPlayer, out Card drawnCard))
                {
                    CallCard.TryGetCardLocation(ownSide, out var currentLocation);

                    FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                        CallCard.Id,
                        currentLocation,
                        FailToActivateEffectReason.NoCardPileRemains
                    );
                
                    failToActivateEvent.RegisterEvent(matchContextEvent);
                    break;
                }
                
                ownPlayerState.IncludeCardIds.EnqueueCardId(drawnCard.Id);

                // if deck is empty then return default value of int (= 0)
                int randomIndex = Enumerable.Range(0, ownSide.Deck.Count).OrderBy(_ => random.Next()).FirstOrDefault();
                ownSide.Deck.Insert(randomIndex, drawnCard);

                CallCard.TryGetCardLocation(ownSide, out var activatedCardLocation);
                
                // additive-recruit is not trigger card effect of drawn card...
                CardBuffArgs buffArgs = new CardBuffArgs(ownSide, otherSide, gameState);
                var appliedCard = new CardReference(drawnCard, buffArgs);

                CardLocation prevLocation = new CardPileLocation(ownPlayer);
                CardLocation curLocation = new DeckLocation(ownPlayer, randomIndex);
                CardMovementInfo movementInfo = new CardMovementInfo(prevLocation, curLocation);
                
                DrawCardFromPileEvent drawCardEvent = new DrawCardFromPileEvent(activatedCardLocation, appliedCard, movementInfo);
                drawCardEvent.RegisterEvent(matchContextEvent);
            }
        }
    }
}