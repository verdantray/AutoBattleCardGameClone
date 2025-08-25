using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class GradeCardPiles : IReadOnlyDictionary<GradeType, CardPile>
    {
        private readonly Dictionary<GradeType, CardPile> _levelCardPiles = new Dictionary<GradeType, CardPile>
        {
            { GradeType.First, new CardPile() },
            { GradeType.Second, new CardPile() },
            { GradeType.Third, new CardPile() },
        };

        #region inheritances of IDictionary

        public IEnumerator<KeyValuePair<GradeType, CardPile>> GetEnumerator() => _levelCardPiles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _levelCardPiles.GetEnumerator();

        public int Count => _levelCardPiles.Count;
        public bool ContainsKey(GradeType key) => _levelCardPiles.ContainsKey(key);

        public bool TryGetValue(GradeType key, out CardPile value) => _levelCardPiles.TryGetValue(key, out value);

        public CardPile this[GradeType key] => _levelCardPiles[key];

        public IEnumerable<GradeType> Keys => _levelCardPiles.Keys;
        public IEnumerable<CardPile> Values => _levelCardPiles.Values;

        #endregion
    }

    [Obsolete("Not used anymore thread safe version of CardPile... Use CardPile instead.")]
    public class ConcurrentCardPile
    {
        private readonly List<Card> _cardList = new List<Card>();
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public async Task AddRangeAsync(IEnumerable<Card> cards, CancellationToken token = default)
        {
            await _gate.WaitAsync(token);

            try
            {
                _cardList.AddRange(cards);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<List<Card>> DrawCardsAsync(int count, CancellationToken token = default)
        {
            await _gate.WaitAsync(token);
            
            try
            {
                List<Card> drawCards = new List<Card>();
                
                if (count < 0)
                {
                    return drawCards;
                }

                if (_cardList.Count < count)
                {
                    throw new InvalidOperationException($"Trying to draw {count} cards, but remains {_cardList.Count} only...");
                }

                drawCards.AddRange(_cardList.GetRange(0, count));
                _cardList.RemoveRange(0, count);
                
                return drawCards;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task ShuffleAsync(int seed = 0, CancellationToken token = default)
        {
            await _gate.WaitAsync(token);

            try
            {
                System.Random random = seed != 0 ?  new System.Random(seed) : new System.Random();
                List<Card> randomized = _cardList.OrderBy(_ => random.Next()).ToList();
                
                _cardList.Clear();
                _cardList.AddRange(randomized);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<int> CountAsync(CancellationToken token = default)
        {
            await _gate.WaitAsync(token);

            try
            {
                return _cardList.Count;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
