using ProjectABC.Data;

namespace ProjectABC.Core
{
    public sealed class EliteCardEffect : CardEffect
    {
        public EliteCardEffect(Card card, JsonObject json) : base(card, json)
        {
            
        }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            // Do nothing
        }
    }
    
    public sealed class EmptyCardEffect : CardEffect
    {
        public EmptyCardEffect(Card card, JsonObject json) : base(card, json)
        {
            
        }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            // Do nothing
        }
    }
}