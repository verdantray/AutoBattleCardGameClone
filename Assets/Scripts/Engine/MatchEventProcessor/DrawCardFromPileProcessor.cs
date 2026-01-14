using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class DrawCardFromPileProcessor : MatchEventProcessor<DrawCardFromPileEvent>
    {
        private readonly ScaledTime _delay = 0.25f;
        private readonly ScaledTime _duration = 0.25f;
        
        public override Task ProcessEventAsync(DrawCardFromPileEvent matchEvent, CancellationToken token)
        {
            return Simulator.Model.onboardController.MoveCardFromPileAsync(
                matchEvent.DrawnCard,
                matchEvent.MovementInfo,
                _delay,
                _duration,
                token
            );
        }
    }
}
