using Xunit;
using FluentAssertions;
using Kelson.Common.DataStructures.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kelson.Common.DataStructures.Tests
{
    public class SubtringCollection_Should
    {                                                         //0123456789012345678901234567890123456789  
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

            catalog.Occurances("ur", catalog.Occurances("et") >> 2).Should().BeEquivalentTo(new uint[] { 37 });
        }
    }

    public class Catalog_Should
    {
        class Item { public string Text; }

        [Fact]
        public async Task FindItemsBySubstring()
        {
            var catalog = new Catalog<Item>(new Dictionary<int, Item>
            {
                { 1, new Item { Text = "Hello World" } },
                { 2, new Item { Text = "aaaaaaaaaaa" } },
                { 3, new Item { Text = "abbaabbaabb" } },
                { 4, new Item { Text = "lorem ipsum" } }
            }, i => i.Text);

            (await catalog.AllContaining("or"))
                .Should()
                .BeEquivalentTo(new uint[] { 1, 4 });
            (await catalog.AllContaining("aa"))
                .Should()
                .BeEquivalentTo(new uint[] { 2, 3 });
            (await catalog.AllContaining("aaaa"))
                .Should()
                .BeEquivalentTo(new uint[] { 2 });
            (await catalog.AllContaining("aa"))
                .Intersect(await catalog.AllContaining("bb"))
                .Should()
                .BeEquivalentTo(new uint[] { 3 });
        }
    }
}
