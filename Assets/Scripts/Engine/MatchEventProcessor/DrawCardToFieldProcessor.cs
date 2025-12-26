using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class DrawCardToFieldProcessor : MatchEventProcessor<DrawCardToFieldEvent>
    {
        private readonly ScaledTime _revealDuration = 0.25f;
        private readonly ScaledTime _remainDelay = 0.5f;
        private readonly ScaledTime _alignDuration = 0.25f;
        
        public override Task ProcessEventAsync(DrawCardToFieldEvent matchEvent, CancellationToken token)
        {
            bool isOwnPlayer = ReferenceEquals(matchEvent.Owner, Simulator.Model.player);
            OnboardController.OnboardSide onboardSide = isOwnPlayer
                ? OnboardController.OnboardSide.Own
                : OnboardController.OnboardSide.Other;



            return Simulator.Model.onboardController.DrawCardToFieldAsync(
                onboardSide,
                matchEvent.MovementInfo,
                _revealDuration,
                _remainDelay,
                _alignDuration,
                token
            );
        }
    }
}
