package knownvalues

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func TestNewKnownValueWithAndWithoutName(t *testing.T) {
	unnamed := NewKnownValue(42)
	if got, want := unnamed.Value(), uint64(42); got != want {
		t.Fatalf("Value mismatch: got %d want %d", got, want)
	}
	if _, ok := unnamed.AssignedName(); ok {
		t.Fatalf("AssignedName should be absent for unnamed value")
	}
	if got, want := unnamed.Name(), "42"; got != want {
		t.Fatalf("Name mismatch: got %q want %q", got, want)
	}

	named := NewKnownValueWithName(1, "isA")
	if got, want := named.Value(), uint64(1); got != want {
		t.Fatalf("Value mismatch: got %d want %d", got, want)
	}
	if got, ok := named.AssignedName(); !ok || got != "isA" {
		t.Fatalf("AssignedName mismatch: got %q ok=%t", got, ok)
	}
	if got, want := named.Name(), "isA"; got != want {
		t.Fatalf("Name mismatch: got %q want %q", got, want)
	}
}

func TestEqualityAndCopyIgnoreAssignedName(t *testing.T) {
	first := NewKnownValueWithName(1, "isA")
	second := NewKnownValueWithName(1, "different")
	if !first.Equal(second) {
		t.Fatalf("expected equal raw values despite different assigned names")
	}

	// Value types copy on assignment in Go.
	copied := first
	if !first.Equal(copied) {
		t.Fatalf("copy mismatch")
	}
	if got, want := copied.Name(), "isA"; got != want {
		t.Fatalf("copy name mismatch: got %q want %q", got, want)
	}
}

func TestStringAndNumericConversionHelpers(t *testing.T) {
	if got, want := KnownValueFromInt(99).String(), "99"; got != want {
		t.Fatalf("KnownValueFromInt string mismatch: got %q want %q", got, want)
	}
	if got, want := KnownValueFromInt32(100).Value(), uint64(100); got != want {
		t.Fatalf("KnownValueFromInt32 value mismatch: got %d want %d", got, want)
	}
	if got, want := NewKnownValueWithName(100, "customValue").String(), "customValue"; got != want {
		t.Fatalf("named String mismatch: got %q want %q", got, want)
	}
}

func TestTaggedCBORRoundTripMatchesExpectedEncoding(t *testing.T) {
	cbor := IsA.TaggedCBOR()
	if got, want := cbor.Hex(), "d99c4001"; got != want {
		t.Fatalf("TaggedCBOR hex mismatch: got %q want %q", got, want)
	}

	decoded, err := DecodeTaggedKnownValue(cbor)
	if err != nil {
		t.Fatalf("DecodeTaggedKnownValue failed: %v", err)
	}
	if !decoded.Equal(IsA) {
		t.Fatalf("DecodeTaggedKnownValue mismatch: got %v want %v", decoded, IsA)
	}

	untaggedDecoded, err := DecodeKnownValue(dcbor.NewCBORUnsigned(1))
	if err != nil {
		t.Fatalf("DecodeKnownValue failed: %v", err)
	}
	if !untaggedDecoded.Equal(IsA) {
		t.Fatalf("DecodeKnownValue mismatch: got %v want %v", untaggedDecoded, IsA)
	}
}

func TestDigestMatchesTaggedCBORDigest(t *testing.T) {
	digest := IsA.Digest()
	if got, want := digest.Hex(), "2be2d79b306a21ff8e3e6bd3d1c2c6c74ff4a693b1e7ba3a0f40cdfb9ea493f8"; got != want {
		t.Fatalf("Digest hex mismatch: got %q want %q", got, want)
	}

	expected := bccomponents.DigestFromImage(IsA.TaggedCBOR().ToCBORData())
	if !digest.Equal(expected) {
		t.Fatalf("Digest mismatch against tagged CBOR image")
	}
}
