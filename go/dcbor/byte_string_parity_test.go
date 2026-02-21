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
