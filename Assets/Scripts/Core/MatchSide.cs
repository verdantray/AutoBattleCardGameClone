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

            foreach (var cardId in playerState.RecruitedCardIds)
            {
                var cardData = Storage.Instance.GetCardData(cardId);
                var card = new Card(Player, cardData);
                
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
                
                if (result.TryGetActiveBuffEvent(cardBuffArgs, out var activeBuffEvent))
                {
                    buffEvents.Add(activeBuffEvent);
                }

                if (result.TryGetInactiveBuffEvent(cardBuffArgs, out var inactiveBuffEvent))
                {
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
            if (!Field.Contains(card))
            {
                return false;
            }
            
            return IsAttacking || Field[^1] == card;
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
            var currentAllApplied = _cardBuff.GetBuffTargets(args).ToList();
            
            var newlyApplyCards = currentAllApplied
                .Where(applied => !_appliedCards.Contains(applied))
                .ToList();
            
            var canceledCards = _appliedCards
                .Where(applied => !currentAllApplied.Contains(applied))
                .ToList();

            foreach (var canceled in canceledCards)
            {
                canceled.RemoveCardBuff(_cardBuff);
                _appliedCards.Remove(canceled);
            }

            foreach (var applied in newlyApplyCards)
            {
                applied.ApplyCardBuff(_cardBuff);
                _appliedCards.Add(applied);
            }
            
            return new CardBuffApplyResult(CallCard, newlyApplyCards, canceledCards);
        }

        public CardBuffApplyResult Release()
        {
            foreach (var applied in _appliedCards)
            {
                applied.RemoveCardBuff(_cardBuff);
            }
            
            CardBuffApplyResult result = new CardBuffApplyResult(CallCard, Array.Empty<Card>(), _appliedCards);
            _appliedCards.Clear();
            
            return result;
        }
    }

    public class CardBuffApplyResult
    {
        private readonly Card _activateCard;
        private readonly List<Card> _applyCards = new List<Card>();
        private readonly List<Card> _canceledCards = new List<Card>();

        public CardBuffApplyResult(Card activateCard, IEnumerable<Card> applyCards, IEnumerable<Card> canceledCards)
        {
            _activateCard = activateCard;
            _applyCards.AddRange(applyCards);
            _canceledCards.AddRange(canceledCards);
        }

        public bool TryGetActiveBuffEvent(CardBuffArgs args, out IMatchEvent buffEvent)
        {
            bool isApplyCardsExist = _applyCards.Count > 0;
            buffEvent = isApplyCardsExist
                ? new ActiveBuffEvent(_activateCard, _applyCards, args)
                : null;
            
            return isApplyCardsExist;
        }

        public bool TryGetInactiveBuffEvent(CardBuffArgs args, out IMatchEvent buffEvent)
        {
            bool isCanceledCardsExist = _canceledCards.Count > 0;
            buffEvent = isCanceledCardsExist
                ? new InactiveBuffEvent(_activateCard, _canceledCards, args)
                : null;

            return isCanceledCardsExist;
        }
    }
}
