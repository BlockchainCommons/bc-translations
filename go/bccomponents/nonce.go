package bccomponents

import (
	"encoding/hex"
	"fmt"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const NonceSize = 12

// Nonce is a random number used once (12 bytes), typically for authenticated
// encryption schemes like ChaCha20-Poly1305.
type Nonce struct {
	data [NonceSize]byte
}

// NewNonce creates a new random nonce using the system CSPRNG.
func NewNonce() Nonce {
	var n Nonce
	rng := bcrand.NewSecureRandomNumberGenerator()
	rng.FillRandomData(n.data[:])
	return n
}

// NonceFromData creates a Nonce from a 12-byte array.
func NonceFromData(data [NonceSize]byte) Nonce {
	return Nonce{data: data}
}

// NonceFromDataRef creates a Nonce from a byte slice, validating its length.
func NonceFromDataRef(data []byte) (Nonce, error) {
	if len(data) != NonceSize {
		return Nonce{}, errInvalidSize("nonce", NonceSize, len(data))
	}
	var n Nonce
	copy(n.data[:], data)
	return n, nil
}

// NonceFromHex creates a Nonce from a hex string. Panics if invalid.
func NonceFromHex(h string) Nonce {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid nonce hex: %v", err))
	}
	n, err := NonceFromDataRef(data)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: %v", err))
	}
	return n
}

// Data returns the 12-byte nonce data.
func (n Nonce) Data() [NonceSize]byte { return n.data }

// Bytes returns the nonce as a byte slice.
func (n Nonce) Bytes() []byte { return n.data[:] }

// Hex returns the nonce as a 24-character hex string.
func (n Nonce) Hex() string { return hex.EncodeToString(n.data[:]) }

// String returns a human-readable representation.
func (n Nonce) String() string { return fmt.Sprintf("Nonce(%s)", n.Hex()) }

// Equal reports whether two nonces are equal.
func (n Nonce) Equal(other Nonce) bool { return n.data == other.data }

// --- CBOR support ---

// NonceCBORTags returns the CBOR tags used for Nonce.
func NonceCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagNonce})
}

// CBORTags implements dcbor.CBORTagged.
func (n Nonce) CBORTags() []dcbor.Tag { return NonceCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (n Nonce) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(n.data[:]) }

// TaggedCBOR returns the tagged CBOR encoding of the Nonce.
func (n Nonce) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(n)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (n Nonce) ToCBOR() dcbor.CBOR { return n.TaggedCBOR() }

// DecodeNonce decodes a Nonce from untagged CBOR.
func DecodeNonce(cbor dcbor.CBOR) (Nonce, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return Nonce{}, err
	}
	return NonceFromDataRef(data)
}

// DecodeTaggedNonce decodes a Nonce from tagged CBOR.
func DecodeTaggedNonce(cbor dcbor.CBOR) (Nonce, error) {
	return dcbor.DecodeTagged(cbor, NonceCBORTags(), DecodeNonce)
}

// NonceFromTaggedCBOR decodes a Nonce from tagged CBOR bytes.
func NonceFromTaggedCBOR(data []byte) (Nonce, error) {
	return dcbor.DecodeTaggedData(data, NonceCBORTags(), DecodeNonce)
}

// --- UR support ---

// NonceToURString encodes a Nonce as a UR string.
func NonceToURString(n Nonce) string { return bcur.ToURString(n) }

// NonceFromURString decodes a Nonce from a UR string.
func NonceFromURString(urString string) (Nonce, error) {
	return bcur.DecodeURString(urString, NonceCBORTags(), DecodeNonce)
}
