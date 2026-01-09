using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DeckConstructionTestPhase : IGamePhase
    {
        public GamePhase Phase => GamePhase.DeckConstruction;
        
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            List<Task<IPlayerAction>> tasks = new List<Task<IPlayerAction>>();
            
            foreach (PlayerState playerState in currentState.PlayerStates)
            {
                var playerActionTask = playerState.Player.DeckConstructAsync();
                tasks.Add(playerActionTask);
            }

            await Task.WhenAll(tasks);

            foreach (var action in tasks.Select(task => task.Result))
            {
                action.ApplyState(currentState);
                action.ApplyContextEvent(simulationContext.CollectedEvents);
            }
            
            DeckConstructionOverviewEvent overviewEvent = new DeckConstructionOverviewEvent();
            
            overviewEvent.Publish();
            simulationContext.CollectedEvents.AddEvent(overviewEvent);

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed(typeof(DeckConstructionOverviewEvent)));
        }
    }
}
