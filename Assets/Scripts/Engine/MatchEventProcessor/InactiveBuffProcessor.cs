using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class InactiveBuffProcessor : MatchEventProcessor<InactiveBuffEvent>
    {
        private const float PROCESS_DELAY = 0.5f;
        
        public override async Task ProcessEventAsync(InactiveBuffEvent matchEvent, CancellationToken token)
        {
            // TODO : show some effect to show active buff at each locations

            var cardData = Storage.Instance.GetCardData(matchEvent.InactiveCardId);
            string cardName = LocalizationHelper.Instance.Localize(cardData.nameKey);
            string cardDesc = LocalizationHelper.Instance.Localize(cardData.descKey);
            string message = $"{matchEvent.InactivatedCardLocation.Owner.Name}의 카드 {cardName}의 효과 제거\n{cardDesc}\n제거 대상 : {string.Join(", ", matchEvent.BuffCancelTargets.Select(target => target.TargetLocation))}";
            MatchLogUI.SendLog(message);
            Debug.Log(message);
            
            foreach (var (cardRef, location) in matchEvent.BuffCancelTargets)
            {
                Simulator.Model.onboardController.ApplyChangeCard(cardRef, location);
            }
            
            ScaledTime scaledDelay = PROCESS_DELAY;
            await scaledDelay.WaitScaledTimeAsync(token);
        }
    }
}
