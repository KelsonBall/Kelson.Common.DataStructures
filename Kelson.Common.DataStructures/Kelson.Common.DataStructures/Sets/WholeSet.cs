using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// A set of whole numbers
    /// </summary>
    public class WholeSet : ISet<int>
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public bool IsReadOnly => throw new NotImplementedException();

        const int BLOCK_SIZE = 64;
        const int BLOCK_SHIFT = 6;

        private int BlockIndex(int i) => i >> BLOCK_SHIFT;
        private int BitIndex(int i) => i % BLOCK_SIZE;

        private readonly List<ImmutableSet64> data;

        public WholeSet()
        {
            data = new List<ImmutableSet64>();
        }

        private int ConfirmLength(int item)
        {
            var index = BlockIndex(item);
            while (data.Count <= index)
                data.Add(new ImmutableSet64());
            return item;
        }

        public bool Add(int item)
        {
            var i = ConfirmLength(item);
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
            for (int i = 0; i < data.Count; i++)
                data[i] = new ImmutableSet64();
            Count = 0;
        }

        public bool Contains(int item)
        {
            var i = ConfirmLength(item);
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
            var i = ConfirmLength(value);
            if (BlockIndex(i) > data.Count || i < 0)
                return false;
            return true;
        }

        void ISet<int>.ExceptWith(IEnumerable<int> other)
        {
            if (other is WholeSet set)
                ExceptWith(set);
            else
            {
                foreach (var item in other)
                {
                    var i = ConfirmLength(item);
                    var index = BlockIndex(i);
                    var count = data[index].Count;
                    data[index] = data[index].Remove(BitIndex(i));
                    Count += data[index].Count - count;
                }
            }
        }

        public void ExceptWith(WholeSet other)
        {
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
            {
                var count = data[i].Count;
                data[i] = data[i].Except(other.data[i]);
                Count += data[i].Count - count;
            }
        }

        void ISet<int>.IntersectWith(IEnumerable<int> other)
        {
            var otherSet = new WholeSet();
            otherSet.AddRange(other);
            IntersectWith(otherSet);
        }

        public void IntersectWith(WholeSet other)
        {
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
                data[i] = data[i].Intersect(other.data[i]);
        }

        public bool IsProperSubsetOf(IEnumerable<int> other)
        {
            var otherSet = new WholeSet();
            otherSet.AddRange(other);
            return IsProperSubsetOf(otherSet);
        }

        public bool IsProperSubsetOf(WholeSet other)
        {
            bool hasProperSubset = false;
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
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
            var otherSet = new WholeSet();
            otherSet.AddRange(other);
            return IsProperSupersetOf(otherSet);
        }

        public bool IsPropertSupersetOf(WholeSet other)
        {
            bool hasProperSuperset = false;
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
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
            var otherSet = new WholeSet();
            otherSet.AddRange(other);
            return IsSubsetOf(otherSet);
        }

        public bool IsSubsetOf(WholeSet other)
        {
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
                if (!data[i].IsSubsetOf(other.data[i]))
                    return false;
            return true;
        }

        public bool IsSupersetOf(IEnumerable<int> other)
        {
            var otherSet = new WholeSet();
            otherSet.AddRange(other);
            return IsSupersetOf(otherSet);
        }

        public bool IsSupersetOf(WholeSet other)
        {
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
                if (!data[i].IsSupersetOf(other.data[i]))
                    return false;
            return true;
        }

        public bool Overlaps(IEnumerable<int> other)
        {
            var otherSet = new WholeSet();
            otherSet.AddRange(other);
            return Overlaps(otherSet);
        }

        public bool Overlaps(WholeSet other)
        {
            for (int i = 0; i < data.Count && i < other.data.Count; i++)
                if (data[i].Overlaps(other.data[i]))
                    return true;
            return false;
        }

        public bool Remove(int item)
        {
            var i = ConfirmLength(item);
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
            for (int i = 0; i < data.Count; i++)
            {
                int block = i << 6;
                foreach (var item in data[i])
                    yield return block + item;
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
