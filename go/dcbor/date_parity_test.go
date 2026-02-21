package dcbor

import (
	"errors"
	"math"
	"testing"
	"time"
)

func TestDateConstructorsAndParsingParity(t *testing.T) {
	if got, want := DateFromYMD(2021, 2, 24).String(), "2021-02-24"; got != want {
		t.Fatalf("DateFromYMD mismatch: got %q want %q", got, want)
	}
	if got, want := DateFromYMDHMS(2021, 2, 24, 12, 34, 56).String(), "2021-02-24T12:34:56Z"; got != want {
		t.Fatalf("DateFromYMDHMS mismatch: got %q want %q", got, want)
	}

	parsedDateOnly, err := DateFromString("2021-02-24")
	if err != nil {
		t.Fatalf("DateFromString(date-only) failed: %v", err)
	}
	if got, want := parsedDateOnly.String(), "2021-02-24"; got != want {
		t.Fatalf("parsed date-only mismatch: got %q want %q", got, want)
	}

	parsedDateTime, err := DateFromString("2021-02-24T12:34:56Z")
	if err != nil {
		t.Fatalf("DateFromString(datetime) failed: %v", err)
	}
	if got, want := parsedDateTime.String(), "2021-02-24T12:34:56Z"; got != want {
		t.Fatalf("parsed datetime mismatch: got %q want %q", got, want)
	}

	if _, err := DateFromString("not-a-date"); err == nil {
		t.Fatalf("expected invalid-date error")
	} else {
		var invalid InvalidDateError
		if !errors.As(err, &invalid) {
			t.Fatalf("expected InvalidDateError, got %T", err)
		}
	}
}

func TestDateTimestampRoundTripParity(t *testing.T) {
	tests := []float64{
		0,
		0.5,
		-0.5,
		-100.0,
		1647887071.0,
		1675854714.25,
	}

	for _, ts := range tests {
		date := DateFromTimestamp(ts)
		got := date.Timestamp()
		if math.Abs(got-ts) > 1e-9 {
			t.Fatalf("timestamp round-trip mismatch for %v: got %.12f want %.12f", ts, got, ts)
		}
	}
}

func TestDateTaggedDecodeErrorParity(t *testing.T) {
	_, err := DateFromTaggedCBOR(ToTaggedValue(TagWithValue(2), MustFromAny("2021-02-24")))
	if err == nil {
		t.Fatalf("expected WrongTagError")
	}
	if _, ok := err.(WrongTagError); !ok {
		t.Fatalf("expected WrongTagError, got %T", err)
	}

	_, err = DateFromUntaggedCBOR(NewCBORMap(NewMap()))
	if !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for invalid untagged date payload, got %v", err)
	}
}

func TestDateArithmeticParity(t *testing.T) {
	base := DateFromTimestamp(1000.25)

	if got, want := base.AddSeconds(10.5).Timestamp(), 1010.75; math.Abs(got-want) > 1e-9 {
		t.Fatalf("AddSeconds mismatch: got %.12f want %.12f", got, want)
	}
	if got, want := base.SubSeconds(0.25).Timestamp(), 1000.0; math.Abs(got-want) > 1e-9 {
		t.Fatalf("SubSeconds mismatch: got %.12f want %.12f", got, want)
	}

	if got, want := base.AddDuration(1500*time.Millisecond).Timestamp(), 1001.75; math.Abs(got-want) > 1e-9 {
		t.Fatalf("AddDuration mismatch: got %.12f want %.12f", got, want)
	}
	if got, want := base.SubDuration(250*time.Millisecond).Timestamp(), 1000.0; math.Abs(got-want) > 1e-9 {
		t.Fatalf("SubDuration mismatch: got %.12f want %.12f", got, want)
	}

	later := DateFromTimestamp(1005.5)
	if got, want := later.DiffSeconds(base), 5.25; math.Abs(got-want) > 1e-9 {
		t.Fatalf("DiffSeconds mismatch: got %.12f want %.12f", got, want)
	}
}

