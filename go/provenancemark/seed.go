package provenancemark

import (
	"encoding/hex"
	"encoding/json"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// ProvenanceSeedLength is the fixed byte length of a provenance seed.
const ProvenanceSeedLength = 32

// ProvenanceSeed is a fixed 32-byte seed wrapper.
type ProvenanceSeed struct {
	data [ProvenanceSeedLength]byte
}

// NewProvenanceSeed creates a new seed using cryptographically strong randomness.
func NewProvenanceSeed() ProvenanceSeed {
	return NewProvenanceSeedUsing(bcrand.NewSecureRandomNumberGenerator())
}

// NewProvenanceSeedUsing creates a new seed using the provided RNG.
func NewProvenanceSeedUsing(rng bcrand.RandomNumberGenerator) ProvenanceSeed {
	data := bcrand.RandomDataFrom(rng, ProvenanceSeedLength)
	var seedData [ProvenanceSeedLength]byte
	copy(seedData[:], data)
	return ProvenanceSeedFromBytes(seedData)
}

// NewProvenanceSeedWithPassphrase deterministically derives a seed from passphrase.
func NewProvenanceSeedWithPassphrase(passphrase string) ProvenanceSeed {
	return ProvenanceSeedFromBytes(ExtendKey([]byte(passphrase)))
}

// ProvenanceSeedFromBytes wraps a fixed seed block.
func ProvenanceSeedFromBytes(bytes [ProvenanceSeedLength]byte) ProvenanceSeed {
	return ProvenanceSeed{data: bytes}
}

// ProvenanceSeedFromSlice decodes a seed from a byte slice.
func ProvenanceSeedFromSlice(bytes []byte) (ProvenanceSeed, error) {
	if len(bytes) != ProvenanceSeedLength {
		return ProvenanceSeed{}, newInvalidSeedLength(len(bytes))
	}
	var out [ProvenanceSeedLength]byte
	copy(out[:], bytes)
	return ProvenanceSeedFromBytes(out), nil
}

// ProvenanceSeedFromBase64 decodes a seed from its JSON/base64 representation.
func ProvenanceSeedFromBase64(value string) (ProvenanceSeed, error) {
	bytes, err := DeserializeBase64(value)
	if err != nil {
		return ProvenanceSeed{}, err
	}
	return ProvenanceSeedFromSlice(bytes)
}

// Bytes returns a copy of the underlying seed bytes.
func (s ProvenanceSeed) Bytes() [ProvenanceSeedLength]byte {
	return s.data
}

// Hex returns the seed bytes as lower-case hex.
func (s ProvenanceSeed) Hex() string {
	return hex.EncodeToString(s.data[:])
}

// ToCBOR encodes the seed as an untagged CBOR byte string.
func (s ProvenanceSeed) ToCBOR() dcbor.CBOR {
	return dcbor.ToByteString(s.data[:])
}

// DecodeProvenanceSeed decodes a seed from CBOR.
func DecodeProvenanceSeed(cbor dcbor.CBOR) (ProvenanceSeed, error) {
	bytes, err := cbor.TryIntoByteString()
	if err != nil {
		return ProvenanceSeed{}, wrapCBORError(err)
	}
	return ProvenanceSeedFromSlice(bytes)
}

// MarshalJSON encodes the seed as a base64 string.
func (s ProvenanceSeed) MarshalJSON() ([]byte, error) {
	return marshalString(SerializeBlock(s.data))
}

// UnmarshalJSON decodes the seed from a base64 string.
func (s *ProvenanceSeed) UnmarshalJSON(data []byte) error {
	value, err := unmarshalString(data)
	if err != nil {
		return err
	}
	decoded, err := ProvenanceSeedFromBase64(value)
	if err != nil {
		return err
	}
	*s = decoded
	return nil
}

// MarshalText encodes the seed as base64 text.
func (s ProvenanceSeed) MarshalText() ([]byte, error) {
	return []byte(SerializeBlock(s.data)), nil
}

// UnmarshalText decodes the seed from base64 text.
func (s *ProvenanceSeed) UnmarshalText(text []byte) error {
	decoded, err := ProvenanceSeedFromBase64(string(text))
	if err != nil {
		return err
	}
	*s = decoded
	return nil
}

var _ json.Marshaler = ProvenanceSeed{}
var _ json.Unmarshaler = (*ProvenanceSeed)(nil)
