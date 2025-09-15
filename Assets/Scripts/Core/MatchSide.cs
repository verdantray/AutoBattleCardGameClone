using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    [Flags]
    public enum MatchState
    {
        Attacking = 1 << 0,
        Defending = 1 << 1,
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
        
        public bool IsAttacking => State == MatchState.Attacking;
        
        public MatchSide(PlayerState playerState)
        {
            Player = playerState.Player;
            PlayerState = playerState;

            foreach (var card in playerState.Deck)
            {
                Deck.Add(card);
            }
            Deck.Shuffle();
        }

        public void SetMatchState(MatchState state) => State = state;

        public bool TryDraw(out Card drawn)
        {
            bool isSuccessToDraw = Deck.TryDraw(out drawn);
            if (isSuccessToDraw)
            {
                Field.Add(drawn);
            }

            return isSuccessToDraw;
        }

        public void CheckApplyCardBuffs(MatchSide otherSide, GameState gameState)
        {
            CardBuffArgs cardBuffArgs = new CardBuffArgs(this, otherSide, gameState);
            
            foreach (var handler in CardBuffHandlers)
            {
                handler.CheckApplyCardBuff(cardBuffArgs);
            }
        }

        public int GetEffectivePower(MatchSide otherSide, GameState gameState)
        {
            if (Field.Count == 0)
            {
                return 0;
            }

            CardBuffArgs cardBuffArgs = new CardBuffArgs(this, otherSide, gameState);

            int effectivePower = Field
                .Where(IsEffectiveStandOnField)
                .Sum(card => card.GetEffectivePower(cardBuffArgs));
            
            return effectivePower;
        }

        public bool IsEffectiveStandOnField(Card card)
        {
            return IsAttacking
                ? Field.Contains(card)
                : Field[^1] == card;
        }
    }

    public class CardBuffHandleEntry
    {
        public readonly Card CallCard;
        
        private readonly CardBuff _cardBuff;
        private readonly HashSet<Card> _appliedCards = new HashSet<Card>();

        public CardBuffHandleEntry(Card callCard, CardBuff cardBuff)
        {
            CallCard = callCard;
            _cardBuff = cardBuff;
        }

        public void CheckApplyCardBuff(CardBuffArgs args)
        {
            var buffTargets = _cardBuff.GetBuffTargets(args);
            var exhaustedTargets = _appliedCards
                .Where(applied => !buffTargets.Contains(applied))
                .ToList();

            foreach (var exhausted in exhaustedTargets)
            {
                exhausted.RemoveCardBuff(_cardBuff);
                _appliedCards.Remove(exhausted);
            }

            foreach (var newlyApply in buffTargets)
            {
                if (_appliedCards.Contains(newlyApply))
                {
                    continue;
                }
                
                newlyApply.ApplyCardBuff(_cardBuff);
                _appliedCards.Add(newlyApply);
            }
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
