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

func DateFromDatetime(t time.Time) Date {
	return Date{time: t.UTC()}
}

func DateFromYMD(year int, month, day int) Date {
	return DateFromDatetime(time.Date(year, time.Month(month), day, 0, 0, 0, 0, time.UTC))
}

func DateFromYMDHMS(year int, month, day, hour, minute, second int) Date {
	return DateFromDatetime(time.Date(year, time.Month(month), day, hour, minute, second, 0, time.UTC))
}

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

func DateFromString(value string) (Date, error) {
	if dt, err := time.Parse(time.RFC3339Nano, value); err == nil {
		return DateFromDatetime(dt), nil
	}
	if d, err := time.Parse("2006-01-02", value); err == nil {
		return DateFromDatetime(d), nil
	}
	return Date{}, InvalidDateError{Value: value}
}

func DateNow() Date {
	return DateFromDatetime(time.Now().UTC())
}

func DateWithDurationFromNow(duration time.Duration) Date {
	return DateFromDatetime(time.Now().UTC().Add(duration))
}

func (d Date) Datetime() time.Time {
	return d.time
}

func (d Date) Timestamp() float64 {
	sec := d.time.Unix()
	nsec := d.time.Nanosecond()
	return float64(sec) + float64(nsec)/1_000_000_000.0
}

func (d Date) String() string {
	if d.time.Nanosecond() == 0 {
		return d.time.Format(time.RFC3339)
	}
	return d.time.Format(time.RFC3339Nano)
}

func (d Date) UntaggedCBOR() CBOR {
	c, _ := FromAny(d.Timestamp())
	return c
}

func (d Date) TaggedCBOR() CBOR {
	return NewCBORTagged(TagWithValue(TAG_DATE), d.UntaggedCBOR())
}

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
