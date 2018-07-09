using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kelson.Common.Bitwise;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// An immutable set of integers in the range [0, 63]
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct ImmutableSet64 : IImmutableSet<int>, IEquatable<ImmutableSet64>, IEquatable<ulong>
    {
        [FieldOffset(0)]
        private readonly ulong values;

        public ImmutableSet64(in ulong value) => values = value;        

        /// <summary>
        /// Enumerates set to determine number of values
        /// </summary>
        public int Count => this.Count();
        
        public bool IsEmpty => values == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Guard(int index)
        {
            if (index > 63 || index < 0)
                throw new IndexOutOfRangeException($"Index {index} is out of {nameof(ImmutableSet64)} range of [0,63]");
        }

        IImmutableSet<int> IImmutableSet<int>.Add(int value)
        {
            Guard(value);            
            return (ImmutableSet64)values.Set(value);
        }

        public ImmutableSet64 Add(int value)
        {
            Guard(value);
            return (ImmutableSet64)values.Set(value);
        }

        public ImmutableSet64 Add(params int[] valuesArray)
        {
            var v = this;
            foreach (var item in valuesArray)
                v = v.Add(item);
            return v;
        }

        IImmutableSet<int> IImmutableSet<int>.Clear() => new ImmutableSet64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet64 Clear() => new ImmutableSet64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int value) => values.IsSet(value);

        IImmutableSet<int> IImmutableSet<int>.Except(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
                return Except(set);
            ulong v = values;
            foreach (var item in other)
            {
                Guard(item);
                v = v.Clear(item);
            }
            return (ImmutableSet64)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet64 Except(ImmutableSet64 other) => (ImmutableSet64)(values & ~other.values);        

        IImmutableSet<int> IImmutableSet<int>.Intersect(IEnumerable<int> other)
        {            
            if (other is ImmutableSet64 set)
                return Intersect(set);
            var v = new ImmutableSet64();
            foreach (var item in other)
                if (Contains(item))                                
                    v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet64 Intersect(ImmutableSet64 other) => (ImmutableSet64)(values & other.values);        

        bool IImmutableSet<int>.IsProperSubsetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
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
        public bool IsProperSubsetOf(ImmutableSet64 other)
        {
            ulong dif = values ^ other.values;
            return ((values & dif) == 0) && ((other.values & dif) != 0);
        }

        bool IImmutableSet<int>.IsProperSupersetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
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
        public bool IsProperSupersetOf(ImmutableSet64 other)
        {
            ulong dif = values ^ other.values;
            return ((values & dif) != 0) && ((other.values & dif) == 0);
        }

        bool IImmutableSet<int>.IsSubsetOf(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
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
        public bool IsSubsetOf(ImmutableSet64 other) => ((values & (values ^ other.values)) == 0);
        
        bool IImmutableSet<int>.IsSupersetOf(IEnumerable<int> other)
        {
            // if all values in `other` are found in `this` return true            
            foreach (var item in other)
                if (!Contains(item))
                    return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(ImmutableSet64 other) => ((values & (values ^ other.values)) != 0);

        bool IImmutableSet<int>.Overlaps(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
                return Overlaps(set);
            foreach (var item in other)
                if (Contains(item))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(ImmutableSet64 other) => (values & other.values) != 0;

        IImmutableSet<int> IImmutableSet<int>.Remove(int value) => Remove(value);

        public ImmutableSet64 Remove(int value)
        {
            Guard(value);
            return (ImmutableSet64)values.Clear(value);
        }        

        IImmutableSet<int> IImmutableSet<int>.SymmetricExcept(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
                return SymmetricExcept(set);
            var v = new ImmutableSet64();
            foreach (var item in other)
                if (!Contains(item))
                    v = v.Add(item);
            foreach (var item in this)
                if (!v.Contains(item))
                    v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet64 SymmetricExcept(ImmutableSet64 other) => (ImmutableSet64)(values ^ other.values);

        public bool TryGetValue(int equalValue, out int actualValue)
        {
            actualValue = equalValue;
            return Contains(equalValue);
        }

        IImmutableSet<int> IImmutableSet<int>.Union(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
                return Union(set);
            var v = this;
            foreach (var item in other)
                v = v.Add(item);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSet64 Union(ImmutableSet64 other) => (ImmutableSet64)(values | other.values);        

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < 64; i++)
                if (values.IsSet(i))
                    yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool IImmutableSet<int>.SetEquals(IEnumerable<int> other)
        {
            if (other is ImmutableSet64 set)
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
        public bool SetEquals(ImmutableSet64 other) => values == other.values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ImmutableSet64 other) => values == other.values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ulong other) => values == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe explicit operator ImmutableSet64(in ulong v)
        {
            fixed (ulong* ptr = &v)
                return *(ImmutableSet64*)ptr;
        }        
    }    
}
