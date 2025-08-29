using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class SimulationContext
    {
        public readonly List<IContextEvent> CollectedEvents = new List<IContextEvent>();
        public readonly GameState CurrentState;

        public SimulationContext(IEnumerable<IPlayer> players)
        {
            CurrentState = new GameState(players);
        }
        
        public IEnumerable<Task> GetTasksOfAllPlayersConfirmToProceed()
        {
            return CurrentState.PlayerStates.Select(state => state.Player.WaitUntilConfirmToProceed());
        }
    }

    public class GameState
    {
        public readonly List<PlayerState> PlayerStates = new List<PlayerState>();
        public readonly RoundPairMap RoundPairMap;
        public readonly ScoreBoard ScoreBoard;

        public int Round { get; private set; } = 0;

        public GameState(IEnumerable<IPlayer> players)
        {
            foreach (var player in players)
            {
                PlayerStates.Add(new PlayerState(player));
            }

            RoundPairMap = new RoundPairMap(GameConst.GameOption.MAX_ROUND, PlayerStates.Count);
            ScoreBoard = new ScoreBoard();
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
        public int WinPoints = 0;

        public readonly GradeCardPiles GradeCardPiles = new GradeCardPiles();
        public readonly List<Card> Deck = new List<Card>();
        public readonly List<Card> Deleted = new List<Card>();

        public PlayerState(IPlayer player)
        {
            Player = player;
        }
    }
}
