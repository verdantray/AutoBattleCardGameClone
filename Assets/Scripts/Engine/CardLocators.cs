using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class CardOnboardLocator : ICardLocator<CardOnboard>
    {
        public ICardHolder<CardOnboard> Deck => _deck;
        public ICardHolder<CardOnboard> Field => _field;
        public ICardHolder<string, CardOnboard> Infirmary =>  _infirmary;

        private readonly CardHolder<CardOnboard> _deck = new CardHolder<CardOnboard>();
        private readonly CardHolder<CardOnboard> _field = new CardHolder<CardOnboard>();
        private readonly CardHolder<string, CardOnboard> _infirmary = new CardHolder<string, CardOnboard>();
    }

    public sealed class CardReferenceLocator : ICardLocator<CardReference>
    {
        public ICardHolder<CardReference> Deck => _deck;
        public ICardHolder<CardReference> Field => _field;
        public ICardHolder<string, CardReference> Infirmary => _infirmary;

        private readonly CardHolder<CardReference> _deck = new CardHolder<CardReference>();
        private readonly CardHolder<CardReference> _field = new CardHolder<CardReference>();
        private readonly CardHolder<string, CardReference> _infirmary = new CardHolder<string, CardReference>();
    }
    
    public sealed class CardHolder<T> : ICardHolder<T>, IEnumerable<T> where T : class
    {
        private readonly List<T> _cardObjects = new List<T>();
        
        public int Count => _cardObjects.Count;
        
        public T Peek(int index)
        {
            if (index < 0 || index >= _cardObjects.Count)
            {
                Debug.LogWarning($"{nameof(CardHolder<T>)} : try access invalid index '{index}' / count : {Count}");
            }
            
            return _cardObjects[index];
        }

        public T Pop(int index)
        {
            T toPop = Peek(index);
            _cardObjects.RemoveAt(index);
            
            return toPop;
        }

        public void Insert(int index, T element)
        {
            if (index < 0 || index > _cardObjects.Count)
            {
                Debug.LogWarning($"{nameof(CardHolder<T>)} : try insert with invalid index '{index}' /  count : {Count}");
            }
            
            _cardObjects.Insert(index, element);
        }

        public void Clear()
        {
            _cardObjects.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _cardObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cardObjects.GetEnumerator();
        }
    }

    public sealed class CardHolder<TKey, T> : ICardHolder<TKey, T>, IReadOnlyDictionary<TKey, CardHolder<T>>
        where TKey : IComparable
        where T : class
    {
        private readonly List<TKey> _keyList = new List<TKey>();
        private readonly Dictionary<TKey, CardHolder<T>> _cardObjects = new Dictionary<TKey, CardHolder<T>>();

        ICardHolder<T> ICardHolder<TKey, T>.this[int index] => _cardObjects[_keyList[index]];

        ICardHolder<T> ICardHolder<TKey, T>.this[TKey key] => _cardObjects[key];

        public int IndexOfKey(TKey key)
        {
            return _keyList.IndexOf(key);
        }

        public T Pop(TKey key, int index)
        {
            if (!_cardObjects.ContainsKey(key))
            {
                Debug.LogWarning($"{nameof(CardHolder<TKey, T>)} : can't find key '{key}'");
            }
            
            return _cardObjects.TryGetValue(key, out var cardHolder)
                ? cardHolder.Pop(index)
                : null;
        }

        public void Insert(TKey key, int index, T element)
        {
            if (_keyList.Contains(key))
            {
                _cardObjects[key].Insert(index, element);
            }
            else
            {
                if (index != 0)
                {
                    Debug.LogWarning($"{nameof(CardHolder<TKey, T>)} : key '{key}' not contains yet, but index is not 0 '{index}'");
                }

                var innerHolder = new CardHolder<T>();
                innerHolder.Insert(index, element);
                
                _cardObjects.Add(key, innerHolder);
                _keyList.Add(key);
            }
        }

        public void Clear()
        {
            foreach (var innerHolder in _cardObjects.Values)
            {
                innerHolder.Clear();
            }
            
            _cardObjects.Clear();
            _keyList.Clear();
        }

        #region inherit of IReadonlyDictionary

        public IEnumerator<KeyValuePair<TKey, CardHolder<T>>> GetEnumerator() => _cardObjects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _cardObjects.GetEnumerator();

        public int Count => _cardObjects.Count;
        public bool ContainsKey(TKey key) => _cardObjects.ContainsKey(key);

        public bool TryGetValue(TKey key, out CardHolder<T> value) => _cardObjects.TryGetValue(key, out value);

        public CardHolder<T> this[TKey key] => _cardObjects[key];

        public IEnumerable<TKey> Keys => _keyList;
        public IEnumerable<CardHolder<T>> Values => _cardObjects.Values;

        #endregion
    }
}
