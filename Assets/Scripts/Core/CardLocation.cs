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
        public readonly IPlayer Player;
        
        protected  CardLocation(IPlayer player)
        {
            Player = player;
        }
    }

    public record CardPileLocation : CardLocation
    {
        public override CardZone CardZone => CardZone.CardPile;
        
        public CardPileLocation(IPlayer player) : base(player)
        {
            
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
    }

    public record FieldLocation : CardLocation
    {
        public override CardZone CardZone => CardZone.Field;
        
        public readonly int IndexOfField;

        public FieldLocation(IPlayer owner, int indexOfField) : base(owner)
        {
            IndexOfField = indexOfField;
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
    }

    public static class CardLocationExtensions
    {
        public static bool TryGetCardLocation(this Card card, MatchSide matchSide, out CardLocation cardLocation)
        {
            if (card.Owner == matchSide.Player)
            {
                if (matchSide.Deck.Contains(card))
                {
                    int indexOfDeck = matchSide.Deck.IndexOf(card);
                    cardLocation = new DeckLocation(matchSide.Player, indexOfDeck);
                }
                else if (matchSide.Field.Contains(card))
                {
                    int indexOfField = matchSide.Field.IndexOf(card);
                    cardLocation = new FieldLocation(matchSide.Player, indexOfField);
                }
                else if (matchSide.Infirmary.TryGetValue(card.CardData.nameKey, out var cardPile) && cardPile.Contains(card))
                {
                    string slotKey = card.CardData.nameKey;
                    int indexOfSlot = cardPile.IndexOf(card);
                    cardLocation = new InfirmaryLocation(matchSide.Player, slotKey, indexOfSlot);
                }
            }

            cardLocation = null;
            return false;
        }
    }
}
