package bcenvelope

import (
	"testing"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func TestReadLegacyLeaf(t *testing.T) {
	// tag 200 (envelope) wrapping tag 24 (encoded CBOR) wrapping integer 42
	legacyEnvelope, err := EnvelopeFromCBORData(mustDecodeHex("d8c8d818182a"))
	if err != nil {
		t.Fatalf("Failed to decode legacy envelope: %v", err)
	}
	e := NewEnvelope(42)
	if !legacyEnvelope.IsIdenticalTo(e) {
		t.Errorf("legacy envelope not identical to new envelope")
	}
	if !legacyEnvelope.IsEquivalentTo(e) {
		t.Errorf("legacy envelope not equivalent to new envelope")
	}
}

func TestIntSubject(t *testing.T) {
	e := checkEncoding(t, NewEnvelope(42))

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    201(42)   / leaf /
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(7f83f7bda2d63959d34767689f06d47576683d378d9eb8d09386c9a020395c53)")

	assertActualExpected(t, e.Format(), "42")

	v, err := ExtractSubject[int](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if v != 42 {
		t.Errorf("expected 42, got %d", v)
	}
}

func TestNegativeIntSubject(t *testing.T) {
	e := checkEncoding(t, NewEnvelope(-42))

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    201(-42)   / leaf /
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(9e0ad272780de7aa1dbdfbc99058bb81152f623d3b95b5dfb0a036badfcc9055)")

	assertActualExpected(t, e.Format(), "-42")

	v, err := ExtractSubject[int](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if v != -42 {
		t.Errorf("expected -42, got %d", v)
	}
}

func TestCBOREncodableSubject(t *testing.T) {
	e := checkEncoding(t, helloEnvelope())

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    201("Hello.")   / leaf /
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59)")

	assertActualExpected(t, e.Format(), `"Hello."`)

	v, err := ExtractSubject[string](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if v != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, v)
	}
}

func TestKnownValueSubject(t *testing.T) {
	e := checkEncoding(t, knownValueEnvelope())

	assertActualExpected(t, e.DiagnosticAnnotated(), "200(4)   / envelope /")

	assertActualExpected(t, e.Digest().String(),
		"Digest(0fcd6a39d6ed37f2e2efa6a96214596f1b28a5cd42a5a27afc32162aaf821191)")

	assertActualExpected(t, e.Format(), "'note'")

	kv, err := ExtractSubject[knownvalues.KnownValue](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if !kv.Equal(knownvalues.Note) {
		t.Errorf("expected Note known value, got %v", kv)
	}
}

func TestAssertionSubject(t *testing.T) {
	e := checkEncoding(t, assertionEnvelope())

	pred, err := e.TryPredicate()
	if err != nil {
		t.Fatalf("TryPredicate failed: %v", err)
	}
	assertActualExpected(t, pred.Digest().String(),
		"Digest(db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba)")

	obj, err := e.TryObject()
	if err != nil {
		t.Fatalf("TryObject failed: %v", err)
	}
	assertActualExpected(t, obj.Digest().String(),
		"Digest(13b741949c37b8e09cc3daa3194c58e4fd6b2f14d4b1d0f035a46d6d5a1d3f11)")

	assertActualExpected(t, e.Subject().Digest().String(),
		"Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)")

	assertActualExpected(t, e.Digest().String(),
		"Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)")

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    {
        201("knows"):   / leaf /
        201("Bob")   / leaf /
    }
)`)

	assertActualExpected(t, e.Format(), `"knows": "Bob"`)

	if !e.Digest().Equal(NewAssertionEnvelope("knows", "Bob").Digest()) {
		t.Errorf("assertion digests should match")
	}
}

func TestSubjectWithAssertion(t *testing.T) {
	e := checkEncoding(t, singleAssertionEnvelope())

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    [
        201("Alice"),   / leaf /
        {
            201("knows"):   / leaf /
            201("Bob")   / leaf /
        }
    ]
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(8955db5e016affb133df56c11fe6c5c82fa3036263d651286d134c7e56c0e9f2)")

	assertActualExpected(t, e.Format(),
		`"Alice" [
    "knows": "Bob"
]`)

	v, err := ExtractSubject[string](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if v != "Alice" {
		t.Errorf("expected Alice, got %s", v)
	}
}

func TestSubjectWithTwoAssertions(t *testing.T) {
	e := checkEncoding(t, doubleAssertionEnvelope())

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    [
        201("Alice"),   / leaf /
        {
            201("knows"):   / leaf /
            201("Carol")   / leaf /
        },
        {
            201("knows"):   / leaf /
            201("Bob")   / leaf /
        }
    ]
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471)")

	assertActualExpected(t, e.Format(),
		`"Alice" [
    "knows": "Bob"
    "knows": "Carol"
]`)

	v, err := ExtractSubject[string](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if v != "Alice" {
		t.Errorf("expected Alice, got %s", v)
	}
}

func TestWrapped(t *testing.T) {
	e := checkEncoding(t, wrappedEnvelope())

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    200(   / envelope /
        201("Hello.")   / leaf /
    )
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(172a5e51431062e7b13525cbceb8ad8475977444cf28423e21c0d1dcbdfcaf47)")

	assertActualExpected(t, e.Format(),
		`{
    "Hello."
}`)
}

func TestDoubleWrapped(t *testing.T) {
	e := checkEncoding(t, doubleWrappedEnvelope())

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    200(   / envelope /
        200(   / envelope /
            201("Hello.")   / leaf /
        )
    )
)`)

	assertActualExpected(t, e.Digest().String(),
		"Digest(8b14f3bcd7c05aac8f2162e7047d7ef5d5eab7d82ee3f9dc4846c70bae4d200b)")

	assertActualExpected(t, e.Format(),
		`{
    {
        "Hello."
    }
}`)
}

