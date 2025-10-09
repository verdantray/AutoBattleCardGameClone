using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class DeckConstructionTestPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            List<Task<DeckConstructAction>> tasks = new List<Task<DeckConstructAction>>();
            
            foreach (PlayerState playerState in currentState.PlayerStates)
            {
                var playerActionTask = playerState.Player.DeckConstructAsync();
                tasks.Add(playerActionTask);
            }

            await Task.WhenAll(tasks);

            foreach (var action in tasks.Select(task => task.Result))
            {
                action.ApplyState(currentState);

                DeckConstructionConsoleEvent contextEvent = action.GetContextEvent();
                contextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(contextEvent);
            }
        }
    }
}
