using FluentAssertions;
using Kelson.Common.DataStructures.Sets;
using System;
using Xunit;
using System.Linq;
using System.Collections.Immutable;

namespace Kelson.Common.DataStructures.Tests
{
    public class Set64_Should
    {
        [Fact]
        public void ContainItems()
        {
            var set = new ImmutableSet64().Add(1, 3, 10);
            for (int i = 0; i < 64; i++)
                set.Contains(i).Should().Be(i == 1 | i == 3 | i == 10);
        }

        [Fact]
        public void BeImmutable()
        {
            var seta = new ImmutableSet64().Add(3, 12);
            var setb = seta.Add(10);
            seta.Contains(10).Should().BeFalse();
            setb.Contains(10).Should().BeTrue();
            setb.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void BeClearable()
        {
            var set = new ImmutableSet64().Add(1);
            set = set.Clear();
            set.Contains(1).Should().BeFalse();
            set.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void ExcludeSets()
        {
            var seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = new ImmutableSet64().Add(1, 2, 3, 4, 5);
            seta.Except(setb)
                .SequenceEqual(new int[] { 8, 16, 32 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ExcludeEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = Enumerable.Range(1, 5);
            seta.Except(setb)
                .SequenceEqual(new int[] { 8, 16, 32 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineIntersectionWithSet()
        {
            var seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = new ImmutableSet64().Add(1, 2, 3, 4, 5);
            seta.Intersect(setb)
                .SequenceEqual(new int[] { 1, 2, 4 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineIntersectionWithEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = Enumerable.Range(1, 5);
            seta.Intersect(setb)
                .SequenceEqual(new int[] { 1, 2, 4 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineProperSubsetOfSet()
        {
            var seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = new ImmutableSet64().Add(1, 2, 3, 4, 5);
            var setc = new ImmutableSet64().Add(1, 2, 4);

            // proper subset should not include items not in super set
            setb.IsProperSubsetOf(seta).Should().BeFalse();

            // proper subset should not contain all items in super set
            seta.IsProperSubsetOf(seta).Should().BeFalse();
            
            // proper subset should only contain values in super set
            setc.IsProperSubsetOf(seta).Should().BeTrue();
        }

        [Fact]
        public void DetermineProperSubsetOfEnumerable()
        {
            var seta = new int[] { 1, 2, 4, 8, 16, 32 };
            IImmutableSet<int> setb = new ImmutableSet64().Add(1, 2, 3, 4, 5);
            IImmutableSet<int> setc = new ImmutableSet64().Add(1, 2, 4);

            // proper subset should not include items not in super set
            setb.IsProperSubsetOf(seta).Should().BeFalse();

            // proper subset should not contain all items in super set
            setc.IsProperSubsetOf(new int[] { 1, 2, 4 }).Should().BeFalse();

            // proper subset should only contain values in super set
            setc.IsProperSubsetOf(seta).Should().BeTrue();
        }

        [Fact]
        public void DetermineProperSupersetOfSet()
        {
            var seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = new ImmutableSet64().Add(1, 2, 3, 4, 5);
            var setc = new ImmutableSet64().Add(1, 2, 4);
            
            // proper superset should contain all values in subset
            seta.IsProperSupersetOf(setb).Should().BeFalse();

            // proper superset should contain some values not in subset
            seta.IsProperSupersetOf(seta).Should().BeFalse();
            
            seta.IsProperSupersetOf(setc).Should().BeTrue();

        }

        [Fact]
        public void DetermineProperSupersetOfEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet64().Add(1, 2, 4, 8, 16, 32);
            var setb = Enumerable.Range(1, 5);
            var setc = new int[]{ 1, 2, 4 };

            // proper superset should contain all values in subset
            seta.IsProperSupersetOf(setb).Should().BeFalse();
            
            // proper superset should contain some values not in subset
            seta.IsProperSupersetOf(new int[] { 1, 2, 4, 8, 16, 32 }).Should().BeFalse();

            seta.IsProperSupersetOf(setc).Should().BeTrue();
        }

        [Fact]
        public void DetermineOverlapWithSet()
        {
            var seta = new ImmutableSet64().Add(1, 2, 3);
            var setb = new ImmutableSet64().Add(3, 4, 5);
            var setc = new ImmutableSet64().Add(5, 6, 7);

            seta.Overlaps(setb).Should().BeTrue();
            seta.Overlaps(setc).Should().BeFalse();
            setb.Overlaps(setc).Should().BeTrue();
        }

        [Fact]
        public void DetermineOverlapWithEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet64().Add(1, 2, 3);
            var setb = Enumerable.Range(3, 3);
            var setc = Enumerable.Range(5, 3);

            seta.Overlaps(setb).Should().BeTrue();
            seta.Overlaps(setc).Should().BeFalse();            
        }        

        [Fact]
        public void DetermineUnionWithSet()
        {
            var seta = new ImmutableSet64().Add(1, 2, 3);
            var setb = new ImmutableSet64().Add(3, 4, 5);
            seta.Union(setb).SequenceEqual(Enumerable.Range(1, 5)).Should().BeTrue();
        }

        [Fact]
        public void DetermineUnionWithEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet64().Add(1, 2, 3);
            var setb = Enumerable.Range(3, 3);
            seta.Union(setb).SequenceEqual(Enumerable.Range(1, 5)).Should().BeTrue();
        }

        [Fact]
        public void RemoveItems()
        {
            var set = new ImmutableSet64().Add(1, 2, 3);
            var removed = set.Remove(2);

            set.Contains(2).Should().BeTrue();
            removed.Contains(2).Should().BeFalse();
        }

        [Fact]
        public void DetermineEquality()
        {
            var set = new ImmutableSet64().Add(1, 2, 3);
            set.SetEquals(new ImmutableSet64().Add(1, 2, 3)).Should().BeTrue();
            set.Equals(new ImmutableSet64().Add(1, 2, 3)).Should().BeTrue();
            set.Equals(14ul).Should().BeTrue();
        }

        [Fact]
        public void DetermineEqualityWithEnumerable()
        {
            IImmutableSet<int> set = new ImmutableSet64().Add(1, 2, 3);
            set.SetEquals(Enumerable.Range(1, 3)).Should().BeTrue();
        }        
    }
}
