//using FluentAssertions;
//using Kelson.Common.DataStructures.Sets;
//using System;
//using System.Diagnostics;
//using System.Linq;
//using Xunit;

//namespace Kelson.Common.DataStructures.Tests
//{
//    public class IntegerSet_Should
//    {
//        [Fact]
//        public void ContainItems()
//        {
//            var set = new IntegerSet(0, 256) { 3, 112, 160, 193, 204 };
//            for (int i = 0; i < 256; i++)
//                if (i == 3 | i == 112 | i == 160 | i == 193 | i == 204)
//                    set.Contains(i).Should().BeTrue();
//                else
//                    set.Contains(i).Should().BeFalse();
//        }

//        [Fact]
//        public void CopyToRange()
//        {
//            int to_length = 8;
//            int length = 40;
//            for (int to_offset = -10; to_offset <= 10; to_offset++)
//            {
//                for (var offset = -(length + 1); offset <= 0; offset++)
//                {
//                    var set = new IntegerSet(offset, length);
//                    set.Flip();
//                    var expected = new IntegerSet(to_offset, to_length);
//                    if (offset + length >= to_offset + to_length && offset <= to_offset)
//                        expected.AddRange(Enumerable.Range(to_offset, Math.Min(to_length, offset + length - to_offset)));
//                    else if (offset > to_offset)
//                        expected.AddRange(Enumerable.Range(offset, Math.Max(0, Math.Min(to_length - (offset - to_offset), to_length))));
//                    else if (offset + length >= to_offset && offset <= to_offset)
//                        expected.AddRange(Enumerable.Range(to_offset, offset + length - to_offset));
//                    var result = set.CopyIntoRange(to_offset, to_length);
//                    result.SetEquals(expected).Should().BeTrue();
//                    result.Count.Should().Be(expected.Count);
//                }
//                for (var offset = 1; offset <= (length + 1); offset++)
//                {
//                    var set = new IntegerSet(offset, length);
//                    set.Flip();
//                    var expected = new IntegerSet(to_offset, to_length);
//                    if (offset + length >= to_offset + to_length && offset <= to_offset)
//                        expected.AddRange(Enumerable.Range(to_offset, Math.Min(to_length, offset + length - to_offset)));
//                    else if (offset > to_offset)
//                        expected.AddRange(Enumerable.Range(offset, Math.Max(0, Math.Min(to_length - (offset - to_offset), to_length))));
//                    var result = set.CopyIntoRange(to_offset, to_length);
//                    result.SetEquals(expected).Should().BeTrue();
//                    result.Count.Should().Be(expected.Count);
//                }
//            }
//        }


//        [Fact]
//        public void EnumerateItems()
//        {
//            var set = new IntegerSet(0, 256) { 3, 4, 250 };

//            set.SequenceEqual(new int[] { 3, 4, 250 }).Should().BeTrue();
//        }

//        [Fact]
//        public void OffsetIndecies()
//        {
//            var set = new IntegerSet(-112, 5) { -112, -110 };
//            for (int i = -112; i < -107; i++)
//                if (i == -112 | i == -110)
//                    set.Contains(i).Should().BeTrue();
//                else
//                    set.Contains(i).Should().BeFalse();
//        }

//        [Fact]
//        public void DetermineExclusion_FromSetOverSameRange()
//        {
//            var seta = new IntegerSet(5, 100) { 12, 90, 101 };
//            var setb = new IntegerSet(5, 100) { 90 };

//            seta.ExceptWith(setb);
//            var result = seta.ToList();
//            result.SequenceEqual(new int[] { 12, 101 })
//                .Should()
//                .BeTrue();
//        }

//        [Fact]
//        public void DetermineExclusion_FromSetOverDifferentRange()
//        {
//            var seta = new IntegerSet(5, 100) { 12, 90, 101 };
//            var setb = new IntegerSet(80, 20) { 90 };

