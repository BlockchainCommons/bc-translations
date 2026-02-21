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

func exactU64FromF64ViaCBOR(n float64) (uint64, bool) {
	c := MustFromAny(n)
	v, err := c.TryIntoUInt64()
	if err != nil {
		return 0, false
	}
	if c.Kind() != CBORKindUnsigned {
		return 0, false
	}
	return v, true
}

func exactI64FromF64ViaCBOR(n float64) (int64, bool) {
	c := MustFromAny(n)
	v, err := c.TryIntoInt64()
	if err != nil {
		return 0, false
	}
	switch c.Kind() {
	case CBORKindUnsigned, CBORKindNegative:
		return v, true
	default:
		return 0, false
	}
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

func TestReflectiveFromAnyConversions(t *testing.T) {
	cborArray, err := FromAny([]int{1, 2, 3})
	if err != nil {
		t.Fatalf("FromAny []int failed: %v", err)
	}
	if got, want := cborArray.DiagnosticFlat(), "[1, 2, 3]"; got != want {
		t.Fatalf("[]int conversion mismatch: got %q want %q", got, want)
	}

	cborMap, err := FromAny(map[string]int{"b": 2, "a": 1})
	if err != nil {
		t.Fatalf("FromAny map[string]int failed: %v", err)
	}
	if got, want := cborMap.DiagnosticFlat(), `{"a": 1, "b": 2}`; got != want {
		t.Fatalf("map conversion mismatch: got %q want %q", got, want)
	}

	cborNested, err := FromAny(map[string]any{
		"name":  "Alice",
		"roles": []string{"admin", "operator"},
	})
	if err != nil {
		t.Fatalf("FromAny nested map failed: %v", err)
	}
	if got := cborNested.DiagnosticFlat(); got == "" {
		t.Fatalf("expected non-empty nested conversion diagnostic")
	}
}

func TestSetFromVecDeterminismAndValidation(t *testing.T) {
	set := SetFromVec([]CBOR{MustFromAny(2), MustFromAny(1), MustFromAny(3)})
	if got, want := NewCBORArray(set.AsVec()).DiagnosticFlat(), "[1, 2, 3]"; got != want {
		t.Fatalf("SetFromVec ordering mismatch: got %q want %q", got, want)
	}

	_, err := TrySetFromVec([]CBOR{MustFromAny(2), MustFromAny(1)})
	if !errors.Is(err, ErrMisorderedMapKey) {
		t.Fatalf("expected ErrMisorderedMapKey from TrySetFromVec, got %v", err)
	}

	_, err = TrySetFromVec([]CBOR{MustFromAny(1), MustFromAny(1)})
	if !errors.Is(err, ErrDuplicateMapKey) {
		t.Fatalf("expected ErrDuplicateMapKey from TrySetFromVec, got %v", err)
	}
}

func TestExactU64FromF64ParityVectors(t *testing.T) {
	cases := []struct {
		name  string
		value float64
		want  uint64
		ok    bool
	}{
		{name: "integer", value: 1234.0, want: 1234, ok: true},
		{name: "negative_integer", value: -1234.0, ok: false},
		{name: "large_exact", value: 18446744073709550000.0, want: 18446744073709549568, ok: true},
		{name: "large_inexact", value: 18446744073709552000.0, ok: false},
		{name: "zero", value: 0.0, want: 0, ok: true},
		{name: "negative_zero", value: math.Copysign(0.0, -1.0), want: 0, ok: true},
		{name: "half", value: 0.5, ok: false},
		{name: "negative_half", value: -0.5, ok: false},
		{name: "nan", value: math.NaN(), ok: false},
		{name: "inf", value: math.Inf(1), ok: false},
		{name: "neg_inf", value: math.Inf(-1), ok: false},
		{name: "max_exact_int_f64", value: 9007199254740991.0, want: 9007199254740991, ok: true},
		{name: "one", value: 1.0, want: 1, ok: true},
		{name: "subnormal", value: 5e-324, ok: false},
		{name: "u64_max_as_f64", value: float64(^uint64(0)), ok: false},
		{name: "u64_max_minus_1_as_f64", value: float64(^uint64(0) - 1), ok: false},
		{name: "u64_max_minus_2_as_f64", value: float64(^uint64(0) - 2), ok: false},
		{name: "smallest_increment", value: 1.0000000000000002, ok: false},
		{name: "non_integer_precision", value: 4503599627370495.5, ok: false},
		{name: "min_positive", value: math.SmallestNonzeroFloat64, ok: false},
		{name: "max_float", value: math.MaxFloat64, ok: false},
	}

	for _, tc := range cases {
		got, ok := exactU64FromF64ViaCBOR(tc.value)
		if ok != tc.ok {
			t.Fatalf("%s ok mismatch: got %v want %v (value=%v)", tc.name, ok, tc.ok, tc.value)
		}
		if tc.ok && got != tc.want {
			t.Fatalf("%s value mismatch: got %d want %d", tc.name, got, tc.want)
		}
	}
}

func TestExactI64FromF64ParityVectors(t *testing.T) {
	cases := []struct {
		name  string
		value float64
		want  int64
		ok    bool
	}{
		{name: "zero", value: 0.0, want: 0, ok: true},
		{name: "negative_zero", value: math.Copysign(0.0, -1.0), want: 0, ok: true},
		{name: "half", value: 0.5, ok: false},
		{name: "negative_half", value: -0.5, ok: false},
		{name: "integer", value: 1234.0, want: 1234, ok: true},
		{name: "negative_integer", value: -1234.0, want: -1234, ok: true},
		{name: "nan", value: math.NaN(), ok: false},
		{name: "inf", value: math.Inf(1), ok: false},
		{name: "neg_inf", value: math.Inf(-1), ok: false},
		{name: "i64_max_as_f64", value: float64(math.MaxInt64), ok: false},
		{name: "i64_min_as_f64", value: float64(math.MinInt64), want: math.MinInt64, ok: true},
		{name: "subnormal_pos", value: 1e-308, ok: false},
		{name: "subnormal_neg", value: -1e-308, ok: false},
		{name: "i64_max_plus_1", value: float64(math.MaxInt64) + 1.0, ok: false},
		{name: "power_two", value: 1024.0, want: 1024, ok: true},
		{name: "power_two_neg", value: -1024.0, want: -1024, ok: true},
		{name: "fractional", value: 1234.56, ok: false},
		{name: "fractional_neg", value: -1234.56, ok: false},
		{name: "largest_exact_f64_integer", value: 9007199254740991.0, want: 9007199254740991, ok: true},
		{name: "largest_exact_f64_integer_neg", value: -9007199254740991.0, want: -9007199254740991, ok: true},
		{name: "most_negative_double_to_i64", value: -9223372036854774784.0, want: -9223372036854774784, ok: true},
	}

	for _, tc := range cases {
		got, ok := exactI64FromF64ViaCBOR(tc.value)
		if ok != tc.ok {
			t.Fatalf("%s ok mismatch: got %v want %v (value=%v)", tc.name, ok, tc.ok, tc.value)
		}
		if tc.ok && got != tc.want {
			t.Fatalf("%s value mismatch: got %d want %d", tc.name, got, tc.want)
		}
	}
}
