package provenancemark

import (
	"bytes"
	"encoding/hex"
	"testing"
)

func TestSHA256(t *testing.T) {
	got := SHA256([]byte("Hello World"))
	want, _ := hex.DecodeString("a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e")
	if !bytes.Equal(got[:], want) {
		t.Fatalf("SHA256 mismatch: got=%x want=%x", got, want)
	}
}

func TestExtendKey(t *testing.T) {
	got := ExtendKey([]byte("Hello World"))
	want, _ := hex.DecodeString("813085a508d5fec645abe5a1fb9a23c2a6ac6bef0a99650017b3ef50538dba39")
	if got != [32]byte(want) {
		t.Fatalf("ExtendKey mismatch: got=%x want=%x", got, want)
	}
}

func TestObfuscate(t *testing.T) {
	obfuscated := Obfuscate([]byte("Hello"), []byte("World"))
	want, _ := hex.DecodeString("c43889aafa")
	if !bytes.Equal(obfuscated, want) {
		t.Fatalf("Obfuscate mismatch: got=%x want=%x", obfuscated, want)
	}
	deobfuscated := Obfuscate([]byte("Hello"), obfuscated)
	if !bytes.Equal(deobfuscated, []byte("World")) {
		t.Fatalf("deobfuscation mismatch: got=%x want=%x", deobfuscated, []byte("World"))
	}
}
