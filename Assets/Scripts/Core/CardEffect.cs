using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public enum EffectTrigger
    {
        OnEnterField,
        OnWhileAttacking,
        OnWhileDefending,
        OnLeaveField,
        OnEnterInfirmary,
        OnLeaveInfirmary,
    }
    
    public interface ICardEffect
    {
        public Card CallCard { get; }
        public bool TryApplyEffect(MatchSide mySide, out IMatchEvent matchEvent);
    }

    public abstract class CardEffect : ICardEffect
    {
        public Card CallCard { get; }
        public string Description => GetDescription();
        
        protected readonly string DescriptionKey;
        
        protected CardEffect(Card card, JsonObject json)
        {
            CallCard = card;

            foreach (var field in json.fields.Where(field => field.key == GameConst.CardEffect.EFFECT_DESCRIPTION_KEY))
            {
                DescriptionKey = field.value.strValue;
            }
        }
        
        public abstract bool TryApplyEffect(MatchSide mySide, out IMatchEvent matchEvent);
        protected abstract string GetDescription();
    }
}
