using System;

namespace uOSC
{

public struct Timestamp
{
    public UInt64 value;

    public Timestamp(UInt64 value)
    {
        this.value = value;
    }

    public static readonly Timestamp Immediate = new Timestamp(0x1u);

    public static Timestamp Now
    {
        get { return Timestamp.CreateFromDateTime(DateTime.UtcNow); }
    }

    public static Timestamp CreateFromDateTime(DateTime time)
    {
        var span = time - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var msec = (UInt64)span.TotalMilliseconds;
        var integerPart = msec / 1000;
        var decimalPart = ((msec % 1000) * 0x100000000L) / 1000;
        var timestamp = ((UInt64)integerPart << 32) | (UInt64)decimalPart;
        return new Timestamp(timestamp);
    }

    public DateTime ToUtcTime()
    {
        var integerPart = (UInt64)((value >> 32) & 0xFFFFFFFF); 
        var decimalPart = (UInt64)(value & 0xFFFFFFFF);
        var msec = (integerPart * 1000) + ((decimalPart * 1000) / 0x100000000L);
        var baseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return baseDate.AddMilliseconds(msec);
    }

    public DateTime ToLocalTime()
    {
        return TimeZoneInfo.ConvertTime(ToUtcTime(), TimeZoneInfo.Local);
    }
}

}
