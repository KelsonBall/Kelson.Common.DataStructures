using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.DataStructures
{
    public interface IObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        event Action OnCleared;
        event Action<TKey, TValue> OnItemRemoved;
        event Action<TKey, TValue, TValue> OnItemUpdated;
        event Action<TKey, TValue> OnItemAdded;
        TKey KeyOf(TValue value);
    }

    public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue> where TKey : struct
    {
        private readonly SortedList<TKey, TValue> backingStore = new SortedList<TKey, TValue>();

        public event Action OnCleared;
        public event Action<TKey, TValue> OnItemRemoved;
        public event Action<TKey, TValue, TValue> OnItemUpdated;
        public event Action<TKey, TValue> OnItemAdded;

        private readonly Func<TValue, TKey> keySelector;

        public TKey KeyOf(TValue value) => keySelector(value);

        public ObservableDictionary(Func<TValue, TKey> keySelector)
        {
            this.keySelector = keySelector;
        }        

        public TValue this[TKey key]
        {
            get => backingStore[key];
            set
            {
                var previous = backingStore[key];
                backingStore[key] = value;
                if (backingStore.ContainsKey(key))
                    OnItemUpdated?.Invoke(key, previous, value);
                else if (value != null)
                    OnItemAdded?.Invoke(key, value);
                else
                    OnItemRemoved?.Invoke(key, previous);
            }
        }

        public ICollection<TKey> Keys => backingStore.Keys;

        public ICollection<TValue> Values => backingStore.Values;

        public int Count => backingStore.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            backingStore.Add(key, value);
            OnItemAdded?.Invoke(key, value);            
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            backingStore.Add(item.Key, item.Value);
            OnItemAdded?.Invoke(item.Key, item.Value);
        }

        public void Clear()
        {            
            backingStore.Clear();
            OnCleared?.Invoke();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => backingStore.Contains(item);

        public bool ContainsKey(TKey key) => backingStore.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var kvp in backingStore)
                array[arrayIndex++] = kvp;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => backingStore.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(TKey key)
        {
            var value = backingStore[key];
            var removed = backingStore.Remove(key);
            if (removed)
                OnItemRemoved?.Invoke(key, value);
            return removed;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (backingStore.Contains(item))
            {
                backingStore.Remove(item.Key);
                OnItemRemoved?.Invoke(item.Key, item.Value);
                return true;
            }
            else
                return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => backingStore.TryGetValue(key, out value);      
    }
}
