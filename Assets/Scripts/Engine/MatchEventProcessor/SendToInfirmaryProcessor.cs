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
            bool isOwnPlayer = ReferenceEquals(matchEvent.Owner, Simulator.Model.player);
            OnboardController.OnboardSide onboardSide = isOwnPlayer
                ? OnboardController.OnboardSide.Own
                : OnboardController.OnboardSide.Other;
            
            return Simulator.Model.onboardController.SendCardToInfirmaryAsync(
                onboardSide,
                matchEvent.MovementInfo,
                _moveDuration,
                token
            );
        }
    }
}
