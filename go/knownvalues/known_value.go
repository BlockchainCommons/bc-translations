package knownvalues

import (
	"strconv"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// KnownValue is a compact identifier for a stand-alone ontological concept.
//
// Equality is based only on the numeric value, not the assigned display name.
type KnownValue struct {
	value           uint64
	assignedName    string
	hasAssignedName bool
}

// NewKnownValue creates a known value with no assigned display name.
func NewKnownValue(value uint64) KnownValue {
	return KnownValue{value: value}
}

// NewKnownValueWithName creates a known value with a dynamically assigned name.
func NewKnownValueWithName(value uint64, assignedName string) KnownValue {
	return KnownValue{
		value:           value,
		assignedName:    assignedName,
		hasAssignedName: true,
	}
}

// newKnownValueWithStaticName creates a known value with a registry-backed
// assigned name. Used internally by registry constants.
func newKnownValueWithStaticName(value uint64, name string) KnownValue {
	return NewKnownValueWithName(value, name)
}

// KnownValueFromInt32 converts a signed 32-bit integer into a known value.
func KnownValueFromInt32(value int32) KnownValue {
	return NewKnownValue(uint64(value))
}

// KnownValueFromInt converts a native-width integer into a known value.
func KnownValueFromInt(value int) KnownValue {
	return NewKnownValue(uint64(value))
}

// Value returns the raw numeric value.
func (k KnownValue) Value() uint64 {
	return k.value
}

// AssignedName returns the assigned display name, if one exists.
func (k KnownValue) AssignedName() (string, bool) {
	if !k.hasAssignedName {
		return "", false
	}
	return k.assignedName, true
}

// Name returns the assigned display name or the numeric value as text.
func (k KnownValue) Name() string {
	if k.hasAssignedName {
		return k.assignedName
	}
	return strconv.FormatUint(k.value, 10)
}

// Equal reports whether two known values have the same raw numeric value.
func (k KnownValue) Equal(other KnownValue) bool {
	return k.value == other.value
}

// String returns the display form of the known value.
func (k KnownValue) String() string {
	return k.Name()
}

// Digest computes the SHA-256 digest of the tagged CBOR representation.
func (k KnownValue) Digest() bccomponents.Digest {
	return bccomponents.DigestFromImage(k.TaggedCBOR().ToCBORData())
}

// KnownValueCBORTags returns the accepted CBOR tags for known values.
func KnownValueCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagKnownValue})
}

// CBORTags returns the accepted CBOR tags for known values.
func (k KnownValue) CBORTags() []dcbor.Tag {
	return KnownValueCBORTags()
}

// UntaggedCBOR returns the untagged CBOR representation of the known value.
func (k KnownValue) UntaggedCBOR() dcbor.CBOR {
	return dcbor.NewCBORUnsigned(k.value)
}

// TaggedCBOR returns the tagged CBOR representation of the known value.
func (k KnownValue) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

// ToCBOR returns the tagged CBOR representation of the known value.
func (k KnownValue) ToCBOR() dcbor.CBOR {
	return k.TaggedCBOR()
}

// DecodeKnownValue decodes an untagged CBOR value into a known value.
func DecodeKnownValue(cbor dcbor.CBOR) (KnownValue, error) {
	value, err := cbor.TryIntoUInt64()
	if err != nil {
		return KnownValue{}, err
	}
	return NewKnownValue(value), nil
}

// DecodeTaggedKnownValue decodes a tagged CBOR value into a known value.
func DecodeTaggedKnownValue(cbor dcbor.CBOR) (KnownValue, error) {
	return dcbor.DecodeTagged(cbor, KnownValueCBORTags(), DecodeKnownValue)
}

var (
	_ bccomponents.DigestProvider = KnownValue{}
	_ dcbor.CBORTaggedEncodable   = KnownValue{}
	_ dcbor.CBORTaggedDecodable   = KnownValue{}
	_ dcbor.CBOREncodable         = KnownValue{}
)
