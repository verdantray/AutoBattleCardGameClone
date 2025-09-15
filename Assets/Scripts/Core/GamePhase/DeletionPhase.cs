using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProjectABC.Core
{
    public class DeletionPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            List<PlayerState> playerStates = simulationContext.CurrentState.PlayerStates;
            List<Task<DeleteCardsAction>> tasks = new List<Task<DeleteCardsAction>>();
            
            foreach (var playerState in playerStates)
            {
                var task = playerState.Player.DeleteCardsAsync(playerState);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            foreach (var action in tasks.Select(task => task.Result))
            {
                action.ApplyState(simulationContext.CurrentState);
                
                var contextEvent = action.GetContextEvent();
                contextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(contextEvent);
            }
        }
    }
}
