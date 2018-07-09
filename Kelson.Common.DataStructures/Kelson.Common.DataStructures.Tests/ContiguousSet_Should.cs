using FluentAssertions;
using Kelson.Common.DataStructures.Sets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Kelson.Common.DataStructures.Tests
{
    public class ContiguousSet_Should
    {
        [Fact]
        public void ContainItems()
        {
            var set = new ContiguousSet(0, 256) { 3, 112, 160, 193, 204 };
            for (int i = 0; i < 256; i++)
                if (i == 3 | i == 112 | i == 160 | i == 193 | i == 204)
                    set.Contains(i).Should().BeTrue();
                else
                    set.Contains(i).Should().BeFalse();
        }


        [Fact]
        public void EnumerateItems()
        {
            var set = new ContiguousSet(0, 256) { 3, 4, 250 };

            set.SequenceEqual(new int[] { 3, 4, 250 }).Should().BeTrue();
        }

        [Fact]
        public void OffsetIndecies()
        {
            var set = new ContiguousSet(-112, 5) { -112, -110 };
            for (int i = -112; i < -107; i++)
                if (i == -112 | i == -110)
                    set.Contains(i).Should().BeTrue();
                else
                    set.Contains(i).Should().BeFalse();
        }
    }
}
