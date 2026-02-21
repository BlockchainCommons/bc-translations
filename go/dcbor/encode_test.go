package dcbor

import (
	"bytes"
	"errors"
	"math"
	"testing"
)

func assertEncodedCBOR(t *testing.T, cbor CBOR, expectedDebug, expectedDisplay, expectedHex string) {
	t.Helper()

	if got := cbor.DebugString(); got != expectedDebug {
		t.Fatalf("debug mismatch: got %q want %q", got, expectedDebug)
	}
	if got := cbor.String(); got != expectedDisplay {
		t.Fatalf("display mismatch: got %q want %q", got, expectedDisplay)
	}
	if got := cbor.Hex(); got != expectedHex {
		t.Fatalf("hex mismatch: got %q want %q", got, expectedHex)
	}

	decoded, err := TryFromData(cbor.ToCBORData())
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	if !decoded.Equal(cbor) {
		t.Fatalf("round-trip mismatch: got %s want %s", decoded.DiagnosticFlat(), cbor.DiagnosticFlat())
	}
}

func assertEncodedFromAny(t *testing.T, value any, expectedDebug, expectedDisplay, expectedHex string) {
	t.Helper()
	cbor, err := FromAny(value)
	if err != nil {
		t.Fatalf("FromAny failed: %v", err)
	}
	assertEncodedCBOR(t, cbor, expectedDebug, expectedDisplay, expectedHex)
}

func assertDecodedCBOR(t *testing.T, hexValue, expectedDebug, expectedDisplay string) {
	t.Helper()
	cbor, err := TryFromHex(hexValue)
	if err != nil {
		t.Fatalf("TryFromHex failed: %v", err)
	}
	if got := cbor.DebugString(); got != expectedDebug {
		t.Fatalf("debug mismatch: got %q want %q", got, expectedDebug)
	}
	if got := cbor.String(); got != expectedDisplay {
		t.Fatalf("display mismatch: got %q want %q", got, expectedDisplay)
	}
}

func TestEncodeUnsignedVectors(t *testing.T) {
	assertEncodedFromAny(t, uint64(0), "unsigned(0)", "0", "00")
	assertEncodedFromAny(t, uint64(1), "unsigned(1)", "1", "01")
	assertEncodedFromAny(t, uint64(23), "unsigned(23)", "23", "17")
	assertEncodedFromAny(t, uint64(24), "unsigned(24)", "24", "1818")
	assertEncodedFromAny(t, uint64(math.MaxUint8), "unsigned(255)", "255", "18ff")
	assertEncodedFromAny(t, uint64(math.MaxUint16), "unsigned(65535)", "65535", "19ffff")
	assertEncodedFromAny(t, uint64(65536), "unsigned(65536)", "65536", "1a00010000")
	assertEncodedFromAny(t, uint64(math.MaxUint32), "unsigned(4294967295)", "4294967295", "1affffffff")
	assertEncodedFromAny(t, uint64(math.MaxUint64), "unsigned(18446744073709551615)", "18446744073709551615", "1bffffffffffffffff")
}

func TestEncodeSignedVectors(t *testing.T) {
	assertEncodedFromAny(t, int64(-1), "negative(-1)", "-1", "20")
	assertEncodedFromAny(t, int64(-2), "negative(-2)", "-2", "21")
	assertEncodedFromAny(t, int64(-127), "negative(-127)", "-127", "387e")
	assertEncodedFromAny(t, int64(math.MinInt8), "negative(-128)", "-128", "387f")
	assertEncodedFromAny(t, int64(math.MinInt16), "negative(-32768)", "-32768", "397fff")
	assertEncodedFromAny(t, int64(math.MinInt32), "negative(-2147483648)", "-2147483648", "3a7fffffff")
	assertEncodedFromAny(t, int64(math.MinInt64), "negative(-9223372036854775808)", "-9223372036854775808", "3b7fffffffffffffff")
}

func TestEncodeBytesAndTextVectors(t *testing.T) {
	assertEncodedFromAny(t, []byte{0x00, 0x11, 0x22, 0x33}, "bytes(00112233)", "h'00112233'", "4400112233")
	assertEncodedFromAny(t, "Hello", `text("Hello")`, `"Hello"`, "6548656c6c6f")
}

