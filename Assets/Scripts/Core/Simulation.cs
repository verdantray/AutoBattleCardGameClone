using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectABC.Core
{
    public class Simulation
    {
        private readonly SimulationContext simulationContext;
        private readonly Queue<IGamePhase> gamePhases = new Queue<IGamePhase>();
        
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

            simulationContext = new SimulationContext(playerList);
            InitializeGamePhases();
        }

        private void InitializeGamePhases()
        {
            gamePhases.Clear();
            gamePhases.Enqueue(new DeckConstructionPhase());

            for (int round = 1; round <= GameConst.GameOption.MAX_ROUND; round++)
            {
                gamePhases.Enqueue(new PreparationPhase(round));
                gamePhases.Enqueue(new RecruitPhase());
                gamePhases.Enqueue(new BattlePhase());
            }
            
            gamePhases.Enqueue(new SettlementPhase());
        }

        public async Task RunAsync()
        {
            try
            {
                while (gamePhases.Count > 0)
                {
                    var phase = gamePhases.Dequeue();
                    await phase.ExecutePhaseAsync(simulationContext);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Simulation has an error occured : {e}");
            }
            finally
            {
                foreach (var contextEvent in simulationContext.CollectedEvents)
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