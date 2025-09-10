using System;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에 있는 카드들의 소속이 n종 이상일 시 일정한 승점 획득
    /// </summary>
    public sealed class GainWinPointsWithClubsFromInfirmary : CardEffect
    {
        private readonly string _failureDescKey;
        
        private readonly ClubType _excludedClubFlag;
        private readonly int _necessaryClubAmount;
        private readonly int _gainWinPoints;
        
        public GainWinPointsWithClubsFromInfirmary(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_FAILURE_DESC_KEY :
                        _failureDescKey = field.value.strValue;
                        break;
                    case "club_excludes":

                        ClubType excludeFlag = 0;

                        foreach (var element in field.value.arr)
                        {
                            excludeFlag |= Enum.Parse<ClubType>(element.strValue, true);
                        }

                        _excludedClubFlag = excludeFlag;
                        break;
                    case "necessary_club_amount":
                        _necessaryClubAmount = field.value.intValue;
                        break;
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

            var cardsInInfirmary = ownSide.Infirmary.GetAllCards();
            int clubsInInfirmary = cardsInInfirmary
                .Select(card => card.ClubType)
                .Where(clubType => !_excludedClubFlag.HasFlag(clubType))
                .Distinct()
                .Count();

            if (clubsInInfirmary < _necessaryClubAmount)
            {
                var failedApplyEffectEvent = new FailToApplyCardEffectEvent(_failureDescKey, new MatchSnapshot(ownSide, otherSide));
                failedApplyEffectEvent.RegisterEvent(matchContextEvent);
                
                return;
            }

            gameState.ScoreBoard.RegisterScoreEntry(
                ownSide.Player,
                new ScoreEntry(_gainWinPoints, ScoreEntry.ScoreReason.ScoreByCardEffect)
            );
            
            MatchSnapshot matchSnapshot = new MatchSnapshot(ownSide, otherSide);
            var gainWinPointsEventEffect = new GainWinPointsByCardEffectEvent(ownSide.Player, _gainWinPoints, matchSnapshot);
            gainWinPointsEventEffect.RegisterEvent(matchContextEvent);
        }

        protected override string GetDescription()
        {
            // TODO : use localization and format
            return DescriptionKey;
        }
    }
    
    public class GainWinPointsByCardEffectEvent : MatchEventBase
    {
        public readonly IPlayer GainedPlayer;
        public readonly int WinPoints;
            
        public GainWinPointsByCardEffectEvent(IPlayer player, int winPoints, MatchSnapshot snapshot) : base(snapshot)
        {
            GainedPlayer = player;
            WinPoints = winPoints;
        }
    }
}
