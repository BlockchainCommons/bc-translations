package bccomponents

import (
	"bytes"
	"encoding/hex"
	"fmt"
	"math"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Salt is random data used to decorrelate other information. Variable length,
// minimum 8 bytes.
type Salt struct {
	data []byte
}

// SaltFromData creates a Salt from a byte slice.
func SaltFromData(data []byte) Salt {
	cp := make([]byte, len(data))
	copy(cp, data)
	return Salt{data: cp}
}

// NewSaltWithLen creates a Salt of the specified length using the system CSPRNG.
// The length must be at least 8.
func NewSaltWithLen(count int) (Salt, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewSaltWithLenUsing(count, rng)
}

// NewSaltWithLenUsing creates a Salt of the specified length using the provided RNG.
func NewSaltWithLenUsing(count int, rng bcrand.RandomNumberGenerator) (Salt, error) {
	if count < 8 {
		return Salt{}, errDataTooShort("salt", 8, count)
	}
	return SaltFromData(bcrand.RandomDataFrom(rng, count)), nil
}

// NewSaltInRange creates a Salt with a length chosen randomly from the given range.
// The minimum must be at least 8.
func NewSaltInRange(min, max int) (Salt, error) {
	if min < 8 {
		return Salt{}, errDataTooShort("salt", 8, min)
	}
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewSaltInRangeUsing(min, max, rng)
}

// NewSaltInRangeUsing creates a Salt with a length chosen randomly from the range.
func NewSaltInRangeUsing(min, max int, rng bcrand.RandomNumberGenerator) (Salt, error) {
	if min < 8 {
		return Salt{}, errDataTooShort("salt", 8, min)
	}
	count := int(bcrand.NextInClosedRange(rng, int64(min), int64(max), 64))
	return NewSaltWithLenUsing(count, rng)
}

// NewSaltForSize creates a Salt with a length proportional to the given data size.
// The salt will be between 5% and 25% of the data size, with a minimum of 8 bytes.
func NewSaltForSize(size int) Salt {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewSaltForSizeUsing(size, rng)
}

// NewSaltForSizeUsing creates a proportional Salt using the provided RNG.
func NewSaltForSizeUsing(size int, rng bcrand.RandomNumberGenerator) Salt {
	count := float64(size)
	minSize := int(math.Max(8, math.Ceil(count*0.05)))
	maxSize := int(math.Max(float64(minSize+8), math.Ceil(count*0.25)))
	s, _ := NewSaltInRangeUsing(minSize, maxSize, rng)
	return s
}

// SaltFromHex creates a Salt from a hex string. Panics if invalid.
func SaltFromHex(h string) Salt {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid salt hex: %v", err))
	}
	return SaltFromData(data)
}

// Len returns the length of the salt in bytes.
func (s Salt) Len() int { return len(s.data) }

// IsEmpty returns true if the salt is empty.
func (s Salt) IsEmpty() bool { return len(s.data) == 0 }

// Bytes returns the salt as a byte slice.
func (s Salt) Bytes() []byte {
	cp := make([]byte, len(s.data))
	copy(cp, s.data)
	return cp
}

// Hex returns the salt as a hex string.
func (s Salt) Hex() string { return hex.EncodeToString(s.data) }

// String returns a human-readable representation.
func (s Salt) String() string { return fmt.Sprintf("Salt(%d)", s.Len()) }

// Equal reports whether two salts are equal.
func (s Salt) Equal(other Salt) bool {
	return bytes.Equal(s.data, other.data)
}

// --- CBOR support ---

// SaltCBORTags returns the CBOR tags used for Salt.
func SaltCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSalt})
}

// CBORTags implements dcbor.CBORTagged.
func (s Salt) CBORTags() []dcbor.Tag { return SaltCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (s Salt) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(s.data) }

// TaggedCBOR returns the tagged CBOR encoding of the Salt.
func (s Salt) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(s)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (s Salt) ToCBOR() dcbor.CBOR { return s.TaggedCBOR() }

// DecodeSalt decodes a Salt from untagged CBOR.
func DecodeSalt(cbor dcbor.CBOR) (Salt, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return Salt{}, err
	}
	return SaltFromData(data), nil
}

// DecodeTaggedSalt decodes a Salt from tagged CBOR.
func DecodeTaggedSalt(cbor dcbor.CBOR) (Salt, error) {
	return dcbor.DecodeTagged(cbor, SaltCBORTags(), DecodeSalt)
}

// SaltFromTaggedCBOR decodes a Salt from tagged CBOR bytes.
func SaltFromTaggedCBOR(data []byte) (Salt, error) {
	return dcbor.DecodeTaggedData(data, SaltCBORTags(), DecodeSalt)
}

// --- UR support ---

// SaltToURString encodes a Salt as a UR string.
func SaltToURString(s Salt) string { return bcur.ToURString(s) }

// SaltFromURString decodes a Salt from a UR string.
func SaltFromURString(urString string) (Salt, error) {
	return bcur.DecodeURString(urString, SaltCBORTags(), DecodeSalt)
}