func TestNormalizedStringEncodingMatchesRustBehavior(t *testing.T) {
	composed := "\u00E9"
	decomposed := "\u0065\u0301"

	cbor1 := MustFromAny(composed).ToCBORData()
	cbor2 := MustFromAny(decomposed).ToCBORData()
	if !bytes.Equal(cbor1, cbor2) {
		t.Fatalf("NFC normalization mismatch: got %x and %x", cbor1, cbor2)
	}

	_, err := TryFromHex("6365cc81")
	if !errors.Is(err, ErrNonCanonicalString) {
		t.Fatalf("expected ErrNonCanonicalString, got %v", err)
	}
}

func TestEncodeArrayVectors(t *testing.T) {
	assertEncodedCBOR(t, NewCBORArray(nil), "array([])", "[]", "80")
	assertEncodedCBOR(
		t,
		NewCBORArray([]CBOR{MustFromAny(1), MustFromAny(2), MustFromAny(3)}),
		"array([unsigned(1), unsigned(2), unsigned(3)])",
		"[1, 2, 3]",
		"83010203",
	)
	assertEncodedCBOR(
		t,
		NewCBORArray([]CBOR{MustFromAny(1), MustFromAny("Hello"), NewCBORArray([]CBOR{MustFromAny(1), MustFromAny(2), MustFromAny(3)})}),
		`array([unsigned(1), text("Hello"), array([unsigned(1), unsigned(2), unsigned(3)])])`,
		`[1, "Hello", [1, 2, 3]]`,
		"83016548656c6c6f83010203",
	)
}

func TestEncodeMapKeyOrderVectors(t *testing.T) {
	m := NewMap()
	m.MustInsertAny(-1, 3)
	m.Insert(NewCBORArray([]CBOR{MustFromAny(-1)}), MustFromAny(7))
	m.MustInsertAny("z", 4)
	m.MustInsertAny(10, 1)
	m.MustInsertAny(false, 8)
	m.MustInsertAny(100, 2)
	m.MustInsertAny("aa", 5)
	m.Insert(NewCBORArray([]CBOR{MustFromAny(100)}), MustFromAny(6))

	if got, want := m.GetAny(false); !want || !got.Equal(MustFromAny(8)) {
		t.Fatalf("GetAny failed for false key")
	}
	if got, err := m.ExtractAny("z"); err != nil || !got.Equal(MustFromAny(4)) {
		t.Fatalf("ExtractAny failed for \"z\": value=%v err=%v", got, err)
	}
	if _, err := m.ExtractAny("foo"); !errors.Is(err, ErrMissingMapKey) {
		t.Fatalf("expected ErrMissingMapKey, got %v", err)
	}

	assertEncodedCBOR(
		t,
		NewCBORMap(m),
		`map({0x0a: (unsigned(10), unsigned(1)), 0x1864: (unsigned(100), unsigned(2)), 0x20: (negative(-1), unsigned(3)), 0x617a: (text("z"), unsigned(4)), 0x626161: (text("aa"), unsigned(5)), 0x811864: (array([unsigned(100)]), unsigned(6)), 0x8120: (array([negative(-1)]), unsigned(7)), 0xf4: (simple(false), unsigned(8))})`,
		`{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}`,
		"a80a011864022003617a046261610581186406812007f408",
	)
}

func TestEncodeMapWithMapKeys(t *testing.T) {
	k1 := NewMap()
	k1.MustInsertAny(1, 2)
	k2 := NewMap()
	k2.MustInsertAny(3, 4)

	m := NewMap()
	m.Insert(NewCBORMap(k1), MustFromAny(5))
	m.Insert(NewCBORMap(k2), MustFromAny(6))

	assertEncodedCBOR(
		t,
		NewCBORMap(m),
		`map({0xa10102: (map({0x01: (unsigned(1), unsigned(2))}), unsigned(5)), 0xa10304: (map({0x03: (unsigned(3), unsigned(4))}), unsigned(6))})`,
		"{{1: 2}: 5, {3: 4}: 6}",
		"a2a1010205a1030406",
	)
}

func TestEncodeTaggedValue(t *testing.T) {
	assertEncodedCBOR(
		t,
		ToTaggedValue(TagWithValue(1), MustFromAny("Hello")),
		`tagged(1, text("Hello"))`,
		`1("Hello")`,
		"c16548656c6c6f",
	)
}

