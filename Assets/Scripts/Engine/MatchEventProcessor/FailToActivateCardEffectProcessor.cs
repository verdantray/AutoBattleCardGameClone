using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class FailToActivateCardEffectProcessor : MatchEventProcessor<FailToActivateCardEffectEvent>
    {
        private const float PROCESS_DELAY = 0.1f;

        public override async Task ProcessEventAsync(FailToActivateCardEffectEvent matchEvent, CancellationToken token)
        {
            // TODO : show some effect to show fail to active effect at card location, contain player name and description to fail message
            
            string message = $"{matchEvent.FailedCardLocation.Owner.Name} : {matchEvent.Reason} / {matchEvent.FailedCardId}";
            MatchLogUI.SendLog(message);
            Debug.Log(message);

            ScaledTime scaledDelay = PROCESS_DELAY;
            await scaledDelay.WaitScaledTimeAsync(token);
        }
    }
}
