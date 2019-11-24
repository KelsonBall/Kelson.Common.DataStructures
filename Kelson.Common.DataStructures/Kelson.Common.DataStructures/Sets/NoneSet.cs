namespace Kelson.Common.DataStructures.Sets
{
    /// <summary>
    /// Represents a selction exactly 0 unsigned integers in the range [0, Max]
    /// </summary>
    public readonly struct NoneSet : IImmutableIndexSet
    {
        private readonly uint _max;
        public uint Max => _max;
        public uint Count => 0;

        public NoneSet(uint max) => _max = max;

        public IImmutableIndexSet Add(uint index) => new SomeSet(_max, included: new UintSet().Add(index));

        public IImmutableIndexSet Clear() => this;

        public bool Contains(uint index) => false;

        public IImmutableIndexSet Except(IImmutableIndexSet other) => this;

        public IImmutableIndexSet Intersect(IImmutableIndexSet other) => this;

        public bool IsSubsetOf(IImmutableIndexSet other) => true;

        public bool IsSupersetOf(IImmutableIndexSet other) => other is NoneSet;

        public bool Overlaps(IImmutableIndexSet other) => false;

        public IImmutableIndexSet Remove(uint index) => this;

        public bool SetEquals(IImmutableIndexSet other) => other is NoneSet;

        public IImmutableIndexSet SymmetricExcept(IImmutableIndexSet other) => other;

        public IImmutableIndexSet Union(IImmutableIndexSet other) => other;

        public IImmutableIndexSet Complement() => new AllSet(_max);
    }
}
