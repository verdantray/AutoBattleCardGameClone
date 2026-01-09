using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(DeckConstructionPhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/DeckConstructionPhase")]
    public sealed class DeckConstructionPhase : GamePhaseAsset
    {
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Default");
            
            GameState currentState = simulationContext.CurrentState;
            List<Task<IPlayerAction>> playerActionTasks = simulationContext.Participants
                .Select(player => player.DeckConstructAsync())
                .ToList();

            await Task.WhenAll(playerActionTasks);
            
            PersistentWorldCameraPoints.Instance.SwapPoint("Noticeboard");
            
            foreach (var playerAction in playerActionTasks.Select(task => task.Result))
            {
                playerAction.ApplyState(currentState);
                playerAction.ApplyContextEvent(simulationContext.CollectedEvents);
            }

            DeckConstructionOverviewEvent overviewEvent = new DeckConstructionOverviewEvent();
            
            overviewEvent.Publish();
            simulationContext.CollectedEvents.AddEvent(overviewEvent);

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed(typeof(DeckConstructionOverviewEvent)));
        }
    }
}
