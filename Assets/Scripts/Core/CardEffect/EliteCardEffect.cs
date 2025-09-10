using ProjectABC.Data;

namespace ProjectABC.Core
{
    public sealed class EliteCardEffect : CardEffect
    {
        public EliteCardEffect(Card card, JsonObject json) : base(card, json)
        {
            
        }

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            // Do nothing
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
    }
    
    public sealed class EmptyCardEffect : CardEffect
    {
        public EmptyCardEffect(Card card, JsonObject json) : base(card, json)
        {
            
        }

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            // Do nothing
        }

        protected override string GetDescription()
        {
            return string.Empty;
        }
    }
}