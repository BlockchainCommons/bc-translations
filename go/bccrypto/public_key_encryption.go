package bccrypto

import (
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	"golang.org/x/crypto/curve25519"
)

const (
	// GenericPrivateKeySize is the byte length of a generic private key.
	GenericPrivateKeySize = 32
	// GenericPublicKeySize is the byte length of a generic public key.
	GenericPublicKeySize = 32
	// X25519PrivateKeySize is the byte length of an X25519 private key.
	X25519PrivateKeySize = 32
	// X25519PublicKeySize is the byte length of an X25519 public key.
	X25519PublicKeySize = 32
)

// DeriveAgreementPrivateKey derives a 32-byte agreement private key from key material.
func DeriveAgreementPrivateKey(keyMaterial []byte) [GenericPrivateKeySize]byte {
	derived := HKDFHMACSHA256(keyMaterial, []byte("agreement"), GenericPrivateKeySize)
	var out [GenericPrivateKeySize]byte
	copy(out[:], derived)
	return out
}

// DeriveSigningPrivateKey derives a 32-byte signing private key from key material.
func DeriveSigningPrivateKey(keyMaterial []byte) [GenericPrivateKeySize]byte {
	derived := HKDFHMACSHA256(keyMaterial, []byte("signing"), GenericPrivateKeySize)
	var out [GenericPrivateKeySize]byte
	copy(out[:], derived)
	return out
}

// X25519NewPrivateKeyUsing creates a new X25519 private key using rng.
func X25519NewPrivateKeyUsing(rng bcrand.RandomNumberGenerator) [X25519PrivateKeySize]byte {
	data := rng.RandomData(X25519PrivateKeySize)
	var out [X25519PrivateKeySize]byte
	copy(out[:], data)
	return out
}

// X25519PublicKeyFromPrivateKey derives an X25519 public key from a private key.
func X25519PublicKeyFromPrivateKey(
	privateKey [X25519PrivateKeySize]byte,
) [X25519PublicKeySize]byte {
	publicKey, err := curve25519.X25519(privateKey[:], curve25519.Basepoint)
	if err != nil {
		panic("bccrypto: invalid X25519 private key")
	}
	var out [X25519PublicKeySize]byte
	copy(out[:], publicKey)
	return out
}

// X25519SharedKey computes the shared symmetric key from private/public key pair.
func X25519SharedKey(
	privateKey [X25519PrivateKeySize]byte,
	publicKey [X25519PublicKeySize]byte,
) [SymmetricKeySize]byte {
	sharedSecret, err := curve25519.X25519(privateKey[:], publicKey[:])
	if err != nil {
		panic("bccrypto: invalid X25519 key material")
	}
	derived := HKDFHMACSHA256(sharedSecret, []byte("agreement"), SymmetricKeySize)
	var out [SymmetricKeySize]byte
	copy(out[:], derived)
	return out
}
