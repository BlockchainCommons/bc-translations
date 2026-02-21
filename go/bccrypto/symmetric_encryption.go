package bccrypto

import "golang.org/x/crypto/chacha20poly1305"

const (
	// SymmetricKeySize is the byte length of a ChaCha20-Poly1305 key.
	SymmetricKeySize = 32
	// SymmetricNonceSize is the byte length of a ChaCha20-Poly1305 nonce.
	SymmetricNonceSize = 12
	// SymmetricAuthSize is the byte length of a Poly1305 authentication tag.
	SymmetricAuthSize = 16
)

// AEADChaCha20Poly1305EncryptWithAAD encrypts plaintext and returns
// (ciphertext, auth tag).
func AEADChaCha20Poly1305EncryptWithAAD(
	plaintext []byte,
	key [SymmetricKeySize]byte,
	nonce [SymmetricNonceSize]byte,
	aad []byte,
) ([]byte, [SymmetricAuthSize]byte) {
	aead, err := chacha20poly1305.New(key[:])
	if err != nil {
		panic(err)
	}

	sealed := aead.Seal(nil, nonce[:], plaintext, aad)
	ciphertextLen := len(sealed) - SymmetricAuthSize
	ciphertext := make([]byte, ciphertextLen)
	copy(ciphertext, sealed[:ciphertextLen])

	var auth [SymmetricAuthSize]byte
	copy(auth[:], sealed[ciphertextLen:])
	return ciphertext, auth
}

// AEADChaCha20Poly1305Encrypt encrypts without additional authenticated data.
func AEADChaCha20Poly1305Encrypt(
	plaintext []byte,
	key [SymmetricKeySize]byte,
	nonce [SymmetricNonceSize]byte,
) ([]byte, [SymmetricAuthSize]byte) {
	return AEADChaCha20Poly1305EncryptWithAAD(plaintext, key, nonce, nil)
}

// AEADChaCha20Poly1305DecryptWithAAD decrypts ciphertext with auth tag.
func AEADChaCha20Poly1305DecryptWithAAD(
	ciphertext []byte,
	key [SymmetricKeySize]byte,
	nonce [SymmetricNonceSize]byte,
	aad []byte,
	auth [SymmetricAuthSize]byte,
) ([]byte, error) {
	aead, err := chacha20poly1305.New(key[:])
	if err != nil {
		return nil, ErrAEAD
	}

	sealed := make([]byte, len(ciphertext)+SymmetricAuthSize)
	copy(sealed, ciphertext)
	copy(sealed[len(ciphertext):], auth[:])

	plaintext, err := aead.Open(nil, nonce[:], sealed, aad)
	if err != nil {
		return nil, ErrAEAD
	}
	return plaintext, nil
}

// AEADChaCha20Poly1305Decrypt decrypts without additional authenticated data.
func AEADChaCha20Poly1305Decrypt(
	ciphertext []byte,
	key [SymmetricKeySize]byte,
	nonce [SymmetricNonceSize]byte,
	auth [SymmetricAuthSize]byte,
) ([]byte, error) {
	return AEADChaCha20Poly1305DecryptWithAAD(ciphertext, key, nonce, nil, auth)
}
