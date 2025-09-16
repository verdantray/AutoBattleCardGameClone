using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실로 보내질 때 : 필드에 있는 카드 중 기본 파워 n인 카드 파워 + m만큼 증가
    /// </summary>
    public sealed class GivePowerUpToSpecificPowerFromInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly int _powerCriteria;
        private readonly int _powerUpBonus;
        
        public GivePowerUpToSpecificPowerFromInfirmary(Card card, JsonObject json) : base(card, json)
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
                    case "power_criteria":
                        _powerCriteria = field.value.intValue;
                        break;
                    case "power_up_bonus":
                        _powerUpBonus = field.value.intValue;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
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

                var inactiveBuffEvent = new InactiveBuffEvent(CallCard, new MatchSnapshot(gameState, ownSide, otherSide));
                inactiveBuffEvent.RegisterEvent(matchContextEvent);
                
                return;
            }
            
            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _powerCriteria, _powerUpBonus);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
                
                var activeBuffEvent = new ActiveCardBuffEvent(CallCard, new MatchSnapshot(gameState, ownSide, otherSide));
                activeBuffEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _powerCriteria, _powerUpBonus);
        }
        
        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;
            
            private readonly int _powerCriteria;
            private readonly int _powerUpBonus;
            
            public ExclusiveCardBuff(Card callCard, int powerCriteria, int powerUpBonus) : base(callCard)
            {
                _powerCriteria = powerCriteria;
                _powerUpBonus = powerUpBonus;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return args.OwnSide.Field.Where(card => card.BasePower == _powerCriteria);
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return target.BasePower == _powerCriteria && args.OwnSide.IsEffectiveStandOnField(target);
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerUpBonus;
            }
        }
    }
}
