using System;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.DataStructures.Text
{
    public class Trie<T> : Trie
    {
        public new IEnumerable<T> Values() => values().Select(v => (T)v.Payload);
        public new IEnumerable<T> Values(string key) => this[key];

        public new IEnumerable<T> this[string key]
        {
            get => string.IsNullOrEmpty(key) ? Enumerable.Empty<T>() : values(key).Select(t => (T)t.Payload);
        }

        public Trie<T> Add(string key, T value)
        {
            if (!validate(key))
                return this;

            if (!nodes.ContainsKey(key[0]))
            {
                nodes[key[0]] = new TrieNode(key, 0, value);
                Count++;
            }
            else
            {
                if (nodes[key[0]].Append(key, 0, value))
                    Count++;
            }
            return this;
        }

        public readonly ref struct Adder
        {
            private readonly Func<T, Trie<T>> add;            

            public Adder(Func<T, Trie<T>> add) => this.add = add;

            public Trie<T> With(T value) => add(value);
        }

        public new Adder Add(string key) => new Adder(v => Add(key, v));

        public void Clear() => nodes.Clear();
        

        public Trie(Predicate<string> validation = null) : base(validation) { }
    }
}