func TestAssertionWithAssertions(t *testing.T) {
	a := NewAssertionEnvelope(1, 2).
		AddAssertion(3, 4).
		AddAssertion(5, 6)
	e, err := NewEnvelope(7).AddAssertionEnvelope(EnvelopeEncodableEnvelope{a})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}

	assertActualExpected(t, e.Format(),
		`7 [
    {
        1: 2
    } [
        3: 4
        5: 6
    ]
]`)
}

func TestDigestLeaf(t *testing.T) {
	digest := helloEnvelope().Digest()
	e := checkEncoding(t, NewEnvelope(digest))

	assertActualExpected(t, e.Format(), "Digest(8cc96cdb)")

	assertActualExpected(t, e.Digest().String(),
		"Digest(07b518af92a6196bc153752aabefedb34ff8e1a7d820c01ef978dfc3e7e52e05)")

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    201(   / leaf /
        40001(   / digest /
            h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'
        )
    )
)`)
}

func TestTrue(t *testing.T) {
	RegisterTags()
	e := checkEncoding(t, NewEnvelope(true))
	if !e.IsBool() {
		t.Error("expected IsBool() == true")
	}
	if !e.IsTrue() {
		t.Error("expected IsTrue() == true")
	}
	if e.IsFalse() {
		t.Error("expected IsFalse() == false")
	}
	if !e.Digest().Equal(TrueEnvelope().Digest()) {
		t.Error("expected digest to match TrueEnvelope()")
	}
	assertActualExpected(t, e.Format(), "true")
}

func TestFalse(t *testing.T) {
	RegisterTags()
	e := checkEncoding(t, NewEnvelope(false))
	if !e.IsBool() {
		t.Error("expected IsBool() == true")
	}
	if e.IsTrue() {
		t.Error("expected IsTrue() == false")
	}
	if !e.IsFalse() {
		t.Error("expected IsFalse() == true")
	}
	if !e.Digest().Equal(FalseEnvelope().Digest()) {
		t.Error("expected digest to match FalseEnvelope()")
	}
	assertActualExpected(t, e.Format(), "false")
}

func TestUnit(t *testing.T) {
	RegisterTags()
	e := checkEncoding(t, UnitEnvelope())
	if !e.IsSubjectUnit() {
		t.Error("expected IsSubjectUnit() == true")
	}
	assertActualExpected(t, e.Format(), "''")

	e = e.AddAssertion("foo", "bar")
	if !e.IsSubjectUnit() {
		t.Error("expected IsSubjectUnit() == true after adding assertion")
	}
	assertActualExpected(t, e.Format(),
		`'' [
    "foo": "bar"
]`)

	subject, err := ExtractSubject[knownvalues.KnownValue](e)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if !subject.Equal(knownvalues.Unit) {
		t.Errorf("expected Unit known value, got %v", subject)
	}
}

func TestPosition(t *testing.T) {
	RegisterTags()

	e := NewEnvelope("Hello")
	_, err := e.Position()
	if err == nil {
		t.Error("expected Position() to fail on envelope without position")
	}

	e, err = e.SetPosition(42)
	if err != nil {
		t.Fatalf("SetPosition failed: %v", err)
	}
	pos, err := e.Position()
	if err != nil {
		t.Fatalf("Position failed: %v", err)
	}
	if pos != 42 {
		t.Errorf("expected position 42, got %d", pos)
	}
	assertActualExpected(t, e.Format(),
		`"Hello" [
    'position': 42
]`)

	e, err = e.SetPosition(0)
	if err != nil {
		t.Fatalf("SetPosition(0) failed: %v", err)
	}
	pos, err = e.Position()
	if err != nil {
		t.Fatalf("Position failed: %v", err)
	}
	if pos != 0 {
		t.Errorf("expected position 0, got %d", pos)
	}
	assertActualExpected(t, e.Format(),
		`"Hello" [
    'position': 0
]`)

	e, err = e.RemovePosition()
	if err != nil {
		t.Fatalf("RemovePosition failed: %v", err)
	}
	_, err = e.Position()
	if err == nil {
		t.Error("expected Position() to fail after removal")
	}
	assertActualExpected(t, e.Format(), `"Hello"`)
}

// TestUnknownLeaf tests decoding an envelope containing an unknown tag.
// This uses UR decoding which may not be available yet. We test what we can
// via CBOR data decoding instead.
func TestUnknownLeaf(t *testing.T) {
	RegisterTags()

	// The UR ur:envelope/tpsotaaodnoyadgdjlssmkcklgoskseodnyteofwwfylkiftaydpdsjz
	// decodes to an envelope containing a tagged CBOR item with tag 555 and
	// a map {1: h'6fc4981e8da778332bf93342f3f77d3a'}.
	// Since we don't have UR decoding on Envelope yet, we use the raw CBOR data.
	// The UR's CBOR payload is the untagged envelope content; we need the
	// tagged form for EnvelopeFromCBORData.
	// For now, construct the envelope directly via CBOR.
	// This tests that unknown tags in leaves are preserved as-is.

	// Build the inner tagged value: 555({1: h'6fc4981e8da778332bf93342f3f77d3a'})
	innerMap := dcbor.NewMap()
	innerMap.Insert(dcbor.MustFromAny(1), dcbor.ToByteString(mustDecodeHex("6fc4981e8da778332bf93342f3f77d3a")))
	innerTagged := dcbor.NewCBORTagged(dcbor.NewTag(555, ""), dcbor.NewCBORMap(innerMap))
	e := NewEnvelope(innerTagged)
	expected := `555({1: h'6fc4981e8da778332bf93342f3f77d3a'})`
	assertActualExpected(t, e.Format(), expected)
}

// Force use of imported package
var _ = dcbor.NewTag
