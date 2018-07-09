using System;
using System.Collections.Generic;
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
    public class Catalog
    {
        private readonly SortedList<char, IntegerSet> chars = new SortedList<char, IntegerSet>();

        public readonly bool CaseSensative;
        private readonly string text;

        public Catalog(string value, bool caseSensative = true)
        {
            if (value.Length > int.MaxValue)
                throw new ArgumentException("Too much text");
            text = value;
            for (int i = 0; i < text.Length; i++)
            {
                var c = value[i];
                if (!chars.ContainsKey(c))
                    chars.Add(c, new IntegerSet(0, value.Length));
                chars[c].Add(i);
            }
        }

        public bool Contains(string sequence) => Occurances(sequence).Any();

        public int Count(string sequence) => Occurances(sequence).Count;

        public IntegerSet Occurances(string sequence)
        {
            if (string.IsNullOrEmpty(sequence))
                return new IntegerSet(0, text.Length);
            IntegerSet values = chars[sequence[0]];
            //starts.AddRange()
            for (int i = 1; i < sequence.Length; i++)
            {
                var next = chars[sequence[i]];
                values.IntersectWith(next << i);
                if (values.Count == 0)
                    break;
            }
            return values;
        }
    }
}
