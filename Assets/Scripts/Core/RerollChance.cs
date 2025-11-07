using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public record RerollBonus
    {
        // TODO : flesh out after implement argument features
        
        public readonly int BonusRerollChance;

        public RerollBonus(int bonusRerollChance)
        {
            BonusRerollChance = bonusRerollChance;
        }
    }
    
    public class RerollChance
    {
        private readonly int _defaultMaxRerollChance;

        private readonly List<RerollBonus> _bonuses =  new List<RerollBonus>();
        
        public int MaxRerollChance => _defaultMaxRerollChance + _bonuses.Sum(bonus => bonus.BonusRerollChance);
        public int RemainRerollChance { get; private set; }

        public RerollChance(int defaultMaxRerollChance = GameConst.GameOption.MULLIGAN_DEFAULT_AMOUNT)
        {
            _defaultMaxRerollChance = defaultMaxRerollChance;
            Reset();
        }

        public List<Card> GetRerollCards(CardPile cardPile, int drawSize)
        {
            RemainRerollChance--;
            return cardPile.DrawCards(drawSize);
        }

        public void AddRerollBonus(RerollBonus bonus)
        {
            _bonuses.Add(bonus);
        }

        public void Reset()
        {
            RemainRerollChance = MaxRerollChance;
        }
    }
}
