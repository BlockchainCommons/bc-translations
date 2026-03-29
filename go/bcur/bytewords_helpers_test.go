package bcur

import "testing"

func TestEncodeToWordsParity(t *testing.T) {
	data := []byte{0x00, 0x01, 0x02, 0x03}
	if got, want := EncodeToWords(data), "able acid also apex"; got != want {
		t.Fatalf("EncodeToWords mismatch: got %q want %q", got, want)
	}
}

func TestEncodeToBytemojisParity(t *testing.T) {
	data := []byte{0x00, 0x01, 0x02, 0x03}
	if got, want := EncodeToBytemojis(data), "😀 😂 😆 😉"; got != want {
		t.Fatalf("EncodeToBytemojis mismatch: got %q want %q", got, want)
	}
}

func TestEncodeToMinimalBytewordsParity(t *testing.T) {
	data := []byte{0x00, 0x01, 0x02, 0x03}
	if got, want := EncodeToMinimalBytewords(data), "aeadaoax"; got != want {
		t.Fatalf("EncodeToMinimalBytewords mismatch: got %q want %q", got, want)
	}
}
