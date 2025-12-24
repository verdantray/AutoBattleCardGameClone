using System;

namespace ProjectABC.Core
{
    public interface ICardLocator<T> where T : class
    {
        public ICardHolder<T> Deck { get; }
        public ICardHolder<T> Field { get; }
        public ICardHolder<string, T> Infirmary { get; }
    }

    public interface ICardHolder<T> where T : class
    {
        public T Pop(int index);
        public void Insert(int index, T element);
    }

    public interface ICardHolder<in TKey, T> where TKey : IComparable where T : class
    {
        public T Pop(TKey key, int index);
        public void Insert(TKey key, int index, T element);
    }
}