func TestDateNowAndDurationFromNowParity(t *testing.T) {
	before := time.Now().UTC().Add(-1 * time.Second)
	now := DateNow().Datetime()
	after := time.Now().UTC().Add(1 * time.Second)
	if now.Before(before) || now.After(after) {
		t.Fatalf("DateNow out of expected bounds: got %s, bounds [%s, %s]", now, before, after)
	}

	baseNow := time.Now().UTC()
	withDuration := DateWithDurationFromNow(2 * time.Second).Datetime()
	diff := withDuration.Sub(baseNow)
	if diff < 1*time.Second || diff > 3*time.Second {
		t.Fatalf("DateWithDurationFromNow out of expected range: got %s", diff)
	}
}

func TestDateConversionHelpersParity(t *testing.T) {
	tagged := DateFromTimestamp(1647887071.0).TaggedCBOR()

	if got, err := tagged.TryIntoDate(); err != nil || got.String() != "2022-03-21T18:24:31Z" {
		t.Fatalf("TryIntoDate mismatch: got=%v err=%v", got, err)
	}
	if got, err := tagged.TryDate(); err != nil || got.String() != "2022-03-21T18:24:31Z" {
		t.Fatalf("TryDate mismatch: got=%v err=%v", got, err)
	}
	if got, ok := tagged.IntoDate(); !ok || got.String() != "2022-03-21T18:24:31Z" {
		t.Fatalf("IntoDate mismatch: got=%v ok=%v", got, ok)
	}
	if got, err := DecodeDate(tagged); err != nil || got.String() != "2022-03-21T18:24:31Z" {
		t.Fatalf("DecodeDate mismatch: got=%v err=%v", got, err)
	}

	if _, err := MustFromAny("not-a-date").TryIntoDate(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for text->date, got %v", err)
	}
	if _, ok := MustFromAny("not-a-date").IntoDate(); ok {
		t.Fatalf("expected IntoDate to fail for non-tagged input")
	}

	wrongTagged := ToTaggedValue(TagWithValue(2), MustFromAny(0))
	_, err := wrongTagged.TryIntoDate()
	if err == nil {
		t.Fatalf("expected wrong-tag failure for TryIntoDate")
	}
	var wrongTag WrongTagError
	if !errors.As(err, &wrongTag) {
		t.Fatalf("expected WrongTagError for TryIntoDate wrong tag, got %T", err)
	}
}

func TestDateTaggedAndUntaggedDataHelpersParity(t *testing.T) {
	date := DateFromTimestamp(1675854714.25)

	taggedData := date.TaggedCBORData()
	fromTaggedData, err := DateFromTaggedCBORData(taggedData)
	if err != nil {
		t.Fatalf("DateFromTaggedCBORData failed: %v", err)
	}
	if math.Abs(fromTaggedData.Timestamp()-date.Timestamp()) > 1e-9 {
		t.Fatalf("DateFromTaggedCBORData timestamp mismatch: got %.12f want %.12f", fromTaggedData.Timestamp(), date.Timestamp())
	}

	untaggedData := date.UntaggedCBORData()
	fromUntaggedData, err := DateFromUntaggedCBORData(untaggedData)
	if err != nil {
		t.Fatalf("DateFromUntaggedCBORData failed: %v", err)
	}
	if math.Abs(fromUntaggedData.Timestamp()-date.Timestamp()) > 1e-9 {
		t.Fatalf("DateFromUntaggedCBORData timestamp mismatch: got %.12f want %.12f", fromUntaggedData.Timestamp(), date.Timestamp())
	}

	if got, want := ToCBORData(date), taggedData; string(got) != string(want) {
		t.Fatalf("ToCBORData(Date) mismatch: got=%x want=%x", got, want)
	}

	wrongTagData := ToTaggedValue(TagWithValue(2), MustFromAny(0)).ToCBORData()
	if _, err := DateFromTaggedCBORData(wrongTagData); err == nil {
		t.Fatalf("expected wrong-tag failure for DateFromTaggedCBORData")
	}

	sameInstant := DateFromDatetime(date.Datetime())
	if !date.Equal(sameInstant) {
		t.Fatalf("Date.Equal expected equality for identical instant")
	}
	if date.Equal(date.AddSeconds(1)) {
		t.Fatalf("Date.Equal expected inequality for different instant")
	}
}
