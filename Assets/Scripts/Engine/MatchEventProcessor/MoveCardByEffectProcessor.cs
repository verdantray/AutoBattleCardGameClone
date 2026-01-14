using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class MoveCardByEffectProcessor : MatchEventProcessor<MoveCardByEffectEvent>
    {
        private readonly ScaledTime _delay = 0.25f;
        private readonly ScaledTime _duration = 0.25f;
        
        public override Task ProcessEventAsync(MoveCardByEffectEvent matchEvent, CancellationToken token)
        {
            Debug.Log($"카드 효과로 인해 {matchEvent.MovementInfo.PreviousLocation} -> {matchEvent.MovementInfo.CurrentLocation}");
            
            return Simulator.Model.onboardController.MoveCardAsync(matchEvent.MovementInfo, _delay, _duration, token);
        }
    }
}
