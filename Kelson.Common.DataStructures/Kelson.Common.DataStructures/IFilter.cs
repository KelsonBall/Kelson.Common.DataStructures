using Kelson.Common.DataStructures.Sets;
using Kelson.Common.DataStructures.Text;
using System;
using System.Collections.Generic;

namespace Kelson.Common.DataStructures
{
    public interface IFilter<TItem, TArg>
    {
        event Action<UintSet, UintSet> OnFilterChanged;
        IImmutableIndexSet<TItem> Values { get; }
        IImmutableIndexSet<TItem> Push(TArg constraint);
        IImmutableIndexSet<TItem> Pop(out TArg constraint);
        bool Evaluate(TItem item);
    }

    public class SubstringFilter<T> : IFilter<T, char>
    {
        private readonly IObservableDictionary<int, T> values;
        private readonly Catalog<T> catalog;
        private readonly Stack<(UintSet values, char constraint)> stack = new Stack<(UintSet values, char constraint)>();
        private readonly UintSet included = new UintSet();

        internal SubstringFilter(IObservableDictionary<int, T> values, Func<T, string> textSelector)
        {
            this.values = values;
            catalog = new Catalog<T>(values.KeyOf, textSelector);
            foreach (var kvp in values)
                catalog.Add(kvp.Value);
            values.OnCleared += () => catalog.Clear();
            values.OnItemAdded += (key, item) => catalog.Add(item);
            values.OnItemRemoved += (key, item) => catalog.Remove(item);
            values.OnItemUpdated += (key, previous, added) =>
            {
                catalog.Remove(previous);
                catalog.Add(added);
            };
        }

        public UintSet Values => throw new System.NotImplementedException();

        public event Action<UintSet, UintSet> OnFilterChanged;

        public bool Evaluate(T item)
        {
            if (stack.Count == 0)
                return true;
            else
                return included.Contains((uint)values.KeyOf(item));
        }

        public UintSet Pop(out char constraint)
        {
            var (values, c) = stack.Pop();
            constraint = c;
            return values;
        }

        public UintSet Push(char constraint)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PrefixFilter<T> : IFilter<T, char>
    {
        private readonly Trie<T> trie;

        internal PrefixFilter(IObservableDictionary<int,T> values, Func<T, string> textSelector)
        {
            trie = new Trie<T>();
            foreach (var kvp in values)
                trie.Add(textSelector(kvp.Value), kvp.Value);
            values.OnCleared += () => trie.Clear();
            values.OnItemAdded += (key, value) => trie.Add(textSelector(value), value);
            values.OnItemRemoved += (key, value) => trie.Remove(textSelector(value));
            values.OnItemUpdated += (key, previous, added) =>
            {
                trie.Remove(textSelector(previous));
                trie.Add(textSelector(added), added);
            };
        }

        public UintSet Values => throw new System.NotImplementedException();

        public event Action<UintSet, UintSet> OnFilterChanged;

        public bool Evaluate(T item)
        {
            throw new NotImplementedException();
        }

        public UintSet Pop(out char constraint)
        {
            throw new System.NotImplementedException();
        }

        public UintSet Push(char constraint)
        {
            throw new System.NotImplementedException();
        }
    }

    public class RangeFilter<T, TComparable> : IFilter<T, (TComparable min, TComparable max)> where TComparable : IComparable<TComparable>
    {
        public RangeFilter(IObservableDictionary<int, T> values, Func<T, TComparable> comparableSelector)
        {

        }

        public UintSet Values => throw new NotImplementedException();

        public event Action<UintSet, UintSet> OnFilterChanged;

        public bool Evaluate(T item)
        {
            throw new NotImplementedException();
        }

        public UintSet Pop(out (TComparable min, TComparable max) constraint)
        {
            throw new NotImplementedException();
        }

        public UintSet Push((TComparable min, TComparable max) constraint)
        {
            throw new NotImplementedException();
        }
    }
}
