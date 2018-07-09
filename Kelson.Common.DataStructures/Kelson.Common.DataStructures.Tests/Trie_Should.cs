using FluentAssertions;
using Kelson.DataStructures;
using System;
using System.Linq;
using Xunit;

namespace Kelson.Common.DataStructures.Tests
{
    public class Trie_Should
    {
        [Fact]
        public void TrackTerminalCount()
        {
            var trie = new Trie()
                .Add("alpaca")
                .Add("dog")
                .Add("dot")
                .Add("ducks")
                .Add("duck");

            trie.Count.Should().Be(5);
        }

        [Fact]
        public void TrackUniquePrefixes()
        {
            var trie = new Trie()
                .Add("alpaca")
                .Add("dog")
                .Add("dot")
                .Add("ducks")
                .Add("duck");

            var prefixes = trie.Prefixes().ToList();

            prefixes.SequenceEqual(new string[] { "a", "do", "duck" }).Should().BeTrue();
        }

        [Fact]
        public void TrackValues()
        {
            var trie = new Trie()
                .Add("alpaca")
                .Add("dog")
                .Add("dot")
                .Add("ducks")
                .Add("duck");

            var values = trie.Values().ToList();

            values.SequenceEqual(new string[] { "alpaca", "dog", "dot", "duck", "ducks" }).Should().BeTrue();
        }

        [Fact]
        public void FilterPrefixes()
        {
            var trie = new Trie()
                .Add("alpaca")
                .Add("dog")
                .Add("dot")
                .Add("ducks")
                .Add("dunes");

            var prefixes = trie.Prefixes("du").ToList();

            prefixes.SequenceEqual(new string[] { "duc", "dun" }).Should().BeTrue();
        }

        [Fact]
        public void FilterValues()
        {
            var trie = new Trie()
                .Add("alpaca")
                .Add("dog")
                .Add("dot")
                .Add("ducks")
                .Add("dunes");

            var values = trie.Values("du").ToList();

            values.SequenceEqual(new string[] { "ducks", "dunes" }).Should().BeTrue();
        }

        [Fact]
        public void FilterValues_EmptyWhenNoneWithPrefix()
        {
            var trie = new Trie()
                .Add("alpaca")
                .Add("dog")
                .Add("dot")
                .Add("ducks")
                .Add("dunes");

            var values = trie.Values("de").ToList();

            values.Should().BeEmpty();
        }
    }
}
