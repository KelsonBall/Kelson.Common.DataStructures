using System;

namespace Kelson.Common.DataStructures.Sets
{
    public interface IImmutableIndexSet
    {
        uint Max { get; }
        uint Count { get; }
        IImmutableIndexSet Add(uint index);
        IImmutableIndexSet Remove(uint index);
        bool Contains(uint index);
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

    public interface IImmutableIndexSet<T> : IImmutableIndexSet
    {       
        Func<uint, T> Accessor { get; }        
    }
}
