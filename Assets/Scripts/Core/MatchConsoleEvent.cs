using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    // public interface IMatchEvent
    // {
    //     public MatchFlowSnapshot Snapshot { get; }
    //
    //     public void RegisterEvent(MatchFlowContextEvent matchFlowEvent)
    //     {
    //         matchFlowEvent.MatchEvents.Add(this);
    //     }
    // }
    //
    // public class MatchFlowContextEvent : IContextEvent
    // {
    //     public readonly IPlayer[] Participants;
    //     public readonly Dictionary<IPlayer, int> GainWinPointsMap;
    //     
    //     public IPlayer Winner { get; private set; } = null;
    //     public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();
    //
    //     public MatchFlowContextEvent(params IPlayer[] participants)
    //     {
    //         Participants = participants;
    //         GainWinPointsMap = new Dictionary<IPlayer, int>(participants.ToDictionary(player => player, _ => 0));
    //     }
    //
    //     public bool IsParticipants(IPlayer player)
    //     {
    //         return Participants.Contains(player);
    //     }
    //
    //     public void AddGainWinPoints(IPlayer player, int gainWinPoints)
    //     {
    //         if (!IsParticipants(player))
    //         {
    //             throw new ArgumentException($"{player.Name} is not included in match...");
    //         }
    //         
    //         GainWinPointsMap[player] += gainWinPoints;
    //     }
    //
    //     public void SetWinner(IPlayer player)
    //     {
    //         Winner = player;
    //     }
    //     
    //     [Obsolete("Use IMatchEvent.RegisterEvent instead ")]
    //     public void AddEvent(IMatchEvent matchEvent)
    //     {
    //         MatchEvents.Add(matchEvent);
    //     }
    // }
    //
    // public class MatchFlowConsoleEvent : ConsoleContextEventBase
    // {
    //     public MatchFlowConsoleEvent(List<IMatchEvent> matchEvents)
    //     {
    //         Message = string.Join("\n\n", matchEvents);
    //     }
    // }
    //
    // public abstract class MatchConsoleEvent : IMatchEvent
    // {
    //     public MatchFlowSnapshot Snapshot { get; }
    //     protected string Message;
    //
    //     public MatchConsoleEvent(MatchFlowSnapshot snapshot)
    //     {
    //         Snapshot = snapshot;
    //     }
    //     
    //     public override string ToString()
    //     {
    //         return Message;
    //     }
    // }
    //
    // public class CommonMatchConsoleEvent : MatchConsoleEvent
    // {
    //     public CommonMatchConsoleEvent(string message, MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         Message = message;
    //     }
    // }
    //
    // public class MatchStartConsoleEvent : MatchConsoleEvent
    // {
    //     public MatchStartConsoleEvent(IPlayer defendingPlayer, IPlayer attackingPlayer, MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         Message = $"매치 시작 - {defendingPlayer.Name} vs {attackingPlayer.Name}\n"
    //                   + $"수비 : {defendingPlayer.Name}\n"
    //                   + $"공격 : {attackingPlayer.Name}";
    //     }
    // }
    //
    // public class DrawCardConsoleEvent : MatchConsoleEvent
    // {
    //     public DrawCardConsoleEvent(MatchSide drawSide, MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         string playerName = drawSide.Player.Name;
    //         Card drawCard = drawSide.Field[^1];
    //
    //         Message = $"플레이어 '{playerName}'가 {drawCard.Name}을 필드에 드로우\n"
    //                   + $"카드 : {drawCard}\n"
    //                   + $"'{playerName}'의 필드 파워 총 합 : {drawSide.GetEffectivePower()}";
    //     }
    // }
    //
    // public class ComparePowerConsoleEvent : MatchConsoleEvent
    // {
    //     public ComparePowerConsoleEvent(MatchSide defendingSide, MatchSide attackingSide,  MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         string defendingPlayerName = defendingSide.Player.Name;
    //         string attackingPlayerName = attackingSide.Player.Name;
    //
    //         int defendingPower = defendingSide.GetEffectivePower();
    //         int attackingPower = attackingSide.GetEffectivePower();
    //
    //         Message = $"수비 플레이어 '{defendingPlayerName}' 파워 : {defendingPower}\n"
    //                   + $"공격 플레이어 '{attackingPlayerName}' 파워 : {attackingPower}\n"
    //                   + $"{(defendingPower > attackingPower ? "수비 성공" : "공격 성공")}";
    //     }
    // }
    //
    // public class TryPutCardInfirmaryConsoleEvent : MatchConsoleEvent
    // {
    //     public TryPutCardInfirmaryConsoleEvent(MatchSide defendingSide, MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         string playerName = defendingSide.Player.Name;
    //         List<Card> cardsToPut = defendingSide.Field;
    //
    //         Message = $"플레이어 '{playerName}' 가 수비에 실패하여 다음 카드들을 양호실로 보냄\n"
    //                   + string.Join('\n', cardsToPut);
    //     }
    // }
    //
    // public class SwitchPositionConsoleEvent : MatchConsoleEvent
    // {
    //     public SwitchPositionConsoleEvent(IPlayer defendingPlayer, IPlayer attackingPlayer, MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         Message = "공수 교대\n"
    //                   + $"수비 : {defendingPlayer.Name}\n"
    //                   + $"공격 : {attackingPlayer.Name}";
    //     }
    // }
    //
    // public class MatchFinishConsoleEvent : MatchConsoleEvent
    // {
    //     public enum MatchEndReason
    //     {
    //         EndByEmptyHand,
    //         EndByFullOfInfirmary
    //     }
    //     
    //     public MatchFinishConsoleEvent(MatchSide winningSide, MatchSide losingSide, MatchEndReason reason, MatchFlowSnapshot snapshot) : base(snapshot)
    //     {
    //         string winnerName = winningSide.Player.Name;
    //         string otherName = losingSide.Player.Name;
    //         string matchEndReason = reason switch
    //         {
    //             MatchEndReason.EndByEmptyHand => $"플레이어 '{otherName}'의 덱이 모두 소진되었습니다.",
    //             MatchEndReason.EndByFullOfInfirmary => $"플레이어 '{otherName}'의 양호실 슬롯이 모두 찼습니다.",
    //             _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    //         };
    //         
    //         Message = $"플레이어 '{winnerName}'의 승리!\n"
    //                   + $"매치 종료 이유 : {matchEndReason}";
    //     }
    // }
}
