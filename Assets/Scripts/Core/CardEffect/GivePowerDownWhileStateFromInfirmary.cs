using System;
using System.Collections.Generic;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 공격 / 수비 시 자신의 필드의 모든 카드들의 파워 n만큼 감소
    /// </summary>
    public sealed class GivePowerDownWhileStateFromInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly MatchState _enableStateFlag;
        private readonly int _powerDownPenalty;
        
        public GivePowerDownWhileStateFromInfirmary(Card card, JsonObject json) : base(card, json)
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
                    case "enable_match_states":
                        
                        MatchState stateFlag = 0;

                        foreach (var element in field.value.arr)
                        {
                            stateFlag |= Enum.Parse<MatchState>(element.strValue, true);
                        }

                        _enableStateFlag = stateFlag;
                        break;
                    case "power_down_penalty":
                        _powerDownPenalty = field.value.intValue;
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

                var inactiveBuffEvent = new CommonMatchMessageEvent($"{CallCard.Title} {CallCard.Name}의 효과가 취소됩니다. / {CallCard.CardEffect.Description}");
                inactiveBuffEvent.RegisterEvent(matchContextEvent);

                return;
            }

            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _enableStateFlag, _powerDownPenalty);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
                
                var activeBuffEvent = new CommonMatchMessageEvent($"{CallCard.Title} {CallCard.Name}의 효과가 발동됩니다. / {CallCard.CardEffect.Description}");
                activeBuffEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _powerDownPenalty);
        }

        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Negative;

            private readonly MatchState _enableStateFlag;
            private readonly int _powerDownPenalty;
            
            public ExclusiveCardBuff(Card callCard, MatchState enableStateFlag, int powerDownPenalty) : base(callCard)
            {
                _enableStateFlag = enableStateFlag;
                _powerDownPenalty = powerDownPenalty;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return args.OwnSide.Field;
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return args.OwnSide.IsEffectiveStandOnField(target)
                       && _enableStateFlag.HasFlag(args.OwnSide.State);
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerDownPenalty;
            }
        }
    }
}