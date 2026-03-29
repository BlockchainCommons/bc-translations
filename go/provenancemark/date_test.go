package provenancemark

import (
	"bytes"
	"testing"
	"time"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func TestDate2Bytes(t *testing.T) {
	base := dcbor.DateFromDatetime(time.Date(2023, 6, 20, 0, 0, 0, 0, time.UTC))
	serialized, err := SerializeDate2Bytes(base)
	if err != nil {
		t.Fatalf("SerializeDate2Bytes failed: %v", err)
	}
	if !bytes.Equal(serialized[:], []byte{0x00, 0xd4}) {
		t.Fatalf("SerializeDate2Bytes mismatch: got=%x want=00d4", serialized)
	}
	deserialized, err := DeserializeDate2Bytes(serialized)
	if err != nil {
		t.Fatalf("DeserializeDate2Bytes failed: %v", err)
	}
	if !deserialized.Equal(base) {
		t.Fatalf("round-trip mismatch: got=%s want=%s", deserialized, base)
	}

	minDate, err := DeserializeDate2Bytes([2]byte{0x00, 0x21})
	if err != nil {
		t.Fatalf("DeserializeDate2Bytes(min) failed: %v", err)
	}
	if want := dcbor.DateFromDatetime(time.Date(2023, 1, 1, 0, 0, 0, 0, time.UTC)); !minDate.Equal(want) {
		t.Fatalf("min date mismatch: got=%s want=%s", minDate, want)
	}

	maxDate, err := DeserializeDate2Bytes([2]byte{0xff, 0x9f})
	if err != nil {
		t.Fatalf("DeserializeDate2Bytes(max) failed: %v", err)
	}
	if want := dcbor.DateFromDatetime(time.Date(2150, 12, 31, 0, 0, 0, 0, time.UTC)); !maxDate.Equal(want) {
		t.Fatalf("max date mismatch: got=%s want=%s", maxDate, want)
	}

	if _, err := DeserializeDate2Bytes([2]byte{0x00, 0x5e}); err == nil {
		t.Fatal("expected invalid 2-byte date to fail")
	}
}

func TestDate4Bytes(t *testing.T) {
	base := dcbor.DateFromDatetime(time.Date(2023, 6, 20, 12, 34, 56, 0, time.UTC))
	serialized, err := SerializeDate4Bytes(base)
	if err != nil {
		t.Fatalf("SerializeDate4Bytes failed: %v", err)
	}
	if !bytes.Equal(serialized[:], []byte{0x2a, 0x41, 0xd4, 0x70}) {
		t.Fatalf("SerializeDate4Bytes mismatch: got=%x want=2a41d470", serialized)
	}
	deserialized, err := DeserializeDate4Bytes(serialized)
	if err != nil {
		t.Fatalf("DeserializeDate4Bytes failed: %v", err)
	}
	if !deserialized.Equal(base) {
		t.Fatalf("round-trip mismatch: got=%s want=%s", deserialized, base)
	}

	minDate, err := DeserializeDate4Bytes([4]byte{})
	if err != nil {
		t.Fatalf("DeserializeDate4Bytes(min) failed: %v", err)
	}
	if want := dcbor.DateFromDatetime(time.Date(2001, 1, 1, 0, 0, 0, 0, time.UTC)); !minDate.Equal(want) {
		t.Fatalf("min date mismatch: got=%s want=%s", minDate, want)
	}

	maxDate, err := DeserializeDate4Bytes([4]byte{0xff, 0xff, 0xff, 0xff})
	if err != nil {
		t.Fatalf("DeserializeDate4Bytes(max) failed: %v", err)
	}
	if want := dcbor.DateFromDatetime(time.Date(2137, 2, 7, 6, 28, 15, 0, time.UTC)); !maxDate.Equal(want) {
		t.Fatalf("max date mismatch: got=%s want=%s", maxDate, want)
	}
}

func TestDate6Bytes(t *testing.T) {
	base := dcbor.DateFromDatetime(time.Date(2023, 6, 20, 12, 34, 56, 789_000_000, time.UTC))
	serialized, err := SerializeDate6Bytes(base)
	if err != nil {
		t.Fatalf("SerializeDate6Bytes failed: %v", err)
	}
	if !bytes.Equal(serialized[:], []byte{0x00, 0xa5, 0x11, 0x25, 0xd8, 0x95}) {
		t.Fatalf("SerializeDate6Bytes mismatch: got=%x want=00a51125d895", serialized)
	}
	deserialized, err := DeserializeDate6Bytes(serialized)
	if err != nil {
		t.Fatalf("DeserializeDate6Bytes failed: %v", err)
	}
	if !deserialized.Equal(base) {
		t.Fatalf("round-trip mismatch: got=%s want=%s", deserialized, base)
	}

	minDate, err := DeserializeDate6Bytes([6]byte{})
	if err != nil {
		t.Fatalf("DeserializeDate6Bytes(min) failed: %v", err)
	}
	if want := dcbor.DateFromDatetime(time.Date(2001, 1, 1, 0, 0, 0, 0, time.UTC)); !minDate.Equal(want) {
		t.Fatalf("min date mismatch: got=%s want=%s", minDate, want)
	}

	maxDate, err := DeserializeDate6Bytes([6]byte{0xe5, 0x94, 0x0a, 0x78, 0xa7, 0xff})
	if err != nil {
		t.Fatalf("DeserializeDate6Bytes(max) failed: %v", err)
	}
	if want := dcbor.DateFromDatetime(time.Date(9999, 12, 31, 23, 59, 59, 999_000_000, time.UTC)); !maxDate.Equal(want) {
		t.Fatalf("max date mismatch: got=%s want=%s", maxDate, want)
	}

	if _, err := DeserializeDate6Bytes([6]byte{0xe5, 0x94, 0x0a, 0x78, 0xa8, 0x00}); err == nil {
		t.Fatal("expected invalid 6-byte date to fail")
	}
}
