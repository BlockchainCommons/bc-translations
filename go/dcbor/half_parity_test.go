package dcbor

import "testing"

func TestFloat16TypeParity(t *testing.T) {
	h := Float16FromBits(0x3d00) // 1.25
	if got, want := h.Bits(), uint16(0x3d00); got != want {
		t.Fatalf("Bits mismatch: got 0x%04x want 0x%04x", got, want)
	}
	if got, want := h.Float64(), 1.25; got != want {
		t.Fatalf("Float64 mismatch: got %v want %v", got, want)
	}

	nan := Float16FromBits(0x7e00)
	if !nan.IsNaN() {
		t.Fatalf("expected canonical half NaN")
	}

	posInf := Float16FromBits(0x7c00)
	if !posInf.IsInf(1) {
		t.Fatalf("expected positive infinity")
	}
	negInf := Float16FromBits(0xfc00)
	if !negInf.IsInf(-1) {
		t.Fatalf("expected negative infinity")
	}
}
