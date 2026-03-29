package provenancemark

import (
	"encoding/binary"
	"fmt"
	"time"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

var dateReference = time.Date(2001, 1, 1, 0, 0, 0, 0, time.UTC)

// ByteRange describes an exclusive byte span. End == -1 means "to the end".
type ByteRange struct {
	Start int
	End   int
}

// SerializeDate2Bytes encodes a date into the provenance 2-byte form.
func SerializeDate2Bytes(date dcbor.Date) ([2]byte, error) {
	t := date.Datetime()
	year := t.Year()
	month := int(t.Month())
	day := t.Day()

	yy := year - 2023
	if yy < 0 || yy >= 128 {
		return [2]byte{}, newYearOutOfRange(year)
	}
	if month < 1 || month > 12 || day < 1 || day > 31 {
		return [2]byte{}, newInvalidMonthOrDay(year, month, day)
	}

	value := (uint16(yy) << 9) | (uint16(month) << 5) | uint16(day)
	var out [2]byte
	binary.BigEndian.PutUint16(out[:], value)
	return out, nil
}

// DeserializeDate2Bytes decodes a date from the provenance 2-byte form.
func DeserializeDate2Bytes(bytes [2]byte) (dcbor.Date, error) {
	value := binary.BigEndian.Uint16(bytes[:])
	day := int(value & 0b11111)
	month := int((value >> 5) & 0b1111)
	yy := int((value >> 9) & 0b1111111)
	year := yy + 2023

	start, end := RangeOfDaysInMonth(year, month)
	if month < 1 || month > 12 || day < start || day >= end {
		return dcbor.Date{}, newInvalidMonthOrDay(year, month, day)
	}

	return dcbor.DateFromDatetime(time.Date(year, time.Month(month), day, 0, 0, 0, 0, time.UTC)), nil
}

// SerializeDate4Bytes encodes a date as seconds since 2001-01-01T00:00:00Z.
func SerializeDate4Bytes(date dcbor.Date) ([4]byte, error) {
	seconds := date.Datetime().Unix() - dateReference.Unix()
	if seconds < 0 || seconds > int64(^uint32(0)) {
		return [4]byte{}, newDateOutOfRange("seconds value too large for u32")
	}

	var out [4]byte
	binary.BigEndian.PutUint32(out[:], uint32(seconds))
	return out, nil
}

// DeserializeDate4Bytes decodes a date from the provenance 4-byte form.
func DeserializeDate4Bytes(bytes [4]byte) (dcbor.Date, error) {
	seconds := binary.BigEndian.Uint32(bytes[:])
	return dcbor.DateFromDatetime(time.Unix(dateReference.Unix()+int64(seconds), 0).UTC()), nil
}

// SerializeDate6Bytes encodes a date as milliseconds since 2001-01-01T00:00:00Z.
func SerializeDate6Bytes(date dcbor.Date) ([6]byte, error) {
	t := date.Datetime()
	milliseconds := (t.Unix()-dateReference.Unix())*1000 + int64(t.Nanosecond()/1_000_000)
	if milliseconds < 0 {
		return [6]byte{}, newDateOutOfRange("milliseconds value too large for u64")
	}
	const max = 0xe5940a78a7ff
	if milliseconds > max {
		return [6]byte{}, newDateOutOfRange("date exceeds maximum representable value")
	}

	var full [8]byte
	binary.BigEndian.PutUint64(full[:], uint64(milliseconds))
	var out [6]byte
	copy(out[:], full[2:])
	return out, nil
}

// DeserializeDate6Bytes decodes a date from the provenance 6-byte form.
func DeserializeDate6Bytes(bytes [6]byte) (dcbor.Date, error) {
	const max = 0xe5940a78a7ff
	var full [8]byte
	copy(full[2:], bytes[:])
	value := binary.BigEndian.Uint64(full[:])
	if value > max {
		return dcbor.Date{}, newDateOutOfRange("date exceeds maximum representable value")
	}
	seconds := int64(value / 1000)
	nanos := int64(value%1000) * 1_000_000
	return dcbor.DateFromDatetime(time.Unix(dateReference.Unix()+seconds, nanos).UTC()), nil
}

// RangeOfDaysInMonth returns the half-open day range [1, lastDay+1).
func RangeOfDaysInMonth(year, month int) (int, int) {
	if month < 1 || month > 12 {
		return 1, 1
	}
	lastDay := time.Date(year, time.Month(month)+1, 0, 0, 0, 0, 0, time.UTC).Day()
	return 1, lastDay + 1
}

func serializeDateByResolution(res ProvenanceMarkResolution, date dcbor.Date) ([]byte, error) {
	switch res {
	case ProvenanceMarkResolutionLow:
		out, err := SerializeDate2Bytes(date)
		return out[:], err
	case ProvenanceMarkResolutionMedium:
		out, err := SerializeDate4Bytes(date)
		return out[:], err
	case ProvenanceMarkResolutionQuartile, ProvenanceMarkResolutionHigh:
		out, err := SerializeDate6Bytes(date)
		return out[:], err
	default:
		return nil, newResolutionError(fmt.Sprintf("invalid provenance mark resolution value: %d", res))
	}
}

func deserializeDateByResolution(res ProvenanceMarkResolution, data []byte) (dcbor.Date, error) {
	switch res {
	case ProvenanceMarkResolutionLow:
		if len(data) != 2 {
			return dcbor.Date{}, newResolutionError(fmt.Sprintf("invalid date length: expected 2, 4, or 6 bytes, got %d", len(data)))
		}
		var buf [2]byte
		copy(buf[:], data)
		return DeserializeDate2Bytes(buf)
	case ProvenanceMarkResolutionMedium:
		if len(data) != 4 {
			return dcbor.Date{}, newResolutionError(fmt.Sprintf("invalid date length: expected 2, 4, or 6 bytes, got %d", len(data)))
		}
		var buf [4]byte
		copy(buf[:], data)
		return DeserializeDate4Bytes(buf)
	case ProvenanceMarkResolutionQuartile, ProvenanceMarkResolutionHigh:
		if len(data) != 6 {
			return dcbor.Date{}, newResolutionError(fmt.Sprintf("invalid date length: expected 2, 4, or 6 bytes, got %d", len(data)))
		}
		var buf [6]byte
		copy(buf[:], data)
		return DeserializeDate6Bytes(buf)
	default:
		return dcbor.Date{}, newResolutionError(fmt.Sprintf("invalid provenance mark resolution value: %d", res))
	}
}
