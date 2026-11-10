using System.Collections.Generic;
using System.Linq;
using AutoClicker.Models;
using AutoClicker.Utils;
using Xunit;

namespace AutoClicker.Tests.Utils
{
    public class IntervalUtilsTests
    {
        private const int Samples = 10_000;

        private static AutoClickerSettings Settings(int hours, int minutes, int seconds, int milliseconds, int? offset = null)
        {
            return new AutoClickerSettings
            {
                Hours = hours,
                Minutes = minutes,
                Seconds = seconds,
                Milliseconds = milliseconds,
                IsRandomOffsetEnabled = offset.HasValue,
                RandomOffsetMilliseconds = offset ?? 0
            };
        }

        [Theory]
        [InlineData(0, 0, 0, 1, 1L)]
        [InlineData(1, 2, 3, 4, 3_723_004L)]
        [InlineData(597, 0, 0, 0, 2_149_200_000L)]
        [InlineData(1194, 0, 0, 0, 4_298_400_000L)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, 7_861_939_779_150_647L)]
        public void CalculateInterval_DoesNotOverflow(int hours, int minutes, int seconds, int milliseconds, long expected)
        {
            Assert.Equal(expected, IntervalUtils.CalculateInterval(Settings(hours, minutes, seconds, milliseconds)));
        }

        [Theory]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 0, 1, 0)]
        [InlineData(24, 0, 0, 0)]
        [InlineData(0, 1440, 0, 0)]
        [InlineData(0, 0, 0, Constants.MAX_INTERVAL_MILLISECONDS)]
        [InlineData(23, 59, 59, 999)]
        public void GetIntervalError_ValidInterval_ReturnsNull(int hours, int minutes, int seconds, int milliseconds)
        {
            Assert.Null(IntervalUtils.GetIntervalError(Settings(hours, minutes, seconds, milliseconds)));
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(24, 0, 0, 1)]
        [InlineData(597, 0, 0, 0)]
        [InlineData(1194, 0, 0, 0)]
        [InlineData(0, 35_792, 0, 0)]
        [InlineData(0, 0, 2_147_484, 0)]
        [InlineData(int.MaxValue, 0, 0, 0)]
        public void GetIntervalError_OutOfRange_ReturnsRangeError(int hours, int minutes, int seconds, int milliseconds)
        {
            Assert.Equal(Constants.INTERVAL_OUT_OF_RANGE_ERROR, IntervalUtils.GetIntervalError(Settings(hours, minutes, seconds, milliseconds)));
        }

        [Theory]
        [InlineData(-1, 0, 0, 3_600_001)]
        [InlineData(0, -1, 0, 100_000)]
        [InlineData(0, 0, -1, 2000)]
        [InlineData(0, 0, 1, -1)]
        public void GetIntervalError_NegativeField_ReturnsNegativeError(int hours, int minutes, int seconds, int milliseconds)
        {
            Assert.Equal(Constants.NEGATIVE_VALUE_ERROR, IntervalUtils.GetIntervalError(Settings(hours, minutes, seconds, milliseconds)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public void GetIntervalError_OffsetSmallerThanInterval_ReturnsNull(int offset)
        {
            Assert.Null(IntervalUtils.GetIntervalError(Settings(0, 0, 1, 0, offset)));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(5000)]
        public void GetIntervalError_OffsetNotSmallerThanInterval_ReturnsOffsetError(int offset)
        {
            Assert.Equal(Constants.OFFSET_TOO_LARGE_ERROR, IntervalUtils.GetIntervalError(Settings(0, 0, 1, 0, offset)));
        }

        [Fact]
        public void GetIntervalError_NegativeOffset_ReturnsNegativeError()
        {
            Assert.Equal(Constants.NEGATIVE_VALUE_ERROR, IntervalUtils.GetIntervalError(Settings(0, 0, 1, 0, -1)));
        }

        [Fact]
        public void GetIntervalError_OffsetDisabled_IgnoresOffset()
        {
            AutoClickerSettings settings = Settings(0, 0, 1, 0);
            settings.RandomOffsetMilliseconds = -1;

            Assert.Null(IntervalUtils.GetIntervalError(settings));
        }

        [Theory]
        [InlineData(nameof(AutoClickerSettings.Hours))]
        [InlineData(nameof(AutoClickerSettings.Minutes))]
        [InlineData(nameof(AutoClickerSettings.Seconds))]
        [InlineData(nameof(AutoClickerSettings.Milliseconds))]
        [InlineData(nameof(AutoClickerSettings.RandomOffsetMilliseconds))]
        public void SettingsDataError_NegativeField_ReturnsNegativeError(string propertyName)
        {
            AutoClickerSettings settings = Settings(-1, -1, -1, -1, -1);

            Assert.Equal(Constants.NEGATIVE_VALUE_ERROR, settings[propertyName]);
            Assert.Null(Settings(0, 0, 0, 0, 0)[propertyName]);
        }

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

        [Fact]
        public void GetRandomizedInterval_NearMaxValue_DoesNotOverflow()
        {
            for (int i = 0; i < Samples; i++)
            {
                Assert.InRange(IntervalUtils.GetRandomizedInterval(int.MaxValue - 10, 1000), int.MaxValue - 1010, int.MaxValue);
            }
        }
    }
}
