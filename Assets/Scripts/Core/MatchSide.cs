using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    [Flags]
    public enum MatchPosition
    {
        Attacking = 1 << 1,
        Defending = 1 << 2,
    }
    
    public class MatchSide
    {
        public readonly CardPile Deck = new CardPile();
        public readonly List<Card> Field = new List<Card>();
        public readonly Infirmary Infirmary = new Infirmary();
        
        public readonly IPlayer Player;
        
        public readonly List<CardBuffHandleEntry> CardBuffHandlers = new List<CardBuffHandleEntry>();
        
        public MatchPosition Position { get; private set; } = MatchPosition.Attacking;
        
        public bool IsAttacking => Position == MatchPosition.Attacking;
        
        public MatchSide(PlayerState playerState)
        {
            Player = playerState.Player;

            foreach (var card in playerState.Deck)
            {
                Deck.Add(card);
            }
            
            Deck.Shuffle();
        }

        public void SetMatchState(MatchPosition position) => Position = position;

        public bool TryDraw(out Card drawn)
        {
            bool isSuccessToDraw = Deck.TryDraw(out drawn);
            if (isSuccessToDraw)
            {
                Field.Add(drawn);
            }

            return isSuccessToDraw;
        }

        public void CheckApplyCardBuffs(MatchSide otherSide, GameState gameState, out List<IMatchEvent> buffEvents)
        {
            CardBuffArgs cardBuffArgs = new CardBuffArgs(this, otherSide, gameState);
            buffEvents = new List<IMatchEvent>();
            
            foreach (var handler in CardBuffHandlers)
            {
                var result = handler.CheckApplyCardBuff(cardBuffArgs);
                
                if (result.ApplyCards.Count > 0)
                {
                    ActiveBuffEvent activeBuffEvent = new ActiveBuffEvent(handler.CallCard, result.ApplyCards, cardBuffArgs);
                    buffEvents.Add(activeBuffEvent);
                }

                if (result.CanceledCards.Count > 0)
                {
                    InactiveBuffEvent inactiveBuffEvent =  new InactiveBuffEvent(handler.CallCard, result.CanceledCards, cardBuffArgs);
                    buffEvents.Add(inactiveBuffEvent);
                }
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

        public CardBuffApplyResult CheckApplyCardBuff(CardBuffArgs args)
        {
            var newlyApplyCards = _cardBuff
                .GetBuffTargets(args)
                .Where(applied => !_appliedCards.Contains(applied))
                .ToList();
            var canceledCards = _appliedCards
                .Where(applied => !newlyApplyCards.Contains(applied))
                .ToList();

            foreach (var canceled in canceledCards)
            {
                canceled.RemoveCardBuff(_cardBuff);
                _appliedCards.Remove(canceled);
            }

            foreach (var applied in newlyApplyCards)
            {
                if (_appliedCards.Contains(applied))
                {
                    continue;
                }
                
                applied.ApplyCardBuff(_cardBuff);
                _appliedCards.Add(applied);
            }
            
            return new CardBuffApplyResult(newlyApplyCards, canceledCards);
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

    public class CardBuffApplyResult
    {
        public readonly IReadOnlyList<Card> ApplyCards;
        public readonly IReadOnlyList<Card> CanceledCards;

        public CardBuffApplyResult(IReadOnlyList<Card> applyCards, IReadOnlyList<Card> canceledCards)
        {
            ApplyCards = applyCards;
            CanceledCards = canceledCards;
        }
    }
}
