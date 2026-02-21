using System.Globalization;

namespace BlockchainCommons.DCbor;

/// <summary>
/// A CBOR-friendly date/time representation. Wraps <see cref="DateTimeOffset"/>
/// and supports encoding/decoding with CBOR tag 1 (epoch seconds).
/// </summary>
public readonly struct CborDate : IEquatable<CborDate>, IComparable<CborDate>
{
    private readonly DateTimeOffset _value;

    public CborDate(DateTimeOffset value) { _value = value; }

    public DateTimeOffset DateTimeValue => _value;

    // --- Constructors ---

    public static CborDate FromDateTime(DateTimeOffset dt) => new(dt);

    public static CborDate FromYmd(int year, int month, int day)
    {
        return new CborDate(new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero));
    }

    public static CborDate FromYmdHms(int year, int month, int day, int hour, int minute, int second)
    {
        return new CborDate(new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero));
    }

    public static CborDate FromTimestamp(double secondsSinceEpoch)
    {
        long wholeSeconds = (long)Math.Truncate(secondsSinceEpoch);
        double frac = secondsSinceEpoch - Math.Truncate(secondsSinceEpoch);
        long ticks = (long)(frac * TimeSpan.TicksPerSecond);
        var dto = DateTimeOffset.UnixEpoch.AddSeconds(wholeSeconds).AddTicks(ticks);
        return new CborDate(dto);
    }

    public static CborDate FromString(string value)
    {
        // Try RFC 3339 / ISO 8601 with time
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var dto))
        {
            return new CborDate(dto.ToUniversalTime());
        }

        // Try date-only
        if (System.DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateOnly))
        {
            return new CborDate(new DateTimeOffset(dateOnly, TimeSpan.Zero));
        }

        throw new CborInvalidDateException("Invalid date string");
    }

    public static CborDate Now() => new(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a CborDate offset from the current time by the given duration.
    /// </summary>
    public static CborDate WithDurationFromNow(TimeSpan duration)
    {
        return new CborDate(DateTimeOffset.UtcNow.Add(duration));
    }

    // --- Properties ---

    public double Timestamp
    {
        get
        {
            double wholeSecs = (double)(_value - DateTimeOffset.UnixEpoch).Ticks / TimeSpan.TicksPerSecond;
            return wholeSecs;
        }
    }

    // --- CBOR Tagged Encoding ---

    public static IReadOnlyList<Tag> CborTags() =>
        GlobalTags.TagsForValues(BlockchainCommons.DCbor.CborTags.TagDate);

    public Cbor UntaggedCbor() => Cbor.FromDouble(Timestamp);

    public Cbor TaggedCbor()
    {
        var tags = CborTags();
        return Cbor.ToTaggedValue(tags[0], UntaggedCbor());
    }

    public static CborDate FromUntaggedCbor(Cbor cbor)
    {
        double ts = cbor.TryIntoDouble();
        return FromTimestamp(ts);
    }

    public static CborDate FromTaggedCbor(Cbor cbor)
    {
        var tags = CborTags();
        // Try each tag
        foreach (var tag in tags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException) { }
            catch (CborWrongTypeException) { }
        }
        throw new CborWrongTypeException();
    }

    // --- Conversions ---

    public static implicit operator Cbor(CborDate date) => date.TaggedCbor();

    // --- Display ---

    public override string ToString()
    {
        if (_value.Hour == 0 && _value.Minute == 0 && _value.Second == 0)
            return _value.ToString("yyyy-MM-dd");
        return _value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    }

    // --- Equality & Comparison ---

    public bool Equals(CborDate other) => _value.Equals(other._value);
    public override bool Equals(object? obj) => obj is CborDate d && Equals(d);
    public override int GetHashCode() => _value.GetHashCode();
    public int CompareTo(CborDate other) => _value.CompareTo(other._value);

    public static bool operator ==(CborDate a, CborDate b) => a.Equals(b);
    public static bool operator !=(CborDate a, CborDate b) => !a.Equals(b);
}
