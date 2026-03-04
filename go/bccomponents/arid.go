package bccomponents

import (
	"encoding/hex"
	"fmt"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const ARIDSize = 32

// ARID is an "Apparently Random Identifier" -- a cryptographically strong,
// universally unique 256-bit identifier. Unlike digests/hashes which identify
// a fixed immutable state, ARIDs can serve as stable identifiers for mutable
// data structures.
//
// As defined in BCR-2022-002.
type ARID struct {
	data [ARIDSize]byte
}

// NewARID creates a new random ARID using the system CSPRNG.
func NewARID() ARID {
	var a ARID
	rng := bcrand.NewSecureRandomNumberGenerator()
	rng.FillRandomData(a.data[:])
	return a
}

// ARIDFromData creates an ARID from a 32-byte array.
func ARIDFromData(data [ARIDSize]byte) ARID {
	return ARID{data: data}
}

// ARIDFromDataRef creates an ARID from a byte slice, validating its length.
func ARIDFromDataRef(data []byte) (ARID, error) {
	if len(data) != ARIDSize {
		return ARID{}, errInvalidSize("ARID", ARIDSize, len(data))
	}
	var a ARID
	copy(a.data[:], data)
	return a, nil
}

// ARIDFromHex creates an ARID from a hex string. Panics if the input is
// not exactly 64 hexadecimal digits.
func ARIDFromHex(h string) ARID {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid ARID hex: %v", err))
	}
	a, err := ARIDFromDataRef(data)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: %v", err))
	}
	return a
}

// Data returns the 32-byte ARID data.
func (a ARID) Data() [ARIDSize]byte { return a.data }

// Bytes returns the ARID as a byte slice.
func (a ARID) Bytes() []byte { return a.data[:] }

// Hex returns the ARID as a 64-character hex string.
func (a ARID) Hex() string { return hex.EncodeToString(a.data[:]) }

// ShortDescription returns the first 4 bytes as an 8-character hex string.
func (a ARID) ShortDescription() string { return hex.EncodeToString(a.data[:4]) }

// String returns a human-readable representation.
func (a ARID) String() string { return fmt.Sprintf("ARID(%s)", a.Hex()) }

// Equal reports whether two ARIDs are equal.
func (a ARID) Equal(other ARID) bool { return a.data == other.data }

// --- CBOR support ---

// ARIDCBORTags returns the CBOR tags used for ARID.
func ARIDCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagARID})
}

// CBORTags implements dcbor.CBORTagged.
func (a ARID) CBORTags() []dcbor.Tag { return ARIDCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (a ARID) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(a.data[:]) }

// TaggedCBOR returns the tagged CBOR encoding of the ARID.
func (a ARID) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(a)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (a ARID) ToCBOR() dcbor.CBOR { return a.TaggedCBOR() }

// DecodeARID decodes an ARID from untagged CBOR.
func DecodeARID(cbor dcbor.CBOR) (ARID, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return ARID{}, err
	}
	return ARIDFromDataRef(data)
}

// DecodeTaggedARID decodes an ARID from tagged CBOR.
func DecodeTaggedARID(cbor dcbor.CBOR) (ARID, error) {
	return dcbor.DecodeTagged(cbor, ARIDCBORTags(), DecodeARID)
}

// ARIDFromTaggedCBOR decodes an ARID from tagged CBOR bytes.
func ARIDFromTaggedCBOR(data []byte) (ARID, error) {
	return dcbor.DecodeTaggedData(data, ARIDCBORTags(), DecodeARID)
}
