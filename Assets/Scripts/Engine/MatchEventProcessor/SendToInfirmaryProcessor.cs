using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class SendToInfirmaryProcessor : MatchEventProcessor<SendToInfirmaryEvent>
    {
        private readonly ScaledTime _delay = ScaledTime.Zero;
        private readonly ScaledTime _duration = 0.5f;
        
        public override Task ProcessEventAsync(SendToInfirmaryEvent matchEvent, CancellationToken token)
        {
            return Simulator.Model.onboardController.MoveCardAsync(matchEvent.MovementInfo, _delay, _duration, token);
        }
    }
}
