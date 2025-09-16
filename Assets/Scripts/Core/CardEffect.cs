using System;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    [Flags]
    public enum EffectTriggerEvent
    {
        OnEnterFieldAsAttacker = 1 << 0,   // drawn to field by attacker
        OnEnterFieldAsDefender = 1 << 1,   // drawn to field by defender
        OnRemainField = 1 << 2,            // remain field either switch position (attack -> defend)
        OnSwitchToDefend = 1 << 3,         // switch position as defend (last one of field)
        OnLeaveField = 1 << 4,
        OnEnterInfirmary = 1 << 5,
        OnLeaveInfirmary = 1 << 6,
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
            
            if (json == null)
            {
                return;
            }

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
        
        public abstract void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent);

        public virtual bool TryReplaceMovement(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            return false;
        }
        
        // activate on recruit phase only
        public virtual bool TryApplyEffectOnRecruit(IPlayer recruiter, GameState gameState, out IContextEvent contextEvent)
        {
            contextEvent = null;
            return false;
        }
        
        protected abstract string GetDescription();
    }

    public class CardEffectArgs
    {
        public readonly EffectTriggerEvent Trigger;
        public readonly MatchSide OwnSide;
        public readonly MatchSide OtherSide;
        public readonly GameState GameState;

        public CardEffectArgs(EffectTriggerEvent trigger, MatchSide ownSide, MatchSide otherSide, GameState gameState)
        {
            Trigger = trigger;
            OwnSide = ownSide;
            OtherSide = otherSide;
            GameState = gameState;
        }

        public void Deconstruct(out EffectTriggerEvent trigger, out MatchSide ownSide, out MatchSide otherSide, out GameState gameState)
        {
            trigger = Trigger;
            ownSide = OwnSide;
            otherSide = OtherSide;
            gameState = GameState;
        }
    }
}
