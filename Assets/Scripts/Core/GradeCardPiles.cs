using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;
using ProjectABC.Utils;


namespace ProjectABC.Core
{
    public sealed class GradeCardIdQueue : IReadOnlyDictionary<GradeType, CardIdQueue>
    {
        private readonly Dictionary<GradeType, CardIdQueue> _cardIdQueues = new Dictionary<GradeType, CardIdQueue>
        {
            { GradeType.First, new CardIdQueue() },
            { GradeType.Second, new CardIdQueue() },
            { GradeType.Third, new CardIdQueue() },
        };

        public void Initialize(IEnumerable<CardData> cardData)
        {
            foreach (var groupedData in cardData.GroupBy(card => card.gradeType))
            {
                GradeType grade = groupedData.Key;
                _cardIdQueues[grade].Initialize(groupedData);
            }
        }
        
        #region inherits of IReadOnlyDictionary

        public CardIdQueue this[GradeType key] => _cardIdQueues[key];

        public int Count => _cardIdQueues.Count;

        public IEnumerable<GradeType> Keys => _cardIdQueues.Keys;
        
        public IEnumerable<CardIdQueue> Values =>  _cardIdQueues.Values;
        
        public IEnumerator<KeyValuePair<GradeType, CardIdQueue>> GetEnumerator()
        {
            return _cardIdQueues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cardIdQueues.GetEnumerator();
        }
        
        public bool ContainsKey(GradeType key) => _cardIdQueues.ContainsKey(key);

        public bool TryGetValue(GradeType key, out CardIdQueue value) => _cardIdQueues.TryGetValue(key, out value);

        #endregion
    }

    public sealed class CardIdQueue : IReadOnlyList<string>
    {
        private readonly List<string> _idList = new List<string>();

        public void Initialize(IEnumerable<CardData> cardData)
        {
            _idList.Clear();
            
            foreach (var data in cardData)
            {
                for (int i = 0; i < data.amount; i++)
                {
                    EnqueueCardId(data.id);
                }
            }
            
            Shuffle();
        }
        
        public List<string> DequeueCardIds(int amount)
        {
            int index = _idList.Count - amount;
            
            List<string> toPop = _idList.GetRange(index, amount);
            _idList.RemoveRange(index, amount);
            
            return toPop;
        }

        public bool TryDraw(IPlayer player, out Card drawnCard)
        {
            bool isDrawable = _idList.Count > 0;

            if (isDrawable)
            {
                string cardId = DequeueCardIds(1)[0];
                var cardData = Storage.Instance.GetCardData(cardId);

                drawnCard = new Card(player, cardData);
            }
            else
            {
                drawnCard = null;
            }
            
            return isDrawable;
        }

        public void EnqueueCardIds(IEnumerable<string> cardIds)
        {
            foreach (var cardId in cardIds)
            {
                EnqueueCardId(cardId);
            }
        }
        
        public void EnqueueCardId(string cardId)
        {
            _idList.Add(cardId);
        }

        public void RemoveCardIds(IEnumerable<string> cardIds)
        {
            foreach (var cardId in cardIds)
            {
                _idList.Remove(cardId);
            }
        }

        public void Shuffle(int? seed = null)
        {
            _idList.Shuffle(seed);
        }

        #region inherits of IReadOnlyList

        public int Count => _idList.Count;

        public string this[int index] => _idList[index];

        public IEnumerator<string> GetEnumerator() => _idList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _idList.GetEnumerator();

        #endregion
    }
}
