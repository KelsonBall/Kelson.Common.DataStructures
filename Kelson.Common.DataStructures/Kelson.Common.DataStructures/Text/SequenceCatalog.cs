using System;
using System.Collections.Generic;
using System.Linq;

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
    public class SequenceCatalog
    {
        const char UPPER_NORMAL = '~';
        const char LOWER_NORMAL = ' ';
        const char UPPER_WHITESPACE = '\r';
        const char LOWER_WHITESPACE = '\t';

        private readonly SortedSet<ushort>[] normalChars = new SortedSet<ushort>[UPPER_NORMAL - LOWER_NORMAL];
        private readonly SortedSet<ushort>[] whitespace = new SortedSet<ushort>[UPPER_WHITESPACE - LOWER_WHITESPACE];
        private readonly SortedSet<ushort> other = new SortedSet<ushort>();

        public readonly bool CaseSensative;
        private readonly string text;

        public SequenceCatalog(string value, bool caseSensative = true)
        {
            if (value.Length > ushort.MaxValue)
                throw new ArgumentException("Too much text");
            text = value;
            for (ushort i = 0; i < text.Length; i++)
            {
                var c = value[i];
                if (c >= LOWER_NORMAL && c <= UPPER_NORMAL)
                    (normalChars[c - LOWER_NORMAL] ?? (normalChars[c - LOWER_NORMAL] = new SortedSet<ushort>())).Add(i);
                else if (c >= LOWER_WHITESPACE && c <= UPPER_WHITESPACE)
                    (whitespace[c - LOWER_WHITESPACE] ?? (whitespace[c - LOWER_WHITESPACE] = new SortedSet<ushort>())).Add(i);
                else
                    other.Add(i);
            }
        } 

        public bool Contains(string sequence)
        {
            if (string.IsNullOrEmpty(sequence))
                return true;
            List<ushort> starts = new List<ushort>();
            
            IEnumerable<ushort> IndexesOfCharAt(int index, ushort[] after)
            {
                IEnumerable<ushort> IncrementedValues(IEnumerator<ushort> values)
                {
                    int a = 0;
                    bool hasValues = values.MoveNext();
                    while (hasValues && a < after.Length)
                    {
                        var value = values.Current;
                        if (after[a] == value + 1)
                        {
                            yield return value;
                            hasValues = values.MoveNext();
                            a++;
                        }
                        else if (after[a] > value + 1)
                            hasValues = values.MoveNext();
                        else if (after[a] < value + 1)
                            a++;
                    }
                }

                char c = sequence[index];
                if (!CaseSensative && c >= 'a' && c <= 'z')
                    c = (char)(c - (int)'a' + 'A');
                if (c >= LOWER_NORMAL && c <= UPPER_NORMAL)             
                    if (normalChars[c - LOWER_NORMAL] != null)
                        return IncrementedValues(normalChars[c - LOWER_NORMAL].GetEnumerator());
                else if (c >= LOWER_WHITESPACE && c <= UPPER_WHITESPACE)
                    if (whitespace[c - LOWER_WHITESPACE] != null)
                        return IncrementedValues(normalChars[c - LOWER_NORMAL].GetEnumerator());
                return Enumerable.Empty<ushort>();
            }

            

            //starts.AddRange()
            for (int i = 1; i < sequence.Length; i++)
            {

            }
            return true;
        }
    }
}
