using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public class MoveCardByEffectProcessor : MatchEventProcessor<MoveCardByEffectEvent>
    {
        private readonly ScaledTime _delay = 0.25f;
        private readonly ScaledTime _duration = 0.25f;
        
        public override Task ProcessEventAsync(MoveCardByEffectEvent matchEvent, CancellationToken token)
        {
            return Simulator.Model.onboardController.MoveCardAsync(matchEvent.MovementInfo, _delay, _duration, token);
        }
    }
}
