using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class GainWinPointsOnSuccessAttack : CardEffect
    {
        private readonly int _gainWinPoints;
        
        public GainWinPointsOnSuccessAttack(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "gain_win_points":
                        _gainWinPoints = field.value.intValue;
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
            
            CallCard.TryGetCardLocation(ownSide, out var currentLocation);
            
            if (ownSide.Field[^1] != CallCard)
            {
                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    currentLocation,
                    FailToActivateEffectReason.NoMeetCondition
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // FailToApplyCardEffectEvent failEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoMeetCondition);
                // failEffectEvent.RegisterEvent(matchContextEvent);
                return;
            }

            ScoreEntry scoreEntry = new ScoreEntry(_gainWinPoints, ScoreEntry.ScoreReason.ScoreByCardEffect);
            gameState.ScoreBoard.RegisterScoreEntry(ownSide.Player, scoreEntry);

            int totalWinPoints = gameState.ScoreBoard.GetTotalWinPoints(ownSide.Player);
            GainWinPointsByCardEffectEvent gainWinPointsEvent = new GainWinPointsByCardEffectEvent(
                ownSide.Player,
                CallCard.Id,
                currentLocation,
                _gainWinPoints,
                totalWinPoints
            );
            
            gainWinPointsEvent.RegisterEvent(matchContextEvent);

            // GainWinPointsByCardEffectMessageEvent matchEvent = new GainWinPointsByCardEffectMessageEvent(ownSide.Player, _gainWinPoints);
            // matchEvent.RegisterEvent(matchContextEvent);
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _gainWinPoints);
        }
    }
}
