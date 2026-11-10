using System;
using AutoClicker.Models;

namespace AutoClicker.Utils
{
    public static class IntervalUtils
    {
        public static long CalculateInterval(AutoClickerSettings settings)
        {
            return settings.Milliseconds
                + (settings.Seconds * 1000L)
                + (settings.Minutes * 60L * 1000)
                + (settings.Hours * 60L * 60 * 1000);
        }

        public static string GetIntervalError(AutoClickerSettings settings)
        {
            if (settings.Hours < 0 || settings.Minutes < 0 || settings.Seconds < 0 || settings.Milliseconds < 0)
                return Constants.NEGATIVE_VALUE_ERROR;

            long interval = CalculateInterval(settings);
            if (interval < 1 || interval > Constants.MAX_INTERVAL_MILLISECONDS)
                return Constants.INTERVAL_OUT_OF_RANGE_ERROR;

            if (settings.IsRandomOffsetEnabled)
            {
                if (settings.RandomOffsetMilliseconds < 0)
                    return Constants.NEGATIVE_VALUE_ERROR;
                if (settings.RandomOffsetMilliseconds >= interval)
                    return Constants.OFFSET_TOO_LARGE_ERROR;
            }

            return null;
        }

        public static int GetRandomizedInterval(int interval, int offset)
        {
            long randomizedInterval = interval + Random.Shared.NextInt64(-offset, offset + 1L);
            return (int)Math.Clamp(randomizedInterval, 1, int.MaxValue);
        }
    }
}
