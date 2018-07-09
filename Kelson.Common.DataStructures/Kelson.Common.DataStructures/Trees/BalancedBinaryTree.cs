using System;

namespace Kelson.Common.DataStructures.Trees
{
    public class BalancedBinaryTree<T>
    {
        private readonly Func<T, T, int> compare;

        private BalancedBinaryTree(Func<T, T, int> comparison) => compare = comparison;
    }
}
