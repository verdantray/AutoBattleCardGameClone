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
            PersistentWorldCameraPoints.Instance.SwapPoint("Default");
            
            GameState currentState = simulationContext.CurrentState;
            RecruitOnRound recruitOnRound = new RecruitOnRound(currentState.Round);

            List <Task<IPlayerAction>> tasks = new List<Task<IPlayerAction>>();

            foreach (var player in simulationContext.Participants)
            {
                PlayerState playerState = currentState.GetPlayerState(player);
                var task = player.RecruitCardsAsync(playerState, recruitOnRound);
                
                tasks.Add(task);
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