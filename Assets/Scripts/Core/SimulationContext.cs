using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public sealed class SimulationContext
    {
        public readonly SimulationContextEvents CollectedEvents = new SimulationContextEvents();
        public readonly List<IPlayer> Participants = new List<IPlayer>();
        
        public readonly GameState CurrentState;

        public SimulationContext(List<IPlayer> players)
        {
            Participants.AddRange(players);
            CurrentState = new GameState(players);
        }
        
        public IEnumerable<Task> GetTasksOfAllPlayersConfirmToProceed(Type eventType)
        {
            return Participants.Select(player => player.WaitUntilConfirmToProceed(eventType));
        }
    }

    public class SimulationContextEvents : IReadOnlyList<IContextEvent>
    {
        private readonly List<IContextEvent> _collectedEvents = new List<IContextEvent>();

        public void AddEvent(IContextEvent contextEvent)
        {
            _collectedEvents.Add(contextEvent);
        }

        #region inherits of IReadOnlyList<IContextEvent>

        public int Count => _collectedEvents.Count;
        public IContextEvent this[int index] => _collectedEvents[index];
        public IEnumerator<IContextEvent> GetEnumerator() => _collectedEvents.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collectedEvents.GetEnumerator();

        #endregion
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

        public readonly RerollChance RerollChance =  new RerollChance();
        public readonly GradeCardPiles GradeCardPiles = new GradeCardPiles();
        public readonly List<Card> Deck = new List<Card>();
        public readonly List<Card> Deleted = new List<Card>();

        public PlayerState(IPlayer player)
        {
            Player = player;
        }
    }
}
