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
        private readonly ClubType _excludedClubFlag;
        private readonly int _necessaryClubAmount;
        private readonly int _gainWinPoints;
        
        public GainWinPointsWithClubsFromInfirmary(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
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

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
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
            
            CallCard.TryGetCardLocation(ownSide, out var currentLocation);
            
            if (clubsInInfirmary < _necessaryClubAmount)
            {
                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    currentLocation,
                    FailToActivateEffectReason.NoMeetCondition
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // var failedApplyEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoMeetCondition);
                // failedApplyEffectEvent.RegisterEvent(matchContextEvent);
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
            
            // var gainWinPointsEventEffect = new GainWinPointsByCardEffectMessageEvent(ownSide.Player, _gainWinPoints);
            // gainWinPointsEventEffect.RegisterEvent(matchContextEvent);
        }

        protected override string GetDescription()
        {
            var excludeClubs = ((ClubType[])Enum.GetValues(typeof(ClubType)))
                .Where(club => _excludedClubFlag.HasFlag(club))
                .Select(club => LocalizationHelper.Instance.Localize(club.GetLocalizationKey()));

            return LocalizationHelper.Instance.Localize(
                DescriptionKey,
                string.Join(", ", excludeClubs),
                _necessaryClubAmount,
                _gainWinPoints
            );
        }
    }
}
