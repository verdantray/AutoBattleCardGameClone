using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public interface IMatchEvent
    {
        public MatchFlowSnapshot Snapshot { get; }
    }

    public class MatchFlowContextEvent : IContextEvent
    {
        public readonly IPlayer[] Participants;
        public IPlayer Winner { get; private set; } = null;
        public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();

        public MatchFlowContextEvent(params IPlayer[] participants)
        {
            Participants = participants;
        }

        public bool IsParticipants(IPlayer player)
        {
            return Participants.Contains(player);
        }

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

    public abstract class MatchEventBase : IMatchEvent
    {
        public MatchFlowSnapshot Snapshot { get; }

        public MatchEventBase(MatchFlowSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    public class CommonMatchEvent : MatchEventBase
    {
        public CommonMatchEvent(string message, MatchFlowSnapshot snapshot) : base(snapshot)
        {
            
        }
    }

    public class MatchStartEvent : MatchEventBase
    {
        public MatchStartEvent(IPlayer defendingPlayer, IPlayer attackingPlayer, MatchFlowSnapshot snapshot) : base(snapshot)
        {
            
        }
    }
    
    public class DrawCardEvent : MatchEventBase
    {
        public readonly IPlayer DrawPlayer;
        public readonly CardInstance DrawCard;
        
        public DrawCardEvent(IPlayer drawPlayer, MatchFlowSnapshot snapshot) : base(snapshot)
        {
            DrawPlayer = drawPlayer;
            DrawCard = snapshot.MatchSideSnapShots[DrawPlayer].Field[^1];
        }
    }

    public class ComparePowerEvent : MatchEventBase
    {
        public ComparePowerEvent(MatchSide defendingSide, MatchSide attackingSide,  MatchFlowSnapshot snapshot) : base(snapshot)
        {
            string defendingPlayerName = defendingSide.Player.Name;
            string attackingPlayerName = attackingSide.Player.Name;
        }
    }

    public class TryPutCardInfirmaryEvent : MatchEventBase
    {
        public TryPutCardInfirmaryEvent(MatchSide defendingSide, MatchFlowSnapshot snapshot) : base(snapshot)
        {
            string playerName = defendingSide.Player.Name;
            List<Card> cardsToPut = defendingSide.Field;
        }
    }

    public class SwitchPositionEvent : MatchEventBase
    {
        public readonly IPlayer DefendingPlayer;
        public readonly IPlayer AttackingPlayer;
        
        public SwitchPositionEvent(IPlayer defendingPlayer, IPlayer attackingPlayer, MatchFlowSnapshot snapshot) : base(snapshot)
        {
            DefendingPlayer = defendingPlayer;
            AttackingPlayer = attackingPlayer;
        }
    }

    public class MatchFinishEvent : MatchEventBase
    {
        public readonly IPlayer WinningPlayer; 
        public readonly IPlayer LosingPlayer;
        public readonly MatchEndReason Reason;
        
        public enum MatchEndReason
        {
            EndByEmptyHand,
            EndByFullOfInfirmary
        }
        
        public MatchFinishEvent(IPlayer winningPlayer, IPlayer losingPlayer, MatchEndReason reason, MatchFlowSnapshot snapshot) : base(snapshot)
        {
            WinningPlayer = winningPlayer;
            LosingPlayer = losingPlayer;
            Reason = reason;
        }
    }
}
