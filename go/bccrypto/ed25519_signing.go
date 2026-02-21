package bccrypto

import (
	"crypto/ed25519"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

const (
	Ed25519PublicKeySize  = ed25519.PublicKeySize
	Ed25519PrivateKeySize = ed25519.SeedSize
	Ed25519SignatureSize  = ed25519.SignatureSize
)

// Ed25519NewPrivateKeyUsing returns a new private key seed using rng.
func Ed25519NewPrivateKeyUsing(
	rng bcrand.RandomNumberGenerator,
) [Ed25519PrivateKeySize]byte {
	data := rng.RandomData(Ed25519PrivateKeySize)
	var out [Ed25519PrivateKeySize]byte
	copy(out[:], data)
	return out
}

// Ed25519PublicKeyFromPrivateKey derives public key from private key seed.
func Ed25519PublicKeyFromPrivateKey(
	privateKey [Ed25519PrivateKeySize]byte,
) [Ed25519PublicKeySize]byte {
	fullPrivateKey := ed25519.NewKeyFromSeed(privateKey[:])
	publicKey := fullPrivateKey.Public().(ed25519.PublicKey)

	var out [Ed25519PublicKeySize]byte
	copy(out[:], publicKey)
	return out
}

// Ed25519Sign signs message using private key seed.
func Ed25519Sign(
	privateKey [Ed25519PrivateKeySize]byte,
	message []byte,
) [Ed25519SignatureSize]byte {
	fullPrivateKey := ed25519.NewKeyFromSeed(privateKey[:])
	signature := ed25519.Sign(fullPrivateKey, message)

	var out [Ed25519SignatureSize]byte
	copy(out[:], signature)
	return out
}

// Ed25519Verify verifies signature for message and public key.
func Ed25519Verify(
	publicKey [Ed25519PublicKeySize]byte,
	message []byte,
	signature [Ed25519SignatureSize]byte,
) bool {
	return ed25519.Verify(publicKey[:], message, signature[:])
}
