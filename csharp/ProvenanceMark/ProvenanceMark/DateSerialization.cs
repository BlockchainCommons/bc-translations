using System.Buffers.Binary;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Resolution-dependent compact date serialization helpers.
/// </summary>
public static class DateSerialization
{
    private static readonly DateTimeOffset ReferenceDate =
        new(2001, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private const ulong MaxSixByteMilliseconds = 0xe5940a78a7ffUL;

    public static byte[] Serialize2Bytes(CborDate date)
    {
        var utc = date.DateTimeValue.ToUniversalTime();
        var year = utc.Year;
        var month = utc.Month;
        var day = utc.Day;

        var yearOffset = year - 2023;
        if (yearOffset is < 0 or >= 128)
        {
            throw ProvenanceMarkException.YearOutOfRange(year);
        }

        if (month is < 1 or > 12 || day is < 1 or > 31)
        {
            throw ProvenanceMarkException.InvalidMonthOrDay(year, month, day);
        }

        var value = ((yearOffset << 9) | (month << 5) | day) & 0xffff;
        return
        [
            (byte)((value >> 8) & 0xff),
            (byte)(value & 0xff)
        ];
    }

    public static CborDate Deserialize2Bytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 2)
        {
            throw new ArgumentException("2-byte date requires exactly 2 bytes", nameof(bytes));
        }

        var value = (bytes[0] << 8) | bytes[1];
        var day = value & 0b11111;
        var month = (value >> 5) & 0b1111;
        var year = ((value >> 9) & 0b1111111) + 2023;

        if (month is < 1 or > 12 || day < 1 || day > RangeOfDaysInMonth(year, month))
        {
            throw ProvenanceMarkException.InvalidMonthOrDay(year, month, day);
        }

        try
        {
            return CborDate.FromYmdHms(year, month, day, 0, 0, 0);
        }
        catch (Exception)
        {
            throw ProvenanceMarkException.InvalidDate(
                $"Cannot construct date {year:D4}-{month:D2}-{day:D2}");
        }
    }

    public static byte[] Serialize4Bytes(CborDate date)
    {
        var seconds = checked((long)(date.DateTimeValue.ToUniversalTime() - ReferenceDate).TotalSeconds);
        if (seconds < 0 || seconds > uint.MaxValue)
        {
            throw ProvenanceMarkException.DateOutOfRange("seconds value too large for u32");
        }

        var data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, (uint)seconds);
        return data;
    }

    public static CborDate Deserialize4Bytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 4)
        {
            throw new ArgumentException("4-byte date requires exactly 4 bytes", nameof(bytes));
        }

        var seconds = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        return CborDate.FromDateTime(ReferenceDate.AddSeconds(seconds));
    }

    public static byte[] Serialize6Bytes(CborDate date)
    {
        var utc = date.DateTimeValue.ToUniversalTime();
        var milliseconds = utc.ToUnixTimeMilliseconds() - ReferenceDate.ToUnixTimeMilliseconds();
        if (milliseconds < 0)
        {
            throw ProvenanceMarkException.DateOutOfRange("milliseconds value too large for u64");
        }

        var value = (ulong)milliseconds;
        if (value > MaxSixByteMilliseconds)
        {
            throw ProvenanceMarkException.DateOutOfRange("date exceeds maximum representable value");
        }

        var full = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(full, value);
        return full[2..];
    }

    public static CborDate Deserialize6Bytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 6)
        {
            throw new ArgumentException("6-byte date requires exactly 6 bytes", nameof(bytes));
        }

        var full = new byte[8];
        bytes.CopyTo(full.AsSpan(2));
        var value = BinaryPrimitives.ReadUInt64BigEndian(full);
        if (value > MaxSixByteMilliseconds)
        {
            throw ProvenanceMarkException.DateOutOfRange("date exceeds maximum representable value");
        }

        return CborDate.FromDateTime(ReferenceDate.AddMilliseconds(value));
    }

    public static int RangeOfDaysInMonth(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }
}
