using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class SettlementPhase : IGamePhase
    {
        public Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            return Task.CompletedTask;
        }
    }
}
