using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
            InitializeGamePhases();
        }

        private void InitializeGamePhases()
        {
            _gamePhases.Clear();
            _gamePhases.Enqueue(new DeckConstructionPhase());

            for (int round = 1; round <= GameConst.GameOption.MAX_ROUND; round++)
            {
                _gamePhases.Enqueue(new PreparationPhase(round));
                _gamePhases.Enqueue(new RecruitPhase());
                _gamePhases.Enqueue(new BattlePhase());
            }
            
            _gamePhases.Enqueue(new SettlementPhase());
        }

        public async Task RunAsync()
        {
            try
            {
                while (_gamePhases.Count > 0)
                {
                    var phase = _gamePhases.Dequeue();
                    await phase.ExecutePhaseAsync(_simulationContext);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Simulation has an error occured : {e}");
            }
            finally
            {
                foreach (var contextEvent in _simulationContext.CollectedEvents)
                {
                    contextEvent.Trigger();
                }
            }
        }
    }
    
    public interface IGamePhase
    {
        public Task ExecutePhaseAsync(SimulationContext simulationContext);
    }
}