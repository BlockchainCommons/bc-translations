package bccomponents

import (
	"bytes"
	"encoding/hex"
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// JSON is a CBOR-tagged container for UTF-8 JSON text (tag 262).
type JSON struct {
	data []byte
}

// JSONFromData creates a JSON from byte data.
func JSONFromData(data []byte) JSON {
	cp := make([]byte, len(data))
	copy(cp, data)
	return JSON{data: cp}
}

// JSONFromString creates a JSON from a string.
func JSONFromString(s string) JSON {
	return JSONFromData([]byte(s))
}

// JSONFromHex creates a JSON from a hex string. Panics if invalid.
func JSONFromHex(h string) JSON {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic(fmt.Sprintf("bccomponents: invalid JSON hex: %v", err))
	}
	return JSONFromData(data)
}

// Len returns the length of the JSON data in bytes.
func (j JSON) Len() int { return len(j.data) }

// IsEmpty returns true if the JSON data is empty.
func (j JSON) IsEmpty() bool { return len(j.data) == 0 }

// Bytes returns the JSON as a byte slice.
func (j JSON) Bytes() []byte {
	cp := make([]byte, len(j.data))
	copy(cp, j.data)
	return cp
}

// Str returns the JSON data as a string.
func (j JSON) Str() string { return string(j.data) }

// Hex returns the JSON data as a hex string.
func (j JSON) Hex() string { return hex.EncodeToString(j.data) }

// String returns a human-readable representation.
func (j JSON) String() string { return fmt.Sprintf("JSON(%s)", j.Str()) }

// Equal reports whether two JSON values are equal.
func (j JSON) Equal(other JSON) bool {
	return bytes.Equal(j.data, other.data)
}

// --- CBOR support ---

// JSONCBORTags returns the CBOR tags used for JSON.
func JSONCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagJSON})
}

// CBORTags implements dcbor.CBORTagged.
func (j JSON) CBORTags() []dcbor.Tag { return JSONCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (j JSON) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(j.data) }

// TaggedCBOR returns the tagged CBOR encoding of the JSON.
func (j JSON) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(j)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (j JSON) ToCBOR() dcbor.CBOR { return j.TaggedCBOR() }

// DecodeJSON decodes a JSON from untagged CBOR.
func DecodeJSON(cbor dcbor.CBOR) (JSON, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return JSON{}, err
	}
	return JSONFromData(data), nil
}

// DecodeTaggedJSON decodes a JSON from tagged CBOR.
func DecodeTaggedJSON(cbor dcbor.CBOR) (JSON, error) {
	return dcbor.DecodeTagged(cbor, JSONCBORTags(), DecodeJSON)
}

// --- UR support ---

// JSONToURString encodes a JSON as a UR string.
func JSONToURString(j JSON) string { return bcur.ToURString(j) }

// JSONFromURString decodes a JSON from a UR string.
func JSONFromURString(urString string) (JSON, error) {
	return bcur.DecodeURString(urString, JSONCBORTags(), DecodeJSON)
}
