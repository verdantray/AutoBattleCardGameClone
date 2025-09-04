

namespace ProjectABC.Core
{
    public enum BuffType
    {
        Aura,
        Disabler,
    }
    
    public abstract class CardBuff
    {
        public virtual BuffType Type { get; }

        public readonly Card CallCard;

        protected CardBuff(Card callCard)
        {
            CallCard = callCard;
        }
        
        public abstract bool IsBuffActive(Card target, MatchSide mySide, MatchSide otherSide);
        
        public abstract int CalculateAdditivePower(Card target, MatchSide mySide, MatchSide otherSide);
        
        public virtual bool ShouldDisable(CardBuff other, Card target, MatchSide mySide, MatchSide otherSide)
        {
            return false;
        }
    }
}
