using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProjectABC.Core
{
    public class DismissTestPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            DismissOnRound dismissOnRound = new DismissOnRound(currentState.Round);
            
            List<PlayerState> playerStates = simulationContext.CurrentState.PlayerStates;
            List<Task<IPlayerAction>> tasks = new List<Task<IPlayerAction>>();
            
            foreach (var playerState in playerStates)
            {
                var task = playerState.Player.DismissCardsAsync(playerState, dismissOnRound);
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
