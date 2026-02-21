package dcbor

import (
	"errors"
	"math"
	"testing"
)

func TestEncodeDecodeCoreScalars(t *testing.T) {
	tests := []struct {
		name       string
		value      any
		expected   string
		diagnostic string
	}{
		{name: "unsigned", value: uint64(24), expected: "1818", diagnostic: "24"},
		{name: "negative", value: int64(-1), expected: "20", diagnostic: "-1"},
		{name: "bytes", value: []byte{0x01, 0x02, 0x03}, expected: "43010203", diagnostic: "h'010203'"},
		{name: "text", value: "hi", expected: "626869", diagnostic: "\"hi\""},
		{name: "true", value: true, expected: "f5", diagnostic: "true"},
		{name: "null", value: nil, expected: "f6", diagnostic: "null"},
	}

	for _, tc := range tests {
		t.Run(tc.name, func(t *testing.T) {
			cborValue, err := FromAny(tc.value)
			if err != nil {
				t.Fatalf("FromAny failed: %v", err)
			}
			actualHex := cborValue.Hex()
			if actualHex != tc.expected {
				t.Fatalf("hex mismatch: got %s want %s", actualHex, tc.expected)
			}
			if cborValue.Diagnostic() != tc.diagnostic {
				t.Fatalf("diagnostic mismatch: got %q want %q", cborValue.Diagnostic(), tc.diagnostic)
			}

			decoded, err := TryFromHex(tc.expected)
			if err != nil {
				t.Fatalf("TryFromHex failed: %v", err)
			}
			if !decoded.Equal(cborValue) {
				t.Fatalf("round-trip mismatch: got %s want %s", decoded.DiagnosticFlat(), cborValue.DiagnosticFlat())
			}
		})
	}
}

func TestMapEncodingIsDeterministicallySortedByKeyEncoding(t *testing.T) {
	m := NewMap()
	keyB, _ := FromAny("b")
	keyA, _ := FromAny("a")
	valueTwo, _ := FromAny(2)
	valueOne, _ := FromAny(1)

	m.Insert(keyB, valueTwo)
	m.Insert(keyA, valueOne)

	cborMap := NewCBORMap(m)
	if got, want := cborMap.Hex(), "a2616101616202"; got != want {
		t.Fatalf("deterministic ordering mismatch: got %s want %s", got, want)
	}
}

func TestDecodeRejectsMisorderedMapKeys(t *testing.T) {
	_, err := TryFromHex("a2616202616101")
	if err == nil {
		t.Fatal("expected decode error for misordered map keys")
	}
	if !errors.Is(err, ErrMisorderedMapKey) {
		t.Fatalf("expected ErrMisorderedMapKey, got %v", err)
	}
}

func TestDiagnosticExpectedTextOutput(t *testing.T) {
	activeKey, _ := FromAny("active")
	nameKey, _ := FromAny("name")
	rolesKey, _ := FromAny("roles")

	activeValue, _ := FromAny(true)
	nameValue, _ := FromAny("Alice")
	roleOne, _ := FromAny("admin")
	roleTwo, _ := FromAny("operator")
	roles := NewCBORArray([]CBOR{roleOne, roleTwo})

	m := NewMap()
	m.Insert(nameKey, nameValue)
	m.Insert(rolesKey, roles)
	m.Insert(activeKey, activeValue)

	actual := NewCBORMap(m).Diagnostic()

	// expected-text-output-rubric:
	const expected = `{
    "name": "Alice",
    "roles": ["admin", "operator"],
    "active": true
}`

	assertActualExpected(t, actual, expected)
}

func TestDateTagRoundTrip(t *testing.T) {
	date, err := DateFromString("2023-02-08T00:00:00Z")
	if err != nil {
		t.Fatalf("DateFromString failed: %v", err)
	}

	tagged := date.TaggedCBOR()
	decoded, err := TryFromData(tagged.ToCBORData())
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}

	restored, err := DateFromTaggedCBOR(decoded)
	if err != nil {
		t.Fatalf("DateFromTaggedCBOR failed: %v", err)
	}

	if restored.String() != date.String() {
		t.Fatalf("date mismatch: got %s want %s", restored.String(), date.String())
	}
}

func TestHexAnnotatedIncludesComment(t *testing.T) {
	value, _ := FromAny("hello")
	annotated := value.HexAnnotated()
	if annotated == value.Hex() {
		t.Fatalf("expected annotated hex to differ from plain hex")
	}
	if annotated == "" {
		t.Fatalf("expected annotated hex output")
	}
}

func TestSimpleValueConvenienceParity(t *testing.T) {
	simple, err := False().TryIntoSimpleValue()
	if err != nil {
		t.Fatalf("TryIntoSimpleValue failed: %v", err)
	}
	if simple.Kind() != SimpleFalse {
		t.Fatalf("unexpected simple kind: got %v want %v", simple.Kind(), SimpleFalse)
	}

	simpleAlias, err := True().TrySimpleValue()
	if err != nil {
		t.Fatalf("TrySimpleValue failed: %v", err)
	}
	if simpleAlias.Kind() != SimpleTrue {
		t.Fatalf("unexpected simple alias kind: got %v want %v", simpleAlias.Kind(), SimpleTrue)
	}

	if _, ok := Null().IntoSimpleValue(); !ok {
		t.Fatalf("IntoSimpleValue unexpectedly failed for null")
	}

	if _, err := MustFromAny(1).TryIntoSimpleValue(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for non-simple value, got %v", err)
	}
	if _, ok := MustFromAny("text").IntoSimpleValue(); ok {
		t.Fatalf("expected IntoSimpleValue to fail for text")
	}

	if !SimpleTrueValue().Equal(SimpleTrueValue()) {
		t.Fatalf("Simple.Equal expected true for identical non-float kind")
	}
	if SimpleTrueValue().Equal(SimpleFalseValue()) {
		t.Fatalf("Simple.Equal expected false for differing non-float kinds")
	}
	if !SimpleFloatValue(1.5).Equal(SimpleFloatValue(1.5)) {
		t.Fatalf("Simple.Equal expected true for identical float value")
	}
	if SimpleFloatValue(1.5).Equal(SimpleFloatValue(1.25)) {
		t.Fatalf("Simple.Equal expected false for differing float values")
	}
	if SimpleFloatValue(math.NaN()).Equal(SimpleFloatValue(math.NaN())) {
		t.Fatalf("Simple.Equal expected false for NaN parity to match floating-point equality semantics")
	}
}
