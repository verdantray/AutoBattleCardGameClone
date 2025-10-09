using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class SettlementTestPhase : IGamePhase
    {
        public Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            return Task.CompletedTask;
        }
    }
}
