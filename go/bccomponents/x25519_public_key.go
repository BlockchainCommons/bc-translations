package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const X25519PublicKeySize = bccrypto.X25519PublicKeySize

// X25519PublicKey is a 32-byte Curve25519 public key for key agreement (ECDH).
type X25519PublicKey struct {
	data [X25519PublicKeySize]byte
}

// X25519PublicKeyFromData creates an X25519PublicKey from a 32-byte array.
func X25519PublicKeyFromData(data [X25519PublicKeySize]byte) X25519PublicKey {
	return X25519PublicKey{data: data}
}

// X25519PublicKeyFromDataRef creates an X25519PublicKey from a byte slice.
func X25519PublicKeyFromDataRef(data []byte) (X25519PublicKey, error) {
	if len(data) != X25519PublicKeySize {
		return X25519PublicKey{}, errInvalidSize("X25519 public key", X25519PublicKeySize, len(data))
	}
	var k X25519PublicKey
	copy(k.data[:], data)
	return k, nil
}

// X25519PublicKeyFromHex creates an X25519PublicKey from a hex string.
func X25519PublicKeyFromHex(h string) (X25519PublicKey, error) {
	data, err := hex.DecodeString(h)
	if err != nil {
		return X25519PublicKey{}, fmt.Errorf("bccomponents: invalid X25519 public key hex: %w", err)
	}
	return X25519PublicKeyFromDataRef(data)
}

// Data returns the 32-byte key data.
func (k X25519PublicKey) Data() [X25519PublicKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k X25519PublicKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a 64-character hex string.
func (k X25519PublicKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// String returns a human-readable representation.
func (k X25519PublicKey) String() string {
	return fmt.Sprintf("X25519PublicKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k X25519PublicKey) Equal(other X25519PublicKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k X25519PublicKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func X25519PublicKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagX25519PublicKey})
}

func (k X25519PublicKey) CBORTags() []dcbor.Tag   { return X25519PublicKeyCBORTags() }
func (k X25519PublicKey) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(k.data[:]) }

func (k X25519PublicKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k X25519PublicKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeX25519PublicKey(cbor dcbor.CBOR) (X25519PublicKey, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return X25519PublicKey{}, err
	}
	return X25519PublicKeyFromDataRef(data)
}

func DecodeTaggedX25519PublicKey(cbor dcbor.CBOR) (X25519PublicKey, error) {
	return dcbor.DecodeTagged(cbor, X25519PublicKeyCBORTags(), DecodeX25519PublicKey)
}

// --- UR support ---

func X25519PublicKeyToURString(k X25519PublicKey) string { return bcur.ToURString(k) }

func X25519PublicKeyFromURString(urString string) (X25519PublicKey, error) {
	return bcur.DecodeURString(urString, X25519PublicKeyCBORTags(), DecodeX25519PublicKey)
}
