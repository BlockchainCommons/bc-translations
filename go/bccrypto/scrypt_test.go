package bccrypto

import "testing"

func TestScryptBasic(t *testing.T) {
	pass := []byte("password")
	salt := []byte("salt")
	out1 := Scrypt(pass, salt, 32)
	if len(out1) != 32 {
		t.Fatalf("len = %d, want 32", len(out1))
	}
	out2 := Scrypt(pass, salt, 32)
	if string(out1) != string(out2) {
		t.Fatalf("scrypt not deterministic")
	}
}

func TestScryptDifferentSalt(t *testing.T) {
	pass := []byte("password")
	out1 := Scrypt(pass, []byte("salt1"), 32)
	out2 := Scrypt(pass, []byte("salt2"), 32)
	if string(out1) == string(out2) {
		t.Fatalf("scrypt outputs unexpectedly equal")
	}
}

func TestScryptWithParamsBasic(t *testing.T) {
	out := ScryptWithParams([]byte("password"), []byte("salt"), 32, 15, 8, 1)
	if len(out) != 32 {
		t.Fatalf("len = %d, want 32", len(out))
	}
}

func TestScryptOutputLength(t *testing.T) {
	for _, length := range []int{16, 24, 32, 64} {
		out := Scrypt([]byte("password"), []byte("salt"), length)
		if len(out) != length {
			t.Fatalf("len = %d, want %d", len(out), length)
		}
	}
}
