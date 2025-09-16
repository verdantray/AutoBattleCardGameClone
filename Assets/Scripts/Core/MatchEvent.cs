using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public interface IMatchContextEvent : IContextEvent
    {
        public MatchResult Result { get; }
        public bool MatchFinished { get; }
        public List<IMatchEvent> MatchEvents { get; }

        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason);
    }
    
    public interface IMatchEvent
    {
        public void RegisterEvent(IMatchContextEvent matchContextEvent);
    }

    public abstract class MatchEventBase : IMatchEvent
    {
        public MatchSnapshot Snapshot { get; }

        protected MatchEventBase(MatchSnapshot snapshot)
        {
            Snapshot = snapshot;
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

    public class MatchStartEvent : MatchEventBase
    {
        public MatchStartEvent(MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }
    
    public class DrawCardEvent : MatchEventBase
    {
        public readonly IPlayer DrawPlayer;
        public readonly CardSnapshot DrawCard;
        
        public DrawCardEvent(IPlayer drawPlayer, MatchSnapshot snapshot) : base(snapshot)
        {
            DrawPlayer = drawPlayer;
            DrawCard = snapshot.MatchSideSnapShots[DrawPlayer].Field[^1];
        }
    }

    public class CheckApplyBuffEvent : MatchEventBase
    {
        public CheckApplyBuffEvent(MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }

    public class ComparePowerEvent : MatchEventBase
    {
        public ComparePowerEvent(MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }

    public class TryPutCardInfirmaryEvent : MatchEventBase
    {
        public TryPutCardInfirmaryEvent(IPlayer player, Card card, MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }

    public class SwitchPositionEvent : MatchEventBase
    {
        public SwitchPositionEvent(MatchSnapshot snapshot) : base(snapshot)
        {
            
        }
    }

    public class MatchFinishEvent : MatchEventBase
    {
        public readonly IPlayer WinningPlayer;
        public readonly MatchEndReason Reason;
        
        public MatchFinishEvent(IPlayer winningPlayer, MatchEndReason reason, MatchSnapshot snapshot) : base(snapshot)
        {
            WinningPlayer = winningPlayer;
            Reason = reason;
        }

        public override void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            var players = Snapshot.MatchSideSnapShots.Keys;
            IPlayer otherPlayer = players.First(player => player != WinningPlayer);
            
            matchContextEvent.SetResult(WinningPlayer, otherPlayer, Reason);
            matchContextEvent.MatchEvents.Add(this);
        }
    }
}