//            seta.ExceptWith(setb);
//            var result = seta.ToList();
//            result.SequenceEqual(new int[] { 12, 101 })
//                .Should()
//                .BeTrue();

//            seta = new IntegerSet(5, 100) { 12, 90, 101 };
//            setb = new IntegerSet(30, 200) { 90, 101, 153, 229 };

//            seta.ExceptWith(setb);
//            seta.Single().Should().Be(12);

//            seta = new IntegerSet(5, 100) { 12, 90, 101 };
//            setb = new IntegerSet(-64, 200) { 12 };

//            seta.ExceptWith(setb);
//            result = seta.ToList();
//            result.SequenceEqual(new int[] { 90, 101 })
//                .Should()
//                .BeTrue();

//            seta = new IntegerSet(0, 20) { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
//            setb = new IntegerSet(6, 10) { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

//            seta.ExceptWith(setb);
//            result = seta.ToList();
//            result.SequenceEqual(new int[] { 1, 3, 5, 17, 19 })
//                  .Should()
//                  .BeTrue();

//            seta = new IntegerSet(-4, 61) { -2, 0, 1, 3, 5, 6, 7, 8, 9, 11, 13, 17, 19, 20, 21, 22, 24, 27, 28, 31, 34, 36, 37, 38, 39, 40, 42, 44, 46, 48, 55, 56 };
//            setb = new IntegerSet(11, 38) { 12, 13, 15, 16, 17, 19, 21, 25, 26, 27, 28, 29, 30, 31, 32, 33, 36, 39, 40, 41, 42, 43, 44, 45, 46, 47 };

//            seta.ExceptWith(setb);
//            result = seta.ToList();
//            result.SequenceEqual(new int[] { -2, 0, 1, 3, 5, 6, 7, 8, 9, 11, 20, 22, 24, 34, 37, 38, 48, 55, 56 })
//                  .Should()
//                  .BeTrue();
//        }

//        // -2,0,1,3,5,6,7,8,9,11,13,17,19,20,21,22,24,27,28,31,34,36,37,38,39,40,42,44,46,48,55,56
//        // 12,13,15,16,17,19,21,25,26,27,28,29,30,31,32,33,36,39,40,41,42,43,44,45,46,47

//        const int EXCLUSION_RANDOM_ITERS = 50;
//        [Fact]
//        public void DetermineExclusion_FromSetOverRandomRange()
//        {
//            var r = new Random();
//            for (int i = 0; i < EXCLUSION_RANDOM_ITERS; i++)
//            {
//                var counta = r.Next() % 30 + 35;
//                var offseta = (r.Next() % 100) - 50;
//                var lengtha = (r.Next() % 100) + 50;
//                var itemsa = Enumerable.Range(0, counta).Select(_ => (r.Next() % lengtha) + offseta).Distinct().OrderBy(v => v).ToList();

//                var countb = counta + (r.Next() % 70) - 35;
//                var offsetb = offseta + (r.Next() % 70) - 35;
//                var lengthb = lengtha + (r.Next() % 70) - 35;
//                var itemsb = Enumerable.Range(0, countb).Select(_ => (r.Next() % lengthb) + offsetb).Distinct().OrderBy(v => v).ToList();

//                var seta = new IntegerSet(offseta, lengtha);
//                seta.AddRange(itemsa);
//                seta.Count.Should().Be(itemsa.Count);

//                var setb = new IntegerSet(offsetb, lengthb);
//                setb.AddRange(itemsb);
//                setb.Count.Should().Be(itemsb.Count);

//                var except = itemsa.Except(itemsb).ToList();
//                seta.ExceptWith(setb);

//                var diference = except.Zip(seta, (a1, a2) => a1 - a2).ToList();

//                if (!seta.SequenceEqual(except))
//                    Debugger.Break();

//                seta.SequenceEqual(except)
//                        .Should()
//                        .BeTrue();
//            }
//        }
//    }
//}
