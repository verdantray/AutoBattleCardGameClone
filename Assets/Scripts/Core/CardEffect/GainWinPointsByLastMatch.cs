using System;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class GainWinPointsByLastMatch : CardEffect
    {
        private readonly LastRoundResult _enableLastResult;
        private readonly int _gainWinPoints;
        
        public GainWinPointsByLastMatch(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "enable_last_result":
                        _enableLastResult = Enum.Parse<LastRoundResult>(field.value.strValue, true);
                        break;
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

            IPlayer ownPlayer = ownSide.Player;
            int lastRound = gameState.Round - 1;

            if (lastRound <= 0)
            {
                return;
            }

            var lastMatchResult = gameState.MatchResults.GetMatchResult(lastRound, ownPlayer);
            bool isEnableResultLastMatch = _enableLastResult == LastRoundResult.Win
                ? lastMatchResult.Winner == ownPlayer
                : lastMatchResult.Loser == ownPlayer;

            if (!isEnableResultLastMatch)
            {
                var failedCard = new CardReference(CallCard, new CardBuffArgs(ownSide, otherSide, gameState));

                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    failedCard,
                    FailToActivateEffectReason.NoMeetCondition
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // FailToApplyCardEffectEvent failEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoMeetCondition);
                // failEffectEvent.RegisterEvent(matchContextEvent);
                return;
            }

            ScoreEntry scoreEntry = new ScoreEntry(_gainWinPoints, ScoreEntry.ScoreReason.ScoreByCardEffect);
            gameState.ScoreBoard.RegisterScoreEntry(ownPlayer, scoreEntry);
            
            GainWinPointsByCardEffectEvent gainWinPointEvent = new GainWinPointsByCardEffectEvent(ownPlayer, _gainWinPoints);
            gainWinPointEvent.RegisterEvent(matchContextEvent);
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _gainWinPoints);
        }
    }
}
