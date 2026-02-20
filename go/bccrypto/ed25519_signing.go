package bccrypto

import (
	"crypto/ed25519"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

const (
	ED25519PublicKeySize  = ed25519.PublicKeySize
	ED25519PrivateKeySize = ed25519.SeedSize
	ED25519SignatureSize  = ed25519.SignatureSize
)

// ED25519NewPrivateKeyUsing returns a new private key seed using rng.
func ED25519NewPrivateKeyUsing(
	rng bcrand.RandomNumberGenerator,
) [ED25519PrivateKeySize]byte {
	data := rng.RandomData(ED25519PrivateKeySize)
	var out [ED25519PrivateKeySize]byte
	copy(out[:], data)
	return out
}

// ED25519PublicKeyFromPrivateKey derives public key from private key seed.
func ED25519PublicKeyFromPrivateKey(
	privateKey [ED25519PrivateKeySize]byte,
) [ED25519PublicKeySize]byte {
	fullPrivateKey := ed25519.NewKeyFromSeed(privateKey[:])
	publicKey := fullPrivateKey.Public().(ed25519.PublicKey)

	var out [ED25519PublicKeySize]byte
	copy(out[:], publicKey)
	return out
}

// ED25519Sign signs message using private key seed.
func ED25519Sign(
	privateKey [ED25519PrivateKeySize]byte,
	message []byte,
) [ED25519SignatureSize]byte {
	fullPrivateKey := ed25519.NewKeyFromSeed(privateKey[:])
	signature := ed25519.Sign(fullPrivateKey, message)

	var out [ED25519SignatureSize]byte
	copy(out[:], signature)
	return out
}

// ED25519Verify verifies signature for message and public key.
func ED25519Verify(
	publicKey [ED25519PublicKeySize]byte,
	message []byte,
	signature [ED25519SignatureSize]byte,
) bool {
	return ed25519.Verify(publicKey[:], message, signature[:])
}
