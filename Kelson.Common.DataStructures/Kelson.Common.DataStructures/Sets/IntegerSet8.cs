using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// SIMD accelerated Set of integers within a bounded range
    /// </summary>
    public class IntegerSet : ISet<int>
    {
        private ImmutableSet8[] data;
        private int offset;
        private readonly int length;

        public IntegerSet(int start, int length)
        {
            offset = start;
            this.length = length;
            data = new ImmutableSet8[((length - 1) >> 3) + 1];
        }

        protected IntegerSet(ImmutableSet8[] data, int offset)
        {
            this.data = data;
            this.offset = offset;
            for (int i = 0; i < data.Length; i++)
                Count += data[i].Count;
        }

        private int BlockIndex(int i) => i >> 3;
        private int BitIndex(int i) => i % 8;

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

        public void Flip()
        {
            for (int i = 0; i < data.Length; i++)
            {
                var count = data[i].Count;
                data[i] = ~data[i];
                Count += data[i].Count - count;
            }
        }

        const int BLOCK_SIZE = 8;
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
            //var block_shift = difference / BLOCK_SIZE;
            //var bit_shift = (Math.Abs(difference) % BLOCK_SIZE);
            //if (difference < 0)
            //    bit_shift = -bit_shift;

            for (int i = 0; i <= data.Length; i++)
            {
                var current = i < data.Length ? data[i] : new ImmutableSet8();

                var value = (i * BLOCK_SIZE) + offset;

                if (value >= newOffset + newLength)
                    yield break;

                var block_index_in_new_range = ((value - newOffset) / BLOCK_SIZE);
                var bit_index_in_new_range = Math.Abs(value - newOffset) % BLOCK_SIZE;

                if (block_index_in_new_range < 0)
                    continue;

                if (bit_index_in_new_range == 0)
                    yield return current;
                else if (difference < 0)
                    yield return current.Shift(-bit_index_in_new_range, i < data.Length - 1 ? data[i + 1] : new ImmutableSet8());
                else
                    yield return current.Shift(bit_index_in_new_range, i > 0 ? data[i - 1] : new ImmutableSet8());
            }

            //for (int origin_block = 0; origin_block < data.Length && origin_block < block_count; origin_block++)
            //{
            //    if (origin_block - block_shift < 0 || origin_block - block_shift > data.Length - 1)
            //        continue;

            //    var value_set = data[origin_block - block_shift];
            //    var adjacent = new ImmutableSet8();
            //    if (block_shift <= 0 && bit_shift <= 0 && origin_block - block_shift + 1 < data.Length)
            //        adjacent = data[origin_block - block_shift + 1];
            //    else if (block_shift >= 0 && bit_shift >= 0 && origin_block - block_shift - 1 > 0)
            //        adjacent = data[origin_block - block_shift - 1];
            //    yield return value_set.Shift(bit_shift, adjacent);
            //}
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
                //int i = 0;
                //foreach (var block in other.ShiftedSets(offset, length))
                //{
                //    var count = data[i].Count;
                //    data[i] = data[i].Except(block);
                //    Count += data[i].Count - count;
                //    i++;
                //}
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
                int block = i << 3;
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
