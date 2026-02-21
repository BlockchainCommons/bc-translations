package dcbor

import (
	"fmt"
	"math"
	"time"
)

// Date wraps UTC time for dCBOR tag 1 semantics.
type Date struct {
	time time.Time
}

// DateFromDatetime constructs a date from a timestamp, normalized to UTC.
func DateFromDatetime(t time.Time) Date {
	return Date{time: t.UTC()}
}

// DateFromYMD constructs a date from year/month/day at midnight UTC.
func DateFromYMD(year int, month, day int) Date {
	return DateFromDatetime(time.Date(year, time.Month(month), day, 0, 0, 0, 0, time.UTC))
}

// DateFromYMDHMS constructs a date from year/month/day/hour/minute/second in UTC.
func DateFromYMDHMS(year int, month, day, hour, minute, second int) Date {
	return DateFromDatetime(time.Date(year, time.Month(month), day, hour, minute, second, 0, time.UTC))
}

// DateFromTimestamp constructs a date from seconds since Unix epoch.
func DateFromTimestamp(secondsSinceUnixEpoch float64) Date {
	sec := int64(math.Floor(secondsSinceUnixEpoch))
	frac := secondsSinceUnixEpoch - float64(sec)
	nsec := int64(math.Round(frac * 1_000_000_000.0))
	if nsec >= 1_000_000_000 {
		sec++
		nsec -= 1_000_000_000
	}
	if nsec < 0 {
		sec--
		nsec += 1_000_000_000
	}
	return DateFromDatetime(time.Unix(sec, nsec))
}

// DateFromString parses an ISO-8601 string in RFC3339 or date-only form.
func DateFromString(value string) (Date, error) {
	if dt, err := time.Parse(time.RFC3339Nano, value); err == nil {
		return DateFromDatetime(dt), nil
	}
	if d, err := time.Parse("2006-01-02", value); err == nil {
		return DateFromDatetime(d), nil
	}
	return Date{}, InvalidDateError{Value: value}
}

// DateNow returns the current UTC date/time.
func DateNow() Date {
	return DateFromDatetime(time.Now().UTC())
}

// DateWithDurationFromNow returns the current UTC time plus the given duration.
func DateWithDurationFromNow(duration time.Duration) Date {
	return DateFromDatetime(time.Now().UTC().Add(duration))
}

// Datetime returns the wrapped UTC time value.
func (d Date) Datetime() time.Time {
	return d.time
}

// Equal reports instant equality between two dates.
func (d Date) Equal(other Date) bool {
	return d.time.Equal(other.time)
}

// Timestamp returns seconds since Unix epoch as float64.
func (d Date) Timestamp() float64 {
	sec := d.time.Unix()
	nsec := d.time.Nanosecond()
	return float64(sec) + float64(nsec)/1_000_000_000.0
}

// AddSeconds returns a date shifted forward by the given seconds.
func (d Date) AddSeconds(seconds float64) Date {
	return DateFromTimestamp(d.Timestamp() + seconds)
}

// SubSeconds returns a date shifted backward by the given seconds.
func (d Date) SubSeconds(seconds float64) Date {
	return DateFromTimestamp(d.Timestamp() - seconds)
}

// AddDuration returns a date shifted forward by the given duration.
func (d Date) AddDuration(duration time.Duration) Date {
	return DateFromDatetime(d.time.Add(duration))
}

// SubDuration returns a date shifted backward by the given duration.
func (d Date) SubDuration(duration time.Duration) Date {
	return DateFromDatetime(d.time.Add(-duration))
}

// DiffSeconds returns `d - other` in seconds.
func (d Date) DiffSeconds(other Date) float64 {
	return d.Timestamp() - other.Timestamp()
}

// String returns RFC3339 text, or date-only text for midnight UTC values.
func (d Date) String() string {
	if d.time.Hour() == 0 && d.time.Minute() == 0 && d.time.Second() == 0 {
		return d.time.Format("2006-01-02")
	}
	return d.time.Format(time.RFC3339)
}

// CBORTags returns accepted CBOR tags for date values.
func (d Date) CBORTags() []Tag {
	return TagsForValues([]TagValue{TAG_DATE})
}

// ToCBOR returns the tagged CBOR representation.
func (d Date) ToCBOR() CBOR {
	return d.TaggedCBOR()
}

// UntaggedCBOR returns the date payload CBOR without tag wrapper.
func (d Date) UntaggedCBOR() CBOR {
	c, _ := FromAny(d.Timestamp())
	return c
}

// UntaggedCBORData returns encoded bytes for the untagged CBOR payload.
func (d Date) UntaggedCBORData() []byte {
	return d.UntaggedCBOR().ToCBORData()
}

// TaggedCBOR returns the tagged CBOR representation.
func (d Date) TaggedCBOR() CBOR {
	tags := d.CBORTags()
	if len(tags) == 0 {
		return NewCBORTagged(TagWithValue(TAG_DATE), d.UntaggedCBOR())
	}
	return NewCBORTagged(tags[0], d.UntaggedCBOR())
}

// TaggedCBORData returns encoded bytes for the tagged representation.
func (d Date) TaggedCBORData() []byte {
	return d.TaggedCBOR().ToCBORData()
}

// DateFromUntaggedCBOR decodes a date from an untagged CBOR payload.
func DateFromUntaggedCBOR(cbor CBOR) (Date, error) {
	if unsigned, ok := cbor.AsUnsigned(); ok {
		return DateFromTimestamp(float64(unsigned)), nil
	}
	if n, ok := cbor.AsInt64(); ok {
		return DateFromTimestamp(float64(n)), nil
	}
	if f, ok := cbor.AsFloat64(); ok {
		return DateFromTimestamp(f), nil
	}
	if s, ok := cbor.AsText(); ok {
		return DateFromString(s)
	}
	return Date{}, fmt.Errorf("%w: date untagged value", ErrWrongType)
}

// DateFromTaggedCBOR decodes a date from a tag-1 CBOR value.
func DateFromTaggedCBOR(cbor CBOR) (Date, error) {
	tag, value, ok := cbor.AsTaggedValue()
	if !ok {
		return Date{}, ErrWrongType
	}
	if tag.Value() != TAG_DATE {
		return Date{}, WrongTagError{Expected: TagWithValue(TAG_DATE), Actual: tag}
	}
	return DateFromUntaggedCBOR(value)
}

// DateFromTaggedCBORData decodes a date from tagged CBOR bytes.
func DateFromTaggedCBORData(data []byte) (Date, error) {
	cbor, err := TryFromData(data)
	if err != nil {
		return Date{}, err
	}
	return DateFromTaggedCBOR(cbor)
}

// DateFromUntaggedCBORData decodes a date from untagged CBOR bytes.
func DateFromUntaggedCBORData(data []byte) (Date, error) {
	cbor, err := TryFromData(data)
	if err != nil {
		return Date{}, err
	}
	return DateFromUntaggedCBOR(cbor)
}
