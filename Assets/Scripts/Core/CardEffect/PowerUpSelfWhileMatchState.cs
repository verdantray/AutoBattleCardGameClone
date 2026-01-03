using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 수비할 때 내 파워 + n 만큼 증가
    /// </summary>
    public sealed class PowerUpSelfWhileMatchState : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly MatchPosition _enablePositionFlag;
        private readonly int _powerUpBonus;
        
        public PowerUpSelfWhileMatchState(Card card, JsonObject json) : base(card, json)
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
                
                return;
            }
            
            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _enablePositionFlag, _powerUpBonus);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
            }
        }

        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;

            private readonly MatchPosition _enablePositionFlag;
            private readonly int _powerUpBonus;
            
            public ExclusiveCardBuff(Card callCard, MatchPosition enablePositionFlag, int powerUpBonus) : base(callCard)
            {
                _enablePositionFlag = enablePositionFlag;
                _powerUpBonus = powerUpBonus;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return args.OwnSide.Field.Contains(CallCard)
                    ? new[] { CallCard }
                    : Array.Empty<Card>();
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return args.OwnSide.IsEffectiveStandOnField(target)
                       && _enablePositionFlag.HasFlag(args.OwnSide.Position);
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerUpBonus;
            }
        }
            
    }
}
