package dcbor

import (
	"errors"
	"testing"
)

type parityEncodable struct {
	value CBOR
}

func (p parityEncodable) ToCBOR() CBOR {
	return p.value.Clone()
}

type parityTaggedValue struct {
	value int64
}

func (parityTaggedValue) CBORTags() []Tag {
	return []Tag{TagWithValue(700), TagWithValue(701)}
}

func (p parityTaggedValue) UntaggedCBOR() CBOR {
	return MustFromAny(p.value)
}

type parityTaggedNoTags struct{}

func (parityTaggedNoTags) CBORTags() []Tag {
	return nil
}

func (parityTaggedNoTags) UntaggedCBOR() CBOR {
	return MustFromAny(1)
}

func decodeParityTagged(c CBOR) (parityTaggedValue, error) {
	value, err := DecodeInt64(c)
	if err != nil {
		return parityTaggedValue{}, err
	}
	return parityTaggedValue{value: value}, nil
}

func TestTraitHelperToCBORDataParity(t *testing.T) {
	encodable := parityEncodable{value: MustFromAny("hello")}
	got := ToCBORData(encodable)
	want := encodable.ToCBOR().ToCBORData()
	if string(got) != string(want) {
		t.Fatalf("ToCBORData mismatch: got=%x want=%x", got, want)
	}
}

func TestTraitHelperTaggedEncodingParity(t *testing.T) {
	value := parityTaggedValue{value: 42}
	tagged, err := TaggedCBOR(value)
	if err != nil {
		t.Fatalf("TaggedCBOR failed: %v", err)
	}
	if got, want := tagged.DiagnosticFlat(), "700(42)"; got != want {
		t.Fatalf("TaggedCBOR diagnostic mismatch: got %q want %q", got, want)
	}

	taggedData, err := TaggedCBORData(value)
	if err != nil {
		t.Fatalf("TaggedCBORData failed: %v", err)
	}
	if got, want := taggedData, tagged.ToCBORData(); string(got) != string(want) {
		t.Fatalf("TaggedCBORData mismatch: got=%x want=%x", got, want)
	}

	if _, err := TaggedCBOR(parityTaggedNoTags{}); err == nil {
		t.Fatalf("expected TaggedCBOR error when no tags are available")
	}
	if _, err := TaggedCBORData(parityTaggedNoTags{}); err == nil {
		t.Fatalf("expected TaggedCBORData error when no tags are available")
	}
}

func TestTraitHelperTaggedDecodingParity(t *testing.T) {
	primaryTagged := ToTaggedValue(TagWithValue(700), MustFromAny(99))
	primaryDecoded, err := DecodeTagged(primaryTagged, parityTaggedValue{}.CBORTags(), decodeParityTagged)
	if err != nil {
		t.Fatalf("DecodeTagged primary failed: %v", err)
	}
	if primaryDecoded.value != 99 {
		t.Fatalf("DecodeTagged primary value mismatch: got %d want %d", primaryDecoded.value, 99)
	}

	legacyTagged := ToTaggedValue(TagWithValue(701), MustFromAny(100))
	legacyDecoded, err := DecodeTagged(legacyTagged, parityTaggedValue{}.CBORTags(), decodeParityTagged)
	if err != nil {
		t.Fatalf("DecodeTagged legacy failed: %v", err)
	}
	if legacyDecoded.value != 100 {
		t.Fatalf("DecodeTagged legacy value mismatch: got %d want %d", legacyDecoded.value, 100)
	}

	wrongTagged := ToTaggedValue(TagWithValue(999), MustFromAny(101))
	_, err = DecodeTagged(wrongTagged, parityTaggedValue{}.CBORTags(), decodeParityTagged)
	var wrongTagErr WrongTagError
	if !errors.As(err, &wrongTagErr) {
		t.Fatalf("expected WrongTagError, got %v", err)
	}
	if got, want := wrongTagErr.Expected.Value(), TagValue(700); got != want {
		t.Fatalf("WrongTagError expected mismatch: got %d want %d", got, want)
	}

	if _, err := DecodeTagged(primaryTagged, nil, decodeParityTagged); err == nil {
		t.Fatalf("expected DecodeTagged error when accepted tags are empty")
	}
}

func TestTraitHelperTaggedAndUntaggedDataDecodingParity(t *testing.T) {
	value := parityTaggedValue{value: -7}
	taggedData, err := TaggedCBORData(value)
	if err != nil {
		t.Fatalf("TaggedCBORData failed: %v", err)
	}

	decodedFromData, err := DecodeTaggedData(taggedData, parityTaggedValue{}.CBORTags(), decodeParityTagged)
	if err != nil {
		t.Fatalf("DecodeTaggedData failed: %v", err)
	}
	if decodedFromData.value != -7 {
		t.Fatalf("DecodeTaggedData value mismatch: got %d want %d", decodedFromData.value, -7)
	}

	textData := MustFromAny("dcbor").ToCBORData()
	decodedText, err := DecodeUntaggedData(textData, DecodeText)
	if err != nil {
		t.Fatalf("DecodeUntaggedData failed: %v", err)
	}
	if decodedText != "dcbor" {
		t.Fatalf("DecodeUntaggedData text mismatch: got %q want %q", decodedText, "dcbor")
	}
}
