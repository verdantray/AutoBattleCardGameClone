using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectABC.Core
{
    public class PreparationPhase : IGamePhase
    {
        private readonly int _round;
        
        public PreparationPhase(int round)
        {
            _round = round;
        }
        
        public Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            try
            {
                simulationContext.CurrentState.SetRound(_round);
                simulationContext.CollectedEvents.Add(new CommonConsoleEvent($"{_round} 라운드 준비"));
                
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Debug.LogError($"PreparationPhase Exception : {e}");
                throw;
            }
        }
    }
}