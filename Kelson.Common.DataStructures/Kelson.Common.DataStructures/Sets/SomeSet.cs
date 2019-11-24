using System;

namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// Represents a selction 1 to Max-1 unsigned integers in the range [0, Max]
    /// </summary>
    public readonly struct SomeSet : IImmutableIndexSet
    {
        private readonly UintSet Included;

        private readonly uint _max;
        public uint Max => _max;

        public uint Count => (uint)Included.Count;

        public SomeSet(uint max, UintSet included) => (Included, _max) = (included, max);

        public SomeSet(uint max, UintSet included, UintSet excluded) => (Included, _max) = (included.Except(excluded), max);

        public SomeSet(uint max, params uint[] excluded) => (Included, _max) = (UintSet.All(max).Except(new UintSet(excluded)), max);

        public IImmutableIndexSet CheckAllOrNone
        {
            get
            {
                var count = Count;
                if (count == Max)
                    return new AllSet(_max);
                else if (count == 0)
                    return new NoneSet(_max);
                else
                    return this;
            }
        }

        public IImmutableIndexSet Add(uint index) => new SomeSet(_max, Included.Add(index)).CheckAllOrNone;

        public IImmutableIndexSet Clear() => new NoneSet();

        public IImmutableIndexSet Complement() => new SomeSet(_max, included: UintSet.All(_max), excluded: Included).CheckAllOrNone;

        public bool Contains(uint index) => Included.Contains(index);

        public IImmutableIndexSet Except(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return new SomeSet(_max, included: Included.Except(some.Included)).CheckAllOrNone;
            else if (other is NoneSet)
                return this;
            else if (other is AllSet)
                return new NoneSet();            
            else
                return throwSetTypeException<IImmutableIndexSet>();
        }

        public IImmutableIndexSet Intersect(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return new SomeSet(_max, included: Included.Intersect(some.Included)).CheckAllOrNone;
            if (other is NoneSet none)
                return none;
            else if (other is AllSet)
                return this;            
            else
                return throwSetTypeException<IImmutableIndexSet>();
        }

        public bool IsSubsetOf(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return Included.IsSubsetOf(some.Included);
            else if (other is NoneSet)
                return false;
            else if (other is AllSet)
                return true;            
            else
                return throwSetTypeException<bool>();
        }

        public bool IsSupersetOf(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return Included.IsSupersetOf(some.Included);
            else if (other is NoneSet)
                return true;
            else if (other is AllSet)
                return false;            
            else
                return throwSetTypeException<bool>();
        }

        public bool Overlaps(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return Included.Overlaps(some.Included);
            else if (other is NoneSet)
                return false;
            else if (other is AllSet)
                return true;            
            else
                return throwSetTypeException<bool>();
        }

        public IImmutableIndexSet Remove(uint index) => new SomeSet(_max, Included.Remove(index)).CheckAllOrNone;        

        public bool SetEquals(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return Included.SetEquals(some.Included);
            if (other is NoneSet || other is AllSet)
                return false;            
            else
                return throwSetTypeException<bool>();
        }

        public IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return new SomeSet(_max, included: Included.SymmetricExcept(some.Included)).CheckAllOrNone;
            else if (other is NoneSet)
                return this;
            else if (other is AllSet)
                return new NoneSet(_max);           
            else
                return throwSetTypeException<IImmutableIndexSet>();
        }

        public IImmutableIndexSet Union(IImmutableIndexSet other)
        {
            if (other is SomeSet some)
                return new SomeSet(_max, included: Included.Union(some.Included)).CheckAllOrNone;
            else if (other is NoneSet)
                return this;
            else if (other is AllSet all)
                return all;            
            else
                return throwSetTypeException<IImmutableIndexSet>();
        }

        private static T throwSetTypeException<T>() => throw new InvalidOperationException($"Only 3 types should implement {nameof(IImmutableIndexSet)}: {nameof(AllSet)}, {nameof(NoneSet)} and {nameof(SomeSet)}.");
    }
}
