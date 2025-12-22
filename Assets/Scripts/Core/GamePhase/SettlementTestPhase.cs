using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class SettlementTestPhase : IGamePhase
    {
        public GamePhase Phase => GamePhase.DeckConstruction;
        
        public Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            return Task.CompletedTask;
        }
    }
}
