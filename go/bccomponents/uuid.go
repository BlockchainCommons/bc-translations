package bccomponents

import (
	"encoding/hex"
	"fmt"
	"strings"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const UUIDSize = 16

// UUID is a Universally Unique Identifier (128-bit, RFC 4122 Type 4).
//
// Type 4 UUIDs are randomly generated with specific bits set for the version
// (4) and variant (2) fields.
type UUID struct {
	data [UUIDSize]byte
}

// NewUUID creates a new random type 4 UUID using the system CSPRNG.
func NewUUID() UUID {
	var u UUID
	rng := bcrand.NewSecureRandomNumberGenerator()
	rng.FillRandomData(u.data[:])
	u.data[6] = (u.data[6] & 0x0f) | 0x40 // version 4
	u.data[8] = (u.data[8] & 0x3f) | 0x80 // variant 2
	return u
}

// UUIDFromData creates a UUID from a 16-byte array.
func UUIDFromData(data [UUIDSize]byte) UUID {
	return UUID{data: data}
}

// UUIDFromDataRef creates a UUID from a byte slice, validating its length.
func UUIDFromDataRef(data []byte) (UUID, error) {
	if len(data) != UUIDSize {
		return UUID{}, errInvalidSize("UUID", UUIDSize, len(data))
	}
	var u UUID
	copy(u.data[:], data)
	return u, nil
}

// Data returns the 16-byte UUID data.
func (u UUID) Data() [UUIDSize]byte { return u.data }

// Bytes returns the UUID as a byte slice.
func (u UUID) Bytes() []byte { return u.data[:] }

// UUIDFromString parses a UUID from the canonical string format
// "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".
func UUIDFromString(s string) (UUID, error) {
	s = strings.TrimSpace(s)
	s = strings.ReplaceAll(s, "-", "")
	data, err := hex.DecodeString(s)
	if err != nil {
		return UUID{}, fmt.Errorf("bccomponents: invalid UUID hex: %v", err)
	}
	if len(data) != UUIDSize {
		return UUID{}, errInvalidSize("UUID", UUIDSize, len(data))
	}
	var u UUID
	copy(u.data[:], data)
	return u, nil
}

// String returns the UUID in canonical format with dashes:
// "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".
func (u UUID) String() string {
	h := hex.EncodeToString(u.data[:])
	return fmt.Sprintf("%s-%s-%s-%s-%s",
		h[0:8], h[8:12], h[12:16], h[16:20], h[20:32])
}

// Equal reports whether two UUIDs are equal.
func (u UUID) Equal(other UUID) bool { return u.data == other.data }

// --- CBOR support ---

// UUIDCBORTags returns the CBOR tags used for UUID.
func UUIDCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagUUID})
}

// CBORTags implements dcbor.CBORTagged.
func (u UUID) CBORTags() []dcbor.Tag { return UUIDCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (u UUID) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(u.data[:]) }

// TaggedCBOR returns the tagged CBOR encoding of the UUID.
func (u UUID) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(u)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (u UUID) ToCBOR() dcbor.CBOR { return u.TaggedCBOR() }

// DecodeUUID decodes a UUID from untagged CBOR.
func DecodeUUID(cbor dcbor.CBOR) (UUID, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return UUID{}, err
	}
	if len(data) != UUIDSize {
		return UUID{}, errInvalidSize("UUID", UUIDSize, len(data))
	}
	var u UUID
	copy(u.data[:], data)
	return u, nil
}

// DecodeTaggedUUID decodes a UUID from tagged CBOR.
func DecodeTaggedUUID(cbor dcbor.CBOR) (UUID, error) {
	return dcbor.DecodeTagged(cbor, UUIDCBORTags(), DecodeUUID)
}

// UUIDFromTaggedCBOR decodes a UUID from tagged CBOR bytes.
func UUIDFromTaggedCBOR(data []byte) (UUID, error) {
	return dcbor.DecodeTaggedData(data, UUIDCBORTags(), DecodeUUID)
}
