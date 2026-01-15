using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class GiveDisablerBothSideForInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        
        public GiveDisablerBothSideForInfirmary(Card card, JsonObject json) : base(card, json)
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
                var ownHandlers = ownSide.CardBuffHandlers.FindAll(entry => entry.CallCard == CallCard);
                var otherHandlers = otherSide.CardBuffHandlers.FindAll(entry => entry.CallCard == CallCard);

                CardBuffArgs ownCardBuffArgs = new CardBuffArgs(ownSide, otherSide, gameState);
                foreach (var handler in ownHandlers)
                {
                    var ownBuffCancelResult = handler.Release();
                    if (ownBuffCancelResult.TryGetInactiveBuffEvent(ownCardBuffArgs, out var inactiveBuffEvent))
                    {
                        inactiveBuffEvent.RegisterEvent(matchContextEvent);
                    }
                    
                    ownSide.CardBuffHandlers.Remove(handler);
                }
                
                CardBuffArgs otherCardBuffArgs = new CardBuffArgs(otherSide, ownSide, gameState);
                foreach (var handler in otherHandlers)
                {
                    var otherBuffCancelResult = handler.Release();
                    if (otherBuffCancelResult.TryGetInactiveBuffEvent(otherCardBuffArgs, out var inactiveBuffEvent))
                    {
                        inactiveBuffEvent.RegisterEvent(matchContextEvent);
                    }
                    
                    otherSide.CardBuffHandlers.Remove(handler);
                }
                
                return;
            }
            
            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
                otherSide.CardBuffHandlers.Add(handler);
            }
        }
        
        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Disabler;
        
            public ExclusiveCardBuff(Card callCard) : base(callCard) { }
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return args.OwnSide.Field;
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                HashSet<Card> infirmaryCardSet = new HashSet<Card>(args.OwnSide.Infirmary.GetAllCards());
                
                return args.OwnSide.IsEffectiveStandOnField(target)
                       && target.AppliedCardBuffs.Exists(buff => infirmaryCardSet.Contains(buff.CallCard));
            }

            public override bool ShouldDisable(CardBuff other, Card target, CardBuffArgs args)
            {
                HashSet<Card> infirmaryCardSet = new HashSet<Card>(args.OwnSide.Infirmary.GetAllCards());

                return infirmaryCardSet.Contains(other.CallCard);
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return 0;
            }
        }
    }
}
