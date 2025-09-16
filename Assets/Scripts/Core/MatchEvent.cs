using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public interface IMatchEvent
    {
        public void RegisterEvent(IMatchContextEvent matchContextEvent);
    }
    
    public interface IMatchContextEvent : IContextEvent
    {
        public MatchResult Result { get; }
        public bool MatchFinished { get; }
        public List<IMatchEvent> MatchEvents { get; }

        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason);
    }
}
