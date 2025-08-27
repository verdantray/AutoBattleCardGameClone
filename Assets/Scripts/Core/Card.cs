using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;
using ProjectABC.Utils;

namespace ProjectABC.Core
{
    public class CardPile : IReadOnlyList<Card>
    {
        private readonly List<Card> _cardList = new List<Card>();

        #region inherits of IReadOnlyList

        public int Count => _cardList.Count;

        public Card this[int index] => _cardList[index];
        public IEnumerator<Card> GetEnumerator() => _cardList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _cardList.GetEnumerator();

        #endregion
        
        public void AddToTop(Card card) => _cardList.Insert(0, card);
        public void AddToTopRange(IEnumerable<Card> cards) => _cardList.InsertRange(0, cards);
        
        public void Add(Card card) => _cardList.Add(card);
        public void AddRange(IEnumerable<Card> cards) => _cardList.AddRange(cards);
        public void Clear() => _cardList.Clear();

        public bool TryDraw(out Card card)
        {
            bool isSuccess = _cardList.Count > 0;

            card = isSuccess ? DrawCards(1).First() : null;
            return isSuccess;
        }

        public List<Card> DrawCards(int amount)
        {
            List<Card> toDraw = _cardList.GetRange(0, amount);
            _cardList.RemoveRange(0, amount);

            return toDraw;
        }

        public void Shuffle(int? seed = null)
        {
            _cardList.Shuffle(seed);
        }
    }

    public class Infirmary : IReadOnlyDictionary<string, CardPile>
    {
        private readonly Dictionary<string, CardPile> _cardMap = new Dictionary<string, CardPile>();
        private readonly Dictionary<string, int> _keyOrderMap = new Dictionary<string, int>(); // order starts at 1
        
        public int SlotLimit { get; private set; } = GameConst.GameOption.DEFAULT_INFIRMARY_SLOT_LIMIT;
        public int RemainSlots => SlotLimit - _cardMap.Count;

        #region inherits of IReadOnlyDictionary

        public int Count => _cardMap.Count;
        
        public CardPile this[string key] => _cardMap[key];

        public IEnumerable<string> Keys => _cardMap.Keys;
        public IEnumerable<CardPile> Values => _cardMap.Values;
        
        public IEnumerator<KeyValuePair<string, CardPile>> GetEnumerator() => _cardMap.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _cardMap.GetEnumerator();
        public bool ContainsKey(string key) => _cardMap.ContainsKey(key);

        public bool TryGetValue(string key, out CardPile value) => _cardMap.TryGetValue(key, out value);

        #endregion

        public void SetSlotLimit(int slotLimit) => SlotLimit = slotLimit;

        public void Clear()
        {
            _cardMap.Clear();
            _keyOrderMap.Clear();
        }

        // Regardless success or failure, cards that comes as arg are put on the bench
        public bool TryPut(IEnumerable<Card> cards, out int remainSlots)
        {
            foreach (var card in cards)
            {
                if (_keyOrderMap.ContainsKey(card.Name))
                {
                    _cardMap[card.Name].Add(card);
                    continue;
                }

                var existingOrders = _keyOrderMap.Values;
                int latestOrder = existingOrders.Count > 0 ? existingOrders.Max() : 0;

                HashSet<int> missingOrders = new HashSet<int>(Enumerable.Range(1, latestOrder));

                foreach (var existingOrder in existingOrders)
                {
                    missingOrders.Remove(existingOrder);
                }

                int newlyPuttingOrder = missingOrders.Count > 0
                    ? missingOrders.Min()
                    : latestOrder + 1;
                
                _keyOrderMap.Add(card.Name, newlyPuttingOrder);

                CardPile cardPile = new CardPile { card };
                _cardMap.Add(card.Name, cardPile);
            }

            remainSlots = RemainSlots;
            return remainSlots > 0;
        }

        public void PutCard(Card card)
        {
            if (_keyOrderMap.ContainsKey(card.Name))
            {
                _cardMap[card.Name].Add(card);
                return;
            }
            
            var existingOrders = _keyOrderMap.Values;
            int latestOrder = existingOrders.Max(); // if existingOrders is empty, then Max will return 0

            HashSet<int> missingOrders = new HashSet<int>(Enumerable.Range(1, latestOrder));

            foreach (var existingOrder in existingOrders)
            {
                missingOrders.Remove(existingOrder);
            }

            int newlyPuttingOrder = missingOrders.Count > 0
                ? missingOrders.Min()
                : latestOrder + 1;
                
            _keyOrderMap.Add(card.Name, newlyPuttingOrder);
                
            CardPile cardPile = new CardPile { card };
            _cardMap.Add(card.Name, cardPile);
        }

        public bool TryDrawCards(out CardPile cardPile)
        {
            if (Count == 0)
            {
                cardPile = null;
                return false;
            }

            var earliestBenchSlotKey = _keyOrderMap.OrderBy(kvPair => kvPair.Value).First().Key;

            return _cardMap.Remove(earliestBenchSlotKey, out cardPile);
        }
        
        public IEnumerable<Card> GetAllCards() => _cardMap.Values.SelectMany(cardPile => cardPile);
    }

    public class Card
    {
        public string Id => _cardData.id;
        public int BasePower => _cardData.basePower;
        
        public ClubType ClubType { get; private set; }
        public GradeType GradeType { get; private set; }
        public int Power { get; private set; }

        public string Title => _cardData.titleKey;
        public string Name => _cardData.nameKey;
        public string Description => _cardData.descKey;
        
        private readonly CardData _cardData;

        public Card(CardData cardData)
        {
            _cardData = cardData;

            ClubType = _cardData.clubType;
            GradeType = _cardData.gradeType;
            Power = _cardData.basePower;
        }

        public override string ToString()
        {
            return $"(Card Id : {Id} / Power : {Power} / Club : {ClubType} / Grade : {GradeType})";
        }
    }
}
