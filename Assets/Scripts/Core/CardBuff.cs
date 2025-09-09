using System.Collections.Generic;


namespace ProjectABC.Core
{
    public enum BuffType
    {
        Positive,
        Negative,
        Disabler,
    }
    
    public abstract class CardBuff
    {
        public abstract BuffType Type { get; }

        public readonly Card CallCard;

        protected CardBuff(Card callCard)
        {
            CallCard = callCard;
        }

        public abstract IEnumerable<Card> GetBuffTargets(CardBuffArgs args);
        
        public abstract bool IsBuffActive(Card target, CardBuffArgs args);
        
        public abstract int CalculateAdditivePower(Card target, CardBuffArgs args);
        
        public virtual bool ShouldDisable(CardBuff other, Card target, CardBuffArgs args)
        {
            return false;
        }
    }

    public class CardBuffArgs
    {
        public readonly MatchSide OwnSide;
        public readonly MatchSide OtherSide;
        public readonly GameState GameState;

        public CardBuffArgs(MatchSide ownSide, MatchSide otherSide, GameState gameState)
        {
            OwnSide = ownSide;
            OtherSide = otherSide;
            GameState = gameState;
        }

        public void Deconstruct(out MatchSide ownSide, out MatchSide otherSide, out GameState gameState)
        {
            ownSide = OwnSide;
            otherSide = OtherSide;
            gameState = GameState;
        }
    }
}
