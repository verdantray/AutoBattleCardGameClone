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
        private readonly MatchPosition _enablePositionFlag;
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
                    case "enable_match_positions":
                        
                        MatchPosition positionFlag = 0;

                        foreach (var element in field.value.arr)
                        {
                            positionFlag |= Enum.Parse<MatchPosition>(element.strValue, true);
                        }

                        _enablePositionFlag = positionFlag;
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
                
                return;
            }

            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _enablePositionFlag, _powerDownPenalty);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
            }
        }

        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Negative;

            private readonly MatchPosition _enablePositionFlag;
            private readonly int _powerDownPenalty;
            
            public ExclusiveCardBuff(Card callCard, MatchPosition enablePositionFlag, int powerDownPenalty) : base(callCard)
            {
                _enablePositionFlag = enablePositionFlag;
                _powerDownPenalty = powerDownPenalty;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                bool isCallCardRemainField = args.OwnSide.Field.Contains(CallCard);
                
                return isCallCardRemainField
                    ? args.OwnSide.Field
                    :  Array.Empty<Card>();
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return args.OwnSide.IsEffectiveStandOnField(target)
                       && _enablePositionFlag.HasFlag(args.OwnSide.Position);
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerDownPenalty;
            }
        }
    }
}