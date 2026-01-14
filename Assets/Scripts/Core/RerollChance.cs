using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public record RerollBonus
    {
        public readonly int BonusRerollChance;
        public readonly bool UnlimitedBonusReroll;

        public RerollBonus(int bonusRerollChance)
        {
            BonusRerollChance = bonusRerollChance;
            UnlimitedBonusReroll = false;
        }

        public RerollBonus(bool unlimitedBonusReroll)
        {
            BonusRerollChance = 0;
            UnlimitedBonusReroll = unlimitedBonusReroll;
        }
    }
    
    public class RerollChance
    {
        public int RemainRerollChance { get; private set; }
        
        private int MaxRerollChance => _maxRerollChances + _bonuses.Sum(bonus => bonus.BonusRerollChance);
        private bool UnlimitedReroll => _bonuses.Any(bonus => bonus.UnlimitedBonusReroll);
        
        private readonly int _maxRerollChances;
        private readonly List<RerollBonus> _bonuses =  new List<RerollBonus>();
        
        public RerollChance(int maxRerollChances)
        {
            _maxRerollChances = maxRerollChances;
            Reset();
        }

        public List<string> GetRerollCardIds(bool consumeChance, CardIdQueue idQueue, int drawSize)
        {
            if (consumeChance)
            {
                ConsumeRerollCost();
            }
            
            return idQueue.DequeueCardIds(drawSize);
        }

        public void AddRerollBonus(RerollBonus bonus)
        {
            _bonuses.Add(bonus);
        }

        private void ConsumeRerollCost()
        {
            if (UnlimitedReroll)
            {
                return;
            }

            RemainRerollChance--;
        }

        public void Reset()
        {
            RemainRerollChance = MaxRerollChance;
        }
    }
}
