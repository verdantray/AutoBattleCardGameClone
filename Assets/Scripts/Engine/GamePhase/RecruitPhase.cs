using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(RecruitPhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/RecruitPhase")]
    public sealed class RecruitPhase : GamePhaseAsset
    {
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            RecruitOnRound recruitOnRound = new RecruitOnRound(currentState.Round);

            List<Task<IPlayerAction>> playerActionTasks = new List<Task<IPlayerAction>>();
            foreach (var player in simulationContext.Participants)
            {
                PlayerState playerState = currentState.GetPlayerState(player);
                var task = player.RecruitCardsAsync(playerState, recruitOnRound);
                
                playerActionTasks.Add(task);
            }
            
            await Task.WhenAll(playerActionTasks);

            List<Task> waitConfirmTasks = new List<Task>();
            foreach (var action in playerActionTasks.Select(task => task.Result))
            {
                action.ApplyState(currentState);
                action.ApplyContextEvent(simulationContext.CollectedEvents);
                
                waitConfirmTasks.Add(action.GetWaitConfirmTask());
            }
            
            await Task.WhenAll(waitConfirmTasks);
        }
    }
}