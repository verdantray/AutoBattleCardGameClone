using System;
using System.Linq;

namespace ProjectABC.Core
{
    public enum CardZone
    {
        CardPile,
        Deck,
        Field,
        Infirmary,
    }

    public abstract record CardLocation
    {
        public abstract CardZone CardZone { get; }
        public readonly IPlayer Owner;
        
        protected CardLocation(IPlayer owner)
        {
            Owner = owner;
        }

        public abstract T PeekFromLocation<T>(ICardLocator<T> locator) where T : class;
        public abstract T PopFromLocation<T>(ICardLocator<T> locator) where T : class;
        public abstract void InsertToLocation<T>(ICardLocator<T> locator, T element) where T : class;
        public abstract void ChangeCardToLocation<T>(ICardLocator<T> locator, T element) where T : class;
        public override string ToString()
        {
            return $"Zone : {CardZone} / Owner : {Owner.Name}";
        }
    }

    public record CardPileLocation : CardLocation
    {
        public override CardZone CardZone => CardZone.CardPile;

        public CardPileLocation(IPlayer owner) : base(owner)
        {
            
        }

        public override T PeekFromLocation<T>(ICardLocator<T> locator)
        {
            // not use on this class
            return null;
        }

        public override T PopFromLocation<T>(ICardLocator<T> locator)
        {
            // not use on this class
            return null;
        }

        public override void InsertToLocation<T>(ICardLocator<T> locator, T element)
        {
            // do nothing
        }

        public override void ChangeCardToLocation<T>(ICardLocator<T> locator, T element)
        {
            // do nothing
        }
    }
    
    public record DeckLocation : CardLocation
    {
        public override CardZone CardZone => CardZone.Deck;

        public readonly int IndexOfDeck;

        public DeckLocation(IPlayer owner, int indexOfDeck) : base(owner)
        {
            IndexOfDeck = indexOfDeck;
        }

        public override T PeekFromLocation<T>(ICardLocator<T> locator)
        {
            return locator[Owner].Deck.Peek(IndexOfDeck);
        }

        public override T PopFromLocation<T>(ICardLocator<T> locator)
        {
            return locator[Owner].Deck.Pop(IndexOfDeck);
        }

        public override void InsertToLocation<T>(ICardLocator<T> locator, T element)
        {
            locator[Owner].Deck.Insert(IndexOfDeck, element);
        }

        public override void ChangeCardToLocation<T>(ICardLocator<T> locator, T element)
        {
            locator[Owner].Deck.Change(IndexOfDeck, element);
        }

        public override string ToString()
        {
            return $"Zone : {CardZone} / Index : {IndexOfDeck} / Owner : {Owner.Name}";
        }
    }

    public record FieldLocation : CardLocation
    {
        public override CardZone CardZone => CardZone.Field;

        public readonly int IndexOfField;

        public FieldLocation(IPlayer owner, int indexOfField) : base(owner)
        {
            IndexOfField = indexOfField;
        }

        public override T PeekFromLocation<T>(ICardLocator<T> locator)
        {
            return locator[Owner].Field.Peek(IndexOfField);
        }

        public override T PopFromLocation<T>(ICardLocator<T> locator)
        {
            return locator[Owner].Field.Pop(IndexOfField);
        }

        public override void InsertToLocation<T>(ICardLocator<T> locator, T element)
        {
            locator[Owner].Field.Insert(IndexOfField, element);
        }

        public override void ChangeCardToLocation<T>(ICardLocator<T> locator, T element)
        {
            locator[Owner].Field.Change(IndexOfField, element);
        }

        public override string ToString()
        {
            return $"Zone : {CardZone} / Index : {IndexOfField} / Owner : {Owner.Name}";
        }
    }

    public record InfirmaryLocation : CardLocation
    {
        public override CardZone CardZone => CardZone.Infirmary;

        public readonly string SlotKey;
        public readonly int IndexOfInfirmarySlot;

        public InfirmaryLocation(IPlayer owner, string slotKey, int indexOfInfirmarySlot) : base(owner)
        {
            SlotKey = slotKey;
            IndexOfInfirmarySlot = indexOfInfirmarySlot;
        }

        public override T PeekFromLocation<T>(ICardLocator<T> locator)
        {
            return locator[Owner].Infirmary.Peek(SlotKey, IndexOfInfirmarySlot);
        }

        public override T PopFromLocation<T>(ICardLocator<T> locator)
        {
            return locator[Owner].Infirmary.Pop(SlotKey, IndexOfInfirmarySlot);
        }

        public override void InsertToLocation<T>(ICardLocator<T> locator, T element)
        {
            locator[Owner].Infirmary.Insert(SlotKey, IndexOfInfirmarySlot, element);
        }

        public override void ChangeCardToLocation<T>(ICardLocator<T> locator, T element)
        {
            locator[Owner].Infirmary.Change(SlotKey, IndexOfInfirmarySlot, element);
        }

        public override string ToString()
        {
            return $"Zone : {CardZone} / SlotKey : {SlotKey} / Index : {IndexOfInfirmarySlot} / Owner : {Owner.Name}";
        }
    }

    public static class CardLocationExtensions
    {
        public static CardLocation GetCardLocation(this Card card, params MatchSide[] matchSides)
        {
            foreach (var matchSide in matchSides)
            {
                if (card.TryGetCardLocation(matchSide, out CardLocation cardLocation))
                {
                    return cardLocation;
                }
            }

            throw new InvalidOperationException($"{card.Id} not found given matchSides... owner : {card.Owner.Name} / given sides : {string.Join(",", matchSides.Select(side => side.Player.Name))}");
        }
        
        public static bool TryGetCardLocation(this Card card, MatchSide matchSide, out CardLocation cardLocation)
        {
            if (ReferenceEquals(card.Owner, matchSide.Player))
            {
                if (matchSide.Deck.Contains(card))
                {
                    int indexOfDeck = matchSide.Deck.IndexOf(card);
                    cardLocation = new DeckLocation(matchSide.Player, indexOfDeck);
                    return true;
                }
                
                if (matchSide.Field.Contains(card))
                {
                    int indexOfField = matchSide.Field.IndexOf(card);
                    cardLocation = new FieldLocation(matchSide.Player, indexOfField);
                    return true;
                }
                
                if (matchSide.Infirmary.TryGetValue(card.CardData.nameKey, out var cardPile) && cardPile.Contains(card))
                {
                    string slotKey = card.CardData.nameKey;
                    int indexOfSlot = cardPile.IndexOf(card);
                    cardLocation = new InfirmaryLocation(matchSide.Player, slotKey, indexOfSlot);
                    return true;
                }

                throw new InvalidOperationException($"{card.Id} is {card.Owner}'s card but cannot found location");
            }

            cardLocation = null;
            return false;
        }
    }
}
