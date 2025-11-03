using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DeckConstructionTestPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            List<Task<IPlayerAction>> tasks = new List<Task<IPlayerAction>>();
            
            foreach (PlayerState playerState in currentState.PlayerStates)
            {
                ClubType fixedClubFlag = ClubType.Council;
                ClubType selectableClubFlag = ClubType.Coastline
                                           | ClubType.Band
                                           | ClubType.GameDevelopment
                                           | ClubType.HauteCuisine
                                           | ClubType.Unregistered
                                           | ClubType.TraditionExperience;
                
                var playerActionTask = playerState.Player.DeckConstructAsync(fixedClubFlag, selectableClubFlag);
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
