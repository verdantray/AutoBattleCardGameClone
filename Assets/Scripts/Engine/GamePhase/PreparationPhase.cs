using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(PreparationPhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/PreparationPhase")]
    public sealed class PreparationPhase : GamePhaseAsset
    {
        [SerializeField] private int round;
        
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Default");
            
            simulationContext.CurrentState.SetRound(round);
            
            IContextEvent contextEvent = new PrepareRoundEvent(round);
            
            contextEvent.Publish();
            simulationContext.CollectedEvents.AddEvent(contextEvent);

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed());
        }
    }
}