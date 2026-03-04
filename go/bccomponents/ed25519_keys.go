package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

const (
	Ed25519PrivateKeySize = bccrypto.Ed25519PrivateKeySize
	Ed25519PublicKeySize  = bccrypto.Ed25519PublicKeySize
	Ed25519SignatureSize  = bccrypto.Ed25519SignatureSize
)

// Ed25519PrivateKey is a 32-byte Ed25519 private key seed.
type Ed25519PrivateKey struct {
	data [Ed25519PrivateKeySize]byte
}

// NewEd25519PrivateKey creates a new random Ed25519 private key.
func NewEd25519PrivateKey() Ed25519PrivateKey {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewEd25519PrivateKeyUsing(rng)
}

// NewEd25519PrivateKeyUsing creates a new random Ed25519 private key using rng.
func NewEd25519PrivateKeyUsing(rng bcrand.RandomNumberGenerator) Ed25519PrivateKey {
	return Ed25519PrivateKey{data: bccrypto.Ed25519NewPrivateKeyUsing(rng)}
}

// Ed25519PrivateKeyFromData creates an Ed25519PrivateKey from a 32-byte array.
func Ed25519PrivateKeyFromData(data [Ed25519PrivateKeySize]byte) Ed25519PrivateKey {
	return Ed25519PrivateKey{data: data}
}

// Ed25519PrivateKeyFromDataRef creates an Ed25519PrivateKey from a byte slice.
func Ed25519PrivateKeyFromDataRef(data []byte) (Ed25519PrivateKey, error) {
	if len(data) != Ed25519PrivateKeySize {
		return Ed25519PrivateKey{}, errInvalidSize("Ed25519 private key", Ed25519PrivateKeySize, len(data))
	}
	var k Ed25519PrivateKey
	copy(k.data[:], data)
	return k, nil
}

// Ed25519PrivateKeyFromHex creates an Ed25519PrivateKey from a hex string.
func Ed25519PrivateKeyFromHex(h string) (Ed25519PrivateKey, error) {
	data, err := hex.DecodeString(h)
	if err != nil {
		return Ed25519PrivateKey{}, fmt.Errorf("bccomponents: invalid Ed25519 private key hex: %w", err)
	}
	return Ed25519PrivateKeyFromDataRef(data)
}

// DeriveEd25519PrivateKey derives an Ed25519 private key from key material.
func DeriveEd25519PrivateKey(keyMaterial []byte) Ed25519PrivateKey {
	derived := bccrypto.DeriveSigningPrivateKey(keyMaterial)
	return Ed25519PrivateKey{data: derived}
}

// Data returns the 32-byte key data.
func (k Ed25519PrivateKey) Data() [Ed25519PrivateKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k Ed25519PrivateKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a hex string.
func (k Ed25519PrivateKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// PublicKey derives the corresponding Ed25519 public key.
func (k Ed25519PrivateKey) PublicKey() Ed25519PublicKey {
	pub := bccrypto.Ed25519PublicKeyFromPrivateKey(k.data)
	return Ed25519PublicKey{data: pub}
}

// Sign signs a message and returns a 64-byte Ed25519 signature.
func (k Ed25519PrivateKey) Sign(message []byte) [Ed25519SignatureSize]byte {
	return bccrypto.Ed25519Sign(k.data, message)
}

// PrivateKeyData implements PrivateKeyDataProvider.
func (k Ed25519PrivateKey) PrivateKeyData() []byte { return k.Bytes() }

// String returns a human-readable representation.
func (k Ed25519PrivateKey) String() string {
	return fmt.Sprintf("Ed25519PrivateKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k Ed25519PrivateKey) Equal(other Ed25519PrivateKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k Ed25519PrivateKey) Reference() Reference {
	return k.PublicKey().Reference()
}

// Ed25519PublicKey is a 32-byte Ed25519 public key.
type Ed25519PublicKey struct {
	data [Ed25519PublicKeySize]byte
}

// Ed25519PublicKeyFromData creates an Ed25519PublicKey from a 32-byte array.
func Ed25519PublicKeyFromData(data [Ed25519PublicKeySize]byte) Ed25519PublicKey {
	return Ed25519PublicKey{data: data}
}

// Ed25519PublicKeyFromDataRef creates an Ed25519PublicKey from a byte slice.
func Ed25519PublicKeyFromDataRef(data []byte) (Ed25519PublicKey, error) {
	if len(data) != Ed25519PublicKeySize {
		return Ed25519PublicKey{}, errInvalidSize("Ed25519 public key", Ed25519PublicKeySize, len(data))
	}
	var k Ed25519PublicKey
	copy(k.data[:], data)
	return k, nil
}

// Ed25519PublicKeyFromHex creates an Ed25519PublicKey from a hex string.
func Ed25519PublicKeyFromHex(h string) (Ed25519PublicKey, error) {
	data, err := hex.DecodeString(h)
	if err != nil {
		return Ed25519PublicKey{}, fmt.Errorf("bccomponents: invalid Ed25519 public key hex: %w", err)
	}
	return Ed25519PublicKeyFromDataRef(data)
}

// Data returns the 32-byte key data.
func (k Ed25519PublicKey) Data() [Ed25519PublicKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k Ed25519PublicKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a hex string.
func (k Ed25519PublicKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// Verify verifies an Ed25519 signature over a message.
func (k Ed25519PublicKey) Verify(signature [Ed25519SignatureSize]byte, message []byte) bool {
	return bccrypto.Ed25519Verify(k.data, message, signature)
}

// String returns a human-readable representation.
func (k Ed25519PublicKey) String() string {
	return fmt.Sprintf("Ed25519PublicKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k Ed25519PublicKey) Equal(other Ed25519PublicKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k Ed25519PublicKey) Reference() Reference {
	return ReferenceFromDigest(DigestFromImage(k.data[:]))
}
