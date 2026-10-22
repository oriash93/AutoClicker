using System.Collections.Generic;
using System.Linq;
using AutoClicker.Utils;
using Xunit;

namespace AutoClicker.Tests.Utils
{
    public class IntervalUtilsTests
    {
        private const int Samples = 10_000;

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(2000)]
        public void GetRandomizedInterval_WithoutOffset_ReturnsInterval(int interval)
        {
            Assert.Equal(interval, IntervalUtils.GetRandomizedInterval(interval, 0));
        }

        [Theory]
        [InlineData(100, 20)]
        [InlineData(1000, 500)]
        [InlineData(2000, 1999)]
        public void GetRandomizedInterval_StaysWithinOffset(int interval, int offset)
        {
            for (int i = 0; i < Samples; i++)
            {
                int result = IntervalUtils.GetRandomizedInterval(interval, offset);
                Assert.InRange(result, interval - offset, interval + offset);
            }
        }

        [Fact]
        public void GetRandomizedInterval_ReachesBothEnds()
        {
            HashSet<int> results = Enumerable.Range(0, Samples)
                .Select(_ => IntervalUtils.GetRandomizedInterval(100, 1))
                .ToHashSet();

            Assert.Equal(new HashSet<int> { 99, 100, 101 }, results);
        }

        [Fact]
        public void GetRandomizedInterval_NeverReturnsLessThanOne()
        {
            for (int i = 0; i < Samples; i++)
            {
                Assert.True(IntervalUtils.GetRandomizedInterval(1, 5) >= 1);
            }
        }
    }
}
