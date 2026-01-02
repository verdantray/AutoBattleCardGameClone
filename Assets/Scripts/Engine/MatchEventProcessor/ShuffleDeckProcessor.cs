using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public class ShuffleDeckProcessor : MatchEventProcessor<ShuffleDeckEvent>
    {
        private readonly ScaledTime _remainDelay = 0.5f;
        private readonly ScaledTime _inoutDuration = 0.15f;
        
        public override Task ProcessEventAsync(ShuffleDeckEvent matchEvent, CancellationToken token)
        {
            return Simulator.Model.onboardController.ShuffleDeckAsync(matchEvent.MovementInfo, _remainDelay, _inoutDuration, token);
        }
    }
}
