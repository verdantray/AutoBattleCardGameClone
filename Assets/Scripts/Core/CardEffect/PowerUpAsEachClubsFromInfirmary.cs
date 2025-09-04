using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에 특정 동아리를 제외한 소속 수 N만큼 자신의 파워 증가
    /// </summary>
    public class PowerUpAsEachClubsFromInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly ClubType _excludedClubFlag;
        private readonly int _powerUpRatio;
        
        public PowerUpAsEachClubsFromInfirmary(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_CANCEL_TRIGGERS_KEY:
                        
                        EffectTriggerEvent flag = 0;
                        
                        foreach (var element in field.value.arr)
                        {
                            flag |= Enum.Parse<EffectTriggerEvent>(element.strValue, true);
                        }

                        _cancelTriggerFlag = flag;
                        break;
                    case "club_excludes":
                        
                        ClubType excludeFlag = 0;

                        foreach (var element in field.value.arr)
                        {
                            excludeFlag |= Enum.Parse<ClubType>(element.strValue, true);
                        }

                        _excludedClubFlag = excludeFlag;
                        break;
                    case "power_up_ratio":
                        _powerUpRatio = field.value.intValue;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;
            
            bool isApplyTrigger = ApplyTriggerFlag.HasFlag(trigger);
            bool isCancelTrigger = _cancelTriggerFlag.HasFlag(trigger);

            bool isBuffActive = ownSide.CardBuffHandlers.Exists(entry => entry.CallCard == CallCard);

            // case : already buff active, but effect canceled
            if (isBuffActive && isCancelTrigger)
            {
                var handlers = ownSide.CardBuffHandlers.FindAll(entry => entry.CallCard == CallCard);
                
                foreach (var handler in handlers)
                {
                    handler.Release();
                    ownSide.CardBuffHandlers.Remove(handler);
                }

                var inactiveBuffEvent = new InactiveBuffEvent(CallCard, new MatchSnapshot(ownSide, otherSide));
                inactiveBuffEvent.RegisterEvent(matchContextEvent);

                return;
            }

            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _excludedClubFlag, _powerUpRatio);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
                
                var activeBuffEvent = new ActiveCardBuffEvent(CallCard, new MatchSnapshot(ownSide, otherSide));
                activeBuffEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
        
        private class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Aura;

            private readonly ClubType _excludedClubFlag;
            private readonly int _powerUpRatio;
        
            public ExclusiveCardBuff(Card callCard, ClubType excludedClubFlag, int powerUpRatio) : base(callCard)
            {
                _excludedClubFlag = excludedClubFlag;
                _powerUpRatio = powerUpRatio;
            }

            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                var (ownSide, otherSide, gameState) = args;
            
                return ownSide.Field.Contains(CallCard)
                    ? new[] { CallCard }
                    : Array.Empty<Card>();
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                var (ownSide, otherSide, gameState) = args;
                
                int clubCountInInfirmary = ownSide.Infirmary.GetAllCards()
                    .Select(card => card.ClubType)
                    .Distinct()
                    .Count(club => !_excludedClubFlag.HasFlag(club));

                return ownSide.Field.Contains(target) && clubCountInInfirmary > 0;
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                var  (ownSide, otherSide, gameState) = args;
                
                int clubCountInInfirmary = ownSide.Infirmary.GetAllCards()
                    .Select(card => card.ClubType)
                    .Distinct()
                    .Count(club => !_excludedClubFlag.HasFlag(club));
            
                return clubCountInInfirmary * _powerUpRatio;
            }
        }
    }
}
