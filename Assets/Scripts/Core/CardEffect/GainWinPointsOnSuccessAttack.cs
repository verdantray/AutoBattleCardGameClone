using ProjectABC.Data;


namespace ProjectABC.Core
{
    public class GainWinPointsOnSuccessAttack : CardEffect
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

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }

            if (ownSide.Field[^1] != CallCard)
            {
                return;
            }

            ScoreEntry scoreEntry = new ScoreEntry(_gainWinPoints, ScoreEntry.ScoreReason.ScoreByCardEffect);
            gameState.ScoreBoard.RegisterScoreEntry(ownSide.Player, scoreEntry);

            GainWinPointsByCardEffectEvent matchEvent = new GainWinPointsByCardEffectEvent(
                ownSide.Player,
                _gainWinPoints,
                new MatchSnapshot(ownSide, otherSide)
            );
            
            matchEvent.RegisterEvent(matchContextEvent);
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
    }
}
