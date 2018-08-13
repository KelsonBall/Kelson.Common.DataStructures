using FluentAssertions;
using Kelson.Common.DataStructures.Sets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Kelson.Common.DataStructures.Tests
{
    public class UintSet_Should
    {
        static readonly Random rng = new Random();

        List<uint> randomList => Enumerable.Range(0, rng.Next(100, 3000)).Select(i => (uint)rng.Next(0, 5000)).ToList();

        (UintSet test, SortedSet<uint> compare) randomSets
        {
            get
            {
                var list = randomList;
                return (new UintSet(list), new SortedSet<uint>(list));
            }
        }

        [Fact]
        public void AddItems()
        {
            var set = new UintSet().Add(0).Add(100).Add(200);
            set.Should().ContainInOrder(0, 100, 200);
        }

        [Fact]
        public void RemoveItems()
        {
            var set = new UintSet(new uint[] { 0, 100, 200 });
            set = set.Remove(100);
            set.Should().ContainInOrder(0, 200);
        }

        [Fact]
        public void Except()
        {
            var a = new uint[] { 0, 50, 100, 150, 200, 250, 300 };
            var test1 = new UintSet(a);
            var compare1 = new SortedSet<uint>(a);

            var b = new uint[] { 50, 200, 250 };
            var test2 = new UintSet(b);
            var compare2 = new SortedSet<uint>(b);

            compare1.ExceptWith(compare2);
            var expected = compare1.ToArray();
            var result = test1.Except(test2);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ExceptRandom()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = randomSets;
                var (test2, compare2) = randomSets;

                var expectedSet = new SortedSet<uint>(compare1);
                expectedSet.ExceptWith(compare2);
                var expected = expectedSet.ToArray();
                var result = test1.Except(test2);
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void SymmetricExcept()
        {
            var a = new uint[] { 0, 50, 100, 150, 200, 250, 300 };
            var test1 = new UintSet(a);
            var compare1 = new SortedSet<uint>(a);

            var b = new uint[] { 50, 200, 250 };
            var test2 = new UintSet(b);
            var compare2 = new SortedSet<uint>(b);

            compare1.SymmetricExceptWith(compare2);
            var expected = compare1.ToArray();
            var result = test1.SymmetricExcept(test2);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SymmetricExceptRandom()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = randomSets;
                var (test2, compare2) = randomSets;

                var expectedSet = new SortedSet<uint>(compare1);
                expectedSet.SymmetricExceptWith(compare2);
                var expected = expectedSet.ToArray();
                var result = test1.SymmetricExcept(test2);
                if (!result.SequenceEqual(expected))
                    Debugger.Break();
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void Intersect()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = randomSets;
                var (test2, compare2) = randomSets;

                var expectedSet = new SortedSet<uint>(compare1);
                expectedSet.IntersectWith(compare2);
                var expected = expectedSet.ToArray();
                var result = test1.Intersect(test2);
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void Union()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = randomSets;
                var (test2, compare2) = randomSets;

                var expectedSet = new SortedSet<uint>(compare1);
                expectedSet.UnionWith(compare2);
                var expected = expectedSet.ToArray();
                var result = test1.Union(test2);
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ShiftDown()
        {
            var set = new UintSet(new uint[] { 0, 64, 200 });

            set = set << 1;

            set.Should().BeEquivalentTo(new uint[] { 63, 199 });

            set = set << 50;

            set.Should().BeEquivalentTo(new uint[] { 13, 149 });
        }

        [Fact]
        public void ShiftUp()
        {
            var set = new UintSet(new uint[] { 0, 63, 200 });

            set = set >> 1;

            set.Should().BeEquivalentTo(new uint[] { 1, 64, 201 });

            set = set >> 50;

            set.Should().BeEquivalentTo(new uint[] { 51, 114, 251 });
        }
    }
}
