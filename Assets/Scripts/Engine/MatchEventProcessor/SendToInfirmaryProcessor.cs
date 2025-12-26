using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class SendToInfirmaryProcessor : MatchEventProcessor<SendToInfirmaryEvent>
    {
        private readonly ScaledTime _moveDuration = 0.5f;
        
        public override Task ProcessEventAsync(SendToInfirmaryEvent matchEvent, CancellationToken token)
        {
            return Simulator.Model.onboardController.SendCardToInfirmaryAsync(
                matchEvent.Owner,
                matchEvent.MovementInfo,
                _moveDuration,
                token
            );
        }
    }
}
