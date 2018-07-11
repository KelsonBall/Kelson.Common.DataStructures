using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kelson.Common.Bitwise;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// An immutable set of integers in the range [0, 7]
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct ImmutableSet8 : IImmutableSet<int>, IEquatable<ImmutableSet8>, IEquatable<byte>
    {
        [FieldOffset(0)]
        private readonly byte values;

        public ImmutableSet8(in byte value) => values = value;

        /// <summary>
        /// Jump table mapping all byte values to the number of bits in that byte
        /// </summary>
        private static readonly byte[] BITCOUNTS = new byte[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        /// <summary>
        /// Enumerates set to determine number of values
        /// </summary>
        public int Count => BITCOUNTS[values];

        public bool IsEmpty => values == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Guard(int index)
        {
            if (index > 7 || index < 0)
                throw new IndexOutOfRangeException($"Index {index} is out of {nameof(ImmutableSet8)} range of [0,7]");
        }

        public bool this[int value] => Contains(value);

        /// <summary>
        /// Shift a set in a positive or negative direction
        /// </summary>
        public ImmutableSet8 Shift(int shift) => shift > 0 ? (ImmutableSet8)(values << shift) : (ImmutableSet8)(values >> -shift);

        /// <summary>
        /// Shifts a set as if it contained the adjacent set
        /// </summary>
        public ImmutableSet8 Shift(int shift, ImmutableSet8 adjacent)
        {
            var shifted = Shift(shift);
            if (shift < 0)
            {
                adjacent = adjacent.Shift(8 + shift);
                return shifted.Union(adjacent);
            }
            else
            {
                adjacent = adjacent.Shift(-8 + shift);
                return shifted.Union(adjacent);
            }
        }

        public static ImmutableSet8 operator ~(ImmutableSet8 set) => (ImmutableSet8)~set.values;

        IImmutableSet<int> IImmutableSet<int>.Add(int value)
        {
            Guard(value);
            return (ImmutableSet8)values.Set(value);
        }

        public ImmutableSet8 Add(int value)
        {
            Guard(value);
            return (ImmutableSet8)values.Set(value);
        }

        public ImmutableSet8 Add(params int[] valuesArray)
        {
            var v = this;
            foreach (var item in valuesArray)
                v = v.Add(item);
            return v;
        }

        public ImmutableSet8 AddRange(IEnumerable<int> valuesEnumerable)
        {
            var v = this;
            foreach (var item in valuesEnumerable)
                v = v.Add(item);
            return v;
        }

        IImmutableSet<int> IImmutableSet<int>.Clear() => new ImmutableSet8();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet8 Clear() => new ImmutableSet8();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int value) => values.IsSet(value);

        IImmutableSet<int> IImmutableSet<int>.Except(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return Except(set);
            byte v = values;
            foreach (var item in other)
            {
                Guard(item);
                v = v.Clear(item);
            }
            return (ImmutableSet8)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet8 Except(ImmutableSet8 other) => (ImmutableSet8)(values & ~other.values);

        IImmutableSet<int> IImmutableSet<int>.Intersect(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return Intersect(set);
            var v = new ImmutableSet8();
            foreach (var item in other)
                if (Contains(item))
                    v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet8 Intersect(ImmutableSet8 other) => (ImmutableSet8)(values & other.values);

        bool IImmutableSet<int>.IsProperSubsetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return IsProperSubsetOf(set);
            var v = this;
            bool foundOther = false;
            foreach (var item in other)
            {
                if (!Contains(item))
                    foundOther = true;
                else
                    v = v.Remove(item);
                if (v.IsEmpty && foundOther)
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSubsetOf(ImmutableSet8 other)
        {
            byte dif = (byte)(values ^ other.values);
            return ((values & dif) == 0) && ((other.values & dif) != 0);
        }

        bool IImmutableSet<int>.IsProperSupersetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return IsProperSupersetOf(set);
            var v = this;
            foreach (var item in other)
            {
                if (!Contains(item))
                    return false;
                else
                    v = v.Remove(item);
            }
            return !v.IsEmpty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSupersetOf(ImmutableSet8 other)
        {
            byte dif = (byte)(values ^ other.values);
            return ((values & dif) != 0) && ((other.values & dif) == 0);
        }

        bool IImmutableSet<int>.IsSubsetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return IsSubsetOf(set);
            // if all values in `this` are found in `other` return true
            var v = this;
            foreach (var item in other)
            {
                v = v.Remove(item);
                if (v.IsEmpty)
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSubsetOf(ImmutableSet8 other) => ((values & (values ^ other.values)) == 0);

        bool IImmutableSet<int>.IsSupersetOf(IEnumerable<int> other)
        {
            // if all values in `other` are found in `this` return true
            foreach (var item in other)
                if (!Contains(item))
                    return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(ImmutableSet8 other) => ((values & (values ^ other.values)) != 0);

        bool IImmutableSet<int>.Overlaps(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return Overlaps(set);
            foreach (var item in other)
                if (Contains(item))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(ImmutableSet8 other) => (values & other.values) != 0;

        IImmutableSet<int> IImmutableSet<int>.Remove(int value) => Remove(value);

        public ImmutableSet8 Remove(int value)
        {
            Guard(value);
            return (ImmutableSet8)values.Clear(value);
        }

        IImmutableSet<int> IImmutableSet<int>.SymmetricExcept(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return SymmetricExcept(set);
            var v = new ImmutableSet8();
            foreach (var item in other)
                if (!Contains(item))
                    v = v.Add(item);
            foreach (var item in this)
                if (!v.Contains(item))
                    v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet8 SymmetricExcept(ImmutableSet8 other) => (ImmutableSet8)(values ^ other.values);

        public bool TryGetValue(int equalValue, out int actualValue)
        {
            actualValue = equalValue;
            return Contains(equalValue);
        }

        IImmutableSet<int> IImmutableSet<int>.Union(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return Union(set);
            var v = this;
            foreach (var item in other)
                v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet8 Union(ImmutableSet8 other) => (ImmutableSet8)(values | other.values);

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < 8; i++)
                if (values.IsSet(i))
                    yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool IImmutableSet<int>.SetEquals(IEnumerable<int> other)
        {
            if (other is ImmutableSet8 set)
                return SetEquals(set);
            var v = this;
            foreach (var item in other)
            {
                if (!v.Contains(item))
                    return false;
                v = v.Remove(item);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetEquals(ImmutableSet8 other) => values == other.values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ImmutableSet8 other) => values == other.values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(byte other) => values == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe explicit operator ImmutableSet8(in byte v)
        {
            fixed (byte* ptr = &v)
                return *(ImmutableSet8*)ptr;
        }

        public override string ToString() => $"[{string.Join(", ", this)}]";
    }
}
