using Xunit;
using FluentAssertions;
using Kelson.Common.DataStructures.Text;
using System.Linq;

namespace Kelson.Common.DataStructures.Tests
{
    public class Catalog_Should
    {
        Catalog catalog => new Catalog("lorem ipsum dolor sit amet, consectetur");
        [Fact]
        public void ContainSequences()
        {
            catalog.Contains("abc").Should().BeFalse();
            catalog.Contains("or").Should().BeTrue();
        }

        [Fact]
        public void FindSequences()
        {
            catalog.Occurances("or").SequenceEqual(new int[] { 1, 16 }).Should().BeTrue();
        }
    }
}
