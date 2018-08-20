using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kelson.Common.DataStructures.Sets;

namespace Kelson.Common.DataStructures.Text
{
    /// <summary>
    /// Stores character locations in SIMD sets for fast substring search
    /// </summary>
    public class SubstringCollection
    {
        private readonly SortedList<char, UintSet> chars = new SortedList<char, UintSet>();

        public readonly bool CaseSensative;        

        public SubstringCollection(IEnumerable<char> value)
        {
            uint i = 0;
            foreach (var c in value)
            {                
                if (!chars.ContainsKey(c))
                    chars[c] = new UintSet();
                chars[c] = chars[c].Add(i++);
            }
        }

        public bool Contains(IEnumerable<char> sequence) => Occurances(sequence).Any();

        public int Count(IEnumerable<char> sequence) => Occurances(sequence).Count;

        public UintSet Occurances(IEnumerable<char> sequence)
        {
            if (!sequence.Any())
                return new UintSet();
            if (!chars.ContainsKey(sequence.First()))
                return new UintSet();
            UintSet locations = chars[sequence.First()];
            int i = 1;
            foreach (var c in sequence.Skip(1))
            {                
                if (!chars.ContainsKey(c))
                    return new UintSet();
                var next = chars[c];
                locations = locations.Intersect(next << i++);
                if (locations.Count == 0)
                    break;                
            }
            return locations;
        }

        public UintSet Occurances(IEnumerable<char> sequence, UintSet starts)
        {
            if (!sequence.Any())
                return starts;
            UintSet locations = starts;
            int i = 0;
            foreach (var c in sequence)
            {
                if (!chars.ContainsKey(c))
                    return new UintSet();
                var next = chars[c];
                locations = locations.Intersect(next << i++);
                if (locations.Count == 0)
                    break;
            }
            return locations;
        }

        public bool SourceEquals(IEnumerable<char> other)
        {
            uint i = 0;
            foreach (var c in other)            
                if (!chars[c].Contains(i++))
                    return false;
            return true;
        }
    }

    public class Catalog<T>
    {
        private readonly IDictionary<int, T> data;
        private readonly IDictionary<int, Task<SubstringCollection>> records;

        private readonly Func<T, int> id;
        private readonly Func<T, string> text;

        public Catalog(IDictionary<int, T> items, Func<T, string> textSelector)
            => (data, text, records) = (items, textSelector, items.ToDictionary(kvp => kvp.Key, kvp => Task.Run(() => new SubstringCollection(textSelector(kvp.Value)))));

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
    }
}
