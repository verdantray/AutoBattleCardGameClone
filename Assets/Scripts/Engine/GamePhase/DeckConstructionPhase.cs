using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(DeckConstructionPhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/DeckConstructionPhase")]
    public sealed class DeckConstructionPhase : GamePhaseAsset
    {
        [SerializeField] private ClubType fixedClubFlag;
        [SerializeField] private ClubType selectableClubFlag;
        
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Default");
            
            GameState currentState = simulationContext.CurrentState;
            List<Task<IPlayerAction>> tasks = simulationContext.Participants
                .Select(player => player.DeckConstructAsync(fixedClubFlag, selectableClubFlag))
                .ToList();

            await Task.WhenAll(tasks);

            foreach (var playerAction in tasks.Select(task => task.Result))
            {
                playerAction.ApplyState(currentState);
                playerAction.ApplyContextEvent(simulationContext.CollectedEvents);
            }

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed());
        }
    }
}
