using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public enum MatchState
    {
        Attacking,
        Defending,
    }
    
    public class MatchSide
    {
        public readonly CardPile Deck = new CardPile();
        public readonly List<Card> Field = new List<Card>();
        public readonly Infirmary Infirmary = new Infirmary();
        
        public readonly IPlayer Player;
        public readonly PlayerState PlayerState;
        
        public readonly List<CardBuffHandleEntry> CardBuffHandlers = new List<CardBuffHandleEntry>();
        
        public MatchState State { get; private set; } = MatchState.Attacking;
        
        private bool IsAttacking => State == MatchState.Attacking;
        
        public MatchSide(PlayerState playerState)
        {
            Player = playerState.Player;
            PlayerState = playerState;
            
            Deck.AddRange(playerState.Deck);
            Deck.Shuffle();
        }

        public void SetMatchState(MatchState state) => State = state;

        public bool TryDraw(out Card drawn)
        {
            bool isSuccessToDraw = Deck.TryDraw(out drawn);
            if (isSuccessToDraw)
            {
                // TODO: call draw effect after implements card abilities
                Field.Add(drawn);
            }

            return isSuccessToDraw;
        }

        public void CheckApplyCardBuffs()
        {
            foreach (var handler in CardBuffHandlers)
            {
                handler.CheckApplyCardBuff(this);
            }
        }

        public int GetEffectivePower(MatchSide otherSide)
        {
            if (Field.Count == 0)
            {
                return 0;
            }

            int effectivePower = IsAttacking
                ? Field.Sum(card => card.GetEffectivePower(this, otherSide))
                : Field[^1].GetEffectivePower(this, otherSide);
            
            return effectivePower;
        }
    }

    public class CardBuffHandleEntry
    {
        public delegate IEnumerable<Card> BuffTargetDelegate(MatchSide matchSide);
        
        public readonly Card CallCard;
        
        private readonly CardBuff _cardBuff;
        private readonly BuffTargetDelegate _targetDelegate;
        
        private readonly HashSet<Card> _appliedCards = new HashSet<Card>();

        public CardBuffHandleEntry(Card callCard, CardBuff cardBuff, BuffTargetDelegate buffTargetDelegate)
        {
            CallCard = callCard;
            _cardBuff = cardBuff;
            _targetDelegate = buffTargetDelegate;
        }

        public void CheckApplyCardBuff(MatchSide matchSide)
        {
            var targets = _targetDelegate(matchSide)
                .Where(target => !_appliedCards.Contains(target))   // check target already apply buff
                .ToArray();

            foreach (var target in targets)
            {
                target.ApplyCardBuff(_cardBuff);
                _appliedCards.Add(target);
            }
            
            // return targets.Any();
        }

        public void Release()
        {
            foreach (var applied in _appliedCards)
            {
                applied.RemoveCardBuff(_cardBuff);
            }
            
            _appliedCards.Clear();
        }
    }
}
