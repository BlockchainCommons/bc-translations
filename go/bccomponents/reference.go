package bccomponents

import (
	"encoding/hex"
	"fmt"
	"strings"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const ReferenceSize = 32

// Reference is a globally unique 32-byte identifier derived from content.
type Reference struct {
	data [ReferenceSize]byte
}

// ReferenceFromData creates a Reference from a 32-byte array.
func ReferenceFromData(data [ReferenceSize]byte) Reference {
	return Reference{data: data}
}

// ReferenceFromDataRef creates a Reference from a byte slice, validating length.
func ReferenceFromDataRef(data []byte) (Reference, error) {
	if len(data) != ReferenceSize {
		return Reference{}, errInvalidSize("reference", ReferenceSize, len(data))
	}
	var r Reference
	copy(r.data[:], data)
	return r, nil
}

// ReferenceFromDigest creates a Reference from a Digest.
func ReferenceFromDigest(d Digest) Reference {
	return ReferenceFromData(d.Data())
}

// ReferenceFromHex creates a Reference from a hex string. Panics if invalid.
func ReferenceFromHex(h string) Reference {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid reference hex: %v", err))
	}
	r, err := ReferenceFromDataRef(data)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: %v", err))
	}
	return r
}

// Data returns the 32-byte reference data.
func (r Reference) Data() [ReferenceSize]byte { return r.data }

// Bytes returns the reference as a byte slice.
func (r Reference) Bytes() []byte { return r.data[:] }

// RefHex returns the full 64-character hex string.
func (r Reference) RefHex() string { return hex.EncodeToString(r.data[:]) }

// RefDataShort returns the first 4 bytes.
func (r Reference) RefDataShort() [4]byte {
	var short [4]byte
	copy(short[:], r.data[:4])
	return short
}

// RefHexShort returns the first 4 bytes as an 8-character hex string.
func (r Reference) RefHexShort() string { return hex.EncodeToString(r.data[:4]) }

// BytewordsIdentifier returns the first 4 bytes as upper-case ByteWords.
func (r Reference) BytewordsIdentifier(prefix string) string {
	s := strings.ToUpper(bcur.BytewordsIdentifier(r.RefDataShort()))
	if prefix != "" {
		return prefix + " " + s
	}
	return s
}

// BytemojiIdentifier returns the first 4 bytes as upper-case Bytemoji.
func (r Reference) BytemojiIdentifier(prefix string) string {
	s := strings.ToUpper(bcur.BytemojiIdentifier(r.RefDataShort()))
	if prefix != "" {
		return prefix + " " + s
	}
	return s
}

// String returns a short human-readable representation.
func (r Reference) String() string { return fmt.Sprintf("Reference(%s)", r.RefHexShort()) }

// Equal reports whether two references are equal.
func (r Reference) Equal(other Reference) bool { return r.data == other.data }

// Digest implements DigestProvider by SHA-256 hashing the tagged CBOR.
func (r Reference) Digest() Digest {
	return DigestFromImage(r.TaggedCBOR().ToCBORData())
}

// Reference implements ReferenceProvider by returning a reference to itself.
func (r Reference) Reference() Reference {
	return ReferenceFromDigest(r.Digest())
}

// --- CBOR support ---

// ReferenceCBORTags returns the CBOR tags used for Reference.
func ReferenceCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagReference})
}

// CBORTags implements dcbor.CBORTagged.
func (r Reference) CBORTags() []dcbor.Tag { return ReferenceCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (r Reference) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(r.data[:]) }

// TaggedCBOR returns the tagged CBOR encoding of the Reference.
func (r Reference) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(r)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (r Reference) ToCBOR() dcbor.CBOR { return r.TaggedCBOR() }

// DecodeReference decodes a Reference from untagged CBOR.
func DecodeReference(cbor dcbor.CBOR) (Reference, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return Reference{}, err
	}
	return ReferenceFromDataRef(data)
}

// DecodeTaggedReference decodes a Reference from tagged CBOR.
func DecodeTaggedReference(cbor dcbor.CBOR) (Reference, error) {
	return dcbor.DecodeTagged(cbor, ReferenceCBORTags(), DecodeReference)
}

// --- UR support ---

// ReferenceToURString encodes a Reference as a UR string.
func ReferenceToURString(r Reference) string { return bcur.ToURString(r) }

// ReferenceFromURString decodes a Reference from a UR string.
func ReferenceFromURString(urString string) (Reference, error) {
	return bcur.DecodeURString(urString, ReferenceCBORTags(), DecodeReference)
}

// ReferenceForCBORTaggedEncodable computes a Reference for any CBORTaggedEncodable
// by SHA-256 hashing its tagged CBOR encoding.
func ReferenceForCBORTaggedEncodable(v dcbor.CBORTaggedEncodable) Reference {
	cbor, _ := dcbor.TaggedCBOR(v)
	return ReferenceFromDigest(DigestFromImage(cbor.ToCBORData()))
}
