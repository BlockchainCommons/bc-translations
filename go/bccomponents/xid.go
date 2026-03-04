package bccomponents

import (
	"encoding/hex"
	"fmt"
	"strings"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const XIDSize = 32

// XID is an eXtensible IDentifier -- a unique 32-byte identifier
// cryptographically tied to a public signing key at inception.
//
// A XID is created by taking the SHA-256 hash of the CBOR encoding of a
// signing public key (the "genesis key"). It remains stable throughout its
// lifecycle even as keys and permissions change.
//
// As defined in BCR-2024-010.
type XID struct {
	data [XIDSize]byte
}

// XIDFromData creates a XID from a 32-byte array.
func XIDFromData(data [XIDSize]byte) XID {
	return XID{data: data}
}

// XIDFromDataRef creates a XID from a byte slice, validating its length.
func XIDFromDataRef(data []byte) (XID, error) {
	if len(data) != XIDSize {
		return XID{}, errInvalidSize("XID", XIDSize, len(data))
	}
	var x XID
	copy(x.data[:], data)
	return x, nil
}

// NewXID creates a new XID from the given public signing key (the "genesis
// key"). The XID is the SHA-256 digest of the tagged CBOR encoding of the key.
func NewXID(genesisKey SigningPublicKey) XID {
	keyCBORData := genesisKey.TaggedCBOR().ToCBORData()
	digest := bccrypto.SHA256(keyCBORData)
	return XIDFromData(digest)
}

// Validate checks whether this XID matches the SHA-256 hash of the CBOR
// encoding of the given public key.
func (x XID) Validate(key SigningPublicKey) bool {
	keyCBORData := key.TaggedCBOR().ToCBORData()
	digest := bccrypto.SHA256(keyCBORData)
	return x.data == digest
}

// XIDFromHex creates a XID from a hex string. Panics if the input is not
// exactly 64 hexadecimal digits.
func XIDFromHex(h string) XID {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid XID hex: %v", err))
	}
	x, err := XIDFromDataRef(data)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: %v", err))
	}
	return x
}

// Data returns the 32-byte XID data.
func (x XID) Data() [XIDSize]byte { return x.data }

// Bytes returns the XID as a byte slice.
func (x XID) Bytes() []byte { return x.data[:] }

// Hex returns the XID as a 64-character hex string.
func (x XID) Hex() string { return hex.EncodeToString(x.data[:]) }

// Reference returns a Reference derived from this XID's data. Since a XID
// is already a 32-byte hash, its data is used directly as the reference.
func (x XID) Reference() Reference {
	return ReferenceFromData(x.data)
}

// ShortDescription returns the first 4 bytes as an 8-character hex string,
// delegating to the reference.
func (x XID) ShortDescription() string {
	return x.Reference().RefHexShort()
}

// BytewordsIdentifier returns the first 4 bytes as upper-case ByteWords,
// optionally prefixed.
func (x XID) BytewordsIdentifier(prefix bool) string {
	p := ""
	if prefix {
		p = "\U0001f167"
	}
	ref := x.Reference()
	s := strings.ToUpper(bcur.BytewordsIdentifier(ref.RefDataShort()))
	if p != "" {
		return p + " " + s
	}
	return s
}

// BytemojiIdentifier returns the first 4 bytes as Bytemoji, optionally
// prefixed.
func (x XID) BytemojiIdentifier(prefix bool) string {
	p := ""
	if prefix {
		p = "\U0001f167"
	}
	ref := x.Reference()
	s := strings.ToUpper(bcur.BytemojiIdentifier(ref.RefDataShort()))
	if p != "" {
		return p + " " + s
	}
	return s
}

// String returns a short human-readable representation.
func (x XID) String() string {
	return fmt.Sprintf("XID(%s)", x.ShortDescription())
}

// Equal reports whether two XIDs are equal.
func (x XID) Equal(other XID) bool { return x.data == other.data }

// XIDProvider is implemented by types that can provide an XID.
type XIDProvider interface {
	XID() XID
}

// XID implements XIDProvider by returning itself.
func (x XID) XID() XID { return x }

// --- CBOR support ---

// XIDCBORTags returns the CBOR tags used for XID.
func XIDCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagXID})
}

// CBORTags implements dcbor.CBORTagged.
func (x XID) CBORTags() []dcbor.Tag { return XIDCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (x XID) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(x.data[:]) }

// TaggedCBOR returns the tagged CBOR encoding of the XID.
func (x XID) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(x)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (x XID) ToCBOR() dcbor.CBOR { return x.TaggedCBOR() }

// DecodeXID decodes a XID from untagged CBOR.
func DecodeXID(cbor dcbor.CBOR) (XID, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return XID{}, err
	}
	return XIDFromDataRef(data)
}

// DecodeTaggedXID decodes a XID from tagged CBOR.
func DecodeTaggedXID(cbor dcbor.CBOR) (XID, error) {
	return dcbor.DecodeTagged(cbor, XIDCBORTags(), DecodeXID)
}

// XIDFromTaggedCBOR decodes a XID from tagged CBOR bytes.
func XIDFromTaggedCBOR(data []byte) (XID, error) {
	return dcbor.DecodeTaggedData(data, XIDCBORTags(), DecodeXID)
}

// --- UR support ---

// XIDToURString encodes a XID as a UR string.
func XIDToURString(x XID) string { return bcur.ToURString(x) }

// XIDFromURString decodes a XID from a UR string.
func XIDFromURString(urString string) (XID, error) {
	return bcur.DecodeURString(urString, XIDCBORTags(), DecodeXID)
}
