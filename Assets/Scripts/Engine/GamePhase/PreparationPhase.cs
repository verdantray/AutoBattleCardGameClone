using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(PreparationPhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/PreparationPhase")]
    public sealed class PreparationPhase : GamePhaseAsset
    {
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Noticeboard");

            int prevRound = simulationContext.CurrentState.Round;
            int currentRound = prevRound += 1;
            
            simulationContext.CurrentState.SetRound(currentRound);
            
            IContextEvent contextEvent = new PrepareRoundEvent(currentRound);
            
            contextEvent.Publish();
            simulationContext.CollectedEvents.AddEvent(contextEvent);

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed(Phase));
        }
    }
}