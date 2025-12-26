using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class CardLocator<T> : ICardLocator<T> where T : class
    {
        private readonly Dictionary<IPlayer, SideLocator<T>> _sideLocators = new Dictionary<IPlayer, SideLocator<T>>();


        public ISideLocator<T> this[IPlayer player] => GetSideLocator(player);

        public bool Contains(IPlayer player)
        {
            return  _sideLocators.ContainsKey(player);
        }

        public ISideLocator<T> GetSideLocator(IPlayer player)
        {
            return _sideLocators[player];
        }

        public void RegisterSideLocator(IPlayer player, IEnumerable<T> deckCards = null)
        {
            _sideLocators.Add(player, deckCards == null ? new SideLocator<T>() : new SideLocator<T>(deckCards));
        }

        public void Clear()
        {
            foreach (var sideLocator in _sideLocators.Values)
            {
                sideLocator.Clear();
            }
            
            _sideLocators.Clear();
        }
    }

    public sealed class SideLocator<T> : ISideLocator<T> where T : class
    {
        public ICardHolder<T> Deck { get; }
        public ICardHolder<T> Field { get; } = new CardHolder<T>();
        public ICardHolder<string, T> Infirmary { get; } = new CardHolder<string, T>();

        public SideLocator()
        {
            Deck = new CardHolder<T>();
        }

        public SideLocator(IEnumerable<T> deckCards)
        {
            Deck = new CardHolder<T>(deckCards);
        }

        public void Clear()
        {
            Deck.Clear();
            Field.Clear();
            Infirmary.Clear();
        }
    }
    
    public sealed class CardHolder<T> : ICardHolder<T>, IEnumerable<T> where T : class
    {
        private readonly List<T> _cardObjects = new List<T>();
        
        public int Count => _cardObjects.Count;
        
        public CardHolder()
        {
            
        }

        public CardHolder(IEnumerable<T> initialCards)
        {
            _cardObjects.AddRange(initialCards);
        }
        
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
