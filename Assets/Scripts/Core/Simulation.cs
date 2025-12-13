using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class Simulation
    {
        private readonly SimulationContext _simulationContext;
        private readonly Queue<IGamePhase> _gamePhases = new Queue<IGamePhase>();

        private CancellationTokenSource _stopSimulationTokenSource;

        public Simulation(IPlayer player, params string[] otherPlayerNames)
        {
            if ((otherPlayerNames.Length + 1) is > GameConst.GameOption.MAX_MATCHING_PLAYERS or 0)
            {
                var exceptionMsg = "Players must have at least one, "
                                   + $"same or less than {GameConst.GameOption.MAX_MATCHING_PLAYERS}";
                
                throw new ArgumentException(exceptionMsg);
            }
            
            List<IPlayer> players = new List<IPlayer> { player };
            
            foreach (string otherPlayerName in otherPlayerNames)
            {
                players.Add(new ScriptedPlayer(otherPlayerName));
            }
            
            if (players.Count % 2 != 0)
            {
                players.Add(new ScriptedPlayer("Unknown Brawler"));
            }

            _simulationContext = new SimulationContext(players);
        }
        
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

        // public void SetModel<T>(T model) where T : class, new()
        // {
        //     _simulationContext.SetModel(model);
        // }

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
            
            _stopSimulationTokenSource = new CancellationTokenSource();
            
            while (_gamePhases.Count > 0 && !_stopSimulationTokenSource.IsCancellationRequested)
            {
                var phase = _gamePhases.Dequeue();
                await phase.ExecutePhaseAsync(_simulationContext, _stopSimulationTokenSource.Token);
            }
        }

        public void Stop()
        {
            _stopSimulationTokenSource.Cancel();
            _stopSimulationTokenSource.Dispose();
            _stopSimulationTokenSource = null;
        }
    }
    
    public interface IGamePhase
    {
        public Task ExecutePhaseAsync(SimulationContext simulationContext);
        
        public async Task ExecutePhaseAsync(SimulationContext simulationContext, CancellationToken token)
        {
            try
            {
                await ExecutePhaseAsync(simulationContext);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                StopPhase();
            }
        }

        public void StopPhase()
        {
            
        }
    }
}