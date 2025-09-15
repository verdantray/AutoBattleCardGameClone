using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 이 카드가 필드에서 벗어날 때 양호실 대신 덱 맨 아래로 이동
    /// </summary>
    public sealed class ReplaceMovementInfirmaryToDeck : CardEffect
    {
        public ReplaceMovementInfirmaryToDeck(Card card, JsonObject json) : base(card, json) { }

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            // Do nothing
        }

        public override bool TryReplaceMovement(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return false;
            }

            ownSide.Field.Remove(CallCard);
            ownSide.Deck.Add(CallCard);

            MoveCardToBottomOfDeckEvent moveCardEvent = new MoveCardToBottomOfDeckEvent(CallCard, new MatchSnapshot(gameState, ownSide, otherSide));
            moveCardEvent.RegisterEvent(matchContextEvent);
            
            return true;
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey);
        }
    }
}
