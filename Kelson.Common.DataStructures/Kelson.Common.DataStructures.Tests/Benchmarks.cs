using FluentAssertions;
using Kelson.Common.DataStructures.Sets;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Kelson.Common.DataStructures.Tests
{
    public class Benchmarks
    {
        [Fact]
        public void Immutable64FasterUnionThanSetT()
        {
            var hash_set_a = ImmutableHashSet<uint>.Empty;
            var hash_set_b = ImmutableHashSet<uint>.Empty;
            var simd_set_a = new UintSet();
            var simd_set_b = new UintSet();

            for (uint i = 0; i < 1000; i++)
            {
                if (i % 3 == 0)
                    hash_set_a = hash_set_a.Add(i);
                if (i % 2 == 0)                
                    hash_set_b = hash_set_b.Add(i);
            }

            for (uint i = 0; i < 1000; i++)
            {
                if (i % 3 == 0)
                    simd_set_a = simd_set_a.Add(i);
                if (i % 2 == 0)                
                    simd_set_b = simd_set_b.Add(i);                
            }

            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100_000; i++)
            {
                _ = hash_set_a.Union(hash_set_b);
            }
            timer.Stop();
            double time1 = timer.Elapsed.TotalMilliseconds;

            timer.Restart();
            for (int i = 0; i < 100_000; i++)
            {
                _ = simd_set_a.Union(simd_set_b);
            }
            timer.Stop();
            double time2 = timer.Elapsed.TotalMilliseconds;

            double ratio = time1 / time2;

            time2.Should().BeLessThan(time1);
            hash_set_a.Union(hash_set_b).ToList().Should().BeEquivalentTo(simd_set_a.Union(simd_set_b).ToList());
        }

        [Fact]
        public void Immutable64FasterIntersectionThanSetT()
        {
            var hash_set_a = ImmutableHashSet<uint>.Empty;
            var hash_set_b = ImmutableHashSet<uint>.Empty;
            var simd_set_a = new UintSet();
            var simd_set_b = new UintSet();

            for (uint i = 0; i < 1000; i++)
            {
                if (i % 3 == 0)
                    hash_set_a = hash_set_a.Add(i);
                if (i % 2 == 0)
                    hash_set_b = hash_set_b.Add(i);
            }

            for (uint i = 0; i < 1000; i++)
            {
                if (i % 3 == 0)
                    simd_set_a = simd_set_a.Add(i);
                if (i % 2 == 0)
                    simd_set_b = simd_set_b.Add(i);
            }

            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100_000; i++)
            {
                _ = hash_set_a.Intersect(hash_set_b);
            }
            timer.Stop();
            double time1 = timer.Elapsed.TotalMilliseconds;

            timer.Restart();
            for (int i = 0; i < 100_000; i++)
            {
                _ = simd_set_a.Intersect(simd_set_b);
            }
            timer.Stop();
            double time2 = timer.Elapsed.TotalMilliseconds;

            double ratio = time1 / time2;

            time2.Should().BeLessThan(time1);
            hash_set_a.Intersect(hash_set_b).ToList().Should().BeEquivalentTo(simd_set_a.Intersect(simd_set_b).ToList());
        }

        [Fact]
        public void CompareCreationTime()
        {
            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 10_000; i++)
            {
                var set = ImmutableHashSet<int>.Empty;
                for (int j = 0; j < 64; j++)
                    set = set.Add(j);
            }
            timer.Stop();
            double time1 = timer.Elapsed.TotalMilliseconds;

            timer.Restart();
            for (int i = 0; i < 10_000; i++)
            {
                var set = new ImmutableSet64();
                for (int j = 0; j < 64; j++)
                    set = set.Add(j);
            }
            timer.Stop();
            double time2 = timer.Elapsed.TotalMilliseconds;

            var ratio = time1 / time2;

            time2.Should().BeLessThan(time1);
        }
    }
}
