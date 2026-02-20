package bccrypto

import (
	"bytes"
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

var (
	aeadPlaintext = []byte("Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.")
	aeadAAD       = mustLen("50515253c0c1c2c3c4c5c6c7", 12)
	aeadKey       = must32("808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f")
	aeadNonce     = must12("070000004041424344454647")
	aeadCipher    = mustLen("d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116", 114)
	aeadAuth      = must16("1ae10b594f09e26a7e902ecbd0600691")
)

func encrypted() ([]byte, [SymmetricAuthSize]byte) {
	return AEADChaCha20Poly1305EncryptWithAAD(aeadPlaintext, aeadKey, aeadNonce, aeadAAD)
}

func TestRFCTestVector(t *testing.T) {
	ciphertext, auth := encrypted()
	if !bytes.Equal(ciphertext, aeadCipher) {
		t.Fatalf("ciphertext = %x, want %x", ciphertext, aeadCipher)
	}
	if auth != aeadAuth {
		t.Fatalf("auth = %x, want %x", auth, aeadAuth)
	}

	decrypted, err := AEADChaCha20Poly1305DecryptWithAAD(ciphertext, aeadKey, aeadNonce, aeadAAD, auth)
	if err != nil {
		t.Fatalf("decrypt returned error: %v", err)
	}
	if !bytes.Equal(decrypted, aeadPlaintext) {
		t.Fatalf("plaintext = %x, want %x", decrypted, aeadPlaintext)
	}
}

func TestRandomKeyAndNonce(t *testing.T) {
	var key [32]byte
	copy(key[:], bcrand.RandomData(32))
	var nonce [12]byte
	copy(nonce[:], bcrand.RandomData(12))

	ciphertext, auth := AEADChaCha20Poly1305EncryptWithAAD(aeadPlaintext, key, nonce, aeadAAD)
	decrypted, err := AEADChaCha20Poly1305DecryptWithAAD(ciphertext, key, nonce, aeadAAD, auth)
	if err != nil {
		t.Fatalf("decrypt returned error: %v", err)
	}
	if !bytes.Equal(decrypted, aeadPlaintext) {
		t.Fatalf("plaintext mismatch")
	}
}

func TestEmptyData(t *testing.T) {
	var key [32]byte
	copy(key[:], bcrand.RandomData(32))
	var nonce [12]byte
	copy(nonce[:], bcrand.RandomData(12))

	ciphertext, auth := AEADChaCha20Poly1305EncryptWithAAD(nil, key, nonce, nil)
	decrypted, err := AEADChaCha20Poly1305DecryptWithAAD(ciphertext, key, nonce, nil, auth)
	if err != nil {
		t.Fatalf("decrypt returned error: %v", err)
	}
	if !bytes.Equal(decrypted, nil) {
		t.Fatalf("plaintext = %x, want empty", decrypted)
	}
}
