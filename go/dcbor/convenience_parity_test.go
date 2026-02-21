package dcbor

import (
	"bytes"
	"encoding/hex"
	"errors"
	"testing"
)

func TestByteStringAndTextConvenienceParity(t *testing.T) {
	byteString, err := ToByteStringFromHex("001122")
	if err != nil {
		t.Fatalf("ToByteStringFromHex failed: %v", err)
	}
	if !byteString.IsByteString() {
		t.Fatalf("expected byte string")
	}
	if got, err := byteString.TryByteString(); err != nil || !bytes.Equal(got, []byte{0x00, 0x11, 0x22}) {
		t.Fatalf("TryByteString mismatch: got=%x err=%v", got, err)
	}
	if got, ok := byteString.IntoByteString(); !ok || !bytes.Equal(got, []byte{0x00, 0x11, 0x22}) {
		t.Fatalf("IntoByteString mismatch: got=%x ok=%v", got, ok)
	}
	if got, ok := byteString.AsByteString(); !ok || !bytes.Equal(got, []byte{0x00, 0x11, 0x22}) {
		t.Fatalf("AsByteString mismatch: got=%x ok=%v", got, ok)
	}

	if _, err := ToByteStringFromHex("zz"); err == nil {
		t.Fatalf("expected hex decode failure")
	}
	if got, want := MustToByteStringFromHex("001122").DiagnosticFlat(), "h'001122'"; got != want {
		t.Fatalf("MustToByteStringFromHex mismatch: got %q want %q", got, want)
	}
	func() {
		defer func() {
			if r := recover(); r == nil {
				t.Fatalf("expected MustToByteStringFromHex to panic on invalid hex")
			}
		}()
		_ = MustToByteStringFromHex("zz")
	}()

	text := MustFromAny("hello")
	if !text.IsText() {
		t.Fatalf("expected text type")
	}
	if got, err := text.TryText(); err != nil || got != "hello" {
		t.Fatalf("TryText mismatch: got=%q err=%v", got, err)
	}
	if got, ok := text.IntoText(); !ok || got != "hello" {
		t.Fatalf("IntoText mismatch: got=%q ok=%v", got, ok)
	}
	if got, ok := text.AsText(); !ok || got != "hello" {
		t.Fatalf("AsText mismatch: got=%q ok=%v", got, ok)
	}

	if _, err := MustFromAny(42).TryIntoText(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for TryIntoText on number, got %v", err)
	}
}

func TestArrayMapAndTaggedConvenienceParity(t *testing.T) {
	array := arrayFromAny(1, 2, 3)
	if !array.IsArray() {
		t.Fatalf("expected array type")
	}
	if got, err := array.TryArray(); err != nil || len(got) != 3 {
		t.Fatalf("TryArray mismatch: len=%d err=%v", len(got), err)
	}
	if got, ok := array.IntoArray(); !ok || len(got) != 3 {
		t.Fatalf("IntoArray mismatch: len=%d ok=%v", len(got), ok)
	}
	if _, err := MustFromAny("x").TryIntoArray(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for TryIntoArray on text, got %v", err)
	}

	m := NewMap()
	m.MustInsertAny("a", 1)
	mapCBOR := NewCBORMap(m)
	if !mapCBOR.IsMap() {
		t.Fatalf("expected map type")
	}
	if got, err := mapCBOR.TryMap(); err != nil || got.Len() != 1 {
		t.Fatalf("TryMap mismatch: len=%d err=%v", got.Len(), err)
	}
	if got, ok := mapCBOR.IntoMap(); !ok || got.Len() != 1 {
		t.Fatalf("IntoMap mismatch: len=%d ok=%v", got.Len(), ok)
	}
	if _, err := MustFromAny(true).TryIntoMap(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for TryIntoMap on bool, got %v", err)
	}

	tagged := ToTaggedValue(NewTag(100, "demo"), MustFromAny("Hello"))
	if !tagged.IsTaggedValue() {
		t.Fatalf("expected tagged value")
	}
	tag, content, err := tagged.TryTaggedValue()
	if err != nil {
		t.Fatalf("TryTaggedValue failed: %v", err)
	}
	if got, want := tag.Value(), TagValue(100); got != want {
		t.Fatalf("tag value mismatch: got %d want %d", got, want)
	}
	if got, want := content.String(), `"Hello"`; got != want {
		t.Fatalf("tag content mismatch: got %q want %q", got, want)
	}
	if _, _, err := MustFromAny(1).TryIntoTaggedValue(); !errors.Is(err, ErrWrongType) {
		t.Fatalf("expected ErrWrongType for TryIntoTaggedValue on number, got %v", err)
	}
}

func TestBoolNullNaNConvenienceParity(t *testing.T) {
	if !True().IsBool() || !False().IsBool() {
		t.Fatalf("expected bool convenience flags")
	}
	if got, err := True().TryBool(); err != nil || !got {
		t.Fatalf("TryBool(true) mismatch: got=%v err=%v", got, err)
	}
	if got, err := False().TryIntoBool(); err != nil || got {
		t.Fatalf("TryIntoBool(false) mismatch: got=%v err=%v", got, err)
	}
	if !True().IsTrue() || True().IsFalse() {
		t.Fatalf("True helper mismatch")
	}
	if !False().IsFalse() || False().IsTrue() {
		t.Fatalf("False helper mismatch")
	}
	if !Null().IsNull() {
		t.Fatalf("Null helper mismatch")
	}
	if !NaN().IsNaN() {
		t.Fatalf("NaN helper mismatch")
	}
}

func TestUtilityHelperParity(t *testing.T) {
	unsorted := []CBOR{MustFromAny("z"), MustFromAny(1), MustFromAny("a")}
	sorted := SortArrayByCBOREncoding(unsorted)
	if got, want := NewCBORArray(sorted).DiagnosticFlat(), `[1, "a", "z"]`; got != want {
		t.Fatalf("SortArrayByCBOREncoding mismatch: got %q want %q", got, want)
	}

	normalized, err := NormalizeViaFxamacker(mustBytes(t, "a2026141016142"))
	if err != nil {
		t.Fatalf("NormalizeViaFxamacker failed: %v", err)
	}
	decoded, err := TryFromData(normalized)
	if err != nil {
		t.Fatalf("TryFromData after NormalizeViaFxamacker failed: %v", err)
	}
	if got, want := decoded.DiagnosticFlat(), `{1: "B", 2: "A"}`; got != want {
		t.Fatalf("normalized map diagnostic mismatch: got %q want %q", got, want)
	}

	defer func() {
		if r := recover(); r == nil {
			t.Fatalf("expected MustEqual to panic on mismatch")
		}
	}()
	MustFromAny(1).MustEqual(MustFromAny(2))
}

func mustBytes(t *testing.T, hexValue string) []byte {
	t.Helper()
	data, err := hex.DecodeString(hexValue)
	if err != nil {
		t.Fatalf("hex decode failed: %v", err)
	}
	return data
}
