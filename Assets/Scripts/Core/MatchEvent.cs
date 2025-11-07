

namespace ProjectABC.Core
{
    public interface IMatchEvent
    {
        public void RegisterEvent(IMatchContextEvent matchContextEvent);
    }

    public abstract class MatchEvent : IMatchEvent
    {
        public virtual void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            if (matchContextEvent.MatchFinished)
            {
                return;
            }
            
            matchContextEvent.MatchEvents.Add(this);
        }
    }

    public class MatchStartEvent : MatchEvent
    {
        public readonly IPlayer Attacker;
        public readonly IPlayer Defender;
        public readonly PositionDecideReason Reason;

        public MatchStartEvent(IPlayer attacker, IPlayer defender, PositionDecideReason reason)
        {
            Attacker = attacker;
            Defender = defender;
            Reason = reason;
        }
    }

    public class MatchFinishEvent : MatchEvent
    {
        public readonly IPlayer Winner;
        public readonly IPlayer Loser;
        public readonly MatchEndReason Reason;

        public MatchFinishEvent(IPlayer winner, IPlayer loser, MatchEndReason reason)
        {
            Winner = winner;
            Loser = loser;
            Reason = reason;
        }

        public override void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            if (matchContextEvent.MatchFinished)
            {
                return;
            }
            
            matchContextEvent.SetResult(Winner, Loser, Reason);
            matchContextEvent.MatchEvents.Add(this);
        }
    }

    public abstract class OnboardMatchEvent : MatchEvent
    {
        public readonly MatchSnapshot MatchSnapshot;
        
        protected OnboardMatchEvent(MatchSnapshot snapshot)
        {
            MatchSnapshot = snapshot;
        }
    }

    public class CardDrawEvent : OnboardMatchEvent
    {
        public readonly IPlayer DrawPlayer;
        public readonly CardSnapshot DrawnCard;
        
        public CardDrawEvent(IPlayer player, CardSnapshot drawnCard, MatchSnapshot snapshot) : base(snapshot)
        {
            DrawPlayer = player;
            DrawnCard = drawnCard;
        }
    }

    public class SuccessAttackEvent : MatchEvent
    {
        
    }

    public class PutInfirmaryEvent : OnboardMatchEvent
    {
        public readonly IPlayer Player;
        public readonly CardSnapshot PutCard;
        
        public PutInfirmaryEvent(IPlayer player, CardSnapshot putCard, MatchSnapshot snapshot) : base(snapshot)
        {
            Player = player;
            PutCard = putCard;
        }
    }
}
