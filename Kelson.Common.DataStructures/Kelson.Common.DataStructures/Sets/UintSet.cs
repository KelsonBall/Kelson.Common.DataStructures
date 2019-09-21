using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kelson.Common.DataStructures.Sets
{
    public class UintSet : IImmutableSet<uint>
    {
        private readonly ImmutableSet64[] sets;

        private int block(uint value) => (int)(value >> 6);

        public UintSet()
        {
            sets = new ImmutableSet64[0];
        }

        public UintSet(IEnumerable<uint> values)
        {
            int maxKey = 0;
            var workingSets = new SortedList<int, ImmutableSet64>();
            foreach (var value in values)
            {
                int i = block(value);
                if (i > maxKey)
                    maxKey = i;
                if (workingSets.TryGetValue(i, out ImmutableSet64 set))
                    workingSets[i] = set.Add((int)value - (i * 64));
                else
                    workingSets[i] = new ImmutableSet64().Add((int)value - (i * 64));
            }

            sets = new ImmutableSet64[maxKey + 1];
            int count = 0;
            foreach (var kvp in workingSets)
            {
                count += kvp.Value.Count;
                sets[kvp.Key] = kvp.Value;
            }
            _count = count;
        }

        internal UintSet(UintSet copy)
        {
            sets = copy.sets;
            _count = copy._count;
        }

        internal UintSet(IEnumerable<ImmutableSet64> values)
        {
            sets = values.ToArray();
            _count = sets.Sum(s => s.Count);
        }

        private readonly int _count;
        public int Count => _count;

        public static UintSet operator <<(UintSet set, int shift)
        {
            var values = set;
            for (int i = 0; i < shift; i++)
                values = shiftDown(values);
            return values;
        }

        public static UintSet operator >>(UintSet set, int shift)
        {
            var values = set;
            for (int i = 0; i < shift; i++)
                values = shiftUp(values);
            return values;
        }

        private static UintSet shiftDown(UintSet set)
        {
            var s = set;
            IEnumerable<ImmutableSet64> shifted()
            {
                for (int i = 0; i < s.sets.Length - 1; i++)                
                    yield return (s.sets[i] << 1).Union(s.sets[i + 1] >> 63);
                if (s.sets.Length > 0)
                    yield return s.sets[s.sets.Length - 1] << 1;
            }
            return new UintSet(shifted());
        }

        private static UintSet shiftUp(UintSet set)
        {
            var s = set;
            IEnumerable<ImmutableSet64> shifted()
            {
                if (s.sets.Length == 0)
                    yield break;
                yield return s.sets[0] >> 1;
                for (int i = 1; i < s.sets.Length; i++)
                    yield return (s.sets[i] >> 1).Union(s.sets[i - 1] << 63);
            }
            return new UintSet(shifted());
        }

        IImmutableSet<uint> IImmutableSet<uint>.Add(uint value) => Add(value);

        public UintSet Add(uint value)
        {
            int index = block(value);
            var values = sets;
            IEnumerable<ImmutableSet64> withAdded()
            {
                int i = 0;
                for (; i < Math.Max(values.Length, index + 1); i++)
                {
                    if (i >= values.Length)
                    {
                        if (i == index)
                            yield return new ImmutableSet64().Add((int)value - (index * 64));
                        else
                            yield return new ImmutableSet64();
                    }
                    else if (i == index)
                        yield return values[i].Add((int)value - (index * 64));
                    else
                        yield return values[i];
                }                    
            }

            return new UintSet(withAdded());
        }

        IImmutableSet<uint> IImmutableSet<uint>.Remove(uint value) => Remove(value);

        public UintSet Remove(uint value)
        {
            int index = block(value);
            var values = sets;
            IEnumerable<ImmutableSet64> withRemoved()
            {
                int i = 0;
                for (; i < values.Length; i++)
                {
                    if (i == index)
                        yield return values[i].Remove((int)value - (index * 64));
                    else
                        yield return values[i];
                }
            }

            return new UintSet(withRemoved());
        }

        public IImmutableSet<uint> Clear() => new UintSet();
        
        public bool Contains(uint value) => sets[block(value)].Contains((int)value - block(value));

        public bool ContainsRange(uint start, uint end)
        {
            throw new NotImplementedException();
            for (int i = 0; i < sets.Length; i++)
            {
                int block_start = i << 6;
                if (block_start < start)
                    continue;                
            }
        }

        public IImmutableSet<uint> Except(IEnumerable<uint> other) => Except(new UintSet(other));

        public UintSet Except(UintSet other)
        {
            var (self, except) = (this, other);
            IEnumerable<ImmutableSet64> exceptWith()
            {
                int i = 0;
                for (; i < self.sets.Length && i < except.sets.Length; i++)
                    yield return self.sets[i].Except(except.sets[i]);
                for (; i < self.sets.Length; i++)
                    yield return self.sets[i];
            }

            return new UintSet(exceptWith());
        }        

        public IImmutableSet<uint> Intersect(IEnumerable<uint> other) => Intersect(new UintSet(other));

        public UintSet Intersect(UintSet other)
        {
            var (self, intersect) = (this, other);
            IEnumerable<ImmutableSet64> intersectWith()
            {
                int i = 0;
                for (; i < self.sets.Length && i < intersect.sets.Length; i++)
                    yield return self.sets[i].Intersect(intersect.sets[i]);                
            }

            return new UintSet(intersectWith());
        }

        public bool IsProperSubsetOf(IEnumerable<uint> other) => IsProperSubsetOf(new UintSet(other));

        public bool IsProperSubsetOf(UintSet other)
        {            
            bool atLeastOneProperSubset = false;
            for (int i = 0; i < sets.Length && i < other.sets.Length; i++)
            {
                if (!atLeastOneProperSubset && sets[i].IsProperSubsetOf(other.sets[i]))                
                    atLeastOneProperSubset = true;                
                else if (!sets[i].IsSubsetOf(other.sets[i]))
                    return false;
            }
            return atLeastOneProperSubset;
        }

        public bool IsProperSupersetOf(IEnumerable<uint> other) => IsProperSupersetOf(new UintSet(other));

        public bool IsProperSupersetOf(UintSet other)
        {            
            bool atLeastOneProperSuperset = false;
            int i = 0;
            for (; i < sets.Length && i < other.sets.Length; i++)
            {
                if (!atLeastOneProperSuperset && sets[i].IsProperSupersetOf(other.sets[i]))
                    atLeastOneProperSuperset = true;
                else if (!sets[i].IsSupersetOf(other.sets[i]))
                    return false;
            }
            if (atLeastOneProperSuperset)
                return true;
            for (; i < other.sets.Length; i++)
                if (other.sets[i].Any())
                    return true;
            return false;
        }

        public bool IsSubsetOf(IEnumerable<uint> other) => IsSubsetOf(new UintSet(other));

        public bool IsSubsetOf(UintSet other)
        {
            var (small, large) = sets.Length < other.sets.Length ? (this, other) : (other, this);
            for (int i = 0; i < small.sets.Length; i++)            
                if (!small.sets[i].IsSubsetOf(large.sets[i]))
                    return false;            
            return true;
        }

        public bool IsSupersetOf(IEnumerable<uint> other) => IsSupersetOf(new UintSet(other));

        public bool IsSupersetOf(UintSet other)
        {
            var (small, large) = sets.Length < other.sets.Length ? (this, other) : (other, this);
            for (int i = 0; i < small.sets.Length; i++)
                if (!small.sets[i].IsSupersetOf(large.sets[i]))
                    return false;
            return true;
        }

        public bool Overlaps(IEnumerable<uint> other)
        {
            foreach (var value in other)
                if (Contains(value))
                    return true;
            return false;
        }

        public bool Overlaps(UintSet other)
        {
            for (int i = 0; i < sets.Length && i < other.sets.Length; i++)
                if (sets[i].Overlaps(other.sets[i]))
                    return true;
            return false;
        }        

        public bool SetEquals(IEnumerable<uint> other) => SetEquals(new UintSet(other));

        public bool SetEquals(UintSet other)
        {
            if (sets.Length != other.sets.Length)
                return false;
            for (int i = 0; i < sets.Length; i++)
                if (!sets[i].SetEquals(other.sets[i]))
                    return false;
            return true;
        }

        public IImmutableSet<uint> SymmetricExcept(IEnumerable<uint> other) => SymmetricExcept(new UintSet(other));

        public UintSet SymmetricExcept(UintSet other)
        {
            var (self, except) = (this, other);
            IEnumerable<ImmutableSet64> exceptWith()
            {
                int i = 0;
                for (; i < self.sets.Length && i < except.sets.Length; i++)                
                    yield return self.sets[i].SymmetricExcept(except.sets[i]);                
                if (self.sets.Length > except.sets.Length)
                    for (; i < self.sets.Length; i++)                
                        yield return self.sets[i];                
                else
                    for (; i < except.sets.Length; i++)
                        yield return except.sets[i];
            }

            return new UintSet(exceptWith());
        }

        public bool TryGetValue(uint equalValue, out uint actualValue)
        {            
            if (Contains(equalValue))
            {
                actualValue = equalValue;
                return true;
            }
            else
            {
                actualValue = 0;
                return false;
            }

        }

        public IImmutableSet<uint> Union(IEnumerable<uint> other) => Union(new UintSet(other));

        public UintSet Union(UintSet other)
        {
            var (small, large) = sets.Length < other.sets.Length ? (this, other) : (other, this);
            IEnumerable<ImmutableSet64> union()
            {
                int i = 0;
                for (; i < small.sets.Length; i++)
                    yield return small.sets[i].Union(large.sets[i]);
                for (; i < large.sets.Length; i++)
                    yield return large.sets[i];
            }

            return new UintSet(union());
        }

        public IEnumerator<uint> GetEnumerator()
        {
            IEnumerable<uint> values()
            {
                for (int i = 0; i < sets.Length; i++)
                {
                    int add = i * 64;
                    foreach (var value in sets[i])
                        yield return (uint)(value + add);
                }
            }
            return values().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"Count = {Count}";
    }
}
