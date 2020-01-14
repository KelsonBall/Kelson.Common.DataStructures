using System;
using System.Collections.Generic;
using System.Linq;
using Kelson.Common.DataStructures.Sets;

namespace Kelson.Common.DataStructures.Text
{
    /// <summary>
    /// Stores character locations in SIMD index sets for fast substring search
    /// </summary>
    public class SubstringCollection
    {
        private readonly SortedList<char, UintSet> chars = new SortedList<char, UintSet>();

        public readonly bool CaseSensative;        

        public SubstringCollection(IEnumerable<char> value, bool caseSensitive = false)
        {
            CaseSensative = caseSensitive;
            uint i = 0;
            foreach (var character in value)
            {
                char c = character;
                if (!CaseSensative && char.IsLetter(c))
                    c = char.ToUpperInvariant(c);
                if (!chars.ContainsKey(c))
                    chars[c] = new UintSet();
                // record existence at index of char c
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
}
