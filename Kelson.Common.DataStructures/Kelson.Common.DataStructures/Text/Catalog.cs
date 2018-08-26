using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kelson.Common.DataStructures.Sets;

namespace Kelson.Common.DataStructures.Text
{
    public class Catalog<T>
    {
        private readonly IDictionary<int, T> data;
        private readonly IDictionary<int, Task<SubstringCollection>> records;

        private readonly Func<T, int> id;
        private readonly Func<T, string> text;

        public Catalog(Func<T, int> idSelector, Func<T, string> textSelector)
            => (id, text) = (idSelector, textSelector);
            

        public async Task<UintSet> AllContaining(IEnumerable<char> sequence, UintSet included = null)
        {
            if (included == null)
            {
                UintSet results = new UintSet();
                foreach (var record in records)
                    if ((await record.Value).Contains(sequence))
                        results = results.Add((uint)record.Key);
                return results;
            }
            else
            {
                UintSet results = included;
                foreach (var id in included)
                    if (!(await records[(int)id]).Contains(sequence))
                        results = results.Remove(id);
                return results;
            }
        }

        public async Task AddRangeAsync(IEnumerable<KeyValuePair<int, T>> values)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<T> values)
        {
            throw new NotImplementedException();
        }

        public void Add(T value)
        {
            throw new NotImplementedException();
        }

        public void Remove(int key)
        {
            throw new NotImplementedException();
        }

        public void Remove(T value)
        {
            throw new NotImplementedException();
        }

        public void Update(T value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
