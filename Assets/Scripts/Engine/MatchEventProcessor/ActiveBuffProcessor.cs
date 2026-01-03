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
        private const float PROCESS_DELAY = 0.2f;
        
        public override async Task ProcessEventAsync(ActiveBuffEvent matchEvent, CancellationToken token)
        {
            // TODO : show some effect to show active buff at each locations

            var cardData = Storage.Instance.GetCardData(matchEvent.ActiveCardId);
            string cardName = LocalizationHelper.Instance.Localize(cardData.nameKey);
            string cardDesc = LocalizationHelper.Instance.Localize(cardData.descKey);
            string message = $"{matchEvent.ActivatedCardLocation.Owner.Name}의 카드 {cardName} 효과 적용\n{cardDesc}\n적용 대상 : {string.Join(", ", matchEvent.BuffTargets.Select(target => target.TargetLocation))}";
            MatchLogUI.SendLog(message);
            Debug.Log(message);
            
            ScaledTime scaledDelay = PROCESS_DELAY;
            await scaledDelay.WaitScaledTimeAsync(token);
        }
    }
}
