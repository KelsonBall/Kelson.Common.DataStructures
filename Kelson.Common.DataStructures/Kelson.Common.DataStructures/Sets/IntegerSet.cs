using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// SIMD accelerated Set of integers within a bounded range
    /// </summary>
    public class IntegerSet : ISet<int>
    {
        private readonly ImmutableSet64[] data;
        private int offset;
        private readonly int length;

        public IntegerSet(int start, int length)
        {
            offset = start;
            this.length = length;
            data = new ImmutableSet64[(length >> 6) + 1];
        }

        protected IntegerSet(ImmutableSet64[] data, int offset)
        {
            this.data = data;
            this.offset = offset;
            for (int i = 0; i < data.Length; i++)
                Count += data[i].Count;
        }

        private int BlockIndex(int i) => i >> 6;
        private int BitIndex(int i) => i % 64;

        public int Count { get; private set; }

        bool ICollection<int>.IsReadOnly => throw new NotImplementedException();

        /// <summary>
        /// !!Mixed mutability operation!!
        /// Returns a set with a reference to the same data but with all values shifted by the displacement value
        /// </summary>
        /// <param name="displacement"></param>
        /// <returns></returns>
        public IntegerSet Shifted(int displacement) => new IntegerSet(data, offset + displacement);

        public static IntegerSet operator <<(IntegerSet set, int displacement) => set.Shifted(-displacement);

        public static IntegerSet operator >>(IntegerSet set, int displacement) => set.Shifted(displacement);

        const int BLOCK_SIZE = sizeof(ulong);
        public IntegerSet CopyIntoRange(int newOffset, int newLength)
        {
            var newSet = new IntegerSet(newOffset, newLength);
            var difference = offset - newOffset;
            var block_shift = difference / BLOCK_SIZE;
            var bit_shift = difference % BLOCK_SIZE;
            for (int block = 0; block < data.Length; block++)
            {
                var aligned = 
            }
            return newSet;
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

        void ICollection<int>.Clear()
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = new ImmutableSet64();
            Count = 0;
        }

        bool ICollection<int>.Contains(int item)
        {
            var i = item - offset;
            return data[BlockIndex(i)].Contains(BitIndex(i));
        }

        void ICollection<int>.CopyTo(int[] array, int arrayIndex)
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
                var block_offset = (offset - other.offset) / 64;
                var bit_offset = -((offset - other.offset) % 64);
                for (int block = 0; block < data.Length && block + block_offset < other.data.Length; block++)
                {
                    if (block + block_offset < 0)
                        continue;
                    var a = data[block];
                    var b = other.data[block + block_offset];
                    var c = new ImmutableSet64();
                    if (bit_offset > 0 && (block + block_offset) - 1 >= 0)
                        c = other.data[block + block_offset - 1];
                    else if ((block + block_offset) + 1 < other.data.Length)
                        c = other.data[block + block_offset + 1];
                    var aligned_b = b.Shift(bit_offset, c);

                    var count = data[block].Count;
                    data[block] = a.Except(aligned_b);
                    Count += data[block].Count - count;
                    var removed = a.Intersect(aligned_b);
                }
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
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                if (data[i].Overlaps(other.data[i]))
                    return true;
            return false;
        }

        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

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
                int block = i << 6;
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
