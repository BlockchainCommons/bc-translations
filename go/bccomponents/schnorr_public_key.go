package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
)

const SchnorrPublicKeySize = bccrypto.SchnorrPublicKeySize

// SchnorrPublicKey is a 32-byte x-only BIP-340 Schnorr public key for secp256k1.
type SchnorrPublicKey struct {
	data [SchnorrPublicKeySize]byte
}

// SchnorrPublicKeyFromData creates a SchnorrPublicKey from a 32-byte array.
func SchnorrPublicKeyFromData(data [SchnorrPublicKeySize]byte) SchnorrPublicKey {
	return SchnorrPublicKey{data: data}
}

// SchnorrPublicKeyFromDataRef creates a SchnorrPublicKey from a byte slice.
func SchnorrPublicKeyFromDataRef(data []byte) (SchnorrPublicKey, error) {
	if len(data) != SchnorrPublicKeySize {
		return SchnorrPublicKey{}, errInvalidSize("Schnorr public key", SchnorrPublicKeySize, len(data))
	}
	var k SchnorrPublicKey
	copy(k.data[:], data)
	return k, nil
}

// Data returns the 32-byte key data.
func (k SchnorrPublicKey) Data() [SchnorrPublicKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k SchnorrPublicKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a hex string.
func (k SchnorrPublicKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// SchnorrVerify verifies a BIP-340 Schnorr signature over a message.
func (k SchnorrPublicKey) SchnorrVerify(signature [bccrypto.SchnorrSignatureSize]byte, message []byte) bool {
	return bccrypto.SchnorrVerify(k.data, signature, message)
}

// String returns a human-readable representation.
func (k SchnorrPublicKey) String() string {
	return fmt.Sprintf("SchnorrPublicKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k SchnorrPublicKey) Equal(other SchnorrPublicKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k SchnorrPublicKey) Reference() Reference {
	digest := DigestFromImage(k.data[:])
	return ReferenceFromDigest(digest)
}
