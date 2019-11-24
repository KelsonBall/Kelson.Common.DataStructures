namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// Represents a selction of exactly Max unsigned integers in the range [0, Max]
    /// </summary>
    public readonly struct AllSet : IImmutableIndexSet
    {
        private readonly uint _max;
        public uint Max => _max;

        public uint Count => _max;

        public AllSet(uint max) => _max = max;
        
        public IImmutableIndexSet Add(uint index) => this;

        public IImmutableIndexSet Clear() => new NoneSet(_max);

        public bool Contains(uint index) => true;

        public IImmutableIndexSet Except(IImmutableIndexSet other) => other.Complement();

        public IImmutableIndexSet Intersect(IImmutableIndexSet other) => other;

        public bool IsSubsetOf(IImmutableIndexSet other) => other is AllSet;

        public bool IsSupersetOf(IImmutableIndexSet other) => true;        

        public bool Overlaps(IImmutableIndexSet other) => true;

        public IImmutableIndexSet Remove(uint index) => new SomeSet(max: _max, excluded: index);

        public bool SetEquals(IImmutableIndexSet other) => other is AllSet;

        public IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other) => new NoneSet(_max);

        public IImmutableIndexSet Union(IImmutableIndexSet other) => other;

        public IImmutableIndexSet Complement() => new NoneSet(_max);
    }
}
