package dcbor

import (
	"errors"
	"math"
	"testing"
)

func decodeInt(c CBOR) (int, error) {
	v, err := c.TryIntoInt64()
	if err != nil {
		return 0, err
	}
	return int(v), nil
}

func TestConversionRoundTripPrimitiveParity(t *testing.T) {
	values := []any{
		10,
		int64(-10),
		false,
		"Hello",
		10.0,
		[]byte{0x00, 0x11, 0x22, 0x33, 0x44, 0x55},
	}

	for _, value := range values {
		cbor := MustFromAny(value)
		decoded := cbor.ToNative()
		if decoded == nil {
			t.Fatalf("unexpected nil decoded value for %v", value)
		}
		// Round-trip through CBOR encoding should be stable.
		decodedCBOR, err := TryFromData(cbor.ToCBORData())
		if err != nil {
			t.Fatalf("TryFromData failed for %v: %v", value, err)
		}
		if !decodedCBOR.Equal(cbor) {
			t.Fatalf("round-trip mismatch for %v", value)
		}
	}
}

func TestConversionMapAndArrayParity(t *testing.T) {
	h := map[int]string{
		1:  "A",
		50: "B",
		25: "C",
	}
	cborMap := MustFromAny(h)
	if got, want := cborMap.DiagnosticFlat(), `{1: "A", 25: "C", 50: "B"}`; got != want {
		t.Fatalf("map diagnostic mismatch: got %q want %q", got, want)
	}

	decodedMap, err := DecodeMap(cborMap, decodeInt, DecodeText)
	if err != nil {
		t.Fatalf("DecodeMap failed: %v", err)
	}
	if len(decodedMap) != len(h) || decodedMap[1] != "A" || decodedMap[25] != "C" || decodedMap[50] != "B" {
		t.Fatalf("decoded map mismatch: got %#v", decodedMap)
	}

	v := []int{1, 50, 25}
	cborArray := MustFromAny(v)
	if got, want := cborArray.DiagnosticFlat(), "[1, 50, 25]"; got != want {
		t.Fatalf("array diagnostic mismatch: got %q want %q", got, want)
	}

	decodedArray, err := DecodeArray(cborArray, decodeInt)
	if err != nil {
		t.Fatalf("DecodeArray failed: %v", err)
	}
	if len(decodedArray) != 3 || decodedArray[0] != 1 || decodedArray[1] != 50 || decodedArray[2] != 25 {
		t.Fatalf("decoded array mismatch: got %#v", decodedArray)
	}
}

func TestConversionSetParity(t *testing.T) {
	set := NewSet()
	set.Insert(MustFromAny(1))
	set.Insert(MustFromAny(50))
	set.Insert(MustFromAny(25))

	cborSet := MustFromAny(set)
	decodedSet, err := DecodeSet(cborSet, decodeInt)
	if err != nil {
		t.Fatalf("DecodeSet failed: %v", err)
	}
	if len(decodedSet) != 3 {
		t.Fatalf("decoded set size mismatch: got %d want %d", len(decodedSet), 3)
	}
	if _, ok := decodedSet[1]; !ok {
		t.Fatalf("decoded set missing 1")
	}
	if _, ok := decodedSet[25]; !ok {
		t.Fatalf("decoded set missing 25")
	}
	if _, ok := decodedSet[50]; !ok {
		t.Fatalf("decoded set missing 50")
	}
}

func TestUsageVectorsParity(t *testing.T) {
	array := []uint32{1000, 2000, 3000}
	cbor := MustFromAny(array)
	if got, want := cbor.Hex(), "831903e81907d0190bb8"; got != want {
		t.Fatalf("usage vector hex mismatch: got %q want %q", got, want)
	}

	decoded, err := TryFromHex("831903e81907d0190bb8")
	if err != nil {
		t.Fatalf("TryFromHex failed: %v", err)
	}
	if got, want := decoded.DiagnosticFlat(), "[1000, 2000, 3000]"; got != want {
		t.Fatalf("decoded diagnostic mismatch: got %q want %q", got, want)
	}
	decodedArray, err := DecodeArray(decoded, func(c CBOR) (uint32, error) {
		v, err := c.TryIntoUInt64()
		if err != nil {
			return 0, err
		}
		return uint32(v), nil
	})
	if err != nil {
		t.Fatalf("DecodeArray uint32 failed: %v", err)
	}
	if len(decodedArray) != 3 || decodedArray[0] != 1000 || decodedArray[1] != 2000 || decodedArray[2] != 3000 {
		t.Fatalf("decoded usage array mismatch: %#v", decodedArray)
	}
}

func TestIntAndFloatCoercionParity(t *testing.T) {
	n := 42
	c := MustFromAny(n)

	f, err := c.TryIntoFloat64()
	if err != nil {
		t.Fatalf("TryIntoFloat64 failed: %v", err)
	}
	if f != float64(n) {
		t.Fatalf("float coercion mismatch: got %v want %v", f, float64(n))
	}

	c2 := MustFromAny(f)
	if !c2.Equal(c) {
		t.Fatalf("int-float cbor mismatch: got %s want %s", c2.DiagnosticFlat(), c.DiagnosticFlat())
	}

	i, err := c.TryIntoInt64()
	if err != nil {
		t.Fatalf("TryIntoInt64 failed: %v", err)
	}
	if i != int64(n) {
		t.Fatalf("int decode mismatch: got %d want %d", i, n)
	}

	fractional := MustFromAny(42.5)
	f2, err := fractional.TryIntoFloat64()
	if err != nil {
		t.Fatalf("TryIntoFloat64 for fractional failed: %v", err)
	}
	if f2 != 42.5 {
		t.Fatalf("fractional float mismatch: got %v want %v", f2, 42.5)
	}
	if _, err := fractional.TryIntoInt64(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for float-to-int conversion, got %v", err)
	}
}

func TestNumericOutOfRangeConversions(t *testing.T) {
	tooLargeUnsigned := MustFromAny(uint64(math.MaxUint64))
	if _, err := tooLargeUnsigned.TryIntoInt64(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for uint64->int64, got %v", err)
	}
	if _, err := tooLargeUnsigned.TryIntoFloat64(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for uint64->float64, got %v", err)
	}

	tooLargeNegative, err := TryFromHex("3b8000000000000000") // -9223372036854775809
	if err != nil {
		t.Fatalf("TryFromHex failed: %v", err)
	}
	if _, err := tooLargeNegative.TryIntoInt64(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for large negative->int64, got %v", err)
	}
}
