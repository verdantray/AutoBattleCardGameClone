using System;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public enum CardMovingEvent
    {
        OnEnterField,
        OnWhileAttacking,
        OnWhileDefending,
        OnLeaveField,
        OnEnterInfirmary,
        OnLeaveInfirmary,
    }

    public abstract class CardEffect
    {
        public Card CallCard { get; }
        public string Description => GetDescription();

        protected readonly string EffectId;
        protected readonly string DescriptionKey;
        protected readonly CardMovingEvent ApplyTrigger;
        
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
                    case GameConst.CardEffect.EFFECT_APPLY_TRIGGER_KEY:
                        ApplyTrigger = Enum.Parse<CardMovingEvent>(field.value.strValue, true);
                        break;
                }
            }
        }

        public abstract bool TryApplyEffect(CardMovingEvent trigger, MatchSide mySide, MatchSide otherSide, out IMatchEvent matchEvent);
        
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
