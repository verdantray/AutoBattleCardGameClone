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
        private readonly List<string> _nameKeyList = new List<string>();   // do not allow duplicates, change order
        private readonly Dictionary<string, CardPile> _cardMap = new Dictionary<string, CardPile>();
        
        public int SlotLimit { get; private set; } = GameConst.GameOption.DEFAULT_INFIRMARY_SLOT_LIMIT;
        public int RemainSlotCount => SlotLimit - _cardMap.Count;
        public bool IsSlotRemains => RemainSlotCount > 0;
        
        public CardPile this[int index] => _cardMap[_nameKeyList[index]];

        #region inherits of IReadOnlyDictionary

        public int Count => _cardMap.Count;
        
        public CardPile this[string key] => _cardMap[key];

        public IEnumerable<string> Keys => _nameKeyList;
        public IEnumerable<CardPile> Values => _cardMap.Values;
        
        public IEnumerator<KeyValuePair<string, CardPile>> GetEnumerator() => _cardMap.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _cardMap.GetEnumerator();
        public bool ContainsKey(string key) => _cardMap.ContainsKey(key);

        public bool TryGetValue(string key, out CardPile value) => _cardMap.TryGetValue(key, out value);

        #endregion

        public void SetSlotLimit(int slotLimit) => SlotLimit = slotLimit;

        public void Clear()
        {
            _nameKeyList.Clear();
            _cardMap.Clear();
        }

        // Regardless success or failure, cards that comes as arg are put on the bench
        public void PutCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                string cardNameKey = card.CardData.nameKey;
                
                if (_nameKeyList.Contains(cardNameKey))
                {
                    _cardMap[cardNameKey].Add(card);
                    continue;
                }

                if (RemainSlotCount <= 0)
                {
                    this[SlotLimit - 1].Add(card);
                    continue;
                }

                _nameKeyList.Add(cardNameKey);
                _cardMap[cardNameKey] = new CardPile { card };
            }
        }

        public bool TryDrawCards(out CardPile cardPile)
        {
            string earliestName = _nameKeyList[0];
            _nameKeyList.RemoveAt(0);

            cardPile = _cardMap[earliestName];
            _cardMap.Remove(earliestName);

            return true;
        }
        
        public IEnumerable<Card> GetAllCards() => _cardMap.Values.SelectMany(cardPile => cardPile);
        public InfirmaryInstance GetSnapshotInstance => new InfirmaryInstance(_nameKeyList, _cardMap);
    }

    public class Card
    {
        public string Id => CardData.id;
        public int BasePower => CardData.basePower;
        
        public ClubType ClubType { get; private set; }
        public GradeType GradeType { get; private set; }
        public int Power { get; private set; }

        public string Title => CardData.titleKey;
        public string Name => CardData.nameKey;
        public string Description => CardData.descKey;
        
        public readonly CardData CardData;

        public Card(CardData cardData)
        {
            CardData = cardData;

            ClubType = CardData.clubType;
            GradeType = CardData.gradeType;
            Power = CardData.basePower;
        }

        public override string ToString()
        {
            return $"(Card Id : {Id} / Power : {Power} / Club : {ClubType} / Grade : {GradeType})";
        }
    }
}
