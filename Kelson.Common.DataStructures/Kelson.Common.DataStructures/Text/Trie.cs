using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kelson.Common.Trees;

namespace Kelson.Common.DataStructures.Text
{
    public class Trie : IEnumerable<string>
    {
        public int Count { get; protected set; }

        private protected IEnumerable<TrieNode> prefixes() => nodes.SelectMany(n => n.Value.Prefixes());
        private protected IEnumerable<TrieNode> prefixes(string prefix) => string.IsNullOrEmpty(prefix) ? Enumerable.Empty<TrieNode>() : nodes[prefix[0]].Prefixes(prefix);
        public IEnumerable<string> Prefixes() => prefixes().Select(v => v.Prefix);
        public IEnumerable<string> Prefixes(string prefix) => string.IsNullOrEmpty(prefix) ? Enumerable.Empty<string>() : prefixes(prefix).Select(v => v.Prefix);
        private protected IEnumerable<TrieNode> values() => nodes.SelectMany(n => n.Value.Values());
        private protected IEnumerable<TrieNode> values(string key) => string.IsNullOrEmpty(key) ? Enumerable.Empty<TrieNode>() : nodes[key[0]].Values(key);
        public IEnumerable<string> Values() => values().Select(v => v.Prefix);
        public IEnumerable<string> Values(string key) => this[key];

        public bool IsReadOnly => false;

        public IEnumerable<string> this[string key]
        {
            get => string.IsNullOrEmpty(key) ? Enumerable.Empty<string>() : values(key).Select(v => v.Prefix);
        }

        public void Add(string key)
        {
            if (!validate(key))
                return;
            if (!nodes.ContainsKey(key[0]))
            {
                nodes[key[0]] = new TrieNode(key, 0);
                Count++;
            }
            else
            {
                if (nodes[key[0]].Append(key, 0))
                    Count++;
            }
            return;
        }

        public void Remove(string key)
        {
            if (nodes.ContainsKey(key[0]))            
                if (nodes[key[0]].Remove(key, 0).decrementTerminals)
                    Count--;            
        }

        public IEnumerator<string> GetEnumerator() => Values().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private protected readonly SortedList<char, TrieNode> nodes = new SortedList<char, TrieNode>();
        protected readonly Predicate<string> validate;

        public Trie(Predicate<string> validation = null)
        {
            validate = validation ?? (key => true);
        }

        internal class TrieNode
        {
            public readonly int Index;
            public readonly char C;
            public readonly string Source;
            public readonly int PrefixLength;
            public string Prefix => Source.Substring(0, PrefixLength);
            public readonly SortedList<char, TrieNode> Children;
            public object Payload;
            public bool IsTerminal;
            public int TerminalDescendents;

            internal TrieNode(string data, int index, object payload = null)
            {
                Index = index;
                C = data[index];
                Source = data;
                PrefixLength = index + 1;
                Children = new SortedList<char, TrieNode>();
                if (index < data.Length - 1)
                {
                    Children.Add(data[index + 1], new TrieNode(data, index + 1, payload));
                    TerminalDescendents = 1;
                    Payload = null;
                    IsTerminal = false;
                }
                else
                {
                    TerminalDescendents = 0;
                    IsTerminal = true;
                    Payload = payload;
                }
            }

            /// <summary>
            /// Appends a string to the Trie, returning true if a new terminal descendent has been added to the parent node
            /// </summary>
            public bool Append(string data, int index, object payload = null)
            {
                if (index == data.Length - 1)
                {
                    if (Payload != null && payload != null)
                        throw new ArgumentException("An item with the same key has already been added");
                    Payload = payload;
                    return IsTerminal != (IsTerminal = true);
                }
                else
                {
                    if (Children.ContainsKey(data[index + 1]))
                    {
                        var add = Children[data[index + 1]].Append(data, index + 1, payload);
                        if (add)
                            TerminalDescendents++;
                        return add;
                    }
                    else
                    {
                        Children.Add(data[index + 1], new TrieNode(data, index + 1, payload));
                        TerminalDescendents++;
                        return true;
                    }
                }
            }

            public (bool decrementTerminals, bool remove) Remove(string data, int index)
            {
                if (index == data.Length - 1)
                {
                    if (IsTerminal)
                    {
                        IsTerminal = false;
                        Payload = null;
                        if (Children.Count == 0)
                            return (true, true);
                        else
                            return (true, false);
                    }
                    else                    
                        return (false, false);                    
                }
                else
                {
                    var (decrementTerminals, remove) = Children[data[index + 1]].Remove(data, index + 1);
                    if (decrementTerminals)
                    {
                        TerminalDescendents--;
                        if (remove)
                            Children.Remove(data[index + 1]);
                        return (true, false);
                    }                    
                    else
                        return (false, false);                    
                }
            }

            private bool TraversePrefix(string prefix, out TrieNode node, int prefixIndex = 0)
            {
                node = this;
                prefixIndex++;
                while (prefixIndex < prefix.Length)
                {
                    if (node.Children.ContainsKey(prefix[prefixIndex]))
                        node = node.Children[prefix[prefixIndex]];
                    else
                        return false;
                    prefixIndex++;
                }
                return true;
            }

            public IEnumerable<TrieNode> Values() => this.DepthFirstTraverse(node => node.Children.Values, node => node.IsTerminal);

            public IEnumerable<TrieNode> Values(string prefix, int prefixIndex = 0) => TraversePrefix(prefix, out TrieNode node, prefixIndex) ? node.Values() : Enumerable.Empty<TrieNode>();

            private static bool isUniquePrefix(TrieNode node) => node.TerminalDescendents == 1 || !node.Children.Any(c => c.Value.TerminalDescendents > 0);
            private static readonly List<TrieNode> noNodes = new List<TrieNode>();

            public IEnumerable<TrieNode> Prefixes() => this.DepthFirstTraverse(node => isUniquePrefix(node) ? noNodes : node.Children.Values, isUniquePrefix);

            public IEnumerable<TrieNode> Prefixes(string prefix, int index = 0) => TraversePrefix(prefix, out TrieNode node, index) ? node.Prefixes() : Enumerable.Empty<TrieNode>();

            public override string ToString()
            {
                string children = Children.Any() ? $"({string.Join(",", Children.Values)})" : "";
                string name = IsTerminal ? $"{TerminalDescendents}[{C}]" : $"{TerminalDescendents} {C}";
                return $" {name} {children}";
            }
        }
    }
}
