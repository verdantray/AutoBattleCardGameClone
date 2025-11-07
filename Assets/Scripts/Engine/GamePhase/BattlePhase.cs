using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class BattlePhase : GamePhaseAsset
    {
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Onboard");

            var matchingPairs = simulationContext.CurrentState.GetMatchingPairs();
            GameState currentState = simulationContext.CurrentState;

            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                
            }
        }
    }
}
