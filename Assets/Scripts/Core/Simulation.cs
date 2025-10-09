using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class Simulation
    {
        private readonly SimulationContext _simulationContext;
        private readonly Queue<IGamePhase> _gamePhases = new Queue<IGamePhase>();
        
        public Simulation(params IPlayer[] players)
        {
            if (players.Length is > GameConst.GameOption.MAX_MATCHING_PLAYERS or 0)
            {
                var exceptionMsg = "Players must have at least one, "
                                   + $"same or less than {GameConst.GameOption.MAX_MATCHING_PLAYERS}";
                
                throw new ArgumentException(exceptionMsg);
            }

            List<IPlayer> playerList = new List<IPlayer>(players);
            
            if (playerList.Count % 2 != 0)
            {
                playerList.Add(new ScriptedPlayer("Scripted Player"));
            }

            _simulationContext = new SimulationContext(playerList);
        }

        public void EnqueueGamePhase(IGamePhase gamePhase)
        {
            _gamePhases.Enqueue(gamePhase);
        }

        public void ClearGamePhases()
        {
            _gamePhases.Clear();
        }

        public void InitializeTestPhases()
        {
            ClearGamePhases();
            EnqueueGamePhase(new DeckConstructionTestPhase());
            
            for (int round = 1; round <= GameConst.GameOption.MAX_ROUND; round++)
            {
                EnqueueGamePhase(new PreparationTestPhase(round));
            
                // TODO: use schedule data
                if (round is 3 or 7)
                {
                    EnqueueGamePhase(new DeletionTestPhase());
                }
                
                EnqueueGamePhase(new RecruitTestPhase());
                EnqueueGamePhase(new BattleTestPhase());
            }
            
            EnqueueGamePhase(new SettlementTestPhase());
        }

        public async Task RunAsync()
        {
            if (_gamePhases.Count == 0)
            {
                throw new ArgumentException("No game phases found");
            }
            
            while (_gamePhases.Count > 0)
            {
                var phase = _gamePhases.Dequeue();
                await phase.ExecutePhaseAsync(_simulationContext);
            }
        }
    }
    
    public interface IGamePhase
    {
        public Task ExecutePhaseAsync(SimulationContext simulationContext);
    }
}