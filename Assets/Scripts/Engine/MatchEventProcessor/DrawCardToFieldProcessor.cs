using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class DrawCardToFieldProcessor : MatchEventProcessor<DrawCardToFieldEvent>
    {
        private readonly ScaledTime _delay = 0.75f;
        private readonly ScaledTime _duration = 0.25f;
        
        public override Task ProcessEventAsync(DrawCardToFieldEvent matchEvent, CancellationToken token)
        {
            return Simulator.Model.onboardController.MoveCardAsync(
                matchEvent.Owner,
                matchEvent.MovementInfo,
                _delay,
                _duration,
                token
            );
        }
    }
}
