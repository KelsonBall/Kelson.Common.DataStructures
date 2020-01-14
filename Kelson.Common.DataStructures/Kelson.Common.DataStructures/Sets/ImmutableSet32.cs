using Kelson.Common.Bitwise;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kelson.Common.DataStructures.Sets
{
    // <summary>
    /// An immutable set of integers in the range [0, 31]
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct ImmutableSet32 : IImmutableSet<int>, IEquatable<ImmutableSet32>, IEquatable<uint>
    {
        [FieldOffset(0)]
        private readonly uint values;

        public ImmutableSet32(in uint value) => values = value;

        /// <summary>
        /// Jump table mapping all byte values to the number of bits in that byte
        /// </summary>
        private static readonly byte[] BITCOUNTS = new byte[] { 
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
            4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        /// <summary>
        /// Enumerates set to determine number of values
        /// </summary>
        public unsafe int Count
        {
            get
            {
                if (values == 0) return 0;
                int sum = 0;                
                fixed (uint* ptr = &values)
                {                        
                    byte* bytes = (byte*)ptr;
                    for (int i = 0; i < 4; i++)
                        sum += BITCOUNTS[*(bytes + i)];                                                
                }                
                return sum;
            }
        }

#if DEBUG
        public int EnumerateAndCount
        {
            get
            {
                var v = values;
                uint sum = v & 1;
                while ((v = (v >> 1)) != 0)
                    sum += v & 1;
                return (int)sum;
            }
        }
#endif

        public bool IsEmpty => values == 0;

        public bool IsFull => values == uint.MaxValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Guard(int index)
        {
            if (index > 31 || index < 0)
                throw new IndexOutOfRangeException($"Index {index} is out of {nameof(ImmutableSet32)} range of [0,31]");
        }

        public static ImmutableSet32 operator >>(ImmutableSet32 s, int shift) => s.Shift(shift);

        public static ImmutableSet32 operator <<(ImmutableSet32 s, int shift) => s.Shift(-shift);

        /// <summary>
        /// Shift a set in a positive or negative direction
        /// </summary>
        public ImmutableSet32 Shift(int shift) => shift > 0 ? (ImmutableSet32)(values << shift) : (ImmutableSet32)(values >> -shift);

        /// <summary>
        /// Shifts a set as if it contained the adjacent set
        /// </summary>
        public ImmutableSet32 Shift(int shift, ImmutableSet32 adjacent)
        {
            var shifted = Shift(shift);
            if (shift < 0)
            {
                adjacent = adjacent.Shift(32 + shift);
                return shifted.Union(adjacent);
            }
            else
            {
                adjacent = adjacent.Shift(-32 + shift);
                return shifted.Union(adjacent);
            }
        }

        public static ImmutableSet32 operator ~(ImmutableSet32 set) => (ImmutableSet32)~set.values;

        IImmutableSet<int> IImmutableSet<int>.Add(int value)
        {
            Guard(value);
            return (ImmutableSet32)values.Set(value);
        }

        public ImmutableSet32 Add(int value)
        {
            Guard(value);
            return (ImmutableSet32)values.Set(value);
        }

        public ImmutableSet32 Add(params int[] valuesArray)
        {
            var v = this;
            foreach (var item in valuesArray)
                v = v.Add(item);
            return v;
        }

        public ImmutableSet32 AddRange(IEnumerable<int> valuesEnumerable)
        {
            var v = this;
            foreach (var item in valuesEnumerable)
                v = v.Add(item);
            return v;
        }

        IImmutableSet<int> IImmutableSet<int>.Clear() => new ImmutableSet32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet32 Clear() => new ImmutableSet32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int value) => values.IsSet(value);

        IImmutableSet<int> IImmutableSet<int>.Except(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
                return Except(set);
            uint v = values;
            foreach (var item in other)
            {
                Guard(item);
                v = v.Clear(item);
            }
            return (ImmutableSet32)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet32 Except(ImmutableSet32 other) => (ImmutableSet32)(values & ~other.values);

        IImmutableSet<int> IImmutableSet<int>.Intersect(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
                return Intersect(set);
            var v = new ImmutableSet32();
            foreach (var item in other)
                if (Contains(item))
                    v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet32 Intersect(ImmutableSet32 other) => (ImmutableSet32)(values & other.values);

        bool IImmutableSet<int>.IsProperSubsetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
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
        public bool IsProperSubsetOf(ImmutableSet32 other)
        {
            uint dif = values ^ other.values;
            return ((values & dif) == 0) && ((other.values & dif) != 0);
        }

        bool IImmutableSet<int>.IsProperSupersetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
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
        public bool IsProperSupersetOf(ImmutableSet32 other)
        {
            uint dif = values ^ other.values;
            return ((values & dif) != 0) && ((other.values & dif) == 0);
        }

        bool IImmutableSet<int>.IsSubsetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
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
        public bool IsSubsetOf(ImmutableSet32 other) => ((values & (values ^ other.values)) == 0);

        bool IImmutableSet<int>.IsSupersetOf(IEnumerable<int> other)
        {
            // if all values in `other` are found in `this` return true
            foreach (var item in other)
                if (!Contains(item))
                    return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(ImmutableSet32 other) => ((values & (values ^ other.values)) != 0);

        bool IImmutableSet<int>.Overlaps(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
                return Overlaps(set);
            foreach (var item in other)
                if (Contains(item))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(ImmutableSet32 other) => (values & other.values) != 0;

        IImmutableSet<int> IImmutableSet<int>.Remove(int value) => Remove(value);

        public ImmutableSet32 Remove(int value)
        {
            Guard(value);
            return (ImmutableSet32)values.Clear(value);
        }

        IImmutableSet<int> IImmutableSet<int>.SymmetricExcept(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
                return SymmetricExcept(set);
            var v = new ImmutableSet32();
            foreach (var item in other)
                if (!Contains(item))
                    v = v.Add(item);
            foreach (var item in this)
                if (!v.Contains(item))
                    v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet32 SymmetricExcept(ImmutableSet32 other) => (ImmutableSet32)(values ^ other.values);

        public bool TryGetValue(int equalValue, out int actualValue)
        {
            actualValue = equalValue;
            return Contains(equalValue);
        }

        IImmutableSet<int> IImmutableSet<int>.Union(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
                return Union(set);
            var v = this;
            foreach (var item in other)
                v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet32 Union(ImmutableSet32 other) => (ImmutableSet32)(values | other.values);

        public IEnumerator<int> GetEnumerator()
        {
            if (values == 0)
                yield break;

            // first 4 bytes
            uint v = values;
            for (int i = 0; i < 4 && v != 0; i++)
            {
                // each byte
                byte b = (byte)(v & 0xFF);
                for (int j = 0; j < 8 && b != 0; j++)
                {
                    if ((b & 1) == 1)
                        yield return (i << 3) + j;
                    b = (byte)(b >> 1);
                }
                v >>= 8;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool IImmutableSet<int>.SetEquals(IEnumerable<int> other)
        {
            if (other is ImmutableSet32 set)
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
        public bool SetEquals(ImmutableSet32 other) => values == other.values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ImmutableSet32 other) => values == other.values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(uint other) => values == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe explicit operator ImmutableSet32(uint v) => *(ImmutableSet32*)&v;
        
    }
}
