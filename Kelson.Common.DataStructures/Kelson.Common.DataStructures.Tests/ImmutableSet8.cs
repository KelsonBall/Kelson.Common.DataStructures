using FluentAssertions;
using Kelson.Common.DataStructures.Sets;
using System;
using Xunit;
using System.Linq;
using System.Collections.Immutable;

namespace Kelson.Common.DataStructures.Tests
{
    public class ImmutableSet8_Should
    {
        [Fact]
        public void ContainItems()
        {
            var set = new ImmutableSet8().Add(1, 3, 6);
            for (int i = 0; i < 8; i++)
                set.Contains(i).Should().Be(i == 1 | i == 3 | i == 6);
        }

        [Fact]
        public void BeImmutable()
        {
            var seta = new ImmutableSet8().Add(3, 7);
            var setb = seta.Add(5);
            seta.Contains(5).Should().BeFalse();
            setb.Contains(5).Should().BeTrue();
            setb.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void BeClearable()
        {
            var set = new ImmutableSet8().Add(1);
            set = set.Clear();
            set.Contains(1).Should().BeFalse();
            set.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void ShiftValues()
        {
            var set = new ImmutableSet8().Add(1);
            set.Shift(1)
               .Single()
               .Should()
               .Be(2);
            set.Shift(-1)
               .Single()
               .Should()
               .Be(0);
            set.Shift(-2)
               .IsEmpty
               .Should()
               .BeTrue();

            set = new ImmutableSet8().Add(0, 1, 2, 3, 7);
            set.Shift(-2)
               .SequenceEqual(new int[] { 0, 1, 5 })
               .Should()
               .BeTrue();
            set.Shift(2)
               .SequenceEqual(new int[] { 2, 3, 4, 5 })
               .Should()
               .BeTrue();
        }

        [Fact]
        public void ShiftAndFillValues()
        {
            var seta = new ImmutableSet8().Add(0);
            var setb = new ImmutableSet8().Add(7);

            seta.Shift(1, setb)
                .SequenceEqual(new int[] { 0, 1 })
                .Should().BeTrue();

            seta = new ImmutableSet8().Add(7);
            setb = new ImmutableSet8().Add(1, 7);

            seta.Shift(-2, setb)
                .SequenceEqual(new int[] { 5, 7 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ExcludeSets()
        {
            var seta = new ImmutableSet8().Add(1, 2, 4, 7);
            var setb = new ImmutableSet8().Add(1, 4, 5);
            seta.Except(setb)
                .SequenceEqual(new int[] { 2, 7 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ExcludeEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet8().Add(1, 2, 4 );
            var setb = Enumerable.Range(1, 3);
            seta.Except(setb)
                .SequenceEqual(new int[] { 4 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineIntersectionWithSet()
        {
            var seta = new ImmutableSet8().Add(1, 2, 4 );
            var setb = new ImmutableSet8().Add(1, 2, 3, 5);
            seta.Intersect(setb)
                .SequenceEqual(new int[] { 1, 2 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineIntersectionWithEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet8().Add(1, 2, 4 );
            var setb = Enumerable.Range(1, 3);
            seta.Intersect(setb)
                .SequenceEqual(new int[] { 1, 2 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineProperSubsetOfSet()
        {
            var seta = new ImmutableSet8().Add(0, 1, 3, 7);
            var setb = new ImmutableSet8().Add(0, 1, 2, 3, 4);
            var setc = new ImmutableSet8().Add(0, 1, 3);

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
            IImmutableSet<int> setb = new ImmutableSet8().Add(1, 2, 3, 4, 5);
            IImmutableSet<int> setc = new ImmutableSet8().Add(1, 2, 4);

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
            var seta = new ImmutableSet8().Add(1, 2, 4, 7);
            var setb = new ImmutableSet8().Add(1, 2, 3, 4, 5);
            var setc = new ImmutableSet8().Add(1, 2, 4);

            // proper superset should contain all values in subset
            seta.IsProperSupersetOf(setb).Should().BeFalse();

            // proper superset should contain some values not in subset
            seta.IsProperSupersetOf(seta).Should().BeFalse();

            seta.IsProperSupersetOf(setc).Should().BeTrue();
        }

        [Fact]
        public void DetermineProperSupersetOfEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet8().Add(1, 2, 4, 7);
            var setb = Enumerable.Range(1, 5);
            var setc = new int[] { 1, 2, 4 };

            // proper superset should contain all values in subset
            seta.IsProperSupersetOf(setb).Should().BeFalse();

            // proper superset should contain some values not in subset
            seta.IsProperSupersetOf(new int[] { 1, 2, 4, 7}).Should().BeFalse();

            seta.IsProperSupersetOf(setc).Should().BeTrue();
        }

        [Fact]
        public void DetermineOverlapWithSet()
        {
            var seta = new ImmutableSet8().Add(1, 2, 3);
            var setb = new ImmutableSet8().Add(3, 4, 5);
            var setc = new ImmutableSet8().Add(5, 6, 7);

            seta.Overlaps(setb).Should().BeTrue();
            seta.Overlaps(setc).Should().BeFalse();
            setb.Overlaps(setc).Should().BeTrue();
        }

        [Fact]
        public void DetermineOverlapWithEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet8().Add(1, 2, 3);
            var setb = Enumerable.Range(3, 3);
            var setc = Enumerable.Range(5, 3);

            seta.Overlaps(setb).Should().BeTrue();
            seta.Overlaps(setc).Should().BeFalse();
        }

        [Fact]
        public void DetermineUnionWithSet()
        {
            var seta = new ImmutableSet8().Add(1, 2, 3);
            var setb = new ImmutableSet8().Add(3, 4, 5);
            seta.Union(setb).SequenceEqual(Enumerable.Range(1, 5)).Should().BeTrue();
        }

        [Fact]
        public void DetermineUnionWithEnumerable()
        {
            IImmutableSet<int> seta = new ImmutableSet8().Add(1, 2, 3);
            var setb = Enumerable.Range(3, 3);
            seta.Union(setb).SequenceEqual(Enumerable.Range(1, 5)).Should().BeTrue();
        }

        [Fact]
        public void RemoveItems()
        {
            var set = new ImmutableSet8().Add(1, 2, 3);
            var removed = set.Remove(2);

            set.Contains(2).Should().BeTrue();
            removed.Contains(2).Should().BeFalse();
        }

        [Fact]
        public void DetermineEquality()
        {
            var set = new ImmutableSet8().Add(1, 2, 3);
            set.SetEquals(new ImmutableSet8().Add(1, 2, 3)).Should().BeTrue();
            set.Equals(new ImmutableSet8().Add(1, 2, 3)).Should().BeTrue();
            set.Equals(14).Should().BeTrue();
        }

        [Fact]
        public void DetermineEqualityWithEnumerable()
        {
            IImmutableSet<int> set = new ImmutableSet8().Add(1, 2, 3);
            set.SetEquals(Enumerable.Range(1, 3)).Should().BeTrue();
        }

        [Fact]
        public void TrackCount()
        {
            var seta = new ImmutableSet8().Add(0, 1, 2, 3, 4, 5, 6, 7);
            seta.Count.Should().Be(8);
            seta = seta.Intersect(new ImmutableSet8().Add(0, 1, 2, 4));
            seta.Count.Should().Be(4);
        }
    }
}
