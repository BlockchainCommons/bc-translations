package bccrypto

import "testing"

func TestArgon2IDBasic(t *testing.T) {
	pass := []byte("password")
	salt := []byte("example salt")
	out1 := Argon2ID(pass, salt, 32)
	if len(out1) != 32 {
		t.Fatalf("len = %d, want 32", len(out1))
	}
	out2 := Argon2ID(pass, salt, 32)
	if string(out1) != string(out2) {
		t.Fatalf("argon2id not deterministic")
	}
}

func TestArgon2IDDifferentSalt(t *testing.T) {
	pass := []byte("password")
	out1 := Argon2ID(pass, []byte("example salt"), 32)
	out2 := Argon2ID(pass, []byte("example salt2"), 32)
	if string(out1) == string(out2) {
		t.Fatalf("argon2id outputs unexpectedly equal")
	}
}
