using System;
using System.Collections.Generic;

namespace ProjectABC.Core
{
    public interface IMatchEventSet
    {
        public List<IMatchEvent> MatchEvents { get; }
        public void AddEvent(IMatchEvent matchEvent);
    }
    
    public interface IMatchEvent
    {
        
    }

    public class MatchResult : IMatchEventSet
    {
        public IPlayer Winner { get; private set; } = null;
        public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();

        public void SetWinner(IPlayer player)
        {
            Winner = player;
        }
        
        public void AddEvent(IMatchEvent matchEvent)
        {
            MatchEvents.Add(matchEvent);
        }
    }
    
    public class MatchFlowConsoleEvent : ConsoleContextEventBase
    {
        public MatchFlowConsoleEvent(List<IMatchEvent> matchEvents)
        {
            Message = string.Join("\n\n", matchEvents);
        }
    }

    public abstract class MatchConsoleEvent : IMatchEvent
    {
        protected string Message;
        
        public override string ToString()
        {
            return Message;
        }
    }

    public class MatchStartConsoleEvent : MatchConsoleEvent
    {
        public MatchStartConsoleEvent(IPlayer defendingPlayer, IPlayer attackingPlayer)
        {
            Message = $"매치 시작 - {defendingPlayer.Name} vs {attackingPlayer.Name}\n"
                      + $"수비 : {defendingPlayer.Name}\n"
                      + $"공격 : {attackingPlayer.Name}";
        }
    }
    
    public class DrawCardConsoleEvent : MatchConsoleEvent
    {
        public DrawCardConsoleEvent(MatchSide drawSide)
        {
            string playerName = drawSide.Player.Name;
            Card drawCard = drawSide.Field[^1];

            Message = $"플레이어 '{playerName}'가 {drawCard.Name}을 필드에 드로우\n"
                      + $"카드 : {drawCard}\n"
                      + $"'{playerName}'의 필드 파워 총 합 : {drawSide.GetPower()}";
        }
    }

    public class ComparePowerConsoleEvent : MatchConsoleEvent
    {
        public ComparePowerConsoleEvent(MatchSide defendingSide, MatchSide attackingSide)
        {
            string defendingPlayerName = defendingSide.Player.Name;
            string attackingPlayerName = attackingSide.Player.Name;

            int defendingPower = defendingSide.GetPower();
            int attackingPower = attackingSide.GetPower();

            Message = $"수비 플레이어 '{defendingPlayerName}' 파워 : {defendingPower}\n"
                      + $"공격 플레이어 '{attackingPlayerName}' 파워 : {attackingPower}\n"
                      + $"{(defendingPower > attackingPower ? "수비 성공" : "공격 성공")}";
        }
    }

    public class TryPutCardInfirmaryConsoleEvent : MatchConsoleEvent
    {
        public TryPutCardInfirmaryConsoleEvent(MatchSide defendingSide)
        {
            string playerName = defendingSide.Player.Name;
            List<Card> cardsToPut = defendingSide.Field;

            Message = $"플레이어 '{playerName}' 가 수비에 실패하여 다음 카드들을 양호실로 보냄\n"
                      + string.Join('\n', cardsToPut);
        }
    }

    public class SwitchPositionConsoleEvent : MatchConsoleEvent
    {
        public SwitchPositionConsoleEvent(IPlayer defendingPlayer, IPlayer attackingPlayer)
        {
            Message = "플래그 이동으로 인한 공수 교대\n"
                      + $"수비 : {defendingPlayer.Name}\n"
                      + $"공격 : {attackingPlayer.Name}";
        }
    }

    public class MatchFinishConsoleEvent : MatchConsoleEvent
    {
        public enum MatchEndReason
        {
            EndByEmptyHand,
            EndByFullOfInfirmary
        }
        
        public MatchFinishConsoleEvent(MatchSide winningSide, MatchSide losingSide, MatchEndReason reason)
        {
            string winnerName = winningSide.Player.Name;
            string otherName = losingSide.Player.Name;
            string matchEndReason = reason switch
            {
                MatchEndReason.EndByEmptyHand => $"플레이어 '{otherName}'의 덱이 모두 소진되었습니다.",
                MatchEndReason.EndByFullOfInfirmary => $"플레이어 '{otherName}'의 양호실 슬롯이 모두 찼습니다.",
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };
            
            Message = $"플레이어 '{winnerName}'의 승리!\n"
                      + $"매치 종료 이유 : {matchEndReason}";
        }
    }
}
