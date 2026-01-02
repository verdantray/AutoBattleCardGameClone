using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class ActiveBuffProcessor : MatchEventProcessor<ActiveBuffEvent>
    {
        private const float PROCESS_DELAY = 0.1f;
        
        public override async Task ProcessEventAsync(ActiveBuffEvent matchEvent, CancellationToken token)
        {
            // TODO : show some effect to show active buff at each locations
            
            string message = $"{matchEvent.ActivatedCardLocation.Owner.Name} : {matchEvent.ActiveCardId}의 효과 다음 대상에게 적용\n{string.Join(", ", matchEvent.BuffTargets.Select(target => target.TargetLocation))}";
            MatchLogUI.SendLog(message);
            Debug.Log(message);
            
            ScaledTime scaledDelay = PROCESS_DELAY;
            await scaledDelay.WaitScaledTimeAsync(token);
        }
    }
}
