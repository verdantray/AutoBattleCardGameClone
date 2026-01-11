using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class PreparationTestPhase : IGamePhase
    {
        private readonly int _round;
        
        public PreparationTestPhase(int round)
        {
            _round = round;
        }
        
        public Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            simulationContext.CurrentState.SetRound(_round);

            var contextEvent = new PrepareRoundEvent(_round);
            contextEvent.Publish();
            
            simulationContext.CollectedEvents.AddEvent(contextEvent);
                
            return Task.CompletedTask;
        }
    }
}