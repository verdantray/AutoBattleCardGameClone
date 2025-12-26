using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 이 카드가 필드에서 벗어날 때 양호실 대신 덱 맨 아래로 이동
    /// </summary>
    public sealed class ReplaceMovementInfirmaryToDeck : CardEffect
    {
        public ReplaceMovementInfirmaryToDeck(Card card, JsonObject json) : base(card, json) { }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            // Do nothing
        }

        public override bool TryReplaceMovement(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return false;
            }

            int indexOfField = ownSide.Field.IndexOf(CallCard);
            CardLocation prevLocation = new FieldLocation(ownSide.Player, indexOfField);
            ownSide.Field.Remove(CallCard);
            
            ownSide.Deck.Add(CallCard);
            int indexOfDeck = ownSide.Deck.Count - 1;
            CardLocation curLocation = new DeckLocation(ownSide.Player, indexOfDeck);
            
            CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(prevLocation);
            CardMovementInfo movementInfo = new CardMovementInfo(prevLocation, curLocation);
            
            SendToDeckInsteadOfInfirmaryEvent cardSendEvent = new SendToDeckInsteadOfInfirmaryEvent(appliedInfo, movementInfo);
            cardSendEvent.RegisterEvent(matchContextEvent);

            // string moveCardToBottomOfDeckMessage = $"{ownSide.Player.Name}가 카드를 덱 맨 아래로 보냄\n{CallCard}";
            // var moveCardEffectEvent = new CommonMatchMessageEvent(moveCardToBottomOfDeckMessage);
            // moveCardEffectEvent.RegisterEvent(matchContextEvent);
            
            return true;
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey);
        }
    }
}
