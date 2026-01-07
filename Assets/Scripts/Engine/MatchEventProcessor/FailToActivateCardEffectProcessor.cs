using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class FailToActivateCardEffectProcessor : MatchEventProcessor<FailToActivateCardEffectEvent>
    {
        private const float PROCESS_DELAY = 2.0f;

        public override async Task ProcessEventAsync(FailToActivateCardEffectEvent matchEvent, CancellationToken token)
        {
            // TODO : show some effect to show fail to active effect at card location, contain player name and description to fail message

            var cardData = Storage.Instance.GetCardData(matchEvent.FailedCardId);
            string cardName = LocalizationHelper.Instance.Localize(cardData.nameKey);
            string cardDesc = LocalizationHelper.Instance.Localize(cardData.descKey);
            string reason = FailReasonLocalization(matchEvent.Reason);
            
            string message = $"{matchEvent.FailedCardLocation.Owner.Name}의 카드 {cardName} 효과 발동 실패\n효과 : {cardDesc}\n이유 : {reason}";
            MatchLogUI.SendLog(message);
            Debug.Log(message);

            ScaledTime scaledDelay = PROCESS_DELAY;
            await scaledDelay.WaitScaledTimeAsync(token);
        }

        private static string FailReasonLocalization(FailToActivateEffectReason reason)
        {
            string localizationKey = reason switch
            {
                FailToActivateEffectReason.NoMeetCondition => GameConst.CardEffect.FAIL_REASON_NO_MEET_CONDITION,
                FailToActivateEffectReason.NoDeckRemains => GameConst.CardEffect.FAIL_REASON_NO_DECK_REMAINS,
                FailToActivateEffectReason.NoCardPileRemains => GameConst.CardEffect.FAIL_REASON_NO_CARD_PILE_REMAINS,
                FailToActivateEffectReason.NoOpponentCardPileRemains => GameConst.CardEffect.FAIL_REASON_NO_OPPONENT_CARD_PILE_REMAINS,
                FailToActivateEffectReason.NoInfirmaryRemains => GameConst.CardEffect.FAIL_REASON_NO_INFIRMARY_REMAINS,
                FailToActivateEffectReason.NoOpponentInfirmaryRemains => GameConst.CardEffect.FAIL_REASON_NO_OPPONENT_INFIRMARY_REMAINS,
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };
            
            return LocalizationHelper.Instance.Localize(localizationKey);
        }
    }
}
