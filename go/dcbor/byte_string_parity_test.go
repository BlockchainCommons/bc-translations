package dcbor

import (
	"errors"
	"testing"
)

var errByteStringLengthMismatch = errors.New("byte string length mismatch")

func byteStringToArray4(b ByteString) ([4]byte, error) {
	data := b.Data()
	if len(data) != 4 {
		return [4]byte{}, errByteStringLengthMismatch
	}
	var out [4]byte
	copy(out[:], data)
	return out, nil
}

func TestByteStringFixedArrayParity(t *testing.T) {
	bytes := NewByteString([]byte{1, 2, 3, 4})
	array, err := byteStringToArray4(bytes)
	if err != nil {
		t.Fatalf("byteStringToArray4 failed: %v", err)
	}
	if array != [4]byte{1, 2, 3, 4} {
		t.Fatalf("array mismatch: got %v", array)
	}

	short := NewByteString([]byte{1, 2, 3})
	if _, err := byteStringToArray4(short); !errors.Is(err, errByteStringLengthMismatch) {
		t.Fatalf("expected errByteStringLengthMismatch, got %v", err)
	}
}

func TestByteStringMethodParity(t *testing.T) {
	b := NewByteString([]byte{1, 2})
	if got, want := b.Len(), 2; got != want {
		t.Fatalf("Len mismatch: got %d want %d", got, want)
	}
	if b.IsEmpty() {
		t.Fatalf("IsEmpty mismatch: expected false")
	}

	data := b.Data()
	data[0] = 99
	if got := b.Data()[0]; got != 1 {
		t.Fatalf("Data should return a copy, got %d", got)
	}

	b.Extend([]byte{3, 4})
	if got, want := b.Len(), 4; got != want {
		t.Fatalf("Len after Extend mismatch: got %d want %d", got, want)
	}

	byt := b.Data()
	if len(byt) != 4 || byt[0] != 1 || byt[3] != 4 {
		t.Fatalf("Bytes mismatch: got %v", byt)
	}

	byt2 := b.Data()
	byt2[0] = 77
	if got := b.Data()[0]; got != 1 {
		t.Fatalf("Bytes should return a copy, got %d", got)
	}

	empty := NewByteString(nil)
	if !empty.IsEmpty() || empty.Len() != 0 {
		t.Fatalf("empty ByteString mismatch: len=%d empty=%v", empty.Len(), empty.IsEmpty())
	}

	if !b.Equal(NewByteString([]byte{1, 2, 3, 4})) {
		t.Fatalf("ByteString.Equal expected equality for identical content")
	}
	if b.Equal(NewByteString([]byte{1, 2, 3, 5})) {
		t.Fatalf("ByteString.Equal expected inequality for differing content")
	}
	if got, want := b.String(), "h'01020304'"; got != want {
		t.Fatalf("ByteString.String mismatch: got %q want %q", got, want)
	}
}
