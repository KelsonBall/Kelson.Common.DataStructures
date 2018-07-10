using FluentAssertions;
using Kelson.Common.DataStructures.Sets;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Kelson.Common.DataStructures.Tests
{
    public class IntegerSet_Should
    {
        [Fact]
        public void ContainItems()
        {
            var set = new IntegerSet(0, 256) { 3, 112, 160, 193, 204 };
            for (int i = 0; i < 256; i++)
                if (i == 3 | i == 112 | i == 160 | i == 193 | i == 204)
                    set.Contains(i).Should().BeTrue();
                else
                    set.Contains(i).Should().BeFalse();
        }

        [Fact]
        public void CopyToRange()
        {
            for (var offset = -100; offset <= 100; offset++)
            {
                var set = new IntegerSet(offset, 128);
                set.Flip();
                var expected = new IntegerSet(0, 64);
                if (offset < 0)
                    expected.AddRange(Enumerable.Range(0, Math.Min(64, offset + 128)));
                else if (offset < 64)
                    expected.AddRange(Enumerable.Range(offset, 64 - offset));
                var result = set.CopyIntoRange(0, 64);
                result.SetEquals(expected).Should().BeTrue();
                result.Count.Should().Be(expected.Count);
            }
        }


        [Fact]
        public void EnumerateItems()
        {
            var set = new IntegerSet(0, 256) { 3, 4, 250 };

            set.SequenceEqual(new int[] { 3, 4, 250 }).Should().BeTrue();
        }

        [Fact]
        public void OffsetIndecies()
        {
            var set = new IntegerSet(-112, 5) { -112, -110 };
            for (int i = -112; i < -107; i++)
                if (i == -112 | i == -110)
                    set.Contains(i).Should().BeTrue();
                else
                    set.Contains(i).Should().BeFalse();
        }

        [Fact]
        public void DetermineExclusion_FromSetOverSameRange()
        {
            var seta = new IntegerSet(5, 100) { 12, 90, 101 };
            var setb = new IntegerSet(5, 100) { 90 };

            seta.ExceptWith(setb);
            var result = seta.ToList();
            result.SequenceEqual(new int[] { 12, 101 })
                .Should()
                .BeTrue();
        }

        [Fact]
        public void DetermineExclusion_FromSetOverDifferentRange()
        {
            var seta = new IntegerSet(5, 100) { 12, 90, 101 };
            var setb = new IntegerSet(80, 20) { 90 };

            seta.ExceptWith(setb);
            var result = seta.ToList();
            result.SequenceEqual(new int[] { 12, 101 })
                .Should()
                .BeTrue();

            seta = new IntegerSet(5, 100) { 12, 90, 101 };
            setb = new IntegerSet(30, 200) { 90, 101, 153, 229 };

            seta.ExceptWith(setb);
            seta.Single().Should().Be(12);

            seta = new IntegerSet(5, 100) { 12, 90, 101 };
            setb = new IntegerSet(-64, 200) { 12 };

            seta.ExceptWith(setb);
            result = seta.ToList();
            result.SequenceEqual(new int[] { 90, 101 })
                .Should()
                .BeTrue();
        }

        const int EXCLUSION_RANDOM_ITERS = 50;
        [Fact]
        public void DetermineExclusion_FromSetOverRandomRange()
        {
            var r = new Random();
            for (int i = 0; i < EXCLUSION_RANDOM_ITERS; i++)
            {
                var counta = r.Next() % 30 + 35;
                var offseta = (r.Next() % 100) - 50;
                var lengtha = (r.Next() % 100) + 50;
                var itemsa = Enumerable.Range(0, counta).Select(_ => (r.Next() % lengtha) + offseta).Distinct().OrderBy(v => v).ToList();

                var countb = counta + (r.Next() % 70) - 35;
                var offsetb = offseta + (r.Next() % 70) - 35;
                var lengthb = lengtha + (r.Next() % 70) - 35;
                var itemsb = Enumerable.Range(0, countb).Select(_ => (r.Next() % lengthb) + offsetb).Distinct().OrderBy(v => v).ToList();

                var seta = new IntegerSet(offseta, lengtha);
                seta.AddRange(itemsa);
                seta.Count.Should().Be(itemsa.Count);

                var setb = new IntegerSet(offsetb, lengthb);
                setb.AddRange(itemsb);
                setb.Count.Should().Be(itemsb.Count);

                var except = itemsa.Except(itemsb).ToList();
                seta.ExceptWith(setb);

                var diference = except.Zip(seta, (a1, a2) => a1 - a2).ToList();

                if (!seta.SequenceEqual(except))
                    Debugger.Break();

                seta.SequenceEqual(except)
                        .Should()
                        .BeTrue();
            }
        }
    }
}
