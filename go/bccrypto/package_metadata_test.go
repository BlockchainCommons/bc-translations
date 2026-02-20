package bccrypto

import (
	"bytes"
	"os"
	"testing"
)

func TestModuleMetadata(t *testing.T) {
	data, err := os.ReadFile("go.mod")
	if err != nil {
		t.Fatalf("read go.mod: %v", err)
	}
	if !bytes.Contains(data, []byte("module github.com/nickel-blockchaincommons/bccrypto-go")) {
		t.Fatalf("go.mod module path mismatch")
	}
}

func TestExpectedExportsPresent(t *testing.T) {
	_ = SHA256
	_ = AEADChaCha20Poly1305Encrypt
	_ = ECDSASign
}
