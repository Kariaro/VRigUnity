using System;

namespace MiniNtp
{
    public static class TimeConstants
    {
        public static readonly DateTime Epoch1900 = DateTime.Parse("1900-01-01 00:00:00.000");
        public static readonly DateTime Epoch2036 = DateTime.Parse("2036-02-07 06:28:15");
        
        public const int TicksPerSecond = (int) TimeSpan.TicksPerMillisecond * 1000;
        public const uint TimestampFractionsPerMs = 5000000;
        public const double FractionMillisecondMultiplier = 200 / (double) 1000000000;
    }
}