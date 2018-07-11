using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// SIMD accelerated Set of integers within a bounded range
    /// </summary>
    public class IntegerSet : ISet<int>
    {
        const int BLOCK_SIZE = 8;
        const int BLOCK_SHIFT = 3;

        private int BlockIndex(int i) => i >> BLOCK_SHIFT;
        private int BitIndex(int i) => i % BLOCK_SIZE;

        private ImmutableSet8[] data;
        private readonly ImmutableSet8 front_mask;
        private readonly ImmutableSet8 end_mask;
        private readonly int offset;
        private readonly int sub_block_offset;
        private readonly int block_offset;
        private readonly int length;
        private readonly int last_value;

        public IntegerSet(int start, int length)
        {
            offset = start;
            this.length = length;
            last_value = offset + length;
            sub_block_offset = BitIndex(start);
            block_offset = -BlockIndex(start);
            data = new ImmutableSet8[BlockIndex(start + length) - BlockIndex(start) + 1];
        }

        protected IntegerSet(ImmutableSet8[] data, int offset)
        {
            this.data = data;
            this.offset = offset;
            for (int i = 0; i < data.Length; i++)
                Count += data[i].Count;
        }

        public bool this[int value]
        {
            get
            {
                if (Guard(value))
                    return data[BlockIndex(value) - sub_block_offset][BitIndex(value)];
                else
                    throw new IndexOutOfRangeException();
            }
        }

        public int Count { get; private set; }

        bool ICollection<int>.IsReadOnly => false;

        public void Flip()
        {
            for (int i = 0; i < data.Length; i++)
            {
                var count = data[i].Count;
                data[i] = ~data[i];
                Count += data[i].Count - count;
            }
        }

        public IntegerSet CopyIntoRange(int newOffset, int newLength)
        {
            if (newOffset + newLength < offset || newOffset > offset + length)
                return new IntegerSet(newOffset, 0);
            var newSet = new IntegerSet(newOffset, newLength);

            int i = Math.Max(0, (offset - newOffset) / BLOCK_SIZE);
            foreach (var block in ShiftedSets(newOffset, newLength))
            {
                if (i == newSet.data.Length)
                    break;
                newSet.data[i++] = block;
                newSet.Count += block.Count;
            }
            return newSet;
        }

        public IEnumerable<ImmutableSet8> ShiftedSets(int newOffset, int newLength)
        {
            var difference = offset - newOffset;
            var bit_index_in_new_range = Math.Abs(offset - newOffset) % BLOCK_SIZE;
            var start_block_offset = (offset - newOffset) / BLOCK_SIZE;
            for (int i = 0; i <= data.Length; i++)
            {
                var current = i < data.Length ? data[i] : new ImmutableSet8();
                var value = (i * BLOCK_SIZE) + offset;
                if (value >= newOffset + newLength)
                    yield break;
                var block_index_in_new_range = start_block_offset + i;
                if (block_index_in_new_range < 0)
                    continue;
                if (bit_index_in_new_range == 0)
                    yield return current;
                else if (difference < 0)
                    yield return current.Shift(-bit_index_in_new_range, i < data.Length - 1 ? data[i + 1] : new ImmutableSet8());
                else
                    yield return current.Shift(bit_index_in_new_range, i > 0 ? data[i - 1] : new ImmutableSet8());
            }
        }

        public bool Add(int item)
        {
            var i = item - offset;
            if (data[BlockIndex(i)].Contains(BitIndex(i)))
                return false;
            data[BlockIndex(i)] = data[BlockIndex(i)].Add(BitIndex(i));
            Count++;
            return true;
        }

        public bool AddRange(IEnumerable<int> other)
        {
            bool newItem = false;
            foreach (var item in other)
                newItem = newItem | Add(item);
            return newItem;
        }

        void ICollection<int>.Add(int item) => Add(item);

        public void Clear()
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = new ImmutableSet8();
            Count = 0;
        }

        public bool Contains(int item)
        {
            var i = item - offset;
            return data[BlockIndex(i)].Contains(BitIndex(i));
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (var item in this)
                array[i++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool InRange(int value)
        {
            var i = value - offset;
            if (BlockIndex(i) > data.Length || i < 0)
                return false;
            return true;
        }

        private bool Guard(IntegerSet other) => (other.offset == offset && other.length == length);
        private bool Guard(int value) => value >= offset && value <= last_value;
        void ISet<int>.ExceptWith(IEnumerable<int> other)
        {
            if (other is IntegerSet set)
                ExceptWith(set);
            else
            {
                foreach (var item in other)
                {
                    var i = item - offset;
                    var index = BlockIndex(i);
                    var count = data[index].Count;
                    data[index] = data[index].Remove(BitIndex(i));
                    Count += data[index].Count - count;
                }
            }
        }

        public void ExceptWith(IntegerSet other)
        {
            if (Guard(other))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var count = data[i].Count;
                    data[i] = data[i].Except(other.data[i]);
                    Count += data[i].Count - count;
                }
            }
            else
            {
                ExceptWith(other.CopyIntoRange(offset, length));
            }
        }

        void ISet<int>.IntersectWith(IEnumerable<int> other)
        {
            var otherSet = new IntegerSet(offset, length);
            otherSet.AddRange(other);
            IntersectWith(otherSet);
        }

        public void IntersectWith(IntegerSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                data[i] = data[i].Intersect(other.data[i]);
        }

        public bool IsProperSubsetOf(IEnumerable<int> other)
        {
            var otherSet = new IntegerSet(offset, length);
            otherSet.AddRange(other);
            return IsProperSubsetOf(otherSet);
        }

        public bool IsProperSubsetOf(IntegerSet other)
        {
            Guard(other);
            bool hasProperSubset = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (!hasProperSubset)
                    hasProperSubset = data[i].IsProperSubsetOf(other.data[i]);
                if (!data[i].IsSubsetOf(other.data[i]))
                    return false;
            }
            return hasProperSubset;
        }

        public bool IsProperSupersetOf(IEnumerable<int> other)
        {
            var otherSet = new IntegerSet(offset, length);
            otherSet.AddRange(other);
            return IsProperSupersetOf(otherSet);
        }

        public bool IsPropertSupersetOf(IntegerSet other)
        {
            Guard(other);
            bool hasProperSuperset = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (!hasProperSuperset)
                    hasProperSuperset = data[i].IsProperSupersetOf(other.data[i]);
                if (!data[i].IsSupersetOf(other.data[i]))
                    return false;
            }
            return hasProperSuperset;
        }

        public bool IsSubsetOf(IEnumerable<int> other)
        {
            var otherSet = new IntegerSet(offset, length);
            otherSet.AddRange(other);
            return IsSubsetOf(otherSet);
        }

        public bool IsSubsetOf(IntegerSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                if (!data[i].IsSubsetOf(other.data[i]))
                    return false;
            return true;
        }

        public bool IsSupersetOf(IEnumerable<int> other)
        {
            var otherSet = new IntegerSet(offset, length);
            otherSet.AddRange(other);
            return IsSupersetOf(otherSet);
        }

        public bool IsSupersetOf(IntegerSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                if (!data[i].IsSupersetOf(other.data[i]))
                    return false;
            return true;
        }

        public bool Overlaps(IEnumerable<int> other)
        {
            var otherSet = new IntegerSet(offset, length);
            otherSet.AddRange(other);
            return Overlaps(otherSet);
        }

        public bool Overlaps(IntegerSet other)
        {
            if (Guard(other))
            {
                for (int i = 0; i < data.Length; i++)
                    if (data[i].Overlaps(other.data[i]))
                        return true;
                return false;
            }
            else
                return Overlaps(other.CopyIntoRange(offset, length));
        }

        public bool Remove(int item)
        {
            var i = item - offset;
            if (data[BlockIndex(i)].Contains(BitIndex(i)))
            {
                Count--;
                data[BlockIndex(i)] = data[BlockIndex(i)].Remove(i);
                return true;
            }
            return false;
        }

        public bool SetEquals(IEnumerable<int> other) => other.OrderBy(i => i).SequenceEqual(this);

        public bool SymmetricExceptWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        public bool UnionWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            for (int i = 0; i < data.Length; i++)
            {
                int block = i << BLOCK_SHIFT;
                foreach (var item in data[i])
                    yield return block + item + offset;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();

        void ISet<int>.SymmetricExceptWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        void ISet<int>.UnionWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }
    }
}
