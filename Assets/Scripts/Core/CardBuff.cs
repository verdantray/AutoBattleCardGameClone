using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public abstract class CardBuff
    {
        public int GetEffectivePower(Card target, MatchSide matchSide)
        {
            return IsBuffActive(target, matchSide)
                ? CalculateAdditivePower(target, matchSide)
                : 0;
        }
        
        public abstract bool IsBuffActive(Card target, MatchSide matchSide);
        public abstract int CalculateAdditivePower(Card target, MatchSide matchSide);
    }
}
