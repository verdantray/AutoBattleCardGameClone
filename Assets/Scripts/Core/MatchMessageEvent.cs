using System;

namespace ProjectABC.Core
{
    public abstract class MatchMessageEvent : IMatchEvent
    {
        public string Message { get; protected set; }

        public MatchMessageEvent() { }
        
        public MatchMessageEvent(string message)
        {
            Message = message;
        }
        
        public virtual void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            if (matchContextEvent.MatchFinished)
            {
                return;
            }
            
            matchContextEvent.MatchEvents.Add(this);
        }
    }

    public class CommonMatchMessageEvent : MatchMessageEvent
    {
        public CommonMatchMessageEvent(string message) : base(message)
        {
            
        }
    }

    public class SwitchPositionMessageEvent : MatchMessageEvent
    {
        public SwitchPositionMessageEvent(IPlayer defender, IPlayer attacker)
        {
            Message = $"공수가 교대됩니다. 공격 : {attacker.Name} / 방어 : {defender.Name}";
        }
    }

    public class MatchFinishMessageEvent : MatchMessageEvent
    {
        public readonly IPlayer WinPlayer;
        public readonly IPlayer LosePlayer;
        public readonly MatchEndReason MatchEndReason;
        
        public MatchFinishMessageEvent(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason)
        {
            WinPlayer = winPlayer;
            LosePlayer = losePlayer;
            MatchEndReason = reason;

            string reasonText = MatchEndReason switch
            {
                MatchEndReason.EndByEmptyDeck => "덱을 모두 소진시켰습니다.",
                MatchEndReason.EndByFullOfInfirmary => "양호실의 슬롯을 모두 소진시켰습니다.",
                _ => throw new ArgumentOutOfRangeException()
            };

            Message = $"플레이어 '{winPlayer.Name}'의 승리. "
                      + $"상대 플레이어 '{losePlayer.Name}'의 {reasonText}";
        }

        public override void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            if (matchContextEvent.MatchFinished)
            {
                return;
            }
            
            matchContextEvent.SetResult(WinPlayer, LosePlayer, MatchEndReason);
            matchContextEvent.MatchEvents.Add(this);
        }
    }

    public class FailToApplyCardEffectEvent : MatchMessageEvent
    {
        public enum FailReason
        {
            NoMeetCondition,
            NoDeckRemains,
            NoCardPileRemains,
            NoOpponentCardPileRemains,
            NoInfirmaryRemains,
            NoOpponentInfirmaryRemains,
        }

        public FailToApplyCardEffectEvent(FailReason reason)
        {
            string localizationKey = reason switch
            {
                FailReason.NoMeetCondition => GameConst.CardEffect.FAIL_REASON_NO_MEET_CONDITION,
                FailReason.NoDeckRemains => GameConst.CardEffect.FAIL_REASON_NO_DECK_REMAINS,
                FailReason.NoCardPileRemains => GameConst.CardEffect.FAIL_REASON_NO_CARD_PILE_REMAINS,
                FailReason.NoOpponentCardPileRemains => GameConst.CardEffect.FAIL_REASON_NO_OPPONENT_CARD_PILE_REMAINS,
                FailReason.NoInfirmaryRemains => GameConst.CardEffect.FAIL_REASON_NO_INFIRMARY_REMAINS,
                FailReason.NoOpponentInfirmaryRemains => GameConst.CardEffect.FAIL_REASON_NO_OPPONENT_INFIRMARY_REMAINS,
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };
            
            Message = LocalizationHelper.Instance.Localize(localizationKey);
        }
    }
    
    public class GainWinPointsByCardEffectEvent : MatchMessageEvent
    {
        public readonly IPlayer GainedPlayer;
        public readonly int WinPoints;
            
        public GainWinPointsByCardEffectEvent(IPlayer player, int winPoints)
        {
            GainedPlayer = player;
            WinPoints = winPoints;

            Message = $"{GainedPlayer.Name}이 카드효과로 인해 승점 {WinPoints} 획득";
        }
    }
}