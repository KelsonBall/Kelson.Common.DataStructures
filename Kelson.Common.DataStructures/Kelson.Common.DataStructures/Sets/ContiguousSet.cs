using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// SIMD Set of integers
    /// </summary>
    public class ContiguousSet : ISet<int>
    {
        private readonly ImmutableSet64[] data;
        private readonly int offset;
        private readonly int length;

        public ContiguousSet(int start, int length)
        {
            offset = start;
            this.length = length;
            data = new ImmutableSet64[(length >> 6) + 1];
        }

        protected ContiguousSet(ImmutableSet64[] data, int offset)
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
            {
                var add = Add(item);
                if (add)
                    Count++;
                newItem = newItem | add;
            }
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

        private void Guard(ContiguousSet other)
        {
            if (other.offset != offset || other.length != length)
                throw new ArgumentException("Sets are over different ranges");
        }

        void ISet<int>.ExceptWith(IEnumerable<int> other)
        {
            if (other is ContiguousSet set)
                ExceptWith(set);
            else
            {
                foreach (var item in other)
                {
                    var i = item - offset;
                    data[BlockIndex(i)].Remove(BitIndex(i));
                }
            }
        }

        void ExceptWith(ContiguousSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                data[i] = data[i].Except(other.data[i]);
        }

        void ISet<int>.IntersectWith(IEnumerable<int> other)
        {
            var otherSet = new ContiguousSet(offset, length);
            otherSet.AddRange(other);
            IntersectWith(otherSet);
        }

        public void IntersectWith(ContiguousSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                data[i] = data[i].Intersect(other.data[i]);
        }

        public bool IsProperSubsetOf(IEnumerable<int> other)
        {
            var otherSet = new ContiguousSet(offset, length);
            otherSet.AddRange(other);
            return IsProperSubsetOf(otherSet);
        }

        public bool IsProperSubsetOf(ContiguousSet other)
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
            var otherSet = new ContiguousSet(offset, length);
            otherSet.AddRange(other);
            return IsProperSupersetOf(otherSet);
        }

        public bool IsPropertSupersetOf(ContiguousSet other)
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
            var otherSet = new ContiguousSet(offset, length);
            otherSet.AddRange(other);
            return IsSubsetOf(otherSet);
        }

        public bool IsSubsetOf(ContiguousSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                if (!data[i].IsSubsetOf(other.data[i]))
                    return false;
            return true;
        }

        public bool IsSupersetOf(IEnumerable<int> other)
        {
            var otherSet = new ContiguousSet(offset, length);
            otherSet.AddRange(other);
            return IsSupersetOf(otherSet);
        }

        public bool IsSupersetOf(ContiguousSet other)
        {
            Guard(other);
            for (int i = 0; i < data.Length; i++)
                if (!data[i].IsSupersetOf(other.data[i]))
                    return false;
            return true;
        }

        public bool Overlaps(IEnumerable<int> other)
        {
            var otherSet = new ContiguousSet(offset, length);
            otherSet.AddRange(other);
            return Overlaps(otherSet);
        }

        public bool Overlaps(ContiguousSet other)
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
                foreach (var item in data[i])
                    yield return item + offset;
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();
    }
}
