using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class SimulationContext
    {
        public readonly List<IContextEvent> CollectedEvents = new List<IContextEvent>();
        public readonly List<IPlayer> Participants = new List<IPlayer>();
        
        public readonly GameState CurrentState;

        public SimulationContext(List<IPlayer> players)
        {
            Participants.AddRange(players);
            CurrentState = new GameState(players);
        }
        
        public IEnumerable<Task> GetTasksOfAllPlayersConfirmToProceed()
        {
            return Participants.Select(player => player.WaitUntilConfirmToProceed());
        }
    }

    public class GameState
    {
        public readonly List<PlayerState> PlayerStates = new List<PlayerState>();
        
        public readonly RoundPairMap RoundPairMap;
        public readonly ScoreBoard ScoreBoard;
        public readonly MatchResults MatchResults;

        public int Round { get; private set; } = 0;

        public GameState(List<IPlayer> players)
        {
            foreach (var player in players)
            {
                PlayerStates.Add(new PlayerState(player));
            }
            
            RoundPairMap = new RoundPairMap(players.Count);
            ScoreBoard = new ScoreBoard(players);
            MatchResults = new MatchResults();
        }

        public PlayerState GetPlayerState(IPlayer player)
        {
            return PlayerStates.Find(state => state.Player == player);
        }

        public void SetRound(int round)
        {
            Round = round;
        }
        
        public List<(PlayerState a, PlayerState b)> GetMatchingPairs()
        {
            RoundPairs roundPairs = RoundPairMap.GetRoundPairs(Round);
            
            return roundPairs.GetMatchingPlayerPairs(PlayerStates);
        }
    }
    
    public class PlayerState
    {
        public readonly IPlayer Player;

        public int MulliganChances { get; private set; } = GameConst.GameOption.MULLIGAN_DEFAULT_AMOUNT;

        public readonly GradeCardPiles GradeCardPiles = new GradeCardPiles();
        public readonly List<Card> Deck = new List<Card>();
        public readonly List<Card> Deleted = new List<Card>();

        public PlayerState(IPlayer player)
        {
            Player = player;
        }
    }
}
