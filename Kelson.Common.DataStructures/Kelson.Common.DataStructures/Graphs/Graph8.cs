using Kelson.Common.DataStructures.Sets;
using System.Collections.Generic;

namespace Kelson.Common.DataStructures.Graphs
{
    public interface IGraph
    {
        int Count { get; }

        bool this[int node_from, int node_to] { get; }
        ISet<int> this[int node] { get; }

        IGraph WithEdge(int node_from, int node_to);
        IGraph WithoutEdge(int node_from, int node_to);

        /// <summary>
        /// Return a new graph with all current edges as non-edges, and all current non-edges as edges (unary not)
        /// </summary>
        IGraph Inverse();

        /// <summary>
        /// Reverse the direction of all edges (unary -)
        /// </summary>
        IGraph Transpose();

        /// <summary>
        /// Return a new graph that only has edges present in both graphs (binary &)
        /// </summary>
        IGraph Intersect(IGraph other);

        /// <summary>
        /// Return a new graph that has all edges from both graphs (binary |)
        /// </summary>
        IGraph Union(IGraph other);

        /// <summary>
        /// Return a new graph that has all edges present in exactly 1 of the graphs (binary ^)
        /// </summary>
        IGraph DisjunctiveUnion(IGraph other);
    }

    /// <summary>
    /// A graph descriptor for undirected graphs with
    /// </summary>
    public readonly struct Graph8 : IGraph
    {
        private readonly ImmutableSet64 data;

        public ISet<int> this[int node] => throw new System.NotImplementedException();

        public bool this[int node_from, int node_to] => throw new System.NotImplementedException();

        public int Count => throw new System.NotImplementedException();

        public IGraph DisjunctiveUnion(IGraph other)
        {
            throw new System.NotImplementedException();
        }

        public IGraph Intersect(IGraph other)
        {
            throw new System.NotImplementedException();
        }

        public IGraph Inverse()
        {
            throw new System.NotImplementedException();
        }

        public IGraph Transpose()
        {
            throw new System.NotImplementedException();
        }

        public IGraph Union(IGraph other)
        {
            throw new System.NotImplementedException();
        }

        public IGraph WithEdge(int node_from, int node_to)
        {
            throw new System.NotImplementedException();
        }

        public IGraph WithoutEdge(int node_from, int node_to)
        {
            throw new System.NotImplementedException();
        }
    }
}
