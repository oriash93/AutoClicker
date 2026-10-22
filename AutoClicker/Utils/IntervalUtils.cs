using System;

namespace AutoClicker.Utils
{
    public static class IntervalUtils
    {
        public static int GetRandomizedInterval(int interval, int offset)
        {
            int randomizedInterval = interval + Random.Shared.Next(-offset, offset + 1);
            return Math.Max(randomizedInterval, 1);
        }
    }
}
