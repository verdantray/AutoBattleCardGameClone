using System;
using System.Collections.Generic;

namespace ProjectABC.Core
{
    public interface ICardLocator<T> where T : class
    {
        public ISideLocator<T> this[IPlayer player] { get; }
        
        public bool Contains(IPlayer player);
        public ISideLocator<T> GetSideLocator(IPlayer player);
        public void RegisterSideLocator(IPlayer player, MatchPosition position, IEnumerable<T> deckCards = null, Action<T> clearCallback = null);
        public void Clear();
    }

    public interface ISideLocator<T> where T : class
    {
        public MatchPosition Position { get; }
        public ICardHolder<T> Deck { get; }
        public ICardHolder<T> Field { get; }
        public ICardHolder<string, T> Infirmary { get; }
        public void SetPosition(MatchPosition position);
        public void Clear();
    }

    public interface ICardHolder<T> where T : class
    {
        public int Count { get; }
        public T Peek(int index);
        public T Pop(int index);
        public void Insert(int index, T element);
        public void Change(int index, T element);
        public void Clear();
    }

    public interface ICardHolder<TKey, T> : IReadOnlyDictionary<TKey, ICardHolder<T>> where TKey : IComparable where T : class
    {
        public ICardHolder<T> this[int keyIndex] { get; }
        public int IndexOfKey(TKey key);
        public T Peek(TKey key, int index);
        public T Pop(TKey key, int index);
        public void Insert(TKey key, int index, T element);
        public void Change(TKey key, int index, T element);
        public void Clear();
    }
}
