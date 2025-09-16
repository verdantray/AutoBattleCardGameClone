using System.Collections.Generic;
using System.Text;


namespace ProjectABC.Core
{
    public enum MatchEndReason
    {
        EndByEmptyDeck,
        EndByFullOfInfirmary
    }

    public abstract class MatchMessageEvent : IMatchEvent
    {
        public readonly string Message;
        public abstract void RegisterEvent(IMatchContextEvent matchContextEvent);
    }
    
    public class MatchContextConsoleEvent : IMatchContextEvent
    {
        public readonly int Round;
        public MatchResult Result { get; private set; }
        public bool MatchFinished { get; private set; } = false;
        public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();

        private MatchContextConsoleEvent(int round)
        {
            Round = round;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            foreach (var matchEvent in MatchEvents)
            {
                if (matchEvent is not MatchMessageEvent messageEvent)
                {
                    continue;
                }

                stringBuilder.AppendLine(messageEvent.Message);
            }

            return stringBuilder.ToString();
        }
        
        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason)
        {
            Result = new MatchResult(Round, winPlayer, losePlayer, reason);
            MatchFinished = true;
        }

        public static MatchContextConsoleEvent RunMatch(GameState currentState, params PlayerState[] playerStates)
        {
            return null;
        }
    }
}