func TestEncodeFloatVectors(t *testing.T) {
	assertEncodedFromAny(t, 1.5, "simple(1.5)", "1.5", "f93e00")
	assertEncodedFromAny(t, 2345678.25, "simple(2345678.25)", "2345678.25", "fa4a0f2b39")
	assertEncodedFromAny(t, 1.2, "simple(1.2)", "1.2", "fb3ff3333333333333")

	assertEncodedFromAny(t, float32(42.0), "unsigned(42)", "42", "182a")
	assertEncodedFromAny(t, 2345678.0, "unsigned(2345678)", "2345678", "1a0023cace")
	assertEncodedFromAny(t, -2345678.0, "negative(-2345678)", "-2345678", "3a0023cacd")
	assertEncodedFromAny(t, math.Copysign(0.0, -1.0), "unsigned(0)", "0", "00")

	assertEncodedFromAny(
		t,
		18446744073709550000.0,
		"unsigned(18446744073709549568)",
		"18446744073709549568",
		"1bfffffffffffff800",
	)
	assertEncodedFromAny(
		t,
		18446744073709552000.0,
		"simple(1.8446744073709552e19)",
		"1.8446744073709552e19",
		"fa5f800000",
	)
	assertEncodedFromAny(
		t,
		-18446744073709551616.0,
		"negative(-18446744073709551616)",
		"-18446744073709551616",
		"3bffffffffffffffff",
	)
	assertEncodedFromAny(
		t,
		-18446744073709555712.0,
		"simple(-1.8446744073709556e19)",
		"-1.8446744073709556e19",
		"fbc3f0000000000001",
	)
}

func TestEncodeDecodeNaNAndInfinityCanonicalization(t *testing.T) {
	nonStandardNaN64 := math.Float64frombits(0x7ff9100000000001)
	if !math.IsNaN(nonStandardNaN64) {
		t.Fatal("expected float64 NaN")
	}
	assertEncodedFromAny(t, nonStandardNaN64, "simple(NaN)", "NaN", "f97e00")

	nonStandardNaN32 := math.Float32frombits(0xffc00001)
	if !math.IsNaN(float64(nonStandardNaN32)) {
		t.Fatal("expected float32 NaN")
	}
	assertEncodedFromAny(t, nonStandardNaN32, "simple(NaN)", "NaN", "f97e00")

	assertEncodedFromAny(t, math.Inf(1), "simple(Infinity)", "Infinity", "f97c00")
	assertEncodedFromAny(t, math.Inf(-1), "simple(-Infinity)", "-Infinity", "f9fc00")

	decodedNaN, err := TryFromHex("f97e00")
	if err != nil {
		t.Fatalf("TryFromHex canonical NaN failed: %v", err)
	}
	if !decodedNaN.IsNaN() {
		t.Fatalf("expected decoded NaN")
	}

	if _, err := TryFromHex("f97e01"); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric for non-canonical half NaN, got %v", err)
	}
	if _, err := TryFromHex("faffc00001"); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric for non-canonical single NaN, got %v", err)
	}
	if _, err := TryFromHex("fb7ff9100000000001"); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric for non-canonical double NaN, got %v", err)
	}
}

func TestDecodeNonCanonicalAndLargeNegativeVectors(t *testing.T) {
	if _, err := TryFromHex("FB3FF8000000000000"); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric, got %v", err)
	}
	if _, err := TryFromHex("F94A00"); !errors.Is(err, ErrNonCanonicalNumeric) {
		t.Fatalf("expected ErrNonCanonicalNumeric, got %v", err)
	}
	if _, err := TryFromHex("0001"); err == nil || err.Error() != "the decoded CBOR had 1 extra bytes at the end" {
		t.Fatalf("expected trailing data error, got %v", err)
	}

	assertDecodedCBOR(
		t,
		"3b8000000000000000",
		"negative(-9223372036854775809)",
		"-9223372036854775809",
	)
	assertDecodedCBOR(
		t,
		"3bfffffffffffffffe",
		"negative(-18446744073709551615)",
		"-18446744073709551615",
	)
}

func TestEncodeDateUsesRegisteredTagName(t *testing.T) {
	RegisterTags()

	date := DateFromTimestamp(-100.0)
	assertEncodedCBOR(
		t,
		date.TaggedCBOR(),
		"tagged(date, negative(-100))",
		"date(-100)",
		"c13863",
	)

	fractionalDate := DateFromTimestamp(0.5)
	if got, want := fractionalDate.String(), "1970-01-01"; got != want {
		t.Fatalf("date string mismatch: got %q want %q", got, want)
	}
}
