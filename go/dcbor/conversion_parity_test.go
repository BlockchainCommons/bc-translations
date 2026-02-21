package dcbor

import (
	"container/list"
	"errors"
	"math"
	"math/big"
	"strconv"
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

func TestConversionOrderedMapParity(t *testing.T) {
	h := map[int]string{
		1:  "A",
		50: "B",
		25: "C",
	}
	cborMap := MustFromAny(h)
	if got, want := cborMap.DiagnosticFlat(), `{1: "A", 25: "C", 50: "B"}`; got != want {
		t.Fatalf("ordered map diagnostic mismatch: got %q want %q", got, want)
	}

	decodedMap, err := DecodeMap(cborMap, decodeInt, DecodeText)
	if err != nil {
		t.Fatalf("DecodeMap failed: %v", err)
	}
	if len(decodedMap) != len(h) || decodedMap[1] != "A" || decodedMap[25] != "C" || decodedMap[50] != "B" {
		t.Fatalf("decoded ordered map mismatch: got %#v", decodedMap)
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

func TestConversionDequeParity(t *testing.T) {
	deque := list.New()
	deque.PushBack(1)
	deque.PushBack(50)
	deque.PushBack(25)

	values := make([]int, 0, deque.Len())
	for element := deque.Front(); element != nil; element = element.Next() {
		values = append(values, element.Value.(int))
	}

	cbor := MustFromAny(values)
	if got, want := cbor.DiagnosticFlat(), "[1, 50, 25]"; got != want {
		t.Fatalf("deque diagnostic mismatch: got %q want %q", got, want)
	}

	decoded, err := DecodeArray(cbor, decodeInt)
	if err != nil {
		t.Fatalf("DecodeArray failed: %v", err)
	}
	if len(decoded) != len(values) {
		t.Fatalf("decoded deque length mismatch: got %d want %d", len(decoded), len(values))
	}
	for i := range values {
		if decoded[i] != values[i] {
			t.Fatalf("decoded deque value mismatch at index %d: got %d want %d", i, decoded[i], values[i])
		}
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
	floatValue, err := tooLargeUnsigned.TryIntoFloat64()
	if err != nil {
		t.Fatalf("expected uint64->float64 conversion success, got %v", err)
	}
	if got, want := floatValue, 18446744073709551616.0; got != want {
		t.Fatalf("uint64->float64 mismatch: got %.0f want %.0f", got, want)
	}

	tooLargeNegative, err := TryFromHex("3b8000000000000000") // -9223372036854775809
	if err != nil {
		t.Fatalf("TryFromHex failed: %v", err)
	}
	if _, err := tooLargeNegative.TryIntoInt64(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for large negative->int64, got %v", err)
	}
	tooLargeNegativeFloat, err := tooLargeNegative.TryIntoFloat64()
	if err != nil {
		t.Fatalf("expected large negative->float64 conversion success, got %v", err)
	}
	if got, want := tooLargeNegativeFloat, -9223372036854775808.0; got != want {
		t.Fatalf("large negative->float64 mismatch: got %.0f want %.0f", got, want)
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

func TestTypedIntegerConversionParity(t *testing.T) {
	int8Value := MustFromAny(-21)
	if got, err := int8Value.TryIntoInt8(); err != nil || got != -21 {
		t.Fatalf("TryIntoInt8 mismatch: got=%d err=%v", got, err)
	}
	if got, err := int8Value.TryInt8(); err != nil || got != -21 {
		t.Fatalf("TryInt8 mismatch: got=%d err=%v", got, err)
	}
	if got, ok := int8Value.IntoInt8(); !ok || got != -21 {
		t.Fatalf("IntoInt8 mismatch: got=%d ok=%v", got, ok)
	}

	int16Value := MustFromAny(-21)
	if got, err := int16Value.TryIntoInt16(); err != nil || got != -21 {
		t.Fatalf("TryIntoInt16 mismatch: got=%d err=%v", got, err)
	}
	if got, err := int16Value.TryInt16(); err != nil || got != -21 {
		t.Fatalf("TryInt16 mismatch: got=%d err=%v", got, err)
	}
	if got, ok := int16Value.IntoInt16(); !ok || got != -21 {
		t.Fatalf("IntoInt16 mismatch: got=%d ok=%v", got, ok)
	}

	int32Value := MustFromAny(2147483647)
	if got, err := int32Value.TryIntoInt32(); err != nil || got != 2147483647 {
		t.Fatalf("TryIntoInt32 mismatch: got=%d err=%v", got, err)
	}
	if got, err := int32Value.TryInt32(); err != nil || got != 2147483647 {
		t.Fatalf("TryInt32 mismatch: got=%d err=%v", got, err)
	}
	if got, ok := int32Value.IntoInt32(); !ok || got != 2147483647 {
		t.Fatalf("IntoInt32 mismatch: got=%d ok=%v", got, ok)
	}

	intValue := MustFromAny(-42)
	if got, err := intValue.TryIntoInt(); err != nil || got != -42 {
		t.Fatalf("TryIntoInt mismatch: got=%d err=%v", got, err)
	}
	if got, err := intValue.TryInt(); err != nil || got != -42 {
		t.Fatalf("TryInt mismatch: got=%d err=%v", got, err)
	}
	if got, ok := intValue.IntoInt(); !ok || got != -42 {
		t.Fatalf("IntoInt mismatch: got=%d ok=%v", got, ok)
	}

	uint8Value := MustFromAny(255)
	if got, err := uint8Value.TryIntoUInt8(); err != nil || got != 255 {
		t.Fatalf("TryIntoUInt8 mismatch: got=%d err=%v", got, err)
	}
	if got, err := uint8Value.TryUInt8(); err != nil || got != 255 {
		t.Fatalf("TryUInt8 mismatch: got=%d err=%v", got, err)
	}
	if got, ok := uint8Value.IntoUInt8(); !ok || got != 255 {
		t.Fatalf("IntoUInt8 mismatch: got=%d ok=%v", got, ok)
	}

	uint16Value := MustFromAny(65535)
	if got, err := uint16Value.TryIntoUInt16(); err != nil || got != 65535 {
		t.Fatalf("TryIntoUInt16 mismatch: got=%d err=%v", got, err)
	}
	if got, err := uint16Value.TryUInt16(); err != nil || got != 65535 {
		t.Fatalf("TryUInt16 mismatch: got=%d err=%v", got, err)
	}
	if got, ok := uint16Value.IntoUInt16(); !ok || got != 65535 {
		t.Fatalf("IntoUInt16 mismatch: got=%d ok=%v", got, ok)
	}

	uint32Value := MustFromAny(uint64(4294967295))
	if got, err := uint32Value.TryIntoUInt32(); err != nil || got != 4294967295 {
		t.Fatalf("TryIntoUInt32 mismatch: got=%d err=%v", got, err)
	}
	if got, err := uint32Value.TryUInt32(); err != nil || got != 4294967295 {
		t.Fatalf("TryUInt32 mismatch: got=%d err=%v", got, err)
	}
	if got, ok := uint32Value.IntoUInt32(); !ok || got != 4294967295 {
		t.Fatalf("IntoUInt32 mismatch: got=%d ok=%v", got, ok)
	}

	uintValue := MustFromAny(uint64(42))
	if got, err := uintValue.TryIntoUInt(); err != nil || got != 42 {
		t.Fatalf("TryIntoUInt mismatch: got=%d err=%v", got, err)
	}
	if got, err := uintValue.TryUInt(); err != nil || got != 42 {
		t.Fatalf("TryUInt mismatch: got=%d err=%v", got, err)
	}
	if got, ok := uintValue.IntoUInt(); !ok || got != 42 {
		t.Fatalf("IntoUInt mismatch: got=%d ok=%v", got, ok)
	}
}

func TestTypedIntegerConversionErrorsParity(t *testing.T) {
	if _, err := MustFromAny(128).TryIntoInt8(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for int8 overflow, got %v", err)
	}
	if _, err := MustFromAny(-129).TryIntoInt8(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for int8 underflow, got %v", err)
	}
	if _, err := MustFromAny(32768).TryIntoInt16(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for int16 overflow, got %v", err)
	}
	if _, err := MustFromAny(-32769).TryIntoInt16(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for int16 underflow, got %v", err)
	}
	if _, err := MustFromAny(uint64(2147483648)).TryIntoInt32(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for int32 overflow, got %v", err)
	}
	if _, err := MustFromAny(-2147483649).TryIntoInt32(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for int32 underflow, got %v", err)
	}
	if _, err := MustFromAny(65536).TryIntoUInt16(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for uint16 overflow, got %v", err)
	}
	if _, err := MustFromAny(256).TryIntoUInt8(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for uint8 overflow, got %v", err)
	}
	if _, err := MustFromAny(uint64(4294967296)).TryIntoUInt32(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for uint32 overflow, got %v", err)
	}
	if strconv.IntSize == 32 {
		if _, err := MustFromAny(int64(math.MaxInt32) + 1).TryIntoInt(); !errors.Is(err, ErrOutOfRange) {
			t.Fatalf("expected ErrOutOfRange for int overflow on 32-bit, got %v", err)
		}
		if _, err := MustFromAny(uint64(math.MaxUint32) + 1).TryIntoUInt(); !errors.Is(err, ErrOutOfRange) {
			t.Fatalf("expected ErrOutOfRange for uint overflow on 32-bit, got %v", err)
		}
	}
	if _, err := MustFromAny(-1).TryIntoUInt16(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for negative->uint16, got %v", err)
	}
	if _, err := MustFromAny(-1).TryIntoUInt8(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for negative->uint8, got %v", err)
	}
	if _, err := MustFromAny(-1).TryIntoUInt32(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for negative->uint32, got %v", err)
	}
	if _, err := MustFromAny(42.5).TryIntoInt32(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for non-integral float->int32, got %v", err)
	}
	if _, err := MustFromAny(42.5).TryIntoInt8(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for non-integral float->int8, got %v", err)
	}
	if _, err := MustFromAny("42").TryIntoUInt16(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for text->uint16, got %v", err)
	}
	if _, err := MustFromAny("42").TryIntoUInt8(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for text->uint8, got %v", err)
	}

	if _, ok := MustFromAny(42.5).IntoInt16(); ok {
		t.Fatalf("expected IntoInt16 to fail for non-integral float")
	}
	if _, ok := MustFromAny(42.5).IntoInt8(); ok {
		t.Fatalf("expected IntoInt8 to fail for non-integral float")
	}
	if _, ok := MustFromAny(-1).IntoUInt32(); ok {
		t.Fatalf("expected IntoUInt32 to fail for negative value")
	}
	if _, ok := MustFromAny(-1).IntoUInt8(); ok {
		t.Fatalf("expected IntoUInt8 to fail for negative value")
	}
}

func TestExactInt16ConversionParity(t *testing.T) {
	assertInt16OK := func(name string, v any, want int16) {
		t.Helper()
		got, err := MustFromAny(v).TryIntoInt16()
		if err != nil || got != want {
			t.Fatalf("%s: TryIntoInt16 got=%d err=%v want=%d", name, got, err, want)
		}
	}
	assertInt16Err := func(name string, v any) {
		t.Helper()
		if _, err := MustFromAny(v).TryIntoInt16(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertInt16OK("f64_21", 21.0, 21)
	assertInt16OK("i64_neg21", int64(-21), -21)
	assertInt16OK("u64_21", uint64(21), 21)
	assertInt16Err("f64_fractional", 21.5)
	assertInt16Err("f64_nan", math.NaN())
	assertInt16Err("f64_inf", math.Inf(1))
	assertInt16Err("f64_neg_inf", math.Inf(-1))
	assertInt16Err("u64_max", uint64(math.MaxUint64))
	assertInt16Err("u64_65536", uint64(65536))
	assertInt16Err("i64_max", int64(math.MaxInt64))
	assertInt16Err("i64_min", int64(math.MinInt64))
	assertInt16Err("i64_neg65536", int64(-65536))
}

func TestExactInt32ConversionParity(t *testing.T) {
	assertInt32OK := func(name string, v any, want int32) {
		t.Helper()
		got, err := MustFromAny(v).TryIntoInt32()
		if err != nil || got != want {
			t.Fatalf("%s: TryIntoInt32 got=%d err=%v want=%d", name, got, err, want)
		}
	}
	assertInt32Err := func(name string, v any) {
		t.Helper()
		if _, err := MustFromAny(v).TryIntoInt32(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertInt32OK("f64_21", 21.0, 21)
	assertInt32OK("i64_neg21", int64(-21), -21)
	assertInt32OK("u64_21", uint64(21), 21)
	assertInt32Err("f64_fractional", 21.5)
	assertInt32Err("f64_nan", math.NaN())
	assertInt32Err("f64_inf", math.Inf(1))
	assertInt32Err("f64_neg_inf", math.Inf(-1))
	assertInt32Err("u64_max", uint64(math.MaxUint64))
	assertInt32Err("u64_4294967296", uint64(4294967296))
	assertInt32Err("i64_max", int64(math.MaxInt64))
	assertInt32Err("i64_min", int64(math.MinInt64))
	assertInt32Err("i64_neg4294967296", int64(-4294967296))
}

func TestExactUInt16ConversionParity(t *testing.T) {
	assertUInt16OK := func(name string, v any, want uint16) {
		t.Helper()
		got, err := MustFromAny(v).TryIntoUInt16()
		if err != nil || got != want {
			t.Fatalf("%s: TryIntoUInt16 got=%d err=%v want=%d", name, got, err, want)
		}
	}
	assertUInt16Err := func(name string, v any) {
		t.Helper()
		if _, err := MustFromAny(v).TryIntoUInt16(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertUInt16OK("f64_21", 21.0, 21)
	assertUInt16OK("i64_21", int64(21), 21)
	assertUInt16OK("u64_21", uint64(21), 21)
	assertUInt16Err("f64_fractional", 21.5)
	assertUInt16Err("f64_nan", math.NaN())
	assertUInt16Err("f64_inf", math.Inf(1))
	assertUInt16Err("f64_neg_inf", math.Inf(-1))
	assertUInt16Err("u64_max", uint64(math.MaxUint64))
	assertUInt16Err("u64_65536", uint64(65536))
	assertUInt16Err("i64_neg21", int64(-21))
	assertUInt16Err("i64_min", int64(math.MinInt64))
	assertUInt16Err("i64_neg65536", int64(-65536))
}

func TestExactUInt32ConversionParity(t *testing.T) {
	assertUInt32OK := func(name string, v any, want uint32) {
		t.Helper()
		got, err := MustFromAny(v).TryIntoUInt32()
		if err != nil || got != want {
			t.Fatalf("%s: TryIntoUInt32 got=%d err=%v want=%d", name, got, err, want)
		}
	}
	assertUInt32Err := func(name string, v any) {
		t.Helper()
		if _, err := MustFromAny(v).TryIntoUInt32(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertUInt32OK("f64_21", 21.0, 21)
	assertUInt32OK("i64_21", int64(21), 21)
	assertUInt32OK("u64_21", uint64(21), 21)
	assertUInt32Err("f64_fractional", 21.5)
	assertUInt32Err("f64_nan", math.NaN())
	assertUInt32Err("f64_inf", math.Inf(1))
	assertUInt32Err("f64_neg_inf", math.Inf(-1))
	assertUInt32Err("u64_max", uint64(math.MaxUint64))
	assertUInt32Err("u64_4294967296", uint64(4294967296))
	assertUInt32Err("i64_neg21", int64(-21))
	assertUInt32Err("i64_min", int64(math.MinInt64))
	assertUInt32Err("i64_neg4294967296", int64(-4294967296))
}

func TestExactInt64ConversionParity(t *testing.T) {
	assertInt64OK := func(name string, v any, want int64) {
		t.Helper()
		got, err := MustFromAny(v).TryIntoInt64()
		if err != nil || got != want {
			t.Fatalf("%s: TryIntoInt64 got=%d err=%v want=%d", name, got, err, want)
		}
	}
	assertInt64Err := func(name string, v any) {
		t.Helper()
		if _, err := MustFromAny(v).TryIntoInt64(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertInt64OK("f64_21", 21.0, 21)
	assertInt64OK("i64_neg21", int64(-21), -21)
	assertInt64OK("i64_max", int64(math.MaxInt64), int64(math.MaxInt64))
	assertInt64OK("i64_min", int64(math.MinInt64), int64(math.MinInt64))
	assertInt64Err("f64_fractional", 21.5)
	assertInt64Err("f64_nan", math.NaN())
	assertInt64Err("f64_inf", math.Inf(1))
	assertInt64Err("f64_neg_inf", math.Inf(-1))
	assertInt64Err("u64_max", uint64(math.MaxUint64))
	assertInt64Err("u64_9223372036854775809", uint64(9223372036854775809))
}

func TestExactUInt64ConversionParity(t *testing.T) {
	assertUInt64OK := func(name string, v any, want uint64) {
		t.Helper()
		got, err := MustFromAny(v).TryIntoUInt64()
		if err != nil || got != want {
			t.Fatalf("%s: TryIntoUInt64 got=%d err=%v want=%d", name, got, err, want)
		}
	}
	assertUInt64Err := func(name string, v any) {
		t.Helper()
		if _, err := MustFromAny(v).TryIntoUInt64(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertUInt64OK("f64_21", 21.0, 21)
	assertUInt64OK("u64_21", uint64(21), 21)
	assertUInt64OK("u64_max", uint64(math.MaxUint64), uint64(math.MaxUint64))
	assertUInt64Err("f64_fractional", 21.5)
	assertUInt64Err("f64_nan", math.NaN())
	assertUInt64Err("f64_inf", math.Inf(1))
	assertUInt64Err("f64_neg_inf", math.Inf(-1))
	assertUInt64Err("i64_neg21", int64(-21))
	assertUInt64Err("i64_min", int64(math.MinInt64))
}

func TestTypedDecodeHelperParity(t *testing.T) {
	if got, err := DecodeUInt8(MustFromAny(255)); err != nil || got != 255 {
		t.Fatalf("DecodeUInt8 mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeUInt16(MustFromAny(65535)); err != nil || got != 65535 {
		t.Fatalf("DecodeUInt16 mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeUInt32(MustFromAny(uint64(4294967295))); err != nil || got != 4294967295 {
		t.Fatalf("DecodeUInt32 mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeUInt(MustFromAny(uint64(42))); err != nil || got != 42 {
		t.Fatalf("DecodeUInt mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeInt8(MustFromAny(-128)); err != nil || got != -128 {
		t.Fatalf("DecodeInt8 mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeInt16(MustFromAny(-32768)); err != nil || got != -32768 {
		t.Fatalf("DecodeInt16 mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeInt32(MustFromAny(-2147483648)); err != nil || got != -2147483648 {
		t.Fatalf("DecodeInt32 mismatch: got=%d err=%v", got, err)
	}
	if got, err := DecodeInt(MustFromAny(-42)); err != nil || got != -42 {
		t.Fatalf("DecodeInt mismatch: got=%d err=%v", got, err)
	}

	if _, err := DecodeUInt8(MustFromAny(256)); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for DecodeUInt8 overflow, got %v", err)
	}
	if _, err := DecodeUInt16(MustFromAny(-1)); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for DecodeUInt16(-1), got %v", err)
	}
	if _, err := DecodeUInt32(MustFromAny(uint64(4294967296))); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for DecodeUInt32 overflow, got %v", err)
	}
	if strconv.IntSize == 32 {
		if _, err := DecodeUInt(MustFromAny(uint64(math.MaxUint32) + 1)); !errors.Is(err, ErrOutOfRange) {
			t.Fatalf("expected ErrOutOfRange for DecodeUInt overflow on 32-bit, got %v", err)
		}
	}
	if _, err := DecodeInt8(MustFromAny(128)); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for DecodeInt8 overflow, got %v", err)
	}
	if _, err := DecodeInt16(MustFromAny(32768)); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for DecodeInt16 overflow, got %v", err)
	}
	if _, err := DecodeInt32(MustFromAny(-2147483649)); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for DecodeInt32 underflow, got %v", err)
	}
	if strconv.IntSize == 32 {
		if _, err := DecodeInt(MustFromAny(int64(math.MaxInt32) + 1)); !errors.Is(err, ErrOutOfRange) {
			t.Fatalf("expected ErrOutOfRange for DecodeInt overflow on 32-bit, got %v", err)
		}
	}
}

func TestExactFloat64ConversionParity(t *testing.T) {
	assertFloat64OK := func(name string, value any, want float64) {
		t.Helper()
		got, err := MustFromAny(value).TryIntoFloat64()
		if err != nil {
			t.Fatalf("%s: unexpected error: %v", name, err)
		}
		if math.IsNaN(want) {
			if !math.IsNaN(got) {
				t.Fatalf("%s: expected NaN, got %v", name, got)
			}
			return
		}
		if got != want {
			t.Fatalf("%s: got %v want %v", name, got, want)
		}
	}
	assertFloat64Err := func(name string, value any) {
		t.Helper()
		if _, err := MustFromAny(value).TryIntoFloat64(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertFloat64OK("f64_21", 21.0, 21.0)
	assertFloat64OK("f64_21_5", 21.5, 21.5)
	assertFloat64OK("f64_nan", math.NaN(), math.NaN())
	assertFloat64OK("f64_inf", math.Inf(1), math.Inf(1))
	assertFloat64OK("f64_neg_inf", math.Inf(-1), math.Inf(-1))

	assertFloat64OK("u64_21", uint64(21), 21.0)
	assertFloat64OK("u64_max", uint64(math.MaxUint64), 18446744073709551616.0)
	assertFloat64Err("u64_9223372036854775809", uint64(9223372036854775809))

	assertFloat64OK("i64_21", int64(21), 21.0)
	assertFloat64OK("i64_neg21", int64(-21), -21.0)
	assertFloat64Err("i64_max", int64(math.MaxInt64))
	assertFloat64Err("i64_min", int64(math.MinInt64))
	assertFloat64Err("i64_neg9223372036854775807", int64(-9223372036854775807))
}

func TestFloat32ConversionParity(t *testing.T) {
	assertFloat32OK := func(name string, value any, want float32) {
		t.Helper()
		got, err := MustFromAny(value).TryIntoFloat32()
		if err != nil {
			t.Fatalf("%s: unexpected error: %v", name, err)
		}
		if math.IsNaN(float64(want)) {
			if !math.IsNaN(float64(got)) {
				t.Fatalf("%s: expected NaN, got %v", name, got)
			}
			return
		}
		if got != want {
			t.Fatalf("%s: got %v want %v", name, got, want)
		}
	}
	assertFloat32Err := func(name string, value any) {
		t.Helper()
		if _, err := MustFromAny(value).TryIntoFloat32(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertFloat32OK("u64_21", uint64(21), float32(21.0))
	assertFloat32OK("i64_neg21", int64(-21), float32(-21.0))
	assertFloat32OK("f64_21_5", 21.5, float32(21.5))
	assertFloat32OK("f64_nan", math.NaN(), float32(math.NaN()))
	assertFloat32OK("f64_inf", math.Inf(1), float32(math.Inf(1)))
	assertFloat32OK("f64_neg_inf", math.Inf(-1), float32(math.Inf(-1)))
	assertFloat32OK("u64_max", uint64(math.MaxUint64), float32(18446744073709551616.0))

	assertFloat32Err("u64_9223372036854775809", uint64(9223372036854775809))
	assertFloat32Err("i64_neg9223372036854775807", int64(-9223372036854775807))
	assertFloat32Err("text", "42")

	if got, err := DecodeFloat32(MustFromAny(21.5)); err != nil || got != float32(21.5) {
		t.Fatalf("DecodeFloat32 mismatch: got=%v err=%v", got, err)
	}
	if got, ok := MustFromAny(21.5).IntoFloat32(); !ok || got != float32(21.5) {
		t.Fatalf("IntoFloat32 mismatch: got=%v ok=%v", got, ok)
	}
	if got, err := MustFromAny(21.5).TryFloat32(); err != nil || got != float32(21.5) {
		t.Fatalf("TryFloat32 mismatch: got=%v err=%v", got, err)
	}
	if _, ok := MustFromAny("x").IntoFloat32(); ok {
		t.Fatalf("expected IntoFloat32 to fail for non-number")
	}
}

func TestBigIntConversionParity(t *testing.T) {
	unsigned := MustFromAny(uint64(math.MaxUint64))
	bigUnsigned, err := unsigned.TryIntoBigInt()
	if err != nil {
		t.Fatalf("TryIntoBigInt unsigned failed: %v", err)
	}
	if got, want := bigUnsigned.String(), "18446744073709551615"; got != want {
		t.Fatalf("unsigned big.Int mismatch: got %q want %q", got, want)
	}

	negative, err := TryFromHex("3bfffffffffffffffe") // -18446744073709551615
	if err != nil {
		t.Fatalf("TryFromHex failed: %v", err)
	}
	bigNegative, err := negative.TryIntoBigInt()
	if err != nil {
		t.Fatalf("TryIntoBigInt negative failed: %v", err)
	}
	if got, want := bigNegative.String(), "-18446744073709551615"; got != want {
		t.Fatalf("negative big.Int mismatch: got %q want %q", got, want)
	}

	if got, err := DecodeBigInt(negative); err != nil || got.String() != "-18446744073709551615" {
		t.Fatalf("DecodeBigInt mismatch: got=%v err=%v", got, err)
	}

	if got, err := unsigned.TryBigInt(); err != nil || got.String() != "18446744073709551615" {
		t.Fatalf("TryBigInt mismatch: got=%v err=%v", got, err)
	}
	if got, ok := unsigned.IntoBigInt(); !ok || got.String() != "18446744073709551615" {
		t.Fatalf("IntoBigInt mismatch: got=%v ok=%v", got, ok)
	}
}

func TestBigUintConversionParity(t *testing.T) {
	unsigned := MustFromAny(uint64(math.MaxUint64))
	bigUnsigned, err := unsigned.TryIntoBigUint()
	if err != nil {
		t.Fatalf("TryIntoBigUint unsigned failed: %v", err)
	}
	if got, want := bigUnsigned.String(), "18446744073709551615"; got != want {
		t.Fatalf("big uint mismatch: got %q want %q", got, want)
	}
	if got, err := DecodeBigUint(unsigned); err != nil || got.String() != "18446744073709551615" {
		t.Fatalf("DecodeBigUint mismatch: got=%v err=%v", got, err)
	}
	if got, err := unsigned.TryBigUint(); err != nil || got.String() != "18446744073709551615" {
		t.Fatalf("TryBigUint mismatch: got=%v err=%v", got, err)
	}
	if got, ok := unsigned.IntoBigUint(); !ok || got.String() != "18446744073709551615" {
		t.Fatalf("IntoBigUint mismatch: got=%v ok=%v", got, ok)
	}

	negative := MustFromAny(-1)
	if _, err := negative.TryIntoBigUint(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for negative->big uint, got %v", err)
	}
	if _, ok := negative.IntoBigUint(); ok {
		t.Fatalf("expected IntoBigUint to fail for negative input")
	}
	if _, err := MustFromAny("x").TryIntoBigUint(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for text->big uint, got %v", err)
	}
}

func TestBigNumTaggedConversionParity(t *testing.T) {
	positiveTagged, err := TryFromHex("c249010000000000000000") // 2^64
	if err != nil {
		t.Fatalf("TryFromHex positive bignum failed: %v", err)
	}
	if got, err := positiveTagged.TryIntoBigUint(); err != nil || got.String() != "18446744073709551616" {
		t.Fatalf("TryIntoBigUint tagged mismatch: got=%v err=%v", got, err)
	}
	if got, err := positiveTagged.TryIntoBigInt(); err != nil || got.String() != "18446744073709551616" {
		t.Fatalf("TryIntoBigInt tagged positive mismatch: got=%v err=%v", got, err)
	}

	negativeTagged, err := TryFromHex("c349010000000000000000") // -2^64 - 1
	if err != nil {
		t.Fatalf("TryFromHex negative bignum failed: %v", err)
	}
	if got, err := negativeTagged.TryIntoBigInt(); err != nil || got.String() != "-18446744073709551617" {
		t.Fatalf("TryIntoBigInt tagged negative mismatch: got=%v err=%v", got, err)
	}
	if _, err := negativeTagged.TryIntoBigUint(); !errors.Is(err, ErrOutOfRange) {
		t.Fatalf("expected ErrOutOfRange for negative tagged bignum to big uint, got %v", err)
	}

	nonCanonicalPositive, err := TryFromHex("c2420001")
	if err != nil {
		t.Fatalf("TryFromHex non-canonical positive failed: %v", err)
	}
	if _, err := nonCanonicalPositive.TryIntoBigInt(); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric for leading-zero positive bignum, got %v", err)
	}

	nonCanonicalNegative, err := TryFromHex("c340")
	if err != nil {
		t.Fatalf("TryFromHex non-canonical negative failed: %v", err)
	}
	if _, err := nonCanonicalNegative.TryIntoBigInt(); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric for empty negative bignum, got %v", err)
	}
}

func TestBigIntFromAnyTaggedEncodingParity(t *testing.T) {
	positive := new(big.Int).Lsh(big.NewInt(1), 64)
	cPositive, err := FromAny(positive)
	if err != nil {
		t.Fatalf("FromAny positive big.Int failed: %v", err)
	}
	if got, want := cPositive.DiagnosticFlat(), "2(h'010000000000000000')"; got != want {
		t.Fatalf("positive big.Int encoding mismatch: got %q want %q", got, want)
	}
	if roundTrip, err := cPositive.TryIntoBigInt(); err != nil || roundTrip.Cmp(positive) != 0 {
		t.Fatalf("positive big.Int round-trip mismatch: got=%v err=%v", roundTrip, err)
	}

	negative := new(big.Int).Neg(new(big.Int).Add(new(big.Int).Lsh(big.NewInt(1), 64), big.NewInt(1)))
	cNegative, err := FromAny(negative)
	if err != nil {
		t.Fatalf("FromAny negative big.Int failed: %v", err)
	}
	if got, want := cNegative.DiagnosticFlat(), "3(h'010000000000000000')"; got != want {
		t.Fatalf("negative big.Int encoding mismatch: got %q want %q", got, want)
	}
	if roundTrip, err := cNegative.TryIntoBigInt(); err != nil || roundTrip.Cmp(negative) != 0 {
		t.Fatalf("negative big.Int round-trip mismatch: got=%v err=%v", roundTrip, err)
	}

	zero := big.NewInt(0)
	cZero, err := FromAny(zero)
	if err != nil {
		t.Fatalf("FromAny zero big.Int failed: %v", err)
	}
	if got, want := cZero.DiagnosticFlat(), "2(h'')"; got != want {
		t.Fatalf("zero big.Int encoding mismatch: got %q want %q", got, want)
	}
}

func TestBigIntRoundTripWithinCBORRange(t *testing.T) {
	values := []*big.Int{
		big.NewInt(0),
		new(big.Int).SetUint64(math.MaxUint64),
		new(big.Int).Neg(new(big.Int).SetUint64(math.MaxUint64)),
	}
	for _, value := range values {
		var c CBOR
		if value.Sign() >= 0 {
			c = MustFromAny(value.Uint64())
		} else {
			neg := new(big.Int).Neg(value)
			neg.Sub(neg, big.NewInt(1))
			c = NewCBORNegative(neg.Uint64())
		}
		decoded, err := c.TryIntoBigInt()
		if err != nil {
			t.Fatalf("TryIntoBigInt failed for %s: %v", value.String(), err)
		}
		if decoded.Cmp(value) != 0 {
			t.Fatalf("big.Int round-trip mismatch: got %s want %s", decoded.String(), value.String())
		}
	}
}

func TestFloat16ConversionParity(t *testing.T) {
	assertFloat16OK := func(name string, value any, want float64) {
		t.Helper()
		got, err := MustFromAny(value).TryIntoFloat16()
		if err != nil {
			t.Fatalf("%s: unexpected error: %v", name, err)
		}
		if math.IsNaN(want) {
			if !got.IsNaN() {
				t.Fatalf("%s: expected NaN, got bits=0x%04x", name, got.Bits())
			}
			return
		}
		if got.Float64() != want {
			t.Fatalf("%s: got %v want %v", name, got.Float64(), want)
		}
	}
	assertFloat16Err := func(name string, value any) {
		t.Helper()
		if _, err := MustFromAny(value).TryIntoFloat16(); err == nil {
			t.Fatalf("%s: expected conversion failure", name)
		}
	}

	assertFloat16OK("f64_21", 21.0, 21.0)
	assertFloat16OK("f64_21_5", 21.5, 21.5)
	assertFloat16OK("f64_nan", math.NaN(), math.NaN())
	assertFloat16OK("f64_inf", math.Inf(1), math.Inf(1))
	assertFloat16OK("f64_neg_inf", math.Inf(-1), math.Inf(-1))
	assertFloat16OK("u64_21", uint64(21), 21.0)
	assertFloat16Err("u64_max", uint64(math.MaxUint64))
	assertFloat16Err("u64_65536", uint64(65536))
	assertFloat16OK("i64_21", int64(21), 21.0)
	assertFloat16OK("i64_neg21", int64(-21), -21.0)
	assertFloat16Err("i64_max", int64(math.MaxInt64))
	assertFloat16Err("i64_min", int64(math.MinInt64))
	assertFloat16Err("i64_neg65536", int64(-65536))

	if got, err := DecodeFloat16(MustFromAny(21.5)); err != nil || got.Float64() != 21.5 {
		t.Fatalf("DecodeFloat16 mismatch: got=%v err=%v", got.Float64(), err)
	}
	if got, err := MustFromAny(21.5).TryFloat16(); err != nil || got.Float64() != 21.5 {
		t.Fatalf("TryFloat16 mismatch: got=%v err=%v", got.Float64(), err)
	}
	if got, ok := MustFromAny(21.5).IntoFloat16(); !ok || got.Float64() != 21.5 {
		t.Fatalf("IntoFloat16 mismatch: got=%v ok=%v", got.Float64(), ok)
	}
	if _, ok := MustFromAny("x").IntoFloat16(); ok {
		t.Fatalf("expected IntoFloat16 to fail for non-number")
	}
}
