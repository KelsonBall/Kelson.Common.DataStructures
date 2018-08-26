using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kelson.Common.DataStructures.Sets
{
    public interface IImmutableIndexSet
    {
        int Max { get; }
        int Count { get; }
        IImmutableIndexSet Add(int index);
        IImmutableIndexSet Remove(int index);
        bool Contains(int index);
        IImmutableIndexSet Clear();        
        IImmutableIndexSet Except(IImmutableIndexSet other);
        IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other);
        IImmutableIndexSet Intersect(IImmutableIndexSet other);
        IImmutableIndexSet Union(IImmutableIndexSet other);
        IImmutableIndexSet Complement();
        bool IsSubsetOf(IImmutableIndexSet other);
        bool IsSupersetOf(IImmutableIndexSet other);
        bool Overlaps(IImmutableIndexSet other);
        bool SetEquals(IImmutableIndexSet other);                
    }

    public interface IImmutableIndexSet<T>
    {
        int Max { get; }
        Func<int, T> Accessor { get; }
        IImmutableIndexSet Add(int index);
        IImmutableIndexSet Remove(int index);
        bool Contains(int index);
        IImmutableIndexSet<T> Clear();
        IImmutableIndexSet<T> Except(IImmutableIndexSet<T> other);
        IImmutableIndexSet<T> SymmetricExcept(IImmutableIndexSet<T> other);
        IImmutableIndexSet<T> Intersect(IImmutableIndexSet<T> other);
        IImmutableIndexSet<T> Union(IImmutableIndexSet<T> other);
        IImmutableIndexSet<T> Complement();
        bool IsSubsetOf(IImmutableIndexSet<T> other);
        bool IsSupersetOf(IImmutableIndexSet<T> other);
        bool Overlaps(IImmutableIndexSet<T> other);
        bool SetEquals(IImmutableIndexSet<T> other);
    }

    public class AllSet : IImmutableIndexSet
    {
        private readonly int _max;
        public int Max => _max;

        public int Count => _max;

        public AllSet(int max) => _max = max;
        
        public IImmutableIndexSet Add(int index) => this;

        public IImmutableIndexSet Clear() => new NoneSet(_max);

        public bool Contains(int index) => true;

        public IImmutableIndexSet Except(IImmutableIndexSet other) => other.Complement();

        public IImmutableIndexSet Intersect(IImmutableIndexSet other) => other;

        public bool IsSubsetOf(IImmutableIndexSet other)
        {
            if (other is AllSet)
                return true;
            else
                return false;
        }

        public bool IsSupersetOf(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool Overlaps(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Remove(int index)
        {
            throw new System.NotImplementedException();
        }

        public bool SetEquals(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Union(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Complement() => new NoneSet(_max);
    }

    public class NoneSet : IImmutableIndexSet
    {

        private readonly int _max;

        public int Max => _max;

        public int Count => 0;

        public NoneSet(int max) => _max = max;

        public IImmutableIndexSet Add(int index)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(int index)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Except(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Intersect(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSubsetOf(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSupersetOf(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool Overlaps(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Remove(int index)
        {
            throw new System.NotImplementedException();
        }

        public bool SetEquals(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Union(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Complement()
        {
            throw new System.NotImplementedException();
        }
    }

    public class IndexSet : IImmutableIndexSet
    {
        private readonly UintSet Included;

        public int Max => throw new System.NotImplementedException();

        public int Count => throw new System.NotImplementedException();

        public IImmutableIndexSet Add(int index)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(int index)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Except(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Intersect(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSubsetOf(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSupersetOf(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public bool Overlaps(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Remove(int index)
        {
            throw new System.NotImplementedException();
        }

        public bool SetEquals(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }

        public IImmutableIndexSet Union(IImmutableIndexSet other)
        {
            throw new System.NotImplementedException();
        }
    }
}
