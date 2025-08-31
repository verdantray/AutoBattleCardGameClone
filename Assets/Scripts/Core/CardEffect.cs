using System;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    [Flags]
    public enum EffectTriggerEvent
    {
        OnEnterFieldAsAttacker = 2^0,   // drawn to field by attacker
        OnEnterFieldAsDefender = 2^1,   // drawn to field by defender
        OnRemainField = 2^2,            // remain field either switch position (attack -> defend)
        OnSwitchToDefend = 2^3,         // switch position as defend (last one of field)
        OnLeaveField = 2^4,
        OnEnterInfirmary = 2^5,
        OnLeaveInfirmary = 2^6,
    }

    public abstract class CardEffect
    {
        public Card CallCard { get; }
        public string Description => GetDescription();

        protected readonly string EffectId;
        protected readonly string DescriptionKey;
        protected readonly EffectTriggerEvent ApplyTriggerFlag;
        
        protected CardEffect(Card card, JsonObject json)
        {
            CallCard = card;

            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_ID:
                        EffectId = field.value.strValue;
                        break;
                    case GameConst.CardEffect.EFFECT_DESC_KEY:
                        DescriptionKey = field.value.strValue;
                        break;
                    case GameConst.CardEffect.EFFECT_APPLY_TRIGGERS_KEY:

                        EffectTriggerEvent flag = 0;
                        
                        foreach (var element in field.value.arr)
                        {
                            flag |= Enum.Parse<EffectTriggerEvent>(element.strValue, true);
                        }

                        ApplyTriggerFlag = flag;
                        break;
                }
            }
        }

        public abstract bool TryApplyEffect(EffectTriggerEvent trigger, MatchSide mySide, MatchSide otherSide, out IMatchEvent matchEvent);
        
        protected abstract string GetDescription();
    }

    public class FailToApplyCardEffectEvent : MatchEventBase
    {
        public enum FailureReason
        {
            TriggerNotMatch,
            FailureMeetCondition,
        }

        public readonly FailureReason Reason;
        public readonly string FailureDescription;
        
        public FailToApplyCardEffectEvent(FailureReason reason, string failureDescription, MatchSnapshot snapshot) : base(snapshot)
        {
            Reason = reason;
            FailureDescription = failureDescription;
        }
    }
}
