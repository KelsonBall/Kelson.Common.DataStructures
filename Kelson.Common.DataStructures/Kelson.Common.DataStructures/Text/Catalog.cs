using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kelson.Common.DataStructures.Sets;

namespace Kelson.Common.DataStructures.Text
{
    /// <summary>
    /// Stores case insensiteve location info for characters of simple english text
    /// Complexity: Roughly O(l*k)
    ///     l: length of sequence
    ///     k: ~number of times the sequence occures (including partialy)
    /// Worse case scenario: long string of the same character, and a sequence that is the same string
    ///     Example text:    aaaaaaaaaaaaaaaaaaaaaa
    ///     Example sequnce: aaaaaaaaaaaaaaaaaaaaaa
    ///     Contains partial sequences,
    ///         a, aa, aaa, aaaa, aaaaa, aaaaaa, and so on... (large k!)
    /// </summary>
    public class SubstringCollection
    {
        private readonly SortedList<char, UintSet> chars = new SortedList<char, UintSet>();

        public readonly bool CaseSensative;        

        public SubstringCollection(ReadOnlySpan<char> value, bool caseSensative = true)
        {
            if (value.Length > short.MaxValue)
                throw new ArgumentException("Whoa nelly!");            
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!chars.ContainsKey(c))
                    chars[c] = new UintSet();
                chars[c] = chars[c].Add((uint)i);
            }
        }

        public bool Contains(ReadOnlySpan<char> sequence) => Occurances(sequence).Any();

        public int Count(ReadOnlySpan<char> sequence) => Occurances(sequence).Count;

        public IImmutableSet<uint> Occurances(ReadOnlySpan<char> sequence)
        {
            if (sequence.Length == 0)
                return new UintSet();
            UintSet locations = chars[sequence[0]];            
            for (int i = 1; i < sequence.Length; i++)
            {
                if (!chars.ContainsKey(sequence[i]))
                    return new UintSet();
                var next = chars[sequence[i]];
                locations = locations.Intersect(next << i);
                if (locations.Count == 0)
                    break;
            }
            return locations;
        }

        public bool SourceEquals(ReadOnlySpan<char> other)
        {
            for (int i = 0; i < other.Length; i++)
                if (!chars[other[i]].Contains((uint)i))
                    return false;
            return true;
        }
    }

    //public class Catalog<T>
    //{
    //    private readonly List<(T item, SubstringCollection keys)> data = new List<(T item, SubstringCollection keys)>();        

    //    public IEnumerable<T> AllContaining(ReadOnlySpan<char> sequence)
    //    {
    //        foreach (var kvp in data)
    //            if (kvp.keys.Contains(sequence))
    //                yield return kvp.item;
    //    }
        
    //    public T this[ReadOnlySpan<char> key]
    //    {
    //        get
    //        {
    //            foreach (var (item, keys) in data)
    //                if (keys.SourceEquals(key))
    //                    return item;
    //            return default;
    //        }

    //        set
    //        {
    //            if (value == null)
    //            {
    //                for (int i = data.Count - 1; i >= 0; i++)
    //                    if (data[i].keys.SourceEquals(key))
    //                        data.RemoveAt(i);
    //            }
    //            else
    //                data.Add((value, new SubstringCollection(key)));
    //        }
    //    }        
    //}
}
