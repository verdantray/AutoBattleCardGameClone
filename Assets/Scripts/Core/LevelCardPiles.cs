using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.Core
{
    public class LevelCardPiles : IReadOnlyDictionary<LevelType, ConcurrentCardPile>
    {
        private readonly Dictionary<LevelType, ConcurrentCardPile> levelCardPiles = new Dictionary<LevelType, ConcurrentCardPile>
        {
            { LevelType.A, new ConcurrentCardPile() },
            { LevelType.B, new ConcurrentCardPile() },
            { LevelType.C, new ConcurrentCardPile() },
        };

        #region inheritances of IDictionary

        public IEnumerator<KeyValuePair<LevelType, ConcurrentCardPile>> GetEnumerator() => levelCardPiles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => levelCardPiles.GetEnumerator();

        public int Count => levelCardPiles.Count;
        public bool ContainsKey(LevelType key) => levelCardPiles.ContainsKey(key);

        public bool TryGetValue(LevelType key, out ConcurrentCardPile value) => levelCardPiles.TryGetValue(key, out value);

        public ConcurrentCardPile this[LevelType key] => levelCardPiles[key];

        public IEnumerable<LevelType> Keys => levelCardPiles.Keys;
        public IEnumerable<ConcurrentCardPile> Values => levelCardPiles.Values;

        #endregion
    }

    public class ConcurrentCardPile
    {
        private readonly List<Card> cardList = new List<Card>();
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);

        public async Task AddRangeAsync(IEnumerable<Card> cards, CancellationToken token = default)
        {
            await gate.WaitAsync(token);

            try
            {
                cardList.AddRange(cards);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<List<Card>> DrawCardsAsync(int count, CancellationToken token = default)
        {
            await gate.WaitAsync(token);
            
            try
            {
                List<Card> drawCards = new List<Card>();
                
                if (count < 0)
                {
                    Debug.LogWarning($"{nameof(ConcurrentCardPile)} : Trying to draw empty cards");
                    return drawCards;
                }

                if (cardList.Count < count)
                {
                    throw new InvalidOperationException($"Trying to draw {count} cards, but remains {cardList.Count} only...");
                }

                drawCards.AddRange(cardList.GetRange(0, count));
                cardList.RemoveRange(0, count);
                
                return drawCards;
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task ShuffleAsync(int seed = 0, CancellationToken token = default)
        {
            await gate.WaitAsync(token);

            try
            {
                System.Random random = seed != 0 ?  new System.Random(seed) : new System.Random();
                List<Card> randomized = cardList.OrderBy(_ => random.Next()).ToList();
                
                cardList.Clear();
                cardList.AddRange(randomized);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<int> CountAsync(CancellationToken token = default)
        {
            await gate.WaitAsync(token);

            try
            {
                return cardList.Count;
            }
            finally
            {
                gate.Release();
            }
        }
    }
}
