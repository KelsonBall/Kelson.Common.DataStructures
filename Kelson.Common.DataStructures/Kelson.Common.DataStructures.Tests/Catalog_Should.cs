using Xunit;
using FluentAssertions;
using Kelson.Common.DataStructures.Text;
using System.Linq;

namespace Kelson.Common.DataStructures.Tests
{
    public class Catalog_Should
    {                                 //0123456789012345678901234567890123456789  
        SubstringCollection catalog => new SubstringCollection("lorem ipsum dolor sit amet, consectetur");
        [Fact]
        public void ContainSequences()
        {
            catalog.Contains("abc").Should().BeFalse();
            catalog.Contains("or").Should().BeTrue();
        }

        [Fact]
        public void FindSequences()
        {
            catalog.Occurances("or").Should().BeEquivalentTo(new uint[] { 1, 15 });
            catalog.Occurances("ore").Should().BeEquivalentTo(new uint[] { 1 });
            catalog.Occurances("ogre").Should().BeEquivalentTo(new uint[0]);
            catalog.Occurances("consectetur").Should().BeEquivalentTo(new uint[] { 28 });
        }
    }
}
