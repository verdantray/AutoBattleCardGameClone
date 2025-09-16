using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 카드 영입 시 승점 n 획득
    /// </summary>
    public sealed class GainWinPointsOnRecruit : CardEffect
    {
        private readonly int _gainWinPoints;
        
        public GainWinPointsOnRecruit(Card card, JsonObject json) : base(card, json)
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
            // Do nothing
        }

        public override bool TryApplyEffectOnRecruit(IPlayer recruiter, GameState gameState, out IContextEvent contextEvent)
        {
            ScoreEntry scoreEntry = new ScoreEntry(_gainWinPoints, ScoreEntry.ScoreReason.ScoreByCardEffect);
            gameState.ScoreBoard.RegisterScoreEntry(recruiter, scoreEntry);

            // TODO: implements ContextEvent for show score
            contextEvent = new CommonConsoleEvent($"{recruiter.Name} gain win points {_gainWinPoints}");
            return true;
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _gainWinPoints);
        }
    }
}
