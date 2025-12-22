using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProjectABC.Core
{
    public class DeletionTestPhase : IGamePhase
    {
        public GamePhase Phase => GamePhase.Deletion;
        
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            List<PlayerState> playerStates = simulationContext.CurrentState.PlayerStates;
            List<Task<IPlayerAction>> tasks = new List<Task<IPlayerAction>>();
            
            foreach (var playerState in playerStates)
            {
                var task = playerState.Player.DeleteCardsAsync(playerState);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            foreach (var action in tasks.Select(task => task.Result))
            {
                action.ApplyState(simulationContext.CurrentState);
                action.ApplyContextEvent(simulationContext.CollectedEvents);
            }
        }
    }
}
