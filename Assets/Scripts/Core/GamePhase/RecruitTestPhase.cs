using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProjectABC.Core
{
	public class RecruitTestPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            RecruitOnRound recruitOnRound = new RecruitOnRound(currentState.Round);
                
            List<PlayerState> allPlayerStates = currentState.PlayerStates;
            List<Task<IPlayerAction>> tasks = new List<Task<IPlayerAction>>();

            foreach (PlayerState playerState in allPlayerStates)
            {
                IPlayer player = playerState.Player;
                var playerActionTask = player.RecruitCardsAsync(playerState, recruitOnRound);
                    
                tasks.Add(playerActionTask);
            }
                
            await Task.WhenAll(tasks);

            foreach (var action in tasks.Select(task => task.Result))
            {
                action.ApplyState(currentState);
                action.ApplyContextEvent(simulationContext.CollectedEvents);
            }
        }
    }
}